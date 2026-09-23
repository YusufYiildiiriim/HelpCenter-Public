import React from "react";
import { AlertTriangle, X } from "lucide-react";
import { cn } from "@/lib/utils";

interface AlertModalProps {
  isOpen: boolean;
  title: string;
  description: string;
  confirmText?: string;
  cancelText?: string;
  onConfirm: () => void;
  onCancel: () => void;
  variant?: "danger" | "warning" | "info";
}

export const AlertModal: React.FC<AlertModalProps> = ({
  isOpen,
  title,
  description,
  confirmText = "Onayla",
  cancelText = "Vazgeç",
  onConfirm,
  onCancel,
  variant = "danger"
}) => {
  if (!isOpen) return null;

  const themes = {
    danger: {
      icon: <AlertTriangle className="text-red-500" size={32} />,
      iconBg: "bg-red-50 dark:bg-red-500/10",
      button: "bg-slate-900 hover:bg-red-600 text-white",
      ring: "focus:ring-red-500/20"
    },
    warning: {
      icon: <AlertTriangle className="text-amber-500" size={32} />,
      iconBg: "bg-amber-50 dark:bg-amber-500/10",
      button: "bg-slate-900 hover:bg-amber-600 text-white",
      ring: "focus:ring-amber-500/20"
    },
    info: {
      icon: <AlertTriangle className="text-blue-500" size={32} />,
      iconBg: "bg-blue-50 dark:bg-blue-500/10",
      button: "bg-slate-900 hover:bg-blue-600 text-white",
      ring: "focus:ring-blue-500/20"
    }
  };

  const theme = themes[variant];

  return (
    <div className="fixed inset-0 z-[200] flex items-center justify-center p-4 sm:p-6 lg:pl-76 lg:pr-6 bg-slate-900/60 backdrop-blur-xl animate-in fade-in duration-300">
      <div 
        className="bg-white dark:bg-slate-900 rounded-[2.5rem] w-full max-w-md overflow-hidden shadow-2xl animate-in zoom-in duration-300 border border-white/20 dark:border-slate-800 relative my-auto"
      >
        {/* Close Button */}
        <button 
          onClick={onCancel}
          className="absolute top-8 right-8 w-10 h-10 flex items-center justify-center rounded-xl text-slate-400 dark:text-slate-500 hover:text-slate-600 dark:hover:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 transition-all"
        >
          <X size={20} />
        </button>

        <div className="p-10 flex flex-col items-center text-center">
          {/* Icon Area */}
          <div className={cn("w-20 h-20 rounded-4xl flex items-center justify-center mb-8 shadow-inner border border-white/50 dark:border-slate-700", theme.iconBg)}>
            {theme.icon}
          </div>

          {/* Content */}
          <h3 className="text-2xl font-black text-slate-900 dark:text-white tracking-tight mb-3">
            {title}
          </h3>
          <p className="text-slate-500 dark:text-slate-400 font-medium leading-relaxed mb-10">
            {description}
          </p>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-4 w-full">
            <button
              onClick={onCancel}
              className="flex-1 px-8 py-4 rounded-2xl font-black text-[10px] uppercase tracking-widest text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-800 transition-all active:scale-95"
            >
              {cancelText}
            </button>
            <button
              onClick={onConfirm}
              className={cn(
                "flex-1 px-8 py-4 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] shadow-xl transition-all active:scale-95",
                theme.button
              )}
            >
              {confirmText}
            </button>
          </div>
        </div>

        {/* Decorative element */}
        <div className="absolute bottom-0 left-0 right-0 h-1 bg-linear-to-r from-transparent via-slate-100 dark:via-slate-800 to-transparent" />
      </div>
    </div>
  );
};
