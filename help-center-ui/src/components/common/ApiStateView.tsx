import type { ReactNode } from "react";

import { ErrorState } from "@/components/common/ErrorState";
import { ForbiddenState } from "@/components/common/ForbiddenState";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { getApiErrorCorrelationId, getApiErrorMessage } from "@/lib/api/errors";

export interface ApiErrorLike {
  message?: string;
  correlationId?: string;
}

export interface ApiStateViewProps {
  isLoading: boolean;
  /** Already classified 401/403 flag — status code parsing is not this component's responsibility. */
  isForbidden?: boolean;
  /** Can be string, Error, or { message, correlationId }; if absent/falsy, the success state is rendered. */
  error?: unknown;
  onRetry?: () => void;
  /** "page": full area at route/tab level. "inline": inside a card/section. */
  variant?: "page" | "inline";
  loadingMessage?: string;
  errorTitle?: string;
  forbiddenTitle?: string;
  forbiddenMessage?: string;
  className?: string;
  children: ReactNode;
}

function normalizeError(error: unknown): ApiErrorLike {
  return {
    message: getApiErrorMessage(error),
    correlationId: getApiErrorCorrelationId(error) ?? undefined,
  };
}

/**
 * Renders the 4 standard states of an API call (loading/forbidden/error/success) from a single point.
 * Order of precedence: isLoading > isForbidden > error > children.
 */
export function ApiStateView({
  isLoading,
  isForbidden = false,
  error,
  onRetry,
  variant = "inline",
  loadingMessage,
  errorTitle,
  forbiddenTitle,
  forbiddenMessage,
  className,
  children,
}: ApiStateViewProps) {
  if (isLoading) {
    return <LoadingSpinner variant={variant} message={loadingMessage} className={className} />;
  }

  if (isForbidden) {
    return (
      <ForbiddenState
        variant={variant}
        title={forbiddenTitle}
        message={forbiddenMessage}
        className={className}
      />
    );
  }

  if (error) {
    const { message, correlationId } = normalizeError(error);
    return (
      <ErrorState
        variant={variant}
        title={errorTitle}
        message={message}
        correlationId={correlationId}
        onRetry={onRetry}
        className={className}
      />
    );
  }

  return <>{children}</>;
}
