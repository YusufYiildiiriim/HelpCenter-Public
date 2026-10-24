import { describe, expect, it } from "vitest";
import { getRequestStatusLabel, isCompletedRequest, RequestStatus } from "@/lib/requestStatus";

describe("request status presentation", () => {
  it("uses the canonical technical-review label from the status id", () => {
    expect(getRequestStatusLabel(RequestStatus.UnderTechnicalReview, "Cevaplandı")).toBe("Teknik İncelemede");
  });

  it("does not mark technical review as completed", () => {
    expect(isCompletedRequest(RequestStatus.UnderTechnicalReview, "Teknik İncelemede")).toBe(false);
    expect(isCompletedRequest(RequestStatus.Completed, "başka bir metin")).toBe(true);
  });
});
