namespace AgenticControlPlane.Api.Providers;

/// <summary>
/// Provider-agnostic pull request model shared across all VCS adapters.
/// </summary>
public record NormalizedPr(
    int Number,
    string Title,
    string HtmlUrl,
    string HeadRef,
    /// <summary>"open" | "closed" | "merged" | "declined"</summary>
    string State,
    IReadOnlyList<string> Labels,
    DateTimeOffset CreatedAt,
    string AuthorLogin);
