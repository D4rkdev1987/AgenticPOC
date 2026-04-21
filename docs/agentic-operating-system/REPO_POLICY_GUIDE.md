# Repo Policy Guide

This guide defines labels, branch protection, and merge controls for agentic delivery.

## 1) Label Taxonomy

Core lifecycle labels:
- `experiment`: Branch PR validating a hypothesis.
- `promote-candidate`: Evidence indicates likely promotion.
- `hardened`: Production-grade implementation in progress or complete.
- `kill`: Experiment closed with documented learnings.

Risk and governance labels:
- `needs-security-review`
- `needs-data-review`
- `needs-legal-review`
- `high-risk-change`

Priority and planning labels:
- `p0`, `p1`, `p2`
- `prototype-lane`, `hardening-lane`

## 2) Branch Naming Rules

- `exp/<domain>/<short-hypothesis>`
- `harden/<domain>/<feature-name>`
- `hotfix/<domain>/<incident-key>`

Examples:
- `exp/onboarding/prefill-business-details`
- `harden/payments/one-click-refund`

## 3) Branch Protection Baseline

Apply to protected branches: `main`, `release/*`

Required settings:
1. Require pull request before merge.
2. Require at least 1 approval for experiments, 2 approvals for hardening and hotfix.
3. Require conversation resolution before merge.
4. Require status checks to pass:
   - build
   - lint
   - unit-tests
   - smoke-tests
   - security-scan
5. Require branch to be up to date before merge.
6. Restrict force pushes and deletions.

Recommended settings:
1. Require signed commits where feasible.
2. Require code owners review for critical paths.
3. Enforce linear history if your release model supports it.

## 4) Merge Policy

For `experiment` PRs:
1. Merge only into non-production integration branches or behind disabled flags.
2. Must include hypothesis and evidence sections in PR template.
3. Timebox to 1-3 days, then decide: promote, iterate once, or kill.

For `hardened` PRs:
1. Must pass promotion checklist.
2. Must define rollout strategy and stop conditions.
3. Must include observability updates.

## 5) Decision SLA

- Experiment PR decision target: <= 3 business days.
- Promotion candidate decision target: <= 2 business days.
- Hardening PR review target: <= 2 business days.

## 6) Required Automation

1. Auto-apply `experiment` label to `exp/*` branches.
2. Auto-apply `hardened` label to `harden/*` branches.
3. Block merge if PR template required sections are empty.
4. Block merge if feature flag or rollback section is missing.

Implemented starter workflow:
- `.github/workflows/agentic-delivery-gates.yml`
- `.github/workflows/hardening-checklist-gate.yml`
- `.github/workflows/staged-release.yml`
- `.github/workflows/promotion-readiness-score.yml`
- `scripts/bootstrap-github-labels.ps1`

Activation notes:
1. Ensure repository labels exist (`experiment`, `hardened`, `high-risk-change`, and optional governance labels).
2. Keep branch protection enabled for `main` and `release/*` because reviewer counts and approval rules are enforced there.
3. Add this workflow as a required status check in branch protection.
4. Add both policy workflows as required checks:
   - `Enforce Agentic PR Policy`
   - `Validate Hardening Checklist`
5. Add `Score Hardening Promotion Readiness` as a required check for hardening merge targets.
5. Run `scripts/bootstrap-github-labels.ps1` after `gh auth login` to create/update labels automatically.

Staged release usage:
1. Run `Staged Release` from GitHub Actions via `workflow_dispatch`.
2. Deploy canary with `approve_full_rollout=false`.
3. Validate canary signals.
4. Re-run with `approve_full_rollout=true` to promote to 100 percent rollout.

Promotion readiness score usage:
1. On any `harden/*` PR, the workflow posts or updates a structured score comment.
2. Use score thresholds for decisions:
   - 85-100: Promote
   - 70-84: Promote with caution
   - 50-69: One focused iteration
   - <50: Block
3. Must-pass categories always block promotion if they fail, regardless of total score:
   - Security baseline
   - Operational readiness
   - Rollout clarity
4. Keep human judgment final, but use the score to standardize review quality and speed.

## 7) Minimal CODEOWNERS Strategy

Use path-based ownership for risk-heavy modules:
- payments, billing, auth, data-export, admin controls

Pattern:
- Critical paths require domain owner review.
- Security-sensitive paths require security owner review.

## 8) Portfolio Hygiene Rules

1. No experiment branch older than 7 days without explicit extension.
2. Every killed experiment must include one learning note.
3. Every promoted feature must have a post-merge metric review date.

## 9) Starter Policy Snippet

Use this text in repo governance docs:

"This repository follows an agentic delivery model. We optimize for rapid validation via short-lived experiment branches and strict production hardening gates. Shipping speed is encouraged only when changes are reversible, observable, and secure."
