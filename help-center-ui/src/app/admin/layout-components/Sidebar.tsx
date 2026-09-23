"use client";

import React, { useMemo } from "react";
import Link from "next/link";
import {
  LogOut,
  X,
  type LucideIcon
} from "lucide-react";
import { cn } from "@/lib/utils";
import { motion, AnimatePresence } from "framer-motion";
import type { VerifyResponse } from "@/services/common/AuthService";
import { useOrganization } from "@/context/OrganizationContext";

interface SidebarItem {
  icon: LucideIcon;
  label: string;
  href: string;
  resourceKey?: string;
}

interface SidebarGroup {
  title: string;
  items: SidebarItem[];
}

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
  pathname: string;
  menuGroups: SidebarGroup[];
  onLogout: () => void;
  userInfo: VerifyResponse | null;
}

export const Sidebar: React.FC<SidebarProps> = ({
  isOpen,
  onClose,
  pathname,
  menuGroups,
  onLogout,
  userInfo
}) => {
  const { orgName, logoUrl } = useOrganization();
  const initials = useMemo(() => {
    const username = userInfo?.username || "Admin Kullanıcı";
    return username.substring(0, 2).toUpperCase();
  }, [userInfo?.username]);

  return (
    <>
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 z-[60] bg-slate-950/80 backdrop-blur-sm lg:hidden"
          />
        )}
      </AnimatePresence>

      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-[70] w-72 flex-col border-r border-slate-900/50 bg-slate-950 text-slate-300 shadow-2xl transition-all duration-300 ease-out lg:flex lg:translate-x-0 lg:shadow-none",
          isOpen ? "flex translate-x-0" : "hidden lg:flex -translate-x-full lg:translate-x-0"
        )}
      >
        <div className="pointer-events-none absolute left-0 top-0 h-96 w-full bg-indigo-600/5 blur-[120px]" />

        {/* Header */}
        <div className="relative z-10 flex items-center justify-between p-5">
          <div className="flex min-w-0 items-center gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl bg-slate-900 border border-slate-800 p-1.5 shadow-lg shadow-indigo-600/20 overflow-hidden">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={logoUrl}
                alt={`${orgName} Logo`}
                className="h-full w-full object-contain"
                onError={(e) => {
                  const img = e.currentTarget;
                  if (img.src !== window.location.origin + "/logo.png") {
                    img.src = "/logo.png";
                  }
                }}
              />
            </div>
            <div className="min-w-0 overflow-hidden">
              <span className="block text-xl font-black leading-none tracking-tight text-white">{orgName}</span>
              <span className="mt-1 block text-[10px] font-black uppercase tracking-widest text-indigo-400">Admin Panel</span>
            </div>
          </div>

          <button
            type="button"
            className="flex h-9 w-9 items-center justify-center rounded-xl border border-slate-800 bg-slate-900/60 text-slate-400 transition-all hover:text-white lg:hidden"
            onClick={onClose}
          >
            <X size={18} />
          </button>
        </div>

        {/* Navigation */}
        <nav className="relative z-10 flex-1 space-y-7 overflow-y-auto px-3 py-3 scrollbar-hide [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden">
          {menuGroups.map((group) => (
            <div key={group.title} className="space-y-2">
              <h3 className="flex items-center gap-2 px-3 text-[10px] font-black uppercase tracking-[0.22em] text-slate-500">
                <span className="h-1.5 w-1.5 rounded-full bg-slate-800" />
                <span className="overflow-hidden whitespace-nowrap">
                  {group.title}
                </span>
              </h3>

              <div className="space-y-1">
                {group.items.map((item) => {
                  const isActive = pathname === item.href;
                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      onClick={() => {
                        if (window.innerWidth < 1024) onClose();
                      }}
                      className={cn(
                        "group relative flex h-11 items-center gap-3 rounded-xl px-3 transition-all duration-300",
                        isActive
                          ? "bg-indigo-600 text-white shadow-lg shadow-indigo-600/20"
                          : "text-slate-500 hover:bg-slate-900 hover:text-white"
                      )}
                    >
                      <item.icon
                        size={19}
                        strokeWidth={isActive ? 2.5 : 2}
                        className={cn("shrink-0 transition-colors", isActive ? "text-white" : "group-hover:text-indigo-400")}
                      />
                      <span className={cn(
                        "flex-1 overflow-hidden whitespace-nowrap text-[13px] font-bold tracking-tight transition-all duration-300",
                        isActive ? "text-white" : "text-slate-400 group-hover:text-slate-200"
                      )}>
                        {item.label}
                      </span>
                    </Link>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>

        {/* Footer Profile & Logout */}
        <div className="relative z-10 mt-auto border-t border-slate-900/70 p-3">
          <div className="mb-2 flex items-center gap-3 rounded-2xl border border-slate-800/80 bg-slate-900/45 px-3 py-3">
            <div className="relative shrink-0">
              <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-white text-sm font-black text-slate-950">
                {initials}
              </div>
              <span className="absolute -bottom-0.5 -right-0.5 h-3.5 w-3.5 rounded-full border-2 border-slate-900 bg-emerald-500" />
            </div>
            <div className="min-w-0 overflow-hidden">
              <p className="truncate text-[12px] font-black text-white">{userInfo?.username || "Admin Kullanıcı"}</p>
              <p className="mt-1 flex items-center gap-1.5 text-[9px] font-black uppercase tracking-widest text-slate-500">
                <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
                {userInfo?.role || "Sistem Yetkilisi"}
              </p>
            </div>
          </div>

          <button
            onClick={onLogout}
            className="group flex h-11 w-full items-center gap-3 rounded-xl border border-transparent bg-slate-900/30 px-3 text-left text-slate-500 transition-all hover:border-red-500/20 hover:bg-red-500/10 hover:text-red-400"
          >
            <LogOut size={19} className="shrink-0 transition-colors group-hover:text-red-400" />
            <span className="overflow-hidden whitespace-nowrap text-[10px] font-black uppercase tracking-widest transition-all duration-300">
              Güvenli Çıkış
            </span>
          </button>
        </div>
      </aside>
    </>
  );
};
