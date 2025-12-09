import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import {
  createApiClient,
  tokenStorage,
  buildQueryString,
  isApiError,
  getErrorMessage,
  withRetry,
} from '../lib/api-base';

describe('tokenStorage', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('stores and retrieves token', () => {
    tokenStorage.setToken('test-token');
    expect(tokenStorage.getToken()).toBe('test-token');
  });

  it('removes token', () => {
    tokenStorage.setToken('test-token');
    tokenStorage.removeToken();
    expect(tokenStorage.getToken()).toBeNull();
  });

  it('checks authentication status', () => {
    expect(tokenStorage.isAuthenticated()).toBe(false);
    tokenStorage.setToken('test-token');
    expect(tokenStorage.isAuthenticated()).toBe(true);
  });
});

describe('buildQueryString', () => {
  it('builds query string from params', () => {
    const result = buildQueryString({ foo: 'bar', baz: 123 });
    expect(result).toBe('?foo=bar&baz=123');
  });

  it('excludes undefined values', () => {
    const result = buildQueryString({ foo: 'bar', baz: undefined });
    expect(result).toBe('?foo=bar');
  });

  it('excludes empty string values', () => {
    const result = buildQueryString({ foo: 'bar', baz: '' });
    expect(result).toBe('?foo=bar');
  });

  it('returns empty string when no valid params', () => {
    const result = buildQueryString({ foo: undefined, baz: '' });
    expect(result).toBe('');
  });

  it('handles boolean values', () => {
    const result = buildQueryString({ active: true, disabled: false });
    expect(result).toBe('?active=true&disabled=false');
  });
});

describe('isApiError', () => {
  it('returns true for valid API error', () => {
    const error = { status: 400, message: 'Bad Request' };
    expect(isApiError(error)).toBe(true);
  });

  it('returns false for regular Error', () => {
    const error = new Error('Something went wrong');
    expect(isApiError(error)).toBe(false);
  });

  it('returns false for null', () => {
    expect(isApiError(null)).toBe(false);
  });

  it('returns false for undefined', () => {
    expect(isApiError(undefined)).toBe(false);
  });
});

describe('getErrorMessage', () => {
  it('extracts message from API error', () => {
    const error = { status: 400, message: 'Bad Request' };
    expect(getErrorMessage(error)).toBe('Bad Request');
  });

  it('extracts message from Error', () => {
    const error = new Error('Something went wrong');
    expect(getErrorMessage(error)).toBe('Something went wrong');
  });

  it('returns default message for unknown error', () => {
    expect(getErrorMessage('string error')).toBe('An unexpected error occurred');
  });
});

describe('createApiClient', () => {
  const mockFetch = vi.fn();

  beforeEach(() => {
    global.fetch = mockFetch;
    mockFetch.mockReset();
    localStorage.clear();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('makes GET request', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      text: () => Promise.resolve(JSON.stringify({ data: 'test' })),
    });

    const client = createApiClient('https://api.example.com');
    const result = await client.get('/endpoint');

    expect(result.success).toBe(true);
    expect(result.data).toEqual({ data: 'test' });
    expect(mockFetch).toHaveBeenCalledWith(
      'https://api.example.com/endpoint',
      expect.objectContaining({ method: 'GET' })
    );
  });

  it('makes POST request with data', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      text: () => Promise.resolve(JSON.stringify({ id: 1 })),
    });

    const client = createApiClient('https://api.example.com');
    const result = await client.post('/endpoint', { name: 'test' });

    expect(result.success).toBe(true);
    expect(mockFetch).toHaveBeenCalledWith(
      'https://api.example.com/endpoint',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ name: 'test' }),
      })
    );
  });

  it('includes auth token when available', async () => {
    tokenStorage.setToken('my-token');
    mockFetch.mockResolvedValueOnce({
      ok: true,
      text: () => Promise.resolve('{}'),
    });

    const client = createApiClient('https://api.example.com');
    await client.get('/endpoint');

    expect(mockFetch).toHaveBeenCalledWith(
      'https://api.example.com/endpoint',
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer my-token',
        }),
      })
    );
  });

  it('handles error response', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 400,
      json: () => Promise.resolve({ message: 'Bad Request' }),
    });

    const client = createApiClient('https://api.example.com');
    const result = await client.get('/endpoint');

    expect(result.success).toBe(false);
    expect(result.error).toBe('Bad Request');
  });

  it('handles network error', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Network error'));

    const client = createApiClient('https://api.example.com');
    const result = await client.get('/endpoint');

    expect(result.success).toBe(false);
    expect(result.error).toBe('Network error');
  });
});

describe('withRetry', () => {
  it('succeeds on first attempt', async () => {
    const fn = vi.fn().mockResolvedValue('success');
    const result = await withRetry(fn, { maxRetries: 3, delay: 10 });

    expect(result).toBe('success');
    expect(fn).toHaveBeenCalledTimes(1);
  });

  it('retries on failure', async () => {
    const fn = vi
      .fn()
      .mockRejectedValueOnce(new Error('Fail 1'))
      .mockRejectedValueOnce(new Error('Fail 2'))
      .mockResolvedValue('success');

    const result = await withRetry(fn, { maxRetries: 3, delay: 10 });

    expect(result).toBe('success');
    expect(fn).toHaveBeenCalledTimes(3);
  });

  it('throws after max retries', async () => {
    const fn = vi.fn().mockRejectedValue(new Error('Always fails'));

    await expect(withRetry(fn, { maxRetries: 3, delay: 10 })).rejects.toThrow('Always fails');
    expect(fn).toHaveBeenCalledTimes(3);
  });
});
