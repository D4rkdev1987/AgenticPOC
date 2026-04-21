import styled from "styled-components";
import type { WorkspaceResponse, RepoView, PortfolioItem } from "../types";

const colors = {
  accent:  "#46d9c3",
  muted:   "#a8b4c4",
  text:    "#ebf4ff",
  ok:      "#55d17a",
  warning: "#f2b94b",
  danger:  "#ff6b6b",
};

// ── Styled primitives ─────────────────────────────────────────────────────────

const Section = styled.section`
  grid-column: span 12;
  background: linear-gradient(170deg, rgba(18, 26, 36, 0.95), rgba(12, 18, 26, 0.85));
  border: 1px solid rgba(168, 180, 196, 0.2);
  border-radius: 16px;
  padding: 1rem 1.25rem;
  backdrop-filter: blur(6px);
`;

const Heading = styled.h2`
  margin: 0 0 0.2rem;
`;

const SubText = styled.p`
  margin: 0 0 1rem;
  font-size: 0.8rem;
  color: ${colors.muted};
`;

const AggRow = styled.div`
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
  margin-bottom: 1.2rem;
`;

const AggCard = styled.div`
  background: rgba(70, 217, 195, 0.07);
  border: 1px solid rgba(70, 217, 195, 0.2);
  border-radius: 10px;
  padding: 0.5rem 1rem;
`;

const AggLabel = styled.div`
  font-size: 0.7rem;
  color: ${colors.muted};
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const AggValue = styled.div`
  font-size: 1.4rem;
  font-weight: 700;
  color: ${colors.accent};
  font-family: "IBM Plex Mono", monospace;
`;

const RepoGrid = styled.div`
  display: grid;
  gap: 0.75rem;
`;

const RepoCard = styled.div<{ $hasError: boolean }>`
  border: 1px solid ${p => p.$hasError ? "rgba(255,107,107,0.4)" : "rgba(168,180,196,0.15)"};
  border-radius: 10px;
  padding: 0.75rem 1rem;
  background: rgba(255,255,255,0.02);
`;

const RepoHeader = styled.div`
  display: flex;
  align-items: baseline;
  gap: 0.6rem;
  margin-bottom: 0.5rem;
  flex-wrap: wrap;
`;

const RepoLink = styled.a`
  font-family: "IBM Plex Mono", monospace;
  font-size: 0.82rem;
  color: ${colors.accent};
  text-decoration: none;
  font-weight: 600;
  &:hover { text-decoration: underline; }
`;

const ProviderBadge = styled.span`
  font-size: 0.65rem;
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
  background: rgba(70, 217, 195, 0.12);
  color: ${colors.accent};
  text-transform: uppercase;
  letter-spacing: 0.06em;
`;

const SyncTime = styled.span`
  font-size: 0.7rem;
  color: ${colors.muted};
  margin-left: auto;
`;

const StatRow = styled.div`
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  font-size: 0.78rem;
  color: ${colors.muted};
`;

const StatItem = styled.span<{ $highlight?: string }>`
  color: ${p => p.$highlight ?? colors.muted};
  font-family: "IBM Plex Mono", monospace;
`;

const ErrorMsg = styled.p`
  color: ${colors.danger};
  font-size: 0.8rem;
  margin: 0.3rem 0 0;
`;

const LaneBadge = styled.span<{ $lane: string }>`
  display: inline-block;
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
  font-size: 0.7rem;
  font-weight: 600;
  background: ${p =>
    p.$lane === "prototype"  ? "rgba(70,217,195,0.12)"  :
    p.$lane === "hardening"  ? "rgba(85,209,122,0.12)"  :
    p.$lane === "hotfix"     ? "rgba(255,107,107,0.12)" :
    "rgba(168,180,196,0.1)"};
  color: ${p =>
    p.$lane === "prototype"  ? "#46d9c3" :
    p.$lane === "hardening"  ? "#55d17a" :
    p.$lane === "hotfix"     ? "#ff6b6b" :
    "#a8b4c4"};
`;

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
  font-size: 0.78rem;
  margin-top: 0.4rem;
`;

const Th = styled.th`
  text-align: left;
  padding: 0.3rem 0.5rem;
  color: ${colors.muted};
  font-weight: 500;
  border-bottom: 1px solid rgba(168,180,196,0.12);
`;

