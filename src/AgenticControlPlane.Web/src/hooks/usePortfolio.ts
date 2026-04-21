import { useEffect, useState } from "react";
import { getPortfolio } from "../api/dashboardApi";
import type { PortfolioResponse } from "../types";

export function usePortfolio() {
  const [data, setData] = useState<PortfolioResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getPortfolio()
      .then((response) => {
        setData(response);
        setError(null);
      })
      .catch((err: Error) => {
        setError(err.message);
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  return { data, error, loading };
}
