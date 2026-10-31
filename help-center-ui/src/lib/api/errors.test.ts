import axios, { AxiosError, type AxiosResponse } from "axios";
import { afterEach, describe, expect, it } from "vitest";
import { getApiErrorMessage } from "./errors";

function apiError(code: string, status: number): AxiosError {
  const error = new AxiosError("request failed");
  error.response = {
    data: { success: false, error: { code, message: "server text" } },
    status,
    statusText: "Error",
    headers: {},
    config: { headers: new axios.AxiosHeaders() },
  } as AxiosResponse;
  return error;
}

describe("localized API errors", () => {
  afterEach(() => { document.cookie = "hc_locale=; Path=/; Max-Age=0"; });

  it("uses the selected locale for known machine error codes", () => {
    document.cookie = "hc_locale=en-US";
    expect(getApiErrorMessage(apiError("InvalidCredentials", 401))).toBe("Incorrect username, email, or password.");
    document.cookie = "hc_locale=en-US";
    expect(getApiErrorMessage(apiError("RequestSubjectLocked", 409))).toBe("This request subject is locked and cannot be changed.");

    document.cookie = "hc_locale=tr-TR";
    expect(getApiErrorMessage(apiError("InvalidCredentials", 401))).toBe("Hatalı kullanıcı adı/e-posta veya şifre.");
    expect(getApiErrorMessage(apiError("RequestSubjectLocked", 409))).toBe("Bu talep konusu kilitli olduğu için değiştirilemiyor.");
  });

  it("does not expose the server-provided error text", () => {
    document.cookie = "hc_locale=en-US";
    expect(getApiErrorMessage(apiError("UnknownBusinessError", 400))).toBe("The submitted information is invalid. Check the fields and try again.");
  });
});
