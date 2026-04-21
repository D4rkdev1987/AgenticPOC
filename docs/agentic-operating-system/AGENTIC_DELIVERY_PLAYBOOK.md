# Agentic Delivery Playbook

## 1) Why this model exists

Classic delivery optimizes for output volume.
Agentic delivery optimizes for validated outcomes.

Shift from:
- Story -> Implement -> Review

Shift to:
- Hypothesis -> Build branch prototype -> Validate -> Promote or Kill

This increases speed and quality at the same time by separating exploration from production hardening.

## 2) Operating principles

1. Prototype in code, not slides.
2. Timebox aggressively (1-3 days).
3. Evidence beats opinion.
4. Promote few, kill many.
5. Production quality is a second lane, not skipped work.
6. Every experiment is reversible via flags and rollback.

## 3) Team lanes

### Lane A: Prototype Lane
- Mission: fast branch-based proof of behavior.
- Typical duration: 4-24 engineer hours.
- Tooling: coding agents, rapid scaffolding, synthetic/test data.
- Output: demo branch plus evidence packet.

### Lane B: Hardening Lane
- Mission: make validated prototypes production-grade.
- Typical duration: 2-10 days depending on risk.
- Tooling: test automation, observability, security checks, performance profiling.
- Output: merge-ready production change with rollout plan.

## 4) Standard flow

1. Draft a one-page hypothesis brief using the template.
2. Open an experiment branch and enable feature flag from day zero.
3. Build a thin vertical slice with agentic tooling.
4. Collect evidence (demo, metrics, logs, UX notes, tech risk).
5. Run the weekly portfolio review.
6. Decision:
   - Promote to hardening lane
   - Iterate once with strict 24-48h limit
   - Kill and archive learnings

## 5) Branch and PR conventions

Branch naming:
- exp/<domain>/<short-hypothesis>
- harden/<domain>/<feature-name>

PR labels:
- experiment
- promote-candidate
- hardened
- kill

PR required sections for experiments:
- Hypothesis
- User behavior changed
- Evidence snapshot
- Risks and unknowns
- Rollback approach

## 6) Guardrails (non-negotiable)

1. Feature flag required for all experiment behavior.
2. CI required for all branches:
   - build
   - lint
   - smoke tests
3. Security baseline scan on PR.
4. No direct production merge from experiment branch.
5. Promotion checklist must pass before merge.
6. Rollback command documented in PR.

## 7) Evidence pack minimum

Every experiment must include:
1. 2-5 minute demo recording or live walkthrough.
2. Before/after user flow map.
3. Instrumentation event list.
4. Early signal metrics:
   - activation
   - completion
   - latency impact
   - error rate impact
5. Engineering risk notes:
   - data model risk
   - integration risk
   - compliance/security risk

## 8) Decision rubric

Score 1-5 for each:
1. User value signal
2. Build-to-learn speed
3. Technical viability
4. Operational risk
5. Strategic fit

Decision rules:
- 20+ and no critical risk: Promote
- 14-19: one fast iteration only
- <=13: Kill

## 9) Delivery KPIs

Track weekly:
1. Time to first usable prototype (hours)
2. Prototype-to-production conversion rate
3. Experiment kill rate
4. Lead time from promote decision to production
5. Post-release defect rate for promoted features
6. Percent of promoted features behind flags

Target ranges after first 6 weeks:
- Time to first prototype: < 2 days median
- Kill rate: 40-70 percent
- Conversion rate: 15-35 percent
- Promote-to-prod lead time: < 10 working days median

## 10) 30-day rollout plan

Week 1:
- Agree lanes, roles, branch and PR conventions.
- Configure CI checks and feature flag policy.

Week 2:
- Run 3-5 experiments with strict timeboxes.
- Capture evidence packs.

Week 3:
- Promote top 1-2 experiments through hardening lane.
- Run staged rollout with telemetry.

Week 4:
- Review KPI baseline.
- Tighten quality gates.
- Remove low-value process friction.

## 11) Anti-patterns to avoid

1. Polishing prototypes too early.
2. Treating all ideas as roadmap commitments.
3. Skipping tests on promoted work.
4. Letting experiment branches age beyond one week.
5. Measuring only output instead of learning.

## 12) Leadership cadence

Weekly 45-minute Agentic Portfolio Review:
1. Top 3 promoted candidates
2. Killed experiments and learnings
3. Capacity split between lanes
4. Metrics trend and gate adjustments

Monthly 60-minute System Tuning:
1. Which guardrails are too strict or too weak
2. Tooling bottlenecks
3. Skills and training gaps
4. KPI target updates
