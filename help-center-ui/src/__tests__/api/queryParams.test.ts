import { describe, it, expect } from 'vitest';
import { buildQueryParams } from '@/lib/api/queryParams';
import type { QueryParamInput } from '@/lib/api/queryParams';

describe('buildQueryParams', () => {
  it('undefined ve null değerleri eler', () => {
    expect(buildQueryParams({ a: undefined, b: null, c: 'x' })).toEqual({ c: 'x' });
  });

  it('boş string değerleri eler', () => {
    expect(buildQueryParams({ search: '' })).toEqual({});
  });

  it('boolean değeri string\'e çevirir', () => {
    expect(buildQueryParams({ onlyActive: true, includeDeleted: false })).toEqual({
      onlyActive: 'true',
      includeDeleted: 'false',
    });
  });

  it('number değeri string\'e çevirir (0 dahil elenmez)', () => {
    expect(buildQueryParams({ pageNumber: 1, pageSize: 0 })).toEqual({
      pageNumber: '1',
      pageSize: '0',
    });
  });

  it('Date değerini ISO string\'e çevirir', () => {
    const date = new Date('2026-01-15T10:30:00.000Z');
    expect(buildQueryParams({ createdAfter: date })).toEqual({
      createdAfter: '2026-01-15T10:30:00.000Z',
    });
  });

  it('dizi değerleri filtreleyip serileştirir (null/undefined elemanlar elenir)', () => {
    // The type signature constrains array elements to QueryParamPrimitive; type assertion
    // is required to prove that runtime defensive code also filters out null/undefined elements.
    const input = { statusIds: [1, 2, null, undefined, 3] } as unknown as Record<string, QueryParamInput>;
    expect(buildQueryParams(input)).toEqual({ statusIds: ['1', '2', '3'] });
  });

  it('boş dizi anahtarı sonuca dahil etmez', () => {
    expect(buildQueryParams({ statusIds: [] })).toEqual({});
  });

  it('tüm elemanları filtrelenen dizi (örn. hepsi null) anahtarı sonuca dahil etmez', () => {
    const input = { statusIds: [null, undefined] } as unknown as Record<string, QueryParamInput>;
    expect(buildQueryParams(input)).toEqual({});
  });

  it('boş obje için boş obje döner', () => {
    expect(buildQueryParams({})).toEqual({});
  });

  it('karışık tipli obje: her tip doğru şekilde işlenir', () => {
    const date = new Date('2026-02-01T00:00:00.000Z');
    const result = buildQueryParams({
      search: 'kullanıcı',
      onlyActive: true,
      pageNumber: 2,
      pageSize: undefined,
      createdAfter: date,
      tags: ['a', 'b', ''],
      empty: '',
      missing: null,
    });
    expect(result).toEqual({
      search: 'kullanıcı',
      onlyActive: 'true',
      pageNumber: '2',
      createdAfter: '2026-02-01T00:00:00.000Z',
      tags: ['a', 'b'],
    });
  });

  it('gerçekçi senaryo: sayfalama + arama + boolean filtre birlikte (AdminModulesController.GetAll ile uyumlu)', () => {
    // Backend signature: GetAll([FromQuery] bool onlyActive = false, [FromQuery] int? pageNumber = null,
    //                     [FromQuery] int? pageSize = null, [FromQuery] string? search = null)
    const result = buildQueryParams({
      onlyActive: true,
      pageNumber: 2,
      pageSize: 25,
      search: 'faturalama',
    });
    expect(result).toEqual({
      onlyActive: 'true',
      pageNumber: '2',
      pageSize: '25',
      search: 'faturalama',
    });
  });
});
