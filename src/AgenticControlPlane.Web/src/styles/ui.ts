import styled, { createGlobalStyle, keyframes } from "styled-components";

const colors = {
  text: "#ebf4ff",
  muted: "#a8b4c4",
  accent: "#46d9c3",
  warning: "#f2b94b",
  danger: "#ff6b6b",
  ok: "#55d17a",
  ring: "rgba(70, 217, 195, 0.28)",
};

const drift = keyframes`
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
`;

export const GlobalStyle = createGlobalStyle`
  * {
    box-sizing: border-box;
  }

  body {
    margin: 0;
    color: ${colors.text};
    background: radial-gradient(circle at 20% 20%, #1d3146 0%, transparent 45%),
      radial-gradient(circle at 80% 0%, #173341 0%, transparent 40%),
      linear-gradient(160deg, #090d13 0%, #0d141d 60%, #0a1119 100%);
    font-family: "Space Grotesk", sans-serif;
    min-height: 100vh;
    overflow-x: hidden;
  }
`;

export const Aurora = styled.div`
  position: fixed;
  inset: -20vmax;
  background: conic-gradient(
    from 180deg at 50% 50%,
    rgba(70, 217, 195, 0.06),
    rgba(242, 185, 75, 0.04),
    rgba(85, 209, 122, 0.05),
    rgba(70, 217, 195, 0.06)
  );
  filter: blur(60px);
  animation: ${drift} 16s linear infinite;
  pointer-events: none;
`;

export const TopBarContainer = styled.header`
  display: flex;
  justify-content: space-between;
  align-items: end;
  padding: 2rem clamp(1rem, 4vw, 3rem) 1rem;
  position: relative;
`;

export const Kicker = styled.p`
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: ${colors.accent};
  font-size: 0.78rem;
`;

export const PageTitle = styled.h1`
  margin: 0.25rem 0 0;
  font-size: clamp(1.8rem, 5vw, 3rem);
`;

export const StatusPill = styled.div<{ $error: boolean }>`
  font-family: "IBM Plex Mono", monospace;
  font-size: 0.82rem;
  padding: 0.5rem 0.75rem;
  border: 1px solid ${colors.ring};
  border-radius: 999px;
  background: rgba(18, 26, 36, 0.75);
  color: ${({ $error }) => ($error ? colors.danger : colors.ok)};
`;

export const MainGrid = styled.main`
  display: grid;
  gap: 1rem;
  padding: 0 1rem 2rem;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  position: relative;

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
  }
`;

export const Panel = styled.section`
  grid-column: span 6;
  background: linear-gradient(170deg, rgba(18, 26, 36, 0.95), rgba(12, 18, 26, 0.85));
  border: 1px solid rgba(168, 180, 196, 0.2);
  border-radius: 16px;
  padding: 1rem;
  backdrop-filter: blur(6px);

  @media (max-width: 900px) {
    grid-column: span 1;
  }
`;

export const HeroPanel = styled(Panel)`
  grid-column: span 12;
`;

export const SpanTwoPanel = styled(Panel)`
  grid-column: span 12;
`;

export const PanelHeading = styled.h2`
  margin: 0 0 0.4rem;
`;

export const PanelDescription = styled.p`
  margin-top: 0;
  color: ${colors.muted};
`;

export const MetricRow = styled.div`
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.75rem;

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
  }
`;

export const MetricCard = styled.article`
  background: #1a2634;
  border: 1px solid rgba(168, 180, 196, 0.2);
  border-radius: 12px;
  padding: 0.75rem;
`;

export const MetricLabel = styled.h3`
  margin: 0;
  font-size: 0.9rem;
  color: ${colors.muted};
`;

export const MetricValue = styled.p`
  margin: 0.5rem 0 0.25rem;
  color: ${colors.text};
  font-size: 1.4rem;
  font-weight: 700;
`;

export const MetricHint = styled.small`
  color: ${colors.muted};
`;

export const SimGrid = styled.div`
  display: grid;
  gap: 0.5rem;
`;

export const SimItem = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(26, 38, 52, 0.7);
  border: 1px solid rgba(168, 180, 196, 0.15);
  border-radius: 10px;
  padding: 0.55rem 0.65rem;
`;

export const SimLabel = styled.label`
  display: flex;
  align-items: center;
  gap: 0.55rem;

  input[type="checkbox"] {
    accent-color: ${colors.accent};
  }
`;

export const ScoreBox = styled.div`
  margin-top: 0.75rem;
  display: flex;
  align-items: baseline;
  gap: 0.8rem;
  font-family: "IBM Plex Mono", monospace;
`;

export const ToneText = styled.span<{ $tone: "good" | "warn" | "bad" }>`
  color: ${({ $tone }) => ($tone === "good" ? colors.ok : $tone === "warn" ? colors.warning : colors.danger)};
`;

export const AssetList = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 0.75rem;
`;

export const AssetCard = styled.article`
  border: 1px solid rgba(168, 180, 196, 0.2);
  border-radius: 12px;
  padding: 0.75rem;
  background: rgba(26, 38, 52, 0.55);
`;

export const AssetTitle = styled.h3`
  margin: 0 0 0.35rem;
  font-size: 0.95rem;
`;

export const AssetMeta = styled.p`
  font-family: "IBM Plex Mono", monospace;
  font-size: 0.75rem;
  color: ${colors.muted};
`;

export const ResultText = styled.p<{ $pass: boolean }>`
  color: ${({ $pass }) => ($pass ? colors.ok : colors.danger)};
`;

export const Checklist = styled.ul`
  margin: 0;
  padding-left: 1.1rem;
  color: ${colors.muted};
`;

export const ChecklistItem = styled.li<{ $pass: boolean }>`
  color: ${({ $pass }) => ($pass ? colors.ok : colors.danger)};

  & + & {
    margin-top: 0.4rem;
  }
`;
