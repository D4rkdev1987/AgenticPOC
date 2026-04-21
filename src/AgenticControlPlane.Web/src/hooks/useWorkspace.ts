import { useEffect, useState } from "react";
import { getWorkspace } from "../api/dashboardApi";
import type { WorkspaceResponse } from "../types";

export function useWorkspace() {
  const [data, setData]       = useState<WorkspaceResponse | null>(null);
  const [error, setError]     = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const result = await getWorkspace();
        if (!cancelled) setData(result);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : String(err));
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    void load();
    return () => { cancelled = true; };
  }, []);

  return { data, error, loading };
}
