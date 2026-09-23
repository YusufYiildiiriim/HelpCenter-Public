"use client";

import React, { useState } from "react";
import {
    Eye, Plus, Edit, Trash2, Download, Printer, CheckCircle, XCircle, UserPlus, RefreshCw,
    Minus, CheckCheck, SlidersHorizontal, ChevronDown, ChevronUp, Users2, Package, Layers, ShieldCheck
} from "lucide-react";
import type { RolePermissionDto } from "@/services/admin/AdminRoleService";
import type { ResourceDefinition } from "@/services/admin/AdminResourceService";
import { cn } from "@/lib/utils";

interface PermissionsTableProps {
    permissions: RolePermissionDto[];
    resources: ResourceDefinition[];
    onToggle: (resourceKey: string, action: string) => void;
    onToggleAll?: (resourceKey: string, value: boolean) => void;
    onAllowedFieldsChange?: (resourceKey: string, fields: string[]) => void;
}

// Visual info (icon + color) according to action key. If an action does not come from backend,
// it falls back to the default icon — UI does not break. Adding here for a new action is optional.
const ACTION_META: Record<string, { icon: React.ReactNode; color: string }> = {
    Read:          { icon: <Eye size={12} />,          color: "indigo" },
    Create:        { icon: <Plus size={12} />,         color: "green" },
    Update:        { icon: <Edit size={12} />,         color: "amber" },
    Delete:        { icon: <Trash2 size={12} />,       color: "red" },
    Export:        { icon: <Download size={12} />,     color: "purple" },
    Print:         { icon: <Printer size={12} />,      color: "cyan" },
    Approve:       { icon: <CheckCircle size={12} />,  color: "emerald" },
    Reject:        { icon: <XCircle size={12} />,      color: "rose" },
    Assign:        { icon: <UserPlus size={12} />,     color: "indigo" },
    ChangeStatus:  { icon: <RefreshCw size={12} />,    color: "violet" },
    ManageMembers: { icon: <Users2 size={12} />,       color: "sky" },
    ManageExperts: { icon: <ShieldCheck size={12} />,  color: "sky" },
    ManageModules: { icon: <Package size={12} />,      color: "teal" },
    ManageProjects:{ icon: <Layers size={12} />,       color: "teal" },
    LookupSelect:  { icon: <CheckCheck size={12} />,   color: "slate" },
};

const DASHBOARD_WIDGET_FIELDS = [
    { key: "totalRequests",    label: "Toplam Talep",         group: "metric",  desc: "Genel talep hacmi ve tamamlanma oranını gösteren öne çıkan kart." },
    { key: "activeRequests",   label: "Aktif Talepler",       group: "metric",  desc: "Şu anda açık/devam eden talep sayısı." },
    { key: "completedRequests",label: "Tamamlanan Talepler",  group: "metric",  desc: "Kapanan talep sayısı." },
    { key: "totalMessages",    label: "Toplam Mesaj",         group: "metric",  desc: "Sistemdeki toplam mesaj sayısı." },
    { key: "totalCustomers",   label: "Toplam Müşteri",       group: "metric",  desc: "Kayıtlı müşteri sayısı." },
    { key: "totalCompanies",   label: "Toplam Firma",         group: "metric",  desc: "Kayıtlı firma sayısı." },
    { key: "ticketFlow",       label: "Talep Akış Çubuğu",    group: "widget",  desc: "Talep yaşam döngüsü aşamalarının canlı akış çubuğu." },
    { key: "weeklyChart",      label: "Haftalık Analiz",      group: "widget",  desc: "Haftalık talep analizi grafiği." },
    { key: "recentTable",      label: "Son Talepler Tablosu", group: "widget",  desc: "Filtreleme özellikli son talepler listesi tablosu." },
    { key: "donutChart",       label: "Öncelik Dağılımı",     group: "widget",  desc: "Önceliklere göre talep dağılımı halka grafiği." },
    { key: "systemHealth",     label: "Sistem Sağlığı",       group: "widget",  desc: "Gerçek zamanlı sistem telemetrisi ve sağlık kartı." },
    { key: "quickActions",     label: "Hızlı İşlemler",       group: "widget",  desc: "Sık kullanılan yönetici işlemlerine kısayollar." },
    { key: "statusDistribution",  label: "Statü Dağılımı",         group: "report", desc: "Talep statülerinin adet bazında kırılımı." },
    { key: "moduleDistribution",  label: "Modül Dağılımı",         group: "report", desc: "Modül başına düşen talep sayısı." },
    { key: "companyTopN",         label: "En Aktif Firmalar",      group: "report", desc: "Talep hacmine göre ilk 10 firma." },
    { key: "dailyTrend",          label: "Günlük Trend",           group: "report", desc: "Son 30 gün açılan/kapanan talep trendi." },
    { key: "avgResolutionMinutes",label: "Ort. Çözüm Süresi",      group: "report", desc: "Tamamlanan taleplerin ortalama çözüm süresi." },
    { key: "agentPerformance",    label: "Temsilci Performansı",   group: "report", desc: "Temsilci başına atanan/çözülen ve ortalama süre tablosu." },
    { key: "reopenRate",          label: "Yeniden Açılma Oranı",   group: "report", desc: "Tamamlandıktan sonra tekrar açılan talep oranı." },
];

