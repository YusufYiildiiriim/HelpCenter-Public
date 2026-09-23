"use client";

import React from "react";
import { Menu } from "lucide-react";

interface TopHeaderProps {
  onOpenMobileMenu: () => void;
}

export const TopHeader: React.FC<TopHeaderProps> = ({ onOpenMobileMenu }) => {
  return (
    <header className="sticky top-0 z-40 flex h-20 items-center justify-between border-b border-slate-100 bg-white/80 px-6 backdrop-blur-xl lg:px-10">
      <div className="flex items-center gap-4">
        <button
          className="flex h-10 w-10 items-center justify-center rounded-xl border border-slate-200 bg-slate-50 text-slate-600 transition-all hover:bg-indigo-50 hover:text-indigo-600 lg:hidden"
          onClick={onOpenMobileMenu}
        >
          <Menu size={20} />
        </button>
      </div>

      <div className="hidden h-10 w-px bg-slate-100 lg:block" />
    </header>
  );
};
