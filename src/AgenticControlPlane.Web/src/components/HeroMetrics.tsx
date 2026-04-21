import type { DashboardMetrics } from "../types";
import {
  HeroPanel,
  MetricCard,
  MetricHint,
  MetricLabel,
  MetricRow,
  MetricValue,
  PanelDescription,
  PanelHeading,
} from "../styles/ui";

interface HeroMetricsProps {
  metrics: DashboardMetrics | null;
}

export function HeroMetrics({ metrics }: HeroMetricsProps) {
  return (
    <HeroPanel>
      <PanelHeading>POC -&gt; Production Engine</PanelHeading>
      <PanelDescription>This program turns your operating docs and workflows into a live governance dashboard.</PanelDescription>
      <MetricRow>
        <MetricCard>
          <MetricLabel>Experiment Cycle</MetricLabel>
          <MetricValue>{metrics?.cycleTime ?? "--"}</MetricValue>
          <MetricHint>Median hours to first prototype</MetricHint>
        </MetricCard>
        <MetricCard>
          <MetricLabel>Conversion</MetricLabel>
          <MetricValue>{metrics?.conversionRate ?? "--"}</MetricValue>
          <MetricHint>Prototype -&gt; production</MetricHint>
        </MetricCard>
        <MetricCard>
          <MetricLabel>Kill Rate</MetricLabel>
          <MetricValue>{metrics?.killRate ?? "--"}</MetricValue>
          <MetricHint>Healthy signal if intentional</MetricHint>
        </MetricCard>
      </MetricRow>
    </HeroPanel>
  );
}
