import { act, cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { DataTable } from '@/components/common/DataTable';

const data = [{ name: 'Alpha' }, { name: 'Beta' }];
const columns = [{ accessorKey: 'name', header: 'Name' }];
const base = { data, columns, manualPagination: true, pageCount: 1 };
const advance = (ms: number) => act(() => vi.advanceTimersByTime(ms));
const type = (value: string) => fireEvent.change(screen.getByRole('textbox'), { target: { value } });

describe('DataTable search', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => { cleanup(); vi.useRealTimers(); });

  it('shows typing immediately and sends only the final search after 300ms idle', () => {
    const search = vi.fn();
    render(<DataTable {...base} searchValue="" onSearchChange={search} />);
    for (const value of ['t', 'te', 'tes', 'test']) { type(value); advance(100); }
    expect(screen.getByRole('textbox')).toHaveValue('test');
    expect(search).not.toHaveBeenCalled();
    advance(199);
    expect(search).not.toHaveBeenCalled();
    advance(1);
    expect(search).toHaveBeenCalledExactlyOnceWith('test');
  });

  it('clearing cancels the pending text search', () => {
    const search = vi.fn();
    render(<DataTable {...base} searchValue="old" onSearchChange={search} />);
    type('new'); advance(150); type(''); advance(300);
    expect(search).toHaveBeenCalledExactlyOnceWith('');
  });

  it('cancels pending search on unmount', () => {
    const search = vi.fn();
    const view = render(<DataTable {...base} onSearchChange={search} />);
    type('test'); view.unmount(); advance(300);
    expect(search).not.toHaveBeenCalled();
  });

  it('respects external resets and cancels pending search', () => {
    const search = vi.fn();
    const view = render(<DataTable {...base} searchValue="old" onSearchChange={search} />);
    type('pending');
    view.rerender(<DataTable {...base} searchValue="" onSearchChange={search} />);
    expect(screen.getByRole('textbox')).toHaveValue('');
    advance(300);
    expect(search).not.toHaveBeenCalled();
  });

  it('uses the latest callback without extending the wait on rerender', () => {
    const first = vi.fn(), latest = vi.fn();
    const view = render(<DataTable {...base} searchValue="" onSearchChange={first} />);
    type('test'); advance(200);
    view.rerender(<DataTable {...base} searchValue="" onSearchChange={latest} />);
    advance(100);
    expect(first).not.toHaveBeenCalled();
    expect(latest).toHaveBeenCalledExactlyOnceWith('test');
  });

  it('keeps local filtering immediate', () => {
    render(<DataTable data={data} columns={columns} />);
    type('Alpha');
    expect(screen.queryByText('Beta')).not.toBeInTheDocument();
    expect(screen.getAllByText('Alpha').length).toBeGreaterThan(0);
  });
});
