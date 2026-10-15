"use client";

import { useEffect, useState } from "react";
import { Loader2 } from "lucide-react";

import { cn } from "@/lib/utils";

const SIZE_MAP = {
  sm: "w-5 h-5",
  md: "w-8 h-8",
  lg: "w-16 h-16",
} as const;

export type LoadingSpinnerSize = keyof typeof SIZE_MAP;

export interface LoadingSpinnerProps {
  /** "page": full-page/section centered large display. "inline": small display inside a card/section. */
  variant?: "page" | "inline";
  size?: LoadingSpinnerSize;
  message?: string;
  className?: string;
  /** Suppress brief loading flashes without delaying the response itself. */
  delayMs?: number;
}

export function LoadingSpinner({
  variant = "inline",
  size,
  message,
  className,
  delayMs = 200,
}: LoadingSpinnerProps) {
  const [visible, setVisible] = useState(delayMs <= 0);
  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), delayMs);
    return () => clearTimeout(timer);
  }, [delayMs]);
  const resolvedSize = size ?? (variant === "page" ? "lg" : "md");
  const accessibility = {
    role: visible ? "status" : undefined,
    "aria-live": "polite" as const,
    "aria-label": message || "Yükleniyor",
    "aria-hidden": !visible,
  };

  if (variant === "page") {
    return (
      <div
        {...accessibility}
        className={cn(
          "flex flex-col items-center justify-center py-32 bg-surface-raised rounded-2xl border border-border-default/80",
          className,
          !visible && "invisible"
        )}
      >
        <Loader2 aria-hidden="true" className={cn(SIZE_MAP[resolvedSize], "text-action-primary animate-spin motion-reduce:animate-none")} />
        {message && (
          <p className="mt-6 text-content-muted font-black uppercase tracking-widest text-xs">
            {message}
          </p>
        )}
      </div>
    );
  }

  return (
    <div
      {...accessibility}
      className={cn("flex items-center justify-center gap-3 py-6", className, !visible && "invisible")}
    >
      <Loader2 aria-hidden="true" className={cn(SIZE_MAP[resolvedSize], "text-action-primary animate-spin motion-reduce:animate-none")} />
      {message && <span className="text-sm font-medium text-content-muted">{message}</span>}
    </div>
  );
}
