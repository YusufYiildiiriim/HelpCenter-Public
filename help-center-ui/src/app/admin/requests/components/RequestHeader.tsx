import React from "react";
import { Building2, Search, ChevronRight, Filter, TicketCheck } from "lucide-react";
type Company = { publicId: string; name: string };
import { cn } from "@/lib/utils";

interface RequestHeaderProps {
  selectedCompanyId: string;
  setSelectedCompanyId: (id: string) => void;
  companies: Company[];
  isCompanyOpen: boolean;
  setIsCompanyOpen: (isOpen: boolean) => void;
  filter: string;
  setFilter: (filter: string) => void;
}

export const RequestHeader: React.FC<RequestHeaderProps> = ({
  selectedCompanyId,
  setSelectedCompanyId,
  companies,
  isCompanyOpen,
  setIsCompanyOpen,
  filter,
  setFilter
}) => {
  return (
    <div className="relative rounded-2xl bg-slate-900/95 p-6 md:p-8 text-white shadow-xl shadow-slate-950/20 backdrop-blur-xl border border-slate-800 z-20">
      {/* Animated background glows */}
      <div className="pointer-events-none absolute -top-24 -right-24 h-72 w-72 rounded-full bg-gradient-to-br from-indigo-600/30 to-violet-600/20 blur-3xl animate-pulse" />
      <div className="pointer-events-none absolute -bottom-20 -left-16 h-64 w-64 rounded-full bg-indigo-600/10 blur-3xl" />

      <div className="relative z-10 flex flex-col xl:flex-row justify-between items-start xl:items-center gap-6">
        <div className="flex items-center gap-4">
          <div className="relative group shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-70 group-hover:opacity-100 transition duration-500" />
            <div className="relative flex h-12 w-12 sm:h-14 sm:w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 text-white shadow-xl shadow-indigo-600/30 transition-transform duration-500 group-hover:scale-105">
              <TicketCheck className="h-6 w-6 sm:h-7 sm:w-7" />
            </div>
          </div>
          <div>
            <h1 className="text-xl sm:text-2xl md:text-3xl font-black tracking-tight text-white">Müşteri Talepleri</h1>
            <p className="mt-1 text-xs sm:text-sm font-medium text-slate-300 max-w-xl">Tüm firmalardan gelen destek ve yardım taleplerini yönetin.</p>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-4 w-full xl:w-auto relative z-10">
          <div className="relative">
            <button
              onClick={() => setIsCompanyOpen(!isCompanyOpen)}
              className={cn(
                "flex items-center justify-between w-full sm:w-auto gap-3 px-5 py-3 rounded-xl transition-all duration-300 border font-bold text-[10px] md:text-xs uppercase tracking-wider group",
                selectedCompanyId !== "all"
                  ? "bg-gradient-to-r from-indigo-600 to-violet-600 border-indigo-500 text-white shadow-lg shadow-indigo-600/30"
                  : "bg-white/10 border-slate-700 text-slate-200 hover:border-indigo-500 hover:bg-white/15"
              )}
            >
              <div className="flex items-center gap-3">
                <Building2 size={18} className={cn(selectedCompanyId === "all" ? "text-slate-400 group-hover:text-indigo-400" : "text-white/80")} />
                <span className="truncate max-w-37.5">
                  {selectedCompanyId === "all" ? "Tüm Firmalar" : companies.find(c => c.publicId === selectedCompanyId)?.name}
                </span>
              </div>
              <ChevronRight size={16} className={cn("transition-transform duration-300", isCompanyOpen ? "rotate-90" : "")} />
            </button>

            {isCompanyOpen && (
              <>
                <div className="fixed inset-0 z-40" onClick={() => setIsCompanyOpen(false)}></div>
                <div className="absolute right-0 left-0 sm:left-auto sm:right-0 mt-3 w-full sm:w-72 bg-slate-900 border border-slate-700 rounded-2xl shadow-2xl shadow-slate-950/60 p-3 z-50 animate-in fade-in zoom-in slide-in-from-top-2 duration-300">
                  <div className="relative mb-3">
                    <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500" size={14} />
                    <input
                      type="text"
                      placeholder="Firma Ara..."
                      className="w-full pl-10 pr-4 py-2.5 bg-slate-800 border border-slate-700 rounded-xl text-[11px] font-bold text-slate-200 outline-none focus:border-indigo-500/50 transition-all placeholder:text-slate-500"
                      onClick={(e) => e.stopPropagation()}
                      onChange={(e) => {
                        const search = e.target.value.toLowerCase();
                        const items = document.querySelectorAll('.company-item');
                        items.forEach((item: Element) => {
                          const name = item.getAttribute('data-name')?.toLowerCase() || "";
                          (item as HTMLElement).style.display = name.includes(search) ? "flex" : "none";
                        });
                      }}
                    />
                  </div>
                  <div className="max-h-60 sm:max-h-72 overflow-y-auto custom-scrollbar space-y-1">
                    <button
                      onClick={() => {
                        setSelectedCompanyId("all");
                        setIsCompanyOpen(false);
                      }}
                      className={cn(
                        "w-full flex items-center gap-3 px-4 py-3 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all company-item",
                        selectedCompanyId === "all"
                          ? "bg-indigo-600 text-white"
                          : "text-slate-400 hover:bg-slate-800 hover:text-white"
                      )}
                      data-name="Tüm Firmalar"
                    >
                      <div className="w-8 h-8 rounded-lg bg-indigo-500/20 flex items-center justify-center text-indigo-300">
                        <Filter size={14} />
                      </div>
                      TÜM FİRMALAR
                    </button>
                    {companies.map((company) => (
                      <button
                        key={company.publicId}
                        onClick={() => {
                          setSelectedCompanyId(company.publicId);
                          setIsCompanyOpen(false);
                        }}
                        className={cn(
                          "w-full flex items-center gap-3 px-4 py-3 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all text-left company-item",
                          selectedCompanyId === company.publicId
                            ? "bg-indigo-600 text-white"
                            : "text-slate-400 hover:bg-slate-800 hover:text-white"
                        )}
                        data-name={company.name}
                      >
                        <div className="w-8 h-8 rounded-lg bg-slate-800 flex items-center justify-center text-[10px] font-black text-slate-300 shrink-0 group-hover:bg-white transition-colors">
                          {company.name.substring(0, 2).toUpperCase()}
                        </div>
                        <span className="truncate flex-1">{company.name}</span>
                      </button>
                    ))}
                  </div>
                </div>
              </>
            )}
          </div>

          <div className="flex bg-white/10 p-1 rounded-xl border border-slate-700/60 overflow-x-auto no-scrollbar whitespace-nowrap">
            {["Hepsi", "Cevap Bekliyor", "Cevaplandı", "Teknik İncelemede", "Tamamlandı"].map((f) => (
              <button
                key={f}
                onClick={() => setFilter(f)}
                className={cn(
                  "px-4 md:px-5 py-2.5 rounded-lg text-[9px] md:text-[10px] font-black uppercase tracking-widest transition-all duration-300",
                  filter === f
                    ? f === "Cevap Bekliyor" ? "bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-lg shadow-amber-500/30" :
                      f === "Cevaplandı" ? "bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/30" :
                      f === "Teknik İncelemede" ? "bg-gradient-to-r from-sky-500 to-blue-600 text-white shadow-lg shadow-sky-500/30" :
                      f === "Tamamlandı" ? "bg-gradient-to-r from-emerald-500 to-green-600 text-white shadow-lg shadow-emerald-500/30" :
                      "bg-white text-slate-900 shadow-md"
                    : "text-slate-400 hover:text-white hover:bg-white/10"
                )}
              >
                {f}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};
