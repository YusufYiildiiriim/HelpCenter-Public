import React from "react";
import { Lock, FileText, Image as ImageIcon, FileSpreadsheet, CheckCheck, Check } from "lucide-react";
import type { AdminTicketMessage } from "@/services/admin/AdminRequestService";
import { cn } from "@/lib/utils";
import { toApiUrl } from "@/lib/env";

interface MessageBubbleProps {
  msg: AdminTicketMessage;
}

export const MessageBubble: React.FC<MessageBubbleProps> = ({ msg }) => {
  return (
    <div
        className={cn(
            "flex flex-col max-w-[90%] md:max-w-[80%] animate-in fade-in slide-in-from-bottom-2 transition-all",
            msg.isAgent ? "ml-auto items-end" : "mr-auto items-start"
        )}
    >
        <div className="flex items-center gap-2 mb-1 px-1">
            <span className="text-[9px] md:text-[10px] font-black text-slate-500 uppercase tracking-tighter truncate max-w-[150px]">
                {msg.senderName}
            </span>
            <span className="text-[9px] md:text-[10px] text-slate-600 font-medium shrink-0">
                {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
            </span>
            {msg.isAgent && msg.type !== 1 && (
                msg.isRead
                    ? <CheckCheck size={12} className="text-indigo-400 shrink-0" />
                    : <Check size={12} className="text-slate-500 shrink-0" />
            )}
        </div>
        <div
            className={cn(
                "p-3 md:p-4 rounded-xl md:rounded-2xl text-[13px] md:text-sm font-medium leading-relaxed shadow-sm relative",
                msg.isAgent
                    ? msg.type === 1 
                        ? "bg-amber-500/10 text-amber-300 border-2 border-amber-500/30 rounded-tr-none"
                        : "bg-indigo-600 text-white rounded-tr-none shadow-indigo-600/10"
                    : "bg-slate-800 text-slate-300 border border-slate-700 rounded-tl-none"
            )}
        >
            {msg.type === 1 && (
                <div className="flex items-center gap-1 text-[8px] md:text-[9px] font-black uppercase tracking-widest text-amber-400 mb-2 border-b border-amber-500/20 pb-1">
                    <Lock size={10} />
                    Dahili Not
                </div>
            )}
            <div className="break-words">
                {msg.messageText}
            </div>

            {msg.documents && msg.documents.length > 0 && (
                <div className="mt-3 md:mt-4 pt-3 md:pt-4 border-t border-white/10 space-y-2">
                    <p className="text-[8px] md:text-[10px] font-black uppercase tracking-widest opacity-50 mb-1">Ekli Dosyalar</p>
                    <div className="flex flex-wrap gap-2">
                        {msg.documents.map((doc) => (
                            <a
                                key={doc.id}
                                href={toApiUrl(doc.path)}
                                target="_blank"
                                rel="noopener noreferrer"
                                className={cn(
                                    "flex items-center gap-2 px-2 md:px-3 py-1.5 md:py-2 rounded-lg md:rounded-xl text-[10px] md:text-xs font-bold transition-all border",
                                    msg.isAgent
                                        ? "bg-white/10 border-white/10 hover:bg-white/20 text-white"
                                        : "bg-slate-900 border-slate-700 hover:bg-indigo-500/10 hover:border-indigo-500/30 text-indigo-400"
                                )}
                            >
                                {doc.fileName.endsWith('.pdf') ? <FileText size={12} /> :
                                    doc.fileName.match(/\.(jpg|jpeg|png)$/i) ? <ImageIcon size={12} /> :
                                        <FileSpreadsheet size={12} />}
                                <span className="max-w-[100px] md:max-w-[120px] truncate">{doc.fileName}</span>
                            </a>
                        ))}
                    </div>
                </div>
            )}
        </div>
    </div>
  );
};
