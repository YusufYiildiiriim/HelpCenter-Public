"use client";

import { useEffect } from "react";
import { useOrganization } from "@/context/OrganizationContext";

export function usePageTitle(pageTitle: string) {
  const { orgName } = useOrganization();

  useEffect(() => {
    if (pageTitle) {
      document.title = `${orgName} - ${pageTitle}`;
    } else {
      document.title = orgName;
    }
  }, [pageTitle, orgName]);
}
