"use client";

import React from "react";
import Link from "next/link";
import { useOrganization } from "@/context/OrganizationContext";

interface HomeFooterProps {
  modulesCount?: number;
  guidesCount?: number;
  faqsCount?: number;
}

export const HomeFooter: React.FC<HomeFooterProps> = ({
  modulesCount = 0,
  guidesCount = 0,
  faqsCount = 0,
}) => {
  const { orgName } = useOrganization();

  return (
    <footer className="w-full space-y-4" data-purpose="footer-layer">
      {/* Quick Stats Pills & Structured Links Bar */}
      <div className="w-full bg-obsidian-900/80 backdrop-blur-md glass-border rounded-2xl p-4 flex flex-col md:flex-row items-center justify-between gap-4">
        {/* Stats Badges */}
        <div className="flex flex-wrap items-center justify-center gap-2" data-purpose="stats-pills">
          <div className="px-3 py-1.5 rounded-xl bg-obsidian-800/90 border border-slate-800 text-xs flex items-center space-x-2">
            <svg className="w-3.5 h-3.5 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
              />
            </svg>
            <span className="font-bold text-white tabular-nums">{modulesCount}</span>
            <span className="text-slate-400 uppercase text-[10px] tracking-wider">Modül</span>
          </div>

          <div className="px-3 py-1.5 rounded-xl bg-obsidian-800/90 border border-slate-800 text-xs flex items-center space-x-2">
            <svg className="w-3.5 h-3.5 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
              />
            </svg>
            <span className="font-bold text-white tabular-nums">{guidesCount}</span>
            <span className="text-slate-400 uppercase text-[10px] tracking-wider">Kılavuz</span>
          </div>

          <div className="px-3 py-1.5 rounded-xl bg-obsidian-800/90 border border-slate-800 text-xs flex items-center space-x-2">
            <svg className="w-3.5 h-3.5 text-pink-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
              />
            </svg>
            <span className="font-bold text-white tabular-nums">{faqsCount}</span>
            <span className="text-slate-400 uppercase text-[10px] tracking-wider">SSS</span>
          </div>
        </div>

        {/* Semantic Sub-footer Navigation Links */}
        <div className="flex flex-wrap items-center justify-center gap-4 text-xs text-slate-400" data-purpose="footer-links">
          <Link className="hover:text-purple-300 transition" href="/dashboard">
            Sistem Durumu
          </Link>
          <span className="text-slate-700">•</span>
          <a className="hover:text-purple-300 transition" href="#gizlilik">
            Gizlilik Politikası
          </a>
          <span className="text-slate-700">•</span>
          <Link className="hover:text-purple-300 transition" href="/dashboard">
            İletişim &amp; Destek
          </Link>
        </div>
      </div>

      {/* Copyright Notice */}
      <div className="text-center text-slate-500 text-[11px] pt-1">
        © 2025 {orgName || "Help Center Enterprise Portal"}. Tüm hakları saklıdır.
      </div>
    </footer>
  );
};
