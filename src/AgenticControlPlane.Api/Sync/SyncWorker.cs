using AgenticControlPlane.Api.Workspace;

namespace AgenticControlPlane.Api.Sync;

/// <summary>
/// Background hosted service that periodically fetches all configured repos
/// and writes snapshots into <see cref="PortfolioCache"/>.
///
/// Sync interval is controlled by SYNC_INTERVAL_SECONDS (default 120 s).
/// An initial sync runs immediately at startup before the timer kicks in.
/// </summary>
public sealed class SyncWorker(
    PortfolioCache cache,
    ProviderResolver resolver,
    ILogger<SyncWorker> logger) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(
        int.TryParse(Environment.GetEnvironmentVariable("SYNC_INTERVAL_SECONDS"), out var s) && s > 0
            ? s : 120);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SyncWorker starting. Interval = {Interval}s", _interval.TotalSeconds);

        // First sync immediately so the cache is warm before the first API call.
        await SyncAllAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncAllAsync(stoppingToken);
        }
    }

    private async Task SyncAllAsync(CancellationToken ct)
    {
        var repos = WorkspaceConfig.Load();
        if (repos.Count == 0)
        {
            logger.LogWarning("SyncWorker: no repos configured. Set REPOS or legacy per-provider env vars.");
            return;
        }

        logger.LogInformation("SyncWorker: syncing {Count} repo(s)…", repos.Count);

        var tasks = repos.Select(r => SyncRepoAsync(r, ct));
        await Task.WhenAll(tasks);

        logger.LogInformation("SyncWorker: sync complete.");
    }

    private async Task SyncRepoAsync(RepoTarget target, CancellationToken ct)
    {
        var key = PortfolioCache.MakeKey(target.Provider, target.Owner, target.Repo);
        try
        {
            var provider = resolver.Resolve(target.Provider);
            var prs = await provider.GetPullRequestsAsync(target.Owner, target.Repo, ct);
            var classified = prs.Select(Classifier.Classify).ToArray();

            var experiments = classified.Where(p => p.Lane == "prototype").ToArray();
            var hardening   = classified.Where(p => p.Lane == "hardening").ToArray();
            var promoted    = classified.Where(p => p.Decision is "promote" or "merged").ToArray();
            var killed      = classified.Where(p => p.Decision == "kill").ToArray();

            double killRate = classified.Length > 0
                ? Math.Round((double)killed.Length / classified.Length * 100, 1) : 0;

            double conversionRate = (experiments.Length + promoted.Length) > 0
                ? Math.Round((double)promoted.Length / (experiments.Length + promoted.Length) * 100, 1) : 0;

            double medianCycle = experiments.Length > 0
                ? Math.Round(experiments.Select(e => e.AgeHours).OrderBy(h => h)
                    .ElementAt(experiments.Length / 2), 1) : 0;

            cache.Set(key, new RepoSnapshot
            {
                Key              = key,
                Provider         = target.Provider,
                Owner            = target.Owner,
                Repo             = target.Repo,
                RepoUrl          = provider.BuildRepoUrl(target.Owner, target.Repo),
                SyncedAt         = DateTimeOffset.UtcNow,
                Items            = classified,
                Experiments      = experiments,
                Hardening        = hardening,
                Promoted         = promoted,
                Killed           = killed,
                KillRate         = killRate,
                ConversionRate   = conversionRate,
                MedianCycleHours = medianCycle
            });

            logger.LogInformation("SyncWorker: {Key} → {Count} PR(s) synced.", key, classified.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SyncWorker: failed to sync {Key}.", key);

            // Write an error snapshot so the API can surface the failure.
            cache.Set(key, new RepoSnapshot
            {
                Key       = key,
                Provider  = target.Provider,
                Owner     = target.Owner,
                Repo      = target.Repo,
                RepoUrl   = string.Empty,
                SyncedAt  = DateTimeOffset.UtcNow,
                SyncError = ex.Message
            });
        }
    }
}
