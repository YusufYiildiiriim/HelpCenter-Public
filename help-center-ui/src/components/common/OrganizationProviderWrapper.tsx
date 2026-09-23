"use client";

import type { ReactNode } from "react";
import { OrganizationProvider } from "@/context/OrganizationContext";

export function OrganizationProviderWrapper({ children }: { children: ReactNode }) {
  return <OrganizationProvider>{children}</OrganizationProvider>;
}
