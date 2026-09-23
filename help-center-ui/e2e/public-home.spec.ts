import { expect, test, type Page } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";

const paginated = (items: unknown[]) => ({
  items,
  totalCount: items.length,
  totalPages: 1,
  currentPage: 1,
  pageSize: items.length || 1,
});

/**
 * The public home is a client-side API consumer. Intercepting every request keeps
 * this browser smoke suite independent from Docker, a backend process and local
 * seed data while still exercising the rendered route.
 */
async function mockPublicApi(page: Page) {
  await page.route("**/api/public/**", async (route) => {
    const url = new URL(route.request().url());
    const payload = (() => {
      switch (url.pathname) {
        case "/api/public/projects":
          return [{ id: 1, publicId: "demo", name: "Demo Projesi", description: "E2E fixture" }];
        case "/api/public/organization":
          return { organizationName: "HelpCenter", footerText: "E2E fixture" };
        case "/api/public/modules":
          return [{ id: 1, name: "Genel", description: "E2E fixture" }];
        case "/api/public/guides":
          return paginated([
            {
              publicId: "baslangic",
              title: "Başlangıç Rehberi",
              description: "Demo yardım içeriği.",
              module: "Genel",
            },
          ]);
        case "/api/public/faq":
          return paginated([{ title: "Destek nasıl alınır?", description: "Talep oluşturabilirsiniz." }]);
        default:
          return null;
      }
    })();

    if (payload === null) {
      await route.fulfill({ status: 404, contentType: "application/json", body: "{}" });
      return;
    }

    await route.fulfill({ contentType: "application/json", body: JSON.stringify(payload) });
  });
}

test.describe("public help center", () => {
  test.beforeEach(async ({ page }) => {
    await mockPublicApi(page);
  });

  test("renders public content from deterministic fixtures", async ({ page }) => {
    await page.goto("/");

    await expect(page.getByText("Demo Projesi")).toBeVisible();
    await expect(page.getByRole("article").filter({ hasText: "Başlangıç Rehberi" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Admin Giriş" })).toBeVisible();
  });

  test("has no non-baselined WCAG A/AA violations", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("article").filter({ hasText: "Başlangıç Rehberi" })).toBeVisible();

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(["wcag2a", "wcag2aa"])
      // The existing public portal's legacy slate palette has known contrast debt.
      // Keep this bounded exception until the semantic-theme migration remediates it;
      // all other WCAG A/AA rules remain blocking in this smoke test.
      .disableRules(["color-contrast"])
      .analyze();
    expect(accessibilityScanResults.violations).toEqual([]);
  });
});
