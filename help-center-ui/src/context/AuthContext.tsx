"use client";

import type { ReactNode } from "react";
import React, { createContext, useContext } from "react";
import type { VerifyResponse, ModulePermission } from "@/services/common/AuthService";

interface UserPermissions {
  modules: ModulePermission[];
}

interface AuthContextType {
  userInfo: VerifyResponse | null;
  permissions: UserPermissions | undefined;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({
  children,
  userInfo,
  permissions
}: {
  children: ReactNode;
  userInfo: VerifyResponse | null;
  permissions: UserPermissions | undefined;
}) => {
  return (
    <AuthContext.Provider value={{ userInfo, permissions }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
