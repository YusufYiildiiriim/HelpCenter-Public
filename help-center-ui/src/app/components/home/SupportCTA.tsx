import React from "react";
import { ChevronRight, MessageSquare } from "lucide-react";
import { motion } from "framer-motion";

export const SupportCTA: React.FC = () => {
  return (
    <section className="max-w-7xl mx-auto px-6 pb-32">
        <motion.div 
            initial={{ opacity: 0, y: 40 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="p-16 md:p-24 bg-slate-900 rounded-[4rem] text-white relative overflow-hidden shadow-2xl group"
        >
            {/* Background Decorative Elements */}
            <div className="absolute top-0 right-0 w-[50%] h-full bg-blue-600/20 blur-[120px] pointer-events-none group-hover:bg-blue-600/30 transition-all duration-1000"></div>
            <div className="absolute -bottom-20 -left-20 w-80 h-80 bg-indigo-600/10 rounded-full blur-[80px] pointer-events-none group-hover:bg-indigo-600/20 transition-all duration-1000"></div>
            
            <div className="relative z-10 flex flex-col lg:flex-row items-center justify-between gap-12 text-center lg:text-left">
                <div className="max-w-2xl">
                    <div className="inline-flex items-center gap-2 bg-blue-600/20 text-blue-400 px-4 py-2 rounded-full text-[10px] font-black uppercase tracking-[0.2em] mb-8">
                        <MessageSquare size={14} className="fill-blue-400" />
                        Destek Ekibi Yanınızda
                    </div>
                    <h3 className="text-4xl md:text-6xl font-black tracking-tight mb-6 leading-[0.95]">
                        Hala aradığınızı <br />
                        <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-400 to-indigo-400">bulamadınız mı?</span>
                    </h3>
                    <p className="text-slate-400 text-lg md:text-xl font-medium italic">
                        Ekibimiz her türlü sorunuz için size yardımcı olmaktan mutluluk duyacaktır. 
                        Size en geç 15 dakika içinde dönüş sağlıyoruz.
                    </p>
                </div>

                <div className="shrink-0 space-y-6">
                    <motion.a
                        whileHover={{ scale: 1.05 }}
                        whileTap={{ scale: 0.95 }}
                        href="/dashboard/login"
                        className="bg-white text-slate-900 px-12 py-6 rounded-[2.5rem] font-black text-sm uppercase tracking-widest flex items-center justify-center gap-4 hover:bg-blue-500 hover:text-white transition-all shadow-2xl relative z-10"
                    >
                        Bizimle İletişime Geçin
                        <ChevronRight size={20} />
                    </motion.a>
                    <div className="flex items-center justify-center lg:justify-start gap-4 text-slate-500">
                        <div className="flex -space-x-3">
                            {[1, 2, 3].map(i => (
                                <div key={i} className="w-8 h-8 rounded-full border-2 border-slate-900 bg-slate-800 flex items-center justify-center">
                                    <div className="w-full h-full rounded-full bg-blue-600/20 flex items-center justify-center">
                                        <div className="w-1.5 h-1.5 rounded-full bg-blue-400 animate-pulse"></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <span className="text-[10px] font-black uppercase tracking-widest">+12 Kişi Şu an Çevrimiçi</span>
                    </div>
                </div>
            </div>

            {/* Subtle Grid Overlay */}
            <div className="absolute inset-0 opacity-[0.03] pointer-events-none" style={{ backgroundImage: 'radial-gradient(#ffffff 1px, transparent 1px)', backgroundSize: '30px 30px' }}></div>
        </motion.div>
    </section>
  );
};
