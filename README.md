# AgenticPOC

AgenticPOC is a control-plane dashboard for an Agentic Delivery model:
- Validate ideas quickly in a prototype lane
- Promote only proven candidates into hardening
- Track governance coverage, PR flow, and readiness signals

Mission:
> Ship better outcomes faster by validating ideas in code first, then hardening only what proves value.

## What this app does

- Reads governance assets from docs and workflow files
- Pulls PR data from GitHub or Bitbucket through a shared provider interface
- Classifies PRs into lanes and decisions
- Computes key metrics:
  - Cycle time
  - Conversion rate
  - Kill rate
- Supports multi-repo workspaces with aggregate metrics
- Runs background sync with in-memory cache for responsive API reads

## Stack

- Backend: ASP.NET Core 8 Minimal API
- Frontend: React + TypeScript + Vite + styled-components
- CI/CD policy: GitHub Actions workflows in .github/workflows

## Repository structure

- src/AgenticControlPlane.Api: backend API and sync services
- src/AgenticControlPlane.Web: frontend UI
- docs/agentic-operating-system: playbook, policy, charter, templates
- .github/workflows: policy and release workflows
- start.ps1: local startup script for API + Web app

## Local run

### Option A: one-command startup (recommended)

From repository root:

```powershell
./start.ps1
```

This script:
- Frees ports 5175, 5173, and 5174
- Starts API on http://localhost:5175
- Starts web app on http://localhost:5173 (or 5174 fallback)

### Option B: run services manually

API terminal:

```powershell
cd src/AgenticControlPlane.Api
dotnet restore
dotnet run --urls http://localhost:5175
```

Web terminal:

```powershell
cd src/AgenticControlPlane.Web
npm install
npm run dev
```

Open the app at http://localhost:5173.

## Environment configuration

API settings are loaded from src/AgenticControlPlane.Api/.env.

### Provider mode (single repo)

- VCS_PROVIDER=github with GITHUB_OWNER and GITHUB_REPO
- VCS_PROVIDER=bitbucket with BITBUCKET_WORKSPACE and BITBUCKET_REPO

Credentials:
- GitHub: GITHUB_TOKEN
- Bitbucket Cloud: BITBUCKET_USERNAME + BITBUCKET_APP_PASSWORD

### Workspace mode (multi repo)

Set REPOS as JSON array:

```env
REPOS=[{"provider":"github","owner":"org-or-user","repo":"repo-a"},{"provider":"bitbucket","owner":"workspace-slug","repo":"repo-b"}]
```

When REPOS is set, it is used as the tracked workspace list.

### Background sync interval

```env
SYNC_INTERVAL_SECONDS=120
```

Default is 120 seconds.

## API endpoints

- GET /api/health: service health
- GET /api/dashboard: governance assets + coverage + highlights + dashboard metrics
- GET /api/portfolio: single-repo portfolio view (legacy compatibility, now cache-backed)
- GET /api/workspace: all tracked repos + aggregate metrics
- GET /api/sync/status: sync observability by repo

Note: /api/portfolio and /api/workspace may return 202 Accepted while initial background sync is still warming cache.

## PR lane and decision model

Lane mapping by branch prefix:
- exp/: prototype
- harden/: hardening
- hotfix/: hotfix
- otherwise: other

Decision mapping:
- label kill: kill
- label hardened: promote
- label promote-candidate: promote-candidate
- closed/merged state without kill label: merged
- otherwise: open

## GitHub workflows

### 1) Agentic Delivery Gates
File: .github/workflows/agentic-delivery-gates.yml

Purpose:
- Applies labels from branch naming
- Enforces PR body sections and required prompts
- Prevents direct exp/* merges to main
- Requires promotion checklist section for hardening PRs

Trigger:
- pull_request on opened, edited, synchronize, reopened, ready_for_review

### 2) Hardening Checklist Gate
File: .github/workflows/hardening-checklist-gate.yml

Purpose:
- Validates required hardening checklist section in PR body
- Verifies required checkboxes are checked for harden/* PRs

Trigger:
- pull_request on opened, edited, synchronize, reopened, ready_for_review

### 3) Promotion Readiness Score
File: .github/workflows/promotion-readiness-score.yml

Purpose:
- Scores hardening PRs on a 100-point rubric
- Comments score and recommendation on the PR
- Fails workflow when critical categories fail

Scored categories:
- Product validation (20)
- Engineering quality (20)
- Security baseline (20)
- Operational readiness (20)
- Rollout clarity (20)

Trigger:
- pull_request on opened, edited, synchronize, reopened, ready_for_review

### 4) Staged Release
File: .github/workflows/staged-release.yml

Purpose:
- Manual deployment flow with canary and optional full rollout
- Validates release inputs and supports environment concurrency

Trigger:
- workflow_dispatch

Inputs:
- ref
- environment (staging or production)
- canary_percentage (1-100)
- approve_full_rollout (true or false)

## Governance docs consumed by dashboard

The dashboard scans these assets:

- docs/agentic-operating-system/AGENTIC_DELIVERY_PLAYBOOK.md
- docs/agentic-operating-system/REPO_POLICY_GUIDE.md
- docs/agentic-operating-system/TEAM_CHARTER_ONE_PAGE.md
- docs/agentic-operating-system/templates/HYPOTHESIS_BRIEF_TEMPLATE.md
- docs/agentic-operating-system/templates/PROMOTION_CHECKLIST.md
- docs/agentic-operating-system/templates/WEEKLY_PORTFOLIO_REVIEW.md
- .github/workflows/agentic-delivery-gates.yml
- .github/workflows/hardening-checklist-gate.yml
- .github/workflows/promotion-readiness-score.yml
- .github/workflows/staged-release.yml

## Troubleshooting

- API returns syncing response:
  - Wait for initial background sync, then retry /api/portfolio or /api/workspace.
- Empty workspace data:
  - Validate REPOS JSON format and credentials.
- Web cannot reach API:
  - Confirm API is running on http://localhost:5175 and Vite proxy is active.
- Bitbucket data missing:
  - Confirm app password scopes and workspace/repo slugs.

## Notes

- app/README.md describes archived app folder status.
- src/README.md contains service-level run notes.
