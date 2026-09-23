import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';

describe('LoadingSpinner', () => {
  it('varsayılan olarak inline variant + md boyutunda render edilir', () => {
    render(<LoadingSpinner />);
    const status = screen.getByRole('status');
    const icon = status.querySelector('svg');
    expect(icon).toHaveClass('w-8', 'h-8');
    expect(status).not.toHaveClass('py-32');
  });

  it('variant="page" iken büyük (lg) boyut ve tam-alan stiliyle render edilir', () => {
    render(<LoadingSpinner variant="page" />);
    const status = screen.getByRole('status');
    const icon = status.querySelector('svg');
    expect(icon).toHaveClass('w-16', 'h-16');
    expect(status).toHaveClass('py-32');
  });

  it('explicit size, variant\'ın varsayılan boyutunu geçersiz kılar', () => {
    render(<LoadingSpinner variant="page" size="sm" />);
    const icon = screen.getByRole('status').querySelector('svg');
    expect(icon).toHaveClass('w-5', 'h-5');
  });

  it('message verilirse gösterir, verilmezse göstermez', () => {
    const { rerender } = render(<LoadingSpinner />);
    expect(screen.queryByText('Yükleniyor...')).not.toBeInTheDocument();

    rerender(<LoadingSpinner message="Yükleniyor..." />);
    expect(screen.getByText('Yükleniyor...')).toBeInTheDocument();
  });

  it('erişilebilirlik için aria-live="polite" taşır', () => {
    render(<LoadingSpinner />);
    expect(screen.getByRole('status')).toHaveAttribute('aria-live', 'polite');
  });
});
