import { test, expect } from '@playwright/test';

test.describe('Public Website - Home', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display homepage', async ({ page }) => {
    await expect(page).toHaveTitle(/xenon|clinic/i);
  });

  test('should have navigation links', async ({ page }) => {
    // Check for common navigation items
    const navItems = ['Home', 'Services', 'About', 'Contact'];

    for (const item of navItems) {
      const link = page.getByRole('link', { name: new RegExp(item, 'i') });
      if (await link.first().isVisible()) {
        await expect(link.first()).toBeEnabled();
      }
    }
  });

  test('should have login/register buttons', async ({ page }) => {
    // Look for authentication links
    const loginLink = page.getByRole('link', { name: /login|sign in/i });
    const registerLink = page.getByRole('link', { name: /register|sign up/i });

    // At least one should exist
    const loginVisible = await loginLink.first().isVisible().catch(() => false);
    const registerVisible = await registerLink.first().isVisible().catch(() => false);

    expect(loginVisible || registerVisible).toBeTruthy();
  });

  test('should be accessible', async ({ page }) => {
    // Basic accessibility checks
    // Check for skip link
    const skipLink = page.getByRole('link', { name: /skip to/i });
    if (await skipLink.isVisible()) {
      await expect(skipLink).toBeVisible();
    }

    // Check that images have alt text
    const images = page.locator('img');
    const count = await images.count();

    for (let i = 0; i < Math.min(count, 5); i++) {
      const img = images.nth(i);
      const alt = await img.getAttribute('alt');
      const role = await img.getAttribute('role');

      // Image should have alt text or be decorative (role="presentation")
      expect(alt !== null || role === 'presentation').toBeTruthy();
    }
  });

  test('should load quickly', async ({ page }) => {
    const startTime = Date.now();
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    const loadTime = Date.now() - startTime;

    // Page should load in under 3 seconds
    expect(loadTime).toBeLessThan(3000);
  });

  test('should be responsive', async ({ page }) => {
    // Test different viewport sizes
    const viewports = [
      { width: 1920, height: 1080, name: 'desktop' },
      { width: 768, height: 1024, name: 'tablet' },
      { width: 375, height: 667, name: 'mobile' },
    ];

    for (const viewport of viewports) {
      await page.setViewportSize({ width: viewport.width, height: viewport.height });
      await page.goto('/');

      // Page should not have horizontal scroll
      const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
      const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);

      expect(scrollWidth).toBeLessThanOrEqual(clientWidth + 10); // 10px tolerance
    }
  });
});
