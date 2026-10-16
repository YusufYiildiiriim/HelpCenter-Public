import { expect, test } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";
import { fixture } from "./qa-fixtures";
test("320px admin audit page exposes filters and has no new non-contrast a11y violations", async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 844 });
  await fixture(page);
  await page.goto("/admin/settings/audit-logs");
  await expect(page.getByText("RoleCreated").first()).toBeVisible();
  await page.getByLabel("Olay türü").fill("RoleCreated");
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  const scan = await new AxeBuilder({ page }).withTags(["wcag2a", "wcag2aa"]).disableRules(["color-contrast"]).analyze();
  expect(scan.violations).toEqual([]);
});

test("audit page without permission shows forbidden and makes no audit request", async ({ page }) => {
  let auditReads = 0;
  page.on("request", request => { if (new URL(request.url()).pathname === "/api/admin/audit-logs") auditReads++; });
  await fixture(page, false);
  await page.goto("/admin/settings/audit-logs");
  await expect(page.getByRole("alert").filter({ hasText: "Erişim Yetkiniz Yok" })).toBeVisible();
  expect(auditReads).toBe(0);
});
