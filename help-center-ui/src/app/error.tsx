"use client";

import { useEffect } from "react";

import { ErrorState } from "@/components/common/ErrorState";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    if (process.env.NODE_ENV !== "production") {
      console.error(error);
    }
  }, [error]);

  return (
    <div className="flex min-h-[70vh] items-center justify-center px-4">
      <ErrorState
        variant="page"
        message="Sayfa yüklenirken beklenmeyen bir hata oluştu. Lütfen tekrar deneyin."
        correlationId={error.digest}
        onRetry={reset}
        className="max-w-lg"
      />
    </div>
  );
}
