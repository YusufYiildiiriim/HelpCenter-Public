"use client";

import React, { useEffect, useState, useCallback, useMemo } from "react";
import { motion } from "framer-motion";
import { Shield, Pencil, Trash2 } from "lucide-react";
import type { ColumnDef } from "@tanstack/react-table";
import { AdminUserService as UserService, type User } from "@/services/admin/AdminUserService";
import type { Role } from "@/services/admin/AdminRoleService";
import { AdminRoleService as RoleService } from "@/services/admin/AdminRoleService";
import { toast } from "sonner";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";
import { cn } from "@/lib/utils";

// Modular Components
import { UserHeader } from "./components/UserHeader";
import { UserModal, type UserFormData } from "./components/UserModal";
import { RoleAssignmentModal } from "./components/RoleAssignmentModal";
import { AlertModal } from "../components/AlertModal";

export default function AdminUsersPage() {
    usePageTitle("Users");
    const { userInfo } = useAuth();
    const { canCreate, canUpdate, canDelete, canExport, canPrint } = usePermission(userInfo);
    const [users, setUsers] = useState<User[]>([]);
    const [roles, setRoles] = useState<Role[]>([]);
    const [initialLoading, setInitialLoading] = useState(true);
    const [tableLoading, setTableLoading] = useState(false);
    const [fetchError, setFetchError] = useState<unknown>(null);
    const [isForbidden, setIsForbidden] = useState(false);
    
    // Pagination states
    const [pageIndex, setPageIndex] = useState(0);
    const [pageSize, setPageSize] = useState(10);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(1);
    const [searchQuery, setSearchQuery] = useState("");

    // Modal States
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [isRoleModalOpen, setIsRoleModalOpen] = useState(false);

    // Form States
    const [userData, setUserData] = useState<UserFormData>({
        id: 0,
        name: "",
        lastName: "",
        email: "",
        username: "",
        password: "",
        roleIds: [] as number[],
        isActive: true
    });

    const [selectedUserForRoles, setSelectedUserForRoles] = useState<User | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [successMessage, setSuccessMessage] = useState("");

    const [alertConfig, setAlertConfig] = useState<{
        isOpen: boolean;
        title: string;
        description: string;
        onConfirm: () => void;
    }>({
        isOpen: false,
        title: "",
        description: "",
        onConfirm: () => {}
    });

    const fetchUsers = useCallback(async (page = pageIndex + 1, size = pageSize, search = searchQuery) => {
        setTableLoading(true);
        try {
            const res = await UserService.getPaginated({ pageNumber: page, pageSize: size, search });
            setUsers(res.items);
            setTotalCount(res.totalCount);
            setTotalPages(res.totalPages);
            setFetchError(null);
            setIsForbidden(false);
        } catch (err) {
            console.error("Error fetching users:", err);
            if (isForbiddenError(err)) setIsForbidden(true);
            else setFetchError(err);
        } finally {
            setTableLoading(false);
            setInitialLoading(false);
        }
    }, [pageIndex, pageSize, searchQuery]);

    const fetchInitialData = useCallback(async () => {
        await fetchUsers(pageIndex + 1, pageSize, searchQuery);
    }, [fetchUsers, pageIndex, pageSize, searchQuery]);

    useEffect(() => {
        let isMounted = true;
        Promise.allSettled([
            UserService.getPaginated({ pageNumber: 1, pageSize: 10 }),
            RoleService.getAll()
        ]).then(([usersRes, rolesRes]) => {
            if (!isMounted) return;
            if (usersRes.status === "fulfilled") {
                setUsers(usersRes.value.items);
                setTotalCount(usersRes.value.totalCount);
                setTotalPages(usersRes.value.totalPages);
            } else {
                console.error("Error fetching users:", usersRes.reason);
                if (isForbiddenError(usersRes.reason)) setIsForbidden(true);
                else setFetchError(usersRes.reason);
            }
            if (rolesRes.status === "fulfilled") setRoles(rolesRes.value);
            else console.error("Error fetching roles:", rolesRes.reason);
            setInitialLoading(false);
        });
        return () => { isMounted = false; };
    }, []);

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage - 1);
        fetchUsers(newPage, pageSize, searchQuery);
    };

    const handlePageSizeChange = (newSize: number) => {
        setPageSize(newSize);
        setPageIndex(0);
        fetchUsers(1, newSize, searchQuery);
    };

    const handleSearchChange = (val: string) => {
        setSearchQuery(val);
        setPageIndex(0);
        fetchUsers(1, pageSize, val);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const email = userData.email.trim().toLowerCase();
        const duplicate = users.find(
            (u) => u.email.trim().toLowerCase() === email && u.id !== userData.id
        );
        if (duplicate) {
            toast.error("Bu kullanıcı zaten kayıtlı.");
            return;
        }
        const normalizedData = { ...userData, email };
        setSubmitting(true);
        try {
            if (userData.id > 0) {
                await UserService.update(userData.id, normalizedData);
                setSuccessMessage("Kullanıcı başarıyla güncellendi.");
            } else {
                await UserService.create({ ...normalizedData, password: normalizedData.password || "" });
                setSuccessMessage("Kullanıcı başarıyla oluşturuldu.");
            }
            
            setTimeout(() => {
                setIsAddModalOpen(false);
                setIsEditModalOpen(false);
                setSuccessMessage("");
                setUserData({ id: 0, name: "", lastName: "", email: "", username: "", password: "", roleIds: [], isActive: true });
                fetchInitialData();
            }, 1800);
        } catch (err: unknown) {
            console.error("Error saving user:", err);
            toast.error(getApiErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    const handleDeleteUser = useCallback((id: number) => {
        setAlertConfig({
            isOpen: true,
            title: "Kullanıcıyı Sil",
            description: "Bu kullanıcı sistemden kalıcı olarak silinecektir. Devam etmek istediğinize emin misiniz?",
            onConfirm: async () => {
                try {
                    await UserService.delete(id);
                    toast.success("Kullanıcı silindi.");
                    fetchInitialData();
                } catch (err) {
                    console.error("Error deleting user:", err);
                    toast.error("Kullanıcı silinirken bir hata oluştu.");
                } finally {
                    setAlertConfig(prev => ({ ...prev, isOpen: false }));
                }
            }
        });
    }, [fetchInitialData]);

    const openEditModal = useCallback((user: User) => {
        setUserData({
            id: user.id,
            name: user.name,
            lastName: user.lastName,
            email: user.email,
            username: user.username || "",
            password: "",
            roleIds: user.userRoles?.map((ur) => ur.roleId) || [],
            isActive: user.isActive
        });
        setIsEditModalOpen(true);
    }, []);

    const openAddModal = () => {
        setUserData({ id: 0, name: "", lastName: "", email: "", username: "", password: "", roleIds: [], isActive: true });
        setIsAddModalOpen(true);
    };

    const openRoleModal = useCallback((user: User) => {
        setSelectedUserForRoles(user);
        setIsRoleModalOpen(true);
    }, []);

    const handleSaveRoles = async (roleIds: number[]) => {
        if (!selectedUserForRoles) return;
        await UserService.update(selectedUserForRoles.id, {
            id: selectedUserForRoles.id,
            name: selectedUserForRoles.name,
            lastName: selectedUserForRoles.lastName,
            email: selectedUserForRoles.email,
            username: selectedUserForRoles.username || "",
            password: "",
            roleIds,
            isActive: selectedUserForRoles.isActive
        });
        toast.success(`${selectedUserForRoles.name} ${selectedUserForRoles.lastName} için roller güncellendi.`);
        await fetchInitialData();
    };

    const canUpdateUsers = canUpdate("Users");
    const canDeleteUsers = canDelete("Users");

    const userColumns = useMemo<ColumnDef<User>[]>(() => [
        {
            accessorKey: "name",
            header: "Kullanıcı",
            cell: (info) => {
                const user = info.row.original;
                return (
                    <div className="flex items-center gap-3.5 min-w-0">
                        <div className={`w-10 h-10 rounded-2xl flex items-center justify-center font-black text-xs text-white shrink-0 shadow-md ${
                            user.isActive ? "bg-gradient-to-br from-indigo-500 to-violet-600 shadow-indigo-500/20" : "bg-slate-800 text-slate-400"
                        }`}>
                            {user.name.charAt(0)}{user.lastName?.charAt(0) || ""}
                        </div>
                        <div className="min-w-0">
                            <p className="text-sm font-bold text-white tracking-tight truncate">{user.name} {user.lastName}</p>
                            {user.createdAt && (
                                <p className="text-[10px] font-mono text-slate-400 mt-0.5">
                                    {new Date(user.createdAt).toLocaleDateString("tr-TR")}
                                </p>
                            )}
                        </div>
                    </div>
                );
            },
        },
        {
            accessorKey: "email",
            header: "E-posta",
            cell: (info) => <span className="text-slate-300 font-medium truncate block max-w-xs">{info.getValue() as string}</span>,
        },
        {
            accessorKey: "userRoles",
            header: "Roller",
            cell: (info) => {
                const roles = (info.getValue() as { id: number; roleName: string }[]) || [];
                if (roles.length === 0) return <span className="text-[10px] text-slate-500 italic">Rol atanmamış</span>;
                return (
                    <div className="flex flex-wrap gap-1.5 max-w-[240px]">
                        {roles.slice(0, 3).map((ur) => (
                            <span key={ur.id} className="px-2 py-0.5 bg-indigo-500/15 text-indigo-300 rounded-lg text-[9px] font-extrabold uppercase tracking-wider border border-indigo-500/30">
                                {ur.roleName}
                            </span>
                        ))}
                        {roles.length > 3 && (
                            <span className="px-2 py-0.5 bg-slate-800 text-slate-400 rounded-lg text-[9px] font-bold border border-slate-700">
                                +{roles.length - 3}
                            </span>
                        )}
                    </div>
                );
            },
        },
        {
            accessorKey: "isActive",
            header: "Durum",
            cell: (info) => {
                const active = info.getValue() as boolean;
                return (
                    <span className={cn(
                        "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-bold border",
                        active 
                            ? "bg-emerald-500/15 text-emerald-400 border-emerald-500/30" 
                            : "bg-slate-800 text-slate-400 border-slate-700"
                    )}>
                        <span className={`w-1.5 h-1.5 rounded-full ${active ? "bg-emerald-400 shadow-sm shadow-emerald-400/50" : "bg-slate-500"}`} />
                        {active ? "Aktif" : "Pasif"}
                    </span>
                );
            },
        },
        {
            id: "actions",
            header: "İşlemler",
            cell: ({ row }) => {
                const user = row.original;
                return (
                    <div className="flex items-center gap-2">
                        {canUpdateUsers && (
                            <>
                                <button onClick={() => openEditModal(user)} className="p-1.5 rounded-lg text-slate-400 hover:text-indigo-400 hover:bg-slate-800 transition-colors" title="Düzenle">
                                    <Pencil size={16} />
                                </button>
                                <button onClick={() => openRoleModal(user)} className="p-1.5 rounded-lg text-slate-400 hover:text-purple-400 hover:bg-slate-800 transition-colors" title="Rol Yönetimi">
                                    <Shield size={16} />
                                </button>
                            </>
                        )}
                        {canDeleteUsers && (
                            <button onClick={() => handleDeleteUser(user.id)} className="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-slate-800 transition-colors" title="Sil">
                                <Trash2 size={16} />
                            </button>
                        )}
                    </div>
                );
            },
        },
    ], [canUpdateUsers, canDeleteUsers, openEditModal, openRoleModal, handleDeleteUser]);

    const excelUserColumns: ExportColumn<User>[] = useMemo(() => [
        { header: "Ad", key: "name" },
        { header: "Soyad", key: "lastName" },
        { header: "E-posta", key: "email" },
        { header: "Roller", key: (u: User) => (u.userRoles?.map((ur) => ur.roleName) || []).join(", ") },
        { header: "Durum", key: (u: User) => (u.isActive ? "Aktif" : "Pasif") },
        { header: "Oluşturulma", key: (u: User) => (u.createdAt ? new Date(u.createdAt).toLocaleDateString("tr-TR") : "-") },
    ], []);

    return (
        <div className="space-y-6 sm:space-y-8 max-w-[1600px] mx-auto px-3 sm:px-6 md:px-8 py-4 sm:py-8 pb-12 animate-in fade-in slide-in-from-bottom-3 duration-500">
            {/* Header Section */}
            <motion.div initial={{ opacity: 0, y: -12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.4 }}>
                <UserHeader 
                    canCreate={canCreate("Users")} 
                    onAddClick={openAddModal} 
                />
            </motion.div>

            {/* Content Section */}
            <ApiStateView
                isLoading={initialLoading}
                isForbidden={isForbidden}
                error={fetchError}
                onRetry={() => fetchUsers()}
                variant="page"
                loadingMessage="Kullanıcılar Getiriliyor..."
            >
                <DataTable
                    data={users}
                    columns={userColumns}
                    excelColumns={excelUserColumns}
                    excelFileName="HelpCenter_Kullanicilar"
                    manualPagination={true}
                    pageIndex={pageIndex}
                    pageSize={pageSize}
                    pageCount={totalPages}
                    totalCount={totalCount}
                    onPageChange={handlePageChange}
                    onPageSizeChange={handlePageSizeChange}
                    onSearchChange={handleSearchChange}
                    searchValue={searchQuery}
                    isLoading={tableLoading}
                    onFetchAllData={() => UserService.getAll({ search: searchQuery })}
                    searchPlaceholder="İsim, soyisim veya e-posta ile ara..."
                    canExport={canExport("Users")}
                    canPrint={canPrint("Users")}
                />
            </ApiStateView>

            {/* Add/Edit User Modal */}
            <UserModal 
                isOpen={isAddModalOpen || isEditModalOpen}
                onClose={() => {
                    setIsAddModalOpen(false);
                    setIsEditModalOpen(false);
                }}
                isEditMode={isEditModalOpen}
                userData={userData}
                setUserData={setUserData}
                onSubmit={handleSubmit}
                submitting={submitting}
                successMessage={successMessage}
            />

            {/* Role Assignment Modal */}
            <RoleAssignmentModal
                isOpen={isRoleModalOpen}
                onClose={() => {
                    setIsRoleModalOpen(false);
                    setSelectedUserForRoles(null);
                }}
                userName={selectedUserForRoles ? `${selectedUserForRoles.name} ${selectedUserForRoles.lastName}` : ""}
                userRoles={selectedUserForRoles?.userRoles || []}
                allRoles={roles}
                onSave={handleSaveRoles}
            />

            {/* Delete Confirm Alert */}
            <AlertModal
                isOpen={alertConfig.isOpen}
                title={alertConfig.title}
                description={alertConfig.description}
                onConfirm={alertConfig.onConfirm}
                onCancel={() => setAlertConfig(prev => ({ ...prev, isOpen: false }))}
            />
        </div>
    );
}
