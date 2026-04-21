import type { SimulatorCategory } from "../types";

export const SIMULATOR_DEFAULTS: SimulatorCategory[] = [
  { key: "product", label: "Product validation", weight: 20, critical: false, checked: true },
  { key: "engineering", label: "Engineering quality", weight: 20, critical: false, checked: true },
  { key: "security", label: "Security baseline", weight: 20, critical: true, checked: true },
  { key: "operational", label: "Operational readiness", weight: 20, critical: true, checked: true },
  { key: "rollout", label: "Rollout clarity", weight: 20, critical: true, checked: true },
];
