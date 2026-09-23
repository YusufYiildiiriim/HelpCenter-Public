"use client";

import { useEffect } from "react";
import { AuthService } from "@/services/common/AuthService";

/**
 * This page executes when navigating directly to the /dashboard URL.
 * Per user requirements:
 * 1. Clears all session data and COOKIES if logged in (Log Out).
 * 2. Redirects the user directly to the /dashboard/login page.
 */
export default function DashboardRootRedirect() {
    useEffect(() => {
        const logoutAndRedirect = async () => {
            await AuthService.logout();

            // Use window.location.href as a definitive solution: router.push only performs client-side
            // navigation and does not clear in-memory React/Context state. The goal here
            // is a complete logout — clearing the API refresh cookie and in-memory access token
            // and landing on the login page with zero React state, so a full page
            // reload was chosen intentionally.
            // eslint-disable-next-line @next/next/no-location-assign-relative-destination
            window.location.href = "/dashboard/login";
        };

        void logoutAndRedirect();
    }, []);

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-950">
            <div className="flex flex-col items-center gap-4">
                <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
                <p className="text-slate-500 font-black uppercase tracking-widest text-[10px] animate-pulse">
                    Giriş sayfasına yönlendiriliyorsunuz...
                </p>
            </div>
        </div>
    );
}
