import { test as base, expect } from '@playwright/test';

/**
 * Test credentials for different user roles.
 */
export const testUsers = {
  admin: {
    email: 'admin@test.com',
    password: 'TestPassword123!',
  },
  doctor: {
    email: 'doctor@test.com',
    password: 'TestPassword123!',
  },
  receptionist: {
    email: 'receptionist@test.com',
    password: 'TestPassword123!',
  },
};

/**
 * Extended test fixture with authentication helpers.
 */
export const test = base.extend<{
  authenticatedPage: ReturnType<typeof base['page']>;
}>({
  authenticatedPage: async ({ page }, use) => {
    // Login before the test
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();

    // Wait for redirect to dashboard
    await expect(page).toHaveURL(/dashboard|home/i, { timeout: 10000 });

    // Provide the authenticated page to the test
    await use(page);
  },
});

export { expect };

/**
 * Helper to get an API token for authenticated API requests.
 */
export async function getApiToken(request: any): Promise<string | null> {
  const response = await request.post('/api/auth/login', {
    data: {
      username: testUsers.admin.email,
      password: testUsers.admin.password,
    },
  });

  if (response.status() === 200) {
    const { token } = await response.json();
    return token;
  }

  return null;
}

/**
 * Helper to create authenticated request headers.
 */
export function authHeaders(token: string) {
  return {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  };
}
