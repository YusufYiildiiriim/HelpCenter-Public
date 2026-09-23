import React from "react";
import { cn } from "@/lib/utils";

interface CompanyFiltersProps {
  filterType: "All" | "Active" | "Demo";
  onFilterChange: (type: "All" | "Active" | "Demo") => void;
}

export const CompanyFilters: React.FC<CompanyFiltersProps> = ({ filterType, onFilterChange }) => {
  return (
    <div className="w-full min-[1441px]:w-auto">
      <div className="flex w-full min-[1441px]:w-auto bg-slate-900/90 p-1 rounded-xl border border-slate-800 shadow-sm overflow-x-auto scrollbar-none">
        {(["All", "Active", "Demo"] as const).map((type) => (
          <button
            key={type}
            onClick={() => onFilterChange(type)}
            className={cn(
              "flex-1 min-[1441px]:flex-initial px-3 sm:px-4 py-2 sm:py-2.5 rounded-lg text-[10px] sm:text-[11px] font-bold uppercase tracking-wider transition-all whitespace-nowrap shrink-0 cursor-pointer text-center",
              filterType === type
                ? "bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/30"
                : "text-slate-400 hover:text-white hover:bg-white/10"
            )}
          >
            {type === "All" ? "Hepsi" : type === "Active" ? "Demo Talebi" : "Demo Kullanıcısı"}
          </button>
        ))}
      </div>
    </div>
  );
};
