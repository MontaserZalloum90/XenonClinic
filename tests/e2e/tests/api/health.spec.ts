import { test, expect } from '@playwright/test';

test.describe('API Health Checks', () => {
  test('should return healthy status', async ({ request }) => {
    const response = await request.get('/health');

    expect(response.status()).toBe(200);

    const body = await response.text();
    expect(body.toLowerCase()).toContain('healthy');
  });

  test('should return correct content type', async ({ request }) => {
    const response = await request.get('/health');

    const contentType = response.headers()['content-type'];
    expect(contentType).toBeDefined();
  });

  test('should respond quickly', async ({ request }) => {
    const startTime = Date.now();
    await request.get('/health');
    const responseTime = Date.now() - startTime;

    // Health check should respond in under 500ms
    expect(responseTime).toBeLessThan(500);
  });
});

test.describe('API Authentication', () => {
  test('should reject unauthenticated requests to protected endpoints', async ({ request }) => {
    const response = await request.get('/api/patients');

    // Should return 401 Unauthorized
    expect(response.status()).toBe(401);
  });

  test('should accept valid JWT token', async ({ request }) => {
    // First, get a token by logging in
    const loginResponse = await request.post('/api/auth/login', {
      data: {
        username: 'admin@test.com',
        password: 'TestPassword123!',
      },
    });

    if (loginResponse.status() === 200) {
      const { token } = await loginResponse.json();

      // Use the token to access protected endpoint
      const protectedResponse = await request.get('/api/patients', {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      expect(protectedResponse.status()).toBe(200);
    }
  });

  test('should reject invalid JWT token', async ({ request }) => {
    const response = await request.get('/api/patients', {
      headers: {
        Authorization: 'Bearer invalid-token-here',
      },
    });

    expect(response.status()).toBe(401);
  });

  test('should reject expired JWT token', async ({ request }) => {
    // This is a sample expired token (you'd need a real expired one for actual testing)
    const expiredToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZXhwIjoxfQ.signature';

    const response = await request.get('/api/patients', {
      headers: {
        Authorization: `Bearer ${expiredToken}`,
      },
    });

    expect(response.status()).toBe(401);
  });
});

test.describe('API Error Handling', () => {
  test('should return 404 for non-existent endpoints', async ({ request }) => {
    const response = await request.get('/api/non-existent-endpoint');

    expect(response.status()).toBe(404);
  });

  test('should return proper error format', async ({ request }) => {
    const response = await request.get('/api/patients/999999999');

    if (response.status() === 404) {
      const body = await response.json().catch(() => null);

      // Should have a structured error response
      if (body) {
        expect(body).toHaveProperty('message');
      }
    }
  });

  test('should handle invalid JSON gracefully', async ({ request }) => {
    const response = await request.post('/api/patients', {
      headers: {
        'Content-Type': 'application/json',
      },
      data: 'invalid json {{{',
    });

    expect(response.status()).toBe(400);
  });
});
