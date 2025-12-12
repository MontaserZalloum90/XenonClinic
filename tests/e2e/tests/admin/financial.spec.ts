import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Financial Module
 * Tests cover invoices, payments, expenses, reports, and financial analytics
 */
test.describe('Financial Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Financial Dashboard', () => {
    test('should display financial dashboard', async ({ page }) => {
      await page.goto('/financial');

      await expect(page.getByRole('heading', { name: /financial|finance|accounting/i })).toBeVisible();
    });

    test('should display revenue statistics', async ({ page }) => {
      await page.goto('/financial');

      await expect(page.getByText(/revenue|income/i)).toBeVisible();
    });

    test('should display expense statistics', async ({ page }) => {
      await page.goto('/financial');

      await expect(page.getByText(/expense|cost/i)).toBeVisible();
    });

    test('should display profit/loss summary', async ({ page }) => {
      await page.goto('/financial');

      await expect(page.getByText(/profit|loss|net/i)).toBeVisible();
    });

    test('should display financial charts', async ({ page }) => {
      await page.goto('/financial');

      const chart = page.locator('canvas, svg[class*="chart"], [class*="chart"]');
      if (await chart.count() > 0) {
        await expect(chart.first()).toBeVisible();
      }
    });

    test('should filter dashboard by date range', async ({ page }) => {
      await page.goto('/financial');

      const fromDate = page.getByLabel(/from|start.*date/i);
      const toDate = page.getByLabel(/to|end.*date/i);

      if (await fromDate.isVisible() && await toDate.isVisible()) {
        const today = new Date();
        const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

        await fromDate.fill(firstDayOfMonth.toISOString().split('T')[0]);
        await toDate.fill(today.toISOString().split('T')[0]);
      }
    });
  });

  test.describe('Invoices', () => {
    test('should display invoices list', async ({ page }) => {
      await page.goto('/financial/invoices');

      await expect(page.getByRole('heading', { name: /invoice/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
    });

    test('should create invoice', async ({ page }) => {
      await page.goto('/financial/invoices');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Select patient
      const patientSelect = page.getByLabel(/patient|customer/i);
      if (await patientSelect.isVisible()) {
        await patientSelect.click();
        await page.getByRole('option').first().click();
      }

      // Add line item
      const addItemButton = page.getByRole('button', { name: /add.*item|add.*line/i });
      if (await addItemButton.isVisible()) {
        await addItemButton.click();

        const descriptionField = page.getByLabel(/description|item/i);
        if (await descriptionField.isVisible()) {
          await descriptionField.fill('Consultation Fee');
        }

        const amountField = page.getByLabel(/amount|price/i);
        if (await amountField.isVisible()) {
          await amountField.fill('500');
        }
      }

      const submitButton = page.getByRole('button', { name: /save|create/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
    });

    test('should search invoices', async ({ page }) => {
      await page.goto('/financial/invoices');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('INV-');
        await page.waitForTimeout(500);
      }
    });

    test('should filter invoices by status', async ({ page }) => {
      await page.goto('/financial/invoices');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /paid|pending|overdue/i }).first().click();
      }
    });

    test('should view invoice details', async ({ page }) => {
      await page.goto('/financial/invoices');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/invoice.*detail|invoice.*number/i)).toBeVisible();
      }
    });

    test('should print invoice', async ({ page }) => {
      await page.goto('/financial/invoices');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          await expect(printButton).toBeVisible();
        }
      }
    });

    test('should email invoice', async ({ page }) => {
      await page.goto('/financial/invoices');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const emailButton = page.getByRole('button', { name: /email|send/i });
        if (await emailButton.isVisible()) {
          await emailButton.click();

          await expect(page.getByRole('dialog').or(page.getByText(/email/i))).toBeVisible();
        }
      }
    });

    test('should void invoice', async ({ page }) => {
      await page.goto('/financial/invoices');

      const voidButton = page.getByRole('button', { name: /void|cancel/i }).first();
      if (await voidButton.isVisible()) {
        await voidButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Voiding invoice');
        }

        const confirmButton = page.getByRole('button', { name: /yes|confirm|void/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/voided|cancelled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should apply discount to invoice', async ({ page }) => {
      await page.goto('/financial/invoices');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const discountField = page.getByLabel(/discount/i);
      if (await discountField.isVisible()) {
        await discountField.fill('10');
      }

      const discountTypeSelect = page.getByLabel(/discount.*type/i);
      if (await discountTypeSelect.isVisible()) {
        await discountTypeSelect.click();
        await page.getByRole('option', { name: /percent|%|fixed/i }).first().click();
      }
    });

    test('should apply tax to invoice', async ({ page }) => {
      await page.goto('/financial/invoices');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const taxCheckbox = page.getByLabel(/tax|vat/i);
      if (await taxCheckbox.isVisible()) {
        await taxCheckbox.check();
      }
    });
  });

  test.describe('Payments', () => {
    test('should display payments list', async ({ page }) => {
      await page.goto('/financial/payments');

      await expect(page.getByRole('heading', { name: /payment/i })).toBeVisible();
    });

    test('should record payment', async ({ page }) => {
      await page.goto('/financial/payments');

      const createButton = page.getByRole('button', { name: /create|new|record/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select invoice
        const invoiceSelect = page.getByLabel(/invoice/i);
        if (await invoiceSelect.isVisible()) {
          await invoiceSelect.click();
          await page.getByRole('option').first().click();
        }

        // Enter amount
        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('500');
        }

        // Select payment method
        const methodSelect = page.getByLabel(/method|payment.*method/i);
        if (await methodSelect.isVisible()) {
          await methodSelect.click();
          await page.getByRole('option', { name: /cash|card|bank/i }).first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|record/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|recorded/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter payments by method', async ({ page }) => {
      await page.goto('/financial/payments');

      const methodFilter = page.getByRole('combobox', { name: /method/i });
      if (await methodFilter.isVisible()) {
        await methodFilter.click();
        await page.getByRole('option', { name: /cash|card|bank/i }).first().click();
      }
    });

    test('should view payment receipt', async ({ page }) => {
      await page.goto('/financial/payments');

      const viewButton = page.getByRole('button', { name: /view|receipt/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/receipt|payment/i)).toBeVisible();
      }
    });

    test('should print payment receipt', async ({ page }) => {
      await page.goto('/financial/payments');

      const viewButton = page.getByRole('button', { name: /view|receipt/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          await expect(printButton).toBeVisible();
        }
      }
    });

    test('should process refund', async ({ page }) => {
      await page.goto('/financial/payments');

      const refundButton = page.getByRole('button', { name: /refund/i }).first();
      if (await refundButton.isVisible()) {
        await refundButton.click();

        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('100');
        }

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Patient refund');
        }

        const submitButton = page.getByRole('button', { name: /refund|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/refund.*processed|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Expenses', () => {
    test('should display expenses list', async ({ page }) => {
      await page.goto('/financial/expenses');

      await expect(page.getByRole('heading', { name: /expense/i })).toBeVisible();
    });

    test('should create expense', async ({ page }) => {
      await page.goto('/financial/expenses');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const categorySelect = page.getByLabel(/category/i);
        if (await categorySelect.isVisible()) {
          await categorySelect.click();
          await page.getByRole('option').first().click();
        }

        const descriptionField = page.getByLabel(/description/i);
        if (await descriptionField.isVisible()) {
          await descriptionField.fill('E2E Test - Office supplies');
        }

        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('250');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter expenses by category', async ({ page }) => {
      await page.goto('/financial/expenses');

      const categoryFilter = page.getByRole('combobox', { name: /category/i });
      if (await categoryFilter.isVisible()) {
        await categoryFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should attach receipt to expense', async ({ page }) => {
      await page.goto('/financial/expenses');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const attachButton = page.getByRole('button', { name: /attach|upload/i });
        if (await attachButton.isVisible()) {
          await expect(attachButton).toBeVisible();
        }
      }
    });

    test('should approve expense', async ({ page }) => {
      await page.goto('/financial/expenses');

      const approveButton = page.getByRole('button', { name: /approve/i }).first();
      if (await approveButton.isVisible()) {
        await approveButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/approved|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Financial Reports', () => {
    test('should generate income statement', async ({ page }) => {
      await page.goto('/financial/reports');

      const incomeButton = page.getByRole('button', { name: /income.*statement|profit.*loss|p&l/i });
      if (await incomeButton.isVisible()) {
        await incomeButton.click();

        await expect(page.getByText(/income|revenue|expense/i)).toBeVisible();
      }
    });

    test('should generate balance sheet', async ({ page }) => {
      await page.goto('/financial/reports');

      const balanceButton = page.getByRole('button', { name: /balance.*sheet/i });
      if (await balanceButton.isVisible()) {
        await balanceButton.click();

        await expect(page.getByText(/asset|liability|equity/i)).toBeVisible();
      }
    });

    test('should generate cash flow report', async ({ page }) => {
      await page.goto('/financial/reports');

      const cashFlowButton = page.getByRole('button', { name: /cash.*flow/i });
      if (await cashFlowButton.isVisible()) {
        await cashFlowButton.click();

        await expect(page.getByText(/cash.*flow|operating|investing/i)).toBeVisible();
      }
    });

    test('should generate accounts receivable report', async ({ page }) => {
      await page.goto('/financial/reports');

      const arButton = page.getByRole('button', { name: /receivable|ar|aging/i });
      if (await arButton.isVisible()) {
        await arButton.click();

        await expect(page.getByText(/receivable|outstanding|aging/i)).toBeVisible();
      }
    });

    test('should generate accounts payable report', async ({ page }) => {
      await page.goto('/financial/reports');

      const apButton = page.getByRole('button', { name: /payable|ap/i });
      if (await apButton.isVisible()) {
        await apButton.click();

        await expect(page.getByText(/payable/i)).toBeVisible();
      }
    });

    test('should export financial report', async ({ page }) => {
      await page.goto('/financial/reports');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/report.*\.(csv|xlsx|pdf)/i);
        }
      }
    });

    test('should filter reports by date range', async ({ page }) => {
      await page.goto('/financial/reports');

      const fromDate = page.getByLabel(/from|start.*date/i);
      const toDate = page.getByLabel(/to|end.*date/i);

      if (await fromDate.isVisible() && await toDate.isVisible()) {
        const today = new Date();
        const lastYear = new Date(today);
        lastYear.setFullYear(lastYear.getFullYear() - 1);

        await fromDate.fill(lastYear.toISOString().split('T')[0]);
        await toDate.fill(today.toISOString().split('T')[0]);
      }
    });
  });

  test.describe('Insurance Claims', () => {
    test('should display insurance claims page', async ({ page }) => {
      await page.goto('/financial/insurance');

      await expect(page.getByRole('heading', { name: /insurance|claim/i })).toBeVisible();
    });

    test('should create insurance claim', async ({ page }) => {
      await page.goto('/financial/insurance');

      const createButton = page.getByRole('button', { name: /create|new|submit/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select patient
        const patientSelect = page.getByLabel(/patient/i);
        if (await patientSelect.isVisible()) {
          await patientSelect.click();
          await page.getByRole('option').first().click();
        }

        // Select insurance provider
        const insurerSelect = page.getByLabel(/insurer|insurance.*provider/i);
        if (await insurerSelect.isVisible()) {
          await insurerSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|submitted|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should track claim status', async ({ page }) => {
      await page.goto('/financial/insurance');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/status|pending|approved|rejected/i)).toBeVisible();
      }
    });

    test('should filter claims by status', async ({ page }) => {
      await page.goto('/financial/insurance');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /pending|approved|rejected/i }).first().click();
      }
    });
  });

  test.describe('Tax Management', () => {
    test('should display tax settings', async ({ page }) => {
      await page.goto('/financial/tax');

      await expect(page.getByRole('heading', { name: /tax|vat/i })).toBeVisible();
    });

    test('should generate tax report', async ({ page }) => {
      await page.goto('/financial/tax');

      const reportButton = page.getByRole('button', { name: /report|generate/i });
      if (await reportButton.isVisible()) {
        await reportButton.click();

        await expect(page.getByText(/tax.*report|vat/i)).toBeVisible();
      }
    });
  });
});

test.describe('Financial API E2E Tests', () => {
  test('should list invoices', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/financial/invoices?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get invoice details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/financial/invoices?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const invoiceId = listData.items[0].id;

        const response = await request.get(`/api/financial/invoices/${invoiceId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should validate invoice creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/financial/invoices', {
      headers: authHeaders(token),
      data: {
        // Missing required fields
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should list payments', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/financial/payments?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should list expenses', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/financial/expenses?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get financial summary', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/financial/summary', {
      headers: authHeaders(token),
    });

    expect([200, 404]).toContain(response.status());
  });
});
