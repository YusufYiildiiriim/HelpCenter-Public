import { ShieldAlert } from "lucide-react";

import { cn } from "@/lib/utils";

export interface ForbiddenStateProps {
  title?: string;
  message?: string;
  /** "page": full-page/section centered large display. "inline": small display inside a card/section. */
  variant?: "page" | "inline";
  className?: string;
}

const DEFAULT_TITLE = "Erişim Yetkiniz Yok";
const DEFAULT_MESSAGE = "Bu içeriği görüntülemek veya bu işlemi gerçekleştirmek için yetkiniz bulunmuyor.";

export function ForbiddenState({
  title = DEFAULT_TITLE,
  message = DEFAULT_MESSAGE,
  variant = "inline",
  className,
}: ForbiddenStateProps) {
  return (
    <div
      role="alert"
      className={cn(
        "flex flex-col items-center justify-center gap-3 rounded-2xl border border-status-warning-border bg-status-warning-surface text-center",
        variant === "page" ? "px-8 py-32" : "px-6 py-10",
        className
      )}
    >
      <ShieldAlert className={cn(variant === "page" ? "w-16 h-16" : "w-10 h-10", "text-status-warning")} />
      <div className="space-y-1">
        <h3
          className={cn(
            "font-black uppercase tracking-widest text-status-warning",
            variant === "page" ? "text-sm" : "text-xs"
          )}
        >
          {title}
        </h3>
        {message && <p className="max-w-md text-sm text-status-warning/90">{message}</p>}
      </div>
    </div>
  );
}
