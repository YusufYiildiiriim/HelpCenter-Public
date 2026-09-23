import React from "react";
import { Info } from "lucide-react";

export const StatusInfoBox: React.FC = () => {
  return (
    <div className="bg-slate-900 p-10 rounded-[3rem] text-white overflow-hidden relative group">
        <div className="absolute right-0 top-0 w-96 h-96 bg-indigo-600 rounded-full blur-[120px] opacity-20 -translate-y-1/2 translate-x-1/2 group-hover:opacity-30 transition-opacity"></div>
        <div className="relative z-10 flex flex-col md:flex-row items-center gap-10">
            <div className="w-20 h-20 rounded-3xl bg-white/10 flex items-center justify-center backdrop-blur-xl border border-white/10">
                <Info className="text-white" size={32} />
            </div>
            <div className="flex-1 text-center md:text-left">
                <h2 className="text-2xl font-black mb-2 tracking-tight">Durum Yönetimi Hakkında</h2>
                <p className="text-slate-400 font-medium leading-relaxed max-w-2xl italic">
                    Bu aşamalar sistemin çekirdek işleyişini temsil eder ve doğrudan veritabanı sabitleri (RequestStatusConstants) ile ilişkilidir. 
                    Taleplerin iş akışını korumak adına bu liste üzerinde ekleme veya silme işlemi yapılamamaktadır.
                </p>
            </div>
        </div>
    </div>
  );
};