type WidgetField = (typeof DASHBOARD_WIDGET_FIELDS)[number];

export const PermissionsTable: React.FC<PermissionsTableProps> = ({
    permissions,
    resources,
    onToggle,
    onToggleAll,
    onAllowedFieldsChange,
}) => {
    const [expandedWidgetsKey, setExpandedWidgetsKey] = useState<string | null>(null);
    const [openResourceKeys, setOpenResourceKeys] = useState<Set<string>>(() => new Set(resources.map(r => r.key)));

    const toggleResourceOpen = (key: string) => {
        setOpenResourceKeys(prev => {
            const next = new Set(prev);
            if (next.has(key)) next.delete(key);
            else next.add(key);
            return next;
        });
    };

    const permByKey = (key: string) => permissions.find(p => p.resourceKey === key);

    const parseFields = (json?: string | null): string[] => {
        if (!json) return DASHBOARD_WIDGET_FIELDS.map(f => f.key);
        try {
            const parsed = JSON.parse(json);
            return Array.isArray(parsed) ? parsed : DASHBOARD_WIDGET_FIELDS.map(f => f.key);
        } catch {
            return DASHBOARD_WIDGET_FIELDS.map(f => f.key);
        }
    };

    const handleWidgetToggle = (permKey: string, fieldKey: string, currentFields: string[]) => {
        const next = currentFields.includes(fieldKey)
            ? currentFields.filter(f => f.toLowerCase() !== fieldKey.toLowerCase())
            : [...currentFields, fieldKey];
        onAllowedFieldsChange?.(permKey, next);
    };

    const renderWidgetGrid = (perm: RolePermissionDto, group: "metric" | "widget" | "report") => {
        const key = perm.resourceKey;
        const fields = parseFields(perm.allowedFieldsJson);
        return DASHBOARD_WIDGET_FIELDS.filter(wf => wf.group === group).map(wf => {
            const checked = fields.some(f => f.toLowerCase() === wf.key.toLowerCase());
            return (
                <WidgetFieldButton
                    key={wf.key}
                    wf={wf}
                    checked={checked}
                    onToggle={() => handleWidgetToggle(key, wf.key, fields)}
                />
            );
        });
    };

    return (
        <div className="space-y-2.5 sm:space-y-4">
            {resources.map(def => {
                const perm = permByKey(def.key);
                if (!perm) return null;

                const activeCount = def.actions.filter(a => perm.actions.includes(a.key)).length;
                const totalCount = def.actions.length;
                const allOn = totalCount > 0 && activeCount === totalCount;
                const isDashboard = def.key === "Dashboard";
                const currentAllowed = parseFields(perm.allowedFieldsJson);
                const isWidgetsExpanded = expandedWidgetsKey === def.key;
                const isOpen = openResourceKeys.has(def.key);

                return (
                    <div key={def.key} className="rounded-xl sm:rounded-2xl border border-slate-800 bg-slate-900/60 overflow-hidden">
                        {/* Resource header */}
                        <div 
                            onClick={() => toggleResourceOpen(def.key)}
                            className="flex items-center justify-between gap-2 p-3 sm:px-4 sm:py-3 bg-slate-800/40 border-b border-slate-800/80 cursor-pointer select-none hover:bg-slate-800/60 transition-colors"
                        >
                            <div className="flex items-center gap-2 min-w-0">
                                <ChevronDown size={16} className={cn("text-slate-400 shrink-0 transition-transform duration-200", isOpen ? "rotate-180" : "rotate-0")} />
                                <span className="text-xs sm:text-sm font-black text-slate-100 truncate">{def.displayName}</span>
                                <span className={cn(
                                    "text-[9px] sm:text-[10px] font-bold px-2 py-0.5 rounded-md border shrink-0",
                                    activeCount > 0 
                                        ? "bg-indigo-500/15 border-indigo-500/30 text-indigo-300"
                                        : "bg-slate-800 border-slate-700 text-slate-500"
                                )}>
                                    {activeCount}/{totalCount} yetki
                                </span>
                            </div>
                            <div className="flex items-center gap-1.5 shrink-0" onClick={e => e.stopPropagation()}>
                                {isDashboard && (
                                    <button
                                        type="button"
                                        onClick={() => setExpandedWidgetsKey(isWidgetsExpanded ? null : def.key)}
                                        className="flex items-center gap-1 px-2 py-1 rounded-lg border border-indigo-500/40 bg-indigo-500/10 text-indigo-300 text-[9px] font-black uppercase tracking-wider hover:bg-indigo-500/20 transition-all cursor-pointer"
                                    >
                                        <SlidersHorizontal size={10} />
                                        <span className="hidden sm:inline">Alan İzinleri ({currentAllowed.length})</span>
                                        <span className="sm:hidden">Alan ({currentAllowed.length})</span>
                                        {isWidgetsExpanded ? <ChevronUp size={10} /> : <ChevronDown size={10} />}
                                    </button>
                                )}
                                {onToggleAll && (
                                    <button
                                        type="button"
                                        onClick={() => onToggleAll(def.key, !allOn)}
                                        title={allOn ? "Tüm izinleri kaldır" : "Tüm izinleri işaretle"}
                                        className={cn(
                                            "flex items-center gap-1 px-2 sm:px-2.5 py-1 rounded-lg border text-[9px] font-black uppercase tracking-wider transition-all cursor-pointer",
                                            allOn
                                                ? "bg-indigo-600 border-indigo-600 text-white shadow-xs"
                                                : "bg-slate-900/60 border-slate-700 text-slate-400 hover:border-indigo-400 hover:text-indigo-400"
                                        )}
                                    >
                                        <CheckCheck size={10} />
                                        Tümü
                                    </button>
                                )}
                            </div>
                        </div>

                        {/* Action buttons — dynamic based on resource's own action set */}
                        {isOpen && (
                            <div className="grid grid-cols-2 sm:flex sm:flex-wrap gap-1.5 sm:gap-2 p-2.5 sm:p-4 animate-in fade-in duration-200">
                                {def.actions.map(actDef => {
                                    const meta = ACTION_META[actDef.key] ?? { icon: <CheckCheck size={12} />, color: "slate" };
                                    const active = perm.actions.includes(actDef.key);
                                    return (
                                        <button
                                            key={actDef.key}
                                            type="button"
                                            onClick={() => onToggle(def.key, actDef.key)}
                                            className={cn(
                                                "w-full sm:w-auto sm:flex-none justify-center flex items-center gap-1.5 px-2.5 sm:px-3 py-2 rounded-xl border text-[10px] sm:text-xs font-bold transition-all active:scale-95 cursor-pointer truncate",
                                                active
                                                    ? "bg-indigo-600 border-indigo-600 text-white shadow-xs"
                                                    : "bg-slate-900/60 border-slate-700/80 text-slate-400 hover:border-indigo-400 hover:text-indigo-300"
                                            )}
                                            title={`${def.displayName}: ${actDef.displayName}`}
                                        >
                                            {active ? meta.icon : <Minus size={11} />}
                                            <span className="truncate">{actDef.displayName}</span>
                                        </button>
                                    );
                                })}
                            </div>
                        )}

                        {/* Widget field permissions for Dashboard */}
                        {isDashboard && isWidgetsExpanded && (
                            <div className="bg-slate-950/60 border-t border-indigo-500/30 p-3 sm:p-4 space-y-3 sm:space-y-4">
                                {(["metric", "widget", "report"] as const).map(group => (
                                    <div key={group} className="space-y-2">
                                        <span className="text-[10px] font-extrabold uppercase tracking-widest text-indigo-400 block">
                                            {group === "metric" ? "Metrik Kartları" : group === "widget" ? "Sayfa Bileşenleri" : "Raporlar & Grafikler"}
                                        </span>
                                        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-2">
                                            {renderWidgetGrid(perm, group)}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                );
            })}
        </div>
    );
};

function WidgetFieldButton({
    wf, checked, onToggle,
}: {
    wf: WidgetField;
    checked: boolean;
    onToggle: () => void;
}) {
    return (
        <button
            type="button"
            onClick={onToggle}
            className={cn(
                "w-full flex items-center gap-2 p-2.5 rounded-xl border text-[10px] font-bold transition-all text-left",
                checked ? "bg-indigo-600/20 border-indigo-500 text-indigo-200" : "bg-slate-900/60 border-slate-800 text-slate-400"
            )}
            title={wf.desc}
        >
            <div className={cn(
                "w-3.5 h-3.5 rounded border flex items-center justify-center shrink-0",
                checked ? "bg-indigo-600 border-indigo-500 text-white" : "border-slate-700"
            )}>
                {checked && <CheckCheck size={10} />}
            </div>
            <span className="truncate">{wf.label}</span>
        </button>
    );
}
