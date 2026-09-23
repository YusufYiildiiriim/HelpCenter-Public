import React from "react";
import { Clock, MessageSquare, History, User, BookOpen } from "lucide-react";
import { cn } from "@/lib/utils";

interface DashboardSidebarProps {
  activeView: "requests" | "history" | "profile" | "guides";
  setActiveView: (view: "requests" | "history" | "profile" | "guides") => void;
  timeLeft: number;
  formatTime: (seconds: number) => string;
}

export const DashboardSidebar: React.FC<DashboardSidebarProps> = ({ 
  activeView, 
  setActiveView, 
  timeLeft, 
  formatTime 
}) => {
  return (
    <aside className="w-full md:w-80 space-y-6 shrink-0">
        <div className="bg-slate-900/90 backdrop-blur-xl rounded-[2rem] p-8 border border-slate-800 shadow-xl shadow-slate-950/30 space-y-6 relative overflow-hidden group">
            <div className="absolute top-0 right-0 w-24 h-24 bg-indigo-600/10 -mr-12 -mt-12 rounded-full opacity-50 group-hover:scale-150 transition-transform duration-500"></div>
            <h3 className="text-[10px] font-black text-slate-400 uppercase tracking-widest relative">Oturum Süresi</h3>
            <div className="bg-indigo-500/10 border border-indigo-500/20 rounded-2xl p-5 relative">
                <div className="flex items-center gap-3 text-indigo-300">
                    <Clock size={20} />
                    <span className="font-black text-lg">{formatTime(timeLeft)}</span>
                </div>
                <p className="text-[10px] text-indigo-400 mt-2 font-bold leading-relaxed">Güvenlik gereği 3 dakika hareketsiz kalırsanız oturumunuz kapatılacaktır.</p>
            </div>
        </div>

        <div className="bg-slate-900/90 backdrop-blur-xl rounded-[2rem] p-8 border border-slate-800 shadow-xl shadow-slate-950/30 space-y-6">
            <h3 className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hızlı Erişim</h3>
            <nav className="space-y-2">
                <button 
                    onClick={() => setActiveView("requests")}
                    className={cn(
                        "w-full flex items-center gap-4 p-4 rounded-2xl font-bold text-sm transition-all",
                        activeView === "requests" ? "bg-linear-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20" : "text-slate-400 hover:bg-slate-800 hover:text-white"
                    )}
                >
                    <MessageSquare size={18} /> Taleplerim
                </button>
                <button 
                    onClick={() => setActiveView("history")}
                    className={cn(
                        "w-full flex items-center gap-4 p-4 rounded-2xl font-bold text-sm transition-all",
                        activeView === "history" ? "bg-linear-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20" : "text-slate-400 hover:bg-slate-800 hover:text-white"
                    )}
                >
                    <History size={18} /> İşlem Geçmişi
                </button>
                <button
                    onClick={() => setActiveView("guides")}
                    className={cn(
                        "w-full flex items-center gap-4 p-4 rounded-2xl font-bold text-sm transition-all",
                        activeView === "guides" ? "bg-linear-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20" : "text-slate-400 hover:bg-slate-800 hover:text-white"
                    )}
                >
                    <BookOpen size={18} /> Rehberler
                </button>
                <button
                    onClick={() => setActiveView("profile")}
                    className={cn(
                        "w-full flex items-center gap-4 p-4 rounded-2xl font-bold text-sm transition-all",
                        activeView === "profile" ? "bg-linear-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20" : "text-slate-400 hover:bg-slate-800 hover:text-white"
                    )}
                >
                    <User size={18} /> Profilim
                </button>
            </nav>
        </div>
    </aside>
  );
};
