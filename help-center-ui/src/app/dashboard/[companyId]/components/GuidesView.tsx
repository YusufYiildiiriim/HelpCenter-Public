"use client";

import React, { useState } from "react";
import { BookOpen, ChevronRight, PlayCircle, FileText, Search, X, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
import type { CustomerGuide } from "@/services/customer/CustomerGuideService";

interface GuidesViewProps {
  guides: CustomerGuide[];
}

interface GuideDetailModalProps {
  guide: CustomerGuide;
  guides: CustomerGuide[];
  onClose: () => void;
}

const GuideDetailModal: React.FC<GuideDetailModalProps> = ({ guide, guides, onClose }) => {
  const [current, setCurrent] = useState(guide);

  const next = guides.find(g => g.previousGuidePublicId === current.publicId);
  const prev = guides.find(g => g.publicId === current.previousGuidePublicId);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
      <div className="bg-slate-900/95 backdrop-blur-xl rounded-3xl shadow-2xl w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden border border-slate-800">
        <div className="flex items-center justify-between p-6 border-b border-slate-800">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-indigo-600 rounded-xl flex items-center justify-center">
              <BookOpen size={18} className="text-white" />
            </div>
            <div>
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{current.module}</p>
              <h2 className="text-lg font-black text-white leading-tight">{current.title}</h2>
            </div>
          </div>
          <button
            onClick={onClose}
            className="w-10 h-10 flex items-center justify-center rounded-xl text-slate-400 hover:text-red-400 hover:bg-red-500/10 transition-all"
          >
            <X size={20} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-6">
          {current.youtubeUrl && (
            <a
              href={current.youtubeUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-2 text-sm font-bold text-red-400 bg-red-500/10 border border-red-500/20 rounded-2xl px-4 py-3 mb-5 hover:bg-red-500/20 transition-all w-fit"
            >
              <PlayCircle size={18} className="fill-red-400" />
              Video Rehberi İzle
            </a>
          )}

          <div
            className="prose prose-sm max-w-none text-slate-300 leading-relaxed prose-headings:text-white prose-strong:text-white prose-a:text-indigo-400"
            dangerouslySetInnerHTML={{ __html: current.description }}
          />

          {current.documents && current.documents.length > 0 && (
            <div className="mt-6 space-y-2">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-3">Ekler</p>
              {current.documents.map((doc, i) => (
                <a
                  key={i}
                  href={doc.path}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-3 p-3 bg-slate-800 border border-slate-700 rounded-xl hover:border-indigo-500/50 hover:bg-indigo-500/10 transition-all"
                >
                  <FileText size={16} className="text-indigo-400 shrink-0" />
                  <span className="text-sm font-bold text-slate-300 truncate">{doc.fileName}</span>
                </a>
              ))}
            </div>
          )}
        </div>

        <div className="flex items-center justify-between p-6 border-t border-slate-800 bg-slate-900/50">
          <button
            onClick={() => prev && setCurrent(prev)}
            disabled={!prev}
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm border border-slate-700 text-slate-400 hover:bg-slate-800 hover:text-white disabled:opacity-40 disabled:cursor-not-allowed transition-all"
          >
            <ChevronDown className="rotate-90" size={16} />
            Önceki Adım
          </button>
          <button
            onClick={() => next && setCurrent(next)}
            disabled={!next}
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm bg-indigo-600 text-white hover:bg-indigo-500 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
          >
            Sonraki Adım
            <ChevronDown className="-rotate-90" size={16} />
          </button>
        </div>
      </div>
    </div>
  );
};

export const GuidesView: React.FC<GuidesViewProps> = ({ guides }) => {
  const [search, setSearch] = useState("");
  const [selectedModule, setSelectedModule] = useState("Hepsi");
  const [selectedGuide, setSelectedGuide] = useState<CustomerGuide | null>(null);

  const modules = ["Hepsi", ...Array.from(new Set(guides.map(g => g.module)))];

  const filtered = guides.filter(g => {
    const matchSearch =
      g.title.toLowerCase().includes(search.toLowerCase()) ||
      g.description.toLowerCase().includes(search.toLowerCase());
    const matchModule = selectedModule === "Hepsi" || g.module === selectedModule;
    return matchSearch && matchModule;
  });

  return (
    <>
      {selectedGuide && (
        <GuideDetailModal
          guide={selectedGuide}
          guides={guides}
          onClose={() => setSelectedGuide(null)}
        />
      )}

      <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
        <div className="bg-slate-900/90 backdrop-blur-xl p-8 rounded-[2.5rem] border border-slate-800 shadow-xl shadow-slate-950/30 space-y-5">
          <div>
            <h2 className="text-2xl font-black text-white tracking-tight">Rehberler</h2>
            <p className="text-sm text-slate-400 font-medium mt-0.5">Ürün ve hizmetlerimize ait kullanım rehberleri.</p>
          </div>
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="relative flex-1">
              <Search size={16} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                value={search}
                onChange={e => setSearch(e.target.value)}
                placeholder="Rehberlerde ara..."
                className="w-full pl-10 pr-4 py-3 bg-slate-800 border border-slate-700 rounded-2xl text-sm font-medium text-white placeholder:text-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 transition-all"
              />
            </div>
            <div className="flex bg-slate-800 p-1.5 rounded-2xl border border-slate-700 gap-1 overflow-x-auto no-scrollbar">
              {modules.map(m => (
                <button
                  key={m}
                  onClick={() => setSelectedModule(m)}
                  className={cn(
                    "px-4 py-2 rounded-xl text-xs font-black transition-all whitespace-nowrap",
                    selectedModule === m
                      ? "bg-indigo-600 text-white shadow-lg shadow-indigo-600/20"
                      : "text-slate-400 hover:text-white hover:bg-slate-700"
                  )}
                >
                  {m}
                </button>
              ))}
            </div>
          </div>
        </div>

        {filtered.length === 0 ? (
          <div className="bg-slate-900/90 backdrop-blur-xl rounded-3xl border border-slate-800 p-12 text-center">
            <BookOpen size={40} className="text-slate-600 mx-auto mb-4" />
            <p className="text-slate-400 font-bold">Rehber bulunamadı.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {filtered.map(guide => (
              <button
                key={guide.publicId}
                onClick={() => setSelectedGuide(guide)}
                className={cn(
                  "bg-slate-900/90 backdrop-blur-xl rounded-3xl border border-slate-800 p-5 text-left hover:shadow-lg hover:border-indigo-500/50 transition-all group space-y-3"
                )}
              >
                <div className="flex items-start justify-between">
                  <span className="text-[9px] font-black uppercase tracking-wider bg-indigo-600 text-white px-2.5 py-1 rounded-lg">
                    {guide.module}
                  </span>
                  <div className="flex items-center gap-2">
                    {guide.youtubeUrl && (
                      <PlayCircle size={16} className="text-red-400 fill-red-400" />
                    )}
                    {guide.documents && guide.documents.length > 0 && (
                      <span className="text-[9px] font-black text-indigo-400 bg-indigo-500/10 border border-indigo-500/20 px-2 py-0.5 rounded-lg flex items-center gap-1">
                        <FileText size={11} />
                        {guide.documents.length}
                      </span>
                    )}
                  </div>
                </div>

                <h3 className="font-black text-white text-sm leading-tight group-hover:text-indigo-400 transition-colors line-clamp-2">
                  {guide.title}
                </h3>

                <div className="flex items-center justify-between pt-2 border-t border-slate-800">
                  <span className="text-[10px] text-slate-500 font-medium">
                    {guide.previousGuidePublicId ? "Zincirli rehber" : "Bağımsız rehber"}
                  </span>
                  <ChevronRight size={14} className="text-slate-500 group-hover:text-indigo-400 group-hover:translate-x-1 transition-all" />
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    </>
  );
};
