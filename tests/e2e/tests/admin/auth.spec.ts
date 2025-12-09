import { test, expect } from '@playwright/test';

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
    // Use test credentials (should be configured in test environment)
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();

    // Should redirect to dashboard
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should preserve login state after page refresh', async ({ page }) => {
    // Login first
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);

    // Refresh page
    await page.reload();

    // Should still be on dashboard (not redirected to login)
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should logout successfully', async ({ page }) => {
    // Login first
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();

    await expect(page).toHaveURL(/dashboard|home/i);

    // Find and click logout
    await page.getByRole('button', { name: /logout|sign out/i }).click();

    // Should redirect to login
    await expect(page).toHaveURL(/login/i);
  });
});
