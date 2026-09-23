import type { VerifyResponse, ResourcePermission } from "@/services/common/AuthService";

export type PermissionAction =
  | "canRead"
  | "canCreate"
  | "canUpdate"
  | "canDelete"
  | "canExport"
  | "canPrint"
  | "canApprove"
  | "canReject"
  | "canAssign"
  | "canChangeStatus"
  | "ManageMembers"
  | "ManageExperts"
  | "ManageModules"
  | "ManageProjects"
  | string;

const ACTION_MAP: Record<string, string> = {
  canRead: "Read",
  canCreate: "Create",
  canUpdate: "Update",
  canDelete: "Delete",
  canExport: "Export",
  canPrint: "Print",
  canApprove: "Approve",
  canReject: "Reject",
  canAssign: "Assign",
  canChangeStatus: "ChangeStatus",
};

export const hasPermission = (
  permissions: { modules: ResourcePermission[] } | undefined,
  resourceKey: string,
  action: string
): boolean => {
  if (!permissions?.modules) return false;

  const perm = permissions.modules.find(m => m.resourceKey === resourceKey);
  if (!perm) return false;

  const actionName = ACTION_MAP[action] || action;
  return Array.isArray(perm.actions) ? perm.actions.includes(actionName) : false;
};

export const usePermission = (userInfo: VerifyResponse | null) => {
  return {
    canRead: (key: string) => hasPermission(userInfo?.permissions, key, "canRead"),
    canCreate: (key: string) => hasPermission(userInfo?.permissions, key, "canCreate"),
    canUpdate: (key: string) => hasPermission(userInfo?.permissions, key, "canUpdate"),
    canDelete: (key: string) => hasPermission(userInfo?.permissions, key, "canDelete"),
    canExport: (key: string) => hasPermission(userInfo?.permissions, key, "canExport"),
    canPrint: (key: string) => hasPermission(userInfo?.permissions, key, "canPrint"),
    canApprove: (key: string) => hasPermission(userInfo?.permissions, key, "canApprove"),
    canReject: (key: string) => hasPermission(userInfo?.permissions, key, "canReject"),
    canAssign: (key: string) => hasPermission(userInfo?.permissions, key, "canAssign"),
    canChangeStatus: (key: string) => hasPermission(userInfo?.permissions, key, "canChangeStatus"),
    canManageMembers: (key: string) => hasPermission(userInfo?.permissions, key, "ManageMembers"),
    canManageExperts: (key: string) => hasPermission(userInfo?.permissions, key, "ManageExperts"),
    canManageModules: (key: string) => hasPermission(userInfo?.permissions, key, "ManageModules"),
    canManageProjects: (key: string) => hasPermission(userInfo?.permissions, key, "ManageProjects"),
    hasAction: (key: string, action: string) => hasPermission(userInfo?.permissions, key, action),
  };
};
