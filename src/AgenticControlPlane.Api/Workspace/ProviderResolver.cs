using AgenticControlPlane.Api.Providers;

namespace AgenticControlPlane.Api.Workspace;

/// <summary>
/// Resolves the correct <see cref="IVcsProvider"/> for a given <see cref="RepoTarget"/>
/// by provider name, without tying callers to the DI container.
/// </summary>
public sealed class ProviderResolver(GitHubProvider github, BitbucketProvider bitbucket)
{
    public IVcsProvider Resolve(string providerName) =>
        providerName.ToLowerInvariant() switch
        {
            "bitbucket" => bitbucket,
            _           => github
        };
}