const Td = styled.td`
  padding: 0.3rem 0.5rem;
  border-bottom: 1px solid rgba(168,180,196,0.06);
`;

const PrLink = styled.a`
  color: ${colors.accent};
  text-decoration: none;
  &:hover { text-decoration: underline; }
`;

// ── Sub-components ────────────────────────────────────────────────────────────

function MiniPrTable({ items, emptyLabel }: { items: PortfolioItem[]; emptyLabel: string }) {
  if (items.length === 0)
    return <p style={{ fontSize: "0.75rem", color: colors.muted, margin: "0.3rem 0 0 0" }}>{emptyLabel}</p>;

  return (
    <Table>
      <thead>
        <tr>
          <Th>#</Th>
          <Th>Title</Th>
          <Th>Lane</Th>
          <Th>Age</Th>
          <Th>Author</Th>
        </tr>
      </thead>
      <tbody>
        {items.map(pr => (
          <tr key={`${pr.number}-${pr.branch}`}>
            <Td><PrLink href={pr.url} target="_blank" rel="noreferrer">#{pr.number}</PrLink></Td>
            <Td><PrLink href={pr.url} target="_blank" rel="noreferrer">{pr.title}</PrLink></Td>
            <Td><LaneBadge $lane={pr.lane}>{pr.lane}</LaneBadge></Td>
            <Td style={{ color: colors.muted }}>{pr.ageHours}h</Td>
            <Td style={{ color: colors.muted }}>{pr.author}</Td>
          </tr>
        ))}
      </tbody>
    </Table>
  );
}

function RepoCard_({ repo }: { repo: RepoView }) {
  const syncedRelative = new Date(repo.syncedAt).toLocaleTimeString();

  return (
    <RepoCard $hasError={!!repo.syncError}>
      <RepoHeader>
        <ProviderBadge>{repo.provider}</ProviderBadge>
        <RepoLink href={repo.repoUrl} target="_blank" rel="noreferrer">
          {repo.owner}/{repo.repo}
        </RepoLink>
        <SyncTime>synced {syncedRelative}</SyncTime>
      </RepoHeader>

      {repo.syncError ? (
        <ErrorMsg>Sync error: {repo.syncError}</ErrorMsg>
      ) : (
        <>
          <StatRow>
            <StatItem>{repo.totalPrs} PRs total</StatItem>
            <StatItem $highlight={colors.accent}>cycle {repo.metrics.cycleTime}</StatItem>
            <StatItem $highlight={colors.ok}>convert {repo.metrics.conversionRate}</StatItem>
            <StatItem $highlight={colors.danger}>kill {repo.metrics.killRate}</StatItem>
          </StatRow>
          <MiniPrTable
            items={[...repo.experiments, ...repo.hardening]}
            emptyLabel="No active PRs in this repo."
          />
        </>
      )}
    </RepoCard>
  );
}

// ── Main component ────────────────────────────────────────────────────────────

interface Props {
  data: WorkspaceResponse;
}

export function WorkspacePanel({ data }: Props) {
  const oldestSync = data.oldestSync ? new Date(data.oldestSync).toLocaleTimeString() : "—";

  return (
    <Section>
      <Heading>Workspace ({data.totalRepos} {data.totalRepos === 1 ? "repo" : "repos"})</Heading>
      <SubText>Last synced: {oldestSync} · refreshes every 2 min</SubText>

      <AggRow>
        <AggCard>
          <AggLabel>Aggregate Cycle Time</AggLabel>
          <AggValue>{data.aggregate.cycleTime}</AggValue>
        </AggCard>
        <AggCard>
          <AggLabel>Aggregate Conversion</AggLabel>
          <AggValue>{data.aggregate.conversionRate}</AggValue>
        </AggCard>
        <AggCard>
          <AggLabel>Aggregate Kill Rate</AggLabel>
          <AggValue>{data.aggregate.killRate}</AggValue>
        </AggCard>
      </AggRow>

      <RepoGrid>
        {data.repos.map(repo => (
          <RepoCard_ key={repo.key} repo={repo} />
        ))}
      </RepoGrid>
    </Section>
  );
}
