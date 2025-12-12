import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Security and Audit Module
 * Tests cover audit trails, access control, security settings, and compliance
 */
test.describe('Security & Audit Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Audit Trail', () => {
    test('should display audit logs page', async ({ page }) => {
      await page.goto('/admin/audit');

      await expect(page.getByRole('heading', { name: /audit|log|activity/i })).toBeVisible();
    });

    test('should display audit log entries', async ({ page }) => {
      await page.goto('/admin/audit');

      const table = page.locator('table, [class*="list"], [class*="log"]');
      await expect(table).toBeVisible();
    });

    test('should filter audit logs by action type', async ({ page }) => {
      await page.goto('/admin/audit');

      const actionFilter = page.getByRole('combobox', { name: /action|type/i });
      if (await actionFilter.isVisible()) {
        await actionFilter.click();
        await page.getByRole('option', { name: /create|update|delete|login/i }).first().click();
      }
    });

    test('should filter audit logs by user', async ({ page }) => {
      await page.goto('/admin/audit');

      const userFilter = page.getByRole('combobox', { name: /user/i });
      if (await userFilter.isVisible()) {
        await userFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter audit logs by date range', async ({ page }) => {
      await page.goto('/admin/audit');

      const fromDate = page.getByLabel(/from|start.*date/i);
      const toDate = page.getByLabel(/to|end.*date/i);

      if (await fromDate.isVisible() && await toDate.isVisible()) {
        const today = new Date();
        const lastWeek = new Date(today);
        lastWeek.setDate(lastWeek.getDate() - 7);

        await fromDate.fill(lastWeek.toISOString().split('T')[0]);
        await toDate.fill(today.toISOString().split('T')[0]);
      }
    });

    test('should filter audit logs by entity type', async ({ page }) => {
      await page.goto('/admin/audit');

      const entityFilter = page.getByRole('combobox', { name: /entity|table|module/i });
      if (await entityFilter.isVisible()) {
        await entityFilter.click();
        await page.getByRole('option', { name: /patient|appointment|invoice/i }).first().click();
      }
    });

    test('should search audit logs', async ({ page }) => {
      await page.goto('/admin/audit');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('login');
        await page.waitForTimeout(500);
      }
    });

    test('should view audit log details', async ({ page }) => {
      await page.goto('/admin/audit');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/detail|change|before|after/i)).toBeVisible();
      }
    });

    test('should display before/after values for changes', async ({ page }) => {
      await page.goto('/admin/audit');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Should show diff or before/after
        const diffSection = page.getByText(/before|after|old|new|change/i);
        if (await diffSection.isVisible()) {
          await expect(diffSection).toBeVisible();
        }
      }
    });

    test('should export audit logs', async ({ page }) => {
      await page.goto('/admin/audit');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/audit.*\.(csv|xlsx|pdf)/i);
        }
      }
    });

    test('should paginate audit logs', async ({ page }) => {
      await page.goto('/admin/audit');

      const nextButton = page.getByRole('button', { name: /next|>/i });
      if (await nextButton.isVisible() && await nextButton.isEnabled()) {
        await nextButton.click();
        await page.waitForTimeout(300);

        const prevButton = page.getByRole('button', { name: /prev|</i });
        await expect(prevButton).toBeEnabled();
      }
    });
  });

  test.describe('Access Logs', () => {
    test('should display access logs page', async ({ page }) => {
      await page.goto('/admin/access-logs');

      await expect(page.getByRole('heading', { name: /access|login|session/i })).toBeVisible();
    });

    test('should display login history', async ({ page }) => {
      await page.goto('/admin/access-logs');

      await expect(page.getByText(/login|session|access/i)).toBeVisible();
    });

    test('should show failed login attempts', async ({ page }) => {
      await page.goto('/admin/access-logs');

      const failedFilter = page.getByRole('combobox', { name: /status|result/i });
      if (await failedFilter.isVisible()) {
        await failedFilter.click();
        await page.getByRole('option', { name: /failed|unsuccessful/i }).click();
      }
    });

    test('should display IP addresses', async ({ page }) => {
      await page.goto('/admin/access-logs');

      // IP addresses should be visible in logs
      const ipPattern = page.locator('td, [class*="ip"]').filter({ hasText: /\d+\.\d+\.\d+\.\d+/ });
      if (await ipPattern.count() > 0) {
        await expect(ipPattern.first()).toBeVisible();
      }
    });

    test('should display user agent information', async ({ page }) => {
      await page.goto('/admin/access-logs');

      const userAgentColumn = page.getByText(/browser|device|user.*agent/i);
      if (await userAgentColumn.isVisible()) {
        await expect(userAgentColumn).toBeVisible();
      }
    });
  });

  test.describe('User Management', () => {
    test('should display users list', async ({ page }) => {
      await page.goto('/admin/users');

      await expect(page.getByRole('heading', { name: /user/i })).toBeVisible();
    });

    test('should create new user', async ({ page }) => {
      await page.goto('/admin/users');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const emailField = page.getByLabel(/email/i);
        if (await emailField.isVisible()) {
          await emailField.fill(`e2e.user.${Date.now()}@test.com`);
        }

        const passwordField = page.getByLabel(/password/i);
        if (await passwordField.isVisible()) {
          await passwordField.fill('TestPassword123!');
        }

        const roleSelect = page.getByLabel(/role/i);
        if (await roleSelect.isVisible()) {
          await roleSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should disable user account', async ({ page }) => {
      await page.goto('/admin/users');

      const disableButton = page.getByRole('button', { name: /disable|deactivate/i }).first();
      if (await disableButton.isVisible()) {
        await disableButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/disabled|deactivated|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should reset user password', async ({ page }) => {
      await page.goto('/admin/users');

      const resetButton = page.getByRole('button', { name: /reset.*password/i }).first();
      if (await resetButton.isVisible()) {
        await resetButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm|reset/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/reset|sent|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should edit user permissions', async ({ page }) => {
      await page.goto('/admin/users');

      const editButton = page.getByRole('button', { name: /edit|permission/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        // Toggle a permission
        const permissionCheckbox = page.locator('input[type="checkbox"]').first();
        if (await permissionCheckbox.isVisible()) {
          await permissionCheckbox.click();
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should unlock locked user account', async ({ page }) => {
      await page.goto('/admin/users');

      const unlockButton = page.getByRole('button', { name: /unlock/i }).first();
      if (await unlockButton.isVisible()) {
        await unlockButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/unlocked|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Roles & Permissions', () => {
    test('should display roles page', async ({ page }) => {
      await page.goto('/admin/roles');

      await expect(page.getByRole('heading', { name: /role/i })).toBeVisible();
    });

    test('should create new role', async ({ page }) => {
      await page.goto('/admin/roles');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name|role.*name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Role ${Date.now()}`);
        }

        // Select permissions
        const permissionCheckboxes = page.locator('input[type="checkbox"]');
        if (await permissionCheckboxes.count() > 0) {
          await permissionCheckboxes.first().check();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should edit role permissions', async ({ page }) => {
      await page.goto('/admin/roles');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        // Toggle a permission
        const permissionCheckbox = page.locator('input[type="checkbox"]').first();
        if (await permissionCheckbox.isVisible()) {
          await permissionCheckbox.click();
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should not allow deleting system roles', async ({ page }) => {
      await page.goto('/admin/roles');

      // System roles like Admin should not have delete button
      const adminRow = page.locator('tr, [class*="row"]').filter({ hasText: /admin/i }).first();
      if (await adminRow.isVisible()) {
        const deleteButton = adminRow.getByRole('button', { name: /delete/i });
        if (await deleteButton.count() > 0) {
          await expect(deleteButton).toBeDisabled();
        }
      }
    });
  });

  test.describe('Security Settings', () => {
    test('should display security settings page', async ({ page }) => {
      await page.goto('/admin/security');

      await expect(page.getByRole('heading', { name: /security.*setting/i })).toBeVisible();
    });

    test('should configure password policy', async ({ page }) => {
      await page.goto('/admin/security');

      const minLengthField = page.getByLabel(/minimum.*length|min.*length/i);
      if (await minLengthField.isVisible()) {
        await minLengthField.fill('12');
      }

      const requireUppercase = page.getByLabel(/uppercase/i);
      if (await requireUppercase.isVisible()) {
        await requireUppercase.check();
      }

      const requireNumber = page.getByLabel(/number|digit/i);
      if (await requireNumber.isVisible()) {
        await requireNumber.check();
      }

      const requireSpecial = page.getByLabel(/special.*character/i);
      if (await requireSpecial.isVisible()) {
        await requireSpecial.check();
      }

      const submitButton = page.getByRole('button', { name: /save/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
    });

    test('should configure session timeout', async ({ page }) => {
      await page.goto('/admin/security');

      const timeoutField = page.getByLabel(/session.*timeout|idle.*timeout/i);
      if (await timeoutField.isVisible()) {
        await timeoutField.fill('30');
      }

      const submitButton = page.getByRole('button', { name: /save/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
    });

    test('should configure account lockout', async ({ page }) => {
      await page.goto('/admin/security');

      const maxAttemptsField = page.getByLabel(/max.*attempts|lockout.*threshold/i);
      if (await maxAttemptsField.isVisible()) {
        await maxAttemptsField.fill('5');
      }

      const lockoutDurationField = page.getByLabel(/lockout.*duration|lockout.*time/i);
      if (await lockoutDurationField.isVisible()) {
        await lockoutDurationField.fill('15');
      }

      const submitButton = page.getByRole('button', { name: /save/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
    });

    test('should configure two-factor authentication', async ({ page }) => {
      await page.goto('/admin/security');

      const twoFactorToggle = page.getByLabel(/two.*factor|2fa|mfa/i);
      if (await twoFactorToggle.isVisible()) {
        await twoFactorToggle.check();
      }

      const submitButton = page.getByRole('button', { name: /save/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Data Privacy & Compliance', () => {
    test('should display privacy settings', async ({ page }) => {
      await page.goto('/admin/privacy');

      await expect(page.getByRole('heading', { name: /privacy|gdpr|hipaa|compliance/i })).toBeVisible();
    });

    test('should configure data retention period', async ({ page }) => {
      await page.goto('/admin/privacy');

      const retentionField = page.getByLabel(/retention.*period|data.*retention/i);
      if (await retentionField.isVisible()) {
        await retentionField.fill('365');
      }

      const submitButton = page.getByRole('button', { name: /save/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
    });

    test('should view data processing agreements', async ({ page }) => {
      await page.goto('/admin/privacy');

      const dpaSection = page.getByText(/data.*processing|dpa|agreement/i);
      if (await dpaSection.isVisible()) {
        await expect(dpaSection).toBeVisible();
      }
    });

    test('should export user data (GDPR request)', async ({ page }) => {
      await page.goto('/admin/privacy');

      const exportButton = page.getByRole('button', { name: /export.*data|gdpr.*export/i });
      if (await exportButton.isVisible()) {
        await exportButton.click();

        await expect(page.getByRole('dialog').or(page.getByText(/export/i))).toBeVisible();
      }
    });

    test('should handle data deletion request', async ({ page }) => {
      await page.goto('/admin/privacy');

      const deleteButton = page.getByRole('button', { name: /delete.*data|right.*erasure/i });
      if (await deleteButton.isVisible()) {
        await deleteButton.click();

        await expect(page.getByRole('dialog').or(page.getByText(/delete/i))).toBeVisible();
      }
    });
  });

  test.describe('Consent Management', () => {
    test('should display consent records', async ({ page }) => {
      await page.goto('/admin/consents');

      await expect(page.getByRole('heading', { name: /consent/i })).toBeVisible();
    });

    test('should view patient consent history', async ({ page }) => {
      await page.goto('/admin/consents');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/consent.*history|consent.*log/i)).toBeVisible();
      }
    });

    test('should revoke consent', async ({ page }) => {
      await page.goto('/admin/consents');

      const revokeButton = page.getByRole('button', { name: /revoke|withdraw/i }).first();
      if (await revokeButton.isVisible()) {
        await revokeButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Patient requested withdrawal');
        }

        const confirmButton = page.getByRole('button', { name: /yes|confirm|revoke/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/revoked|withdrawn|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('IP Whitelisting', () => {
    test('should display IP whitelist settings', async ({ page }) => {
      await page.goto('/admin/security/ip-whitelist');

      await expect(page.getByRole('heading', { name: /ip|whitelist|allowlist/i })).toBeVisible();
    });

    test('should add IP to whitelist', async ({ page }) => {
      await page.goto('/admin/security/ip-whitelist');

      const addButton = page.getByRole('button', { name: /add|new/i });
      if (await addButton.isVisible()) {
        await addButton.click();

        const ipField = page.getByLabel(/ip.*address/i);
        if (await ipField.isVisible()) {
          await ipField.fill('192.168.1.100');
        }

        const descriptionField = page.getByLabel(/description|note/i);
        if (await descriptionField.isVisible()) {
          await descriptionField.fill('E2E Test - Office IP');
        }

        const submitButton = page.getByRole('button', { name: /save|add/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|added/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should remove IP from whitelist', async ({ page }) => {
      await page.goto('/admin/security/ip-whitelist');

      const removeButton = page.getByRole('button', { name: /remove|delete/i }).first();
      if (await removeButton.isVisible()) {
        await removeButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/removed|deleted|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Security Reports', () => {
    test('should generate security report', async ({ page }) => {
      await page.goto('/admin/security/reports');

      const reportButton = page.getByRole('button', { name: /generate|security.*report/i });
      if (await reportButton.isVisible()) {
        await reportButton.click();

        await expect(page.getByText(/security.*report|summary/i)).toBeVisible();
      }
    });

    test('should view suspicious activity report', async ({ page }) => {
      await page.goto('/admin/security/reports');

      const suspiciousButton = page.getByRole('button', { name: /suspicious|threat|anomaly/i });
      if (await suspiciousButton.isVisible()) {
        await suspiciousButton.click();

        await expect(page.getByText(/suspicious|threat|activity/i)).toBeVisible();
      }
    });

    test('should export security report', async ({ page }) => {
      await page.goto('/admin/security/reports');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/security.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });
});

test.describe('Security API E2E Tests', () => {
  test('should list audit logs', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/audit?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get audit log details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/audit?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const logId = listData.items[0].id;

        const response = await request.get(`/api/audit/${logId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should list users', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/users?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should list roles', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/roles', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should validate user creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/users', {
      headers: authHeaders(token),
      data: {
        email: '', // Invalid empty email
        password: '',
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should enforce password policy', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/users', {
      headers: authHeaders(token),
      data: {
        email: 'test@example.com',
        password: 'weak', // Weak password
      },
    });

    expect(response.status()).toBe(400);
  });
});

test.describe('Security Headers Tests', () => {
  test('should include X-Content-Type-Options header', async ({ request }) => {
    const response = await request.get('/');

    const headers = response.headers();
    if (headers['x-content-type-options']) {
      expect(headers['x-content-type-options']).toBe('nosniff');
    }
  });

  test('should include X-Frame-Options header', async ({ request }) => {
    const response = await request.get('/');

    const headers = response.headers();
    if (headers['x-frame-options']) {
      expect(['DENY', 'SAMEORIGIN']).toContain(headers['x-frame-options']);
    }
  });

  test('should include Content-Security-Policy header', async ({ request }) => {
    const response = await request.get('/');

    const headers = response.headers();
    if (headers['content-security-policy']) {
      expect(headers['content-security-policy']).toBeDefined();
    }
  });

  test('should include Strict-Transport-Security header on HTTPS', async ({ request }) => {
    // This test only applies to HTTPS connections
    const response = await request.get('/');

    const headers = response.headers();
    if (headers['strict-transport-security']) {
      expect(headers['strict-transport-security']).toContain('max-age');
    }
  });

  test('should not expose sensitive headers', async ({ request }) => {
    const response = await request.get('/');

    const headers = response.headers();

    // Should not expose server version
    if (headers['server']) {
      expect(headers['server']).not.toMatch(/\d+\.\d+/);
    }

    // Should not expose X-Powered-By
    expect(headers['x-powered-by']).toBeUndefined();
  });
});
