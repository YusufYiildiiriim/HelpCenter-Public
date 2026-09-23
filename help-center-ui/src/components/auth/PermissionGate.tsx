"use client";

import React from "react";
import { useAuth } from "@/context/AuthContext";
import type { PermissionAction } from "@/lib/permissions";
import { hasPermission } from "@/lib/permissions";

interface PermissionGateProps {
  resourceKey: string;
  action: PermissionAction;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function PermissionGate({
  resourceKey,
  action,
  children,
  fallback = null
}: PermissionGateProps) {
  const { permissions } = useAuth();

  if (!hasPermission(permissions, resourceKey, action)) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
