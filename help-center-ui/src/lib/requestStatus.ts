/** Stable wire IDs matching the immutable system status seeds. Never reorder or recycle. */
export const RequestStatus = {
  WaitingForReply: "10000000-0000-4000-8000-000000000001",
  Replied: "10000000-0000-4000-8000-000000000002",
  Completed: "10000000-0000-4000-8000-000000000003",
  UnderTechnicalReview: "10000000-0000-4000-8000-000000000004",
} as const;

export const requestStatusLabels: Record<string, string> = {
  [RequestStatus.WaitingForReply]: "Cevap Bekliyor",
  [RequestStatus.Replied]: "Cevaplandı",
  [RequestStatus.Completed]: "Tamamlandı",
  [RequestStatus.UnderTechnicalReview]: "Teknik İncelemede",
};

const requestStatusLabelsEn: Record<string, string> = {
  [RequestStatus.WaitingForReply]: "Waiting for reply",
  [RequestStatus.Replied]: "Replied",
  [RequestStatus.Completed]: "Completed",
  [RequestStatus.UnderTechnicalReview]: "Under technical review",
};

export function getRequestStatusLabel(statusId?: string, fallback?: string, locale: "tr-TR" | "en-US" = "tr-TR"): string {
  if (statusId && locale === "en-US" && requestStatusLabelsEn[statusId]) return requestStatusLabelsEn[statusId];
  if (statusId && requestStatusLabels[statusId]) return requestStatusLabels[statusId];
  if (locale === "en-US") {
    const fallbackTranslations: Record<string, string> = {
      "Cevap Bekliyor": "Waiting for reply",
      "Cevaplandı": "Replied",
      "Tamamlandı": "Completed",
      "Teknik İncelemede": "Under technical review",
      "Durum bilinmiyor": "Unknown status",
    };
    return fallbackTranslations[fallback ?? ""] ?? fallback ?? "Unknown status";
  }
  return fallback || "Durum bilinmiyor";
}

export function isCompletedRequest(statusId?: string, status?: string): boolean {
  return statusId === RequestStatus.Completed || status === requestStatusLabels[RequestStatus.Completed];
}
