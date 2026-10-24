import { describe, expect, it, vi } from "vitest";
import { LOGOUT_EVENT_KEY, subscribeToLogout } from "@/lib/authSession";

describe("cross-tab logout", () => {
  it("notifies subscribers when another tab writes the logout key", () => {
    const handler = vi.fn();
    const unsubscribe = subscribeToLogout(handler);
    window.dispatchEvent(new StorageEvent("storage", { key: LOGOUT_EVENT_KEY, newValue: "123" }));
    expect(handler).toHaveBeenCalledTimes(1);
    unsubscribe();
  });
});
