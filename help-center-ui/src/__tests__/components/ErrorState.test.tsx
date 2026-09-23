import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ErrorState } from '@/components/common/ErrorState';

describe('ErrorState', () => {
  it('varsayılan başlık ve mesajı render eder', () => {
    render(<ErrorState />);
    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.getByText('Bir şeyler ters gitti')).toBeInTheDocument();
    expect(screen.getByText('İşlem gerçekleştirilirken beklenmeyen bir hata oluştu.')).toBeInTheDocument();
  });

  it('özel title/message prop\'larını render eder', () => {
    render(<ErrorState title="Yükleme Başarısız" message="Sunucudan veri alınamadı." />);
    expect(screen.getByText('Yükleme Başarısız')).toBeInTheDocument();
    expect(screen.getByText('Sunucudan veri alınamadı.')).toBeInTheDocument();
  });

  it('onRetry verilmezse "Tekrar Dene" butonu render edilmez', () => {
    render(<ErrorState />);
    expect(screen.queryByRole('button', { name: /tekrar dene/i })).not.toBeInTheDocument();
  });

  it('onRetry verilirse buton render edilir ve tıklanınca callback tetiklenir', async () => {
    const user = userEvent.setup();
    const onRetry = vi.fn();
    render(<ErrorState onRetry={onRetry} />);

    const button = screen.getByRole('button', { name: /tekrar dene/i });
    await user.click(button);

    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it('correlationId verilirse destek bilgisi bölümünde gösterir', () => {
    render(<ErrorState correlationId="corr-abc-123" />);
    expect(screen.getByText('corr-abc-123')).toBeInTheDocument();
  });

  it('correlationId verilmezse destek bilgisi bölümü render edilmez', () => {
    render(<ErrorState />);
    expect(screen.queryByText('Destek Bilgisi')).not.toBeInTheDocument();
  });
});
