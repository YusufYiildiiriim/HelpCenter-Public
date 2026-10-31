import { NextResponse, type NextRequest } from "next/server";
import { isDemoSsoEnabled } from "@/lib/demoSso";
import { DEFAULT_LOCALE, localeFromPreference, localePath } from "@/lib/i18n";

function resolveLocale(request: NextRequest): "tr-TR" | "en-US" {
  const pathLocale = request.nextUrl.pathname.split("/")[1];
  if (pathLocale === "tr") return "tr-TR";
  if (pathLocale === "en") return "en-US";
  return localeFromPreference(request.cookies.get("hc_locale")?.value)
    ?? localeFromPreference(request.headers.get("accept-language"))
    ?? DEFAULT_LOCALE;
}

export async function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const locale = resolveLocale(request);
  const pathLocale = pathname.split("/")[1];
  const explicitLocale = pathLocale === "tr" || pathLocale === "en";

  const isLocalizedPublicPath = pathname === "/tr" || pathname.startsWith("/tr/") || pathname === "/en" || pathname.startsWith("/en/");
  if (!isLocalizedPublicPath && !pathname.startsWith("/admin") && !pathname.startsWith("/dashboard")) {
    const target = request.nextUrl.clone();
    target.pathname = "/" + localePath(locale) + (pathname === "/" ? "" : pathname);
    return NextResponse.redirect(target);
  }

  const forwardedHeaders = new Headers(request.headers);
  forwardedHeaders.set("x-helpcenter-locale", locale);

  const hasRefreshSession = request.cookies.has("refresh_token");
  const isAdminRoute = pathname.startsWith("/admin");
  const isDashboardRoute = pathname.startsWith("/dashboard");
  const isAdminLogin = pathname === "/admin/login";
  const isDashboardLogin = pathname === "/dashboard/login";
  const isDashboardSso = pathname === "/dashboard/sso";

  if (isDashboardSso) {
    if (!isDemoSsoEnabled()) return NextResponse.redirect(new URL("/dashboard/login", request.url));
    return NextResponse.next({ request: { headers: forwardedHeaders } });
  }

  if (isAdminLogin || isDashboardLogin) return NextResponse.next({ request: { headers: forwardedHeaders } });

  if ((isAdminRoute || isDashboardRoute) && !hasRefreshSession) {
    const loginUrl = isAdminRoute ? "/admin/login" : "/dashboard/login";
    return NextResponse.redirect(new URL(loginUrl, request.url));
  }

  const response = NextResponse.next({ request: { headers: forwardedHeaders } });
  if (explicitLocale) {
    response.cookies.set("hc_locale", locale, { path: "/", maxAge: 60 * 60 * 24 * 365, sameSite: "lax" });
  }
  return response;
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};
