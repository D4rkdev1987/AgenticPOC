import { AssetGrid, ChecklistPanel, HeroMetrics, PortfolioPanel, ReadinessSimulator, TopBar } from "./components";
import { WorkspacePanel } from "./components/WorkspacePanel";
import { useDashboard } from "./hooks/useDashboard";
import { usePortfolio } from "./hooks/usePortfolio";
import { useWorkspace } from "./hooks/useWorkspace";
import { Aurora, GlobalStyle, MainGrid } from "./styles/ui";

export function App() {
  const { data, error, loading } = useDashboard();
  const { data: portfolio } = usePortfolio();
  const { data: workspace } = useWorkspace();

  const statusText = loading
    ? "Loading workspace assets..."
    : error
      ? error
      : `${data?.assets.filter((x) => x.ok).length ?? 0}/${data?.assets.length ?? 0} assets loaded`;

  return (
    <>
      <GlobalStyle />
      <Aurora />
      <TopBar statusText={statusText} hasError={Boolean(error)} />

      <MainGrid>
        <HeroMetrics metrics={workspace?.aggregate ?? portfolio?.metrics ?? data?.metrics ?? null} />
        <ReadinessSimulator />
        {workspace && <WorkspacePanel data={workspace} />}
        {portfolio && <PortfolioPanel data={portfolio} />}
        <AssetGrid assets={data?.assets ?? []} />
        <ChecklistPanel title="Workflow Coverage" items={data?.workflowCoverage ?? []} />
        <ChecklistPanel title="Operating Highlights" items={data?.highlights ?? []} />
      </MainGrid>
    </>
  );
}
