export const queryKeys = {
  admin: {
    statuses: {
      all: ["admin", "statuses"] as const,
      list: (params: StatusListQuery) => ["admin", "statuses", "list", params] as const,
    },
  },
} as const;

export interface StatusListQuery {
  pageNumber: number;
  pageSize: number;
  search: string;
}
