import type { CoverageItem } from "../types";
import { Checklist, ChecklistItem, Panel, PanelHeading } from "../styles/ui";

interface ChecklistPanelProps {
  title: string;
  items: CoverageItem[];
}

export function ChecklistPanel({ title, items }: ChecklistPanelProps) {
  return (
    <Panel>
      <PanelHeading>{title}</PanelHeading>
      <Checklist>
        {items.map((item) => (
          <ChecklistItem $pass={item.pass} key={item.label}>
            {item.pass ? "PASS" : "FAIL"} - {item.label}
          </ChecklistItem>
        ))}
      </Checklist>
    </Panel>
  );
}
