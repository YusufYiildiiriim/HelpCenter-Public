import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, act, cleanup } from '@testing-library/react';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';

describe('LoadingSpinner', () => {
  it('varsayılan olarak inline variant + md boyutunda render edilir', () => {
    render(<LoadingSpinner delayMs={0} />);
    const status = screen.getByRole('status');
    const icon = status.querySelector('svg');
    expect(icon).toHaveClass('w-8', 'h-8');
    expect(status).not.toHaveClass('py-32');
  });

  it('variant="page" iken büyük (lg) boyut ve tam-alan stiliyle render edilir', () => {
    render(<LoadingSpinner delayMs={0} variant="page" />);
    const status = screen.getByRole('status');
    const icon = status.querySelector('svg');
    expect(icon).toHaveClass('w-16', 'h-16');
    expect(status).toHaveClass('py-32');
  });

  it('explicit size, variant\'ın varsayılan boyutunu geçersiz kılar', () => {
    render(<LoadingSpinner delayMs={0} variant="page" size="sm" />);
    const icon = screen.getByRole('status').querySelector('svg');
    expect(icon).toHaveClass('w-5', 'h-5');
  });

  it('message verilirse gösterir, verilmezse göstermez', () => {
    const { rerender } = render(<LoadingSpinner delayMs={0} />);
    expect(screen.queryByText('Yükleniyor...')).not.toBeInTheDocument();

    rerender(<LoadingSpinner delayMs={0} message="Yükleniyor..." />);
    expect(screen.getByText('Yükleniyor...')).toBeInTheDocument();
  });

  it('erişilebilirlik için aria-live="polite" taşır', () => {
    render(<LoadingSpinner delayMs={0} />);
    expect(screen.getByRole('status')).toHaveAttribute('aria-live', 'polite');
  });
});


describe('LoadingSpinner delayed visibility', () => {
  afterEach(() => { cleanup(); vi.useRealTimers(); });
  it.each(['page', 'inline'] as const)('reserves space but only exposes a slow %s load after 200ms', (variant) => {
    vi.useFakeTimers();
    const { container } = render(<LoadingSpinner variant={variant} />);
    expect(container.firstChild).toHaveClass('invisible');
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    act(() => vi.advanceTimersByTime(199));
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    act(() => vi.advanceTimersByTime(1));
    expect(screen.getByRole('status', { name: 'Yükleniyor' })).not.toHaveClass('invisible');
    expect(screen.getByRole('status').querySelector('svg')).toHaveClass('motion-reduce:animate-none');
  });
  it('cancels pending visibility on unmount and starts a fresh delay on remount', () => {
    vi.useFakeTimers();
    const first = render(<LoadingSpinner />);
    act(() => vi.advanceTimersByTime(100));
    first.unmount();
    expect(vi.getTimerCount()).toBe(0);
    render(<LoadingSpinner message="Yeni istek" />);
    act(() => vi.advanceTimersByTime(100));
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    act(() => vi.advanceTimersByTime(100));
    expect(screen.getByRole('status', { name: 'Yeni istek' })).toBeInTheDocument();
  });
});
