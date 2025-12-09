import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for XenonClinic E2E tests.
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { open: 'never' }],
    ['list'],
    ...(process.env.CI ? [['github' as const]] : []),
  ],

  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
  },

  projects: [
    // Admin App Tests
    {
      name: 'admin',
      testDir: './tests/admin',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: process.env.ADMIN_URL || 'http://localhost:3000',
      },
    },
    {
      name: 'admin-mobile',
      testDir: './tests/admin',
      use: {
        ...devices['iPhone 13'],
        baseURL: process.env.ADMIN_URL || 'http://localhost:3000',
      },
    },

    // Public Website Tests
    {
      name: 'public',
      testDir: './tests/public',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: process.env.PUBLIC_URL || 'http://localhost:3001',
      },
    },
    {
      name: 'public-mobile',
      testDir: './tests/public',
      use: {
        ...devices['iPhone 13'],
        baseURL: process.env.PUBLIC_URL || 'http://localhost:3001',
      },
    },

    // API Tests
    {
      name: 'api',
      testDir: './tests/api',
      use: {
        baseURL: process.env.API_URL || 'http://localhost:5000',
      },
    },
  ],

  // Web server configuration for local development
  webServer: [
    {
      command: 'npm run dev',
      cwd: '../../XenonClinic.React',
      url: 'http://localhost:3000',
      reuseExistingServer: !process.env.CI,
      timeout: 120 * 1000,
    },
    {
      command: 'npm run dev',
      cwd: '../../Xenon.PublicWebsite',
      url: 'http://localhost:3001',
      reuseExistingServer: !process.env.CI,
      timeout: 120 * 1000,
    },
  ],
});
