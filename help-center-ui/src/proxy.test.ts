import { NextRequest } from "next/server";
import { describe, expect, it } from "vitest";
import { proxy } from "./proxy";

describe("public locale routing", () => {
  it("falls unsupported locale prefixes back to Turkish and preserves the query string", async () => {
    const request = new NextRequest("http://localhost/fr-FR?from=qa&search=fatura", {
      headers: {
        cookie: "hc_locale=en-US",
        "accept-language": "en-US,en;q=0.9",
      },
    });

    const response = await proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe("http://localhost/tr?from=qa&search=fatura");
  });

  it("canonicalizes a supported full locale prefix to the public short path", async () => {
    const request = new NextRequest("http://localhost/en-US?search=invoice");

    const response = await proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe("http://localhost/en?search=invoice");
  });
});
