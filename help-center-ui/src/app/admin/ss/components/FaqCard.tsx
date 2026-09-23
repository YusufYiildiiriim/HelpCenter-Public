import React from "react";
import { Edit2, HelpCircle, Trash2 } from "lucide-react";
import type { Faq } from "@/services/admin/AdminFaqService";
import { cn } from "@/lib/utils";

interface FaqCardProps {
  faq: Faq;
  onEdit: (faq: Faq) => void;
  onDelete: (id: number) => void;
}

export const FaqCard: React.FC<FaqCardProps> = ({ faq, onEdit, onDelete }) => {
  return (
    <div
      className={cn(
        "group rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:border-blue-200 hover:shadow-[0_12px_28px_rgba(15,23,42,0.06)]",
        !faq.isActive && "opacity-75"
      )}
    >
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div className="flex min-w-0 flex-1 items-start gap-3">
          <div
            className={cn(
              "flex h-10 w-10 shrink-0 items-center justify-center rounded-xl border transition-all",
              faq.isActive
                ? "border-blue-100 bg-blue-50 text-blue-600"
                : "border-slate-200 bg-slate-100 text-slate-300"
            )}
          >
            <HelpCircle size={19} />
          </div>

          <div className="min-w-0 flex-1">
            <div className="mb-1.5 flex flex-wrap items-center gap-2">
              <span className="text-[10px] font-black uppercase tracking-wider text-slate-400">Sıkça Sorulan Soru</span>
              <span
                className={cn(
                  "rounded-md border px-2 py-0.5 text-[9px] font-black uppercase tracking-wider",
                  faq.isActive
                    ? "border-emerald-200 bg-emerald-50 text-emerald-700"
                    : "border-slate-200 bg-slate-50 text-slate-500"
                )}
              >
                {faq.isActive ? "Yayında" : "Taslak"}
              </span>
              {faq.id && <span className="text-[10px] font-bold text-slate-300">#{faq.id}</span>}
            </div>

            <h3 className="truncate text-base font-black leading-tight text-slate-900 transition-colors group-hover:text-blue-700">
              {faq.title}
            </h3>
            <p className="mt-1 line-clamp-2 text-sm font-medium leading-5 text-slate-500">
              {faq.description}
            </p>
          </div>
        </div>

        <div className="flex shrink-0 items-center justify-end gap-2 border-t border-slate-100 pt-3 md:border-t-0 md:pt-0">
          <button
            onClick={() => onEdit(faq)}
            className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-50 text-slate-500 transition-all hover:bg-blue-600 hover:text-white active:scale-95"
            title="Düzenle"
          >
            <Edit2 size={16} />
          </button>
          <button
            onClick={() => onDelete(faq.id!)}
            className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-50 text-slate-500 transition-all hover:bg-red-600 hover:text-white active:scale-95"
            title="Sil"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>
    </div>
  );
};
