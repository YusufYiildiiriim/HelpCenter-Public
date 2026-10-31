import React from "react";
import { CheckCircle2, Star, Loader2, Save } from "lucide-react";
import { cn } from "@/lib/utils";
import { useLocale } from "@/context/LocaleContext";

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
  const { t } = useLocale();
  if (!isOpen) return null;

  return (
    <div role="dialog" aria-modal="true" aria-labelledby="close-request-title" className="fixed inset-0 bg-slate-950/80 backdrop-blur-md z-[100] flex items-center justify-center p-6 animate-in fade-in duration-300">
        <div className="bg-slate-900/95 backdrop-blur-xl rounded-[3rem] shadow-2xl w-full max-w-md overflow-hidden relative border border-slate-800">
            <div className="p-8 border-b border-slate-800 flex justify-between items-center bg-slate-900/50 text-center flex-col gap-2">
                <div className="w-16 h-16 bg-emerald-500/10 text-emerald-400 rounded-2xl flex items-center justify-center mb-2 border border-emerald-500/20">
                    <CheckCircle2 size={32} />
                </div>
                <h2 id="close-request-title" className="text-2xl font-black text-white tracking-tight">{t("rateServiceTitle")}</h2>
                <p className="text-sm text-slate-400 font-medium px-4">{t("serviceRatingPrompt")}</p>
            </div>

            <div className="p-8 space-y-8">
                <div className="flex flex-col items-center gap-4">
                    <div className="flex gap-2">
                        {[1, 2, 3, 4, 5].map((s) => (
                            <button
                                key={s}
                                aria-label={`${t("rating")} ${s}`}
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
                        {rating === 1 ? t("ratingVeryBad") :
                         rating === 2 ? t("ratingBad") :
                         rating === 3 ? t("ratingAverage") :
                         rating === 4 ? t("ratingGood") : t("ratingExcellent")}
                    </span>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">{t("feedbackOptional")}</label>
                    <textarea
                        rows={3}
                        value={closingNote}
                        onChange={(e) => setClosingNote(e.target.value)}
                        className="w-full px-5 py-4 bg-slate-800 border border-slate-700 rounded-2xl text-white focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all font-medium resize-none text-sm placeholder:text-slate-500"
                        placeholder={t("feedbackPlaceholder")}
                    />
                </div>

                <div className="flex gap-4 pt-2">
                    <button
                        type="button"
                        onClick={onClose}
                        className="flex-1 px-6 py-4 rounded-xl font-black text-slate-400 hover:bg-slate-800 hover:text-white transition-colors text-sm"
                    >
                        {t("cancelAction")}
                    </button>
                    <button
                        onClick={onConfirm}
                        disabled={isClosing}
                        className="flex-[2] bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-xl shadow-xl shadow-indigo-600/20 transition-all active:scale-95 disabled:opacity-50 flex items-center justify-center gap-3 text-sm"
                    >
                        {isClosing ? <Loader2 className="animate-spin" size={20} /> : (
                            <><Save size={18} /> {t("saveAndClose")}</>
                        )}
                    </button>
                </div>
            </div>
        </div>
    </div>
  );
};
