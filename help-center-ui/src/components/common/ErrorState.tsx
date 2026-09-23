import { AlertTriangle, RotateCw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  correlationId?: string;
  /** "page": full-page/section centered large display. "inline": small display inside a card/section. */
  variant?: "page" | "inline";
  className?: string;
}

const DEFAULT_TITLE = "Bir şeyler ters gitti";
const DEFAULT_MESSAGE = "İşlem gerçekleştirilirken beklenmeyen bir hata oluştu.";

export function ErrorState({
  title = DEFAULT_TITLE,
  message = DEFAULT_MESSAGE,
  onRetry,
  correlationId,
  variant = "inline",
  className,
}: ErrorStateProps) {
  return (
    <div
      role="alert"
      className={cn(
        "flex flex-col items-center justify-center gap-3 rounded-2xl border border-status-danger-border bg-status-danger-surface text-center",
        variant === "page" ? "px-8 py-32" : "px-6 py-10",
        className
      )}
    >
      <AlertTriangle className={cn(variant === "page" ? "w-16 h-16" : "w-10 h-10", "text-status-danger")} />
      <div className="space-y-1">
        <h3
          className={cn(
            "font-black uppercase tracking-widest text-status-danger",
            variant === "page" ? "text-sm" : "text-xs"
          )}
        >
          {title}
        </h3>
        {message && <p className="max-w-md text-sm text-status-danger/90">{message}</p>}
      </div>
      {onRetry && (
        <Button variant="destructive" size="sm" onClick={onRetry} className="mt-2">
          <RotateCw />
          Tekrar Dene
        </Button>
      )}
      {correlationId && (
        <details className="mt-2 text-left">
          <summary className="cursor-pointer text-[10px] font-bold uppercase tracking-widest text-status-danger hover:text-status-danger/80">
            Destek Bilgisi
          </summary>
          <code className="mt-1 block break-all text-[11px] text-status-danger/80">{correlationId}</code>
        </details>
      )}
    </div>
  );
}
