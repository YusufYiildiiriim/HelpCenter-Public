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
const TIMEOUT_ERROR_MESSAGE =
  "Sunucu yanıtı zaman aşımına uğradı. Lütfen tekrar deneyin.";
const SERVER_ERROR_MESSAGE =
  "Sunucu tarafında geçici bir sorun oluştu. Lütfen tekrar deneyin.";
const GENERIC_FALLBACK_MESSAGE = "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.";

const STATUS_MESSAGES: Record<number, string> = {
  400: "Gönderilen bilgiler geçersiz. Lütfen alanları kontrol edin.",
  401: "Oturumunuz geçersiz veya sona ermiş. Lütfen tekrar giriş yapın.",
  403: "Bu işlem için yetkiniz yok.",
  404: "İstenen kayıt bulunamadı.",
  409: "Bu işlem kaydın mevcut durumu nedeniyle tamamlanamadı.",
  422: "Gönderilen bilgiler işlenemedi. Lütfen alanları kontrol edin.",
  429: "Çok fazla istek gönderildi. Lütfen kısa süre sonra tekrar deneyin.",
  500: SERVER_ERROR_MESSAGE,
  502: SERVER_ERROR_MESSAGE,
  503: SERVER_ERROR_MESSAGE,
  504: SERVER_ERROR_MESSAGE,
};

const STATUS_MESSAGES_EN: Record<number, string> = {
  400: "The submitted information is invalid. Check the fields and try again.",
  401: "Your session is invalid or has expired. Sign in again.",
  403: "You do not have permission to perform this action.",
  404: "The requested record was not found.",
  409: "This action could not be completed because the record has changed.",
  422: "The submitted information could not be processed.",
  429: "Too many requests. Try again shortly.",
  500: "A temporary server problem occurred. Try again.", 502: "A temporary server problem occurred. Try again.",
  503: "A temporary server problem occurred. Try again.", 504: "A temporary server problem occurred. Try again.",
};

function isEnglishLocale(): boolean {
  return typeof document !== "undefined" && document.cookie.split("; ").some((value) => value === "hc_locale=en-US");
}

const ENGLISH_NETWORK_ERROR = "Could not connect to the server. Check your connection and try again.";
const ENGLISH_TIMEOUT_ERROR = "The server response timed out. Try again.";
const ENGLISH_GENERIC_ERROR = "Something went wrong. Try again.";

