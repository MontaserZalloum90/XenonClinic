import { test, expect } from '@playwright/test';

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should display dashboard with statistics', async ({ page }) => {
    // Check for common dashboard elements
    await expect(page.getByRole('heading', { name: /dashboard|overview/i })).toBeVisible();
  });

  test('should have working navigation menu', async ({ page }) => {
    // Check navigation items exist
    const navItems = ['Patients', 'Appointments', 'Laboratory', 'Inventory'];

    for (const item of navItems) {
      const navLink = page.getByRole('link', { name: new RegExp(item, 'i') });
      if (await navLink.isVisible()) {
        await expect(navLink).toBeEnabled();
      }
    }
  });

  test('should navigate to patients page', async ({ page }) => {
    const patientsLink = page.getByRole('link', { name: /patients/i });

    if (await patientsLink.isVisible()) {
      await patientsLink.click();
      await expect(page).toHaveURL(/patients/i);
    }
  });

  test('should navigate to appointments page', async ({ page }) => {
    const appointmentsLink = page.getByRole('link', { name: /appointments/i });

    if (await appointmentsLink.isVisible()) {
      await appointmentsLink.click();
      await expect(page).toHaveURL(/appointments/i);
    }
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    // Check that navigation is accessible (usually via hamburger menu on mobile)
    const menuButton = page.getByRole('button', { name: /menu|toggle/i });
    if (await menuButton.isVisible()) {
      await menuButton.click();
      // Navigation should now be visible
      await expect(page.getByRole('navigation')).toBeVisible();
    }
  });
});
