"use client";

import React, { useState, useMemo } from "react";
import { toast } from "sonner";
import { X, Shield, Search, Loader2, CheckCircle2, SlidersHorizontal } from "lucide-react";
import type { Role } from "@/services/admin/AdminRoleService";
import { cn } from "@/lib/utils";

interface RoleAssignmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  userName: string;
  userRoles: { roleId: number; roleName: string }[];
  allRoles: Role[];
  onSave: (roleIds: number[]) => Promise<void>;
}

export const RoleAssignmentModal: React.FC<RoleAssignmentModalProps> = ({
  isOpen,
  onClose,
  userName,
  userRoles,
  allRoles,
  onSave
}) => {
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState(false);
  const [wasOpen, setWasOpen] = useState(false);

  if (isOpen && !wasOpen) {
    setWasOpen(true);
    setSelectedIds(userRoles.map(ur => ur.roleId));
    setSearchTerm("");
    setSaving(false);
    setSuccess(false);
  } else if (!isOpen && wasOpen) {
    setWasOpen(false);
  }

  const filteredRoles = useMemo(() => {
    if (!searchTerm.trim()) return allRoles;
    const term = searchTerm.toLowerCase();
    return allRoles.filter(r =>
      r.name.toLowerCase().includes(term) ||
      (r.description && r.description.toLowerCase().includes(term))
    );
  }, [allRoles, searchTerm]);

  const toggleRole = (roleId: number) => {
    setSelectedIds(prev =>
      prev.includes(roleId) ? prev.filter(id => id !== roleId) : [...prev, roleId]
    );
  };

  const handleSelectAll = () => {
    const filteredIds = filteredRoles.map(r => r.id);
    const allSelected = filteredIds.every(id => selectedIds.includes(id));
    if (allSelected) {
      setSelectedIds(prev => prev.filter(id => !filteredIds.includes(id)));
    } else {
      setSelectedIds(prev => [...new Set([...prev, ...filteredIds])]);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await onSave(selectedIds);
      setSuccess(true);
      setTimeout(() => {
        onClose();
        setSuccess(false);
      }, 1200);
    } catch (err) {
      console.error("Failed to save roles:", err);
      toast.error("Roller güncellenirken bir hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  const changed = JSON.stringify([...selectedIds].sort()) !== JSON.stringify([...userRoles.map(ur => ur.roleId)].sort());

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-4 lg:pl-76 lg:pr-6 overflow-y-auto">
      <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm transition-opacity" onClick={() => !saving && onClose()} />

      <div className="bg-slate-900 w-full max-w-lg rounded-2xl shadow-2xl relative overflow-hidden animate-in zoom-in-95 fade-in duration-200 my-auto z-10 border border-slate-800">
        {/* Header */}
        <div className="px-6 py-4 sm:px-7 sm:py-5 flex justify-between items-center bg-gradient-to-br from-violet-600 to-indigo-700 text-white">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-2xl bg-white/20 flex items-center justify-center text-white backdrop-blur-md">
              <Shield size={20} />
            </div>
            <div>
              <h3 className="text-base sm:text-lg font-black tracking-tight">Rol Ataması</h3>
              <p className="text-xs text-white/80 font-medium">
                <span className="font-bold text-white underline">{userName}</span> için rol seçin
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            disabled={saving}
            className="w-8 h-8 rounded-xl bg-white/10 hover:bg-white/20 flex items-center justify-center text-white transition-all"
          >
            <X size={16} />
          </button>
        </div>

        {success ? (
          <div className="px-6 py-12 flex flex-col items-center text-center gap-3">
            <div className="w-14 h-14 bg-emerald-500/15 text-emerald-400 rounded-2xl flex items-center justify-center border border-emerald-500/30">
              <CheckCircle2 size={30} />
            </div>
            <p className="text-emerald-400 font-black text-base">Roller güncellendi!</p>
            <p className="text-slate-400 text-xs font-medium">Lütfen bekleyin...</p>
          </div>
        ) : (
          <div className="p-5 sm:p-6 space-y-4">
            {/* Search & Actions */}
            <div className="flex items-center gap-2">
              <div className="relative flex-1">
                <Search size={14} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500" />
                <input
                  type="text"
                  placeholder="Rol ara..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-9 pr-3.5 py-2.5 bg-slate-800/70 border border-slate-700 rounded-2xl text-xs font-semibold focus:border-indigo-500 focus:bg-slate-800 focus:outline-hidden transition-all text-slate-100 placeholder:text-slate-500"
                />
              </div>
              <button
                type="button"
                onClick={handleSelectAll}
                className="flex items-center gap-1.5 px-3 py-2.5 rounded-2xl border border-slate-700 text-[10px] font-black text-slate-300 uppercase tracking-wider hover:bg-slate-800 transition-all shrink-0"
              >
                <SlidersHorizontal size={13} />
                Tümünü Seç
              </button>
            </div>

            {/* Role List */}
            <div className="max-h-[300px] overflow-y-auto pr-1 space-y-2 rounded-2xl">
              {filteredRoles.length === 0 ? (
                <div className="text-center py-8 bg-slate-800/60 rounded-2xl border border-dashed border-slate-700">
                  <p className="text-xs font-bold text-slate-400">Rol bulunamadı</p>
                </div>
              ) : (
                filteredRoles.map(role => {
                  const selected = selectedIds.includes(role.id);
                  return (
                    <div
                      key={role.id}
                      onClick={() => toggleRole(role.id)}
                      className={cn(
                        "flex items-center justify-between gap-3 px-4 py-3 rounded-2xl border cursor-pointer transition-all duration-150",
                        selected
                          ? "bg-indigo-600/20 border-indigo-500/50 text-white shadow-xs"
                          : "bg-slate-800/50 border-slate-700 text-slate-300 hover:border-slate-500 hover:bg-slate-800"
                      )}
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div className={cn(
                          "w-5 h-5 rounded-lg shrink-0 border flex items-center justify-center transition-all",
                          selected ? "bg-indigo-600 border-indigo-600 text-white" : "border-slate-600 bg-slate-900/60"
                        )}>
                          {selected && <CheckCircle2 size={13} />}
                        </div>
                        <div className="min-w-0">
                          <span className="text-xs font-bold tracking-tight block truncate">
                            {role.name}
                          </span>
                          {role.description && (
                            <p className="text-[10px] font-medium text-slate-500 truncate">
                              {role.description}
                            </p>
                          )}
                        </div>
                      </div>

                      {role.permissionCount > 0 && (
                        <span className="text-[9px] font-black uppercase tracking-wider px-2 py-0.5 rounded-full bg-slate-800 text-slate-400 border border-slate-700 shrink-0">
                          {role.permissionCount} izin
                        </span>
                      )}
                    </div>
                  );
                })
              )}
            </div>

            {/* Footer Actions */}
            <div className="flex items-center gap-3 pt-3 border-t border-slate-800">
              <button
                type="button"
                onClick={onClose}
                disabled={saving}
                className="flex-1 px-4 py-3 rounded-2xl border border-slate-700 text-slate-300 text-xs font-bold hover:bg-slate-800 transition-all active:scale-95 disabled:opacity-50"
              >
                İptal
              </button>
              <button
                type="button"
                onClick={handleSave}
                disabled={!changed || saving}
                className={cn(
                  "flex-1 flex items-center justify-center gap-2 py-3 rounded-2xl text-xs font-bold text-white transition-all active:scale-95 disabled:opacity-50 shadow-md",
                  changed ? "bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 shadow-indigo-600/20" : "bg-slate-700 cursor-not-allowed"
                )}
              >
                {saving ? (
                  <>
                    <Loader2 className="animate-spin" size={15} />
                    Kaydediliyor...
                  </>
                ) : (
                  "Kaydet"
                )}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};