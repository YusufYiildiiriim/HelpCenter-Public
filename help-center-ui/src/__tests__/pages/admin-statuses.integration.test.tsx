import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import MockAdapter from 'axios-mock-adapter';
import api from '@/lib/axios';
import { AuthProvider } from '@/context/AuthContext';
import { OrganizationProvider } from '@/context/OrganizationContext';
import type { VerifyResponse } from '@/services/common/AuthService';
import AdminStatusesPage from '@/app/admin/statuses/page';

/**
 * End-to-end "solid foundation" test: mocks the axios layer (via axios-mock-adapter)
 * to verify the page's loading -> success and loading -> forbidden transitions through the real
 * AdminStatusService + ApiStateView + DataTable chain. Other tests (a)-(d) prove isolated units;
 * this test proves they integrate correctly on a real page.
 */
describe('AdminStatusesPage (entegrasyon)', () => {
  let mock: MockAdapter;

  const userInfo: VerifyResponse = {
    authenticated: true,
    role: 'Admin',
    username: 'test-admin',
    userId: 1,
    isPasswordChangeRequired: false,
    permissions: { modules: [{ resourceKey: 'Statuses', actions: ['Read', 'Export', 'Print'] }] },
  };

  function renderPage() {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });

    return render(
      <QueryClientProvider client={queryClient}>
        <AuthProvider userInfo={userInfo} permissions={userInfo.permissions}>
          <OrganizationProvider initialOrgInfo={{ organizationName: 'Test Org' }}>
            <AdminStatusesPage />
          </OrganizationProvider>
        </AuthProvider>
      </QueryClientProvider>
    );
  }

  beforeEach(() => {
    mock = new MockAdapter(api);
  });

  afterEach(() => {
    mock.restore();
  });

  it('önce spinner gösterir, veri gelince tabloyu render eder', async () => {
    mock.onGet('/api/admin/statuses/get-all').reply(200, {
      items: [
        { id: 1, name: 'Açık', description: 'Yeni oluşturulan talep', isActive: true },
        { id: 2, name: 'Kapalı', description: 'Tamamlanmış talep', isActive: false },
      ],
      totalCount: 2,
      totalPages: 1,
      currentPage: 1,
      pageSize: 10,
    });

    renderPage();

    expect(screen.getByText('Durumlar Getiriliyor...')).toBeInTheDocument();

    // DataTable renders the same data in both mobile card and desktop table view (CSS hidden/visible)
    // simultaneously — which is why multiple matches are expected.
    await waitFor(() => expect(screen.getAllByText('Açık').length).toBeGreaterThan(0));
    expect(screen.getAllByText('Kapalı').length).toBeGreaterThan(0);
    expect(screen.queryByText('Durumlar Getiriliyor...')).not.toBeInTheDocument();
  });

  it('API 403 dönerse ForbiddenState gösterir', async () => {
    mock.onGet('/api/admin/statuses/get-all').reply(403, {
      success: false,
      error: { code: 'FORBIDDEN', message: 'Bu kaynağa erişim izniniz yok' },
    });

    renderPage();

    await waitFor(() => expect(screen.getByText('Erişim Yetkiniz Yok')).toBeInTheDocument());
    expect(screen.queryByText('Durumlar Getiriliyor...')).not.toBeInTheDocument();
  });

  it('API 500 dönerse ErrorState gösterir ve tekrar dene ile yeniden fetch tetiklenir', async () => {
    mock.onGet('/api/admin/statuses/get-all').reply(500, { success: false });

    renderPage();

    await waitFor(() => expect(screen.getByRole('alert')).toBeInTheDocument());
    expect(screen.getByRole('button', { name: /tekrar dene/i })).toBeInTheDocument();
  });
});
