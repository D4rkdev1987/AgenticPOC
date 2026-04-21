import type { DashboardResponse, PortfolioResponse, WorkspaceResponse } from "../types";

export async function getDashboard(): Promise<DashboardResponse> {
  const response = await fetch("/api/dashboard", { cache: "no-store" });
  if (!response.ok) {
    throw new Error(`Failed to load dashboard data: ${response.status}`);
  }
  return (await response.json()) as DashboardResponse;
}

export async function getPortfolio(): Promise<PortfolioResponse> {
  const response = await fetch("/api/portfolio", { cache: "no-store" });
  if (!response.ok) {
    throw new Error(`Failed to load portfolio data: ${response.status}`);
  }
  return (await response.json()) as PortfolioResponse;
}

export async function getWorkspace(): Promise<WorkspaceResponse> {
  const response = await fetch("/api/workspace", { cache: "no-store" });
  if (!response.ok) {
    throw new Error(`Failed to load workspace data: ${response.status}`);
  }
  return (await response.json()) as WorkspaceResponse;
}
