/** Stable wire IDs matching the immutable system status seeds. Never reorder or recycle. */
export const RequestStatus = {
  WaitingForReply: 1,
  Replied: 2,
  Completed: 3,
  UnderTechnicalReview: 4,
} as const;

export const requestStatusLabels: Record<number, string> = {
  [RequestStatus.WaitingForReply]: "Cevap Bekliyor",
  [RequestStatus.Replied]: "Cevaplandı",
  [RequestStatus.Completed]: "Tamamlandı",
  [RequestStatus.UnderTechnicalReview]: "Teknik İncelemede",
};