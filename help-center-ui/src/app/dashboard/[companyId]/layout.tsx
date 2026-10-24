"use client";

import React from "react";
import { usePathname, useRouter } from "next/navigation";
import type { UserPermissions, VerifyResponse } from "@/services/common/AuthService";
import { AuthService } from "@/services/common/AuthService";
import { AuthProvider } from "@/context/AuthContext";
import PasswordChangeModal from "@/components/auth/PasswordChangeModal";
import { Toaster } from "sonner";
import { Loader2 } from "lucide-react";
import { ErrorState } from "@/components/common/ErrorState";
import { getApiErrorMessage, getApiErrorStatus, getApiRetryAfterSeconds } from "@/lib/api/errors";
import { clearAccessToken, subscribeToLogout } from "@/lib/authSession";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const router = useRouter();

  const [isAuthorized, setIsAuthorized] = React.useState(false);
  const [isVerifying, setIsVerifying] = React.useState(true);
  const [verificationFailed, setVerificationFailed] = React.useState(false);
  const [verificationError, setVerificationError] = React.useState<unknown>(null);
  const [retryDeadline, setRetryDeadline] = React.useState<number | null>(null);
  const [retrySeconds, setRetrySeconds] = React.useState(0);
  const [attempt, setAttempt] = React.useState(0);
  const [permissions, setPermissions] = React.useState<UserPermissions | undefined>(undefined);
  const [userInfo, setUserInfo] = React.useState<VerifyResponse | null>(null);
  const [showPasswordChange, setShowPasswordChange] = React.useState(false);

  React.useEffect(() => {
    return subscribeToLogout(() => {
      clearAccessToken();
      router.replace("/dashboard/login");
    });
  }, [router]);

  React.useEffect(() => {
    let mounted = true;
    // Verify token validity from backend
    const verifyAuth = async () => {
      try {
        const response = await AuthService.verify();
        if (!mounted) return;

        if (response.authenticated) {
          setIsAuthorized(true);
          setPermissions(response.permissions);
          setUserInfo(response);
          if (response.isPasswordChangeRequired) {
            setShowPasswordChange(true);
          }
        } else {
          await AuthService.logout();
          router.replace("/dashboard/login");
        }
      } catch (error) {
        if (!mounted) return;
        console.error("Dashboard auth verification failed:", error);
        if (getApiErrorStatus(error) === 429) {
          const seconds = getApiRetryAfterSeconds(error);
          setRetrySeconds(seconds);
          setRetryDeadline(Date.now() + seconds * 1000);
          setVerificationFailed(false);
        } else if ([401, 403].includes(getApiErrorStatus(error) ?? 0)) {
          await AuthService.logout();
          router.replace("/dashboard/login");
        } else {
          setVerificationError(error);
          setVerificationFailed(true);
        }
      } finally {
        if (mounted) setIsVerifying(false);
      }
    };

    verifyAuth();
    return () => { mounted = false; };
  }, [pathname, router, attempt]);

  React.useEffect(() => {
    if (retryDeadline === null) return;
    const update = () => setRetrySeconds(Math.max(0, Math.ceil((retryDeadline - Date.now()) / 1000)));
    const timer = window.setInterval(update, 1000);
    return () => window.clearInterval(timer);
  }, [retryDeadline]);

  if (isVerifying) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-950">
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="animate-spin text-indigo-500" size={40} />
          <p className="text-slate-400 font-bold">Oturum doğrulanıyor...</p>
        </div>
      </div>
    );
  }

  if (verificationFailed) {
    return <ErrorState variant="page" message={getApiErrorMessage(verificationError)} onRetry={() => {
      setVerificationFailed(false);
      setVerificationError(null);
      setIsVerifying(true);
      setAttempt(value => value + 1);
    }} />;
  }

  if (retryDeadline !== null) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-950 p-6">
        <div className="max-w-md text-center space-y-5 text-slate-200">
          <h1 className="text-xl font-semibold">Kısa bir süre bekleyin</h1>
          <p>İstek sınırına ulaşıldığı için oturumunuz şu anda doğrulanamıyor.</p>
          <p role="status">{retrySeconds > 0 ? `${retrySeconds} saniye sonra yeniden deneyebilirsiniz.` : "Şimdi yeniden deneyebilirsiniz."}</p>
          <button type="button" disabled={retrySeconds > 0}
            onClick={() => {
              if (Date.now() < retryDeadline) return;
              setRetryDeadline(null);
              setIsVerifying(true);
              setAttempt(value => value + 1);
            }}
            className="rounded-lg bg-indigo-600 px-5 py-3 font-semibold disabled:opacity-50 disabled:cursor-not-allowed">
            Yeniden dene
          </button>
        </div>
      </div>
    );
  }

  if (!isAuthorized) {
    return null;
  }

  return (
    <AuthProvider userInfo={userInfo} permissions={permissions}>
      <a
        href="#customer-main"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-[200] focus:rounded-lg focus:bg-indigo-600 focus:px-4 focus:py-3 focus:font-bold focus:text-white focus:outline-none focus:ring-4 focus:ring-indigo-300/50"
      >
        Ana içeriğe geç
      </a>
      {children}
      <Toaster richColors position="top-right" theme="system" />
      {showPasswordChange && (
        <PasswordChangeModal
          email={userInfo?.username || ""}
          isCustomer={true}
          onSuccess={() => {
            setShowPasswordChange(false);
            if (userInfo) {
              setUserInfo({ ...userInfo, isPasswordChangeRequired: false });
            }
          }}
        />
      )}
    </AuthProvider>
  );
}
