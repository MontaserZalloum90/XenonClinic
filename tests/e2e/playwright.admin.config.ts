import { defineConfig, devices } from "@playwright/test";

/**
 * Simplified Playwright config for running admin tests only.
 * Assumes the dev server is already running at localhost:5173
 */
export default defineConfig({
  testDir: "./tests/admin",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 4,
  reporter: [["list"]],
  timeout: 60000,

  use: {
    baseURL: "http://localhost:5173",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "off",
  },

  projects: [
    {
      name: "admin",
      use: {
        ...devices["Desktop Chrome"],
      },
    },
  ],
});
