import React from "react";
import { UserPlus, X, CheckCircle2, Info, Loader2, Send } from "lucide-react";
import type { ModuleExpertDto } from "@/services/admin/AdminModuleExpertService";
import { cn } from "@/lib/utils";

interface ConsultModalProps {
  onClose: () => void;
  experts: ModuleExpertDto[];
  selectedExpertId: number | null;
  setSelectedExpertId: (id: number) => void;
  consultNote: string;
  setConsultNote: (note: string) => void;
  onConsult: () => void;
  isConsulting: boolean;
}

export const ConsultModal: React.FC<ConsultModalProps> = ({
  onClose,
  experts,
  selectedExpertId,
  setSelectedExpertId,
  consultNote,
  setConsultNote,
  onConsult,
  isConsulting
}) => {
  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-in fade-in duration-300">
        <div className="bg-slate-950 w-full max-w-lg rounded-[2.5rem] border border-slate-800 shadow-2xl overflow-hidden animate-in zoom-in slide-in-from-bottom-10 duration-500">
            <div className="p-8 border-b border-slate-800 flex items-center justify-between bg-slate-900">
                <div>
                    <h3 className="text-xl font-black text-white tracking-tight flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-indigo-600 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20">
                            <UserPlus size={20} />
                        </div>
                        Uzmana Danış
                    </h3>
                    <p className="text-xs text-slate-500 font-medium mt-1">Talebi teknik inceleme için bir uzmana yönlendirin.</p>
                </div>
                <button onClick={onClose} className="w-10 h-10 rounded-full hover:bg-slate-800 flex items-center justify-center text-slate-500 transition-colors">
                    <X size={20} />
                </button>
            </div>
            <div className="p-8 space-y-6">
                <div className="space-y-3">
                    <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1">Bir Uzman Seçin</label>
                    <div className="grid grid-cols-1 gap-2 max-h-48 overflow-y-auto pr-2 custom-scrollbar">
                        {experts.length > 0 ? experts.map((expert) => (
                            <button
                                key={expert.userId}
                                onClick={() => setSelectedExpertId(expert.userId)}
                                className={cn(
                                    "flex items-center justify-between p-4 rounded-2xl border-2 transition-all text-left",
                                    selectedExpertId === expert.userId 
                                        ? "bg-indigo-500/10 border-indigo-500 text-indigo-300" 
                                        : "bg-slate-900 border-transparent text-slate-400 hover:border-slate-700"
                                )}
                            >
                                <div className="flex items-center gap-3">
                                    <div className="w-8 h-8 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-400 font-black text-[10px]">
                                        {expert.fullName.substring(0, 2).toUpperCase()}
                                    </div>
                                    <div>
                                        <p className="text-[11px] font-black uppercase tracking-tight text-slate-300">{expert.fullName}</p>
                                        <p className="text-[9px] text-slate-500 font-medium">{expert.email}</p>
                                    </div>
                                </div>
                                {selectedExpertId === expert.userId && <CheckCircle2 size={16} className="text-indigo-400" />}
                            </button>
                        )) : (
                            <div className="p-8 text-center bg-slate-900 rounded-2xl border border-dashed border-slate-700">
                                <Info size={24} className="text-slate-600 mx-auto mb-2" />
                                <p className="text-[10px] font-bold text-slate-500">Bu modül için atanmış uzman bulunmuyor.</p>
                            </div>
                        )}
                    </div>
                </div>
                <div className="space-y-3">
                    <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1">Uzmana Notunuz</label>
                    <textarea
                        value={consultNote}
                        onChange={(e) => setConsultNote(e.target.value)}
                        placeholder="Teknik detaylar veya uzmana sorunuz..."
                        className="w-full bg-slate-900 border border-slate-700 rounded-2xl p-4 text-sm font-bold text-slate-200 placeholder-slate-500 outline-none focus:border-indigo-500/30 transition-all min-h-[120px] resize-none shadow-inner"
                    />
                </div>
            </div>
            <div className="p-8 bg-slate-950 border-t border-slate-800 flex gap-3">
                <button
                    onClick={onClose}
                    className="flex-1 py-4 rounded-2xl text-[10px] font-black uppercase tracking-widest text-slate-500 hover:bg-slate-800 border border-slate-700 transition-all"
                >
                    Vazgeç
                </button>
                <button
                    onClick={onConsult}
                    disabled={isConsulting || !selectedExpertId || !consultNote.trim()}
                    className="flex-[2] py-4 rounded-2xl bg-indigo-600 text-white text-[10px] font-black uppercase tracking-widest shadow-lg shadow-indigo-600/20 hover:bg-indigo-500 transition-all disabled:opacity-50 active:scale-95 flex items-center justify-center gap-2"
                >
                    {isConsulting ? <Loader2 className="animate-spin" size={16} /> : <Send size={16} />}
                    Yönlendirmeyi Başlat
                </button>
            </div>
        </div>
    </div>
  );
};
