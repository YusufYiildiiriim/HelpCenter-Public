import React from "react";
import { BookOpen, Sparkles, Edit3, Trash2, ChevronRight, PlayCircle, FileText } from "lucide-react";
import { motion } from "framer-motion";
import type { Guide } from "@/services/admin/AdminGuideService";
import { cn } from "@/lib/utils";

interface GuideCardProps {
  guide: Guide;
  index: number;
  onEdit: (guide: Guide) => void;
  onDelete: (id: number) => void;
}

export const GuideCard: React.FC<GuideCardProps> = ({ guide, index, onEdit, onDelete }) => {
  const stripHtml = (html: string) => {
    if (typeof document === "undefined") return html;
    const tmp = document.createElement("DIV");
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || "";
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.98 }}
      transition={{ delay: index * 0.03 }}
      className={cn(
        "group bg-white rounded-3xl border border-slate-200 p-5 hover:shadow-lg hover:shadow-slate-200/50 transition-all duration-300 flex flex-col gap-4 relative overflow-hidden",
        !guide.isActive && "opacity-80"
      )}
    >
      {/* Header: Icon & Actions */}
      <div className="flex items-start justify-between">
        <div className={cn(
          "w-10 h-10 rounded-xl flex items-center justify-center transition-all duration-300 border relative shrink-0",
          guide.isActive 
            ? "bg-slate-50 text-slate-400 group-hover:bg-blue-600 group-hover:text-white border-slate-100" 
            : "bg-slate-100 text-slate-300 border-slate-200"
        )}>
          <BookOpen size={20} className="transition-transform duration-300" />
          {guide.isActive && <div className="absolute -top-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-white bg-green-500 shadow-sm" />}
        </div>

        <div className="flex items-center gap-1 bg-slate-50 p-1 rounded-lg border border-slate-100">
          <button 
            onClick={() => onEdit(guide)}
            className="p-1.5 text-slate-400 hover:text-blue-600 transition-all"
            title="Düzenle"
          >
            <Edit3 size={16} />
          </button>
          <button 
            onClick={() => onDelete(guide.id)}
            className="p-1.5 text-slate-400 hover:text-red-600 transition-all"
            title="Sil"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>

      {/* Content: Title & Badges */}
      <div className="min-w-0">
        <div className="flex flex-wrap items-center gap-1.5 mb-2">
          <span className="text-[8px] font-black uppercase tracking-wider bg-slate-900 text-white px-2 py-0.5 rounded-md">
            {guide.module}
          </span>
          {guide.previousGuideId && (
            <span className="text-[8px] font-black uppercase tracking-wider bg-blue-50 text-blue-600 px-2 py-0.5 rounded-md border border-blue-100 flex items-center gap-1">
               <Sparkles size={8} className="fill-blue-600" /> Bağlantılı
            </span>
          )}
        </div>
        <h3 className="text-lg font-black text-slate-900 tracking-tight leading-tight group-hover:text-blue-600 transition-colors line-clamp-2">
          {guide.title}
        </h3>
      </div>

      {/* Description */}
      <div className="relative">
        <p className="text-xs text-slate-500 font-medium leading-relaxed line-clamp-2">
          {stripHtml(guide.description)}
        </p>
      </div>

      {/* Footer: Metadata & Link */}
      <div className="flex items-center justify-between gap-4 pt-4 mt-auto border-t border-slate-50">
        <div className="flex items-center gap-2">
          {guide.youtubeUrl && (
            <div className="flex items-center justify-center w-6 h-6 bg-amber-50 rounded-lg border border-amber-100 text-amber-600" title="Video Mevcut">
              <PlayCircle size={14} className="fill-amber-600" />
            </div>
          )}
          {(guide.documents && guide.documents.length > 0) && (
            <div className="flex items-center gap-1.5 text-[9px] font-black text-blue-600 uppercase tracking-widest bg-blue-50 px-2 py-1 rounded-lg border border-blue-100">
              <FileText size={12} className="fill-blue-600" />
              {guide.documents.length}
            </div>
          )}
        </div>

        <button 
          onClick={() => onEdit(guide)}
          className="flex items-center gap-1 text-[9px] font-black text-slate-900 uppercase tracking-widest hover:text-blue-600 transition-colors group/link"
        >
          Yönet
          <ChevronRight size={12} className="group-hover/link:translate-x-1 transition-transform" />
        </button>
      </div>
    </motion.div>
  );
};
