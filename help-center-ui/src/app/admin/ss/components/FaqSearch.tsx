import React from "react";
import { Search } from "lucide-react";

interface FaqSearchProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
}

export const FaqSearch: React.FC<FaqSearchProps> = ({ searchTerm, onSearchChange }) => {
  return (
    <div className="rounded-2xl border border-slate-100 bg-slate-50/70 p-3">
      <div className="relative max-w-xl">
        <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
        <input
          type="text"
          placeholder="Sorularda veya içeriklerde ara..."
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          className="h-11 w-full rounded-xl border border-slate-200 bg-white pl-10 pr-4 text-sm font-bold text-slate-700 outline-none shadow-sm transition-all placeholder:text-slate-300 focus:border-blue-500 focus:ring-4 focus:ring-blue-500/5"
        />
      </div>
    </div>
  );
};
