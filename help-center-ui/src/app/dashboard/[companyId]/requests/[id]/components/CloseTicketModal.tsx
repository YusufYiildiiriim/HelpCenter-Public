import React from "react";
import { CheckCircle2, Star, Loader2, Save } from "lucide-react";
import { cn } from "@/lib/utils";

interface CloseTicketModalProps {
  isOpen: boolean;
  onClose: () => void;
  rating: number;
  setRating: (rating: number) => void;
  closingNote: string;
  setClosingNote: (note: string) => void;
  onConfirm: () => void;
  isClosing: boolean;
}

export const CloseTicketModal: React.FC<CloseTicketModalProps> = ({
  isOpen,
  onClose,
  rating,
  setRating,
  closingNote,
  setClosingNote,
  onConfirm,
  isClosing
}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-md z-[100] flex items-center justify-center p-6 animate-in fade-in duration-300">
        <div className="bg-slate-900/95 backdrop-blur-xl rounded-[3rem] shadow-2xl w-full max-w-md overflow-hidden relative border border-slate-800">
            <div className="p-8 border-b border-slate-800 flex justify-between items-center bg-slate-900/50 text-center flex-col gap-2">
                <div className="w-16 h-16 bg-emerald-500/10 text-emerald-400 rounded-2xl flex items-center justify-center mb-2 border border-emerald-500/20">
                    <CheckCircle2 size={32} />
                </div>
                <h2 className="text-2xl font-black text-white tracking-tight">Talebi Sonuçlandır</h2>
                <p className="text-sm text-slate-400 font-medium px-4">Size verdiğimiz hizmeti nasıl değerlendirirsiniz?</p>
            </div>

            <div className="p-8 space-y-8">
                <div className="flex flex-col items-center gap-4">
                    <div className="flex gap-2">
                        {[1, 2, 3, 4, 5].map((s) => (
                            <button
                                key={s}
                                onClick={() => setRating(s)}
                                className="transition-all active:scale-90"
                            >
                                <Star 
                                    size={36} 
                                    className={cn(
                                        "transition-all duration-300",
                                        s <= rating ? "fill-amber-400 text-amber-400 scale-110 drop-shadow-md" : "text-slate-600"
                                    )} 
                                />
                            </button>
                        ))}
                    </div>
                    <span className="text-xs font-black text-amber-400 uppercase tracking-widest">
                        {rating === 1 ? "Çok Kötü" : 
                         rating === 2 ? "Kötü" : 
                         rating === 3 ? "Orta" : 
                         rating === 4 ? "İyi" : "Mükemmel"}
                    </span>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Geri Bildiriminiz (Opsiyonel)</label>
                    <textarea
                        rows={3}
                        value={closingNote}
                        onChange={(e) => setClosingNote(e.target.value)}
                        className="w-full px-5 py-4 bg-slate-800 border border-slate-700 rounded-2xl text-white focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all font-medium resize-none text-sm placeholder:text-slate-500"
                        placeholder="Eklemek istediğiniz bir not var mı?"
                    />
                </div>

                <div className="flex gap-4 pt-2">
                    <button
                        type="button"
                        onClick={onClose}
                        className="flex-1 px-6 py-4 rounded-xl font-black text-slate-400 hover:bg-slate-800 hover:text-white transition-colors text-sm"
                    >
                        Vazgeç
                    </button>
                    <button
                        onClick={onConfirm}
                        disabled={isClosing}
                        className="flex-[2] bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-xl shadow-xl shadow-indigo-600/20 transition-all active:scale-95 disabled:opacity-50 flex items-center justify-center gap-3 text-sm"
                    >
                        {isClosing ? <Loader2 className="animate-spin" size={20} /> : (
                            <><Save size={18} /> Kaydet ve Kapat</>
                        )}
                    </button>
                </div>
            </div>
        </div>
    </div>
  );
};
