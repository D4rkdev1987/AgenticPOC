using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using AgenticControlPlane.Api.Providers;
using AgenticControlPlane.Api.Sync;
using AgenticControlPlane.Api.Workspace;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ── HTTP clients ──────────────────────────────────────────────────────────────

builder.Services.AddHttpClient("github", client =>
{
    var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? string.Empty;
    client.BaseAddress = new Uri("https://api.github.com");
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AgenticControlPlane", "1.0"));
    if (!string.IsNullOrWhiteSpace(token))
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
});

builder.Services.AddHttpClient("bitbucket", client =>
{
    var username    = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME") ?? string.Empty;
    var appPassword = Environment.GetEnvironmentVariable("BITBUCKET_APP_PASSWORD") ?? string.Empty;
    client.BaseAddress = new Uri("https://api.bitbucket.org");
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AgenticControlPlane", "1.0"));
    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(appPassword))
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{appPassword}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
});

// ── VCS providers ─────────────────────────────────────────────────────────────

builder.Services.AddSingleton<GitHubProvider>();
builder.Services.AddSingleton<BitbucketProvider>();

builder.Services.AddSingleton<IVcsProvider>(sp =>
{
    var providerName = (Environment.GetEnvironmentVariable("VCS_PROVIDER") ?? "github")
        .Trim().ToLowerInvariant();

    return providerName switch
    {
        "bitbucket" => sp.GetRequiredService<BitbucketProvider>(),
        _           => sp.GetRequiredService<GitHubProvider>()
    };
});

// ── Workspace / sync (Phase 3 + 4) ───────────────────────────────────────────

builder.Services.AddSingleton<ProviderResolver>();
builder.Services.AddSingleton<PortfolioCache>();
builder.Services.AddHostedService<SyncWorker>();

// ── CORS ──────────────────────────────────────────────────────────────────────

