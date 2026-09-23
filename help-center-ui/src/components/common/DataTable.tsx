"use client";

import type { ReactNode } from "react";
import React, { useState, useEffect } from "react";
import type {
  ColumnDef,
  SortingState} from "@tanstack/react-table";
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  flexRender
} from "@tanstack/react-table";
import {
  FileSpreadsheet,
  FileText,
  ArrowUpDown,
  Search,
  Download,
  Check,
  X,
  Loader2,
  LayoutGrid,
  Table,
} from "lucide-react";
import { cn } from "@/lib/utils";
import type { ExportColumn } from "@/utils/csvExport";
import { exportToCsv } from "@/utils/csvExport";
import { exportToPdf } from "@/utils/pdfExport";
import { Pagination } from "./Pagination";

interface DataTableProps<TData> {
  data: TData[];
  columns: ColumnDef<TData, unknown>[];
  excelColumns?: ExportColumn<TData>[];
  excelFileName?: string;
  pdfFileName?: string;
  pdfTitle?: string;
  canExport?: boolean;
  canPrint?: boolean;
  searchPlaceholder?: string;
  pageSize?: number;
  emptyMessage?: string;
  isLoading?: boolean;
  toolbarActions?: ReactNode;
  onRowClick?: (row: TData) => void;
  // Manual / Server-side Pagination
  manualPagination?: boolean;
  pageCount?: number;
  totalCount?: number;
  pageIndex?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
  onSearchChange?: (search: string) => void;
  searchValue?: string;
  onFetchAllData?: () => Promise<TData[]>;
}

