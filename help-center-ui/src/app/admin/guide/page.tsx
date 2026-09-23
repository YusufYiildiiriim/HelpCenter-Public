"use client";

import React, { useEffect, useState, useCallback, useMemo } from "react";
import { FileText, Pencil, Trash2 } from "lucide-react";
import type { Guide, GuideDocument } from "@/services/admin/AdminGuideService";
import { AdminGuideService as GuideService } from "@/services/admin/AdminGuideService";
import type { Module } from "@/services/admin/AdminModuleService";
import { AdminModuleService as ModuleService } from "@/services/admin/AdminModuleService";
import { toast } from "sonner";
import { AnimatePresence } from "framer-motion";
import type { ColumnDef } from "@tanstack/react-table";

import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";
import { cn } from "@/lib/utils";

// Modular Components
import { GuideHeader } from "./components/GuideHeader";
import { GuideModal } from "./components/GuideModal";
import { AlertModal } from "../components/AlertModal";

export default function GuidePage() {
  usePageTitle("Guides");
  const { userInfo } = useAuth();
  const { canRead, canCreate, canUpdate, canDelete, canExport, canPrint } = usePermission(userInfo);
  const hasViewPerm = canRead("Guide");

  const [guides, setGuides] = useState<Guide[]>([]);
  const [modules, setModules] = useState<Module[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [initialLoading, setInitialLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [fetchError, setFetchError] = useState<unknown>(null);
  const [isForbidden, setIsForbidden] = useState(false);

  // Pagination states
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [searchQuery, setSearchQuery] = useState("");

  const [newGuide, setNewGuide] = useState({
    title: "",
    description: "",
    module: "",
    previousGuideId: null as number | null,
    youtubeUrl: "",
    files: null as FileList | null,
    existingDocuments: [] as GuideDocument[],
    isActive: true,
    isPublic: false,
  });

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

  const fetchGuides = useCallback(async (page = pageIndex + 1, size = pageSize, search = searchQuery) => {
    setTableLoading(true);
    try {
      const res = await GuideService.getPaginated({ pageNumber: page, pageSize: size, search });
      setGuides(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages);
      setFetchError(null);
      setIsForbidden(false);
    } catch (err) {
      console.error("Error fetching guides:", err);
      if (isForbiddenError(err)) setIsForbidden(true);
      else setFetchError(err);
    } finally {
      setTableLoading(false);
      setInitialLoading(false);
    }
  }, [pageIndex, pageSize, searchQuery]);

  const fetchInitialData = useCallback(async () => {
    await fetchGuides(pageIndex + 1, pageSize, searchQuery);
  }, [fetchGuides, pageIndex, pageSize, searchQuery]);

  useEffect(() => {
    let isMounted = true;
    Promise.allSettled([
      GuideService.getPaginated({ pageNumber: 1, pageSize: 10 }),
      ModuleService.getAll()
    ]).then(([guideRes, moduleRes]) => {
      if (!isMounted) return;
      if (guideRes.status === "fulfilled") {
        setGuides(guideRes.value.items);
        setTotalCount(guideRes.value.totalCount);
        setTotalPages(guideRes.value.totalPages);
      } else {
        console.error("Error fetching guides:", guideRes.reason);
        if (isForbiddenError(guideRes.reason)) setIsForbidden(true);
        else setFetchError(guideRes.reason);
      }
      if (moduleRes.status === "fulfilled") setModules(moduleRes.value);
      else console.error("Error fetching modules:", moduleRes.reason);
      setInitialLoading(false);
    });
    return () => { isMounted = false; };
  }, []);

  const handlePageChange = (newPage: number) => {
    setPageIndex(newPage - 1);
    fetchGuides(newPage, pageSize, searchQuery);
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPageIndex(0);
    fetchGuides(1, newSize, searchQuery);
  };

  const handleSearchChange = (val: string) => {
    setSearchQuery(val);
    setPageIndex(0);
    fetchGuides(1, pageSize, val);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newGuide.description || newGuide.description === "<p>&nbsp;</p>") {
      toast.error("Lütfen bir açıklama girin.");
      return;
    }
    
    const moduleObj = modules.find(m => m.name === newGuide.module);
    if (!moduleObj) {
      toast.error("Geçerli bir modül seçmelisiniz.");
      return;
    }

    const payload = {
      title: newGuide.title,
      description: newGuide.description,
      module: moduleObj.name,
      previousGuideId: newGuide.previousGuideId,
      youtubeUrl: newGuide.youtubeUrl,
      isActive: newGuide.isActive,
      isPublic: newGuide.isPublic,
      files: newGuide.files ? Array.from(newGuide.files) : []
    };

    setActionLoading(true);
    try {
      if (isEditMode && editingId) {
        await GuideService.update({ ...payload, id: editingId });
        toast.success("Rehber başarıyla güncellendi.");
      } else {
        await GuideService.create(payload);
        toast.success("Rehber başarıyla eklendi.");
      }
      await fetchInitialData();
      closeModal();
    } catch (err: unknown) {
      console.error("Error saving guide:", err);
      toast.error(getApiErrorMessage(err));
    } finally {
      setActionLoading(false);
    }
  };

  const openEditModal = useCallback((guide: Guide) => {
    setIsEditMode(true);
    setEditingId(guide.id);
    setNewGuide({
      title: guide.title,
      description: guide.description,
      module: guide.module,
      previousGuideId: guide.previousGuideId || null,
      youtubeUrl: guide.youtubeUrl || "",
      files: null,
      existingDocuments: guide.documents || [],
      isActive: guide.isActive,
      isPublic: guide.isPublic,
    });
    setIsModalOpen(true);
  }, []);

  const closeModal = () => {
    setIsModalOpen(false);
    setIsEditMode(false);
    setEditingId(null);
    setNewGuide({
      title: "",
      description: "",
      module: modules[0]?.name || "",
      previousGuideId: null,
      youtubeUrl: "",
      files: null,
      existingDocuments: [],
      isActive: true,
      isPublic: false,
    });
  };

  const handleDelete = useCallback(async (id: number) => {
    setAlertConfig({
      isOpen: true,
      title: "Rehberi Sil",
      description: "Bu rehber dökümantasyondan kalıcı olarak kaldırılacaktır. Devam etmek istediğinize emin misiniz?",
      onConfirm: async () => {
        try {
          await GuideService.delete(id);
          toast.success("Rehber başarıyla silindi.");
          await fetchInitialData();
          setAlertConfig(prev => ({ ...prev, isOpen: false }));
        } catch (err) {
          console.error("Error deleting guide:", err);
          toast.error("Silme işlemi sırasında bir hata oluştu.");
        }
      }
    });
  }, [fetchInitialData]);

  const stripHtml = (html: string) => html.replace(/<[^>]*>/g, "");

  const columns = useMemo<ColumnDef<Guide>[]>(() => [
    {
      accessorKey: "title",
      header: "Başlık",
      cell: (info) => (
        <div className="flex items-center gap-3 min-w-0">
          <span className="w-8 h-8 rounded-lg bg-linear-to-br from-indigo-500 to-violet-600 flex items-center justify-center text-white shrink-0">
            <FileText size={14} />
          </span>
          <div className="min-w-0">
            <span className="block truncate font-semibold text-slate-50">{info.getValue() as string}</span>
            <span className="block max-w-xs truncate text-xs text-slate-400">
              {stripHtml(info.row.original.description)}
            </span>
          </div>
        </div>
      ),
    },
    {
      accessorKey: "module",
      header: "Kategori (Modül)",
      cell: (info) => (
        <span className="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold bg-indigo-50 dark:bg-indigo-950/50 text-indigo-600 dark:text-indigo-400 border border-indigo-200/50 dark:border-indigo-800/50">
          {info.getValue() as string}
        </span>
      ),
    },
    {
      accessorKey: "isPublic",
      header: "Erişim",
      cell: (info) => {
        const isPub = info.getValue() as boolean;
        return (
          <span className={cn(
            "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-bold border",
            isPub 
              ? "bg-emerald-50 dark:bg-emerald-950/50 text-emerald-600 dark:text-emerald-400 border-emerald-200/50 dark:border-emerald-800/50" 
              : "bg-amber-50 dark:bg-amber-950/50 text-amber-600 dark:text-amber-400 border-amber-200/50 dark:border-amber-800/50"
          )}>
            {isPub ? "Herkese Açık" : "Özel (Login Şart)"}
          </span>
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
            "inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-bold",
            active ? "text-emerald-500 bg-emerald-50 dark:bg-emerald-950/40" : "text-slate-400 bg-slate-100 dark:bg-slate-800"
          )}>
            <span className={cn("w-1.5 h-1.5 rounded-full", active ? "bg-emerald-500" : "bg-slate-400")} />
            {active ? "Aktif" : "Pasif"}
          </span>
        );
      },
    },
    {
      accessorKey: "documents",
      header: "Dökümanlar",
      cell: (info) => {
        const docs = info.getValue() as GuideDocument[];
        return <span className="font-semibold text-slate-300">{(docs ?? []).length} Adet</span>;
      },
    },
    {
      id: "actions",
      header: "İşlemler",
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          {canUpdate("Guide") && (
            <button
              onClick={() => openEditModal(row.original)}
              className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
              title="Düzenle"
            >
              <Pencil size={16} />
            </button>
          )}
          {canDelete("Guide") && (
            <button
              onClick={() => handleDelete(row.original.id)}
              className="p-1.5 rounded-lg text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/50 transition-colors"
              title="Sil"
            >
              <Trash2 size={16} />
            </button>
          )}
        </div>
      ),
    },
  ], [canUpdate, canDelete, openEditModal, handleDelete]);

  const excelColumns: ExportColumn<Guide>[] = useMemo(() => [
    { header: "Başlık", key: "title" },
    { header: "Açıklama", key: (g) => stripHtml(g.description) },
    { header: "Modül", key: "module" },
    { header: "YouTube URL", key: (g) => g.youtubeUrl || "-" },
    { header: "Döküman Sayısı", key: (g) => (g.documents ?? []).length },
    { header: "Erişim", key: (g) => (g.isPublic ? "Herkese Açık" : "Özel") },
    { header: "Durum", key: (g) => (g.isActive ? "Aktif" : "Pasif") },
  ], []);

  if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

  return (
    <div className="max-w-400 mx-auto px-3 sm:px-6 md:px-8 py-4 sm:py-8 space-y-6 sm:space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
      
      {/* Header Section */}
      <GuideHeader onAddClick={() => { setIsEditMode(false); setIsModalOpen(true); }} canCreate={canCreate("Guide")} />

      {/* Main List */}
      <ApiStateView
        isLoading={initialLoading}
        isForbidden={isForbidden}
        error={fetchError}
        onRetry={() => fetchGuides()}
        variant="page"
        loadingMessage="Rehberler Getiriliyor..."
      >
        <DataTable
          data={guides}
          columns={columns}
          excelColumns={excelColumns}
          excelFileName="HelpCenter_Rehberler"
          searchPlaceholder="Rehber başlığı veya açıklama ile ara..."
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
          onFetchAllData={() => GuideService.getAll({ search: searchQuery })}
          canExport={canExport("Guide")}
          canPrint={canPrint("Guide")}
        />
      </ApiStateView>

      {/* Modal */}
      <AnimatePresence>
        {isModalOpen && (
          <GuideModal 
            isOpen={isModalOpen}
            isEditMode={isEditMode}
            onClose={closeModal}
            onSubmit={handleSubmit}
            formData={newGuide}
            setFormData={setNewGuide}
            modules={modules}
            guides={guides}
            editingId={editingId}
            loading={actionLoading}
          />
        )}
      </AnimatePresence>

      <AlertModal 
        isOpen={alertConfig.isOpen}
        title={alertConfig.title}
        description={alertConfig.description}
        onConfirm={alertConfig.onConfirm}
        onCancel={() => setAlertConfig(prev => ({ ...prev, isOpen: false }))}
      />

      <style jsx global>{`
        .custom-scrollbar::-webkit-scrollbar {
          width: 8px;
        }
        .custom-scrollbar::-webkit-scrollbar-track {
          background: #f8fafc;
          border-radius: 20px;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb {
          background: #e2e8f0;
          border-radius: 20px;
          border: 2px solid #f8fafc;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb:hover {
          background: #cbd5e1;
        }
      `}</style>
    </div>
  );
}
