import { describe, expect, it } from "vitest";
import { isPublicId } from "@/lib/publicId";

describe("isPublicId", () => {
  it("accepts GUID route identifiers and rejects malformed paths", () => {
    expect(isPublicId("60000000-0000-4000-8000-000000000001")).toBe(true);
    expect(isPublicId("not-a-request")).toBe(false);
    expect(isPublicId("")).toBe(false);
    expect(isPublicId(undefined)).toBe(false);
  });
});
