import { render, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { clearAccessToken, getAccessToken } from "@/lib/authSession";

const navigation = vi.hoisted(() => ({
  replace: vi.fn(),
  searchParams: new URLSearchParams(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: navigation.replace }),
  useSearchParams: () => navigation.searchParams,
}));

import SSOPage from "@/app/dashboard/sso/page";

describe("dashboard demo SSO bridge", () => {
  const originalLocation = window.location;

  beforeEach(() => {
    clearAccessToken();
    navigation.replace.mockReset();
    navigation.searchParams = new URLSearchParams();
    vi.stubEnv("NEXT_PUBLIC_DEMO_SSO_ENABLED", "true");
    Object.defineProperty(window, "location", {
      configurable: true,
      value: { assign: vi.fn() },
    });
  });

  afterEach(() => {
    vi.unstubAllEnvs();
    Object.defineProperty(window, "location", { configurable: true, value: originalLocation });
  });

  it("starts a valid demo session in memory and removes the token from the address bar", async () => {
    const token = "header.payload.signature";
    navigation.searchParams = new URLSearchParams({
      token,
      companyId: "50000000-0000-4000-8000-000000000019",
      userId: "20000000-0000-4000-8000-000000000042",
    });
    const replaceState = vi.spyOn(window.history, "replaceState");

    render(<SSOPage />);

    await waitFor(() => {
      expect(getAccessToken()).toBe(token);
      expect(navigation.replace).toHaveBeenCalledWith("/dashboard/50000000-0000-4000-8000-000000000019");
    });
    expect(replaceState).toHaveBeenCalledWith(null, "", "/dashboard/sso");
    expect(sessionStorage.getItem("auth_token")).toBeNull();
    expect(localStorage.getItem("auth_token")).toBeNull();
  });

  it("returns missing demo parameters to dashboard login without setting a token", async () => {
    render(<SSOPage />);

    await waitFor(() => {
      expect(window.location.assign).toHaveBeenCalledWith("/dashboard/login");
    });
    expect(getAccessToken()).toBeNull();
  });

  it("returns to dashboard login when the demo flag is disabled", async () => {
    vi.stubEnv("NEXT_PUBLIC_DEMO_SSO_ENABLED", "false");
    navigation.searchParams = new URLSearchParams({
      token: "header.payload.signature",
      companyId: "50000000-0000-4000-8000-000000000019",
      userId: "20000000-0000-4000-8000-000000000042",
    });

    render(<SSOPage />);

    await waitFor(() => {
      expect(window.location.assign).toHaveBeenCalledWith("/dashboard/login");
    });
    expect(getAccessToken()).toBeNull();
  });
});
