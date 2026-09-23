import React from "react";
import { Paperclip, FileText, FileSpreadsheet, Image as ImageIcon, Send, Loader2, X, CheckCircle2 } from "lucide-react";

interface ChatInputAreaProps {
  status?: string;
  selectedFiles: File[];
  removeFile: (index: number) => void;
  isFileTypeMenuOpen: boolean;
  setIsFileTypeMenuOpen: (open: boolean) => void;
  handleFileSelect: (type: "pdf" | "excel" | "image") => void;
  fileInputRef: React.RefObject<HTMLInputElement | null>;
  acceptedType: string;
  onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  newMessage: string;
  setNewMessage: (msg: string) => void;
  onSendMessage: (e: React.FormEvent) => void;
  sending: boolean;
  menuRef: React.RefObject<HTMLDivElement | null>;
}

export const ChatInputArea: React.FC<ChatInputAreaProps> = ({
  status,
  selectedFiles,
  removeFile,
  isFileTypeMenuOpen,
  setIsFileTypeMenuOpen,
  handleFileSelect,
  fileInputRef,
  acceptedType,
  onFileChange,
  newMessage,
  setNewMessage,
  onSendMessage,
  sending,
  menuRef
}) => {
  if (status === "Tamamlandı") {
    return (
      <div className="p-8 bg-slate-900/50 border-t border-slate-800 text-center">
          <p className="text-slate-400 font-bold text-sm uppercase tracking-widest italic flex items-center justify-center gap-3">
              <CheckCircle2 size={18} />
              Bu talep sonuçlandırıldığı için yeni mesaj gönderilemez.
          </p>
      </div>
    );
  }

  return (
    <div className="p-6 bg-slate-900/90 backdrop-blur-xl border-t border-slate-800">
        {/* File Preview */}
        {selectedFiles.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-4 animate-in fade-in slide-in-from-bottom-2">
                {selectedFiles.map((file, index) => (
                    <div key={index} className="flex items-center gap-2 bg-indigo-500/10 text-indigo-300 px-3 py-1.5 rounded-xl text-xs font-bold border border-indigo-500/20">
                        {file.name.endsWith('.pdf') ? <FileText size={14} /> : 
                         file.name.match(/\.(jpg|jpeg|png)$/i) ? <ImageIcon size={14} /> :
                         <FileSpreadsheet size={14} />}
                        <span className="max-w-[150px] truncate">{file.name}</span>

                        <button onClick={() => removeFile(index)} className="hover:text-red-400 transition-colors">
                            <X size={14} />
                        </button>
                    </div>
                ))}
            </div>
        )}

        <form 
            onSubmit={onSendMessage}
            className="flex items-center gap-3 bg-slate-800 p-2 pl-4 rounded-[1.5rem] border border-slate-700 focus-within:border-indigo-500 transition-all shadow-inner relative"
        >
            <div className="relative" ref={menuRef}>
                <button 
                    type="button"
                    onClick={() => setIsFileTypeMenuOpen(!isFileTypeMenuOpen)}
                    className="w-10 h-10 rounded-xl flex items-center justify-center text-slate-400 hover:text-indigo-400 hover:bg-indigo-500/10 transition-all"
                >
                    <Paperclip size={20} />
                </button>

                {isFileTypeMenuOpen && (
                    <div className="absolute bottom-full mb-4 left-0 bg-slate-800 border border-slate-700 rounded-2xl shadow-2xl p-2 w-48 animate-in fade-in zoom-in slide-in-from-bottom-4 z-50">
                        <div className="p-2 mb-1 border-b border-slate-700">
                            <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest px-2">Dosya Türü Seçin</span>
                        </div>
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
                placeholder={selectedFiles.length > 0 ? "Mesajınızı buraya yazın veya gönderin..." : "Mesajınızı buraya yazın..."}
                className="flex-1 bg-transparent border-none outline-none text-white text-sm font-bold placeholder:text-slate-500 py-3"
            />
            <button 
                disabled={(!newMessage.trim() && selectedFiles.length === 0) || sending}
                className="bg-indigo-600 hover:bg-indigo-500 text-white w-12 h-12 rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-600/20 transition-all active:scale-95 disabled:opacity-50"
            >
                {sending ? <Loader2 className="animate-spin" size={20} /> : <Send size={20} />}
            </button>
        </form>
    </div>
  );
};