builder.Services.AddCors(options =>
    options.AddPolicy("dev", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();
app.UseCors("dev");

// ── Health ────────────────────────────────────────────────────────────────────

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// ── Portfolio (legacy single-repo; now reads from cache) ─────────────────────

app.MapGet("/api/portfolio", (PortfolioCache cache, IVcsProvider provider) =>
{
    var (owner, repo) = ResolveOwnerRepo();

    if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        return Results.BadRequest(new
        {
            error = "Owner/repo env vars must be set. " +
                    "For GitHub: GITHUB_OWNER + GITHUB_REPO. " +
                    "For Bitbucket: BITBUCKET_WORKSPACE + BITBUCKET_REPO."
        });

    var providerName = (Environment.GetEnvironmentVariable("VCS_PROVIDER") ?? "github")
        .Trim().ToLowerInvariant();

    var key = PortfolioCache.MakeKey(providerName, owner, repo);
    var snapshot = cache.Get(key);

    if (snapshot is null)
        return Results.Accepted("/api/portfolio", new { status = "syncing", message = "Initial sync in progress — please retry in a moment." });

    if (snapshot.SyncError is not null)
        return Results.Problem(snapshot.SyncError, title: "Sync error", statusCode: 502);

    var repoMeta = new
    {
        owner    = snapshot.Owner,
        repo     = snapshot.Repo,
        url      = snapshot.RepoUrl,
        provider = provider.ProviderName,
        syncedAt = snapshot.SyncedAt
    };

    return Results.Ok(new PortfolioResponse(
        RepoMeta: repoMeta,
        Items:       snapshot.Items.Select(ToItem).ToArray(),
        Experiments: snapshot.Experiments.Select(ToItem).ToArray(),
        Hardening:   snapshot.Hardening.Select(ToItem).ToArray(),
        Promoted:    snapshot.Promoted.Select(ToItem).ToArray(),
        Killed:      snapshot.Killed.Select(ToItem).ToArray(),
        Metrics: new DashboardMetrics(
            CycleTime:      $"{snapshot.MedianCycleHours}h",
            ConversionRate: $"{snapshot.ConversionRate}%",
            KillRate:       $"{snapshot.KillRate}%"
        )
    ));
});

// ── Workspace (Phase 3) — all repos, aggregated metrics ──────────────────────

app.MapGet("/api/workspace", (PortfolioCache cache) =>
{
    var snapshots = cache.All();

    if (snapshots.Count == 0)
        return Results.Accepted("/api/workspace", new { status = "syncing", message = "Initial sync in progress — please retry in a moment." });

    var repoViews = snapshots.Select(s => new RepoView(
        Key:             s.Key,
        Provider:        s.Provider,
        Owner:           s.Owner,
        Repo:            s.Repo,
        RepoUrl:         s.RepoUrl,
        SyncedAt:        s.SyncedAt,
        SyncError:       s.SyncError,
        TotalPrs:        s.Items.Count,
        Experiments:     s.Experiments.Select(ToItem).ToArray(),
        Hardening:       s.Hardening.Select(ToItem).ToArray(),
        Promoted:        s.Promoted.Select(ToItem).ToArray(),
        Killed:          s.Killed.Select(ToItem).ToArray(),
        Metrics: new DashboardMetrics(
            CycleTime:      $"{s.MedianCycleHours}h",
            ConversionRate: $"{s.ConversionRate}%",
            KillRate:       $"{s.KillRate}%"
        )
    )).ToArray();

    // Aggregate across all repos
    var allItems       = snapshots.SelectMany(s => s.Items).ToArray();
    var allExperiments = snapshots.SelectMany(s => s.Experiments).ToArray();
    var allPromoted    = snapshots.SelectMany(s => s.Promoted).ToArray();
    var allKilled      = snapshots.SelectMany(s => s.Killed).ToArray();

    double aggKill = allItems.Length > 0
        ? Math.Round((double)allKilled.Length / allItems.Length * 100, 1) : 0;

    double aggConversion = (allExperiments.Length + allPromoted.Length) > 0
        ? Math.Round((double)allPromoted.Length / (allExperiments.Length + allPromoted.Length) * 100, 1) : 0;

    double aggCycle = allExperiments.Length > 0
        ? Math.Round(allExperiments.Select(e => e.AgeHours).OrderBy(h => h)
            .ElementAt(allExperiments.Length / 2), 1) : 0;

    return Results.Ok(new WorkspaceResponse(
        Repos: repoViews,
        TotalRepos: repoViews.Length,
        OldestSync: cache.OldestSync,
        Aggregate: new DashboardMetrics(
            CycleTime:      $"{aggCycle}h",
            ConversionRate: $"{aggConversion}%",
            KillRate:       $"{aggKill}%"
        )
    ));
});

// ── Sync status (Phase 4 observability) ──────────────────────────────────────

app.MapGet("/api/sync/status", (PortfolioCache cache) =>
{
    var snapshots = cache.All();
    return Results.Ok(new
    {
        repoCount  = snapshots.Count,
        oldestSync = cache.OldestSync,
        repos      = snapshots.Select(s => new
        {
            key       = s.Key,
            syncedAt  = s.SyncedAt,
            prCount   = s.Items.Count,
            syncError = s.SyncError
        }).ToArray()
    });
});

app.MapGet("/api/dashboard", () =>
{
    var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
    var assets = GetAssetDefinitions();

    var results = assets.Select(asset =>
    {
        var fullPath = Path.Combine(repoRoot, asset.Path.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return new AssetResult(asset.Label, asset.Path, asset.Kind, false, null, "File not found");
        }

        var text = File.ReadAllText(fullPath);
        if (asset.Kind == "markdown")
        {
            var sections = Regex.Matches(text, "^##\\s+", RegexOptions.Multiline).Count;
            var bullets = Regex.Matches(text, "^[-*]\\s+", RegexOptions.Multiline).Count;
            return new AssetResult(asset.Label, asset.Path, asset.Kind, true, new { sections, bullets }, null);
        }

        var jobs = Regex.Matches(text, "^  ([A-Za-z0-9_-]+):\\s*$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Where(name => name is not ("on" or "permissions" or "concurrency" or "jobs"))
            .Distinct()
            .ToArray();

        var hasPullRequest = Regex.IsMatch(text, "pull_request:", RegexOptions.Multiline);
        var hasDispatch = Regex.IsMatch(text, "workflow_dispatch:", RegexOptions.Multiline);

        return new AssetResult(asset.Label, asset.Path, asset.Kind, true,
            new { jobs = jobs.Length, hasPullRequest, hasDispatch }, null);
    }).ToArray();

    var workflowNames = new[]
    {
        ".github/workflows/agentic-delivery-gates.yml",
        ".github/workflows/hardening-checklist-gate.yml",
        ".github/workflows/promotion-readiness-score.yml",
        ".github/workflows/staged-release.yml"
    };

    var workflowCoverage = workflowNames.Select(path =>
        new CoverageItem(path, results.Any(r => r.Path == path && r.Ok))).ToArray();

    var highlights = new[]
    {
        new CoverageItem("Two-lane model defined", ContainsFileText(repoRoot, "docs/agentic-operating-system/AGENTIC_DELIVERY_PLAYBOOK.md", "Lane A: Prototype Lane")
            && ContainsFileText(repoRoot, "docs/agentic-operating-system/AGENTIC_DELIVERY_PLAYBOOK.md", "Lane B: Hardening Lane")),
        new CoverageItem("KPI model defined", ContainsFileText(repoRoot, "docs/agentic-operating-system/AGENTIC_DELIVERY_PLAYBOOK.md", "## 9) Delivery KPIs")),
        new CoverageItem("Branch protection baseline exists", ContainsFileText(repoRoot, "docs/agentic-operating-system/REPO_POLICY_GUIDE.md", "## 3) Branch Protection Baseline")),
        new CoverageItem("Automation policy exists", ContainsFileText(repoRoot, "docs/agentic-operating-system/REPO_POLICY_GUIDE.md", "## 6) Required Automation"))
    };

    var metrics = new DashboardMetrics("18h", "27%", "51%");

    return Results.Ok(new DashboardResponse(results, workflowCoverage, highlights, metrics));
});

app.Run();

// ── Helpers ───────────────────────────────────────────────────────────────────

static (string owner, string repo) ResolveOwnerRepo()
{
    var providerName = (Environment.GetEnvironmentVariable("VCS_PROVIDER") ?? "github")
        .Trim().ToLowerInvariant();

    return providerName == "bitbucket"
        ? (Environment.GetEnvironmentVariable("BITBUCKET_WORKSPACE") ?? string.Empty,
           Environment.GetEnvironmentVariable("BITBUCKET_REPO") ?? string.Empty)
        : (Environment.GetEnvironmentVariable("GITHUB_OWNER") ?? string.Empty,
           Environment.GetEnvironmentVariable("GITHUB_REPO") ?? string.Empty);
}

static PortfolioItem ToItem(ClassifiedPr p) => new(
    p.Number, p.Title, p.Branch, p.Lane, p.State,
    p.Labels, p.Decision, p.AgeHours, p.Author, p.Url, p.CreatedAt);

static string FindRepoRoot(string startPath)
{
    var dir = new DirectoryInfo(startPath);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "docs", "agentic-operating-system")))
            return dir.FullName;
        dir = dir.Parent;
    }
    throw new InvalidOperationException("Could not locate repository root.");
}

