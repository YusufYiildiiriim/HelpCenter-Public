import { expect, test, type Page } from "@playwright/test";

// Exercise the real route, layout, permissions, service and animated cards without
// depending on a database or putting real credentials in browser test fixtures.
async function mockDashboard(page: Page, total: number | null = 3, statisticsGate?: Promise<void>) {
  let currentTotal = total;
  await page.context().addCookies([
    { name: "refresh_token", value: "e2e-fixture", url: "http://127.0.0.1:3001" },
  ]);
  await page.route("**/api/**", async (route) => {
    const path = new URL(route.request().url()).pathname;
    let body: unknown;
    switch (path) {
      case "/api/auth/verify":
        body = {
          authenticated: true, role: "Admin", username: "fixture", userId: 1,
          isPasswordChangeRequired: false,
          permissions: { modules: [{ resourceKey: "Dashboard", actions: ["Read"] }] },
        };
        break;
      case "/api/admin/menu-items": body = []; break;
      case "/api/admin/statistics":
        await statisticsGate;
        body = {
          totalRequests: currentTotal, completedRequests: 1, activeRequests: 2,
          totalMessages: 6, totalCompanies: 3, totalCustomers: 4,
          allowedFields: ["totalRequests", "completedRequests", "activeRequests", "totalMessages", "totalCompanies", "totalCustomers"],
        };
        break;
      case "/api/admin/reports": body = {}; break;
      case "/api/admin/organization":
      case "/api/public/organization": body = { organizationName: "HelpCenter fixture" }; break;
      default:
        await route.fulfill({ status: 404, json: {} });
        return;
    }
    await route.fulfill({ json: body });
  });
  return { setTotal: (next: number) => { currentTotal = next; } };
}

for (const reducedMotion of ["no-preference", "reduce"] as const) {
  test(`total request counter survives load, refresh and reload (${reducedMotion})`, async ({ page }, testInfo) => {
    await page.emulateMedia({ reducedMotion });
    await page.setViewportSize({ width: 390, height: 844 });
    const fixture = await mockDashboard(page);
    await page.goto("/admin");
    const card = page.getByText("Toplam Talep", { exact: true }).locator("..");
    const counter = card.locator("span.font-mono").first();
    await expect(counter).toHaveText("3");
    await expect(counter).toBeVisible();
    // The value is painted, not merely present in an opacity-zero ancestor.
    await expect.poll(() => counter.evaluate((el) => {
      let opacity = 1;
      for (let node: Element | null = el; node; node = node.parentElement) {
        opacity *= Number(getComputedStyle(node).opacity);
      }
      return opacity;
    })).toBe(1);
    await page.screenshot({ path: testInfo.outputPath("dashboard-loaded.png"), fullPage: true });
    fixture.setTotal(0);
    await page.getByRole("button", { name: "Yenile", exact: true }).click();
    await expect(counter).toHaveText("0");
    await expect(counter).toBeVisible();
    fixture.setTotal(1200);
    await page.reload();
    await expect(counter).toHaveText("1.200");
    await expect(counter).toBeVisible();
  });
}

test("server-hidden total is omitted rather than rendered as an empty card", async ({ page }) => {
  await mockDashboard(page, null);
  await page.goto("/admin");
  await expect(page.getByText("Aktif Talepler", { exact: true })).toBeVisible();
  await expect(page.getByText("Toplam Talep", { exact: true })).toHaveCount(0);
});

test("pending statistics show a skeleton and resolve to a visible total", async ({ page }) => {
  let release!: () => void;
  const gate = new Promise<void>((resolve) => { release = resolve; });
  await mockDashboard(page, 3, gate);
  await page.goto("/admin");
  await expect(page.getByRole("status", { name: "İstatistikler yükleniyor" })).toBeVisible();
  await expect(page.getByText("Toplam Talep", { exact: true })).toHaveCount(0);
  release();
  const counter = page.getByText("Toplam Talep", { exact: true }).locator("..").locator("span.font-mono").first();
  await expect(counter).toHaveText("3");
  await expect(counter).toBeVisible();
  await expect(page.getByRole("status", { name: "İstatistikler yükleniyor" })).toHaveCount(0);
});
