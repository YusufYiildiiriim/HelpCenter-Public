import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";
import { useStatusMutation } from "@/hooks/admin/useStatusesQuery";
import { queryKeys } from "@/lib/query/queryKeys";

describe("useStatusMutation", () => {
  it("successful mutation invalidates every cached status list", async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    const listKey = queryKeys.admin.statuses.list({ pageNumber: 1, pageSize: 10, search: "" });
    queryClient.setQueryData(listKey, { items: [], totalCount: 0, totalPages: 1, currentPage: 1, pageSize: 10 });

    function wrapper({ children }: { children: ReactNode }) {
      return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
    }

    const { result } = renderHook(
      () => useStatusMutation(async (name: string) => ({ name })),
      { wrapper },
    );

    await act(async () => {
      await result.current.mutateAsync("Yeni durum");
    });

    await waitFor(() => {
      expect(queryClient.getQueryState(listKey)?.isInvalidated).toBe(true);
    });
  });
});
