"use client";

import React, { useEffect, useState, useCallback, useMemo } from "react";
import { Sparkles, Edit, Trash2, UserPlus, Lock } from "lucide-react";
import type { Module } from "@/services/admin/AdminModuleService";
import { AdminModuleService as ModuleService } from "@/services/admin/AdminModuleService";
import type { ModuleExpertDto } from "@/services/admin/AdminModuleExpertService";
import { AdminModuleExpertService as ModuleExpertService } from "@/services/admin/AdminModuleExpertService";
import type { User } from "@/services/admin/AdminUserService";
import { toast } from "sonner";
import { AnimatePresence } from "framer-motion";
import type { ColumnDef } from "@tanstack/react-table";

import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";

// Modular Components & Reusable DataTable
import { ModuleHeader } from "./components/ModuleHeader";
import { ModuleModal } from "./components/ModuleModal";
import { ExpertManagementModal } from "./components/ExpertManagementModal";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";
import { AlertModal } from "../components/AlertModal";

export default function ModulesPage() {
  usePageTitle("Modules");
  const { userInfo } = useAuth();
  const { canRead, canCreate, canUpdate, canDelete, canExport, canPrint, canManageExperts } = usePermission(userInfo);
  const hasViewPerm = canRead("Modules");

  const [modules, setModules] = useState<Module[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [loading, setLoading] = useState(false);
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

  const [isEditMode, setIsEditMode] = useState(false);
  const [selectedModuleId, setSelectedModuleId] = useState<number | null>(null);
  const [selectedModulePublicId, setSelectedModulePublicId] = useState<string | null>(null);
  const [isExpertModalOpen, setIsExpertModalOpen] = useState(false);
  const [allUsers, setAllUsers] = useState<User[]>([]);
  const [currentExperts, setCurrentExperts] = useState<ModuleExpertDto[]>([]);
  const [expertLoading, setExpertLoading] = useState(false);
  const [selectedExpertIds, setSelectedExpertIds] = useState<number[]>([]);

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

  // Shim adapting the minimal lookup response (id + full name) to the User shape
  // expected by the modals.
  const mapLookupToUsers = (items: { id: number; name: string }[]): User[] =>
    items.map(item => {
      const parts = item.name.trim().split(/\s+/);
      return {
        id: item.id,
        name: parts[0] ?? "",
        lastName: parts.slice(1).join(" "),
        email: "",
        role: "",
        roles: [],
        userRoles: [],
        isActive: true,
        createdAt: "",
      };
    });

  const fetchModules = useCallback(async (page = pageIndex + 1, size = pageSize, search = searchQuery) => {
    setTableLoading(true);
    try {
      const res = await ModuleService.getPaginated({ pageNumber: page, pageSize: size, search, onlyActive: false });
      setModules(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages);
      setFetchError(null);
      setIsForbidden(false);
    } catch (err) {
      console.error("Error fetching modules:", err);
      if (isForbiddenError(err)) setIsForbidden(true);
      else setFetchError(err);
    } finally {
      setTableLoading(false);
      setInitialLoading(false);
    }
  }, [pageIndex, pageSize, searchQuery]);

  useEffect(() => {
    let isMounted = true;
    Promise.allSettled([
      ModuleService.getPaginated({ pageNumber: 1, pageSize: 10, onlyActive: false }),
      ModuleService.getAvailableExperts()
    ]).then(([modRes, userRes]) => {
      if (!isMounted) return;
      if (modRes.status === "fulfilled") {
        setModules(modRes.value.items);
        setTotalCount(modRes.value.totalCount);
        setTotalPages(modRes.value.totalPages);
      } else {
        console.error("Error fetching modules:", modRes.reason);
        if (isForbiddenError(modRes.reason)) setIsForbidden(true);
        else setFetchError(modRes.reason);
      }
      if (userRes.status === "fulfilled") setAllUsers(mapLookupToUsers(userRes.value));
      else console.error("Error fetching available experts:", userRes.reason);
      setInitialLoading(false);
    });
    return () => { isMounted = false; };
  }, []);

  const handlePageChange = (newPage: number) => {
    setPageIndex(newPage - 1);
    fetchModules(newPage, pageSize, searchQuery);
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPageIndex(0);
    fetchModules(1, newSize, searchQuery);
  };

  const handleSearchChange = (val: string) => {
    setSearchQuery(val);
    setPageIndex(0);
    fetchModules(1, pageSize, val);
  };

  const handleSaveModule = async () => {
    setLoading(true);
    try {
      if (isEditMode && selectedModulePublicId && selectedModuleId) {
        await ModuleService.update(selectedModulePublicId, name, description, isActive, selectedExpertIds);
      } else {
        await ModuleService.create(name, description, isActive, selectedExpertIds);
      }
      toast.success("Modül başarıyla kaydedildi.");
      await fetchModules();
      closeModal();
    } catch (err: unknown) {
      console.error("Error saving module:", err);
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteModule = useCallback((publicId: string) => {
    setAlertConfig({
      isOpen: true,
      title: "Modülü Sil",
      description: "Bu modül sistemden kalıcı olarak silinecektir. Devam etmek istediğinize emin misiniz?",
      onConfirm: async () => {
        try {
          await ModuleService.delete(publicId);
          toast.success("Modül başarıyla silindi.");
          await fetchModules();
        } catch (err: unknown) {
          console.error("Error deleting module:", err);
          toast.error(getApiErrorMessage(err));
        } finally {
          setAlertConfig(prev => ({ ...prev, isOpen: false }));
        }
      }
    });
  }, [fetchModules]);

  const openEditModal = useCallback(async (mod: Module) => {
    setName(mod.name);
    setDescription(mod.description);
    setIsActive(mod.isActive);
    setSelectedModuleId(mod.id);
    setSelectedModulePublicId(mod.publicId);
    
    try {
        const experts = await ModuleExpertService.getByModuleId(mod.id);
        setSelectedExpertIds(experts.map(e => e.userId));
    } catch (err) {
        console.error("Error fetching module experts:", err);
    }

    setIsEditMode(true);
    setIsModalOpen(true);
  }, []);

  const closeModal = () => {
    setIsModalOpen(false);
    setIsEditMode(false);
    setSelectedModuleId(null);
    setSelectedModulePublicId(null);
    setName("");
    setDescription("");
    setIsActive(true);
    setSelectedExpertIds([]);
  };

  const openExpertModal = useCallback(async (mod: Module) => {
    setSelectedModuleId(mod.id);
    setExpertLoading(true);
    setIsExpertModalOpen(true);
    try {
        const [users, experts] = await Promise.all([
            ModuleService.getAvailableExperts(),
            ModuleExpertService.getByModuleId(mod.id)
        ]);
        setAllUsers(mapLookupToUsers(users));
        setCurrentExperts(experts);
    } catch (err) {
        console.error("Error loading expert list:", err);
        toast.error("Uzman listesi yüklenirken hata oluştu.");
    } finally {
        setExpertLoading(false);
    }
  }, []);

  const handleAssignExpert = async (userId: number) => {
    if (!selectedModuleId) return;
    const user = allUsers.find(u => u.id === userId);
    if (!user) return;

    const optimistic: ModuleExpertDto = { id: 0, userId, fullName: `${user.name} ${user.lastName}`, email: user.email };
    setCurrentExperts(prev => [...prev, optimistic]);

    try {
        await ModuleExpertService.assign(selectedModuleId, userId);
        toast.success("Uzman başarıyla atandı.");
        const experts = await ModuleExpertService.getByModuleId(selectedModuleId);
        setCurrentExperts(experts);
    } catch (err) {
        console.error("Error assigning expert:", err);
        setCurrentExperts(prev => prev.filter(e => e.userId !== userId));
        toast.error("Atama sırasında hata oluştu.");
    }
  };

  const handleRemoveExpert = async (expertId: number) => {
    setCurrentExperts(prev => prev.filter(e => e.id !== expertId));

    try {
        await ModuleExpertService.remove(expertId);
        toast.success("Uzman başarıyla kaldırıldı.");
        if (selectedModuleId) {
            const experts = await ModuleExpertService.getByModuleId(selectedModuleId);
            setCurrentExperts(experts);
        }
    } catch (err) {
        console.error("Error removing expert:", err);
        toast.error("Kaldırma sırasında hata oluştu.");
        if (selectedModuleId) {
            const experts = await ModuleExpertService.getByModuleId(selectedModuleId);
            setCurrentExperts(experts);
        }
    }
  };

  // Table Columns Definition for TanStack Table
  const columns = useMemo<ColumnDef<Module>[]>(() => [
    {
      accessorKey: "name",
      header: "Modül Adı",
      cell: (info) => <span className="font-semibold text-slate-50">{info.getValue() as string}</span>,
    },
    {
      accessorKey: "description",
      header: "Açıklama",
      cell: (info) => <span className="block max-w-xs truncate text-slate-300">{(info.getValue() as string) || "-"}</span>,
    },
    {
      accessorKey: "usageCount",
      header: "Kullanım",
      cell: (info) => (
        <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
          {info.getValue() as number} Kayıt
        </span>
      ),
    },
    {
      accessorKey: "isLocked",
      header: "Durum Kilit",
      cell: (info) => (
        info.getValue() ? (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-amber-50 dark:bg-amber-950/60 text-amber-700 dark:text-amber-300 border border-amber-200 dark:border-amber-800">
            <Lock size={12} /> Kilitli
          </span>
        ) : (
          <span className="text-slate-400 text-xs">-</span>
        )
      ),
    },
    {
      accessorKey: "isActive",
      header: "Aktiflik",
      cell: (info) => (
        <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${
          info.getValue() 
            ? "bg-emerald-50 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800" 
            : "bg-rose-50 dark:bg-rose-950/60 text-rose-700 dark:text-rose-300 border-rose-200 dark:border-rose-800"
        }`}>
          {info.getValue() ? "Aktif" : "Pasif"}
        </span>
      ),
    },
    {
      id: "actions",
      header: "İşlemler",
      cell: ({ row }) => {
        const mod = row.original;
        return (
          <div className="flex items-center gap-2">
            {(canManageExperts("Modules") || canUpdate("Modules")) && (
              <button
                onClick={() => openExpertModal(mod)}
                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                title="Uzman Yönetimi"
              >
                <UserPlus size={16} />
              </button>
            )}
            {canUpdate("Modules") && (
              <button
                onClick={() => openEditModal(mod)}
                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                title="Düzenle"
              >
                <Edit size={16} />
              </button>
            )}
            {canDelete("Modules") && (
              <button
                onClick={() => handleDeleteModule(mod.publicId)}
                className="p-1.5 rounded-lg text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/50 transition-colors"
                title="Sil"
              >
                <Trash2 size={16} />
              </button>
            )}
          </div>
        );
      },
    },
  ], [canManageExperts, canUpdate, canDelete, handleDeleteModule, openEditModal, openExpertModal]);

  const excelColumns: ExportColumn<Module>[] = useMemo(() => [
    { header: "Modül Adı", key: "name" },
    { header: "Açıklama", key: "description" },
    { header: "Kullanım Sayısı", key: "usageCount" },
    { header: "Kilit Durumu", key: (m) => (m.isLocked ? "Kilitli" : "Açık") },
    { header: "Aktif mi", key: (m) => (m.isActive ? "Evet" : "Hayır") },
  ], []);

  if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

  return (
    <div className="max-w-350 mx-auto px-3 sm:px-6 md:px-8 py-4 sm:py-8 space-y-6 sm:space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
      
      {/* Header */}
      <ModuleHeader onAddClick={() => setIsModalOpen(true)} canCreate={canCreate("Modules")} />

      {/* Main Content */}
      <ApiStateView
        isLoading={initialLoading}
        isForbidden={isForbidden}
        error={fetchError}
        onRetry={() => fetchModules()}
        variant="page"
        loadingMessage="Modüller Getiriliyor..."
      >
        <DataTable
          data={modules}
          columns={columns}
          excelColumns={excelColumns}
          excelFileName="HelpCenter_Moduller"
          searchPlaceholder="Modül adı veya açıklama ile ara..."
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
          onFetchAllData={() => ModuleService.getAll({ search: searchQuery, onlyActive: false })}
          canExport={canExport("Modules")}
          canPrint={canPrint("Modules")}
        />
      </ApiStateView>

      {/* Footer Decoration */}
      {!initialLoading && modules.length > 0 && (
          <div className="flex items-center justify-center gap-2 py-6 opacity-40">
             <Sparkles size={16} className="text-indigo-500" />
             <p className="text-[10px] font-black uppercase tracking-[0.3em] text-slate-500">
               Departman Yapılandırma Sistemi
             </p>
             <Sparkles size={16} className="text-indigo-500" />
          </div>
      )}

      {/* Main Module Modal (Create/Edit) */}
      <AnimatePresence>
        {isModalOpen && (
          <ModuleModal 
            isEditMode={isEditMode}
            name={name}
            setName={setName}
            description={description}
            setDescription={setDescription}
            isActive={isActive}
            setIsActive={setIsActive}
            allUsers={allUsers}
            selectedExpertIds={selectedExpertIds}
            setSelectedExpertIds={setSelectedExpertIds}
            loading={loading}
            onSave={handleSaveModule}
            onClose={closeModal}
          />
        )}
      </AnimatePresence>

      {/* Expert Management Modal */}
      <AnimatePresence>
        {isExpertModalOpen && (
          <ExpertManagementModal 
            onClose={() => setIsExpertModalOpen(false)}
            expertLoading={expertLoading}
            currentExperts={currentExperts}
            allUsers={allUsers}
            onAssignExpert={handleAssignExpert}
            onRemoveExpert={handleRemoveExpert}
          />
        )}
      </AnimatePresence>

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
