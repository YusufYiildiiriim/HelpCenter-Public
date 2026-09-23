"use client";

import React, { useEffect, useState, useMemo, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { AdminRequestService } from "@/services/admin/AdminRequestService";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";
import type { ColumnDef } from "@tanstack/react-table";

type Company = { publicId: string; name: string };

// Modular Components
import { RequestHeader } from "./components/RequestHeader";

export default function AdminRequestsPage() {
    usePageTitle("Requests");
    const router = useRouter();
    const { userInfo } = useAuth();
    const { canRead, canExport, canPrint } = usePermission(userInfo);
    const hasViewPerm = canRead("Requests");
    const [requests, setRequests] = useState<AdminRequest[]>([]);
    const [companies, setCompanies] = useState<Company[]>([]);
    const [selectedCompanyId, setSelectedCompanyId] = useState<string>("all");
    const [isCompanyOpen, setIsCompanyOpen] = useState(false);
    const [loading, setLoading] = useState(true);
    const [tableLoading, setTableLoading] = useState(false);
    const [filter, setFilter] = useState("Hepsi");
    const [fetchError, setFetchError] = useState<unknown>(null);
    const [isForbidden, setIsForbidden] = useState(false);

    // Pagination & search states
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(1);
    const [searchQuery, setSearchQuery] = useState("");

    const companiesLoadedRef = React.useRef(false);
    const requestSeqRef = useRef(0);

    const fetchRequests = useCallback(async (
        statusFilter: string | undefined,
        companyPublicId: string | undefined,
        page: number,
        size: number,
        search: string
    ) => {
        const seq = ++requestSeqRef.current;
        setTableLoading(true);
        const requestsPromise = AdminRequestService.getPaginated({
            status: statusFilter,
            companyPublicId,
            pageNumber: page,
            pageSize: size,
            search: search.trim() ? search.trim() : undefined,
        });
        const companiesPromise = companiesLoadedRef.current
            ? Promise.resolve(null)
            : AdminRequestService.getCompanies();

        const [reqRes, compRes] = await Promise.allSettled([requestsPromise, companiesPromise]);
        if (seq !== requestSeqRef.current) return;

        if (reqRes.status === "fulfilled") {
            setRequests(reqRes.value.items ?? []);
            setTotalCount(reqRes.value.totalCount);
            setTotalPages(reqRes.value.totalPages);
            setFetchError(null);
            setIsForbidden(false);
        } else {
            console.error("Error fetching requests:", reqRes.reason);
            if (isForbiddenError(reqRes.reason)) setIsForbidden(true);
            else setFetchError(reqRes.reason);
        }
        if (compRes.status === "fulfilled" && compRes.value) {
            setCompanies(compRes.value);
            companiesLoadedRef.current = true;
        } else if (compRes.status === "rejected") {
            console.error("Error fetching companies:", compRes.reason);
        }
        setLoading(false);
        setTableLoading(false);
    }, []);

    useEffect(() => {
        const statusFilter = filter === "Hepsi" ? undefined : filter;
        const companyPublicId = selectedCompanyId !== "all" ? selectedCompanyId : undefined;
        // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional fetch triggering on mount/filter change
        setLoading(true);
        setPageNumber(1);
        fetchRequests(statusFilter, companyPublicId, 1, pageSize, searchQuery);
        // eslint-disable-next-line react-hooks/exhaustive-deps -- page/search changes are triggered from separate handlers, only filter/company changes are observed here
    }, [filter, selectedCompanyId, fetchRequests]);

    const filteredRequests = useMemo(() => {
        const list = Array.isArray(requests) ? requests : [];
        return [...list].sort((a, b) => b.id - a.id);
    }, [requests]);

    const handleCompanyChange = (id: string) => {
        setSelectedCompanyId(id);
    };
    const handleFilterChange = (nextFilter: string) => {
        setFilter(nextFilter);
    };

    const currentStatusAndCompany = () => ({
        statusFilter: filter === "Hepsi" ? undefined : filter,
        companyPublicId: selectedCompanyId !== "all" ? selectedCompanyId : undefined,
    });

    const handlePageChange = (newPage: number) => {
        setPageNumber(newPage);
        const { statusFilter, companyPublicId } = currentStatusAndCompany();
        fetchRequests(statusFilter, companyPublicId, newPage, pageSize, searchQuery);
    };

    const handlePageSizeChange = (newSize: number) => {
        setPageSize(newSize);
        setPageNumber(1);
        const { statusFilter, companyPublicId } = currentStatusAndCompany();
        fetchRequests(statusFilter, companyPublicId, 1, newSize, searchQuery);
    };

    const handleSearchChange = (val: string) => {
        setSearchQuery(val);
        setPageNumber(1);
        const { statusFilter, companyPublicId } = currentStatusAndCompany();
        fetchRequests(statusFilter, companyPublicId, 1, pageSize, val);
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
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
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
        { header: "Atanan Uzman", key: (r) => r.assignedUserName || "-" },
        { header: "Mesaj Sayısı", key: "messageCount" },
        { header: "Oluşturulma", key: (r) => formatDate(r.createdAt) },
    ], []);

    if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

    return (
        <div className="pt-3 md:pt-4 lg:pt-6 space-y-6 md:space-y-8 pb-12">
            
            {/* Header Section */}
            <RequestHeader 
                selectedCompanyId={selectedCompanyId}
                setSelectedCompanyId={handleCompanyChange}
                companies={companies}
                isCompanyOpen={isCompanyOpen}
                setIsCompanyOpen={setIsCompanyOpen}
                filter={filter}
                setFilter={handleFilterChange}
            />

            {/* List Section */}
            <ApiStateView
                isLoading={loading}
                isForbidden={isForbidden}
                error={fetchError}
                onRetry={() => {
                    const { statusFilter, companyPublicId } = currentStatusAndCompany();
                    setLoading(true);
                    fetchRequests(statusFilter, companyPublicId, pageNumber, pageSize, searchQuery);
                }}
                variant="page"
                loadingMessage="Talepler Getiriliyor..."
            >
                <DataTable
                    data={filteredRequests}
                    columns={columns}
                    excelColumns={excelColumns}
                    excelFileName="HelpCenter_Talepler"
                    manualPagination={true}
                    pageIndex={pageNumber - 1}
                    pageSize={pageSize}
                    pageCount={totalPages}
                    totalCount={totalCount}
                    isLoading={tableLoading}
                    onPageChange={handlePageChange}
                    onPageSizeChange={handlePageSizeChange}
                    onSearchChange={handleSearchChange}
                    searchValue={searchQuery}
                    onFetchAllData={() => {
                        const { statusFilter, companyPublicId } = currentStatusAndCompany();
                        return AdminRequestService.getAll({ status: statusFilter, companyPublicId, search: searchQuery });
                    }}
                    searchPlaceholder="Talep başlığı, firma veya müşteri ara..."
                    canExport={canExport("Requests")}
                    canPrint={canPrint("Requests")}
                    onRowClick={(r) => router.push(`/admin/requests/${r.publicId}`)}
                />
            </ApiStateView>
        </div>
    );
}
