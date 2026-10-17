"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import type { ColumnDef } from "@tanstack/react-table";
import api from "@/lib/axios";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { ApiStateView } from "@/components/common/ApiStateView";
import { DataTable } from "@/components/common/DataTable";
import { isForbiddenError } from "@/lib/api/errors";

interface AuditRow {
  id: string;
  eventType: string;
  actorUserId: string | null;
  targetType: string | null;
  targetId: string | null;
  correlationId: string | null;
  occurredAt: string;
}

const columns: ColumnDef<AuditRow>[] = [
  { accessorKey: "occurredAt", header: "Zaman", cell: info => new Date(info.getValue<string>()).toLocaleString("tr-TR") },
  { accessorKey: "eventType", header: "Olay" },
  { accessorKey: "actorUserId", header: "İşlemi yapan" },
  { accessorKey: "targetType", header: "Hedef türü" },
  { accessorKey: "targetId", header: "Hedef kimliği" },
  { accessorKey: "correlationId", header: "Takip kimliği" },
];

export default function AuditLogsPage() {
  const { userInfo } = useAuth();
  const allowed = usePermission(userInfo).canRead("AuditLogs");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [eventType, setEventType] = useState("");
  const [targetType, setTargetType] = useState("");
  const [actor, setActor] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const query = useQuery({
    queryKey: ["audit-logs", userInfo?.userId, page, pageSize, eventType, targetType, actor, from, to],
    enabled: allowed,
    queryFn: async () => (await api.get<{ items: AuditRow[]; totalCount: number; totalPages: number }>("/api/admin/audit-logs", {
      params: { pageNumber: page, pageSize, eventType: eventType || undefined, targetType: targetType || undefined,
        actorUserId: actor || undefined, from: from ? new Date(from).toISOString() : undefined, to: to ? new Date(to).toISOString() : undefined },
    })).data,
  });
  const filters = [
    { label: "Olay türü", value: eventType, set: setEventType, type: "text" },
    { label: "Hedef türü", value: targetType, set: setTargetType, type: "text" },
    { label: "Kullanıcı kimliği", value: actor, set: setActor, type: "text" },
    { label: "Başlangıç", value: from, set: setFrom, type: "datetime-local" },
    { label: "Bitiş", value: to, set: setTo, type: "datetime-local" },
  ];
  return <div className="space-y-6 p-4 md:p-6">
    <h1 className="text-2xl font-semibold">Denetim kayıtları</h1>
    <ApiStateView isLoading={allowed && query.isLoading} isForbidden={!allowed || isForbiddenError(query.error)} error={query.error} onRetry={() => void query.refetch()} variant="page">
      <div className="mb-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-5">
        {filters.map(filter => <label key={filter.label} className="flex min-w-0 flex-col gap-1 text-sm">
          {filter.label}
          <input className="min-w-0 rounded border border-slate-600 bg-slate-900 p-2 text-slate-50" type={filter.type} value={filter.value} maxLength={100}
            onChange={event => { filter.set(event.target.value); setPage(1); }} />
        </label>)}
      </div>
      <DataTable data={query.data?.items ?? []} columns={columns} manualPagination pageIndex={page - 1} pageSize={pageSize}
        totalCount={query.data?.totalCount ?? 0} pageCount={query.data?.totalPages ?? 0} onPageChange={setPage}
        onPageSizeChange={size => { setPageSize(size > 0 ? Math.min(size, 100) : 100); setPage(1); }} canExport={false} canPrint={false} isLoading={query.isFetching} />
    </ApiStateView>
  </div>;
}
