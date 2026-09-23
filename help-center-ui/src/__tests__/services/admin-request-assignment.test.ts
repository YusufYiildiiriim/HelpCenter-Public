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

  it("uses the server-scoped assigned route for request detail", async () => {
    mock.onGet("/api/admin/requests/assigned/request-1").reply(200, { publicId: "request-1" });

    await expect(AdminRequestService.getAssignedById("request-1")).resolves.toMatchObject({ publicId: "request-1" });
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
    mock.onGet("/api/admin/requests/assigned/request-1/messages?pageSize=20").reply(200, {
      messages: [], hasMore: false, totalCount: 0,
    });

    await expect(AdminRequestService.getMessages("request-1", 20, undefined, true)).resolves.toMatchObject({
      totalCount: 0,
    });
  });
});
