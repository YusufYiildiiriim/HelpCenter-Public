import { act, cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import Layout from '@/app/dashboard/[companyId]/layout';

const mocks = vi.hoisted(() => ({ verify: vi.fn(), logout: vi.fn(), replace: vi.fn() }));
const router = { replace: mocks.replace };
vi.mock('next/navigation', () => ({ usePathname: () => '/dashboard/test', useRouter: () => router }));
vi.mock('@/services/common/AuthService', () => ({ AuthService: { verify: mocks.verify, logout: mocks.logout } }));
vi.mock('@/components/auth/PasswordChangeModal', () => ({ default: () => null }));

describe('customer verification failures', () => {
  beforeEach(() => { vi.resetAllMocks(); vi.spyOn(console, 'error').mockImplementation(() => {}); });
  afterEach(() => { cleanup(); vi.restoreAllMocks(); });

  it('retains session after a temporary failure and retries without exposing protected content', async () => {
    mocks.verify.mockRejectedValueOnce(Object.assign(new Error('unavailable'), { isAxiosError: true, response: { status: 503 } }));
    await act(async () => { render(<Layout>Protected customer</Layout>); });
    expect(screen.queryByText('Protected customer')).not.toBeInTheDocument();
    expect(mocks.logout).not.toHaveBeenCalled();
    expect(mocks.replace).not.toHaveBeenCalled();
    mocks.verify.mockResolvedValueOnce({ authenticated: true, userId: 2, username: 'customer', role: 'Customer' });
    await act(async () => fireEvent.click(screen.getByRole('button')));
    expect(screen.getByText('Protected customer')).toBeInTheDocument();
  });

  it('rejects a real authentication failure', async () => {
    mocks.verify.mockRejectedValueOnce(Object.assign(new Error('unauthorized'), { isAxiosError: true, response: { status: 401 } }));
    await act(async () => { render(<Layout>Protected customer</Layout>); });
    expect(mocks.logout).toHaveBeenCalledOnce();
    expect(mocks.replace).toHaveBeenCalledWith('/dashboard/login');
    expect(screen.queryByText('Protected customer')).not.toBeInTheDocument();
  });
});
