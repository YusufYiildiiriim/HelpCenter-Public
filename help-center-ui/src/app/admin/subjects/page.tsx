"use client";

import React, { useEffect, useState, useCallback, useMemo } from "react";
import { Edit, Trash2, Lock } from "lucide-react";
import type { RequestSubject } from "@/services/admin/AdminRequestSubjectService";
import { AdminRequestSubjectService as RequestSubjectService } from "@/services/admin/AdminRequestSubjectService";
import { toast } from "sonner";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { AnimatePresence } from "framer-motion";
import type { ColumnDef } from "@tanstack/react-table";

// Modular Components & Reusable DataTable
import { AlertModal } from "../components/AlertModal";
import { SubjectHeader } from "./components/SubjectHeader";
import { SubjectModal } from "./components/SubjectModal";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";

export default function AdminSubjectsPage() {
    usePageTitle("Subjects");
    const { userInfo } = useAuth();
    const { canRead, canCreate, canUpdate, canDelete, canExport, canPrint } = usePermission(userInfo);
    const hasViewPerm = canRead("Subjects");
    const [subjects, setSubjects] = useState<RequestSubject[]>([]);
    const [initialLoading, setInitialLoading] = useState(true);
    const [tableLoading, setTableLoading] = useState(false);
    const [fetchError, setFetchError] = useState<unknown>(null);
    const [isForbidden, setIsForbidden] = useState(false);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isEditMode, setIsEditMode] = useState(false);
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [subjectData, setSubjectData] = useState({ name: "", description: "", isActive: true });
    const [submitting, setSubmitting] = useState(false);
    const [success, setSuccess] = useState(false);

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

    const fetchSubjects = useCallback(async (page = pageIndex + 1, size = pageSize, search = searchQuery) => {
        setTableLoading(true);
        try {
            const res = await RequestSubjectService.getPaginated({ pageNumber: page, pageSize: size, search });
            setSubjects(res.items);
            setTotalCount(res.totalCount);
            setTotalPages(res.totalPages);
            setFetchError(null);
            setIsForbidden(false);
        } catch (err) {
            console.error("Error fetching subjects:", err);
            if (isForbiddenError(err)) setIsForbidden(true);
            else setFetchError(err);
        } finally {
            setTableLoading(false);
            setInitialLoading(false);
        }
    }, [pageIndex, pageSize, searchQuery]);

    const fetchData = useCallback(async () => {
        await fetchSubjects(pageIndex + 1, pageSize, searchQuery);
    }, [fetchSubjects, pageIndex, pageSize, searchQuery]);

    useEffect(() => {
        // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional one-time fetch on mount
        fetchSubjects(1, 10, "");
        // eslint-disable-next-line react-hooks/exhaustive-deps -- intended to run only on mount
    }, []);

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage - 1);
        fetchSubjects(newPage, pageSize, searchQuery);
    };

    const handlePageSizeChange = (newSize: number) => {
        setPageSize(newSize);
        setPageIndex(0);
        fetchSubjects(1, newSize, searchQuery);
    };

    const handleSearchChange = (val: string) => {
        setSearchQuery(val);
        setPageIndex(0);
        fetchSubjects(1, pageSize, val);
    };

    const handleSaveSubject = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        try {
            if (isEditMode && selectedId) {
                await RequestSubjectService.update(selectedId, subjectData.name, subjectData.description, subjectData.isActive);
                toast.success("Konu başarıyla güncellendi.");
            } else {
                await RequestSubjectService.create(subjectData.name, subjectData.description, subjectData.isActive);
                toast.success("Yeni konu başarıyla eklendi.");
            }
            setSuccess(true);
            await fetchData();
            setTimeout(() => {
                closeModal();
            }, 1000);
        } catch (err: unknown) {
            console.error("Error saving subject:", err);
            toast.error(getApiErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = useCallback((id: number) => {
        setAlertConfig({
            isOpen: true,
            title: "Konuyu Sil",
            description: "Bu talep konusu sistemden kalıcı olarak silinecektir. Devam etmek istediğinize emin misiniz?",
            onConfirm: async () => {
                try {
                    await RequestSubjectService.delete(id);
                    toast.success("Konu başarıyla silindi.");
                    await fetchData();
                } catch (err: unknown) {
                    console.error("Error deleting subject:", err);
                    toast.error(getApiErrorMessage(err));
                } finally {
                    setAlertConfig(prev => ({ ...prev, isOpen: false }));
                }
            }
        });
    }, [fetchData]);

    const openEditModal = useCallback((subject: RequestSubject) => {
        setSubjectData({ name: subject.name, description: subject.description, isActive: subject.isActive });
        setSelectedId(subject.id);
        setIsEditMode(true);
        setIsModalOpen(true);
    }, []);

    const closeModal = () => {
        setIsModalOpen(false);
        setIsEditMode(false);
        setSelectedId(null);
        setSubjectData({ name: "", description: "", isActive: true });
        setSuccess(false);
    };

    const columns = useMemo<ColumnDef<RequestSubject>[]>(() => [
        {
            accessorKey: "name",
            header: "Konu Adı",
            cell: (info) => <span className="font-semibold text-slate-50">{info.getValue() as string}</span>,
        },
        {
            accessorKey: "description",
            header: "Açıklama",
            cell: (info) => <span className="block max-w-xs truncate text-slate-300">{(info.getValue() as string) || "-"}</span>,
        },
        {
            accessorKey: "usageCount",
            header: "Kullanım Sayısı",
            cell: (info) => (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
                    {info.getValue() as number} Talep
                </span>
            ),
        },
        {
            accessorKey: "isLocked",
            header: "Kilit",
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
                const sub = row.original;
                return (
                    <div className="flex items-center gap-2">
                        {canUpdate("Subjects") && (
                            <button
                                onClick={() => openEditModal(sub)}
                                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                                title="Düzenle"
                            >
                                <Edit size={16} />
                            </button>
                        )}
                        {canDelete("Subjects") && (
                            <button
                                onClick={() => handleDelete(sub.id)}
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
    ], [canUpdate, canDelete, openEditModal, handleDelete]);

    const excelColumns: ExportColumn<RequestSubject>[] = useMemo(() => [
        { header: "Konu Adı", key: "name" },
        { header: "Açıklama", key: "description" },
        { header: "Kullanım Sayısı", key: "usageCount" },
        { header: "Kilit Durumu", key: (s) => (s.isLocked ? "Kilitli" : "Açık") },
        { header: "Durum", key: (s) => (s.isActive ? "Aktif" : "Pasif") },
    ], []);

    if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

    return (
        <div className="max-w-[1400px] mx-auto px-3 sm:px-6 md:px-8 py-4 sm:py-8 space-y-6 sm:space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
            
            {/* Header Section */}
            <SubjectHeader 
                canCreate={canCreate("Subjects")} 
                onAddClick={() => setIsModalOpen(true)} 
            />

            {/* List Section */}
            <ApiStateView
                isLoading={initialLoading}
                isForbidden={isForbidden}
                error={fetchError}
                onRetry={() => fetchSubjects()}
                variant="page"
                loadingMessage="Konular Getiriliyor..."
            >
                <DataTable
                    data={subjects}
                    columns={columns}
                    excelColumns={excelColumns}
                    excelFileName="HelpCenter_Talep_Konulari"
                    searchPlaceholder="Konu adı veya açıklama ile ara..."
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
                    onFetchAllData={() => RequestSubjectService.getAll({ search: searchQuery })}
                    canExport={canExport("Subjects")}
                    canPrint={canPrint("Subjects")}
                />
            </ApiStateView>

            {/* Modal */}
            <AnimatePresence mode="wait">
                {isModalOpen && (
                    <SubjectModal 
                        isEditMode={isEditMode}
                        subjectData={subjectData}
                        setSubjectData={setSubjectData}
                        onSubmit={handleSaveSubject}
                        onClose={closeModal}
                        submitting={submitting}
                        success={success}
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
