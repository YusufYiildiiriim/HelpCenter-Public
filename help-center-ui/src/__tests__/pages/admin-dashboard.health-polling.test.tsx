import { act, cleanup, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import AdminDashboard from '@/app/admin/page';
import { AdminStatisticsService } from '@/services/admin/AdminStatisticsService';
import { apiMetrics } from '@/lib/axios';
import { Profiler } from 'react';
import { SystemHealthCard } from '@/app/admin/components/SystemHealthCard';

const { statsRender, chartRender } = vi.hoisted(() => ({ statsRender: vi.fn(), chartRender: vi.fn() }));
vi.mock('next/navigation', () => ({ useRouter: () => ({ push: vi.fn() }) }));
vi.mock('@/context/AuthContext', () => ({ useAuth: () => ({ userInfo: {} }) }));
vi.mock('@/lib/permissions', () => ({ usePermission: () => ({ canRead: () => true }) }));
vi.mock('@/lib/usePageTitle', () => ({ usePageTitle: vi.fn() }));
vi.mock('@/services/admin/AdminStatisticsService', () => ({
  AdminStatisticsService: { getStatistics: vi.fn(), getReports: vi.fn() },
}));
vi.mock('@/app/admin/components/DashboardHeader', () => ({ DashboardHeader: () => null }));
vi.mock('@/app/admin/components/DashboardStats', () => ({ DashboardStats: () => { statsRender(); return null; } }));
vi.mock('@/app/admin/components/WeeklyAnalysisChart', () => ({ WeeklyAnalysisChart: () => { chartRender(); return null; } }));

describe('Dashboard health polling isolation', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.clearAllMocks();
    apiMetrics.averageDuration = 0;
    apiMetrics.lastDuration = 0;
    apiMetrics.slowRequests = [];
    vi.mocked(AdminStatisticsService.getStatistics).mockResolvedValue({ allowedFields: ['systemHealth', 'weeklyChart'] });
    vi.mocked(AdminStatisticsService.getReports).mockResolvedValue({});
  });
  afterEach(() => { cleanup(); vi.useRealTimers(); });

  it('updates health values without rerendering statistics or charts or refetching reports', async () => {
    await act(async () => { render(<AdminDashboard />); });
    statsRender.mockClear(); chartRender.mockClear();
    apiMetrics.averageDuration = 250;
    apiMetrics.lastDuration = 650;
    apiMetrics.slowRequests.push({ url: '/test', duration: 650, time: new Date() });
    act(() => vi.advanceTimersByTime(3000));
    expect(screen.getByText('250')).toBeInTheDocument();
    expect(screen.getByText('650')).toBeInTheDocument();
    expect(screen.getByText('İyi')).toBeInTheDocument();
    expect(statsRender).not.toHaveBeenCalled();
    expect(chartRender).not.toHaveBeenCalled();
    expect(AdminStatisticsService.getStatistics).toHaveBeenCalledTimes(1);
    expect(AdminStatisticsService.getReports).toHaveBeenCalledTimes(1);
  });

  it('keeps other widgets untouched during idle polling', async () => {
    await act(async () => { render(<AdminDashboard />); });
    statsRender.mockClear(); chartRender.mockClear();
    for (let tick = 0; tick < 3; tick++) act(() => vi.advanceTimersByTime(3000));
    expect(statsRender).not.toHaveBeenCalled();
    expect(chartRender).not.toHaveBeenCalled();
  });

  it('removes its polling timer on unmount', async () => {
    let view!: ReturnType<typeof render>;
    await act(async () => { view = render(<AdminDashboard />); });
    view.unmount();
    expect(vi.getTimerCount()).toBe(0);
  });

  it('does not commit a health render when displayed values remain unchanged', () => {
    const commits = vi.fn();
    render(<Profiler id="health" onRender={commits}><SystemHealthCard /></Profiler>);
    commits.mockClear();
    for (let tick = 0; tick < 3; tick++) act(() => vi.advanceTimersByTime(3000));
    expect(commits).not.toHaveBeenCalled();
  });
});
