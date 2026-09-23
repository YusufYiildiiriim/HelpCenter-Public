import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ForbiddenState } from '@/components/common/ForbiddenState';

describe('ForbiddenState', () => {
  it('varsayılan başlık ve mesajı render eder', () => {
    render(<ForbiddenState />);
    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.getByText('Erişim Yetkiniz Yok')).toBeInTheDocument();
    expect(
      screen.getByText('Bu içeriği görüntülemek veya bu işlemi gerçekleştirmek için yetkiniz bulunmuyor.')
    ).toBeInTheDocument();
  });

  it('özel title/message prop\'larını render eder', () => {
    render(<ForbiddenState title="Yasaklı" message="Bu modüle erişiminiz kısıtlanmış." />);
    expect(screen.getByText('Yasaklı')).toBeInTheDocument();
    expect(screen.getByText('Bu modüle erişiminiz kısıtlanmış.')).toBeInTheDocument();
  });

  it('variant="page" iken tam-alan stiliyle render edilir', () => {
    render(<ForbiddenState variant="page" />);
    expect(screen.getByRole('alert')).toHaveClass('py-32');
  });

  it('variant="inline" (varsayılan) iken kart-içi stiliyle render edilir', () => {
    render(<ForbiddenState />);
    const alert = screen.getByRole('alert');
    expect(alert).not.toHaveClass('py-32');
    expect(alert).toHaveClass('py-10');
  });

  it('ShieldAlert ikonunu render eder', () => {
    render(<ForbiddenState />);
    expect(screen.getByRole('alert').querySelector('svg')).toBeInTheDocument();
  });
});
