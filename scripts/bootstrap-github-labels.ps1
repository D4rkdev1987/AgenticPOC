param(
    [Parameter(Mandatory = $false)]
    [string]$Owner,

    [Parameter(Mandatory = $false)]
    [string]$Repo
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-Command {
    param([string]$Name)
    return [bool](Get-Command -Name $Name -ErrorAction SilentlyContinue)
}

function Resolve-RepoFromGitRemote {
    $url = git remote get-url origin 2>$null
    if (-not $url) {
        throw 'Could not resolve origin remote. Pass -Owner and -Repo explicitly.'
    }

    if ($url -match 'github.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)(\.git)?$') {
        return @{ Owner = $Matches.owner; Repo = $Matches.repo }
    }

    throw "Origin remote does not look like GitHub: $url"
}

if (-not (Test-Command -Name 'gh')) {
    throw 'GitHub CLI (gh) is required. Install from https://cli.github.com/ and run gh auth login.'
}

if (-not $Owner -or -not $Repo) {
    $resolved = Resolve-RepoFromGitRemote
    if (-not $Owner) { $Owner = $resolved.Owner }
    if (-not $Repo) { $Repo = $resolved.Repo }
}

Write-Host "Bootstrapping labels for $Owner/$Repo"

$labels = @(
    @{ name = 'experiment'; color = '0E8A16'; description = 'Branch PR validating a hypothesis' },
    @{ name = 'promote-candidate'; color = '1D76DB'; description = 'Evidence indicates likely promotion' },
    @{ name = 'hardened'; color = '5319E7'; description = 'Production hardening in progress or complete' },
    @{ name = 'kill'; color = 'B60205'; description = 'Experiment closed with documented learnings' },
    @{ name = 'high-risk-change'; color = 'D93F0B'; description = 'Requires deeper review due to operational risk' },
    @{ name = 'needs-security-review'; color = 'FBCA04'; description = 'Security review required before merge' },
    @{ name = 'needs-data-review'; color = 'FBCA04'; description = 'Data review required before merge' },
    @{ name = 'needs-legal-review'; color = 'FBCA04'; description = 'Legal/compliance review required before merge' },
    @{ name = 'prototype-lane'; color = 'C2E0C6'; description = 'Work item currently in prototype lane' },
    @{ name = 'hardening-lane'; color = 'BFDADC'; description = 'Work item currently in hardening lane' },
    @{ name = 'p0'; color = 'B60205'; description = 'Highest priority' },
    @{ name = 'p1'; color = 'D93F0B'; description = 'High priority' },
    @{ name = 'p2'; color = 'FBCA04'; description = 'Normal priority' }
)

foreach ($label in $labels) {
    $name = $label.name
    $color = $label.color
    $description = $label.description

    try {
        gh api "repos/$Owner/$Repo/labels/$name" 1>$null 2>$null
        gh api "repos/$Owner/$Repo/labels/$name" --method PATCH -f name="$name" -f color="$color" -f description="$description" 1>$null
        Write-Host "Updated label: $name"
    }
    catch {
        gh api "repos/$Owner/$Repo/labels" --method POST -f name="$name" -f color="$color" -f description="$description" 1>$null
        Write-Host "Created label: $name"
    }
}

Write-Host 'Label bootstrap complete.'
Write-Host 'Next: set required status checks in branch protection to include Agentic Delivery Gates and Hardening Checklist Gate.'
