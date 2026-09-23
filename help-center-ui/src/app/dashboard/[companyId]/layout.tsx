"use client";

import React from "react";
import { usePathname, useRouter } from "next/navigation";
import type { UserPermissions, VerifyResponse } from "@/services/common/AuthService";
import { AuthService } from "@/services/common/AuthService";
import { AuthProvider } from "@/context/AuthContext";
import PasswordChangeModal from "@/components/auth/PasswordChangeModal";
import { Toaster } from "sonner";
import { Loader2 } from "lucide-react";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const router = useRouter();

  const [isAuthorized, setIsAuthorized] = React.useState(false);
  const [isVerifying, setIsVerifying] = React.useState(true);
  const [permissions, setPermissions] = React.useState<UserPermissions | undefined>(undefined);
  const [userInfo, setUserInfo] = React.useState<VerifyResponse | null>(null);
  const [showPasswordChange, setShowPasswordChange] = React.useState(false);

  React.useEffect(() => {
    // Verify token validity from backend
    const verifyAuth = async () => {
      try {
        const response = await AuthService.verify();

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
        console.error("Dashboard auth verification failed:", error);
        await AuthService.logout();
        router.replace("/dashboard/login");
      } finally {
        setIsVerifying(false);
      }
    };

    verifyAuth();
  }, [pathname, router]);

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

  if (!isAuthorized) {
    return null;
  }

  return (
    <AuthProvider userInfo={userInfo} permissions={permissions}>
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
