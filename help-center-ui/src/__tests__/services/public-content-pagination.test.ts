import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import MockAdapter from 'axios-mock-adapter';
import api from '@/lib/axios';
import { PublicFaqService } from '@/services/public/PublicFaqService';
import { PublicGuideService } from '@/services/public/PublicGuideService';

describe('public content pagination services', () => {
  let mock: MockAdapter;

  beforeEach(() => {
    mock = new MockAdapter(api);
  });

  afterEach(() => {
    mock.restore();
  });

  it('sends guide page parameters and preserves the paginated API response', async () => {
    mock.onGet('/api/public/guides').reply((config) => {
      expect(config.params).toEqual({ projectId: 4, module: 'Billing', pageNumber: 2, pageSize: 12 });
      return [200, { items: [{ publicId: 'guide-2', title: 'Step 2' }], totalCount: 25, totalPages: 3, currentPage: 2, pageSize: 12 }];
    });

    const result = await PublicGuideService.getPaginated({ projectId: 4, module: 'Billing', pageNumber: 2, pageSize: 12 });

    expect(result).toMatchObject({ totalCount: 25, totalPages: 3, currentPage: 2, pageSize: 12 });
    expect(result.items).toHaveLength(1);
  });

  it('uses the explicit unlimited page size only for the guide reader chain', async () => {
    mock.onGet('/api/public/guides').reply((config) => {
      expect(config.params).toEqual({ projectId: 4, pageSize: 0 });
      return [200, { items: [{ publicId: 'guide-1', title: 'Step 1' }], totalCount: 1, totalPages: 1, currentPage: 1, pageSize: 0 }];
    });

    await expect(PublicGuideService.getAllForChain({ projectId: 4 })).resolves.toHaveLength(1);
  });

  it('sends FAQ page parameters and preserves the paginated API response', async () => {
    mock.onGet('/api/public/faq').reply((config) => {
      expect(config.params).toEqual({ projectId: 4, moduleId: 7, pageNumber: 3, pageSize: 24 });
      return [200, { items: [{ title: 'FAQ 3', description: 'Answer' }], totalCount: 49, totalPages: 3, currentPage: 3, pageSize: 24 }];
    });

    const result = await PublicFaqService.getPaginated({ projectId: 4, moduleId: 7, pageNumber: 3, pageSize: 24 });

    expect(result).toMatchObject({ totalCount: 49, totalPages: 3, currentPage: 3, pageSize: 24 });
    expect(result.items).toHaveLength(1);
  });
});
