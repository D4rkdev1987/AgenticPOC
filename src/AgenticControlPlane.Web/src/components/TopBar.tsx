import { Kicker, PageTitle, StatusPill, TopBarContainer } from "../styles/ui";

interface TopBarProps {
  statusText: string;
  hasError: boolean;
}

export function TopBar({ statusText, hasError }: TopBarProps) {
  return (
    <TopBarContainer>
      <div>
        <Kicker>Agentic Delivery</Kicker>
        <PageTitle>Control Plane</PageTitle>
      </div>
      <StatusPill $error={hasError}>{statusText}</StatusPill>
    </TopBarContainer>
  );
}
