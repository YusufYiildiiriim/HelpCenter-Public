import { expect, test } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";
import { fixture } from "./qa-fixtures";

for (const route of ["companies", "projects", "users", "ss", "guide", "statuses", "subjects", "modules", "requests", "requests/assigned", "settings/roles", "settings/organization"]) {
  test(`320px admin ${route} renders without horizontal overflow or non-contrast a11y violations`, async ({ page }, info) => {
    await page.setViewportSize({ width: 320, height: 844 });
    await fixture(page);
    await page.goto(`/admin/${route}`);
    await expect(page.locator("main")).toBeVisible();
    await expect(page.getByText("Sistem Yetkilendiriliyor...")).toHaveCount(0);
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
    const scan = await new AxeBuilder({ page }).withTags(["wcag2a", "wcag2aa"]).disableRules(["color-contrast"]).analyze();
    await info.attach("accessibility", { body: JSON.stringify(scan.violations, null, 2), contentType: "application/json" });
    expect(scan.violations).toEqual([]);
  });
}
