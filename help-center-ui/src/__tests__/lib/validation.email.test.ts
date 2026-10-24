import { describe, expect, it } from "vitest";
import { isValidEmailAddress } from "@/lib/validation";

describe("email validation", () => {
  it("accepts a normal address and rejects malformed input", () => {
    expect(isValidEmailAddress("user@example.com")).toBe(true);
    expect(isValidEmailAddress("user@localhost")).toBe(false);
    expect(isValidEmailAddress("user@@example.com")).toBe(false);
  });
});