export function DataTable<TData extends object>({
  data,
  columns,
  excelColumns,
  excelFileName = "Rapor",
  pdfFileName = "Rapor",
  pdfTitle = "Rapor",
  canExport = true,
  canPrint = true,
  searchPlaceholder = "Ara...",
  pageSize = 10,
  emptyMessage = "Gösterilecek kayıt bulunamadı.",
  isLoading = false,
  toolbarActions,
  onRowClick,
  manualPagination = false,
  pageCount,
  totalCount,
  pageIndex = 0,
  onPageChange,
  onPageSizeChange,
  onSearchChange,
  searchValue,
  onFetchAllData,
}: DataTableProps<TData>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [internalGlobalFilter, setInternalGlobalFilter] = useState("");
  const [exportModalType, setExportModalType] = useState<"excel" | "pdf" | null>(null);
  const [exportScope, setExportScope] = useState<"page" | "all">("all");
  const [exporting, setExporting] = useState(false);
  const [mobileViewMode, setMobileViewMode] = useState<"cards" | "table">("cards");

  const globalFilter = searchValue !== undefined ? searchValue : internalGlobalFilter;

  const handleFilterChange = (value: string) => {
    if (onSearchChange) {
      onSearchChange(value);
    } else {
      setInternalGlobalFilter(value);
      if (!manualPagination && table.setPageIndex) {
        table.setPageIndex(0);
      }
    }
  };

  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data,
    columns,
    pageCount: manualPagination ? (pageCount ?? -1) : undefined,
    manualPagination,
    state: {
      sorting,
      globalFilter,
      ...(manualPagination
        ? {
            pagination: {
              pageIndex,
              pageSize,
            },
          }
        : {}),
    },
    onSortingChange: setSorting,
    onGlobalFilterChange: (val) => handleFilterChange(String(val)),
    getCoreRowModel: getCoreRowModel(),
    ...(!manualPagination ? { getPaginationRowModel: getPaginationRowModel() } : {}),
    getSortedRowModel: getSortedRowModel(),
    ...(!manualPagination ? { getFilteredRowModel: getFilteredRowModel() } : {}),
    initialState: {
      pagination: {
        pageSize: pageSize,
      },
    },
  });

  useEffect(() => {
    if (!manualPagination && table.getPageCount) {
      const pc = table.getPageCount();
      if (pc > 0 && table.getState().pagination.pageIndex >= pc) {
        table.setPageIndex(0);
      }
    }
  }, [data, manualPagination, table]);

  const openExportModal = (type: "excel" | "pdf") => {
    setExportModalType(type);
    setExportScope("all");
  };

  const executeExport = async () => {
    if (!exportModalType || !excelColumns || excelColumns.length === 0) return;
    setExporting(true);

    try {
      let exportData = data;
      if (exportScope === "all" && onFetchAllData) {
        exportData = await onFetchAllData();
      } else if (exportScope === "all" && !manualPagination) {
        exportData = table.getFilteredRowModel ? table.getFilteredRowModel().rows.map(r => r.original) : data;
      } else {
        exportData = data;
      }

      if (exportModalType === "excel") {
        exportToCsv(exportData, excelColumns, excelFileName);
      } else {
        exportToPdf(exportData, excelColumns, pdfFileName, pdfTitle);
      }
      setExportModalType(null);
    } catch (err) {
      console.error("Error exporting data:", err);
    } finally {
      setExporting(false);
    }
  };

  return (
    <div className="w-full space-y-4">
      {/* Executive Toolbar Header */}
      <div className="flex flex-col min-[1441px]:flex-row min-[1441px]:items-center justify-between gap-3.5 p-3.5 sm:p-4 rounded-2xl bg-slate-900/80 backdrop-blur-xl border border-slate-800/80 shadow-lg shadow-slate-950/40">
        {/* 1. Üstte: Arama & Mobil Switcher */}
        <div className="flex items-center gap-2.5 w-full min-[1441px]:max-w-md">
          <div className="relative flex-1 min-w-0">
            <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-indigo-400" />
            <input
              type="text"
              value={globalFilter ?? ""}
              onChange={(e) => handleFilterChange(e.target.value)}
              placeholder={searchPlaceholder}
              className="w-full pl-10 pr-4 py-2.5 text-xs sm:text-sm bg-slate-800/70 border border-slate-800 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/40 focus:border-indigo-500 text-slate-100 placeholder-slate-400 transition-all font-medium"
            />
          </div>

          {/* Mobile View Toggle Switcher */}
          <div className="md:hidden flex items-center p-1 bg-slate-800/90 rounded-xl border border-slate-700/80 shrink-0">
            <button
              type="button"
              onClick={() => setMobileViewMode("cards")}
              className={cn(
                "p-2 rounded-lg text-xs font-bold transition-all flex items-center gap-1",
                mobileViewMode === "cards"
                  ? "bg-indigo-600 text-white shadow-sm"
                  : "text-slate-400 hover:text-slate-200"
              )}
              title="Kart Görünümü"
            >
              <LayoutGrid size={15} />
            </button>
            <button
              type="button"
              onClick={() => setMobileViewMode("table")}
              className={cn(
                "p-2 rounded-lg text-xs font-bold transition-all flex items-center gap-1",
                mobileViewMode === "table"
                  ? "bg-indigo-600 text-white shadow-sm"
                  : "text-slate-400 hover:text-slate-200"
              )}
              title="Tablo Görünümü"
            >
              <Table size={15} />
            </button>
          </div>
        </div>

        {/* 2 & 3. Altında: Hazır Filtreler ve İndirme Butonları */}
        <div className="flex flex-col min-[1441px]:flex-row items-stretch min-[1441px]:items-center gap-3 w-full min-[1441px]:w-auto">
          {/* 2. Hazır Filtreler */}
          {toolbarActions && (
            <div className="w-full min-[1441px]:w-auto overflow-x-auto scrollbar-none">
              {toolbarActions}
            </div>
          )}

          {/* 3. Excel & PDF İndirme Butonları */}
          {(excelColumns && (canExport || canPrint)) && (
            <div className="flex items-center gap-2.5 w-full sm:w-auto">
              {canExport && (
                <button
                  type="button"
                  onClick={() => openExportModal("excel")}
                  className="flex-1 sm:flex-initial inline-flex items-center justify-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 text-xs sm:text-sm font-bold text-white bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 rounded-xl transition-all shadow-md shadow-emerald-600/20 active:scale-95 cursor-pointer border border-emerald-500/30 whitespace-nowrap min-w-0"
                  title="CSV İndir (.csv)"
                >
                  <FileSpreadsheet className="w-3.5 h-3.5 sm:w-4 sm:h-4 shrink-0 text-white" />
                  <span className="hidden sm:inline">CSV İndir (.csv)</span>
                  <span className="sm:hidden">CSV İndir</span>
                </button>
              )}

              {canPrint && (
                <button
                  type="button"
                  onClick={() => openExportModal("pdf")}
                  className="flex-1 sm:flex-initial inline-flex items-center justify-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 text-xs sm:text-sm font-bold text-white bg-gradient-to-r from-rose-600 to-red-600 hover:from-rose-500 hover:to-red-500 rounded-xl transition-all shadow-md shadow-rose-600/20 active:scale-95 cursor-pointer border border-rose-500/30 whitespace-nowrap min-w-0"
                  title="PDF İndir (.pdf)"
                >
                  <FileText className="w-3.5 h-3.5 sm:w-4 sm:h-4 shrink-0 text-white" />
                  <span className="hidden sm:inline">PDF İndir (.pdf)</span>
                  <span className="sm:hidden">PDF İndir</span>
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Main Container */}
      <div className="bg-slate-900/90 backdrop-blur-xl border border-slate-800/80 rounded-2xl overflow-hidden shadow-xl shadow-slate-950/50">
        
        {/* Mobile Cards View (Visible on mobile when mobileViewMode === 'cards') */}
        {mobileViewMode === "cards" && (
          <div className="block md:hidden p-3.5 space-y-3">
            {isLoading && table.getRowModel().rows.length === 0 ? (
              <div className="p-8 text-center text-slate-400 font-semibold text-xs">Yükleniyor...</div>
            ) : table.getRowModel().rows.length > 0 ? (
              table.getRowModel().rows.map((row, rowIndex) => (
                <div
                  key={row.id}
                  onClick={onRowClick ? () => onRowClick(row.original) : undefined}
                  className={cn(
                    "p-4 rounded-xl bg-slate-900/90 border border-slate-800/90 shadow-md space-y-3 transition-all",
                    onRowClick && "cursor-pointer active:scale-[0.99]",
                    isLoading && "opacity-40 pointer-events-none"
                  )}
                >
                  <div className="flex items-center justify-between border-b border-slate-800/80 pb-2.5">
                    <span className="text-[11px] font-mono font-bold text-slate-400 bg-slate-800 px-2.5 py-0.5 rounded-md">
                      #{manualPagination
                        ? pageIndex * pageSize + rowIndex + 1
                        : table.getState().pagination.pageIndex * table.getState().pagination.pageSize + rowIndex + 1}
                    </span>
                    {/* Render action or status if available */}
                    {row.getVisibleCells().length > 0 && (
                      <div className="text-xs font-bold text-indigo-400">
                        {flexRender(row.getVisibleCells()[0].column.columnDef.cell, row.getVisibleCells()[0].getContext())}
                      </div>
                    )}
                  </div>

                  <div className="space-y-2 text-xs">
                    {row.getVisibleCells().slice(1).map((cell) => (
                      <div key={cell.id} className="flex items-center justify-between gap-3 border-b border-slate-800/40 pb-2 last:border-0 last:pb-0">
                        <span className="text-[10px] font-bold uppercase tracking-wider text-slate-400 shrink-0">
                          {/* eslint-disable-next-line @typescript-eslint/no-explicit-any */}
                          {flexRender(cell.column.columnDef.header, cell.getContext() as any)}
                        </span>
                        <div className="text-slate-200 text-right min-w-0 font-medium overflow-hidden">
                          {flexRender(cell.column.columnDef.cell, cell.getContext())}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))
            ) : (
              <div className="p-8 text-center text-slate-400 font-semibold text-xs">{emptyMessage}</div>
            )}
          </div>
        )}

        {/* Standard Table View (Visible on desktop or when mobileViewMode === 'table') */}
        <div className={cn("overflow-x-auto scrollbar-thin scrollbar-thumb-slate-700/50", mobileViewMode === "cards" ? "hidden md:block" : "block")}>
          <table className="w-full min-w-[650px] text-left text-xs text-slate-200 sm:text-sm">
            <thead className="border-b border-slate-700/80 bg-slate-800/80 text-[11px] font-bold uppercase tracking-[0.08em] text-slate-300">
              {table.getHeaderGroups().map((headerGroup) => (
                <tr key={headerGroup.id}>
                  <th className="px-4 py-4 w-12 text-center">#</th>
                  {headerGroup.headers.map((header) => (
                    <th key={header.id} className="px-5 py-4 whitespace-nowrap">
                      {header.isPlaceholder ? null : (
                        <div
                          className={
                            header.column.getCanSort()
                              ? "cursor-pointer select-none flex items-center gap-1.5 hover:text-indigo-400 transition-colors"
                              : ""
                          }
                          onClick={header.column.getToggleSortingHandler()}
                        >
                          {flexRender(
                            header.column.columnDef.header,
                            header.getContext()
                          )}
                          {header.column.getCanSort() && (
                            <ArrowUpDown className="w-3.5 h-3.5 opacity-50" />
                          )}
                        </div>
                      )}
                    </th>
                  ))}
                </tr>
              ))}
            </thead>
            <tbody className="relative divide-y divide-slate-800/60 font-medium leading-5">
              {isLoading && table.getRowModel().rows.length === 0 ? (
                <tr>
                  <td
                    colSpan={columns.length + 1}
                    className="px-5 py-14 text-center text-slate-400 font-semibold"
                  >
                    Yükleniyor...
                  </td>
                </tr>
              ) : table.getRowModel().rows.length > 0 ? (
                table.getRowModel().rows.map((row, rowIndex) => (
                  <tr
                    key={row.id}
                    onClick={onRowClick ? () => onRowClick(row.original) : undefined}
                    className={cn(
                      "hover:bg-slate-800/50 transition-colors duration-150",
                      onRowClick && "cursor-pointer",
                      isLoading && "opacity-40 pointer-events-none"
                    )}
                  >
                    <td className="w-12 px-4 py-4 text-center font-semibold text-slate-400">
                      {manualPagination
                        ? pageIndex * pageSize + rowIndex + 1
                        : table.getState().pagination.pageIndex * table.getState().pagination.pageSize + rowIndex + 1}
                    </td>
                    {row.getVisibleCells().map((cell) => (
                      <td key={cell.id} className="whitespace-nowrap px-5 py-4 text-slate-200">
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext()
                        )}
                      </td>
                    ))}
                  </tr>
                ))
              ) : (
                <tr>
                  <td
                    colSpan={columns.length + 1}
                    className="px-5 py-14 text-center text-slate-400 font-semibold"
                  >
                    {emptyMessage}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        <Pagination
          currentPage={manualPagination ? pageIndex + 1 : table.getState().pagination.pageIndex + 1}
          totalPages={manualPagination ? (pageCount || 1) : (table.getPageCount() || 1)}
          totalItems={manualPagination ? (totalCount ?? data.length) : table.getFilteredRowModel().rows.length}
          pageSize={manualPagination ? pageSize : table.getState().pagination.pageSize}
          onPageChange={(page) => {
            if (manualPagination && onPageChange) {
              onPageChange(page);
            } else {
              table.setPageIndex(page - 1);
            }
          }}
          onPageSizeChange={(size) => {
            if (manualPagination && onPageSizeChange) {
              onPageSizeChange(size);
            } else {
              table.setPageSize(size);
              table.setPageIndex(0);
            }
          }}
          containerClassName="w-full flex flex-col sm:flex-row items-center justify-between gap-4 px-5 py-4 bg-slate-800/60 border-t border-slate-800/80 text-xs font-medium text-slate-300"
        />
      </div>

      {/* Export Options Modal */}
      {exportModalType && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/75 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-slate-900 border border-slate-800 w-full max-w-md rounded-2xl shadow-2xl overflow-hidden z-10 animate-in zoom-in-95 duration-200">
            {/* Modal Header */}
            <div className="px-6 py-5 bg-gradient-to-r from-slate-900 via-indigo-950/40 to-slate-900 border-b border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div
                  className={cn(
                    "w-10 h-10 rounded-xl flex items-center justify-center text-white shadow-lg",
                    exportModalType === "excel"
                      ? "bg-gradient-to-br from-emerald-500 to-teal-600 shadow-emerald-500/20"
                      : "bg-gradient-to-br from-rose-500 to-red-600 shadow-rose-500/20"
                  )}
                >
                  {exportModalType === "excel" ? <FileSpreadsheet size={20} /> : <FileText size={20} />}
                </div>
                <div>
                  <h3 className="text-sm sm:text-base font-black text-slate-100 tracking-tight">
                    {exportModalType === "excel" ? "CSV İndirme Kapsamı" : "PDF İndirme Kapsamı"}
                  </h3>
                  <p className="text-[11px] font-medium text-slate-400">Dışa aktarılacak veri kümesini seçin</p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => !exporting && setExportModalType(null)}
                className="w-8 h-8 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white flex items-center justify-center transition-colors"
              >
                <X size={16} />
              </button>
            </div>

            {/* Modal Body */}
            <div className="p-6 space-y-3.5">
              <button
                type="button"
                onClick={() => setExportScope("all")}
                className={cn(
                  "w-full p-4 rounded-xl border text-left cursor-pointer transition-all flex items-start gap-3.5",
                  exportScope === "all"
                    ? "bg-indigo-600/15 border-indigo-500/50 text-white shadow-md shadow-indigo-600/10"
                    : "bg-slate-800/40 border-slate-800 text-slate-400 hover:bg-slate-800/80 hover:border-slate-700"
                )}
              >
                <div
                  className={cn(
                    "w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0 mt-0.5 transition-colors",
                    exportScope === "all" ? "border-indigo-500 bg-indigo-600 text-white" : "border-slate-600"
                  )}
                >
                  {exportScope === "all" && <Check size={12} strokeWidth={3} />}
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between">
                    <p className="text-xs font-black text-slate-100 uppercase tracking-wider">Tüm Sistem Verileri</p>
                    <span className="text-[10px] font-extrabold px-2 py-0.5 rounded-md bg-indigo-500/20 text-indigo-300 font-mono">
                      {totalCount ?? data.length} Kayıt
                    </span>
                  </div>
                  <p className="text-[11px] font-medium text-slate-400 mt-1">
                    Filtrelenmiş veya mevcut olan tüm kayıtların tamamını dışa aktarır.
                  </p>
                </div>
              </button>

              <button
                type="button"
                onClick={() => setExportScope("page")}
                className={cn(
                  "w-full p-4 rounded-xl border text-left cursor-pointer transition-all flex items-start gap-3.5",
                  exportScope === "page"
                    ? "bg-indigo-600/15 border-indigo-500/50 text-white shadow-md shadow-indigo-600/10"
                    : "bg-slate-800/40 border-slate-800 text-slate-400 hover:bg-slate-800/80 hover:border-slate-700"
                )}
              >
                <div
                  className={cn(
                    "w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0 mt-0.5 transition-colors",
                    exportScope === "page" ? "border-indigo-500 bg-indigo-600 text-white" : "border-slate-600"
                  )}
                >
                  {exportScope === "page" && <Check size={12} strokeWidth={3} />}
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between">
                    <p className="text-xs font-black text-slate-100 uppercase tracking-wider">Aktif Ekran Verileri</p>
                    <span className="text-[10px] font-extrabold px-2 py-0.5 rounded-md bg-slate-700 text-slate-300 font-mono">
                      {data.length} Kayıt
                    </span>
                  </div>
                  <p className="text-[11px] font-medium text-slate-400 mt-1">
                    Sadece ekranda görünen mevcut sayfanın kayıtlarını dışa aktarır.
                  </p>
                </div>
              </button>
            </div>

            {/* Modal Footer */}
            <div className="px-6 py-4 bg-slate-900/80 border-t border-slate-800 flex items-center justify-end gap-3">
              <button
                type="button"
                onClick={() => setExportModalType(null)}
                disabled={exporting}
                className="px-4 py-2.5 rounded-xl border border-slate-700 text-xs font-bold text-slate-300 hover:bg-slate-800 transition-colors"
              >
                İptal
              </button>
              <button
                type="button"
                onClick={executeExport}
                disabled={exporting}
                className={cn(
                  "inline-flex items-center justify-center gap-2 px-5 py-2.5 text-xs font-black text-white rounded-xl transition-all shadow-md active:scale-95 cursor-pointer",
                  exportModalType === "excel"
                    ? "bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 shadow-emerald-600/20"
                    : "bg-gradient-to-r from-rose-600 to-red-600 hover:from-rose-500 hover:to-red-500 shadow-rose-600/20"
                )}
              >
                {exporting ? (
                  <>
                    <Loader2 size={14} className="animate-spin" />
                    <span>Hazırlanıyor...</span>
                  </>
                ) : (
                  <>
                    <Download size={14} />
                    <span>Dışa Aktar ({exportScope === "all" ? "Tüm Veriler" : "Aktif Sayfa"})</span>
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
