import type { Page } from "@playwright/test";
export async function fixture(page: Page, allowed = true) {
  await page.context().addCookies([{ name: "refresh_token", value: "fixture", url: "http://127.0.0.1:3001" }]);
  await page.route("**/api/**", async route => {
    const url = new URL(route.request().url());
    let body: unknown = [];
    if (url.pathname.endsWith("/refresh")) body = { accessToken: "fixture-access" };
    else if (url.pathname.endsWith("/verify")) body = { authenticated: true, role: "Admin", username: "qa", userId: 1,
      isPasswordChangeRequired: false, permissions: { modules: allowed ? ["Statuses", "AuditLogs"].map(resourceKey => ({ resourceKey, actions: ["Read", "Export", "Print"] })) : [] } };
    else if (url.pathname.includes("organization")) body = { organizationName: "HelpCenter" };
    else if (url.pathname.includes("statuses/get-all")) {
      const all = Array.from({ length: 80 }, (_, i) => ({ id: i + 1, name: `Türkçe Durum ${i + 1}`, description: `İşlem açıklaması: ğüşöçİ ${i + 1}`, isActive: true }));
      const size = Number(url.searchParams.get("pageSize") ?? 10);
      body = { items: size === 0 ? all : all.slice(0, size), totalCount: 80, totalPages: size === 0 ? 1 : Math.ceil(80 / size), currentPage: 1, pageSize: size };
    } else if (url.pathname.endsWith("/audit-logs")) body = { items: [{ id: 1, eventType: "RoleCreated", actorUserId: 3, targetType: "Role", targetId: 2, occurredAt: "2026-10-16T12:00:00Z", correlationId: "qa-correlation" }], totalCount: 1, totalPages: 1 };
    await route.fulfill({ json: body });
  });
}

