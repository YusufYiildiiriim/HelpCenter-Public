"use client";

import type { ReactNode } from "react";
import React, { createContext, useContext, useState, useEffect } from "react";
import type { PublicOrganizationInfo } from "@/services/public/PublicOrganizationService";
import { PublicOrganizationService } from "@/services/public/PublicOrganizationService";
import { toApiUrl } from "@/lib/env";

interface OrganizationContextType {
  orgInfo: PublicOrganizationInfo | null;
  orgName: string;
  logoUrl: string;
  footerText: string;
  loading: boolean;
  setOrgInfo: (info: PublicOrganizationInfo) => void;
}

const defaultOrg: PublicOrganizationInfo = {
  organizationName: "Help Center",
  logoUrl: "/logo.png",
  footerText: "© 2026 Tarf Yazılım — Tüm hakları saklıdır.",
};

/**
 * Converts a logo URL received from the backend as a relative path (e.g. /Uploads/Organization/xxx.png)
 * into a full URL. Leaves local assets (/logo.png), data URLs, or http(s) URLs as they are.
 */
const resolveLogoUrl = (src: string | null | undefined): string => {
  if (!src) return "/logo.png";
  if (src.startsWith("data:") || src.startsWith("http") || src.startsWith("/logo")) return src;
  return toApiUrl(src);
};

const OrganizationContext = createContext<OrganizationContextType>({
  orgInfo: defaultOrg,
  orgName: defaultOrg.organizationName,
  logoUrl: defaultOrg.logoUrl ?? "/logo.png",
  footerText: defaultOrg.footerText ?? "",
  loading: false,
  setOrgInfo: () => {},
});

export const OrganizationProvider = ({
  children,
  initialOrgInfo,
}: {
  children: ReactNode;
  initialOrgInfo?: PublicOrganizationInfo;
}) => {
  const [orgInfo, setOrgInfo] = useState<PublicOrganizationInfo | null>(
    initialOrgInfo ?? null
  );
  const [loading, setLoading] = useState(!initialOrgInfo);

  useEffect(() => {
    if (initialOrgInfo) {
      return;
    }

    let isMounted = true;

    async function fetchOrgInfo() {
      try {
        const data = await PublicOrganizationService.getOrganizationInfo();
        if (isMounted) {
          setOrgInfo(data);
        }
      } catch (err) {
        console.error("Kurum bilgisi alınamadı:", err);
      } finally {
        if (isMounted) setLoading(false);
      }
    }

    fetchOrgInfo();
    return () => {
      isMounted = false;
    };
  }, [initialOrgInfo]);

  const current = orgInfo ?? defaultOrg;

  return (
    <OrganizationContext.Provider
      value={{
        orgInfo: current,
        orgName: current.organizationName,
        logoUrl: resolveLogoUrl(current.logoUrl),
        footerText: current.footerText ?? defaultOrg.footerText ?? "",
        loading,
        setOrgInfo,
      }}
    >
      {children}
    </OrganizationContext.Provider>
  );
};

export const useOrganization = () => {
  const context = useContext(OrganizationContext);
  return context;
};
