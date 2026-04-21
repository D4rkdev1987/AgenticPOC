using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticControlPlane.Api.Providers;

/// <summary>
/// Fetches pull requests from the Bitbucket Cloud REST API (v2) and normalises
/// them into the shared <see cref="NormalizedPr"/> model.
///
/// Required environment variables:
///   BITBUCKET_USERNAME   – your Bitbucket username
///   BITBUCKET_APP_PASSWORD – Bitbucket App Password (repo read scope)
///   BITBUCKET_WORKSPACE  – workspace slug (owner)
///   BITBUCKET_REPO       – repository slug
///
/// Lane classification uses the same branch-prefix convention as the GitHub
/// adapter (exp/, harden/, hotfix/).
///
/// Decision classification maps Bitbucket PR states:
///   OPEN     → "open"
///   MERGED   → "merged"
///   DECLINED → "kill"   (declined ≈ intentionally killed)
///   SUPERSEDED → "kill"
///
/// Promote signals can also be embedded in the PR title or description using
/// the tags [promote-candidate] or [hardened].
/// </summary>
public sealed class BitbucketProvider(IHttpClientFactory httpClientFactory) : IVcsProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ProviderName => "Bitbucket";

    public string BuildRepoUrl(string owner, string repo) =>
        $"https://bitbucket.org/{owner}/{repo}";

    public async Task<IReadOnlyList<NormalizedPr>> GetPullRequestsAsync(
        string owner, string repo, CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("bitbucket");

        // Bitbucket paginates; fetch open + the three closed states.
        var states = new[] { "OPEN", "MERGED", "DECLINED", "SUPERSEDED" };
        var query = string.Join("&", states.Select(s => $"state={s}"));

        var allPrs = new List<BitbucketPrDto>();
        var url = $"/2.0/repositories/{owner}/{repo}/pullrequests?{query}&pagelen=50";

        while (!string.IsNullOrEmpty(url))
        {
            var json = await http.GetStringAsync(url, ct);
            var page = JsonSerializer.Deserialize<BitbucketPageDto<BitbucketPrDto>>(json, JsonOptions);
            if (page?.Values is { Length: > 0 })
                allPrs.AddRange(page.Values);

            // Follow Bitbucket's "next" cursor, but strip the base address first.
            url = page?.Next is not null
                ? new Uri(page.Next).PathAndQuery
                : null;
        }

        return allPrs.Select(pr =>
        {
            var state = (pr.State ?? "OPEN").ToUpperInvariant();
            var normalizedState = state switch
            {
                "MERGED" => "merged",
                "DECLINED" or "SUPERSEDED" => "closed",
                _ => "open"
            };

            // Promote signals: embedded in title or description as [tag].
            var searchText = $"{pr.Title} {pr.Description}".ToLowerInvariant();
            var labels = new List<string>();
            if (state is "DECLINED" or "SUPERSEDED") labels.Add("kill");
            if (searchText.Contains("[hardened]")) labels.Add("hardened");
            if (searchText.Contains("[promote-candidate]")) labels.Add("promote-candidate");

            return new NormalizedPr(
                Number: pr.Id,
                Title: pr.Title ?? "(no title)",
                HtmlUrl: pr.Links?.Html?.Href ?? string.Empty,
                HeadRef: pr.Source?.Branch?.Name ?? string.Empty,
                State: normalizedState,
                Labels: labels,
                CreatedAt: pr.CreatedOn ?? DateTimeOffset.UtcNow,
                AuthorLogin: pr.Author?.Nickname ?? pr.Author?.DisplayName ?? "unknown"
            );
        }).ToArray();
    }
}

// ── Bitbucket Cloud API DTOs (private to this file) ──────────────────────────

file class BitbucketPageDto<T>
{
    [JsonPropertyName("values")] public T[]? Values { get; set; }
    [JsonPropertyName("next")] public string? Next { get; set; }
}

file class BitbucketPrDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("state")] public string? State { get; set; }
    [JsonPropertyName("created_on")] public DateTimeOffset? CreatedOn { get; set; }
    [JsonPropertyName("source")] public BitbucketEndpointDto? Source { get; set; }
    [JsonPropertyName("links")] public BitbucketLinksDto? Links { get; set; }
    [JsonPropertyName("author")] public BitbucketActorDto? Author { get; set; }
}

file class BitbucketEndpointDto
{
    [JsonPropertyName("branch")] public BitbucketBranchDto? Branch { get; set; }
}

file class BitbucketBranchDto
{
    [JsonPropertyName("name")] public string? Name { get; set; }
}

file class BitbucketLinksDto
{
    [JsonPropertyName("html")] public BitbucketHrefDto? Html { get; set; }
}

file class BitbucketHrefDto
{
    [JsonPropertyName("href")] public string? Href { get; set; }
}

file class BitbucketActorDto
{
    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
    [JsonPropertyName("nickname")] public string? Nickname { get; set; }
}
