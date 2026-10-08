import type { ReactNode } from 'react';
import { useEffect } from 'react';
import { act, cleanup, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import AdminLayout from '@/app/admin/layout';
import { AuthService } from '@/services/common/AuthService';
import type { MenuItemDto, VerifyResponse } from '@/services/common/AuthService';
import { useAuth } from '@/context/AuthContext';

const { push, mounted } = vi.hoisted(() => ({ push: vi.fn(), mounted: vi.fn() }));
const router = { push };
vi.mock('next/navigation', () => ({ useRouter: () => router, usePathname: () => '/admin' }));
vi.mock('@/services/common/AuthService', () => ({
  AuthService: { verify: vi.fn(), getMenuItems: vi.fn(), logout: vi.fn() },
}));
vi.mock('@/services/admin/AdminOrganizationService', () => ({
  AdminOrganizationService: { getOrganizationInfo: vi.fn(async () => ({ organizationName: 'Test' })) },
}));
vi.mock('@/context/OrganizationContext', () => ({
  OrganizationProvider: ({ children }: { children: ReactNode }) => <>{children}</>,
}));
vi.mock('@/components/auth/PasswordChangeModal', () => ({ default: () => <div>Password change required</div> }));
vi.mock('@/app/admin/layout-components/Sidebar', () => ({
  Sidebar: ({ menuGroups }: { menuGroups: { items: { label: string }[] }[] }) =>
    <nav>{menuGroups.flatMap((g) => g.items).map((i) => <span key={i.label}>{i.label}</span>)}</nav>,
}));

function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (reason: Error) => void;
  const promise = new Promise<T>((res, rej) => { resolve = res; reject = rej; });
  return { promise, resolve, reject };
}
const user: VerifyResponse = {
  authenticated: true, role: 'Admin', username: 'admin', userId: 1,
  isPasswordChangeRequired: false,
  permissions: { modules: [{ resourceKey: 'Dashboard', actions: ['Read'] }] },
};
const menu = [{ id: 1, label: 'Server menu', route: '/admin', icon: 'BarChart3', resourceKey: 'Dashboard' }] as MenuItemDto[];
function ProtectedContent() {
  const { userInfo, permissions } = useAuth();
  useEffect(() => { mounted(userInfo, permissions); }, [userInfo, permissions]);
  return <div>Protected dashboard</div>;
}

describe('AdminLayout menu loading', () => {
  let verification: ReturnType<typeof deferred<VerifyResponse>>;
  let menuRequest: ReturnType<typeof deferred<MenuItemDto[]>>;
  beforeEach(() => {
    vi.clearAllMocks();
    verification = deferred<VerifyResponse>();
    menuRequest = deferred<MenuItemDto[]>();
    vi.mocked(AuthService.verify).mockReturnValue(verification.promise);
    vi.mocked(AuthService.getMenuItems).mockReturnValue(menuRequest.promise);
  });
  afterEach(() => { cleanup(); vi.restoreAllMocks(); });

  it('mounts authorized content with permissions before the menu resolves', async () => {
    render(<AdminLayout><ProtectedContent /></AdminLayout>);
    expect(screen.queryByText('Protected dashboard')).not.toBeInTheDocument();
    expect(AuthService.getMenuItems).not.toHaveBeenCalled();
    await act(async () => verification.resolve(user));
    expect(screen.getByText('Protected dashboard')).toBeInTheDocument();
    expect(mounted).toHaveBeenCalledWith(user, user.permissions);
    expect(AuthService.getMenuItems).toHaveBeenCalledTimes(1);
    expect(screen.queryByText('Server menu')).not.toBeInTheDocument();
    await act(async () => menuRequest.resolve(menu));
    expect(screen.getByText('Server menu')).toBeInTheDocument();
  });

  it('keeps authorized content available when the menu request fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    render(<AdminLayout><ProtectedContent /></AdminLayout>);
    await act(async () => verification.resolve(user));
    expect(screen.getByText('Protected dashboard')).toBeInTheDocument();
    await act(async () => menuRequest.reject(new Error('menu unavailable')));
    expect(screen.getByText('Protected dashboard')).toBeInTheDocument();
    expect(push).not.toHaveBeenCalled();
  });

  it.each([
    { ...user, authenticated: false },
    { ...user, role: 'Customer' },
  ])('does not mount protected content for rejected identity $role / $authenticated', async (identity) => {
    render(<AdminLayout><ProtectedContent /></AdminLayout>);
    await act(async () => verification.resolve(identity));
    expect(screen.queryByText('Protected dashboard')).not.toBeInTheDocument();
    expect(AuthService.getMenuItems).not.toHaveBeenCalled();
    expect(push).toHaveBeenCalledWith('/admin/login');
  });

  it('keeps protected content hidden if verification fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    render(<AdminLayout><ProtectedContent /></AdminLayout>);
    await act(async () => verification.reject(new Error('unauthorized')));
    expect(mounted).not.toHaveBeenCalled();
    expect(AuthService.getMenuItems).not.toHaveBeenCalled();
    expect(push).toHaveBeenCalledWith('/admin/login');
  });

  it('preserves the required password change prompt without waiting for menu', async () => {
    render(<AdminLayout><ProtectedContent /></AdminLayout>);
    await act(async () => verification.resolve({ ...user, isPasswordChangeRequired: true }));
    expect(screen.getByText('Password change required')).toBeInTheDocument();
    await act(async () => menuRequest.resolve(menu));
  });

  it('does not start a menu request for verification resolved after unmount', async () => {
    const view = render(<AdminLayout><ProtectedContent /></AdminLayout>);
    view.unmount();
    await act(async () => verification.resolve(user));
    expect(mounted).not.toHaveBeenCalled();
    expect(AuthService.getMenuItems).not.toHaveBeenCalled();
  });
});
