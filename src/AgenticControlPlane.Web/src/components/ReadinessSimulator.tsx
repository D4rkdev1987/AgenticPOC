import { useMemo, useState } from "react";
import { SIMULATOR_DEFAULTS } from "../data/simulatorDefaults";
import { Panel, PanelDescription, PanelHeading, ScoreBox, SimGrid, SimItem, SimLabel, ToneText } from "../styles/ui";
import type { SimulatorCategory } from "../types";

type SimulatorTone = "good" | "warn" | "bad";
type SimulatorState = {
  score: number;
  recommendation: string;
  tone: SimulatorTone;
};

export function ReadinessSimulator() {
  const [categories, setCategories] = useState<SimulatorCategory[]>(SIMULATOR_DEFAULTS);

  const simulator = useMemo<SimulatorState>(() => {
    const score = categories.filter((c) => c.checked).reduce((sum, c) => sum + c.weight, 0);
    const criticalFailures = categories.filter((c) => c.critical && !c.checked).map((c) => c.label);

    if (criticalFailures.length > 0) {
      return {
        score,
        recommendation: `Block (critical: ${criticalFailures.join(", ")})`,
        tone: "bad",
      };
    }

    if (score >= 85) return { score, recommendation: "Promote", tone: "good" };
    if (score >= 70) return { score, recommendation: "Promote with caution", tone: "warn" };
    if (score >= 50) return { score, recommendation: "One focused iteration", tone: "warn" };

    return { score, recommendation: "Block", tone: "bad" };
  }, [categories]);

  return (
    <Panel>
      <PanelHeading>Readiness Simulator</PanelHeading>
      <PanelDescription>Model hardening outcomes before opening a promotion PR.</PanelDescription>
      <SimGrid>
        {categories.map((cat) => (
          <SimItem key={cat.key}>
            <SimLabel>
              <input
                type="checkbox"
                checked={cat.checked}
                onChange={(event) => {
                  const next = categories.map((current) =>
                    current.key === cat.key ? { ...current, checked: event.target.checked } : current
                  );
                  setCategories(next);
                }}
              />
              <span>
                {cat.label} ({cat.weight}){cat.critical ? " *must-pass" : ""}
              </span>
            </SimLabel>
          </SimItem>
        ))}
      </SimGrid>
      <ScoreBox>
        <span>Score</span>
        <strong>{simulator.score}/100</strong>
        <ToneText $tone={simulator.tone}>{simulator.recommendation}</ToneText>
      </ScoreBox>
    </Panel>
  );
}
