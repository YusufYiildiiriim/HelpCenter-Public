import { afterEach, beforeEach, describe, expect, it } from "vitest";
import MockAdapter from "axios-mock-adapter";
import api from "@/lib/axios";
import { AdminUserService, type User } from "@/services/admin/AdminUserService";

describe("admin user service", () => {
  let mock: MockAdapter;

  beforeEach(() => {
    mock = new MockAdapter(api);
  });

  afterEach(() => {
    mock.restore();
  });

  it("sends the concurrency token and current user fields when updating roles", async () => {
    const user: User = {
      id: "10000000-0000-4000-8000-000000000001",
      rowVersion: "20000000-0000-4000-8000-000000000002",
      email: "agent@example.test",
      name: "Ada",
      lastName: "Lovelace",
      username: "ada",
      roles: [],
      userRoles: [],
      isActive: true,
      createdAt: "2026-10-30T00:00:00Z",
    };
    const roleIds = ["30000000-0000-4000-8000-000000000003"];
    mock.onPut(`/api/admin/users/${user.id}`).reply(200, true);

    await expect(AdminUserService.updateRoles(user, roleIds)).resolves.toBe(true);

    expect(JSON.parse(mock.history.put[0].data)).toEqual({
      id: user.id,
      rowVersion: user.rowVersion,
      email: user.email,
      name: user.name,
      lastName: user.lastName,
      username: user.username,
      password: "",
      roleIds,
      isActive: user.isActive,
    });
  });
});
