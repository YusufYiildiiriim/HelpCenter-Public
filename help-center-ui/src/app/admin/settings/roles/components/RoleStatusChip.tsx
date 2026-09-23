import React from "react";
import { cn } from "@/lib/utils";

interface RoleStatusChipProps {
  label: string;
  color: 'indigo' | 'emerald' | 'amber';
}

export const RoleStatusChip: React.FC<RoleStatusChipProps> = ({ label, color }) => {
    const colors = {
        indigo: "bg-indigo-50 text-indigo-700 border-indigo-100",
        emerald: "bg-emerald-50 text-emerald-700 border-emerald-100",
        amber: "bg-amber-50 text-amber-700 border-amber-100",
    };

    return (
        <span className={cn(
            "px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-wider border",
            colors[color]
        )}>
            {label}
        </span>
    );
};
