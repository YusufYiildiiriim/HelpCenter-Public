"use client";

import React, { useState, useMemo } from "react";
import { motion } from "framer-motion";
import { Loader2, Trash2, Shield, Database, Search, X, Check } from "lucide-react";
import type { Role } from "@/services/admin/AdminRoleService";
import { cn } from "@/lib/utils";

interface RoleSelectorListProps {
  roles: Role[];
  selectedRole: Role | null;
  onSelectRole: (role: Role) => void;
  onDeleteRole: (id: string) => void;
  deletingId: string | null;
  canDelete: boolean;
}

export const RoleSelectorList: React.FC<RoleSelectorListProps> = ({
  roles,
  selectedRole,
  onSelectRole,
  onDeleteRole,
  deletingId,
  canDelete,
}) => {
  const [searchTerm, setSearchTerm] = useState("");

  const filteredRoles = useMemo(() => {
    if (!searchTerm.trim()) return roles;
    const q = searchTerm.toLowerCase().trim();
    return roles.filter(
      (r) => r.name.toLowerCase().includes(q) || (r.description && r.description.toLowerCase().includes(q))
    );
  }, [roles, searchTerm]);

  return (
    <div className="w-full lg:w-80 shrink-0 flex flex-col gap-3">
      {/* Header & Search Bar */}
      <div className="flex flex-col gap-2.5 bg-slate-900/90 backdrop-blur-xl p-3.5 rounded-2xl border border-slate-800 shadow-xl shadow-slate-950/20">
        <div className="flex items-center justify-between px-1">
          <span className="text-[10px] font-black uppercase tracking-widest text-slate-400">
            Rol Listesi ({filteredRoles.length}/{roles.length})
          </span>
        </div>

        {/* Live Search Input */}
        <div className="relative">
          <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Rol ismi ile ara..."
            aria-label="Rollerde ara"
            className="w-full bg-slate-800/70 hover:bg-slate-800/80 focus:bg-slate-800 text-slate-100 placeholder:text-slate-500 text-xs font-semibold pl-8 pr-7 py-2 rounded-xl border border-slate-700 focus:border-indigo-500 focus:outline-hidden transition-all"
          />
          {searchTerm && (
            <button
              type="button"
              aria-label="Rol aramasını temizle"
              onClick={() => setSearchTerm("")}
              className="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-200 p-0.5 rounded-full"
            >
              <X size={12} />
            </button>
          )}
        </div>

      </div>

      {/* Keep role choices visible at every viewport size. */}
      <div role="group" aria-label="Roller" className="flex flex-col gap-2 overflow-y-auto max-h-[420px] lg:max-h-[calc(100vh-320px)] pb-1 pr-1 custom-scrollbar">
        {filteredRoles.length === 0 ? (
          <div className="w-full p-6 text-center bg-slate-900 rounded-2xl border border-dashed border-slate-700">
            <p className="text-xs font-bold text-slate-400">Rol bulunamadı</p>
          </div>
        ) : (
          filteredRoles.map((role, i) => {
            const isSelected = selectedRole?.id === role.id;
            return (
              <motion.div
                key={role.id}
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: i * 0.03 }}
                onClick={() => onSelectRole(role)}
                role="button"
                aria-pressed={isSelected}
                aria-label={`${role.name} rolünü seç`}
                tabIndex={0}
                onKeyDown={(e) => {
                  if (e.target !== e.currentTarget) return;
                  if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    onSelectRole(role);
                  }
                }}
                className={cn(
                  "group shrink-0 min-w-0 w-full text-left p-3 lg:p-4 rounded-2xl border transition-all duration-200 cursor-pointer select-none focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-400",
                  isSelected
                    ? "bg-slate-900 border-purple-500/60 text-white shadow-lg shadow-purple-950/20 ring-1 ring-purple-500/20"
                    : "bg-slate-900 border-slate-800 text-slate-300 hover:border-slate-600 hover:shadow-md"
                )}
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0 flex-1">
                    <div className="hidden lg:flex items-center gap-1.5 mb-1">
                      <span
                        className={cn(
                          "w-1.5 h-1.5 rounded-full shrink-0",
                          isSelected
                            ? "bg-indigo-400"
                            : role.name === "Admin"
                            ? "bg-amber-500"
                            : "bg-emerald-500"
                        )}
                      />
                      <span
                        className={cn(
                          "text-[9px] font-black uppercase tracking-widest",
                          isSelected ? "text-indigo-300" : "text-slate-500"
                        )}
                      >
                        {role.name === "Admin" ? "Sistem Rolü" : "Özel Rol"}
                      </span>
                    </div>
                    <p className={cn("font-black text-sm leading-tight break-words", isSelected ? "text-white" : "text-slate-100")}>
                      {role.name}
                    </p>
                    {role.description && (
                      <p className={cn("text-[10px] mt-0.5 truncate font-medium", isSelected ? "text-slate-300" : "text-slate-400")}>
                        {role.description}
                      </p>
                    )}
                  </div>

                  {isSelected && <Check size={16} aria-hidden="true" className="text-purple-300 shrink-0 mt-1" />}

                  {role.name !== "Admin" && canDelete && (
                    <button
                      type="button"
                      aria-label={`${role.name} rolünü sil`}
                      onClick={(e) => {
                        e.stopPropagation();
                        if (!role.hasUsers) onDeleteRole(role.id);
                      }}
                      disabled={deletingId === role.id || role.hasUsers}
                      title={role.hasUsers ? "Kullanıcıya atanmış, silinemez" : "Sil"}
                      className={cn(
                        "p-1.5 rounded-xl transition-all shrink-0",
                        isSelected
                          ? "hover:bg-slate-800/80 text-slate-400 hover:text-rose-400"
                          : role.hasUsers
                          ? "text-slate-600 cursor-not-allowed"
                          : "hover:bg-rose-500/15 text-slate-500 hover:text-rose-400"
                      )}
                    >
                      {deletingId === role.id ? <Loader2 size={13} className="animate-spin" /> : <Trash2 size={13} />}
                    </button>
                  )}
                </div>

                {/* Stats Chips */}
                <div className="flex items-center gap-1.5 mt-2 pt-1.5 lg:mt-3 lg:pt-2 border-t border-slate-800">
                  <span
                    className={cn(
                      "flex items-center gap-1 text-[9px] font-bold px-2 py-0.5 rounded-lg border",
                      isSelected
                        ? "bg-indigo-500/15 border-indigo-500/30 text-indigo-300"
                        : "bg-indigo-500/10 border-indigo-500/30 text-indigo-400"
                    )}
                  >
                    <Shield size={9} />
                    {role.permissionCount} izin
                  </span>
                  <span
                    className={cn(
                      "flex items-center gap-1 text-[9px] font-bold px-2 py-0.5 rounded-lg border",
                      isSelected
                        ? "bg-slate-800/80 border-slate-700 text-slate-200"
                        : role.hasUsers
                        ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-400"
                        : "bg-slate-800/50 border-slate-700 text-slate-400"
                    )}
                  >
                    <Database size={9} />
                    {role.hasUsers ? "Kullanıcı Var" : "Kullanıcı Yok"}
                  </span>
                </div>
              </motion.div>
            );
          })
        )}
      </div>
    </div>
  );
};
