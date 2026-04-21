using System.Collections.Concurrent;

namespace AgenticControlPlane.Api.Sync;

/// <summary>
/// Thread-safe in-memory store for synced portfolio snapshots.
/// Each key is "{provider}:{owner}/{repo}".
/// The value is an immutable snapshot fetched by <see cref="SyncWorker"/>.
/// </summary>
public sealed class PortfolioCache
{
    private readonly ConcurrentDictionary<string, RepoSnapshot> _store = new();

    public static string MakeKey(string provider, string owner, string repo) =>
        $"{provider}:{owner}/{repo}";

    /// <summary>Stores (or replaces) a snapshot for the given repo.</summary>
    public void Set(string key, RepoSnapshot snapshot) => _store[key] = snapshot;

    /// <summary>Returns the snapshot if available; null otherwise.</summary>
    public RepoSnapshot? Get(string key) => _store.GetValueOrDefault(key);

    /// <summary>All currently cached snapshots, ordered by key.</summary>
    public IReadOnlyList<RepoSnapshot> All() =>
        _store.Values.OrderBy(s => s.Key).ToArray();

    /// <summary>Timestamp of the oldest snapshot (null if cache is empty).</summary>
    public DateTimeOffset? OldestSync =>
        _store.IsEmpty ? null : _store.Values.Min(s => s.SyncedAt);
}

/// <summary>
/// A point-in-time snapshot of all classified PRs for one repository.
/// </summary>
public sealed class RepoSnapshot
{
    public required string Key         { get; init; }   // "{provider}:{owner}/{repo}"
    public required string Provider    { get; init; }
    public required string Owner       { get; init; }
    public required string Repo        { get; init; }
    public required string RepoUrl     { get; init; }
    public DateTimeOffset  SyncedAt    { get; init; } = DateTimeOffset.UtcNow;
    public string?         SyncError   { get; init; }

    // Classified items (set when sync succeeds)
    public IReadOnlyList<ClassifiedPr> Items        { get; init; } = [];
    public IReadOnlyList<ClassifiedPr> Experiments  { get; init; } = [];
    public IReadOnlyList<ClassifiedPr> Hardening    { get; init; } = [];
    public IReadOnlyList<ClassifiedPr> Promoted     { get; init; } = [];
    public IReadOnlyList<ClassifiedPr> Killed       { get; init; } = [];

    // Aggregated metrics for this repo
    public double KillRate         { get; init; }
    public double ConversionRate   { get; init; }
    public double MedianCycleHours { get; init; }
}

/// <summary>
/// A pull request with all classification applied — lane, decision, age.
/// Mirrors the API's <c>PortfolioItem</c> shape for easy serialisation.
/// </summary>
public sealed class ClassifiedPr
{
    public int      Number    { get; init; }
    public string   Title     { get; init; } = string.Empty;
    public string   Branch    { get; init; } = string.Empty;
    public string   Lane      { get; init; } = string.Empty;
    public string   State     { get; init; } = string.Empty;
    public string[] Labels    { get; init; } = [];
    public string   Decision  { get; init; } = string.Empty;
    public double   AgeHours  { get; init; }
    public string   Author    { get; init; } = string.Empty;
    public string   Url       { get; init; } = string.Empty;
    public string   CreatedAt { get; init; } = string.Empty;
}
