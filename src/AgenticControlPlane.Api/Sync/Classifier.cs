using AgenticControlPlane.Api.Providers;

namespace AgenticControlPlane.Api.Sync;

/// <summary>
/// Pure static helper that applies lane and decision classification to a
/// <see cref="NormalizedPr"/> from any provider, producing a <see cref="ClassifiedPr"/>.
/// </summary>
public static class Classifier
{
    public static ClassifiedPr Classify(NormalizedPr pr)
    {
        var lane = pr.HeadRef switch
        {
            var b when b.StartsWith("exp/")    => "prototype",
            var b when b.StartsWith("harden/") => "hardening",
            var b when b.StartsWith("hotfix/") => "hotfix",
            _                                  => "other"
        };

        var decision = pr.Labels.Contains("kill")                             ? "kill"
            : pr.Labels.Contains("hardened")                                  ? "promote"
            : pr.Labels.Contains("promote-candidate")                         ? "promote-candidate"
            : pr.State is "closed" or "merged" && !pr.Labels.Contains("kill") ? "merged"
            : "open";

        var ageHours = Math.Round((DateTimeOffset.UtcNow - pr.CreatedAt).TotalHours, 1);

        return new ClassifiedPr
        {
            Number    = pr.Number,
            Title     = pr.Title,
            Branch    = pr.HeadRef,
            Lane      = lane,
            State     = pr.State,
            Labels    = pr.Labels.ToArray(),
            Decision  = decision,
            AgeHours  = ageHours,
            Author    = pr.AuthorLogin,
            Url       = pr.HtmlUrl,
            CreatedAt = pr.CreatedAt.ToString("yyyy-MM-dd")
        };
    }
}
