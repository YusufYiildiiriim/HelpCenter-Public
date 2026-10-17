import React, { StrictMode } from 'react';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import CompanyDashboardPage from '@/app/dashboard/[companyId]/page';

const mocks = vi.hoisted(() => ({
  push: vi.fn(), verify: vi.fn(), requests: vi.fn(), profile: vi.fn(),
  modules: vi.fn(), subjects: vi.fn(), guides: vi.fn(),
}));
const router = { push: mocks.push };
vi.mock('next/navigation', () => ({ useRouter: () => router }));
vi.mock('@/lib/usePageTitle', () => ({ usePageTitle: vi.fn() }));
vi.mock('@/services/common/AuthService', () => ({ AuthService: { verify: mocks.verify, logout: vi.fn() } }));
vi.mock('@/services/customer/CustomerRequestService', () => ({ CustomerRequestService: { getMyRequests: mocks.requests } }));
vi.mock('@/services/customer/CustomerProfileService', () => ({ CustomerProfileService: { getMyProfile: mocks.profile } }));
vi.mock('@/services/customer/CustomerModuleService', () => ({ CustomerModuleService: { getAll: mocks.modules } }));
vi.mock('@/services/customer/CustomerRequestSubjectService', () => ({ CustomerRequestSubjectService: { getAll: mocks.subjects } }));
vi.mock('@/services/customer/CustomerGuideService', () => ({ CustomerGuideService: { getAll: mocks.guides } }));
vi.mock('@/app/dashboard/[companyId]/components/DashboardNav', () => ({ DashboardNav: () => null }));
vi.mock('@/app/dashboard/[companyId]/components/ProfileView', () => ({ ProfileView: () => <div>Loaded profile</div> }));
vi.mock('@/app/dashboard/[companyId]/components/GuidesView', () => ({ GuidesView: () => <div>Loaded guides</div> }));
vi.mock('@/app/dashboard/[companyId]/components/RequestsView', () => ({
  RequestsView: ({ onNewRequest }: { onNewRequest: () => void }) =>
    <button onClick={onNewRequest}>Yeni Talep Oluştur</button>,
}));
vi.mock('@/app/dashboard/[companyId]/components/NewRequestModal', () => ({
  NewRequestModal: ({ isOpen }: { isOpen: boolean }) => isOpen ? <div>New request form</div> : null,
}));

async function mountDashboard(strict = true) {
  const params = Promise.resolve({ companyId: '50000000-0000-4000-8000-000000000001' });
  let view!: ReturnType<typeof render>;
  await act(async () => {
    const page = <CompanyDashboardPage params={params} />;
    view = render(strict ? <StrictMode>{page}</StrictMode> : page);
  });
  return view;
}

describe('customer dashboard loading lifecycle', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.verify.mockResolvedValue({ authenticated: true });
    mocks.requests.mockResolvedValue([]);
    mocks.profile.mockResolvedValue({ firstName: 'QA' });
    mocks.modules.mockResolvedValue([]);
    mocks.subjects.mockResolvedValue([]);
    mocks.guides.mockResolvedValue([]);
  });
  afterEach(cleanup);

  it.each([true, false])('finishes loading and exposes request/profile/guide views (StrictMode=%s)', async (strict) => {
    await mountDashboard(strict);
    const create = await screen.findByRole('button', { name: 'Yeni Talep Oluştur' });
    expect(screen.queryByText('Veriler yükleniyor...')).not.toBeInTheDocument();
    fireEvent.click(create);
    expect(screen.getByText('New request form')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: /Profilim/ }));
    expect(screen.getByText('Loaded profile')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: /Rehberler/ }));
    expect(screen.getByText('Loaded guides')).toBeInTheDocument();
  });

  it('shows a failed load and recovers through retry under StrictMode', async () => {
    mocks.requests.mockRejectedValue(new Error('test outage'));
    const silence = vi.spyOn(console, 'error').mockImplementation(() => {});
    try {
      await mountDashboard();
      const retry = await screen.findByRole('button', { name: /Tekrar Dene/ });
      mocks.requests.mockResolvedValue([]);
      fireEvent.click(retry);
      expect(await screen.findByRole('button', { name: 'Yeni Talep Oluştur' })).toBeInTheDocument();
    } finally {
      silence.mockRestore();
    }
  });

  it('ignores a pending authorization result after unmount', async () => {
    let resolve!: (value: { authenticated: boolean }) => void;
    mocks.verify.mockReturnValue(new Promise<{ authenticated: boolean }>((done) => { resolve = done; }));
    const view = await mountDashboard();
    await waitFor(() => expect(mocks.verify).toHaveBeenCalled());
    view.unmount();
    await act(async () => { resolve({ authenticated: false }); });
    expect(mocks.push).not.toHaveBeenCalled();
  });
});
