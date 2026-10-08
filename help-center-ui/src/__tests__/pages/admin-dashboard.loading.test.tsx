import { act, cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import AdminDashboard from '@/app/admin/page';
import { AdminStatisticsService } from '@/services/admin/AdminStatisticsService';
import type { AdminReports, AdminStatistics } from '@/services/admin/AdminStatisticsService';

vi.mock('next/navigation', () => ({ useRouter: () => ({ push: vi.fn() }) }));
vi.mock('@/context/AuthContext', () => ({ useAuth: () => ({ userInfo: {} }) }));
vi.mock('@/lib/permissions', () => ({ usePermission: () => ({ canRead: () => true }) }));
vi.mock('@/lib/usePageTitle', () => ({ usePageTitle: vi.fn() }));
vi.mock('@/services/admin/AdminStatisticsService', () => ({
  AdminStatisticsService: { getStatistics: vi.fn(), getReports: vi.fn() },
}));
vi.mock('@/app/admin/components/DashboardHeader', () => ({ DashboardHeader: () => null }));
vi.mock('@/app/admin/components/DashboardStats', () => ({
  DashboardStats: ({ loading, stats }: { loading: boolean; stats: AdminStatistics | null }) =>
    <div data-testid="stats">{loading ? 'loading' : stats?.totalRequests ?? 'empty'}</div>,
}));
vi.mock('@/app/admin/components/WeeklyAnalysisChart', () => ({
  WeeklyAnalysisChart: ({ loading, points }: { loading: boolean; points?: unknown[] }) =>
    <div data-testid="reports">{loading ? 'loading' : points?.length ?? 'empty'}</div>,
}));

function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (reason: Error) => void;
  const promise = new Promise<T>((res, rej) => { resolve = res; reject = rej; });
  return { promise, resolve, reject };
}
const stats: AdminStatistics = { totalRequests: 42, allowedFields: ['weeklyChart'] };
const reports: AdminReports = { dashboard: { weeklyOpened: [{ date: '2026-10-08', count: 3 }] } };

describe('AdminDashboard independent loading', () => {
  let statsRequest: ReturnType<typeof deferred<AdminStatistics>>;
  let reportsRequest: ReturnType<typeof deferred<AdminReports>>;
  beforeEach(() => {
    vi.clearAllMocks();
    statsRequest = deferred<AdminStatistics>();
    reportsRequest = deferred<AdminReports>();
    vi.mocked(AdminStatisticsService.getStatistics).mockReturnValue(statsRequest.promise);
    vi.mocked(AdminStatisticsService.getReports).mockReturnValue(reportsRequest.promise);
  });
  afterEach(() => { cleanup(); vi.restoreAllMocks(); });

  it('shows statistics while reports are still pending', async () => {
    render(<AdminDashboard />);
    expect(AdminStatisticsService.getStatistics).toHaveBeenCalledTimes(1);
    expect(AdminStatisticsService.getReports).toHaveBeenCalledTimes(1);
    await act(async () => statsRequest.resolve(stats));
    expect(screen.getByTestId('stats')).toHaveTextContent('42');
    expect(screen.getByTestId('reports')).toHaveTextContent('loading');
    await act(async () => reportsRequest.resolve(reports));
    expect(screen.getByTestId('reports')).toHaveTextContent('1');
  });

  it('shows reports while statistics are still pending', async () => {
    render(<AdminDashboard />);
    await act(async () => reportsRequest.resolve(reports));
    expect(screen.getByTestId('reports')).toHaveTextContent('1');
    expect(screen.getByTestId('stats')).toHaveTextContent('loading');
    await act(async () => statsRequest.resolve(stats));
  });

  it('settles a failed reports request without blocking statistics', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    render(<AdminDashboard />);
    await act(async () => reportsRequest.reject(new Error('reports unavailable')));
    expect(screen.getByTestId('reports')).toHaveTextContent('empty');
    expect(screen.getByTestId('stats')).toHaveTextContent('loading');
    await act(async () => statsRequest.resolve(stats));
    expect(screen.getByTestId('stats')).toHaveTextContent('42');
  });

  it('publishes refreshed statistics before refreshed reports and retains existing charts', async () => {
    render(<AdminDashboard />);
    await act(async () => { statsRequest.resolve(stats); reportsRequest.resolve(reports); });
    const newStats = deferred<AdminStatistics>();
    const newReports = deferred<AdminReports>();
    vi.mocked(AdminStatisticsService.getStatistics).mockReturnValue(newStats.promise);
    vi.mocked(AdminStatisticsService.getReports).mockReturnValue(newReports.promise);
    fireEvent.click(screen.getByRole('button', { name: 'Yenile' }));
    await act(async () => newStats.resolve({ ...stats, totalRequests: 99 }));
    expect(screen.getByTestId('stats')).toHaveTextContent('99');
    expect(screen.getByTestId('reports')).toHaveTextContent('1');
    expect(screen.getByRole('button', { name: 'Yenileniyor' })).toBeDisabled();
    await act(async () => newReports.resolve(reports));
    expect(screen.getByRole('button', { name: 'Yenile' })).toBeEnabled();
  });
});
