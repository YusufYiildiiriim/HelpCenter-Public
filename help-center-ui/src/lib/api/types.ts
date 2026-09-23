/**
 * camelCase (default ASP.NET Core JSON) counterparts of the backend
 * HelpCenter.Application.Common.Models.ApiResponse<T> / PaginatedResponse<T> wrappers.
 */

export interface ApiError {
  code: string;
  message: string;
  details?: string[] | null;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string | null;
  data?: T | null;
  error?: ApiError | null;
  correlationId?: string | null;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
