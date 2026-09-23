"use client";

import {
  useMutation,
  useQuery,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { AdminStatusService, type RequestStatus } from "@/services/admin/AdminStatusService";
import { queryKeys, type StatusListQuery } from "@/lib/query/queryKeys";

export interface PaginatedStatuses {
  items: RequestStatus[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export function useStatusesQuery(params: StatusListQuery) {
  return useQuery<PaginatedStatuses, unknown>({
    queryKey: queryKeys.admin.statuses.list(params),
    queryFn: () => AdminStatusService.getPaginated(params),
    placeholderData: (previousData) => previousData,
  });
}

/**
 * Statuses currently have no write endpoint. Keep invalidation beside their
 * query key so a future create/update/delete mutation cannot leave a stale
 * status list in the cache.
 */
export function useStatusMutation<TData, TVariables>(
  mutationFn: (variables: TVariables) => Promise<TData>,
  options?: Omit<UseMutationOptions<TData, unknown, TVariables>, "mutationFn" | "onSuccess"> & {
    onSuccess?: (data: TData, variables: TVariables) => void | Promise<void>;
  },
) {
  const queryClient = useQueryClient();

  return useMutation<TData, unknown, TVariables>({
    ...options,
    mutationFn,
    onSuccess: async (data, variables) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.admin.statuses.all });
      await options?.onSuccess?.(data, variables);
    },
  });
}
