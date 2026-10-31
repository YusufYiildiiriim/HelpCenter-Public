import React from "react";
import { Plus, CheckCircle2, ShieldCheck } from "lucide-react";
import { cn } from "@/lib/utils";
import type { CustomerRequest } from "@/services/customer/CustomerRequestService";
import { RequestCard } from "./RequestCard";
import { useLocale } from "@/context/LocaleContext";
import { RequestStatus } from "@/lib/requestStatus";

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
  const { t } = useLocale();
  const availableRequests = Array.isArray(requests) ? requests : [];
  const baseRequests = activeView === "requests"
    ? availableRequests.filter(r => r.statusId !== RequestStatus.Completed)
    : availableRequests.filter(r => r.statusId === RequestStatus.Completed);

  const filteredRequests = (activeView === "requests" && requestFilter !== "Hepsi"
    ? baseRequests.filter(r => r.statusId === requestFilter)
    : baseRequests
  ).sort((a, b) => b.createdAt.localeCompare(a.createdAt));

  return (
    <div className="space-y-8">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-slate-900/90 backdrop-blur-xl p-8 rounded-[2.5rem] border border-slate-800 shadow-xl shadow-slate-950/30">
            <div>
                <h2 className="text-2xl font-black text-white tracking-tight">
                    {activeView === "requests" ? t("activeSupportRequests") : t("completedRequests")}
                </h2>
                <p className="text-sm text-slate-400 font-medium mt-0.5">
                    {activeView === "requests" ? t("activeSupportDescription") : t("completedSupportDescription")}
                </p>
            </div>
            {activeView === "requests" && (
                <button 
                    type="button"
                    onClick={onNewRequest}
                    className="relative z-10 bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-4 rounded-2xl font-black shadow-xl shadow-indigo-600/20 flex items-center gap-3 transition-all active:scale-95"
                >
                    <Plus size={22} /> {t("createNewRequest")}
                </button>
            )}
        </div>

        {activeView === "requests" && (
            <div className="flex bg-slate-900/90 backdrop-blur-xl p-2 rounded-2xl border border-slate-800 shadow-xl shadow-slate-950/30 self-start overflow-x-auto no-scrollbar max-w-full">
                {[
                  { id: "Hepsi", label: t("allRequests") },
                  { id: RequestStatus.WaitingForReply, label: t("statusWaitingForReply") },
                  { id: RequestStatus.Replied, label: t("statusReplied") },
                ].map(({ id, label }) => (
                    <button 
                        key={id}
                        onClick={() => setRequestFilter(id)}
                        className={cn(
                            "px-6 py-2.5 rounded-xl text-xs font-black transition-all whitespace-nowrap",
                            requestFilter === id
                                ? id === RequestStatus.WaitingForReply ? "bg-amber-500 text-white shadow-lg shadow-amber-500/20" :
                                  id === RequestStatus.Replied ? "bg-indigo-500 text-white shadow-lg shadow-indigo-500/20" :
                                  "bg-indigo-600 text-white shadow-lg shadow-indigo-600/10"
                                : "text-slate-400 hover:text-white hover:bg-slate-800"
                        )}
                    >
                        {label}
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
                        {activeView === "history" ? t("noCompletedRequests") : t("noActiveRequests")}
                    </h3>
                    <p className="text-slate-400 mt-2 max-w-sm font-medium">{t("emptyRequestList")}</p>
                </div>
            )}
        </div>
    </div>
  );
};
