import React from "react";
import { Plus, CheckCircle2, ShieldCheck } from "lucide-react";
import { cn } from "@/lib/utils";
import type { CustomerRequest } from "@/services/customer/CustomerRequestService";
import { RequestCard } from "./RequestCard";

interface RequestsViewProps {
  activeView: "requests" | "history";
  requests: CustomerRequest[];
  requestFilter: string;
  setRequestFilter: (filter: string) => void;
  onNewRequest: () => void;
  companyId: string;
}

export const RequestsView: React.FC<RequestsViewProps> = ({
  activeView,
  requests,
  requestFilter,
  setRequestFilter,
  onNewRequest,
  companyId
}) => {
  const baseRequests = activeView === "requests"
    ? requests.filter(r => r.status !== "Tamamlandı")
    : requests.filter(r => r.status === "Tamamlandı");

  const filteredRequests = (activeView === "requests" && requestFilter !== "Hepsi"
    ? baseRequests.filter(r => r.status === requestFilter)
    : baseRequests
  ).sort((a, b) => b.id - a.id);

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-slate-900/90 backdrop-blur-xl p-8 rounded-[2.5rem] border border-slate-800 shadow-xl shadow-slate-950/30">
            <div>
                <h2 className="text-2xl font-black text-white tracking-tight">
                    {activeView === "requests" ? "Aktif Destek Talepleri" : "Tamamlanan Talepler"}
                </h2>
                <p className="text-sm text-slate-400 font-medium mt-0.5">
                    {activeView === "requests" ? "Şu an sistemde işlem gören destek kayıtlarınız." : "Geçmişte sonuçlandırılmış destek kayıtlarınız."}
                </p>
            </div>
            {activeView === "requests" && (
                <button 
                    onClick={onNewRequest}
                    className="bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-4 rounded-2xl font-black shadow-xl shadow-indigo-600/20 flex items-center gap-3 transition-all active:scale-95"
                >
                    <Plus size={22} /> Yeni Talep Oluştur
                </button>
            )}
        </div>

        {activeView === "requests" && (
            <div className="flex bg-slate-900/90 backdrop-blur-xl p-2 rounded-2xl border border-slate-800 shadow-xl shadow-slate-950/30 self-start overflow-x-auto no-scrollbar max-w-full">
                {["Hepsi", "Cevap Bekliyor", "Cevaplandı"].map((f) => (
                    <button 
                        key={f}
                        onClick={() => setRequestFilter(f)}
                        className={cn(
                            "px-6 py-2.5 rounded-xl text-xs font-black transition-all whitespace-nowrap",
                            requestFilter === f 
                                ? f === "Cevap Bekliyor" ? "bg-amber-500 text-white shadow-lg shadow-amber-500/20" :
                                  f === "Cevaplandı" ? "bg-indigo-500 text-white shadow-lg shadow-indigo-500/20" :
                                  "bg-indigo-600 text-white shadow-lg shadow-indigo-600/10"
                                : "text-slate-400 hover:text-white hover:bg-slate-800"
                        )}
                    >
                        {f}
                    </button>
                ))}
            </div>
        )}

        <div className="grid grid-cols-1 gap-6">
            {filteredRequests.map((request) => (
                <RequestCard key={request.id} request={request} companyId={companyId} />
            ))}

            {filteredRequests.length === 0 && (
                <div className="bg-slate-900/50 rounded-[3rem] p-24 border-2 border-dashed border-slate-800 flex flex-col items-center justify-center text-center">
                    <div className="w-24 h-24 bg-slate-800 rounded-4xl flex items-center justify-center text-slate-600 shadow-sm mb-6">
                        {activeView === "history" ? <CheckCircle2 size={48} /> : <ShieldCheck size={48} />}
                    </div>
                    <h3 className="text-2xl font-black text-white">
                        {activeView === "history" ? "Tamamlanan Talep Bulunmuyor" : "Aktif Talebiniz Bulunmuyor"}
                    </h3>
                    <p className="text-slate-400 mt-2 max-w-sm font-medium">Bu bölümde henüz listelenecek bir kayıt bulunmamaktadır.</p>
                </div>
            )}
        </div>
    </div>
  );
};
