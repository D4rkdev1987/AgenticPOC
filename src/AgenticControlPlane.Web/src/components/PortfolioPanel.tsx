import styled from "styled-components";
import type { PortfolioItem, PortfolioResponse } from "../types";

const colors = {
  accent: "#46d9c3",
  muted: "#a8b4c4",
  text: "#ebf4ff",
  ok: "#55d17a",
  warning: "#f2b94b",
  danger: "#ff6b6b",
};

const Section = styled.section`
  grid-column: span 12;
  background: linear-gradient(170deg, rgba(18, 26, 36, 0.95), rgba(12, 18, 26, 0.85));
  border: 1px solid rgba(168, 180, 196, 0.2);
  border-radius: 16px;
  padding: 1rem;
  backdrop-filter: blur(6px);
`;

const Heading = styled.h2`
  margin: 0 0 0.3rem;
`;

const RepoLink = styled.a`
  font-family: "IBM Plex Mono", monospace;
  font-size: 0.78rem;
  color: ${colors.accent};
  text-decoration: none;
  &:hover { text-decoration: underline; }
`;

const SubHeading = styled.h3`
  margin: 1.2rem 0 0.5rem;
  font-size: 1rem;
  color: ${colors.muted};
`;

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
`;

const Th = styled.th`
  text-align: left;
  padding: 0.4rem 0.6rem;
  color: ${colors.muted};
  font-weight: 500;
  border-bottom: 1px solid rgba(168, 180, 196, 0.15);
`;

const Td = styled.td`
  padding: 0.4rem 0.6rem;
  border-bottom: 1px solid rgba(168, 180, 196, 0.07);
  color: ${colors.text};
  vertical-align: middle;
`;

const LaneBadge = styled.span<{ $lane: string }>`
  display: inline-block;
  font-size: 0.72rem;
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  font-family: "IBM Plex Mono", monospace;
  background: ${({ $lane }) =>
    $lane === "prototype" ? "rgba(70,217,195,0.15)"
    : $lane === "hardening" ? "rgba(85,209,122,0.15)"
    : $lane === "hotfix" ? "rgba(255,107,107,0.15)"
    : "rgba(168,180,196,0.1)"};
  color: ${({ $lane }) =>
    $lane === "prototype" ? colors.accent
    : $lane === "hardening" ? colors.ok
    : $lane === "hotfix" ? colors.danger
    : colors.muted};
`;

const DecisionBadge = styled.span<{ $decision: string }>`
  display: inline-block;
  font-size: 0.72rem;
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  font-family: "IBM Plex Mono", monospace;
  background: ${({ $decision }) =>
    $decision === "promote" || $decision === "merged" ? "rgba(85,209,122,0.15)"
    : $decision === "kill" ? "rgba(255,107,107,0.15)"
    : $decision === "promote-candidate" ? "rgba(242,185,75,0.15)"
    : "rgba(168,180,196,0.1)"};
  color: ${({ $decision }) =>
    $decision === "promote" || $decision === "merged" ? colors.ok
    : $decision === "kill" ? colors.danger
    : $decision === "promote-candidate" ? colors.warning
    : colors.muted};
`;

const PrLink = styled.a`
  color: ${colors.text};
  text-decoration: none;
  &:hover { color: ${colors.accent}; }
`;

const Empty = styled.p`
  color: ${colors.muted};
  font-style: italic;
  font-size: 0.85rem;
`;

type GroupKey = "experiments" | "hardening" | "promoted" | "killed";

const groups: { key: GroupKey; label: string }[] = [
  { key: "experiments", label: "Prototype Lane (exp/)" },
  { key: "hardening", label: "Hardening Lane (harden/)" },
  { key: "promoted", label: "Promoted / Merged" },
  { key: "killed", label: "Killed" },
];

function PrTable({ items }: { items: PortfolioItem[] }) {
  if (items.length === 0) return <Empty>No items in this group.</Empty>;

  return (
    <Table>
      <thead>
        <tr>
          <Th>#</Th>
          <Th>Title</Th>
          <Th>Branch</Th>
          <Th>Lane</Th>
          <Th>Decision</Th>
          <Th>Age (h)</Th>
          <Th>Author</Th>
        </tr>
      </thead>
      <tbody>
        {items.map((pr) => (
          <tr key={pr.number}>
            <Td><PrLink href={pr.url} target="_blank" rel="noreferrer">#{pr.number}</PrLink></Td>
            <Td><PrLink href={pr.url} target="_blank" rel="noreferrer">{pr.title}</PrLink></Td>
            <Td style={{ fontFamily: '"IBM Plex Mono", monospace', fontSize: "0.75rem", color: colors.muted }}>{pr.branch}</Td>
            <Td><LaneBadge $lane={pr.lane}>{pr.lane}</LaneBadge></Td>
            <Td><DecisionBadge $decision={pr.decision}>{pr.decision}</DecisionBadge></Td>
            <Td>{pr.ageHours}h</Td>
            <Td>{pr.author}</Td>
          </tr>
        ))}
      </tbody>
    </Table>
  );
}

interface Props {
  data: PortfolioResponse;
}

export function PortfolioPanel({ data }: Props) {
  const providerLabel = data.repoMeta.provider ?? "GitHub";
  return (
    <Section>
      <Heading>Live Portfolio</Heading>
      <RepoLink href={data.repoMeta.url} target="_blank" rel="noreferrer">
        {data.repoMeta.owner}/{data.repoMeta.repo} ({providerLabel})
      </RepoLink>

      {groups.map(({ key, label }) => (
        <div key={key}>
          <SubHeading>{label} ({data[key].length})</SubHeading>
          <PrTable items={data[key]} />
        </div>
      ))}
    </Section>
  );
}
