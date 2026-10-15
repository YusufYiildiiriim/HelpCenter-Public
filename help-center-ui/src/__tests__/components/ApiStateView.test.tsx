import { describe, it, expect, vi } from 'vitest';
import { render, screen, act } from '@testing-library/react';
import { ApiStateView } from '@/components/common/ApiStateView';

/**
 * ApiStateView manages 4 states from a single place, order of precedence:
 * isLoading > isForbidden > error > children. If this order is broken (e.g. if the
 * isForbidden check is accidentally placed ahead of isLoading), the user sees the wrong screen —
 * therefore, we verify that each precedence level wins even if subsequent conditions are true.
 */
describe('ApiStateView', () => {
  it('isLoading=true iken diğer tüm bayraklar dolu olsa bile spinner gösterir', () => {
    vi.useFakeTimers();
    render(
      <ApiStateView isLoading={true} isForbidden={true} error="bir hata" onRetry={vi.fn()}>
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    act(() => vi.advanceTimersByTime(200));
    expect(screen.getByRole('status')).toBeInTheDocument();
    vi.useRealTimers();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    expect(screen.queryByText('İçerik')).not.toBeInTheDocument();
  });

  it('isLoading=false + isForbidden=true iken error set olsa bile forbidden gösterir', () => {
    render(
      <ApiStateView isLoading={false} isForbidden={true} error="bir hata">
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.getByText('Erişim Yetkiniz Yok')).toBeInTheDocument();
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    expect(screen.queryByText('bir hata')).not.toBeInTheDocument();
    expect(screen.queryByText('İçerik')).not.toBeInTheDocument();
  });

  it('isLoading=false + isForbidden=false + error set iken güvenli hata metni gösterir', () => {
    render(
      <ApiStateView isLoading={false} isForbidden={false} error="beklenmedik hata">
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.getByText('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.')).toBeInTheDocument();
    expect(screen.queryByText('beklenmedik hata')).not.toBeInTheDocument();
    expect(screen.queryByText('Erişim Yetkiniz Yok')).not.toBeInTheDocument();
    expect(screen.queryByText('İçerik')).not.toBeInTheDocument();
  });

  it('hepsi false/null iken children (success durumu) gösterir', () => {
    render(
      <ApiStateView isLoading={false} isForbidden={false} error={null}>
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.getByText('İçerik')).toBeInTheDocument();
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it('Error instance içindeki ham mesajı göstermez', () => {
    render(
      <ApiStateView isLoading={false} error={new Error('ağ hatası')}>
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.getByText('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.')).toBeInTheDocument();
    expect(screen.queryByText('ağ hatası')).not.toBeInTheDocument();
  });

  it('axios olmayan error objesinin message/correlationId alanlarını göstermez', () => {
    render(
      <ApiStateView isLoading={false} error={{ message: 'sunucu hatası', correlationId: 'corr-1' }}>
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.getByText('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.')).toBeInTheDocument();
    expect(screen.queryByText('sunucu hatası')).not.toBeInTheDocument();
    expect(screen.queryByText('corr-1')).not.toBeInTheDocument();
  });

  it('onRetry error durumunda ErrorState\'e iletilir', () => {
    render(
      <ApiStateView isLoading={false} error="hata" onRetry={vi.fn()}>
        <div>İçerik</div>
      </ApiStateView>
    );

    expect(screen.getByRole('button', { name: /tekrar dene/i })).toBeInTheDocument();
  });
});


it.each(['success', 'error', 'forbidden'])('settles a fast request to %s without waiting for the spinner', (result) => {
  vi.useFakeTimers();
  const view = render(<ApiStateView isLoading><div>Haz?r</div></ApiStateView>);
  act(() => vi.advanceTimersByTime(50));
  view.rerender(<ApiStateView isLoading={false} isForbidden={result === 'forbidden'} error={result === 'error' ? 'failure' : null}><div>Haz?r</div></ApiStateView>);
  expect(screen.queryByRole('status')).not.toBeInTheDocument();
  if (result === 'success') expect(screen.getByText('Haz?r')).toBeInTheDocument();
  else expect(screen.getByRole('alert')).toBeInTheDocument();
  act(() => vi.advanceTimersByTime(500));
  expect(screen.queryByRole('status')).not.toBeInTheDocument();
  vi.useRealTimers();
});
