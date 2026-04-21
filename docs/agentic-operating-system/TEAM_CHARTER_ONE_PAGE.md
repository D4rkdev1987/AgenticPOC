# Team Charter (One Page)

## Mission
Ship better outcomes faster by validating ideas in code first, then hardening only what proves value.

## Operating Model
We run two lanes:
1. Prototype Lane: rapid branch experiments to test hypotheses.
2. Hardening Lane: productionization of validated candidates.

## Roles and Decision Rights

Product Lead:
- Owns problem framing and success signals.
- Decides promote/kill with Engineering Lead based on evidence.

Engineering Lead:
- Owns technical viability and risk posture.
- Final decision on production readiness for hardening merges.

Prototype Engineer(s):
- Build thin vertical slices quickly.
- Must include feature flags and instrumentation from day zero.

Hardening Engineer(s):
- Convert validated behavior to durable architecture.
- Own tests, observability, and rollout controls.

Design Partner:
- Validates usability and interaction quality in prototype stage.

Data Partner:
- Validates metric definitions and reads evidence quality.

Security Partner:
- Reviews high-risk changes and approves required controls.

## Working Agreements

1. Every experiment starts with a written hypothesis.
2. Every experiment is timeboxed to 1-3 days.
3. Every experiment ends with a decision: promote, iterate once, or kill.
4. No direct production merge from experiment branch.
5. Every promoted feature ships behind a feature flag.
6. Every production rollout has stop conditions and owner on point.

## Required Artifacts

For each experiment:
- Hypothesis brief
- Demo or walkthrough
- Evidence snapshot
- Risk notes

For each promotion:
- Completed promotion checklist
- Rollout plan
- Post-merge metric review date

## Cadence

Daily (15 min):
- Lane standup, blockers, risk escalation.

Weekly (45 min):
- Portfolio review of promote/kill decisions.

Monthly (60 min):
- System tuning on gates, metrics, and tooling.

## KPI Scoreboard

Track weekly:
1. Time to first prototype (median hours)
2. Prototype-to-production conversion rate
3. Experiment kill rate
4. Promote-to-production lead time
5. Defect rate on promoted features

## Escalation Rules

Escalate immediately if:
- Security/compliance uncertainty exists
- User data model change is irreversible
- Rollback cannot be executed safely
- SLO risk exceeds agreed threshold

## Definition of Success

We are successful when:
- Teams learn faster than they build waste.
- High-confidence features reach production quickly.
- Reliability and safety do not regress while speed increases.
