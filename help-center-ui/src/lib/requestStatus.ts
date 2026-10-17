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