const CODE_MESSAGES: Record<string, { tr: string; en: string }> = {
  AccountInactive: { tr: "Hesabınız etkin değil.", en: "Your account is inactive." },
  CustomerInactive: { tr: "Müşteri hesabınız etkin değil. Firma yöneticinizle iletişime geçin.", en: "Your customer account is inactive. Contact your company administrator." },
  UserInactive: { tr: "Kullanıcı hesabınız etkin değil.", en: "Your user account is inactive." },
  InvalidCredentials: { tr: "Hatalı kullanıcı adı, e-posta veya şifre.", en: "Incorrect username, email, or password." },
  InvalidIdentifier: { tr: "E-posta adresi veya kullanıcı adı girin.", en: "Enter an email address or username." },
  InvalidResetCode: { tr: "Doğrulama kodu hatalı. Kontrol edip tekrar deneyin.", en: "The verification code is incorrect. Check it and try again." },
  ResetCodeExpired: { tr: "Doğrulama kodunun süresi doldu. Yeni bir kod isteyin.", en: "The verification code expired. Request a new one." },
  EmailAlreadyExists: { tr: "Bu e-posta adresi zaten kullanımda.", en: "This email address is already in use." },
  UsernameAlreadyExists: { tr: "Bu kullanıcı adı zaten kullanımda.", en: "This username is already in use." },
  CompanyNotFound: { tr: "Firma bulunamadı.", en: "Company not found." },
  CompanyNameAlreadyExists: { tr: "Bu firma adı zaten kullanımda.", en: "This company name is already in use." },
  FaqNotFound: { tr: "SSS kaydı bulunamadı.", en: "FAQ entry not found." },
  FaqTitleAlreadyExists: { tr: "Bu başlıkta bir SSS kaydı zaten var.", en: "An FAQ entry with this title already exists." },
  GuideNotFound: { tr: "Kılavuz bulunamadı.", en: "Guide not found." },
  GuideTitleAlreadyExists: { tr: "Bu başlıkta bir kılavuz zaten var.", en: "A guide with this title already exists." },
  ModuleNotFound: { tr: "Modül bulunamadı.", en: "Module not found." },
  ModuleLocked: { tr: "Bu modül kilitli olduğu için değiştirilemiyor.", en: "This module is locked and cannot be changed." },
  ModuleNameAlreadyExists: { tr: "Bu modül adı zaten kullanımda.", en: "This module name is already in use." },
  ModuleNotAllowedForCompany: { tr: "Bu modül firmanız için etkin değil.", en: "This module is not enabled for your company." },
  ProjectNotFound: { tr: "Proje bulunamadı.", en: "Project not found." },
  ProjectNameAlreadyExists: { tr: "Bu proje adı zaten kullanımda.", en: "This project name is already in use." },
  RequestSubjectNotFound: { tr: "Talep konusu bulunamadı.", en: "Request subject not found." },
  RequestSubjectLocked: { tr: "Bu talep konusu kilitli olduğu için değiştirilemiyor.", en: "This request subject is locked and cannot be changed." },
  RequestSubjectNameAlreadyExists: { tr: "Bu talep konusu adı zaten kullanımda.", en: "This request subject name is already in use." },
  RequestNotFound: { tr: "Talep bulunamadı.", en: "Request not found." },
  RequestCompleted: { tr: "Tamamlanan talep artık değiştirilemiyor.", en: "A completed request can no longer be changed." },
  RequestNotBelongToCustomer: { tr: "Bu talep hesabınıza ait değil.", en: "This request does not belong to your account." },
  DashboardAccessForbidden: { tr: "Bu firma paneline erişim yetkiniz yok.", en: "You do not have access to this company dashboard." },
  ReportsAccessForbidden: { tr: "Raporlara erişim yetkiniz yok.", en: "You do not have permission to access reports." },
  ConcurrencyConflict: { tr: "Kayıt başka biri tarafından güncellendi. Yenileyip tekrar deneyin.", en: "Another user updated this record. Refresh and try again." },
  ValidationError: { tr: "Gönderilen bilgileri kontrol edin.", en: "Check the submitted information." },
  UnsupportedLocale: { tr: "Desteklenmeyen dil seçimi.", en: "The selected language is not supported." },
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

/** Retry-After accepts delay seconds or an HTTP date. Missing/invalid values wait one minute. */
export function getApiRetryAfterSeconds(error: unknown, now = Date.now()): number {
  if (!axios.isAxiosError(error)) return 60;
  const value: unknown = error.response?.headers?.["retry-after"];
  if (typeof value !== "string" && typeof value !== "number") return 60;
  const raw = String(value).trim();
  if (!raw) return 60;
  if (/^\d+$/.test(raw)) {
    const seconds = Number(raw);
    return Number.isFinite(seconds) ? seconds : 60;
  }
  const deadline = Date.parse(raw);
  return Number.isFinite(deadline) ? Math.max(0, Math.ceil((deadline - now) / 1000)) : 60;
}

export function isNetworkError(error: unknown): boolean {
  if (!axios.isAxiosError(error)) return false;
  // In cases like timeout / DNS / CORS / connection refused, axios does not produce a response.
  return !error.response;
}

export function isTimeoutError(error: unknown): boolean {
  if (!axios.isAxiosError(error)) return false;
  return error.code === "ECONNABORTED" || error.code === "ETIMEDOUT" ||
    (typeof error.message === "string" && error.message.toLowerCase().includes("timeout"));
}

export function getApiErrorMessage(error: unknown): string {
  const english = isEnglishLocale();
  if (isTimeoutError(error)) return english ? ENGLISH_TIMEOUT_ERROR : TIMEOUT_ERROR_MESSAGE;
  if (isNetworkError(error)) return english ? ENGLISH_NETWORK_ERROR : NETWORK_ERROR_MESSAGE;

  const status = getApiErrorStatus(error);
  const code = getApiErrorCode(error);
  if (code && CODE_MESSAGES[code]) return english ? CODE_MESSAGES[code].en : CODE_MESSAGES[code].tr;
  if (status) {
    const message = (english ? STATUS_MESSAGES_EN : STATUS_MESSAGES)[status];
    if (message) return message;
  }

  // Never render server-provided messages or Error.message. They can contain exception,
  // proxy, database, or upstream-service details. Keep the correlation id available for support.
  return english ? ENGLISH_GENERIC_ERROR : GENERIC_FALLBACK_MESSAGE;
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

export function isNotFoundError(error: unknown): boolean {
  return getApiErrorStatus(error) === 404;
}

export function isValidationError(error: unknown): boolean {
  if (getApiErrorStatus(error) !== 400) return false;
  const details = getApiErrorDetails(error);
  return details !== null && details.length > 0;
}
