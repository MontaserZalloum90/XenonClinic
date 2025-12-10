import { test as base, expect } from '@playwright/test';

/**
 * Test credentials for different user roles.
 * Credentials are loaded from environment variables for security.
 * Falls back to test defaults only in non-CI environments.
 */
const getTestCredentials = () => {
  const isCI = !!process.env.CI;

  // In CI, require environment variables
  if (isCI) {
    const adminEmail = process.env.TEST_ADMIN_EMAIL;
    const adminPassword = process.env.TEST_ADMIN_PASSWORD;
    const doctorEmail = process.env.TEST_DOCTOR_EMAIL;
    const doctorPassword = process.env.TEST_DOCTOR_PASSWORD;
    const receptionistEmail = process.env.TEST_RECEPTIONIST_EMAIL;
    const receptionistPassword = process.env.TEST_RECEPTIONIST_PASSWORD;

    if (!adminEmail || !adminPassword) {
      throw new Error('TEST_ADMIN_EMAIL and TEST_ADMIN_PASSWORD environment variables are required in CI');
    }

    return {
      admin: {
        email: adminEmail,
        password: adminPassword,
      },
      doctor: {
        email: doctorEmail || adminEmail,
        password: doctorPassword || adminPassword,
      },
      receptionist: {
        email: receptionistEmail || adminEmail,
        password: receptionistPassword || adminPassword,
      },
    };
  }

  // Local development fallbacks
  return {
    admin: {
      email: process.env.TEST_ADMIN_EMAIL || 'admin@test.com',
      password: process.env.TEST_ADMIN_PASSWORD || 'TestPassword123!',
    },
    doctor: {
      email: process.env.TEST_DOCTOR_EMAIL || 'doctor@test.com',
      password: process.env.TEST_DOCTOR_PASSWORD || 'TestPassword123!',
    },
    receptionist: {
      email: process.env.TEST_RECEPTIONIST_EMAIL || 'receptionist@test.com',
      password: process.env.TEST_RECEPTIONIST_PASSWORD || 'TestPassword123!',
    },
  };
};

export const testUsers = getTestCredentials();

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
 * Note: API uses 'username' field which accepts email addresses.
 */
export async function getApiToken(request: any): Promise<string | null> {
  const response = await request.post('/api/AuthApi/login', {
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
 * Helper to get an API token for a specific user role.
 */
export async function getApiTokenForRole(
  request: any,
  role: 'admin' | 'doctor' | 'receptionist'
): Promise<string | null> {
  const user = testUsers[role];
  const response = await request.post('/api/AuthApi/login', {
    data: {
      username: user.email,
      password: user.password,
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
