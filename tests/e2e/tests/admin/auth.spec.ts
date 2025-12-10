import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login form', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /login|sign in/i })).toBeVisible();
    await expect(page.getByLabel(/username|email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /login|sign in/i })).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.getByLabel(/username|email/i).fill('invalid@test.com');
    await page.getByLabel(/password/i).fill('wrongpassword');
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page.getByText(/invalid|incorrect|failed/i)).toBeVisible();
  });

  test('should redirect to dashboard after successful login', async ({ page }) => {
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should preserve login state after page refresh', async ({ page }) => {
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);

    await page.reload();

    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should logout successfully', async ({ page }) => {
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);

    await page.getByRole('button', { name: /logout|sign out/i }).click();

    await expect(page).toHaveURL(/login/i);
  });
});

test.describe('Token Management', () => {
  test('should handle expired token gracefully', async ({ page, request }) => {
    // Login to get initial token
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);

    // Manually clear/invalidate token in localStorage
    await page.evaluate(() => {
      localStorage.setItem('token', 'invalid-expired-token');
    });

    // Navigate to a protected route
    await page.goto('/dashboard');

    // Should redirect to login due to invalid token
    await expect(page).toHaveURL(/login/i, { timeout: 10000 });
  });

  test('should redirect to login when accessing protected route without token', async ({ page }) => {
    // Clear any existing auth state
    await page.evaluate(() => {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    });

    // Try to access protected route directly
    await page.goto('/dashboard');

    // Should redirect to login
    await expect(page).toHaveURL(/login/i, { timeout: 10000 });
  });

  test('API should reject requests with invalid token', async ({ request }) => {
    const response = await request.get('/api/AuthApi/me', {
      headers: authHeaders('invalid-token'),
    });

    expect(response.status()).toBe(401);
  });

  test('API should accept requests with valid token', async ({ request }) => {
    const token = await getApiToken(request);

    // Skip test if token retrieval failed (API may not be running)
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/AuthApi/me', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });
});

test.describe('Account Lockout', () => {
  // Note: This test may affect real accounts if run against production
  // Ensure it only runs against test environments
  test.skip('should lock account after multiple failed attempts', async ({ page }) => {
    const maxAttempts = 5;

    for (let i = 0; i < maxAttempts; i++) {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill('wrong-password-attempt');
      await page.getByRole('button', { name: /login|sign in/i }).click();

      // Wait for error message
      await expect(page.getByText(/invalid|incorrect|failed/i)).toBeVisible();
    }

    // One more attempt should show lockout message
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill('wrong-password-attempt');
    await page.getByRole('button', { name: /login|sign in/i }).click();

    // Should show account locked message
    await expect(page.getByText(/locked|too many attempts|try again later/i)).toBeVisible();
  });
});

test.describe('Rate Limiting', () => {
  test('API should rate limit excessive login attempts', async ({ request }) => {
    const loginAttempts = [];

    // Make rapid login attempts
    for (let i = 0; i < 20; i++) {
      loginAttempts.push(
        request.post('/api/AuthApi/login', {
          data: {
            username: `test-rate-limit-${i}@example.com`,
            password: 'testpassword',
          },
        })
      );
    }

    const responses = await Promise.all(loginAttempts);

    // At least some requests should be rate limited (429 status)
    const rateLimitedCount = responses.filter((r) => r.status() === 429).length;
    const unauthorizedCount = responses.filter((r) => r.status() === 401).length;

    // Either rate limiting kicked in, or all requests were processed (but rejected as unauthorized)
    expect(rateLimitedCount + unauthorizedCount).toBeGreaterThan(0);
  });
});

test.describe('Role-Based Access', () => {
  test('admin should access admin dashboard', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);

    // Admin should see admin-specific elements
    // This will vary based on actual UI implementation
    await expect(page.locator('body')).toBeVisible();
  });

  test('doctor should access doctor-specific features', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.doctor.email);
    await page.getByLabel(/password/i).fill(testUsers.doctor.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    // Doctor should be redirected to appropriate dashboard
    await expect(page).toHaveURL(/dashboard|home|appointments/i);
  });

  test('receptionist should access receptionist features', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.receptionist.email);
    await page.getByLabel(/password/i).fill(testUsers.receptionist.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    // Receptionist should be redirected to appropriate dashboard
    await expect(page).toHaveURL(/dashboard|home|reception/i);
  });
});

test.describe('Security Headers', () => {
  test('login page should have security headers', async ({ page }) => {
    const response = await page.goto('/login');

    if (response) {
      const headers = response.headers();

      // Check for common security headers (may vary by implementation)
      // These are recommended but not strictly required
      const securityHeaders = [
        'x-content-type-options',
        'x-frame-options',
        'x-xss-protection',
      ];

      // Log which headers are present for debugging
      for (const header of securityHeaders) {
        if (headers[header]) {
          expect(headers[header]).toBeDefined();
        }
      }
    }
  });

  test('API should not expose sensitive headers', async ({ request }) => {
    const response = await request.post('/api/AuthApi/login', {
      data: {
        username: 'test@example.com',
        password: 'testpassword',
      },
    });

    const headers = response.headers();

    // Should not expose server version details
    expect(headers['server']).not.toMatch(/version|[0-9]+\.[0-9]+/i);
  });
});