static bool ContainsFileText(string repoRoot, string relativePath, string expected)
{
    var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    return File.Exists(fullPath) && File.ReadAllText(fullPath).Contains(expected, StringComparison.Ordinal);
}

static AssetDefinition[] GetAssetDefinitions() =>
[
    new("Playbook",              "docs/agentic-operating-system/AGENTIC_DELIVERY_PLAYBOOK.md",           "markdown"),
    new("Policy Guide",          "docs/agentic-operating-system/REPO_POLICY_GUIDE.md",                  "markdown"),
    new("Team Charter",          "docs/agentic-operating-system/TEAM_CHARTER_ONE_PAGE.md",               "markdown"),
    new("Hypothesis Template",   "docs/agentic-operating-system/templates/HYPOTHESIS_BRIEF_TEMPLATE.md","markdown"),
    new("Promotion Checklist",   "docs/agentic-operating-system/templates/PROMOTION_CHECKLIST.md",      "markdown"),
    new("Portfolio Review",      "docs/agentic-operating-system/templates/WEEKLY_PORTFOLIO_REVIEW.md",  "markdown"),
    new("Agentic Gates Workflow",    ".github/workflows/agentic-delivery-gates.yml",    "workflow"),
    new("Hardening Checklist Gate",  ".github/workflows/hardening-checklist-gate.yml", "workflow"),
    new("Promotion Readiness Score", ".github/workflows/promotion-readiness-score.yml","workflow"),
    new("Staged Release",            ".github/workflows/staged-release.yml",            "workflow")
];

// ── Domain record types ───────────────────────────────────────────────────────

record AssetDefinition(string Label, string Path, string Kind);
record AssetResult(string Label, string Path, string Kind, bool Ok, object? Summary, string? Error);
record CoverageItem(string Label, bool Pass);
record DashboardMetrics(string CycleTime, string ConversionRate, string KillRate);
record DashboardResponse(AssetResult[] Assets, CoverageItem[] WorkflowCoverage, CoverageItem[] Highlights, DashboardMetrics Metrics);

record PortfolioItem(
    int Number, string Title, string Branch, string Lane, string State,
    string[] Labels, string Decision, double AgeHours, string Author,
    string Url, string CreatedAt);

record PortfolioResponse(
    object RepoMeta,
    PortfolioItem[] Items,
    PortfolioItem[] Experiments,
    PortfolioItem[] Hardening,
    PortfolioItem[] Promoted,
    PortfolioItem[] Killed,
    DashboardMetrics Metrics);

// Phase 3 — workspace aggregation types
record RepoView(
    string Key, string Provider, string Owner, string Repo,
    string RepoUrl, DateTimeOffset SyncedAt, string? SyncError,
    int TotalPrs,
    PortfolioItem[] Experiments, PortfolioItem[] Hardening,
    PortfolioItem[] Promoted, PortfolioItem[] Killed,
    DashboardMetrics Metrics);

record WorkspaceResponse(
    RepoView[] Repos,
    int TotalRepos,
    DateTimeOffset? OldestSync,
    DashboardMetrics Aggregate);
