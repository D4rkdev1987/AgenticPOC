using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticControlPlane.Api.Providers;

/// <summary>
/// Fetches pull requests from the GitHub REST API and normalises them.
/// Requires the "github" named <see cref="HttpClient"/> registered in DI.
/// </summary>
public sealed class GitHubProvider(IHttpClientFactory httpClientFactory) : IVcsProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ProviderName => "GitHub";

    public string BuildRepoUrl(string owner, string repo) =>
        $"https://github.com/{owner}/{repo}";

    public async Task<IReadOnlyList<NormalizedPr>> GetPullRequestsAsync(
        string owner, string repo, CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("github");
        var json = await http.GetStringAsync(
            $"/repos/{owner}/{repo}/pulls?state=all&per_page=50&sort=updated&direction=desc", ct);

        var prs = JsonSerializer.Deserialize<GitHubPrDto[]>(json, JsonOptions) ?? [];

        return prs.Select(pr => new NormalizedPr(
            Number: pr.Number,
            Title: pr.Title ?? "(no title)",
            HtmlUrl: pr.HtmlUrl ?? string.Empty,
            HeadRef: pr.Head?.Ref ?? string.Empty,
            State: pr.State ?? "unknown",
            Labels: pr.Labels?.Select(l => l.Name ?? string.Empty).ToArray() ?? [],
            CreatedAt: pr.CreatedAt ?? DateTimeOffset.UtcNow,
            AuthorLogin: pr.User?.Login ?? "unknown"
        )).ToArray();
    }
}

// ── GitHub API DTOs (private to this file) ────────────────────────────────────

file class GitHubPrDto
{
    [JsonPropertyName("number")] public int Number { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("state")] public string? State { get; set; }
    [JsonPropertyName("html_url")] public string? HtmlUrl { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset? CreatedAt { get; set; }
    [JsonPropertyName("head")] public GitHubRefDto? Head { get; set; }
    [JsonPropertyName("labels")] public GitHubLabelDto[]? Labels { get; set; }
    [JsonPropertyName("user")] public GitHubUserDto? User { get; set; }
}

file class GitHubRefDto
{
    [JsonPropertyName("ref")] public string? Ref { get; set; }
}

file class GitHubLabelDto
{
    [JsonPropertyName("name")] public string? Name { get; set; }
}

file class GitHubUserDto
{
    [JsonPropertyName("login")] public string? Login { get; set; }
}
