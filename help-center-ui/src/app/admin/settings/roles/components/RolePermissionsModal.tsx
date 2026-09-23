"use client";

import React, { useEffect, useMemo, useState } from "react";
import { X, Save, Loader2, Shield, Package, Sliders, Check } from "lucide-react";
import type {
    Role, RolePermissionDto
} from "@/services/admin/AdminRoleService";
import {
    AdminRoleService as RoleService
} from "@/services/admin/AdminRoleService";
import type { ResourceDefinition } from "@/services/admin/AdminResourceService";
import { AdminResourceService } from "@/services/admin/AdminResourceService";
import type { FeaturePackage } from "@/services/admin/AdminFeatureService";
import { AdminFeatureService } from "@/services/admin/AdminFeatureService";
import { toast } from "sonner";
import { PermissionsTable } from "./PermissionsTable";
import { cn } from "@/lib/utils";

interface RolePermissionsModalProps {
    role: Role;
    onClose: () => void;
    onSuccess: () => void;
}

type Tab = "packages" | "details";

const RESOURCE_TURKISH: Record<string, string> = {
    Dashboard: "İstatistikler",
    Requests: "Talepler",
    AssignedRequests: "Bana Atanan Talepler",
    CustomerRequests: "Müşteri Talepleri",
    Companies: "Firmalar",
    Projects: "Projeler",
    Users: "Kullanıcılar",
    Customers: "Müşteriler",
    FAQ: "SSS Yönetimi",
    Guide: "Kullanım Kılavuzu",
    Roles: "Roller",
    Modules: "Modüller",
    Subjects: "Talep Konuları",
    Statuses: "Talep Durumları",
};

const ACTION_TURKISH: Record<string, string> = {
    Read: "Görüntüle",
    Create: "Oluştur",
    Update: "Güncelle",
    Delete: "Sil",
    Export: "Dışa Aktar",
    Print: "Yazdır",
    Approve: "Onayla",
    Reject: "Reddet",
    Assign: "Ata",
    ChangeStatus: "Durum Değiştir",
    ManageMembers: "Üye Yönetimi",
    ManageProjects: "Proje Yönetimi",
    ManageModules: "Modül Yönetimi",
    ManageExperts: "Uzman Yönetimi",
    LookupSelect: "Seçim Listesi",
};

function formatPermissionTurkish(resourceKey: string, actionKey: string): string {
    const res = RESOURCE_TURKISH[resourceKey] || resourceKey;
    const act = ACTION_TURKISH[actionKey] || actionKey;
    return `${res}: ${act}`;
}

