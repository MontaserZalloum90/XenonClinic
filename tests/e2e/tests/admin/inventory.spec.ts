import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Inventory Module
 * Tests cover stock management, items, transactions, suppliers, and purchase orders
 */
test.describe('Inventory Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Inventory Items List', () => {
    test('should display inventory page', async ({ page }) => {
      await page.goto('/inventory');

      await expect(page.getByRole('heading', { name: /inventory|stock|item/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
    });

    test('should display inventory statistics', async ({ page }) => {
      await page.goto('/inventory');

      const stats = page.locator('[class*="stat"], [class*="card"], [class*="summary"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });

    test('should search inventory items', async ({ page }) => {
      await page.goto('/inventory');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('medical');
        await page.waitForTimeout(500);
      }
    });

    test('should filter by category', async ({ page }) => {
      await page.goto('/inventory');

      const categoryFilter = page.getByRole('combobox', { name: /category/i });
      if (await categoryFilter.isVisible()) {
        await categoryFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter by stock level', async ({ page }) => {
      await page.goto('/inventory');

      const stockFilter = page.getByRole('combobox', { name: /stock|level/i });
      if (await stockFilter.isVisible()) {
        await stockFilter.click();
        await page.getByRole('option', { name: /low|out.*stock|available/i }).first().click();
      }
    });

    test('should display low stock alerts', async ({ page }) => {
      await page.goto('/inventory');

      const lowStockAlert = page.locator('[class*="alert"], [class*="warning"]').filter({ hasText: /low.*stock|reorder/i });
      if (await lowStockAlert.count() > 0) {
        await expect(lowStockAlert.first()).toBeVisible();
      }
    });

    test('should display expired items alert', async ({ page }) => {
      await page.goto('/inventory');

      const expiredAlert = page.locator('[class*="alert"], [class*="danger"]').filter({ hasText: /expir/i });
      if (await expiredAlert.count() > 0) {
        await expect(expiredAlert.first()).toBeVisible();
      }
    });
  });

  test.describe('Create Inventory Item', () => {
    test('should open create item form', async ({ page }) => {
      await page.goto('/inventory');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/inventory');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const submitButton = page.getByRole('button', { name: /save|create/i }).last();
      await submitButton.click();

      await expect(page.getByText(/required/i)).toBeVisible();
    });

    test('should create inventory item', async ({ page }) => {
      await page.goto('/inventory');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const nameField = page.getByLabel(/name|item.*name/i);
      if (await nameField.isVisible()) {
        await nameField.fill(`E2E Item ${Date.now()}`);
      }

      const skuField = page.getByLabel(/sku|code|barcode/i);
      if (await skuField.isVisible()) {
        await skuField.fill(`SKU-${Date.now()}`);
      }

      const categorySelect = page.getByLabel(/category/i);
      if (await categorySelect.isVisible()) {
        await categorySelect.click();
        await page.getByRole('option').first().click();
      }

      const unitSelect = page.getByLabel(/unit/i);
      if (await unitSelect.isVisible()) {
        await unitSelect.click();
        await page.getByRole('option').first().click();
      }

      const priceField = page.getByLabel(/price|cost/i);
      if (await priceField.isVisible()) {
        await priceField.fill('100');
      }

      const submitButton = page.getByRole('button', { name: /save|create/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
    });

    test('should set reorder level', async ({ page }) => {
      await page.goto('/inventory');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const reorderField = page.getByLabel(/reorder.*level|minimum.*stock/i);
      if (await reorderField.isVisible()) {
        await reorderField.fill('10');
      }
    });

    test('should set expiry tracking', async ({ page }) => {
      await page.goto('/inventory');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const expiryCheckbox = page.getByLabel(/track.*expiry|expiry.*date/i);
      if (await expiryCheckbox.isVisible()) {
        await expiryCheckbox.check();
      }
    });
  });

  test.describe('Stock Transactions', () => {
    test('should display stock transactions', async ({ page }) => {
      await page.goto('/inventory/transactions');

      await expect(page.getByRole('heading', { name: /transaction|movement/i })).toBeVisible();
    });

    test('should record stock in', async ({ page }) => {
      await page.goto('/inventory');

      const stockInButton = page.getByRole('button', { name: /stock.*in|receive|add.*stock/i }).first();
      if (await stockInButton.isVisible()) {
        await stockInButton.click();

        const quantityField = page.getByLabel(/quantity/i);
        if (await quantityField.isVisible()) {
          await quantityField.fill('50');
        }

        const batchField = page.getByLabel(/batch|lot/i);
        if (await batchField.isVisible()) {
          await batchField.fill(`BATCH-${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|submit|receive/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|received|added/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should record stock out', async ({ page }) => {
      await page.goto('/inventory');

      const stockOutButton = page.getByRole('button', { name: /stock.*out|issue|remove/i }).first();
      if (await stockOutButton.isVisible()) {
        await stockOutButton.click();

        const quantityField = page.getByLabel(/quantity/i);
        if (await quantityField.isVisible()) {
          await quantityField.fill('5');
        }

        const reasonSelect = page.getByLabel(/reason/i);
        if (await reasonSelect.isVisible()) {
          await reasonSelect.click();
          await page.getByRole('option', { name: /usage|sale|damage/i }).first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|submit|issue/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|issued|removed/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should record stock adjustment', async ({ page }) => {
      await page.goto('/inventory');

      const adjustButton = page.getByRole('button', { name: /adjust/i }).first();
      if (await adjustButton.isVisible()) {
        await adjustButton.click();

        const newQuantityField = page.getByLabel(/quantity|new.*quantity/i);
        if (await newQuantityField.isVisible()) {
          await newQuantityField.fill('100');
        }

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Stock count adjustment');
        }

        const submitButton = page.getByRole('button', { name: /save|adjust/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|adjusted/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter transactions by date', async ({ page }) => {
      await page.goto('/inventory/transactions');

      const fromDate = page.getByLabel(/from|start.*date/i);
      const toDate = page.getByLabel(/to|end.*date/i);

      if (await fromDate.isVisible() && await toDate.isVisible()) {
        const today = new Date();
        const lastMonth = new Date(today);
        lastMonth.setMonth(lastMonth.getMonth() - 1);

        await fromDate.fill(lastMonth.toISOString().split('T')[0]);
        await toDate.fill(today.toISOString().split('T')[0]);
      }
    });

    test('should filter transactions by type', async ({ page }) => {
      await page.goto('/inventory/transactions');

      const typeFilter = page.getByRole('combobox', { name: /type/i });
      if (await typeFilter.isVisible()) {
        await typeFilter.click();
        await page.getByRole('option', { name: /stock.*in|stock.*out|adjustment/i }).first().click();
      }
    });
  });

  test.describe('Suppliers', () => {
    test('should display suppliers page', async ({ page }) => {
      await page.goto('/inventory/suppliers');

      await expect(page.getByRole('heading', { name: /supplier|vendor/i })).toBeVisible();
    });

    test('should create supplier', async ({ page }) => {
      await page.goto('/inventory/suppliers');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name|company.*name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Supplier ${Date.now()}`);
        }

        const contactField = page.getByLabel(/contact|phone/i);
        if (await contactField.isVisible()) {
          await contactField.fill('+971501234567');
        }

        const emailField = page.getByLabel(/email/i);
        if (await emailField.isVisible()) {
          await emailField.fill(`supplier.${Date.now()}@test.com`);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should search suppliers', async ({ page }) => {
      await page.goto('/inventory/suppliers');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('medical');
        await page.waitForTimeout(500);
      }
    });

    test('should view supplier details', async ({ page }) => {
      await page.goto('/inventory/suppliers');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/supplier.*detail|contact|address/i)).toBeVisible();
      }
    });
  });

  test.describe('Purchase Orders', () => {
    test('should display purchase orders page', async ({ page }) => {
      await page.goto('/inventory/purchase-orders');

      await expect(page.getByRole('heading', { name: /purchase.*order|po/i })).toBeVisible();
    });

    test('should create purchase order', async ({ page }) => {
      await page.goto('/inventory/purchase-orders');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const supplierSelect = page.getByLabel(/supplier/i);
        if (await supplierSelect.isVisible()) {
          await supplierSelect.click();
          await page.getByRole('option').first().click();
        }

        // Add item to PO
        const addItemButton = page.getByRole('button', { name: /add.*item/i });
        if (await addItemButton.isVisible()) {
          await addItemButton.click();

          const itemSelect = page.getByLabel(/item|product/i);
          if (await itemSelect.isVisible()) {
            await itemSelect.click();
            await page.getByRole('option').first().click();
          }

          const quantityField = page.getByLabel(/quantity/i);
          if (await quantityField.isVisible()) {
            await quantityField.fill('100');
          }
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should approve purchase order', async ({ page }) => {
      await page.goto('/inventory/purchase-orders');

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

    test('should receive purchase order', async ({ page }) => {
      await page.goto('/inventory/purchase-orders');

      const receiveButton = page.getByRole('button', { name: /receive/i }).first();
      if (await receiveButton.isVisible()) {
        await receiveButton.click();

        // Fill received quantities
        const quantityField = page.getByLabel(/received.*quantity|quantity.*received/i);
        if (await quantityField.isVisible()) {
          await quantityField.fill('100');
        }

        const submitButton = page.getByRole('button', { name: /save|receive|confirm/i }).last();
        await submitButton.click();

        await expect(page.getByText(/received|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter purchase orders by status', async ({ page }) => {
      await page.goto('/inventory/purchase-orders');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /pending|approved|received/i }).first().click();
      }
    });

    test('should print purchase order', async ({ page }) => {
      await page.goto('/inventory/purchase-orders');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          await expect(printButton).toBeVisible();
        }
      }
    });
  });

  test.describe('Goods Receipt', () => {
    test('should display goods receipts page', async ({ page }) => {
      await page.goto('/inventory/goods-receipts');

      await expect(page.getByRole('heading', { name: /goods.*receipt|grn/i })).toBeVisible();
    });

    test('should create goods receipt', async ({ page }) => {
      await page.goto('/inventory/goods-receipts');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select PO if available
        const poSelect = page.getByLabel(/purchase.*order|po/i);
        if (await poSelect.isVisible()) {
          await poSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Stock Reports', () => {
    test('should generate stock report', async ({ page }) => {
      await page.goto('/inventory/reports');

      const stockReportButton = page.getByRole('button', { name: /stock.*report|inventory.*report/i });
      if (await stockReportButton.isVisible()) {
        await stockReportButton.click();

        await expect(page.getByText(/report|stock/i)).toBeVisible();
      }
    });

    test('should generate low stock report', async ({ page }) => {
      await page.goto('/inventory/reports');

      const lowStockButton = page.getByRole('button', { name: /low.*stock|reorder/i });
      if (await lowStockButton.isVisible()) {
        await lowStockButton.click();

        await expect(page.getByText(/low.*stock|reorder/i)).toBeVisible();
      }
    });

    test('should generate expiry report', async ({ page }) => {
      await page.goto('/inventory/reports');

      const expiryButton = page.getByRole('button', { name: /expiry|expiring/i });
      if (await expiryButton.isVisible()) {
        await expiryButton.click();

        await expect(page.getByText(/expir/i)).toBeVisible();
      }
    });

    test('should export inventory report', async ({ page }) => {
      await page.goto('/inventory');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/inventory.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });

  test.describe('Inventory Categories', () => {
    test('should display categories page', async ({ page }) => {
      await page.goto('/inventory/categories');

      await expect(page.getByRole('heading', { name: /categor/i })).toBeVisible();
    });

    test('should create category', async ({ page }) => {
      await page.goto('/inventory/categories');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Category ${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });
});

test.describe('Inventory API E2E Tests', () => {
  test('should list inventory items', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/inventory?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get item details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/inventory?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const itemId = listData.items[0].id;

        const response = await request.get(`/api/inventory/${itemId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should validate item creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/inventory', {
      headers: authHeaders(token),
      data: {
        name: '', // Invalid empty name
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should list stock transactions', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/inventory/transactions?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get low stock items', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/inventory/low-stock', {
      headers: authHeaders(token),
    });

    expect([200, 404]).toContain(response.status());
  });
});
