import { describe, it, expect } from 'vitest';
import {
  getApiErrorMessage,
  getApiErrorCode,
  getApiErrorCorrelationId,
  getApiErrorDetails,
  getApiErrorStatus,
  isUnauthorizedError,
  isForbiddenError,
  isValidationError,
  isNetworkError,
} from '@/lib/api/errors';

/**
 * `axios.isAxiosError` performs a duck-type check (`isObject && isAxiosError === true`),
 * no need to instantiate a real `AxiosError` class — tests mock this shape.
 */
function makeAxiosError(opts: {
  status?: number;
  data?: unknown;
  message?: string;
  withResponse?: boolean;
}): unknown {
  const { status, data, message = 'Request failed with status code', withResponse = true } = opts;
  return {
    isAxiosError: true,
    message,
    response: withResponse ? { status, data, statusText: '', headers: {}, config: {} } : undefined,
  };
}

function makeBackendEnvelope(overrides: {
  message?: string;
  code?: string;
  details?: string[];
} = {}) {
  return {
    success: false,
    message: overrides.message,
    error: {
      code: overrides.code ?? 'GENERIC_ERROR',
      message: overrides.message ?? 'Backend hata mesajı',
      details: overrides.details,
    },
  };
}

describe('getApiErrorMessage', () => {
  it('400 response içindeki backend mesajını değil güvenli yerel metni döner', () => {
    const error = makeAxiosError({
      status: 400,
      data: makeBackendEnvelope({ message: 'SQL Server connection string leaked' }),
    });
    expect(getApiErrorMessage(error)).toBe('Gönderilen bilgiler geçersiz. Lütfen alanları kontrol edin.');
  });

  it('5xx response içindeki backend mesajını göstermez', () => {
    const error = makeAxiosError({
      status: 500,
      data: { success: false, message: 'stack trace: internal service host' },
    });
    expect(getApiErrorMessage(error)).toBe('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.');
  });

  it('response yoksa (network hatası) sabit ağ hatası mesajını döner', () => {
    const error = makeAxiosError({ withResponse: false });
    expect(getApiErrorMessage(error)).toBe(
      'Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edip tekrar deneyin.'
    );
  });

  it('backend gövdesi boş/parse edilemez ise Axios mesajını göstermez', () => {
    const error = makeAxiosError({ status: 500, data: undefined, message: 'Request failed with status code 500' });
    expect(getApiErrorMessage(error)).toBe('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.');
  });

  it('axios hatası olmayan Error mesajını göstermez', () => {
    expect(getApiErrorMessage(new Error('internal stack detail'))).toBe(
      'İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.'
    );
  });

  it('hiçbir tanınan şekle uymuyorsa genel fallback mesajı döner', () => {
    expect(getApiErrorMessage('string olmayan bir şey')).toBe(
      'İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.'
    );
    expect(getApiErrorMessage(null)).toBe('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.');
  });

  it('message boş olsa da Axios mesajına düşmez', () => {
    const error = makeAxiosError({
      status: 400,
      data: { success: false, message: '', error: { code: 'X', message: '' } },
      message: 'Request failed with status code 400',
    });
    expect(getApiErrorMessage(error)).toBe('Gönderilen bilgiler geçersiz. Lütfen alanları kontrol edin.');
  });
});

describe('getApiErrorCorrelationId', () => {
  it('yalnız correlation idyi destek bilgisi olarak ayırır', () => {
    const error = makeAxiosError({
      status: 500,
      data: { success: false, message: 'internal', correlationId: 'corr-123' },
    });
    expect(getApiErrorCorrelationId(error)).toBe('corr-123');
  });

  it('geçerli correlation id yoksa null döner', () => {
    expect(getApiErrorCorrelationId(makeAxiosError({ status: 500 }))).toBeNull();
  });
});

describe('getApiErrorCode', () => {
  it('backend error.code alanını döner', () => {
    const error = makeAxiosError({ status: 400, data: makeBackendEnvelope({ code: 'VALIDATION_FAILED' }) });
    expect(getApiErrorCode(error)).toBe('VALIDATION_FAILED');
  });

  it('error objesi yoksa null döner', () => {
    const error = makeAxiosError({ status: 500, data: { success: false, message: 'x' } });
    expect(getApiErrorCode(error)).toBeNull();
  });

  it('axios hatası değilse null döner', () => {
    expect(getApiErrorCode(new Error('boom'))).toBeNull();
  });
});

describe('getApiErrorDetails', () => {
  it('details dizisini döner', () => {
    const error = makeAxiosError({
      status: 400,
      data: makeBackendEnvelope({ details: ['Ad zorunludur', 'E-posta geçersiz'] }),
    });
    expect(getApiErrorDetails(error)).toEqual(['Ad zorunludur', 'E-posta geçersiz']);
  });

  it('details yoksa null döner', () => {
    const error = makeAxiosError({ status: 400, data: makeBackendEnvelope() });
    expect(getApiErrorDetails(error)).toBeNull();
  });

  it('details dizi değilse null döner', () => {
    const error = makeAxiosError({
      status: 400,
      data: { success: false, error: { code: 'X', message: 'y', details: 'dizi değil' } },
    });
    expect(getApiErrorDetails(error)).toBeNull();
  });
});

describe('getApiErrorStatus', () => {
  it('response.status değerini döner', () => {
    expect(getApiErrorStatus(makeAxiosError({ status: 404 }))).toBe(404);
  });

  it('axios hatası değilse null döner', () => {
    expect(getApiErrorStatus(new Error('boom'))).toBeNull();
    expect(getApiErrorStatus(undefined)).toBeNull();
  });

  it('response yoksa null döner', () => {
    expect(getApiErrorStatus(makeAxiosError({ withResponse: false }))).toBeNull();
  });
});

describe('isNetworkError', () => {
  it('response olmayan axios hatasında true döner', () => {
    expect(isNetworkError(makeAxiosError({ withResponse: false }))).toBe(true);
  });

  it('response olan axios hatasında false döner', () => {
    expect(isNetworkError(makeAxiosError({ status: 500 }))).toBe(false);
  });

  it('axios hatası değilse false döner', () => {
    expect(isNetworkError(new Error('boom'))).toBe(false);
  });
});

describe('isUnauthorizedError / isForbiddenError', () => {
  it.each([
    [401, true, false],
    [403, false, true],
    [404, false, false],
    [500, false, false],
  ])('status %i için unauthorized=%s forbidden=%s', (status, expectedUnauthorized, expectedForbidden) => {
    const error = makeAxiosError({ status });
    expect(isUnauthorizedError(error)).toBe(expectedUnauthorized);
    expect(isForbiddenError(error)).toBe(expectedForbidden);
  });
});

describe('isValidationError', () => {
  it('400 + dolu details dizisi varsa true döner', () => {
    const error = makeAxiosError({
      status: 400,
      data: makeBackendEnvelope({ details: ['Alan zorunlu'] }),
    });
    expect(isValidationError(error)).toBe(true);
  });

  it('400 ama details boşsa false döner', () => {
    const error = makeAxiosError({ status: 400, data: makeBackendEnvelope({ details: [] }) });
    expect(isValidationError(error)).toBe(false);
  });

  it('400 ama details hiç yoksa false döner', () => {
    const error = makeAxiosError({ status: 400, data: makeBackendEnvelope() });
    expect(isValidationError(error)).toBe(false);
  });

  it('status 400 değilse (details olsa bile) false döner', () => {
    const error = makeAxiosError({
      status: 422,
      data: makeBackendEnvelope({ details: ['Alan zorunlu'] }),
    });
    expect(isValidationError(error)).toBe(false);
  });
});
