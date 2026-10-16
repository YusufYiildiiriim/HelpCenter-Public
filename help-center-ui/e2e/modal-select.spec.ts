import { expect, test, type Locator, type Page } from "@playwright/test";

async function fixture(page: Page) {
  const authOrder: string[] = [];
  await page.context().addCookies([{ name: "refresh_token", value: "fixture", url: "http://127.0.0.1:3001" }]);
  await page.route("**/api/**", async route => {
    const path = new URL(route.request().url()).pathname;
    let body: unknown = [];
    if (path === "/api/auth/refresh") {
      authOrder.push("refresh");
      body = { accessToken: "fixture-access" };
    } else if (path === "/api/auth/verify") {
      authOrder.push("verify");
      expect(route.request().headers().authorization).toBe("Bearer fixture-access");
      body = { authenticated: true, role: "Admin", username: "fixture", userId: 1, isPasswordChangeRequired: false,
        permissions: { modules: ["Companies", "Guide"].map(resourceKey => ({ resourceKey, actions: ["Read", "Create", "Update"] })) } };
    } else if (path.includes("organization")) body = { organizationName: "Fixture" };
    else if (path.includes("modules/get-all")) body = [{ id: 1, name: "Billing", isActive: true }];
    else if (path.includes("projects/get-all")) body = Array.from({ length: 30 }, (_, i) => ({ id: i + 1, name: `Project ${i + 1}`, moduleIds: [1], isActive: true }));
    else if (path.includes("companies/get-all")) body = [{ id: 1, publicId: "fixture-company", name: "Fixture Co", mail: "co@example.test", phone: "02125551234", address: "Test address", branchCount: 1,
      contactPersonName: "Test", contactPersonSurname: "User", contactPersonEmail: "user@example.test", contactPersonPhone: "05325551234", moduleIds: [], projectId: null, isDemoActive: false }];
    else if (path.includes("guides/get-all")) body = [{ id: 1, publicId: "fixture-guide", title: "First step", description: "<p>Fixture guide</p>", module: "Billing", isActive: true, isPublic: true, documents: [], createdAt: "2026-10-16T00:00:00Z" }];
    await route.fulfill({ json: body });
  });
  return authOrder;
}

async function positioned(page: Page, trigger: Locator) {
  await trigger.click();
  const menu = page.getByRole("menu");
  await expect(menu).toBeVisible();
  const anchor = (await trigger.boundingBox())!;
  const box = (await menu.boundingBox())!;
  const viewport = page.viewportSize()!;
  expect(box.x).toBeGreaterThanOrEqual(0);
  expect(box.y).toBeGreaterThanOrEqual(0);
  expect(box.x + box.width).toBeLessThanOrEqual(viewport.width);
  expect(box.y + box.height).toBeLessThanOrEqual(viewport.height);
  expect(Math.abs(box.x - anchor.x)).toBeLessThan(10);
  expect(Math.min(Math.abs(box.y - anchor.y - anchor.height), Math.abs(box.y + box.height - anchor.y))).toBeLessThan(10);
  return menu;
}

for (const width of [320, 390, 1440]) {
  test(`company modal selection stays anchored at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 844 });
    const order = await fixture(page);
    await page.goto("/admin/companies");
    await page.getByRole("button", { name: "Düzenle", exact: true }).click();
    await page.getByRole("button", { name: "Sistem & Demo" }).click();
    const trigger = page.getByRole("button", { name: "Proje", exact: true });
    await trigger.scrollIntoViewIfNeeded();
    const menu = await positioned(page, trigger);
    await menu.getByRole("menuitemradio", { name: "Project 30", exact: true }).click();
    await expect(trigger).toHaveText("Project 30");
    const modules = page.getByRole("button", { name: "Modül ekle", exact: true });
    await positioned(page, modules);
    await page.getByRole("menuitemradio", { name: "Billing", exact: true }).click();
    await expect(modules).toBeDisabled();
    await trigger.click();
    await page.keyboard.press("Escape");
    await expect(page.getByRole("menu")).toHaveCount(0);
    await expect(trigger).toBeFocused();
    expect(order).toEqual(["refresh", "verify"]);
  });

  test(`guide modal selection stays anchored at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 844 });
    await fixture(page);
    await page.goto("/admin/guide");
    await page.getByRole("button", { name: "Yeni Rehber Ekle" }).click();
    const moduleTrigger = page.getByRole("button", { name: "Kategori (Modül)", exact: true });
    await positioned(page, moduleTrigger);
    await page.getByRole("menuitemradio", { name: "Billing", exact: true }).click();
    await expect(moduleTrigger).toHaveText("Billing");
    const previous = page.getByRole("button", { name: "Önceki Adım (Zincirleme)", exact: true });
    await positioned(page, previous);
    await page.getByRole("menuitemradio", { name: "First step", exact: true }).click();
    await expect(previous).toHaveText("First step");
    await moduleTrigger.focus();
    await page.keyboard.press("ArrowDown");
    await expect(page.getByRole("menu")).toBeVisible();
    await page.keyboard.press("Escape");
    await expect(moduleTrigger).toBeFocused();
  });
}
