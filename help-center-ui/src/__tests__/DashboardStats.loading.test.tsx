import { cleanup, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it } from 'vitest';
import { DashboardStats } from '@/app/admin/components/DashboardStats';
import type { AdminStatistics } from '@/services/admin/AdminStatisticsService';

const stats: AdminStatistics = {
  totalRequests: 1200, completedRequests: 800, activeRequests: 400,
  totalMessages: 4200, totalCompanies: 20, totalCustomers: 50,
};
afterEach(cleanup);

describe('DashboardStats loading layout', () => {
  it('reserves five card slots before statistics arrive without presenting zero metrics', () => {
    const { container } = render(<DashboardStats stats={null} loading />);
    expect(screen.getByRole('status', { name: 'İstatistikler yükleniyor' })).toBeInTheDocument();
    expect(container.querySelector('.grid')?.children).toHaveLength(5);
    expect(screen.queryByText('0')).not.toBeInTheDocument();
  });

  it('only reserves slots permitted by the widget filter', () => {
    const { container } = render(<DashboardStats stats={null} loading canWidget={(key) => key === 'totalRequests'} />);
    expect(container.querySelector('.grid')?.children).toHaveLength(1);
  });

  it('does not create slots for a user with no permitted metrics', () => {
    const { container } = render(<DashboardStats stats={null} loading canWidget={() => false} />);
    expect(container.querySelector('.grid')?.children).toHaveLength(0);
  });

  it('replaces loading slots with actual permitted metrics', async () => {
    const view = render(<DashboardStats stats={null} loading />);
    view.rerender(<DashboardStats stats={stats} loading={false} />);
    expect(screen.queryByRole('status', { name: 'İstatistikler yükleniyor' })).not.toBeInTheDocument();
    expect(view.container.querySelector('.grid')?.children).toHaveLength(5);
    await waitFor(() => expect(screen.getByText('1.200')).toBeInTheDocument());
  });

  it('does not reveal null or missing metrics returned by the server', () => {
    render(<DashboardStats stats={{ totalRequests: 0, totalCompanies: null }} loading={false} />);
    expect(screen.getByText('Toplam Talep')).toBeInTheDocument();
    expect(screen.queryByText('Toplam Firma')).not.toBeInTheDocument();
    expect(screen.queryByText('Toplam Müşteri')).not.toBeInTheDocument();
  });

  it('removes loading slots when the request settles without data', () => {
    const view = render(<DashboardStats stats={null} loading />);
    view.rerender(<DashboardStats stats={null} loading={false} />);
    expect(screen.queryByRole('status', { name: 'İstatistikler yükleniyor' })).not.toBeInTheDocument();
    expect(view.container.querySelector('.grid')?.children).toHaveLength(0);
  });
});
