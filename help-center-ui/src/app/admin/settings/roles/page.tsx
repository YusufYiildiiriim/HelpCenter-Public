"use client";

import React, { useEffect, useState, useCallback } from "react";
import { ShieldCheck } from "lucide-react";
import type { Role } from "@/services/admin/AdminRoleService";
import { AdminRoleService as RoleService } from "@/services/admin/AdminRoleService";
import { toast } from "sonner";
import { motion } from "framer-motion";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";

import { RoleHeader } from "./components/RoleHeader";
import { RoleSelectorList } from "./components/RoleSelectorList";
import { RoleCard } from "./components/RoleCard";
import RolePermissionsModal from "./components/RolePermissionsModal";
import { RoleAddModal } from "./components/RoleAddModal";
import { AlertModal } from "../../components/AlertModal";

export default function RolesPage() {
    usePageTitle("Settings");
    const { userInfo } = useAuth();
    const { canRead, canCreate, canUpdate, canDelete } = usePermission(userInfo);
    const hasViewPerm = canRead("Roles");

    const [roles, setRoles] = useState<Role[]>([]);
    const [loading, setLoading] = useState(true);
    const [fetchError, setFetchError] = useState<unknown>(null);
    const [isForbidden, setIsForbidden] = useState(false);
    const [selectedRole, setSelectedRole] = useState<Role | null>(null);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [isPermModalOpen, setIsPermModalOpen] = useState(false);
    const [newRole, setNewRole] = useState({ name: "", description: "" });
    const [saving, setSaving] = useState(false);
    const [deletingId, setDeletingId] = useState<number | null>(null);
    const [pendingDeleteId, setPendingDeleteId] = useState<number | null>(null);

    const fetchRoles = useCallback(async () => {
        try {
            setLoading(true);
            const data = await RoleService.getAll();
            setRoles(data);
            if (data.length > 0) setSelectedRole(data[0]);
            setFetchError(null);
            setIsForbidden(false);
        } catch (err) {
            if (isForbiddenError(err)) setIsForbidden(true);
            else setFetchError(err);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional fetch on mount
        fetchRoles();
    }, [fetchRoles]);

    const refreshRoles = async (keepSelectedId?: number) => {
        const data = await RoleService.getAll();
        setRoles(data);
        if (keepSelectedId) {
            const updated = data.find(r => r.id === keepSelectedId);
            setSelectedRole(updated ?? data[0] ?? null);
        }
    };

    const handleCreateRole = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            setSaving(true);
            const created = await RoleService.create(newRole);
            // If create succeeds, there is no need for an extra getAll call — we append
            // the returned created object directly into state. This prevents a misleading
            // "error" toast if the second fetch fails.
            toast.success("Yeni rol oluşturuldu.");
            setIsAddModalOpen(false);
            setNewRole({ name: "", description: "" });
            setRoles(prev => [...prev, created]);
            setSelectedRole(created);
        } catch (err: unknown) {
            toast.error(getApiErrorMessage(err));
        } finally {
            setSaving(false);
        }
    };

    const requestDeleteRole = (id: number) => {
        const target = roles.find(r => r.id === id);
        if (target?.hasUsers) {
            toast.error(`"${target.name}" rolüne bağlı kullanıcılar var. Önce onları başka bir role taşıyın.`);
            return;
        }
        setPendingDeleteId(id);
    };

    const confirmDeleteRole = async () => {
        const id = pendingDeleteId;
        if (id === null) return;
        setPendingDeleteId(null);
        try {
            setDeletingId(id);
            await RoleService.delete(id);
            toast.success("Rol silindi.");
            setRoles(prev => {
                const next = prev.filter(r => r.id !== id);
                if (selectedRole?.id === id) {
                    setSelectedRole(next[0] ?? null);
                }
                return next;
            });
        } catch (err: unknown) {
            toast.error(getApiErrorMessage(err));
        } finally {
            setDeletingId(null);
        }
    };

    const pendingDeleteRole = pendingDeleteId !== null ? roles.find(r => r.id === pendingDeleteId) : null;

    if (!hasViewPerm) {
        return (
            <div className="flex flex-col items-center justify-center min-h-[50vh] gap-3 p-8 text-center bg-slate-900 rounded-3xl border border-slate-800 shadow-sm">
                <ShieldCheck size={32} className="text-rose-400" />
                <h2 className="text-lg font-black text-white">Erişim Yetkisi Sınırlı</h2>
                <p className="text-xs text-slate-400">Rol ve Yetki Yönetimi sayfasını görüntüleme yetkiniz bulunmamaktadır.</p>
            </div>
        );
    }

    return (
        <div className="max-w-[1600px] mx-auto px-3 sm:px-6 md:px-8 py-4 sm:py-8 space-y-6 pb-12 animate-in fade-in slide-in-from-bottom-3 duration-500">
            {/* Header */}
            <motion.div initial={{ opacity: 0, y: -12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.4 }}>
                <RoleHeader onAddClick={() => setIsAddModalOpen(true)} canCreate={canCreate("Roles")} />
            </motion.div>

            {/* Split / Responsive Layout Container */}
            <ApiStateView
                isLoading={loading}
                isForbidden={isForbidden}
                error={fetchError}
                onRetry={fetchRoles}
                variant="page"
                loadingMessage="Roller Yükleniyor..."
            >
                <div className="flex flex-col lg:flex-row items-stretch gap-5 min-h-[500px]">
                    {/* Left/Top Role Selector */}
                    <RoleSelectorList
                        roles={roles}
                        selectedRole={selectedRole}
                        onSelectRole={setSelectedRole}
                        onDeleteRole={requestDeleteRole}
                        deletingId={deletingId}
                        canDelete={canDelete("Roles")}
                    />

                    {/* Main Content */}
                    <div className="flex-1 min-w-0">
                        {selectedRole ? (
                            <RoleCard
                                key={selectedRole.id}
                                role={selectedRole}
                                onManagePermissions={() => setIsPermModalOpen(true)}
                                onDelete={requestDeleteRole}
                                deletingId={deletingId}
                                canDelete={canDelete("Roles")}
                                canUpdate={canUpdate("Roles")}
                            />
                        ) : (
                            <div className="h-full min-h-[300px] flex flex-col items-center justify-center bg-slate-900 rounded-3xl border border-slate-800 border-dashed gap-3 p-8 text-center">
                                <ShieldCheck size={36} className="text-slate-600" />
                                <p className="text-slate-300 font-bold text-sm">Bir rol seçin</p>
                                <p className="text-xs text-slate-500 max-w-xs">Yetki matrisini görüntülemek ve düzenlemek için bir rol seçin.</p>
                            </div>
                        )}
                    </div>
                </div>
            </ApiStateView>

            {/* Add Role Modal */}
            <RoleAddModal
                isOpen={isAddModalOpen}
                onClose={() => setIsAddModalOpen(false)}
                newRole={newRole}
                setNewRole={setNewRole}
                onSubmit={handleCreateRole}
                saving={saving}
            />

            {/* Permissions Modal */}
            {selectedRole && isPermModalOpen && (
                <RolePermissionsModal
                    role={selectedRole}
                    onClose={() => setIsPermModalOpen(false)}
                    onSuccess={() => refreshRoles(selectedRole.id)}
                />
            )}

            {/* Delete Confirm Alert */}
            <AlertModal
                isOpen={pendingDeleteId !== null}
                title="Rolü Sil"
                description={pendingDeleteRole
                    ? `"${pendingDeleteRole.name}" rolünü silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`
                    : "Bu rolü silmek istediğinize emin misiniz?"}
                confirmText="Sil"
                cancelText="Vazgeç"
                variant="danger"
                onConfirm={confirmDeleteRole}
                onCancel={() => setPendingDeleteId(null)}
            />
        </div>
    );
}
