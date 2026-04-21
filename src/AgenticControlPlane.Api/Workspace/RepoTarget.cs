namespace AgenticControlPlane.Api.Workspace;

/// <summary>
/// A single repository entry in the multi-repo workspace.
/// </summary>
/// <param name="Provider">"github" | "bitbucket"</param>
/// <param name="Owner">GitHub org/user or Bitbucket workspace slug.</param>
/// <param name="Repo">Repository slug / name.</param>
public record RepoTarget(string Provider, string Owner, string Repo);
