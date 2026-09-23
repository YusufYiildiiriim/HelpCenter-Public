import { NextResponse, type NextRequest } from 'next/server';
import { isDemoSsoEnabled } from '@/lib/demoSso';

export async function proxy(request: NextRequest) {
  // Access tokens exist only in browser memory, so Proxy cannot inspect their
  // claims. It uses the HttpOnly refresh cookie only as a coarse route gate;
  // each protected layout verifies the refreshed access token and its role.
  const hasRefreshSession = request.cookies.has('refresh_token');
  const { pathname } = request.nextUrl;

  const isAdminRoute = pathname.startsWith('/admin');
  const isDashboardRoute = pathname.startsWith('/dashboard');
  const isAdminLogin = pathname === '/admin/login';
  const isDashboardLogin = pathname === '/dashboard/login';
  const isDashboardSso = pathname === '/dashboard/sso';

  if (isDashboardSso) {
    if (!isDemoSsoEnabled()) {
      return NextResponse.redirect(new URL('/dashboard/login', request.url));
    }
    return NextResponse.next();
  }

  if (isAdminLogin || isDashboardLogin) return NextResponse.next();

  if (isAdminRoute || isDashboardRoute) {
    if (!hasRefreshSession) {
      const loginUrl = isAdminRoute ? '/admin/login' : '/dashboard/login';
      return NextResponse.redirect(new URL(loginUrl, request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/admin', '/admin/:path*', '/dashboard', '/dashboard/:path*'],
};
