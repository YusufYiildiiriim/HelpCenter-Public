import React, { useState, useRef, useEffect } from "react";
import { LayoutGrid, ChevronDown, Check } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import type { Module } from "@/services/admin/AdminModuleService";
import { cn } from "@/lib/utils";

interface GuideFiltersProps {
  selectedModule: string;
  setSelectedModule: (module: string) => void;
  modules: Module[];
}

export const GuideFilters: React.FC<GuideFiltersProps> = ({
  selectedModule,
  setSelectedModule,
  modules
}) => {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const safeModules = Array.isArray(modules) ? modules : [];

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsDropdownOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <motion.div 
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-slate-50/50 p-6 md:p-8 rounded-2xl border border-slate-200/80 mb-10"
    >
      <div className="flex flex-col lg:flex-row gap-6 items-stretch">
        {/* Custom Premium Dropdown */}
        <div className="lg:w-80 relative" ref={dropdownRef}>
          <button
            onClick={() => setIsDropdownOpen(!isDropdownOpen)}
            className={cn(
              "w-full flex items-center justify-between px-6 py-5 bg-white border rounded-xl transition-all duration-300 shadow-sm outline-none group",
              isDropdownOpen ? "border-indigo-500 ring-8 ring-indigo-500/5" : "border-slate-200 hover:border-slate-300"
            )}
          >
            <div className="flex items-center gap-3">
              <div className={cn(
                "w-8 h-8 rounded-lg flex items-center justify-center transition-colors",
                isDropdownOpen ? "bg-indigo-600 text-white" : "bg-slate-50 text-slate-400 group-hover:bg-slate-100"
              )}>
                <LayoutGrid size={16} />
              </div>
              <div className="text-left">
                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Kategori</p>
                <p className="font-bold text-slate-700 text-sm truncate max-w-[150px]">
                  {selectedModule === "All" ? "Tüm Kategoriler" : selectedModule}
                </p>
              </div>
            </div>
            <ChevronDown className={cn("text-slate-400 transition-transform duration-300", isDropdownOpen && "rotate-180")} size={20} />
          </button>

          <AnimatePresence>
            {isDropdownOpen && (
              <motion.div
                initial={{ opacity: 0, y: 10, scale: 0.95 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: 10, scale: 0.95 }}
                className="absolute top-full left-0 right-0 mt-3 bg-white border border-slate-200/80 rounded-2xl shadow-2xl z-[50] overflow-hidden p-2"
              >
                <div className="max-h-[300px] overflow-y-auto custom-scrollbar">
                  <button
                    onClick={() => { setSelectedModule("All"); setIsDropdownOpen(false); }}
                    className={cn(
                      "w-full flex items-center justify-between px-5 py-4 rounded-xl transition-all mb-1",
                      selectedModule === "All" ? "bg-indigo-50 text-indigo-600" : "hover:bg-slate-50 text-slate-600"
                    )}
                  >
                    <span className="font-bold text-sm">Tüm Kategoriler</span>
                    {selectedModule === "All" && <Check size={18} />}
                  </button>
                  
                  {safeModules.map((m) => (
                    <button
                      key={m.id}
                      onClick={() => { setSelectedModule(m.name); setIsDropdownOpen(false); }}
                      className={cn(
                        "w-full flex items-center justify-between px-5 py-4 rounded-xl transition-all mb-1 text-left",
                        selectedModule === m.name ? "bg-slate-900 text-white" : "hover:bg-slate-50 text-slate-600"
                      )}
                    >
                      <span className="font-bold text-sm truncate">{m.name}</span>
                      {selectedModule === m.name && <Check size={18} className="text-indigo-500" />}
                    </button>
                  ))}
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>
    </motion.div>
  );
};
