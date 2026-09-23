"use client";

import React, { useEffect, useState, useMemo, useCallback, useRef } from "react";
import { motion } from "framer-motion";
import type { ColumnDef } from "@tanstack/react-table";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { AdminRequestService } from "@/services/admin/AdminRequestService";
import { usePageTitle } from "@/lib/usePageTitle";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";

import { AssignedRequestHeader } from "./components/AssignedRequestHeader";

export default function AssignedRequestsPage() {
    usePageTitle("Assigned Requests");
    const router = useRouter();
    const { userInfo } = useAuth();
    const { canRead, canExport, canPrint } = usePermission(userInfo);
    const hasViewPerm = canRead("AssignedRequests");
    const [requests, setRequests] = useState<AdminRequest[]>([]);
    const [loading, setLoading] = useState(true);
    const [tableLoading, setTableLoading] = useState(false);
    const [filter, setFilter] = useState("Hepsi");
    const [fetchError, setFetchError] = useState<unknown>(null);
    const [isForbidden, setIsForbidden] = useState(false);

    // Pagination states
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(1);

    const requestSeqRef = useRef(0);

    const fetchAssigned = useCallback(async (statusFilter: string, page: number, size: number) => {
        const seq = ++requestSeqRef.current;
        try {
            setTableLoading(true);
            const data = await AdminRequestService.getAssigned({
                status: statusFilter === "Hepsi" ? undefined : statusFilter,
                pageNumber: page,
                pageSize: size,
            });
            if (seq !== requestSeqRef.current) return;
            setRequests(data.items);
            setTotalCount(data.totalCount);
            setTotalPages(data.totalPages);
            setFetchError(null);
            setIsForbidden(false);
        } catch (err) {
            console.error("Error fetching assigned requests:", err);
            if (seq !== requestSeqRef.current) return;
            if (isForbiddenError(err)) setIsForbidden(true);
            else setFetchError(err);
        } finally {
            if (seq === requestSeqRef.current) {
                setLoading(false);
                setTableLoading(false);
            }
        }
    }, []);

    useEffect(() => {
        if (!userInfo?.userId) return;
        // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional fetch triggering on mount/filter change
        setLoading(true);
        setPageNumber(1);
        fetchAssigned(filter, 1, pageSize);
        // eslint-disable-next-line react-hooks/exhaustive-deps -- page change is triggered from a separate handler, only user/filter changes are observed here
    }, [userInfo?.userId, filter, fetchAssigned]);

    const sortedRequests = useMemo(() => [...requests].sort((a, b) => b.id - a.id), [requests]);

    const handleFilterChange = (nextFilter: string) => {
        setFilter(nextFilter);
    };

    const handlePageChange = (newPage: number) => {
        setPageNumber(newPage);
        if (userInfo?.userId) fetchAssigned(filter, newPage, pageSize);
    };

    const handlePageSizeChange = (newSize: number) => {
        setPageSize(newSize);
        setPageNumber(1);
        if (userInfo?.userId) fetchAssigned(filter, 1, newSize);
    };

    const formatDate = (date: string) => {
        if (!date) return "-";
        return new Date(date).toLocaleString("tr-TR", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    };

    const statusBadgeClass = (status: string) => {
        if (status === "Cevap Bekliyor") return "border-amber-200 bg-amber-50 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800";
        if (status === "Cevaplandi" || status === "Cevaplandı") return "border-blue-200 bg-blue-50 text-blue-600 dark:bg-blue-950/60 dark:text-blue-300 dark:border-blue-800";
        if (status === "Teknik Incelemede" || status === "Teknik İncelemede") return "border-indigo-200 bg-indigo-50 text-indigo-600 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800";
        return "border-emerald-200 bg-emerald-50 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800";
    };

    const priorityBadgeClass = (priority: string) => {
        if (priority === "Critical") return "border-red-200 bg-red-50 text-red-600 dark:bg-red-950/60 dark:text-red-300 dark:border-red-800";
        if (priority === "High") return "border-amber-200 bg-amber-50 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800";
        if (priority === "Medium") return "border-blue-200 bg-blue-50 text-blue-600 dark:bg-blue-950/60 dark:text-blue-300 dark:border-blue-800";
        return "border-slate-200 bg-slate-50 text-slate-500 dark:bg-slate-800 dark:text-slate-400 dark:border-slate-700";
    };

    const columns = useMemo<ColumnDef<AdminRequest>[]>(() => [
        {
            accessorKey: "ticketId",
            header: "Talep No",
            cell: (info) => (
                <span className="inline-flex rounded-md bg-slate-950 dark:bg-slate-800 px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider text-white">
                    {info.getValue() as string}
                </span>
            ),
        },
        {
            accessorKey: "title",
            header: "Başlık",
            cell: (info) => (
                <div className="min-w-0">
                    <span className="block max-w-xs truncate font-semibold text-slate-50">{info.getValue() as string}</span>
                    <span className="block max-w-xs truncate text-xs text-slate-400">
                        {(info.row.original.description || "").replace(/<[^>]*>/g, "")}
                    </span>
                </div>
            ),
        },
        {
            accessorKey: "companyName",
            header: "Firma",
            cell: (info) => <span className="text-slate-300">{(info.getValue() as string) || "-"}</span>,
        },
        {
            accessorKey: "moduleName",
            header: "Modül",
            cell: (info) => (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-blue-50 dark:bg-blue-950/60 text-blue-700 dark:text-blue-300 border border-blue-200 dark:border-blue-800">
                    {(info.getValue() as string) || "-"}
                </span>
            ),
        },
        {
            accessorKey: "priorityName",
            header: "Öncelik",
            cell: (info) => (
                <span className={`inline-flex rounded-md border px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider ${priorityBadgeClass(info.getValue() as string)}`}>
                    {info.getValue() as string}
                </span>
            ),
        },
        {
            accessorKey: "status",
            header: "Durum",
            cell: (info) => (
                <span className={`inline-flex rounded-md border px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider ${statusBadgeClass(info.getValue() as string)}`}>
                    {info.getValue() as string}
                </span>
            ),
        },
        {
            accessorKey: "createdAt",
            header: "Oluşturulma",
            cell: (info) => <span className="text-slate-300">{formatDate(info.getValue() as string)}</span>,
        },
        {
            accessorKey: "messageCount",
            header: "Mesaj",
            cell: (info) => <span className="font-semibold text-slate-300">{info.getValue() as number} Mesaj</span>,
        },
    ], []);

    const excelColumns: ExportColumn<AdminRequest>[] = useMemo(() => [
        { header: "Talep No", key: "ticketId" },
        { header: "Başlık", key: "title" },
        { header: "Açıklama", key: (r) => (r.description || "").replace(/<[^>]*>/g, "") },
        { header: "Firma", key: (r) => r.companyName || "-" },
        { header: "Müşteri", key: (r) => r.customerName || "-" },
        { header: "Modül", key: (r) => r.moduleName || "-" },
        { header: "Öncelik", key: "priorityName" },
        { header: "Durum", key: "status" },
        { header: "Mesaj Sayısı", key: "messageCount" },
        { header: "Oluşturulma", key: (r) => formatDate(r.createdAt) },
    ], []);

    if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

    return (
        <div className="max-w-[1600px] mx-auto space-y-6 pb-12 animate-in fade-in slide-in-from-bottom-3 duration-500">
            <motion.div initial={{ opacity: 0, y: -12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.4 }}>
                <AssignedRequestHeader
                    filter={filter}
                    setFilter={handleFilterChange}
                    totalCount={totalCount}
                />
            </motion.div>

            <ApiStateView
                isLoading={loading}
                isForbidden={isForbidden}
                error={fetchError}
                onRetry={() => userInfo?.userId && fetchAssigned(filter, pageNumber, pageSize)}
                variant="page"
                loadingMessage="Talepler Getiriliyor..."
            >
                <DataTable
                    data={sortedRequests}
                    columns={columns}
                    excelColumns={excelColumns}
                    excelFileName="HelpCenter_AtananTalepler"
                    manualPagination={true}
                    pageIndex={pageNumber - 1}
                    pageSize={pageSize}
                    pageCount={totalPages}
                    totalCount={totalCount}
                    isLoading={tableLoading}
                    onPageChange={handlePageChange}
                    onPageSizeChange={handlePageSizeChange}
                    searchPlaceholder="Talep başlığı, firma veya müşteri ara..."
                    canExport={canExport("AssignedRequests")}
                    canPrint={canPrint("AssignedRequests")}
                    onRowClick={(request) => router.push(`/admin/requests/${request.publicId}`)}
                />
            </ApiStateView>
        </div>
    );
}
