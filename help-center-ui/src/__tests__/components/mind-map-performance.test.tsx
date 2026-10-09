import React from 'react';
import { act, cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MindMapFlow } from '@/app/components/home/MindMapFlow';
import type { PublicGuide } from '@/services/public/PublicGuideService';

const renders = vi.hoisted(() => ({ nodes: 0, paths: 0 }));
vi.mock('framer-motion', () => ({
  AnimatePresence: ({ children }: { children: React.ReactNode }) => children,
  motion: {
    div: ({ initial, animate, exit, transition, ...props }: Record<string, unknown>) => {
      void initial; void animate; void exit; void transition;
      renders.nodes++;
      return React.createElement('div', props);
    },
    path: ({ initial, animate, exit, transition, ...props }: Record<string, unknown>) => {
      void initial; void animate; void exit; void transition;
      renders.paths++;
      return React.createElement('path', props);
    },
  },
}));

const guides: PublicGuide[] = Array.from({ length: 100 }, (_, i) => ({
  publicId: `guide-${i}`, title: `Guide ${i}`, description: '', module: 'Module',
  previousGuidePublicId: i === 0 ? null : `guide-${Math.floor((i - 1) / 3)}`,
}));

describe('MindMap interaction performance and graph invalidation', () => {
  beforeEach(() => { renders.nodes = 0; renders.paths = 0; });
  afterEach(cleanup);

  it('keeps all nodes and connections mounted without rerendering them during desktop pan and zoom', () => {
    const { container } = render(<MindMapFlow guides={guides} onSelectGuide={vi.fn()} />);
    const viewport = container.querySelector('.touch-none')!;
    const initial = { ...renders };
    expect(initial).toEqual({ nodes: 100, paths: 99 });
    const before = (viewport.lastElementChild as HTMLElement).style.transform;
    fireEvent.mouseDown(viewport, { clientX: 20, clientY: 20 });
    for (let i = 0; i < 20; i++) fireEvent.mouseMove(viewport, { clientX: 40 + i, clientY: 60 + i });
    fireEvent.mouseUp(viewport);
    expect((viewport.lastElementChild as HTMLElement).style.transform).not.toBe(before);
    fireEvent.wheel(viewport, { ctrlKey: true, deltaY: -40, clientX: 100, clientY: 100 });
    fireEvent.click(screen.getByTitle('Yakınlaştır'));
    fireEvent.click(screen.getByTitle('Görünümü Sıfırla'));
    expect(renders).toEqual(initial);
    expect((viewport.lastElementChild as HTMLElement).style.transform).toBe(before);
  });

  it('keeps graph renders constant during touch pan and pinch', () => {
    const { container } = render(<MindMapFlow guides={guides} onSelectGuide={vi.fn()} />);
    const viewport = container.querySelector('.touch-none')!;
    const initial = { ...renders };
    const before = (viewport.lastElementChild as HTMLElement).style.transform;
    const touch = (x: number, y: number) => ({ clientX: x, clientY: y, target: viewport });
    fireEvent.touchStart(viewport, { touches: [touch(10, 10)] });
    fireEvent.touchMove(viewport, { touches: [touch(70, 80)] });
    fireEvent.touchEnd(viewport, { touches: [] });
    fireEvent.touchStart(viewport, { touches: [touch(20, 20), touch(100, 20)] });
    fireEvent.touchMove(viewport, { touches: [touch(10, 20), touch(140, 20)] });
    fireEvent.touchEnd(viewport, { touches: [] });
    expect((viewport.lastElementChild as HTMLElement).style.transform).not.toBe(before);
    expect(renders).toEqual(initial);
  });

  it('accumulates a burst of native wheel events without losing zoom steps', () => {
    const { container } = render(<MindMapFlow guides={guides} onSelectGuide={vi.fn()} />);
    const viewport = container.querySelector('.touch-none')!;
    const initial = { ...renders };
    act(() => {
      for (let i = 0; i < 20; i++) {
        const event = new WheelEvent('wheel', { deltaY: -8, bubbles: true, cancelable: true });
        // happy-dom's WheelEvent doesn't currently inherit the mouse modifier fields.
        Object.defineProperties(event, { ctrlKey: { value: true }, clientX: { value: 100 }, clientY: { value: 100 } });
        viewport.dispatchEvent(event);
      }
    });
    expect(screen.getByText('140%')).toBeInTheDocument();
    expect(renders).toEqual(initial);
  });

  it('updates collapse/expand geometry and guide selection after memoization', () => {
    const select = vi.fn();
    const { container } = render(<MindMapFlow guides={guides.slice(0, 4)} onSelectGuide={select} />);
    expect(container.querySelectorAll('path[d^="M "]')).toHaveLength(3);
    // Child centers are 66, 138, 210; the root center is 138.
    expect(container.querySelector('path[d^="M "]')).toHaveAttribute('d', 'M 300 138 C 345 138, 345 66, 390 66');
    fireEvent.click(screen.getByRole('button', { name: 'Guide 0 alt adımlarını daralt' }));
    expect(container.querySelectorAll('path[d^="M "]')).toHaveLength(0);
    expect(screen.queryByText('Guide 1')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Guide 0 alt adımlarını göster' }));
    fireEvent.click(screen.getByText('Guide 1'));
    expect(select).toHaveBeenCalledWith(guides[1]);
  });

  it('refreshes callbacks, guide data and module labels rather than retaining stale graph props', () => {
    const first = vi.fn(), second = vi.fn();
    const { rerender } = render(<MindMapFlow guides={guides.slice(0, 2)} onSelectGuide={first} />);
    rerender(<MindMapFlow guides={[{ ...guides[0], title: 'Updated' }]} selectedModule="Module" onSelectGuide={second} />);
    expect(screen.queryByText('Guide 1')).not.toBeInTheDocument();
    fireEvent.click(screen.getByText('Updated'));
    expect(first).not.toHaveBeenCalled();
    expect(second).toHaveBeenCalledOnce();
    expect(screen.queryByText('Module')).not.toBeInTheDocument();
    fireEvent.click(screen.getByText('Sıralı Akış'));
    expect(screen.getByText('Updated')).toBeInTheDocument();
    fireEvent.click(screen.getByText('Ağaç Bağlantıları'));
    expect(screen.getByText('Updated')).toBeInTheDocument();
  });
});
