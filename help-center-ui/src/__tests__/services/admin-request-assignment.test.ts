import { afterEach, beforeEach, describe, expect, it } from "vitest";
import MockAdapter from "axios-mock-adapter";
import api from "@/lib/axios";
import { AdminRequestService } from "@/services/admin/AdminRequestService";

describe("assigned request service", () => {
  let mock: MockAdapter;

  beforeEach(() => {
    mock = new MockAdapter(api);
  });

  afterEach(() => {
    mock.restore();
  });

  it.each([false, true])("uses the matching mark-read route (assigned=%s)", async assigned => {
    const path = assigned ? "assigned/60000000-0000-4000-8000-000000000001" : "60000000-0000-4000-8000-000000000001";
    mock.onPost(`/api/admin/requests/${path}/mark-read`).reply(200, true);
    await expect(AdminRequestService.markAsRead("60000000-0000-4000-8000-000000000001", assigned)).resolves.toBeUndefined();
    expect(mock.history.post).toHaveLength(1);
  });

  it("propagates mark-read rejection to the caller", async () => {
    mock.onPost("/api/admin/requests/60000000-0000-4000-8000-000000000001/mark-read").reply(404, { code: "RequestNotFound" });
    await expect(AdminRequestService.markAsRead("60000000-0000-4000-8000-000000000001")).rejects.toMatchObject({ response: { status: 404 } });
  });

  it("uses the server-scoped assigned route for request detail", async () => {
    mock.onGet("/api/admin/requests/assigned/60000000-0000-4000-8000-000000000001").reply(200, { publicId: "60000000-0000-4000-8000-000000000001" });

    await expect(AdminRequestService.getAssignedById("60000000-0000-4000-8000-000000000001")).resolves.toMatchObject({ publicId: "60000000-0000-4000-8000-000000000001" });
  });

  it("does not send a user id when listing assigned requests", async () => {
    mock.onGet("/api/admin/requests/assigned?status=Cevapland%C4%B1&pageNumber=2&pageSize=10").reply(200, {
      items: [], totalCount: 0, totalPages: 1, currentPage: 2, pageSize: 10,
    });

    await expect(AdminRequestService.getAssigned({ status: "Cevaplandı", pageNumber: 2, pageSize: 10 })).resolves.toMatchObject({
      currentPage: 2,
    });
  });

  it("uses the same scoped route for assigned request messages", async () => {
    mock.onGet("/api/admin/requests/assigned/60000000-0000-4000-8000-000000000001/messages?pageSize=20").reply(200, {
      messages: [], hasMore: false, totalCount: 0,
    });

    await expect(AdminRequestService.getMessages("60000000-0000-4000-8000-000000000001", 20, undefined, true)).resolves.toMatchObject({
      totalCount: 0,
    });
  });
});
