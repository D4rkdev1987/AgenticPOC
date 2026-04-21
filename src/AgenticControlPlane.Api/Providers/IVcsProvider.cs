namespace AgenticControlPlane.Api.Providers;

/// <summary>
/// Abstraction over any VCS hosting provider (GitHub, Bitbucket, etc.).
/// Implementations fetch raw data and normalise it into <see cref="NormalizedPr"/>.
/// </summary>
public interface IVcsProvider
{
    /// <summary>Human-readable provider name, e.g. "GitHub" or "Bitbucket".</summary>
    string ProviderName { get; }

    /// <summary>
    /// Returns the most-recently-updated pull requests (open + closed) for the given repo.
    /// </summary>
    Task<IReadOnlyList<NormalizedPr>> GetPullRequestsAsync(
        string owner,
        string repo,
        CancellationToken ct = default);

    /// <summary>Builds the canonical web URL for the repository.</summary>
    string BuildRepoUrl(string owner, string repo);
}
