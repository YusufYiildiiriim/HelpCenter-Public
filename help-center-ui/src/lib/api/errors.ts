import axios from "axios";
import type { ApiError, ApiResponse } from "./types";

/**
 * Pure classification helpers. They do not duplicate or conflict with the 401-redirect
 * interceptor in `src/lib/axios.ts` — it performs global side-effects
 * (token clearing, redirect), while these are read-only classifications at the component level.
 *
 * Note: When ASP.NET Core's [Authorize] middleware generates a 401/403 without passing
 * through BaseException (e.g. no/invalid token), the body may arrive empty — response.data
 * may be undefined/empty object in that scenario, so data existence is never assumed anywhere.
 */

const NETWORK_ERROR_MESSAGE =
  "Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edip tekrar deneyin.";
const GENERIC_FALLBACK_MESSAGE = "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.";

const STATUS_MESSAGES: Record<number, string> = {
  400: "Gönderilen bilgiler geçersiz. Lütfen alanları kontrol edin.",
  401: "Oturumunuz geçersiz veya sona ermiş. Lütfen tekrar giriş yapın.",
  403: "Bu işlem için yetkiniz yok.",
  404: "İstenen kayıt bulunamadı.",
  409: "Bu işlem kaydın mevcut durumu nedeniyle tamamlanamadı.",
  422: "Gönderilen bilgiler işlenemedi. Lütfen alanları kontrol edin.",
  429: "Çok fazla istek gönderildi. Lütfen kısa süre sonra tekrar deneyin.",
};

function getResponseData(error: unknown): Partial<ApiResponse<unknown>> | undefined {
  if (!axios.isAxiosError(error)) return undefined;
  const data: unknown = error.response?.data;
  if (data && typeof data === "object") {
    return data as Partial<ApiResponse<unknown>>;
  }
  return undefined;
}

function getResponseError(error: unknown): Partial<ApiError> | undefined {
  const data = getResponseData(error);
  const err = data?.error;
  if (err && typeof err === "object") return err;
  return undefined;
}

export function getApiErrorStatus(error: unknown): number | null {
  if (!axios.isAxiosError(error)) return null;
  return error.response?.status ?? null;
}

export function isNetworkError(error: unknown): boolean {
  if (!axios.isAxiosError(error)) return false;
  // In cases like timeout / DNS / CORS / connection refused, axios does not produce a response.
  return !error.response;
}

export function getApiErrorMessage(error: unknown): string {
  if (isNetworkError(error)) return NETWORK_ERROR_MESSAGE;

  const status = getApiErrorStatus(error);
  if (status && STATUS_MESSAGES[status]) return STATUS_MESSAGES[status];

  // Never render server-provided messages or Error.message. They can contain exception,
  // proxy, database, or upstream-service details. The correlation id stays available for support.
  return GENERIC_FALLBACK_MESSAGE;
}

export function getApiErrorCorrelationId(error: unknown): string | null {
  const correlationId = getResponseData(error)?.correlationId;
  return typeof correlationId === "string" && correlationId.trim().length > 0
    ? correlationId
    : null;
}

export function getApiErrorCode(error: unknown): string | null {
  const code = getResponseError(error)?.code;
  return typeof code === "string" ? code : null;
}

export function getApiErrorDetails(error: unknown): string[] | null {
  const details = getResponseError(error)?.details;
  return Array.isArray(details) ? details : null;
}

export function isUnauthorizedError(error: unknown): boolean {
  return getApiErrorStatus(error) === 401;
}

export function isForbiddenError(error: unknown): boolean {
  return getApiErrorStatus(error) === 403;
}

export function isValidationError(error: unknown): boolean {
  if (getApiErrorStatus(error) !== 400) return false;
  const details = getApiErrorDetails(error);
  return details !== null && details.length > 0;
}
