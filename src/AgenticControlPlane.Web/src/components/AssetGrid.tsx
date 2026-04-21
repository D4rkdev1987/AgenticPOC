import type { Asset } from "../types";
import {
  AssetCard,
  AssetList,
  AssetMeta,
  AssetTitle,
  PanelDescription,
  PanelHeading,
  ResultText,
  SpanTwoPanel,
} from "../styles/ui";

interface AssetGridProps {
  assets: Asset[];
}

export function AssetGrid({ assets }: AssetGridProps) {
  return (
    <SpanTwoPanel>
      <PanelHeading>Live Governance Assets</PanelHeading>
      <PanelDescription>Loaded from repository docs and workflow files.</PanelDescription>
      <AssetList>
        {assets.map((asset) => (
          <AssetCard key={asset.path}>
            <AssetTitle>{asset.label}</AssetTitle>
            <AssetMeta>{asset.path}</AssetMeta>
            {asset.ok ? (
              <ResultText $pass>
                {asset.kind === "markdown"
                  ? `${asset.summary?.sections ?? 0} sections, ${asset.summary?.bullets ?? 0} bullets`
                  : `${asset.summary?.jobs ?? 0} jobs | PR: ${asset.summary?.hasPullRequest ? "yes" : "no"} | Dispatch: ${asset.summary?.hasDispatch ? "yes" : "no"}`}
              </ResultText>
            ) : (
              <ResultText $pass={false}>{asset.error ?? "Missing asset"}</ResultText>
            )}
          </AssetCard>
        ))}
      </AssetList>
    </SpanTwoPanel>
  );
}
