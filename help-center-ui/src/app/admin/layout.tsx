"use client";

import React, { useState, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import type { LucideIcon } from "lucide-react";
import {
  BarChart3,
  ClipboardList,
  HelpCircle,
  Users,
  Building2,
  BookOpen,
  Terminal,
  MessageSquare,
  LayoutList,
  ShieldCheck,
  LayoutGrid,
  Inbox,
  FolderKanban,
  Menu
} from "lucide-react";
import { AuthService, type ModulePermission, type VerifyResponse, type MenuItemDto } from "@/services/common/AuthService";
import PasswordChangeModal from "@/components/auth/PasswordChangeModal";
import { AuthProvider } from "@/context/AuthContext";
import { OrganizationProvider } from "@/context/OrganizationContext";
import { AdminOrganizationService } from "@/services/admin/AdminOrganizationService";
import { Toaster } from "sonner";
import { cn } from "@/lib/utils";

// Modular Components
import { Sidebar } from "./layout-components/Sidebar";

const ICON_MAP: Record<string, LucideIcon> = {
  BarChart3,
  ClipboardList,
  Inbox,
  Building2,
  FolderKanban,
  Users,
  HelpCircle,
  BookOpen,
  Terminal,
  ShieldCheck,
  LayoutGrid,
  MessageSquare,
  LayoutList
};

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const router = useRouter();
  const isLoginPage = pathname === "/admin/login";

  const [isAuthorized, setIsAuthorized] = useState(false);
  const [isVerifying, setIsVerifying] = useState(true);
  const [modulePermissions, setModulePermissions] = useState<ModulePermission[]>([]);
  const [userInfo, setUserInfo] = useState<VerifyResponse | null>(null);
  const [menuItems, setMenuItems] = useState<MenuItemDto[]>([]);
  const [showPasswordChange, setShowPasswordChange] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [orgInfo, setOrgInfo] = useState<{ organizationName: string; logoUrl?: string; footerText?: string } | undefined>(undefined);

  useEffect(() => {
    if (isLoginPage) {
      return;
    }

    let isMounted = true;

    async function verifyAuth() {
      try {
        const response = await AuthService.verify();

        if (!isMounted) return;

        if (response.authenticated && response.role !== "Customer") {
          setIsAuthorized(true);
          setModulePermissions(response.permissions?.modules || []);
          setUserInfo(response);
          if (response.isPasswordChangeRequired) {
            setShowPasswordChange(true);
          }

          try {
            const dynamicItems = await AuthService.getMenuItems();
            if (isMounted) setMenuItems(dynamicItems);
          } catch (e) {
            console.error("Failed to fetch menu items:", e);
          }
        } else {
          router.push("/admin/login");
        }
      } catch (error) {
        console.error("Auth verification failed:", error);
        router.push("/admin/login");
      } finally {
        if (isMounted) {
          setIsVerifying(false);
        }
      }
    }

    verifyAuth();

    return () => {
      isMounted = false;
    };
  }, [isLoginPage, router]);

  useEffect(() => {
    if (!isLoginPage && isAuthorized) {
      AdminOrganizationService.getOrganizationInfo()
        .then((data) => setOrgInfo({ organizationName: data.organizationName, logoUrl: data.logoUrl, footerText: data.footerText }))
        .catch(() => {});
    }
  }, [isLoginPage, isAuthorized]);

  const filteredMenuGroups = React.useMemo(() => {
    if (menuItems.length > 0) {
      const groupsMap = new Map<string, { title: string; items: { icon: LucideIcon; label: string; href: string; resourceKey?: string }[] }>();
      let currentGroup = "Genel";

      for (const item of menuItems) {
        if (item.groupTitle) {
          currentGroup = item.groupTitle;
        }
        if (!groupsMap.has(currentGroup)) {
          groupsMap.set(currentGroup, { title: currentGroup, items: [] });
        }
        const IconComponent = ICON_MAP[item.icon] || BarChart3;
        groupsMap.get(currentGroup)!.items.push({
          icon: IconComponent,
          label: item.label,
          href: item.route,
          resourceKey: item.resourceKey
        });
      }
      return Array.from(groupsMap.values());
    }

    return [];
  }, [menuItems]);

  const handleLogout = async () => {
    await AuthService.logout();
    router.push("/admin/login");
  };

  if (isLoginPage) {
    return <>{children}</>;
  }

  if (isVerifying) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center gap-6">
        <div className="relative">
          <div className="w-16 h-16 border-4 border-indigo-600/20 border-t-indigo-600 rounded-full animate-spin" />
          <div className="absolute inset-0 bg-indigo-500 blur-3xl opacity-20 animate-pulse" />
        </div>
        <p className="text-slate-500 font-black tracking-[0.3em] uppercase text-[10px] animate-pulse">Sistem Yetkilendiriliyor...</p>
      </div>
    );
  }

  if (!isAuthorized) return null;

  return (
    <AuthProvider userInfo={userInfo} permissions={{ modules: modulePermissions }}>
      <OrganizationProvider initialOrgInfo={orgInfo}>
      <div className="min-h-screen bg-[#F8FAFC]">
        {/* Permanent Executive Sidebar */}
        <Sidebar
          isOpen={isMobileMenuOpen}
          onClose={() => setIsMobileMenuOpen(false)}
          pathname={pathname}
          menuGroups={filteredMenuGroups}
          onLogout={handleLogout}
          userInfo={userInfo}
        />

        {/* Modern Slim Mobile Top Bar (< lg) */}
        <header className="sticky top-0 z-40 flex h-12 w-full items-center justify-between border-b border-slate-800/80 bg-slate-950/90 px-3.5 text-white backdrop-blur-md lg:hidden">
          {/* Left: Menu Icon */}
          <button
            type="button"
            onClick={() => setIsMobileMenuOpen(true)}
            className="flex h-8 w-8 items-center justify-center rounded-lg border border-slate-800 bg-slate-900/80 text-slate-300 transition-all hover:bg-slate-800 hover:text-white active:scale-95 cursor-pointer"
            aria-label="Menüyü Aç"
          >
            <Menu size={17} />
          </button>

          {/* Center: Organization Name (Dynamic) */}
          <div className="flex items-center gap-2">
            <span className="text-xs font-black tracking-tight text-white truncate max-w-[180px] sm:max-w-xs">
              {orgInfo?.organizationName ?? "HelpCenter"}
            </span>
          </div>

          {/* Right: Balance spacer / Status indicator */}
          <div className="flex items-center justify-end w-8">
            <span className="h-2 w-2 rounded-full bg-emerald-500 shadow-sm shadow-emerald-500/50" />
          </div>
        </header>

        {/* Main Content Wrapper */}
        <div className={cn(
          "flex flex-col lg:ml-72 transition-all duration-300 ease-out",
          pathname.startsWith("/admin/requests/") && pathname !== "/admin/requests" && pathname !== "/admin/requests/assigned"
            ? "h-[calc(100dvh-3rem)] lg:h-dvh overflow-hidden" 
            : "min-h-screen"
        )}>
          {/* Dynamic Content Area */}
          <main className={cn(
            "relative",
            pathname.startsWith("/admin/requests/") && pathname !== "/admin/requests" && pathname !== "/admin/requests/assigned"
              ? "flex-1 p-2 sm:p-4 md:p-5 lg:p-6 flex flex-col h-full overflow-hidden"
              : "flex-1 p-3 sm:p-6 md:p-8 lg:p-10 overflow-hidden"
          )}>
            {/* Background Decorative Accents */}
            <div className="absolute top-0 right-0 w-125 h-125 bg-indigo-50/50 rounded-full blur-[120px] -z-10 pointer-events-none translate-x-1/2 -translate-y-1/2" />
            <div className="absolute bottom-0 left-0 w-125 h-125 bg-slate-100/50 rounded-full blur-[120px] -z-10 pointer-events-none -translate-x-1/2 translate-y-1/2" />

            <div className={cn(
              "relative z-10",
              pathname.startsWith("/admin/requests/") && pathname !== "/admin/requests" && pathname !== "/admin/requests/assigned"
                ? "w-full max-w-6xl mx-auto h-full flex flex-col min-h-0"
                : "max-w-400 mx-auto h-full"
            )}>
              {children}
            </div>
          </main>
        </div>

        <Toaster richColors position="top-right" theme="system" />

        {showPasswordChange && (
          <PasswordChangeModal
            email={userInfo?.username ?? ""}
            isCustomer={false}
            onSuccess={() => setShowPasswordChange(false)}
          />
        )}
      </div>
      </OrganizationProvider>
    </AuthProvider>
  );
}
