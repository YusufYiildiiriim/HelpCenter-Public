import React from "react";
import { MessageSquare, FileText, Image as ImageIcon, FileSpreadsheet } from "lucide-react";
import { cn } from "@/lib/utils";
import { toApiUrl } from "@/lib/env";
import type { TicketMessage } from "@/services/customer/CustomerRequestService";

interface ChatMessageListProps {
  messages: TicketMessage[];
  messagesEndRef: React.RefObject<HTMLDivElement | null>;
}

export const ChatMessageList: React.FC<ChatMessageListProps> = ({
  messages,
  messagesEndRef
}) => {
  return (
    <div className="flex-1 overflow-y-auto p-8 space-y-6 bg-slate-950/50">
        {messages.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-full text-slate-600 gap-4">
                <MessageSquare size={48} className="opacity-20" />
                <p className="font-bold">Henüz mesaj bulunmuyor.</p>
            </div>
        ) : (
            messages.map((msg) => (
                <div 
                    key={msg.id} 
                    className={cn(
                        "flex flex-col max-w-[80%] animate-in fade-in slide-in-from-bottom-2 transition-all",
                        !msg.isAgent ? "ml-auto items-end" : "mr-auto items-start"
                    )}
                >
                    <div className="flex items-center gap-2 mb-1.5 px-1">
                        <span className="text-[10px] font-black text-slate-400 uppercase tracking-tighter">
                            {msg.senderName}
                        </span>
                        <span className="text-[10px] text-slate-500 font-medium">
                            {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </span>
                    </div>
                    <div 
                        className={cn(
                            "p-4 rounded-2xl text-sm font-medium leading-relaxed shadow-sm",
                            !msg.isAgent 
                                ? "bg-indigo-600 text-white rounded-tr-none" 
                                : "bg-slate-800 text-slate-200 border border-slate-700 rounded-tl-none"
                        )}
                    >
                    {msg.messageText}

                    {msg.documents && msg.documents.length > 0 && (
                        <div className="mt-4 pt-4 border-t border-slate-100/20 space-y-2">
                            <p className="text-[10px] font-black uppercase tracking-widest opacity-50 mb-2">Ekli Dosyalar</p>
                            <div className="flex flex-wrap gap-2">
                                {msg.documents.map((doc) => (
                                    <a 
                                        key={doc.id}
                                        href={toApiUrl(doc.path)}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className={cn(
                                            "flex items-center gap-2 px-3 py-2 rounded-xl text-xs font-bold transition-all border",
                                            !msg.isAgent 
                                                ? "bg-white/10 border-white/10 hover:bg-white/20 text-white" 
                                                : "bg-slate-700 border-slate-600 hover:bg-indigo-500/10 hover:border-indigo-500/30 text-indigo-400"
                                        )}
                                    >
                                        {doc.fileName.endsWith('.pdf') ? <FileText size={14} /> : 
                                         doc.fileName.match(/\.(jpg|jpeg|png)$/i) ? <ImageIcon size={14} /> :
                                         <FileSpreadsheet size={14} />}
                                        <span className="max-w-[120px] truncate">{doc.fileName}</span>
                                    </a>
                                ))}
                            </div>
                        </div>
                    )}
                </div>

                </div>
            ))
        )}
        <div ref={messagesEndRef} />
    </div>
  );
};
