import React from "react";
import { Paperclip, FileText, FileSpreadsheet, Image as ImageIcon, Send, Loader2, X } from "lucide-react";
import { cn } from "@/lib/utils";

interface ChatInputProps {
  messageType: number;
  setMessageType: (type: number) => void;
  newMessage: string;
  setNewMessage: (msg: string) => void;
  selectedFiles: File[];
  removeFile: (index: number) => void;
  isFileTypeMenuOpen: boolean;
  setIsFileTypeMenuOpen: (open: boolean) => void;
  handleFileSelect: (type: "pdf" | "excel" | "image") => void;
  onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  handleSendMessage: (e: React.FormEvent) => void;
  sending: boolean;
  fileInputRef: React.RefObject<HTMLInputElement | null>;
  menuRef: React.RefObject<HTMLDivElement | null>;
  acceptedType: string;
}

export const ChatInput: React.FC<ChatInputProps> = ({
  messageType,
  setMessageType,
  newMessage,
  setNewMessage,
  selectedFiles,
  removeFile,
  isFileTypeMenuOpen,
  setIsFileTypeMenuOpen,
  handleFileSelect,
  onFileChange,
  handleSendMessage,
  sending,
  fileInputRef,
  menuRef,
  acceptedType
}) => {
  return (
    <div className="p-4 md:p-6 bg-slate-950 border-t border-slate-800 shrink-0">
        {/* File Preview */}
        {selectedFiles.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-4 animate-in fade-in slide-in-from-bottom-2">
                {selectedFiles.map((file, index) => (
                    <div key={index} className="flex items-center gap-2 bg-indigo-500/10 text-indigo-300 px-2 md:px-3 py-1 md:py-1.5 rounded-lg md:rounded-xl text-[10px] md:text-xs font-bold border border-indigo-500/20">
                        {file.name.endsWith('.pdf') ? <FileText size={12} /> :
                            file.name.match(/\.(jpg|jpeg|png)$/i) ? <ImageIcon size={12} /> :
                                <FileSpreadsheet size={12} />}
                        <span className="max-w-[120px] md:max-w-[150px] truncate">{file.name}</span>
                        <button onClick={() => removeFile(index)} className="hover:text-red-400 transition-colors">
                            <X size={12} />
                        </button>
                    </div>
                ))}
            </div>
        )}

        <div className="flex items-center gap-2 md:gap-4 mb-4">
            <button
                type="button"
                onClick={() => setMessageType(0)}
                className={cn(
                    "flex-1 py-2.5 md:py-3 rounded-xl md:rounded-2xl text-[9px] md:text-[10px] font-black uppercase tracking-widest transition-all border-2",
                    messageType === 0 
                        ? "bg-indigo-600 border-indigo-600 text-white shadow-lg shadow-indigo-600/20" 
                        : "bg-transparent border-slate-700 text-slate-500 hover:border-slate-600"
                )}
            >
                Yanıt
            </button>
            <button
                type="button"
                onClick={() => setMessageType(1)}
                className={cn(
                    "flex-1 py-2.5 md:py-3 rounded-xl md:rounded-2xl text-[9px] md:text-[10px] font-black uppercase tracking-widest transition-all border-2",
                    messageType === 1 
                        ? "bg-amber-500 border-amber-500 text-white shadow-lg shadow-amber-500/20" 
                        : "bg-transparent border-slate-700 text-slate-500 hover:border-slate-600"
                )}
            >
                Dahili Not
            </button>
        </div>

        <form
            onSubmit={handleSendMessage}
            className={cn(
                "relative flex items-center gap-2 md:gap-3 p-1.5 md:p-2 pl-3 md:pl-4 rounded-2xl md:rounded-[1.5rem] border transition-all shadow-inner",
                messageType === 1 ? "bg-amber-500/5 border-amber-500/30 focus-within:border-amber-500" : "bg-slate-900 border-slate-700 focus-within:border-indigo-500"
            )}
        >
            <div className="relative" ref={menuRef}>
                <button
                    type="button"
                    onClick={() => setIsFileTypeMenuOpen(!isFileTypeMenuOpen)}
                    className="text-slate-500 hover:text-indigo-400 transition-colors w-8 h-8 md:w-10 md:h-10 flex items-center justify-center rounded-lg md:rounded-xl hover:bg-indigo-500/10"
                >
                    <Paperclip size={18} />
                </button>

                {isFileTypeMenuOpen && (
                    <div className="absolute bottom-full mb-4 left-0 bg-slate-900 border border-slate-700 rounded-2xl shadow-2xl p-2 w-48 animate-in fade-in zoom-in slide-in-from-bottom-4 z-50">
                        <button
                            type="button"
                            onClick={() => handleFileSelect("pdf")}
                            className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-red-500/10 text-slate-300 hover:text-red-400 transition-all text-xs font-bold"
                        >
                            <div className="w-8 h-8 rounded-lg bg-red-500/10 flex items-center justify-center text-red-400">
                                <FileText size={16} />
                            </div>
                            PDF Dosyası
                        </button>
                        <button
                            type="button"
                            onClick={() => handleFileSelect("excel")}
                            className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-emerald-500/10 text-slate-300 hover:text-emerald-400 transition-all text-xs font-bold"
                        >
                            <div className="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-400">
                                <FileSpreadsheet size={16} />
                            </div>
                            Excel Dosyası
                        </button>
                        <button
                            type="button"
                            onClick={() => handleFileSelect("image")}
                            className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-indigo-500/10 text-slate-300 hover:text-indigo-400 transition-all text-xs font-bold"
                        >
                            <div className="w-8 h-8 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-400">
                                <ImageIcon size={16} />
                            </div>
                            Görsel / Fotoğraf
                        </button>
                    </div>
                )}
            </div>

            <input
                type="file"
                ref={fileInputRef}
                className="hidden"
                accept={acceptedType}
                multiple
                onChange={onFileChange}
            />

            <input
                value={newMessage}
                onChange={(e) => setNewMessage(e.target.value)}
                placeholder={selectedFiles.length > 0 ? "Mesaj yazın..." : "Bir yanıt yazın..."}
                className="flex-1 bg-transparent border-none outline-none text-slate-200 text-[13px] md:text-sm font-bold placeholder:text-slate-500 py-2 md:py-3 min-w-0"
            />
            <button
                disabled={(!newMessage.trim() && selectedFiles.length === 0) || sending}
                className="bg-indigo-600 hover:bg-indigo-500 text-white w-10 h-10 md:w-12 md:h-12 rounded-xl md:rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-600/20 transition-all active:scale-95 disabled:opacity-50 shrink-0"
            >
                {sending ? <Loader2 className="animate-spin" size={18} /> : <Send size={18} />}
            </button>
        </form>
    </div>
  );
};
