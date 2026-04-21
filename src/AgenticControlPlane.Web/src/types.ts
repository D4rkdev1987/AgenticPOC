export interface MarkdownSummary {
  sections: number;
  bullets: number;
}

export interface WorkflowSummary {
  jobs: number;
  hasPullRequest: boolean;
  hasDispatch: boolean;
}

export type AssetSummary = MarkdownSummary | WorkflowSummary;

export interface Asset {
  label: string;
  path: string;
  kind: "markdown" | "workflow";
  ok: boolean;
  summary: AssetSummary | null;
  error: string | null;
}

export interface CoverageItem {
  label: string;
  pass: boolean;
}

export interface DashboardMetrics {
  cycleTime: string;
  conversionRate: string;
  killRate: string;
}

export interface DashboardResponse {
  assets: Asset[];
  workflowCoverage: CoverageItem[];
  highlights: CoverageItem[];
  metrics: DashboardMetrics;
}

export interface SimulatorCategory {
  key: string;
  label: string;
  weight: number;
  critical: boolean;
  checked: boolean;
}

export interface PortfolioItem {
  number: number;
  title: string;
  branch: string;
  lane: "prototype" | "hardening" | "hotfix" | "other";
  state: string;
  labels: string[];
  decision: string;
  ageHours: number;
  author: string;
  url: string;
  createdAt: string;
}

export interface PortfolioResponse {
  repoMeta: { owner: string; repo: string; url: string; provider: string; syncedAt?: string };
  items: PortfolioItem[];
  experiments: PortfolioItem[];
  hardening: PortfolioItem[];
  promoted: PortfolioItem[];
  killed: PortfolioItem[];
  metrics: DashboardMetrics;
}

// ── Phase 3: Workspace types ──────────────────────────────────────────────────

export interface RepoView {
  key: string;
  provider: string;
  owner: string;
  repo: string;
  repoUrl: string;
  syncedAt: string;
  syncError: string | null;
  totalPrs: number;
  experiments: PortfolioItem[];
  hardening: PortfolioItem[];
  promoted: PortfolioItem[];
  killed: PortfolioItem[];
  metrics: DashboardMetrics;
}

export interface WorkspaceResponse {
  repos: RepoView[];
  totalRepos: number;
  oldestSync: string | null;
  aggregate: DashboardMetrics;
}