export default function RolePermissionsModal({ role, onClose, onSuccess }: RolePermissionsModalProps) {
    const [resources, setResources] = useState<ResourceDefinition[]>([]);
    const [features, setFeatures] = useState<FeaturePackage[]>([]);
    const [permissions, setPermissions] = useState<RolePermissionDto[]>([]);
    const [loadingPerms, setLoadingPerms] = useState(true);
    const [savingPerms, setSavingPerms] = useState(false);
    const [activeTab, setActiveTab] = useState<Tab>("packages");

    useEffect(() => {
        let mounted = true;
        Promise.allSettled([
            AdminResourceService.getDefinitions(),
            AdminFeatureService.getFeatures(),
            RoleService.getPermissions(role.id),
        ]).then(([defsRes, featsRes, permsRes]) => {
            if (!mounted) return;
            const defs = defsRes.status === "fulfilled" && Array.isArray(defsRes.value) ? defsRes.value : [];
            const feats = featsRes.status === "fulfilled" && Array.isArray(featsRes.value) ? featsRes.value : [];
            const permsRaw = permsRes.status === "fulfilled" ? permsRes.value : [];
            const perms: RolePermissionDto[] = Array.isArray(permsRaw)
                ? permsRaw
                : (permsRaw as { items?: RolePermissionDto[]; data?: RolePermissionDto[] })?.items ||
                  (permsRaw as { items?: RolePermissionDto[]; data?: RolePermissionDto[] })?.data || [];

            if (defsRes.status === "rejected") console.error("Error fetching resource definitions:", defsRes.reason);
            if (featsRes.status === "rejected") console.error("Error fetching feature packages:", featsRes.reason);
            if (permsRes.status === "rejected") console.error("Error fetching role permissions:", permsRes.reason);

            setResources(defs);
            setFeatures(feats);

            const merged: RolePermissionDto[] = defs.map(def => {
                const existing = perms.find((p: RolePermissionDto) => p?.resourceKey === def.key);
                return existing
                    ? { ...existing, resourceKey: existing.resourceKey || def.key, actions: Array.isArray(existing.actions) ? existing.actions : [] }
                    : {
                        roleId: role.id,
                        resourceKey: def.key,
                        actions: [],
                        allowedFieldsJson: null,
                    };
            });
            setPermissions(merged);
            setLoadingPerms(false);
        });
        return () => { mounted = false; };
    }, [role.id]);

    const activePackageKeys = useMemo(() => {
        const permMap = new Map(permissions.map(p => [p.resourceKey, new Set(p.actions)]));
        return new Set(
            features
                .filter(pkg => pkg.permissions.every(pair => permMap.get(pair.resource)?.has(pair.action)))
                .map(pkg => pkg.key)
        );
    }, [features, permissions]);

    const handleToggleAction = (resourceKey: string, action: string) => {
        setPermissions(prev => prev.map(p => {
            if (p.resourceKey !== resourceKey) return p;
            const has = p.actions.includes(action);
            return { ...p, actions: has ? p.actions.filter(a => a !== action) : [...p.actions, action] };
        }));
    };

    const handleToggleAll = (resourceKey: string, value: boolean) => {
        const def = resources.find(r => r.key === resourceKey);
        if (!def) return;
        const allActions = def.actions.map(a => a.key);
        setPermissions(prev => prev.map(p =>
            p.resourceKey === resourceKey ? { ...p, actions: value ? [...allActions] : [] } : p
        ));
    };

    const handleAllowedFieldsChange = (resourceKey: string, fields: string[]) => {
        setPermissions(prev => prev.map(p =>
            p.resourceKey === resourceKey ? { ...p, allowedFieldsJson: JSON.stringify(fields) } : p
        ));
    };

    const applyPackage = (pkg: FeaturePackage, apply: boolean) => {
        setPermissions(prev => prev.map(p => {
            const pairsForResource = pkg.permissions
                .filter(x => x.resource === p.resourceKey)
                .map(x => x.action);
            if (pairsForResource.length === 0) return p;

            const set = new Set(p.actions);
            if (apply) {
                pairsForResource.forEach(a => set.add(a));
            } else {
                pairsForResource.forEach(a => set.delete(a));
            }
            return { ...p, actions: Array.from(set) };
        }));
    };

    const handleSavePermissions = async () => {
        try {
            setSavingPerms(true);
            await RoleService.updatePermissions(role.id, permissions);
            toast.success("Yetkiler başarıyla güncellendi.");
            onSuccess();
        } catch {
            toast.error("Yetkiler kaydedilirken hata oluştu.");
        } finally {
            setSavingPerms(false);
        }
    };

    return (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-4 md:p-6 lg:pl-76 lg:pr-6 overflow-y-auto">
            <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-md animate-in fade-in duration-300" onClick={onClose} />
            <div className="relative w-full max-w-5xl bg-slate-900 rounded-2xl sm:rounded-3xl shadow-2xl border border-slate-800 flex flex-col max-h-[82vh] sm:max-h-[86vh] my-auto animate-in zoom-in slide-in-from-bottom-4 duration-300 overflow-hidden">

                {/* Header */}
                <div className="p-3.5 sm:p-6 border-b border-slate-800 bg-slate-950/80 shrink-0">
                    <div className="flex justify-between items-center gap-3">
                        <div className="flex items-center gap-3 min-w-0">
                            <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl sm:rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20 shrink-0">
                                <Shield size={20} className="sm:w-6 sm:h-6" />
                            </div>
                            <div className="min-w-0">
                                <h2 className="text-base sm:text-xl md:text-2xl font-black text-white leading-none truncate">Yetki Yönetimi</h2>
                                <p className="text-slate-400 text-[11px] sm:text-xs font-medium mt-1 truncate max-w-xs sm:max-w-none">
                                    {role.name} · {role.description || "Rol yetkileri"}
                                </p>
                            </div>
                        </div>
                        <button 
                            onClick={onClose} 
                            className="w-8 h-8 sm:w-10 sm:h-10 rounded-xl bg-slate-800/80 border border-slate-700 flex items-center justify-center text-slate-400 hover:bg-slate-800 hover:text-white transition-all shadow-sm shrink-0 cursor-pointer"
                        >
                            <X size={18} />
                        </button>
                    </div>

                    {/* Segmented Tab Switcher */}
                    <div className="mt-3 sm:mt-5 grid grid-cols-2 sm:flex gap-1.5 p-1 bg-slate-950/90 rounded-xl sm:rounded-2xl border border-slate-800">
                        <TabButton
                            active={activeTab === "packages"}
                            onClick={() => setActiveTab("packages")}
                            icon={<Package size={14} />}
                            label="Hazır Paketler"
                            count={activePackageKeys.size}
                        />
                        <TabButton
                            active={activeTab === "details"}
                            onClick={() => setActiveTab("details")}
                            icon={<Sliders size={14} />}
                            label="Detay Yetkiler"
                        />
                    </div>
                </div>

                {/* Content */}
                <div className="flex-1 overflow-y-auto p-3 sm:p-6 bg-slate-900 custom-scrollbar">
                    {loadingPerms ? (
                        <LoadingState label="Yetkiler Yükleniyor..." />
                    ) : activeTab === "packages" ? (
                        <PackageGrid
                            packages={features}
                            activeKeys={activePackageKeys}
                            onToggle={applyPackage}
                        />
                    ) : (
                        <PermissionsTable
                            permissions={permissions}
                            resources={resources}
                            onToggle={handleToggleAction}
                            onToggleAll={handleToggleAll}
                            onAllowedFieldsChange={handleAllowedFieldsChange}
                        />
                    )}
                </div>

                {/* Footer */}
                <div className="p-3 sm:p-5 border-t border-slate-800 bg-slate-950/90 flex items-center justify-end gap-2.5 shrink-0">
                    <button 
                        onClick={onClose} 
                        className="px-4 sm:px-6 py-2.5 sm:py-3 bg-white/10 border border-slate-700/80 text-slate-300 rounded-xl sm:rounded-2xl font-black uppercase tracking-wider text-xs hover:bg-white/20 transition-all active:scale-95 cursor-pointer shrink-0"
                    >
                        Vazgeç
                    </button>
                    <button
                        onClick={handleSavePermissions}
                        disabled={savingPerms || loadingPerms}
                        className="flex-1 sm:flex-none px-4 sm:px-6 py-2.5 sm:py-3 bg-gradient-to-r from-indigo-600 to-violet-600 text-white rounded-xl sm:rounded-2xl font-black uppercase tracking-wider text-xs hover:from-indigo-500 hover:to-violet-500 shadow-lg shadow-indigo-600/30 transition-all disabled:opacity-50 flex items-center justify-center gap-2 active:scale-95 cursor-pointer min-w-0"
                    >
                        {savingPerms ? <Loader2 className="animate-spin shrink-0" size={16} /> : <Save className="shrink-0" size={16} />}
                        <span className="truncate whitespace-nowrap">Değişiklikleri Kaydet</span>
                    </button>
                </div>
            </div>
        </div>
    );
}

