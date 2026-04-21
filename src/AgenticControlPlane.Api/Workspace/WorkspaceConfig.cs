using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticControlPlane.Api.Workspace;

/// <summary>
/// Parses the REPOS environment variable into a list of <see cref="RepoTarget"/>s.
///
/// REPOS accepts two formats:
///
/// 1. JSON array (multi-repo workspace):
///    REPOS=[{"provider":"github","owner":"acme","repo":"alpha"},{"provider":"bitbucket","owner":"acme","repo":"beta"}]
///
/// 2. Legacy single-repo (falls back to individual env vars):
///    REPOS not set  →  reads VCS_PROVIDER / GITHUB_OWNER / GITHUB_REPO
///                       or BITBUCKET_WORKSPACE / BITBUCKET_REPO
/// </summary>
public static class WorkspaceConfig
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyList<RepoTarget> Load()
    {
        var raw = (Environment.GetEnvironmentVariable("REPOS") ?? string.Empty).Trim();

        if (!string.IsNullOrEmpty(raw))
        {
            try
            {
                var dtos = JsonSerializer.Deserialize<RepoTargetDto[]>(raw, _json);
                if (dtos is { Length: > 0 })
                    return dtos.Select(d => new RepoTarget(
                        (d.Provider ?? "github").ToLowerInvariant(),
                        d.Owner ?? string.Empty,
                        d.Repo ?? string.Empty)).ToArray();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "REPOS env var is not valid JSON. Expected a JSON array of {provider,owner,repo} objects.", ex);
            }
        }

        // Legacy single-repo fallback.
        var provider = (Environment.GetEnvironmentVariable("VCS_PROVIDER") ?? "github")
            .Trim().ToLowerInvariant();

        var (owner, repo) = provider == "bitbucket"
            ? (Environment.GetEnvironmentVariable("BITBUCKET_WORKSPACE") ?? string.Empty,
               Environment.GetEnvironmentVariable("BITBUCKET_REPO") ?? string.Empty)
            : (Environment.GetEnvironmentVariable("GITHUB_OWNER") ?? string.Empty,
               Environment.GetEnvironmentVariable("GITHUB_REPO") ?? string.Empty);

        return string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo)
            ? []
            : [new RepoTarget(provider, owner, repo)];
    }

    // DTO for JSON deserialization
    private class RepoTargetDto
    {
        [JsonPropertyName("provider")] public string? Provider { get; set; }
        [JsonPropertyName("owner")]    public string? Owner    { get; set; }
        [JsonPropertyName("repo")]     public string? Repo     { get; set; }
    }
}
