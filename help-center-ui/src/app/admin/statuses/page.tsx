"use client";

import React, { useState, useMemo } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import type { RequestStatus } from "@/services/admin/AdminStatusService";
import { AdminStatusService as StatusService } from "@/services/admin/AdminStatusService";
import { useStatusesQuery } from "@/hooks/admin/useStatusesQuery";

import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";

// Modular Components
import { StatusHeader } from "./components/StatusHeader";
import { StatusInfoBox } from "./components/StatusInfoBox";

export default function AdminStatusesPage() {
    usePageTitle("Statuses");
    const { userInfo } = useAuth();
    const { canRead, canExport, canPrint } = usePermission(userInfo);
    const hasViewPerm = canRead("Statuses");

    // Pagination states
    const [pageIndex, setPageIndex] = useState(0);
    const [pageSize, setPageSize] = useState(10);
    const [searchQuery, setSearchQuery] = useState("");
    const statusQuery = useStatusesQuery({
        pageNumber: pageIndex + 1,
        pageSize,
        search: searchQuery,
    });
    const statuses = statusQuery.data?.items ?? [];
    const totalCount = statusQuery.data?.totalCount ?? 0;
    const totalPages = statusQuery.data?.totalPages ?? 1;
    const isForbidden = isForbiddenError(statusQuery.error);

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage - 1);
    };

    const handlePageSizeChange = (newSize: number) => {
        setPageSize(newSize);
        setPageIndex(0);
    };

    const handleSearchChange = (val: string) => {
        setSearchQuery(val);
        setPageIndex(0);
    };

    const columns = useMemo<ColumnDef<RequestStatus>[]>(() => [
        {
            accessorKey: "name",
            header: "Durum Adı",
            cell: (info) => <span className="font-semibold text-slate-50">{info.getValue() as string}</span>,
        },
        {
            accessorKey: "description",
            header: "Açıklama",
            cell: (info) => <span className="block max-w-md truncate text-xs font-medium text-slate-300">{(info.getValue() as string) || "-"}</span>,
        },
        {
            accessorKey: "isActive",
            header: "Durum",
            cell: (info) => (
                <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${
                    info.getValue()
                        ? "bg-emerald-50 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800"
                        : "bg-slate-800 text-slate-300 border-slate-700"
                }`}>
                    {info.getValue() ? "Aktif" : "Pasif"}
                </span>
            ),
        },
    ], []);

    const excelColumns: ExportColumn<RequestStatus>[] = useMemo(() => [
        { header: "Durum Adı", key: "name" },
        { header: "Açıklama", key: (s) => s.description || "-" },
        { header: "Durum", key: (s) => (s.isActive ? "Aktif" : "Pasif") },
    ], []);

    if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

    return (
        <div className="space-y-6 sm:space-y-8 px-3 sm:px-6 md:px-8 py-4 sm:py-8 animate-in fade-in duration-700">
            {/* Header Area */}
            <StatusHeader />

            {/* Content Area */}
            <ApiStateView
                isLoading={statusQuery.isPending}
                isForbidden={isForbidden}
                error={statusQuery.error}
                onRetry={() => void statusQuery.refetch()}
                variant="page"
                loadingMessage="Durumlar Getiriliyor..."
            >
                <DataTable
                    data={statuses}
                    columns={columns}
                    excelColumns={excelColumns}
                    excelFileName="HelpCenter_Durumlar"
                    searchPlaceholder="Durum adı veya açıklama ile ara..."
                    manualPagination={true}
                    pageIndex={pageIndex}
                    pageSize={pageSize}
                    pageCount={totalPages}
                    totalCount={totalCount}
                    onPageChange={handlePageChange}
                    onPageSizeChange={handlePageSizeChange}
                    onSearchChange={handleSearchChange}
                    searchValue={searchQuery}
                    isLoading={statusQuery.isFetching}
                    onFetchAllData={() => StatusService.getAll({ search: searchQuery })}
                    canExport={canExport("Statuses")}
                    canPrint={canPrint("Statuses")}
                />
            </ApiStateView>

            {/* Info Box */}
            <StatusInfoBox />
        </div>
    );
}