function TabButton({
    active, onClick, icon, label, count,
}: {
    active: boolean;
    onClick: () => void;
    icon: React.ReactNode;
    label: string;
    count?: number;
}) {
    return (
        <button
            onClick={onClick}
            className={cn(
                "w-full justify-center flex items-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 rounded-lg text-[10px] sm:text-xs font-black uppercase tracking-wider transition-all cursor-pointer truncate",
                active
                    ? "bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-md shadow-indigo-600/30"
                    : "text-slate-400 hover:text-slate-200 hover:bg-slate-900/60"
            )}
        >
            {icon}
            <span className="truncate">{label}</span>
            {count !== undefined && count > 0 && (
                <span className="px-1.5 py-0.5 rounded-md bg-white/20 text-white text-[9px] font-bold shrink-0">{count}</span>
            )}
        </button>
    );
}

function PackageGrid({
    packages, activeKeys, onToggle,
}: {
    packages: FeaturePackage[];
    activeKeys: Set<string>;
    onToggle: (pkg: FeaturePackage, apply: boolean) => void;
}) {
    if (packages.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 gap-3">
                <Package className="text-slate-600" size={32} />
                <p className="text-slate-500 font-bold text-xs">Hazır paket bulunmuyor.</p>
            </div>
        );
    }

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3 sm:gap-4">
            {packages.map(pkg => {
                const active = activeKeys.has(pkg.key);
                return (
                    <button
                        key={pkg.key}
                        type="button"
                        onClick={() => onToggle(pkg, !active)}
                        className={cn(
                            "text-left p-4 sm:p-5 rounded-2xl border transition-all active:scale-[0.98] cursor-pointer flex flex-col justify-between",
                            active
                                ? "bg-indigo-600/15 border-indigo-500/80 shadow-lg shadow-indigo-600/10"
                                : "bg-slate-800/40 border-slate-800 hover:border-indigo-500/40 hover:bg-slate-800/70"
                        )}
                    >
                        <div className="flex items-start justify-between gap-3">
                            <div className="min-w-0 flex-1">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <span className={cn(
                                        "text-sm sm:text-base font-black tracking-tight",
                                        active ? "text-indigo-200" : "text-slate-100"
                                    )}>
                                        {pkg.displayName}
                                    </span>
                                </div>
                                <p className="text-xs text-slate-400 mt-1 leading-relaxed">{pkg.description}</p>
                            </div>
                            <div className={cn(
                                "w-8 h-8 rounded-xl flex items-center justify-center shrink-0 border transition-all",
                                active
                                    ? "bg-indigo-600 border-indigo-500 text-white shadow-md"
                                    : "bg-slate-900 border-slate-700 text-slate-500"
                            )}>
                                {active ? <Check size={16} strokeWidth={3} /> : <Package size={14} />}
                            </div>
                        </div>

                        {/* Tüm alt maddeler Türkçe isimleriyle ve eksiksiz gösterilir */}
                        <div className="mt-3 sm:mt-4 pt-3 border-t border-slate-800/80">
                            <span className="text-[10px] font-black uppercase tracking-wider text-slate-500 mb-2 block">
                                Dahil Olan İzinler ({pkg.permissions.length}):
                            </span>
                            <div className="flex flex-wrap gap-1.5">
                                {pkg.permissions.map((p, i) => (
                                    <span 
                                        key={i} 
                                        className={cn(
                                            "text-[9px] sm:text-[10px] font-bold px-2 py-0.5 sm:px-2.5 sm:py-1 rounded-lg border flex items-center gap-1.5 transition-colors truncate max-w-full",
                                            active 
                                                ? "bg-indigo-500/20 border-indigo-500/40 text-indigo-200" 
                                                : "bg-slate-950/80 border-slate-800 text-slate-300"
                                        )}
                                    >
                                        <span className={cn("w-1.5 h-1.5 rounded-full shrink-0", active ? "bg-indigo-400" : "bg-slate-500")} />
                                        <span className="truncate">{formatPermissionTurkish(p.resource, p.action)}</span>
                                    </span>
                                ))}
                            </div>
                        </div>
                    </button>
                );
            })}
            <div className="md:col-span-2 mt-2 p-4 rounded-2xl bg-slate-800/40 border border-slate-800 text-xs text-slate-400 leading-relaxed">
                <strong className="text-slate-200 font-bold">Not:</strong> Paket seçimi izinleri toplu işaretler. İzin detaylarını özelleştirmek için <strong className="text-indigo-400 font-bold">Detay Yetkiler</strong> sekmesini kullanabilirsiniz.
            </div>
        </div>
    );
}

function LoadingState({ label }: { label: string }) {
    return (
        <div className="flex flex-col items-center justify-center py-20 gap-4">
            <Loader2 className="animate-spin text-indigo-500" size={40} />
            <p className="text-slate-400 font-bold uppercase tracking-widest text-xs">{label}</p>
        </div>
    );
}
