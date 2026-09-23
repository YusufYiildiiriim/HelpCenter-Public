"use client";

import { useEffect, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { useRouter } from "next/navigation";
import { Loader2 } from "lucide-react";
import { setAccessToken } from "@/lib/authSession";
import { hasValidDemoSsoParams, isDemoSsoEnabled } from "@/lib/demoSso";

const redirectToLogin = () => {
    // A full navigation guarantees the dashboard guard is evaluated again.
    // eslint-disable-next-line @next/next/no-location-assign-relative-destination
    window.location.assign("/dashboard/login");
};

function SSOContent() {
    const searchParams = useSearchParams();
    const router = useRouter();

    useEffect(() => {
        const token = searchParams.get("token");
        const companyId = searchParams.get("companyId");
        const userId = searchParams.get("userId");

        if (!isDemoSsoEnabled() || !token || !companyId || !hasValidDemoSsoParams(token, companyId, userId)) {
            redirectToLogin();
            return;
        }

        // Remove the one-time demo token before the app navigates anywhere else.
        window.history.replaceState(null, "", "/dashboard/sso");
        setAccessToken(token);
        router.replace(`/dashboard/${companyId}`);
    }, [router, searchParams]);

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-950">
            <div className="flex flex-col items-center gap-4">
                <Loader2 className="animate-spin text-indigo-500" size={40} />
                <p className="text-slate-500 font-black uppercase tracking-widest text-[10px] animate-pulse">
                    Oturum açılıyor...
                </p>
                <p className="max-w-sm text-center text-slate-600 text-xs">
                    Bu yalnızca CV/demo amaçlı bir oturum köprüsüdür; OIDC veya SAML SSO değildir.
                </p>
            </div>
        </div>
    );
}

export default function SSOPage() {
    return (
        <Suspense fallback={
            <div className="min-h-screen flex items-center justify-center bg-slate-950">
                <Loader2 className="animate-spin text-indigo-500" size={40} />
            </div>
        }>
            <SSOContent />
        </Suspense>
    );
}
