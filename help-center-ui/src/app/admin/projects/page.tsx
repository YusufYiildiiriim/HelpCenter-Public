"use client";

import React, { useEffect, useState, useCallback, useMemo } from "react";
import { Sparkles, Edit, Trash2, UserPlus } from "lucide-react";
import type { Project } from "@/services/admin/AdminProjectService";
import { AdminProjectService as ProjectService } from "@/services/admin/AdminProjectService";
import type { ProjectUserDto } from "@/services/admin/AdminUserProjectService";
import { AdminUserProjectService as UserProjectService } from "@/services/admin/AdminUserProjectService";
import type { User } from "@/services/admin/AdminUserService";
import type { Module } from "@/services/admin/AdminModuleService";
import { toast } from "sonner";
import { AnimatePresence } from "framer-motion";
import type { ColumnDef } from "@tanstack/react-table";

import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";

// Modular Components & Reusable DataTable
import { AlertModal } from "../components/AlertModal";
import { ProjectHeader } from "./components/ProjectHeader";
import { ProjectModal } from "./components/ProjectModal";
import { UserManagementModal } from "./components/UserManagementModal";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";

export default function ProjectsPage() {
  usePageTitle("Projects");
  const { userInfo } = useAuth();
  const { canRead, canCreate, canUpdate, canDelete, canExport, canPrint, canManageMembers } = usePermission(userInfo);
  const hasViewPerm = canRead("Projects");

  const [projects, setProjects] = useState<Project[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [fetchError, setFetchError] = useState<unknown>(null);
  const [isForbidden, setIsForbidden] = useState(false);

  // State: Alert Modal
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

  // Pagination states
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [searchQuery, setSearchQuery] = useState("");

  const [isEditMode, setIsEditMode] = useState(false);
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(null);
  const [selectedProjectPublicId, setSelectedProjectPublicId] = useState<string | null>(null);
  const [isUserModalOpen, setIsUserModalOpen] = useState(false);
  const [allUsers, setAllUsers] = useState<User[]>([]);
  const [currentUsers, setCurrentUsers] = useState<ProjectUserDto[]>([]);
  const [userLoading, setUserLoading] = useState(false);
  const [selectedUserIds, setSelectedUserIds] = useState<number[]>([]);
  const [allModules, setAllModules] = useState<Module[]>([]);
  const [selectedModuleIds, setSelectedModuleIds] = useState<number[]>([]);

  // Lookup shims — adapt minimal (id + name) responses to the full shape expected by modals.
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

  const mapLookupToModules = (items: { id: number; name: string }[]): Module[] =>
    items.map(item => ({
      id: item.id,
      publicId: "",
      name: item.name,
      description: "",
      usageCount: 0,
      isActive: true,
      isLocked: false,
    }));

  const fetchProjects = useCallback(async (page = pageIndex + 1, size = pageSize, search = searchQuery) => {
    setTableLoading(true);
    try {
      const res = await ProjectService.getPaginated({ pageNumber: page, pageSize: size, search });
      setProjects(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages);
      setFetchError(null);
      setIsForbidden(false);
    } catch (err) {
      console.error("Error fetching projects:", err);
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
      ProjectService.getPaginated({ pageNumber: 1, pageSize: 10 }),
      ProjectService.getAvailableUsers(),
      ProjectService.getAvailableModules()
    ]).then(([projRes, userRes, modRes]) => {
      if (!isMounted) return;
      if (projRes.status === "fulfilled") {
        setProjects(projRes.value.items);
        setTotalCount(projRes.value.totalCount);
        setTotalPages(projRes.value.totalPages);
      } else {
        console.error("Error fetching projects:", projRes.reason);
        if (isForbiddenError(projRes.reason)) setIsForbidden(true);
        else setFetchError(projRes.reason);
      }
      if (userRes.status === "fulfilled") setAllUsers(mapLookupToUsers(userRes.value));
      else console.error("Error fetching available users:", userRes.reason);
      if (modRes.status === "fulfilled") setAllModules(mapLookupToModules(modRes.value));
      else console.error("Error fetching available modules:", modRes.reason);
      setInitialLoading(false);
    });
    return () => { isMounted = false; };
  }, []);

  const handlePageChange = (newPage: number) => {
    setPageIndex(newPage - 1);
    fetchProjects(newPage, pageSize, searchQuery);
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPageIndex(0);
    fetchProjects(1, newSize, searchQuery);
  };

  const handleSearchChange = (val: string) => {
    setSearchQuery(val);
    setPageIndex(0);
    fetchProjects(1, pageSize, val);
  };

  const handleSaveProject = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (isEditMode && selectedProjectPublicId && selectedProjectId) {
        await ProjectService.update(selectedProjectPublicId, name, description, isActive, selectedUserIds, selectedModuleIds);
      } else {
        await ProjectService.create(name, description, isActive, selectedUserIds, selectedModuleIds);
      }
      toast.success("Proje başarıyla kaydedildi.");
      await fetchProjects();
      closeModal();
    } catch (err: unknown) {
      console.error("Error saving project:", err);
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteProject = useCallback((publicId: string) => {
    setAlertConfig({
      isOpen: true,
      title: "Projeyi Sil",
      description: "Bu proje sistemden kalıcı olarak silinecektir. Devam etmek istediğinize emin misiniz?",
      onConfirm: async () => {
        try {
          await ProjectService.delete(publicId);
          toast.success("Proje başarıyla silindi.");
          await fetchProjects();
        } catch (err: unknown) {
          console.error("Error deleting project:", err);
          toast.error(getApiErrorMessage(err));
        } finally {
          setAlertConfig(prev => ({ ...prev, isOpen: false }));
        }
      }
    });
  }, [fetchProjects]);

  const openEditModal = useCallback(async (project: Project) => {
    setName(project.name);
    setDescription(project.description);
    setIsActive(project.isActive);
    setSelectedProjectId(project.id);
    setSelectedProjectPublicId(project.publicId);
    setSelectedModuleIds(project.moduleIds || []);

    try {
      const users = await UserProjectService.getByProjectId(project.id);
      setSelectedUserIds(users.map(u => u.userId));
    } catch (err) {
      console.error("Error fetching project details:", err);
    }

    setIsEditMode(true);
    setIsModalOpen(true);
  }, []);

  const closeModal = () => {
    setIsModalOpen(false);
    setIsEditMode(false);
    setSelectedProjectId(null);
    setSelectedProjectPublicId(null);
    setName("");
    setDescription("");
    setIsActive(true);
    setSelectedUserIds([]);
    setSelectedModuleIds([]);
  };

  const openUserModal = useCallback(async (project: Project) => {
    setSelectedProjectId(project.id);
    setUserLoading(true);
    setIsUserModalOpen(true);
    try {
        const [users, projectUsers] = await Promise.all([
            ProjectService.getAvailableUsers(),
            UserProjectService.getByProjectId(project.id)
        ]);
        setAllUsers(mapLookupToUsers(users));
        setCurrentUsers(projectUsers);
    } catch (err) {
        console.error("Error loading user list:", err);
        toast.error("Kullanıcı listesi yüklenirken hata oluştu.");
    } finally {
        setUserLoading(false);
    }
  }, []);

  const handleAssignUser = async (userId: number) => {
    if (!selectedProjectId) return;
    const user = allUsers.find(u => u.id === userId);
    if (!user) return;

    const optimistic: ProjectUserDto = { id: 0, userId, fullName: `${user.name} ${user.lastName}`, email: user.email };
    setCurrentUsers(prev => [...prev, optimistic]);

    try {
        await UserProjectService.assign(selectedProjectId, userId);
        toast.success("Kullanıcı başarıyla atandı.");
        const users = await UserProjectService.getByProjectId(selectedProjectId);
        setCurrentUsers(users);
        await fetchProjects();
    } catch (err) {
        console.error("Error assigning user:", err);
        setCurrentUsers(prev => prev.filter(u => u.userId !== userId));
        toast.error("Atama sırasında hata oluştu.");
    }
  };

  const handleRemoveUser = async (id: number) => {
    setCurrentUsers(prev => prev.filter(u => u.id !== id));

    try {
        await UserProjectService.remove(id);
        toast.success("Kullanıcı başarıyla kaldırıldı.");
        if (selectedProjectId) {
            const users = await UserProjectService.getByProjectId(selectedProjectId);
            setCurrentUsers(users);
        }
        await fetchProjects();
    } catch (err) {
        console.error("Error removing user:", err);
        toast.error("Kaldırma sırasında hata oluştu.");
        if (selectedProjectId) {
            const users = await UserProjectService.getByProjectId(selectedProjectId);
            setCurrentUsers(users);
        }
    }
  };

  const columns = useMemo<ColumnDef<Project>[]>(() => [
    {
      accessorKey: "name",
      header: "Proje Adı",
      cell: (info) => <span className="font-semibold text-slate-50">{info.getValue() as string}</span>,
    },
    {
      accessorKey: "description",
      header: "Açıklama",
      cell: (info) => <span className="block max-w-xs truncate text-slate-300">{(info.getValue() as string) || "-"}</span>,
    },
    {
      accessorKey: "companyCount",
      header: "Bağlı Firma",
      cell: (info) => (
        <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-blue-50 dark:bg-blue-950/60 text-blue-700 dark:text-blue-300 border border-blue-200 dark:border-blue-800">
          {info.getValue() as number} Firma
        </span>
      ),
    },
    {
      accessorKey: "userCount",
      header: "Atanmış Kullanıcı",
      cell: (info) => (
        <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
          {info.getValue() as number} Kullanıcı
        </span>
      ),
    },
    {
      accessorKey: "isActive",
      header: "Durum",
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
        const proj = row.original;
        return (
          <div className="flex items-center gap-2">
            {(canManageMembers("Projects") || canUpdate("Projects")) && (
              <button
                onClick={() => openUserModal(proj)}
                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                title="Kullanıcı Yönetimi"
              >
                <UserPlus size={16} />
              </button>
            )}
            {canUpdate("Projects") && (
              <button
                onClick={() => openEditModal(proj)}
                className="p-1.5 rounded-lg text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-950/50 transition-colors"
                title="Düzenle"
              >
                <Edit size={16} />
              </button>
            )}
            {canDelete("Projects") && (
              <button
                onClick={() => handleDeleteProject(proj.publicId)}
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
  ], [canManageMembers, canUpdate, canDelete, handleDeleteProject, openEditModal, openUserModal]);

  const excelColumns: ExportColumn<Project>[] = useMemo(() => [
    { header: "Proje Adı", key: "name" },
    { header: "Açıklama", key: "description" },
    { header: "Firma Sayısı", key: "companyCount" },
    { header: "Kullanıcı Sayısı", key: "userCount" },
    { header: "Durum", key: (p) => (p.isActive ? "Aktif" : "Pasif") },
  ], []);

  if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

  return (
    <div className="max-w-[1400px] mx-auto px-3 sm:px-6 md:px-8 py-4 sm:py-8 space-y-6 sm:space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">

      {/* Header */}
      <ProjectHeader onAddClick={() => setIsModalOpen(true)} canCreate={canCreate("Projects")} />

      {/* Main Content */}
      <ApiStateView
        isLoading={initialLoading}
        isForbidden={isForbidden}
        error={fetchError}
        onRetry={() => fetchProjects()}
        variant="page"
        loadingMessage="Projeler Getiriliyor..."
      >
        <DataTable
          data={projects}
          columns={columns}
          excelColumns={excelColumns}
          excelFileName="HelpCenter_Projeler"
          searchPlaceholder="Proje adı veya açıklama ile ara..."
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
          onFetchAllData={() => ProjectService.getAll({ search: searchQuery })}
          canExport={canExport("Projects")}
          canPrint={canPrint("Projects")}
        />
      </ApiStateView>

      {/* Footer Decoration */}
      {!initialLoading && projects.length > 0 && (
          <div className="flex items-center justify-center gap-2 py-6 opacity-40">
             <Sparkles size={16} className="text-indigo-500" />
             <p className="text-[10px] font-black uppercase tracking-[0.3em] text-slate-500">
               Çoklu Proje Yapılandırma Sistemi
             </p>
             <Sparkles size={16} className="text-indigo-500" />
          </div>
      )}

      {/* Main Project Modal (Create/Edit) */}
      <AnimatePresence>
        {isModalOpen && (
          <ProjectModal
            isEditMode={isEditMode}
            name={name}
            setName={setName}
            description={description}
            setDescription={setDescription}
            isActive={isActive}
            setIsActive={setIsActive}
            allUsers={allUsers}
            selectedUserIds={selectedUserIds}
            setSelectedUserIds={setSelectedUserIds}
            allModules={allModules}
            selectedModuleIds={selectedModuleIds}
            setSelectedModuleIds={setSelectedModuleIds}
            loading={loading}
            onSubmit={handleSaveProject}
            onClose={closeModal}
          />
        )}
      </AnimatePresence>

      {/* User Management Modal */}
      <AnimatePresence>
        {isUserModalOpen && (
          <UserManagementModal
            onClose={() => setIsUserModalOpen(false)}
            userLoading={userLoading}
            currentUsers={currentUsers}
            allUsers={allUsers}
            onAssignUser={handleAssignUser}
            onRemoveUser={handleRemoveUser}
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
