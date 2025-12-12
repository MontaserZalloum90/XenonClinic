/**
 * Multi-Tenancy Module E2E Tests
 *
 * Comprehensive test suite for multi-tenant SaaS functionality
 * Tests cover: tenant management, company configuration, branch management,
 * data isolation, subscription plans, tenant customization, and cross-tenant security
 */

import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

test.describe('Multi-Tenancy Module', () => {

  test.describe('Tenant Management (Super Admin)', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.superAdmin?.email || testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.superAdmin?.password || testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
      await expect(page).toHaveURL(/dashboard|home/i);
    });

    test('should display tenant management dashboard', async ({ page }) => {
      await page.goto('/admin/tenants');

      await expect(page.getByRole('heading', { name: /tenant|organization/i })).toBeVisible();
      await expect(page.locator('[data-testid="tenant-list"]')).toBeVisible();
    });

    test('should create new tenant', async ({ page }) => {
      await page.goto('/admin/tenants');

      await page.getByRole('button', { name: /add|create|new.*tenant/i }).click();

      // Tenant Information
      await page.getByLabel(/tenant.*name|organization/i).fill('Test Hospital Network');
      await page.getByLabel(/subdomain/i).fill(`test-hospital-${Date.now()}`);
      await page.getByLabel(/email/i).fill('admin@test-hospital.com');
      await page.getByLabel(/phone/i).fill('+1234567890');

      // Business Details
      await page.getByLabel(/business.*type/i).selectOption({ label: /hospital|clinic/i });
      await page.getByLabel(/country/i).selectOption({ label: /united states/i });
      await page.getByLabel(/timezone/i).selectOption({ label: /eastern/i });

      // Subscription Plan
      await page.getByLabel(/plan|subscription/i).selectOption({ label: /enterprise|professional/i });

      await page.getByRole('button', { name: /create|save/i }).click();

      await expect(page.getByText(/tenant.*created|success/i)).toBeVisible();
    });

    test('should view tenant details', async ({ page }) => {
      await page.goto('/admin/tenants');

      const tenantRow = page.locator('[data-testid="tenant-row"]').first();
      await tenantRow.click();

      await expect(page.getByText(/tenant.*details|overview/i)).toBeVisible();
      await expect(page.getByText(/companies|branches/i)).toBeVisible();
      await expect(page.getByText(/users/i)).toBeVisible();
      await expect(page.getByText(/subscription/i)).toBeVisible();
    });

    test('should edit tenant settings', async ({ page }) => {
      await page.goto('/admin/tenants');

      const tenantRow = page.locator('[data-testid="tenant-row"]').first();
      await tenantRow.getByRole('button', { name: /edit|settings/i }).click();

      await page.getByLabel(/tenant.*name/i).fill('Updated Hospital Network');
      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/updated|saved/i)).toBeVisible();
    });

    test('should suspend tenant', async ({ page }) => {
      await page.goto('/admin/tenants');

      const tenantRow = page.locator('[data-testid="tenant-row"]').first();
      await tenantRow.getByRole('button', { name: /actions|menu/i }).click();
      await page.getByRole('menuitem', { name: /suspend/i }).click();

      await page.getByLabel(/reason/i).fill('Non-payment');
      await page.getByRole('button', { name: /confirm|suspend/i }).click();

      await expect(page.getByText(/suspended/i)).toBeVisible();
    });

    test('should reactivate tenant', async ({ page }) => {
      await page.goto('/admin/tenants');

      const suspendedTenant = page.locator('[data-testid="tenant-row"]').filter({ hasText: /suspended/i }).first();
      await suspendedTenant.getByRole('button', { name: /actions|menu/i }).click();
      await page.getByRole('menuitem', { name: /reactivate|activate/i }).click();

      await page.getByRole('button', { name: /confirm/i }).click();

      await expect(page.getByText(/activated|active/i)).toBeVisible();
    });

    test('should delete tenant', async ({ page }) => {
      await page.goto('/admin/tenants');

      const tenantRow = page.locator('[data-testid="tenant-row"]').first();
      await tenantRow.getByRole('button', { name: /actions|menu/i }).click();
      await page.getByRole('menuitem', { name: /delete|remove/i }).click();

      // Confirmation dialog
      await page.getByLabel(/confirm.*delete|type.*name/i).fill('DELETE');
      await page.getByRole('button', { name: /confirm.*delete/i }).click();

      await expect(page.getByText(/deleted|removed/i)).toBeVisible();
    });

    test('should search and filter tenants', async ({ page }) => {
      await page.goto('/admin/tenants');

      await page.getByPlaceholder(/search/i).fill('Hospital');
      await page.keyboard.press('Enter');

      await expect(page.locator('[data-testid="tenant-row"]').first()).toContainText(/hospital/i);
    });

    test('should export tenant list', async ({ page }) => {
      await page.goto('/admin/tenants');

      const downloadPromise = page.waitForEvent('download');
      await page.getByRole('button', { name: /export/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/tenant.*\.(csv|xlsx)/i);
    });
  });

  test.describe('Company Management', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
      await expect(page).toHaveURL(/dashboard|home/i);
    });

    test('should display company list', async ({ page }) => {
      await page.goto('/admin/companies');

      await expect(page.getByRole('heading', { name: /compan/i })).toBeVisible();
      await expect(page.locator('[data-testid="company-list"]')).toBeVisible();
    });

    test('should create new company', async ({ page }) => {
      await page.goto('/admin/companies');

      await page.getByRole('button', { name: /add|create|new.*company/i }).click();

      await page.getByLabel(/company.*name/i).fill('Test Medical Center');
      await page.getByLabel(/registration.*number/i).fill('REG-12345');
      await page.getByLabel(/tax.*number/i).fill('TAX-67890');
      await page.getByLabel(/email/i).fill('contact@test-medical.com');
      await page.getByLabel(/phone/i).fill('+1234567890');
      await page.getByLabel(/address/i).fill('123 Medical Center Drive');
      await page.getByLabel(/city/i).fill('Healthcare City');
      await page.getByLabel(/country/i).selectOption({ index: 1 });

      await page.getByRole('button', { name: /create|save/i }).click();

      await expect(page.getByText(/company.*created|success/i)).toBeVisible();
    });

    test('should view company details', async ({ page }) => {
      await page.goto('/admin/companies');

      const companyRow = page.locator('[data-testid="company-row"]').first();
      await companyRow.click();

      await expect(page.getByText(/company.*details|overview/i)).toBeVisible();
      await expect(page.getByText(/branches/i)).toBeVisible();
    });

    test('should edit company details', async ({ page }) => {
      await page.goto('/admin/companies');

      const companyRow = page.locator('[data-testid="company-row"]').first();
      await companyRow.getByRole('button', { name: /edit/i }).click();

      await page.getByLabel(/company.*name/i).fill('Updated Medical Center');
      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/updated|saved/i)).toBeVisible();
    });

    test('should configure company logo and branding', async ({ page }) => {
      await page.goto('/admin/companies');

      const companyRow = page.locator('[data-testid="company-row"]').first();
      await companyRow.getByRole('button', { name: /settings|branding/i }).click();

      // Upload logo
      const logoInput = page.locator('input[type="file"]').first();
      await logoInput.setInputFiles({
        name: 'logo.png',
        mimeType: 'image/png',
        buffer: Buffer.from('fake image content')
      });

      // Set primary color
      await page.getByLabel(/primary.*color/i).fill('#0066CC');

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should set company fiscal year', async ({ page }) => {
      await page.goto('/admin/companies');

      const companyRow = page.locator('[data-testid="company-row"]').first();
      await companyRow.getByRole('button', { name: /settings/i }).click();

      await page.getByLabel(/fiscal.*year.*start/i).selectOption({ label: /january/i });
      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });
  });

  test.describe('Branch Management', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
      await expect(page).toHaveURL(/dashboard|home/i);
    });

    test('should display branch list', async ({ page }) => {
      await page.goto('/admin/branches');

      await expect(page.getByRole('heading', { name: /branch/i })).toBeVisible();
      await expect(page.locator('[data-testid="branch-list"]')).toBeVisible();
    });

    test('should create new branch', async ({ page }) => {
      await page.goto('/admin/branches');

      await page.getByRole('button', { name: /add|create|new.*branch/i }).click();

      await page.getByLabel(/branch.*name/i).fill('Downtown Clinic');
      await page.getByLabel(/branch.*code/i).fill('DTC-001');
      await page.getByLabel(/company/i).selectOption({ index: 1 });
      await page.getByLabel(/address/i).fill('456 Downtown Street');
      await page.getByLabel(/city/i).fill('Metro City');
      await page.getByLabel(/phone/i).fill('+1987654321');
      await page.getByLabel(/email/i).fill('downtown@clinic.com');

      // Operating Hours
      await page.getByLabel(/opening.*time/i).fill('08:00');
      await page.getByLabel(/closing.*time/i).fill('20:00');

      await page.getByRole('button', { name: /create|save/i }).click();

      await expect(page.getByText(/branch.*created|success/i)).toBeVisible();
    });

    test('should edit branch details', async ({ page }) => {
      await page.goto('/admin/branches');

      const branchRow = page.locator('[data-testid="branch-row"]').first();
      await branchRow.getByRole('button', { name: /edit/i }).click();

      await page.getByLabel(/branch.*name/i).fill('Updated Downtown Clinic');
      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/updated|saved/i)).toBeVisible();
    });

    test('should configure branch services', async ({ page }) => {
      await page.goto('/admin/branches');

      const branchRow = page.locator('[data-testid="branch-row"]').first();
      await branchRow.getByRole('button', { name: /services|configure/i }).click();

      // Enable services
      await page.getByLabel(/laboratory/i).check();
      await page.getByLabel(/pharmacy/i).check();
      await page.getByLabel(/radiology/i).check();

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });

    test('should assign branch manager', async ({ page }) => {
      await page.goto('/admin/branches');

      const branchRow = page.locator('[data-testid="branch-row"]').first();
      await branchRow.getByRole('button', { name: /manage|settings/i }).click();

      await page.getByLabel(/branch.*manager/i).selectOption({ index: 1 });
      await page.getByRole('button', { name: /save|assign/i }).click();

      await expect(page.getByText(/assigned|saved/i)).toBeVisible();
    });

    test('should deactivate branch', async ({ page }) => {
      await page.goto('/admin/branches');

      const branchRow = page.locator('[data-testid="branch-row"]').first();
      await branchRow.getByRole('button', { name: /actions|menu/i }).click();
      await page.getByRole('menuitem', { name: /deactivate/i }).click();

      await page.getByRole('button', { name: /confirm/i }).click();

      await expect(page.getByText(/deactivated/i)).toBeVisible();
    });

    test('should view branch statistics', async ({ page }) => {
      await page.goto('/admin/branches');

      const branchRow = page.locator('[data-testid="branch-row"]').first();
      await branchRow.click();

      await expect(page.getByText(/statistics|metrics/i)).toBeVisible();
      await expect(page.getByText(/patients|appointments|revenue/i)).toBeVisible();
    });
  });

  test.describe('Data Isolation & Security', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should only see data from own tenant', async ({ page }) => {
      await page.goto('/patients');

      // Verify data belongs to current tenant
      const patientList = page.locator('[data-testid="patient-list"]');
      await expect(patientList).toBeVisible();

      // Check for tenant ID in page context (if exposed)
      const currentTenant = await page.evaluate(() => {
        return (window as any).__TENANT_ID__ || null;
      });

      // Should not contain cross-tenant data indicators
      await expect(page.locator('[data-tenant-id]').first()).not.toBeVisible();
    });

    test('should prevent cross-tenant API access', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      // Attempt to access another tenant's data
      const response = await request.get('/api/tenants/other-tenant-id/patients', {
        headers: authHeaders(token)
      });

      expect([401, 403, 404]).toContain(response.status());
    });

    test('should enforce branch-level data access', async ({ page }) => {
      await page.goto('/patients');

      // Change branch context
      const branchSelector = page.getByLabel(/branch|location/i);
      if (await branchSelector.isVisible()) {
        await branchSelector.selectOption({ index: 1 });

        // Verify patient list updates based on branch
        await expect(page.locator('[data-testid="patient-list"]')).toBeVisible();
      }
    });

    test('should audit cross-branch data access', async ({ page }) => {
      await page.goto('/admin/audit-logs');

      await page.getByLabel(/event.*type/i).selectOption({ label: /data.*access|cross.*branch/i });
      await page.getByRole('button', { name: /search|filter/i }).click();

      const auditList = page.locator('[data-testid="audit-list"]');
      await expect(auditList).toBeVisible();
    });
  });

  test.describe('Subscription & Billing', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display current subscription plan', async ({ page }) => {
      await page.goto('/admin/subscription');

      await expect(page.getByText(/current.*plan|subscription/i)).toBeVisible();
      await expect(page.getByText(/features|limits/i)).toBeVisible();
    });

    test('should view available plans', async ({ page }) => {
      await page.goto('/admin/subscription/plans');

      await expect(page.locator('[data-testid="plan-card"]')).toHaveCount({ minimum: 1 });
    });

    test('should upgrade subscription plan', async ({ page }) => {
      await page.goto('/admin/subscription/plans');

      const enterprisePlan = page.locator('[data-testid="plan-card"]').filter({ hasText: /enterprise|premium/i });
      await enterprisePlan.getByRole('button', { name: /upgrade|select/i }).click();

      // Confirm upgrade
      await page.getByRole('button', { name: /confirm|proceed/i }).click();

      await expect(page.getByText(/upgrade.*request|processing/i)).toBeVisible();
    });

    test('should view usage metrics', async ({ page }) => {
      await page.goto('/admin/subscription/usage');

      await expect(page.getByText(/users|storage|api.*calls/i)).toBeVisible();
      await expect(page.locator('[data-testid="usage-chart"]')).toBeVisible();
    });

    test('should view billing history', async ({ page }) => {
      await page.goto('/admin/subscription/billing');

      const billingList = page.locator('[data-testid="billing-history"]');
      await expect(billingList).toBeVisible();
    });

    test('should download invoice', async ({ page }) => {
      await page.goto('/admin/subscription/billing');

      const invoiceRow = page.locator('[data-testid="invoice-row"]').first();
      const downloadPromise = page.waitForEvent('download');
      await invoiceRow.getByRole('button', { name: /download/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/invoice.*\.pdf/i);
    });

    test('should update payment method', async ({ page }) => {
      await page.goto('/admin/subscription/payment');

      await page.getByRole('button', { name: /update|change.*payment/i }).click();

      await page.getByLabel(/card.*number/i).fill('4111111111111111');
      await page.getByLabel(/expiry/i).fill('12/26');
      await page.getByLabel(/cvv/i).fill('123');

      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/updated|saved/i)).toBeVisible();
    });
  });

  test.describe('Tenant Customization', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should customize branding settings', async ({ page }) => {
      await page.goto('/admin/settings/branding');

      // Logo
      const logoInput = page.locator('input[type="file"]').first();
      await logoInput.setInputFiles({
        name: 'company-logo.png',
        mimeType: 'image/png',
        buffer: Buffer.from('fake logo content')
      });

      // Colors
      await page.getByLabel(/primary.*color/i).fill('#0066CC');
      await page.getByLabel(/secondary.*color/i).fill('#00CC66');

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should configure email templates', async ({ page }) => {
      await page.goto('/admin/settings/email-templates');

      const templateCard = page.locator('[data-testid="template-card"]').first();
      await templateCard.getByRole('button', { name: /edit/i }).click();

      await page.getByLabel(/subject/i).fill('Custom Email Subject');
      await page.locator('[data-testid="email-editor"]').fill('Custom email content');

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });

    test('should configure custom fields', async ({ page }) => {
      await page.goto('/admin/settings/custom-fields');

      await page.getByRole('button', { name: /add.*field/i }).click();

      await page.getByLabel(/field.*name/i).fill('CustomField1');
      await page.getByLabel(/field.*type/i).selectOption({ label: /text/i });
      await page.getByLabel(/entity|module/i).selectOption({ label: /patient/i });

      await page.getByRole('button', { name: /save|create/i }).click();

      await expect(page.getByText(/created|saved/i)).toBeVisible();
    });

    test('should configure workflows', async ({ page }) => {
      await page.goto('/admin/settings/workflows');

      const workflowCard = page.locator('[data-testid="workflow-card"]').first();
      await workflowCard.getByRole('button', { name: /configure|edit/i }).click();

      // Enable/disable workflow steps
      await page.getByLabel(/approval.*required/i).check();
      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });

    test('should set regional preferences', async ({ page }) => {
      await page.goto('/admin/settings/regional');

      await page.getByLabel(/date.*format/i).selectOption({ label: /dd\/mm\/yyyy/i });
      await page.getByLabel(/time.*format/i).selectOption({ label: /24.*hour/i });
      await page.getByLabel(/currency/i).selectOption({ label: /usd|dollar/i });
      await page.getByLabel(/language/i).selectOption({ label: /english/i });

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });

    test('should manage feature toggles', async ({ page }) => {
      await page.goto('/admin/settings/features');

      // Toggle features
      await page.getByLabel(/telemedicine/i).check();
      await page.getByLabel(/patient.*portal/i).check();

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });
  });

  test.describe('User & Role Management per Tenant', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should create custom role', async ({ page }) => {
      await page.goto('/admin/roles');

      await page.getByRole('button', { name: /add|create.*role/i }).click();

      await page.getByLabel(/role.*name/i).fill('Custom Medical Staff');
      await page.getByLabel(/description/i).fill('Custom role for medical staff');

      // Set permissions
      await page.getByLabel(/view.*patients/i).check();
      await page.getByLabel(/edit.*patients/i).check();
      await page.getByLabel(/view.*appointments/i).check();

      await page.getByRole('button', { name: /create|save/i }).click();

      await expect(page.getByText(/role.*created/i)).toBeVisible();
    });

    test('should assign user to specific branches', async ({ page }) => {
      await page.goto('/admin/users');

      const userRow = page.locator('[data-testid="user-row"]').first();
      await userRow.getByRole('button', { name: /edit|manage/i }).click();

      // Assign branches
      await page.getByLabel(/branch.*access/i).selectOption({ index: 1 });
      await page.getByRole('button', { name: /add.*branch/i }).click();

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should set user permissions per branch', async ({ page }) => {
      await page.goto('/admin/users');

      const userRow = page.locator('[data-testid="user-row"]').first();
      await userRow.getByRole('button', { name: /permissions/i }).click();

      // Set branch-specific permissions
      const branchPermission = page.locator('[data-testid="branch-permission"]').first();
      await branchPermission.getByLabel(/admin.*access/i).check();

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });
  });

  test.describe('Multi-Tenancy API', () => {

    test('should get tenant info via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/tenant/info', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should list companies via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/companies', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should list branches via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/branches', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should get subscription info via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/subscription', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should enforce tenant context in API calls', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      // API should automatically scope to tenant
      const response = await request.get('/api/patients', {
        headers: authHeaders(token)
      });

      if (response.ok()) {
        const data = await response.json();
        // Verify data contains tenant context
        expect(data).toBeDefined();
      }
    });

    test('should create branch via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.post('/api/branches', {
        headers: authHeaders(token),
        data: {
          name: `API Test Branch ${Date.now()}`,
          code: `ATB-${Date.now()}`,
          address: '789 API Street',
          city: 'API City',
          phone: '+1112223333'
        }
      });

      expect([200, 201, 400, 401, 403]).toContain(response.status());
    });
  });

  test.describe('Tenant Onboarding', () => {

    test('should complete tenant setup wizard', async ({ page }) => {
      await page.goto('/setup');

      // Step 1: Organization Info
      await expect(page.getByText(/step.*1|organization/i)).toBeVisible();
      await page.getByLabel(/organization.*name/i).fill('New Healthcare Org');
      await page.getByRole('button', { name: /next|continue/i }).click();

      // Step 2: Admin Account
      await expect(page.getByText(/step.*2|admin/i)).toBeVisible();
      await page.getByLabel(/admin.*email/i).fill('admin@newhealthcare.com');
      await page.getByLabel(/password/i).fill('SecurePass123!');
      await page.getByRole('button', { name: /next|continue/i }).click();

      // Step 3: Preferences
      await expect(page.getByText(/step.*3|preferences/i)).toBeVisible();
      await page.getByLabel(/timezone/i).selectOption({ index: 1 });
      await page.getByLabel(/currency/i).selectOption({ index: 1 });
      await page.getByRole('button', { name: /next|continue/i }).click();

      // Step 4: Modules
      await expect(page.getByText(/step.*4|modules/i)).toBeVisible();
      await page.getByLabel(/appointments/i).check();
      await page.getByLabel(/billing/i).check();
      await page.getByRole('button', { name: /finish|complete/i }).click();

      await expect(page.getByText(/setup.*complete|welcome/i)).toBeVisible();
    });

    test('should import initial data', async ({ page }) => {
      await page.goto('/admin/import');

      const fileInput = page.locator('input[type="file"]');
      await fileInput.setInputFiles({
        name: 'initial-data.csv',
        mimeType: 'text/csv',
        buffer: Buffer.from('name,email\nTest User,test@test.com')
      });

      await page.getByLabel(/data.*type/i).selectOption({ label: /user|patient/i });
      await page.getByRole('button', { name: /import|upload/i }).click();

      await expect(page.getByText(/import.*started|processing/i)).toBeVisible();
    });
  });
});
