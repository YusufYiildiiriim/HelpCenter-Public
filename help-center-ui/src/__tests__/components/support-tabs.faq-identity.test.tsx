import React from 'react';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SupportTabs } from '@/app/components/home/SupportTabs';
import { LocaleProvider } from '@/context/LocaleContext';
import type { PublicFaq } from '@/services/public/PublicFaqService';

vi.mock('@/app/components/home/MindMapFlow', () => ({ MindMapFlow: () => null }));
vi.mock('next/navigation', () => ({ usePathname: () => '/', useRouter: () => ({ push: vi.fn() }) }));
vi.mock('framer-motion', () => ({
  AnimatePresence: ({ children }: { children: React.ReactNode }) => children,
  motion: {
    div: ({ initial, animate, exit, transition, ...props }: Record<string, unknown>) => {
      void initial; void animate; void exit; void transition;
      return React.createElement('div', props);
    },
  },
}));

afterEach(cleanup);

describe('FAQ card identity', () => {
  it('preserves each card and keyboard focus when sorting changes its position', () => {
    const faqs: PublicFaq[] = [
      { publicId: '60000000-0000-4000-8000-000000000002', title: 'Zebra', description: 'Last alphabetically' },
      { publicId: '60000000-0000-4000-8000-000000000001', title: 'Alpha', description: 'First alphabetically' },
    ];
    const onSelectFaq = vi.fn();
    const props = {
      activeTab: 'sss' as const, loading: false, filteredFaqs: faqs,
      filteredGuides: [], setSelectedGuide: vi.fn(), onSelectFaq,
    };
    const { rerender } = render(<LocaleProvider initialLocale="tr-TR"><SupportTabs {...props} sortBy="newest" /></LocaleProvider>);
    const zebra = screen.getByRole('button', { name: 'Zebra SSS detayını aç' });
    const alpha = screen.getByRole('button', { name: 'Alpha SSS detayını aç' });
    zebra.focus();

    rerender(<LocaleProvider initialLocale="tr-TR"><SupportTabs {...props} sortBy="az" /></LocaleProvider>);

    expect(screen.getByRole('button', { name: 'Zebra SSS detayını aç' })).toBe(zebra);
    expect(screen.getByRole('button', { name: 'Alpha SSS detayını aç' })).toBe(alpha);
    expect(document.activeElement).toBe(zebra);
    expect(screen.getAllByRole('button')[0]).toBe(alpha);
    fireEvent.click(zebra);
    expect(onSelectFaq).toHaveBeenCalledExactlyOnceWith(faqs[0]);
  });
});
