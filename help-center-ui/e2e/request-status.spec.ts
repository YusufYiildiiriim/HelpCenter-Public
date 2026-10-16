import { expect, test, type Page, type WebSocketRoute } from "@playwright/test";

const requestId = "11111111-1111-4111-8111-111111111111";
const companyId = "22222222-2222-4222-8222-222222222222";

// HTTP and SignalR protocol fixtures exercise two independent browser sessions.
// Handler persistence and production hub group dispatch are tested separately in .NET.
function scenario() {
  const peers = new Set<WebSocketRoute>();
  const joined = new Set<string>();
  let completed = false;
  let detailsRead = 0;
  const ticket = () => ({ id: 1, publicId: requestId, ticketId: "TKT-STATUS", title: "Live status test",
    statusId: completed ? 3 : 1, status: completed ? "Tamamlandı" : "Cevap Bekliyor",
    priority: 1, priorityName: "Medium", createdAt: "2026-10-16T00:00:00Z", moduleName: "Billing",
    companyName: "Fixture Co", customerName: "Fixture Customer", moduleId: 1,
    messageCount: 0, documentCount: 0, description: "Test" });
  const emitClosed = (id = requestId) => {
    for (const peer of peers) peer.send(JSON.stringify({ type: 1, target: "RequestStatusChanged",
      arguments: [{ requestPublicId: id, statusId: 3, status: "Tamamlandı" }] }) + "\u001e");
  };
  async function setup(page: Page, customer: boolean, expert = false) {
    await page.context().addCookies([{ name: "refresh_token", value: "fixture", url: "http://127.0.0.1:3001" }]);
    await page.routeWebSocket(/\/requestHub(?:\?|$)/, peer => {
      peers.add(peer);
      peer.onMessage(raw => {
        for (const frame of raw.toString().split("\u001e").filter(Boolean)) {
          const message = JSON.parse(frame);
          if (message.protocol) peer.send("{}\u001e");
          else if (message.type === 1) {
            if (message.target === "JoinRequestGroup") joined.add(customer ? "customer" : "admin");
            if (message.invocationId) peer.send(JSON.stringify({ type: 3, invocationId: message.invocationId, result: null }) + "\u001e");
          }
        }
      });
      peer.onClose(() => peers.delete(peer));
    });
    await page.route("**/requestHub/negotiate**", route => route.fulfill({ json: {
      negotiateVersion: 1, connectionId: "fixture", connectionToken: "fixture",
      availableTransports: [{ transport: "WebSockets", transferFormats: ["Text"] }]
    } }));
    await page.route("**/api/**", async route => {
      const path = new URL(route.request().url()).pathname;
      let body: unknown = [];
      if (path === "/api/auth/refresh") body = { accessToken: "fixture-token" };
      else if (path === "/api/auth/verify") body = { authenticated: true, role: customer ? "Customer" : expert ? "Expert" : "Admin", userId: customer ? 7 : 1,
        username: "fixture", companyId, isPasswordChangeRequired: false,
        permissions: { modules: [{ resourceKey: expert ? "AssignedRequests" : "Requests", actions: ["Read", "Update"] }] } };
      else if (path.includes("organization")) body = { organizationName: "Fixture" };
      else if (path.includes("get-by-id") || path === `/api/admin/requests/assigned/${requestId}`) { detailsRead++; body = ticket(); }
      else if (path.endsWith("/messages")) body = customer ? [] : { messages: [], hasMore: false, totalCount: 0 };
      else if (path.endsWith("/mark-read")) body = true;
      else if (path === "/api/customer/requests/close") {
        completed = true;
        await route.fulfill({ json: true });
        emitClosed();
        return;
      } else if (path === "/api/admin/requests/send-message" && completed) {
        await route.fulfill({ status: 409, json: { success: false, error: { code: "RequestCompleted", message: "Untrusted server text" } } });
        return;
      }
      await route.fulfill({ json: body });
    });
  }
  return { setup, joined, emitClosed, detailsRead: () => detailsRead, closeSilently: () => { completed = true; } };
}

test("customer close updates the open admin session without reload or a fake message", async ({ browser }) => {
  const state = scenario();
  const adminContext = await browser.newContext();
  const customerContext = await browser.newContext();
  try {
    const admin = await adminContext.newPage();
    const customer = await customerContext.newPage();
    await state.setup(admin, false);
    await state.setup(customer, true);
    await admin.goto(`http://127.0.0.1:3001/admin/requests/${requestId}`);
    await expect(admin.getByPlaceholder("Bir yanıt yazın...")).toBeVisible();
    await customer.goto(`http://127.0.0.1:3001/dashboard/${companyId}/requests/${requestId}`);
    await expect(customer.getByRole("button", { name: "Talebi Kapat", exact: true })).toBeVisible();
    await expect.poll(() => state.joined.size).toBe(2);
    const initialReads = state.detailsRead();
    state.emitClosed("another-request");
    await expect(admin.getByPlaceholder("Bir yanıt yazın...")).toBeVisible();
    await customer.getByRole("button", { name: "Talebi Kapat", exact: true }).click();
    await customer.getByRole("button", { name: "Kaydet ve Kapat", exact: true }).click();
    await expect(admin.getByText("Bu talep sonuçlandırıldığı için yeni mesaj gönderilemez.")).toBeVisible();
    await expect(admin.getByPlaceholder("Bir yanıt yazın...")).toHaveCount(0);
    await expect(admin.getByRole("button", { name: "Danış", exact: true })).toHaveCount(0);
    await expect(admin.getByText("Henüz mesaj bulunmuyor.")).toBeVisible();
    expect(state.detailsRead()).toBe(initialReads);
  } finally {
    await adminContext.close();
    await customerContext.close();
  }
});

test("a missed close event is recovered from the send rejection with a clear local message", async ({ page }) => {
  const state = scenario();
  await state.setup(page, false);
  await page.goto(`http://127.0.0.1:3001/admin/requests/${requestId}`);
  await page.getByPlaceholder("Bir yanıt yazın...").fill("Draft reply");
  state.closeSilently();
  await page.getByPlaceholder("Bir yanıt yazın...").press("Enter");
  await expect(page.getByText("Bu talep kapatılmış. Yeni mesaj gönderilemez.").first()).toBeVisible();
  await expect(page.getByText("Bu talep sonuçlandırıldığı için yeni mesaj gönderilemez.")).toBeVisible();
  await expect(page.getByText("Untrusted server text")).toHaveCount(0);
});

test("320px expert chat reads assigned endpoints without general request permission", async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 844 });
  const paths: string[] = [];
  page.on("request", request => paths.push(new URL(request.url()).pathname));
  await scenario().setup(page, false, true);
  await page.goto(`/admin/requests/${requestId}`);
  await expect(page.getByPlaceholder("Bir yanıt yazın...")).toBeVisible();
  await expect.poll(() => paths).toContain(`/api/admin/requests/assigned/${requestId}`);
  expect(paths).not.toContain(`/api/admin/requests/get-by-id/${requestId}`);
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});

test("320px customer chat exposes customer controls and stays inside the viewport", async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 844 });
  await scenario().setup(page, true);
  await page.goto(`/dashboard/${companyId}/requests/${requestId}`);
  await expect(page.getByRole("button", { name: "Talebi Kapat", exact: true })).toBeVisible();
  await expect(page.getByRole("button", { name: "Danış", exact: true })).toHaveCount(0);
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});
