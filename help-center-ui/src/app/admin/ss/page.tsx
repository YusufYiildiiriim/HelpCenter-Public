"use client";

import React, { useState, useEffect, useCallback, useMemo } from "react";
import { toast } from "sonner";
import { Edit, Trash2 } from "lucide-react";
import type { Faq } from "@/services/admin/AdminFaqService";
import { AdminFaqService as FaqService } from "@/services/admin/AdminFaqService";
import type { Project } from "@/services/admin/AdminProjectService";
import { AdminProjectService as ProjectService } from "@/services/admin/AdminProjectService";
import type { Module } from "@/services/admin/AdminModuleService";
import { AdminModuleService as ModuleService } from "@/services/admin/AdminModuleService";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import type { ColumnDef } from "@tanstack/react-table";

// Modular Components & Reusable DataTable
import { FaqHeader } from "./components/FaqHeader";
import { FaqModal } from "./components/FaqModal";
import { AlertModal } from "../components/AlertModal";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";

export default function FaqPage() {
  usePageTitle("FAQ");
  const { userInfo } = useAuth();
  const { canRead, canCreate, canUpdate, canDelete, canExport, canPrint } = usePermission(userInfo);
  const hasViewPerm = canRead("FAQ");

  const [faqs, setFaqs] = useState<Faq[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [modules, setModules] = useState<Module[]>([]);
  const [initialLoading, setInitialLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [fetchError, setFetchError] = useState<unknown>(null);
  const [isForbidden, setIsForbidden] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);

  // Pagination states
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [searchQuery, setSearchQuery] = useState("");

  const [newFaq, setNewFaq] = useState<Faq>({
    title: "",
    description: "",
    projectId: null,
    moduleId: null,
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

  const fetchFaqs = useCallback(async (page = pageIndex + 1, size = pageSize, search = searchQuery) => {
    setTableLoading(true);
    try {
      const res = await FaqService.getPaginated({ pageNumber: page, pageSize: size, search });
      setFaqs(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages);
      setFetchError(null);
      setIsForbidden(false);
    } catch (err) {
      console.error("Error fetching FAQs:", err);
      if (isForbiddenError(err)) setIsForbidden(true);
      else setFetchError(err);
    } finally {
      setTableLoading(false);
      setInitialLoading(false);
    }
  }, [pageIndex, pageSize, searchQuery]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional one-time fetch on mount
    fetchFaqs(1, 10, "");
    Promise.all([
      ProjectService.getAll({ onlyActive: true }),
      ModuleService.getAll({ onlyActive: true }),
    ])
      .then(([projData, modData]) => {
        setProjects(projData);
        setModules(modData);
      })
      .catch((err) => console.error("Error loading lookups:", err));
    // eslint-disable-next-line react-hooks/exhaustive-deps -- intended to run only on mount
  }, []);

  const handlePageChange = (newPage: number) => {
    setPageIndex(newPage - 1);
    fetchFaqs(newPage, pageSize, searchQuery);
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPageIndex(0);
    fetchFaqs(1, newSize, searchQuery);
  };

  const handleSearchChange = (val: string) => {
    setSearchQuery(val);
    setPageIndex(0);
    fetchFaqs(1, pageSize, val);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await FaqService.update({ ...newFaq, id: editingId });
        toast.success("Soru başarıyla güncellendi.");
      } else {
        await FaqService.create(newFaq);
        toast.success("Yeni soru başarıyla eklendi.");
      }
      setIsModalOpen(false);
      setNewFaq({ title: "", description: "", projectId: null, moduleId: null, isActive: true, isPublic: false });
      setEditingId(null);
      fetchFaqs();
    } catch (err: unknown) {
      console.error("Error saving FAQ:", err);
      toast.error(getApiErrorMessage(err));
    }
  };

  const handleEdit = useCallback((faq: Faq) => {
    setNewFaq(faq);
    setEditingId(faq.id!);
    setIsModalOpen(true);
  }, []);

  const handleDelete = useCallback(async (id: number) => {
    setAlertConfig({
      isOpen: true,
      title: "Soruyu Sil",
      description: "Bu soru sistemden kalıcı olarak silinecektir. Devam etmek istediğinize emin misiniz?",
      onConfirm: async () => {
        try {
          await FaqService.delete(id);
          toast.success("Soru başarıyla silindi.");
          fetchFaqs();
          setAlertConfig(prev => ({ ...prev, isOpen: false }));
        } catch (err) {
          console.error("Error deleting FAQ:", err);
          toast.error("Silme işlemi başarısız.");
        }
      }
    });
  }, [fetchFaqs]);

  const columns = useMemo<ColumnDef<Faq>[]>(() => [
    {
      accessorKey: "title",
      header: "Soru Başlığı",
      cell: (info) => <span className="font-semibold text-slate-50">{info.getValue() as string}</span>,
    },
    {
      accessorKey: "projectName",
      header: "Proje",
      cell: (info) => {
        const val = info.getValue() as string | undefined;
        return (
          <span className="inline-flex items-center px-2 py-0.5 rounded-lg text-xs font-bold bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
            {val || "—"}
          </span>
        );
      },
    },
    {
      accessorKey: "moduleName",
      header: "Modül",
      cell: (info) => {
        const val = info.getValue() as string | undefined;
        return (
          <span className="inline-flex items-center px-2 py-0.5 rounded-lg text-xs font-bold bg-violet-500/10 text-violet-400 border border-violet-500/20">
            {val || "—"}
          </span>
        );
      },
    },
    {
      accessorKey: "description",
      header: "Cevap / Açıklama",
      cell: (info) => (
        <span className="block max-w-md line-clamp-2 font-medium text-slate-300">
          {info.getValue() as string}
        </span>
      ),
    },
    {
      accessorKey: "isPublic",
      header: "Erişim",
      cell: (info) => (
        <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${
          info.getValue()
            ? "bg-purple-50 dark:bg-purple-950/60 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-800"
            : "bg-slate-800 text-slate-300 border-slate-700"
        }`}>
          {info.getValue() ? "Herkese Açık" : "Özel (Login Şart)"}
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
        const faq = row.original;
        return (
          <div className="flex items-center gap-2">
            {canUpdate("FAQ") && (
              <button
                onClick={() => handleEdit(faq)}
                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                title="Düzenle"
              >
                <Edit size={16} />
              </button>
            )}
            {canDelete("FAQ") && (
              <button
                onClick={() => handleDelete(faq.id!)}
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
  ], [canUpdate, canDelete, handleEdit, handleDelete]);

  const excelColumns: ExportColumn<Faq>[] = useMemo(() => [
    { header: "Soru Başlığı", key: "title" },
    { header: "Proje", key: (f) => f.projectName || "-" },
    { header: "Modül", key: (f) => f.moduleName || "-" },
    { header: "Cevap / Açıklama", key: "description" },
    { header: "Herkese Açık mı", key: (f) => (f.isPublic ? "Evet" : "Hayır") },
    { header: "Durum", key: (f) => (f.isActive ? "Aktif" : "Pasif") },
  ], []);

  if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

  return (
    <div className="mx-auto max-w-7xl px-3 sm:px-6 md:px-8 py-4 sm:py-8 space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
      <FaqHeader
        onAddClick={() => {
          setNewFaq({ title: "", description: "", projectId: null, moduleId: null, isActive: true, isPublic: false });
          setEditingId(null);
          setIsModalOpen(true);
        }}
        canCreate={canCreate("FAQ")}
      />

      <ApiStateView
        isLoading={initialLoading}
        isForbidden={isForbidden}
        error={fetchError}
        onRetry={() => fetchFaqs()}
        variant="page"
        loadingMessage="Sorular Getiriliyor..."
      >
        <DataTable
          data={faqs}
          columns={columns}
          excelColumns={excelColumns}
          excelFileName="HelpCenter_SSS_Sorulari"
          searchPlaceholder="Soru başlığı veya açıklama ile ara..."
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
          onFetchAllData={() => FaqService.getAll({ search: searchQuery })}
          canExport={canExport("FAQ")}
          canPrint={canPrint("FAQ")}
        />
      </ApiStateView>

      {isModalOpen && (
        <FaqModal
          isEditMode={!!editingId}
          faq={newFaq}
          projects={projects}
          modules={modules}
          onFaqChange={setNewFaq}
          onSave={handleSave}
          onClose={() => setIsModalOpen(false)}
        />
      )}

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
