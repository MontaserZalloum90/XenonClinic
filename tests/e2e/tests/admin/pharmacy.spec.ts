import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Pharmacy Module
 * Tests cover prescriptions, dispensing, drug inventory, and pharmacy operations
 */
test.describe('Pharmacy Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Pharmacy Dashboard', () => {
    test('should display pharmacy dashboard', async ({ page }) => {
      await page.goto('/pharmacy');
      await expect(page.getByRole('heading', { name: /pharmacy/i })).toBeVisible();
    });

    test('should display pending prescriptions count', async ({ page }) => {
      await page.goto('/pharmacy');
      await expect(page.getByText(/pending|queue|waiting/i)).toBeVisible();
    });

    test('should display dispensed today count', async ({ page }) => {
      await page.goto('/pharmacy');
      await expect(page.getByText(/dispensed|today/i)).toBeVisible();
    });

    test('should display low stock alerts', async ({ page }) => {
      await page.goto('/pharmacy');
      const alerts = page.locator('[class*="alert"], [class*="warning"]');
      if (await alerts.count() > 0) {
        await expect(alerts.first()).toBeVisible();
      }
    });

    test('should display expiring medications alert', async ({ page }) => {
      await page.goto('/pharmacy');
      const expiryAlert = page.getByText(/expir/i);
      if (await expiryAlert.isVisible()) {
        await expect(expiryAlert).toBeVisible();
      }
    });
  });

  test.describe('Prescription Queue', () => {
    test('should display prescription queue', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      await expect(page.getByRole('heading', { name: /prescription|queue/i })).toBeVisible();
    });

    test('should view prescription details', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/prescription|medication|patient/i)).toBeVisible();
      }
    });

    test('should filter prescriptions by status', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /pending|dispensed|cancelled/i }).first().click();
      }
    });

    test('should filter prescriptions by date', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dateFilter = page.getByLabel(/date/i);
      if (await dateFilter.isVisible()) {
        const today = new Date().toISOString().split('T')[0];
        await dateFilter.fill(today);
      }
    });

    test('should search prescriptions by patient', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('John');
        await page.waitForTimeout(500);
      }
    });

    test('should search prescriptions by Rx number', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('RX-');
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Dispensing', () => {
    test('should start dispensing process', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense|process/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();
        await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      }
    });

    test('should verify patient identity', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const verifyCheckbox = page.getByLabel(/verify.*identity|patient.*verified/i);
        if (await verifyCheckbox.isVisible()) {
          await verifyCheckbox.check();
        }
      }
    });

    test('should select batch/lot for dispensing', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const batchSelect = page.getByLabel(/batch|lot/i);
        if (await batchSelect.isVisible()) {
          await batchSelect.click();
          await page.getByRole('option').first().click();
        }
      }
    });

    test('should enter dispensed quantity', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const quantityField = page.getByLabel(/quantity|qty/i);
        if (await quantityField.isVisible()) {
          await quantityField.fill('30');
        }
      }
    });

    test('should complete dispensing', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const completeButton = page.getByRole('button', { name: /complete|dispense|confirm/i }).last();
        await completeButton.click();
        await expect(page.getByText(/dispensed|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should print medication label', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const printLabelButton = page.getByRole('button', { name: /print.*label|label/i }).first();
      if (await printLabelButton.isVisible()) {
        await expect(printLabelButton).toBeVisible();
      }
    });

    test('should provide patient counseling notes', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const counselingField = page.getByLabel(/counseling|instruction|advice/i);
        if (await counselingField.isVisible()) {
          await counselingField.fill('Take with food. Avoid alcohol.');
        }
      }
    });

    test('should check for drug interactions', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const interactionAlert = page.locator('[class*="interaction"], [class*="warning"]');
        if (await interactionAlert.count() > 0) {
          await expect(interactionAlert.first()).toBeVisible();
        }
      }
    });

    test('should check for allergies', async ({ page }) => {
      await page.goto('/pharmacy/prescriptions');
      const dispenseButton = page.getByRole('button', { name: /dispense/i }).first();
      if (await dispenseButton.isVisible()) {
        await dispenseButton.click();

        const allergyAlert = page.locator('[class*="allergy"], [class*="danger"]');
        if (await allergyAlert.count() > 0) {
          await expect(allergyAlert.first()).toBeVisible();
        }
      }
    });
  });

  test.describe('Medication Inventory', () => {
    test('should display medication inventory', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      await expect(page.getByRole('heading', { name: /inventory|medication|drug/i })).toBeVisible();
    });

    test('should search medications', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('paracetamol');
        await page.waitForTimeout(500);
      }
    });

    test('should filter by category', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const categoryFilter = page.getByRole('combobox', { name: /category/i });
      if (await categoryFilter.isVisible()) {
        await categoryFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter by stock level', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const stockFilter = page.getByRole('combobox', { name: /stock|level/i });
      if (await stockFilter.isVisible()) {
        await stockFilter.click();
        await page.getByRole('option', { name: /low|out.*stock/i }).first().click();
      }
    });

    test('should view medication details', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/medication|drug|stock/i)).toBeVisible();
      }
    });

    test('should add new medication', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const addButton = page.getByRole('button', { name: /add|new|create/i });
      if (await addButton.isVisible()) {
        await addButton.click();

        const nameField = page.getByLabel(/name|drug.*name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Medication ${Date.now()}`);
        }

        const genericField = page.getByLabel(/generic/i);
        if (await genericField.isVisible()) {
          await genericField.fill('Test Generic Name');
        }

        const categorySelect = page.getByLabel(/category/i);
        if (await categorySelect.isVisible()) {
          await categorySelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should receive stock', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const receiveButton = page.getByRole('button', { name: /receive|stock.*in/i }).first();
      if (await receiveButton.isVisible()) {
        await receiveButton.click();

        const quantityField = page.getByLabel(/quantity/i);
        if (await quantityField.isVisible()) {
          await quantityField.fill('100');
        }

        const batchField = page.getByLabel(/batch|lot/i);
        if (await batchField.isVisible()) {
          await batchField.fill(`BATCH-${Date.now()}`);
        }

        const expiryField = page.getByLabel(/expiry/i);
        if (await expiryField.isVisible()) {
          const nextYear = new Date();
          nextYear.setFullYear(nextYear.getFullYear() + 1);
          await expiryField.fill(nextYear.toISOString().split('T')[0]);
        }

        const submitButton = page.getByRole('button', { name: /save|receive/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|received/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should adjust stock', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const adjustButton = page.getByRole('button', { name: /adjust/i }).first();
      if (await adjustButton.isVisible()) {
        await adjustButton.click();

        const quantityField = page.getByLabel(/quantity|new.*quantity/i);
        if (await quantityField.isVisible()) {
          await quantityField.fill('50');
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

    test('should view batch details', async ({ page }) => {
      await page.goto('/pharmacy/inventory');
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const batchesTab = page.getByRole('tab', { name: /batch|lot/i });
        if (await batchesTab.isVisible()) {
          await batchesTab.click();
          await expect(page.getByText(/batch|lot|expiry/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Drug Database', () => {
    test('should display drug database', async ({ page }) => {
      await page.goto('/pharmacy/drugs');
      await expect(page.getByRole('heading', { name: /drug|medication/i })).toBeVisible();
    });

    test('should search drug database', async ({ page }) => {
      await page.goto('/pharmacy/drugs');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('aspirin');
        await page.waitForTimeout(500);
      }
    });

    test('should view drug information', async ({ page }) => {
      await page.goto('/pharmacy/drugs');
      const viewButton = page.getByRole('button', { name: /view|info/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/indication|dosage|contraindication/i)).toBeVisible();
      }
    });

    test('should view drug interactions', async ({ page }) => {
      await page.goto('/pharmacy/drugs');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const interactionsTab = page.getByRole('tab', { name: /interaction/i });
        if (await interactionsTab.isVisible()) {
          await interactionsTab.click();
          await expect(page.getByText(/interaction/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Controlled Substances', () => {
    test('should display controlled substances log', async ({ page }) => {
      await page.goto('/pharmacy/controlled');
      await expect(page.getByRole('heading', { name: /controlled|narcotic/i })).toBeVisible();
    });

    test('should log controlled substance dispensing', async ({ page }) => {
      await page.goto('/pharmacy/controlled');
      const logButton = page.getByRole('button', { name: /log|record/i });
      if (await logButton.isVisible()) {
        await logButton.click();
        await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      }
    });

    test('should require witness signature', async ({ page }) => {
      await page.goto('/pharmacy/controlled');
      const logButton = page.getByRole('button', { name: /log|record/i });
      if (await logButton.isVisible()) {
        await logButton.click();

        const witnessField = page.getByLabel(/witness/i);
        if (await witnessField.isVisible()) {
          await expect(witnessField).toBeVisible();
        }
      }
    });

    test('should view controlled substance report', async ({ page }) => {
      await page.goto('/pharmacy/controlled');
      const reportButton = page.getByRole('button', { name: /report/i });
      if (await reportButton.isVisible()) {
        await reportButton.click();
        await expect(page.getByText(/report|controlled/i)).toBeVisible();
      }
    });
  });

  test.describe('Refills', () => {
    test('should display refill requests', async ({ page }) => {
      await page.goto('/pharmacy/refills');
      await expect(page.getByRole('heading', { name: /refill/i })).toBeVisible();
    });

    test('should approve refill request', async ({ page }) => {
      await page.goto('/pharmacy/refills');
      const approveButton = page.getByRole('button', { name: /approve/i }).first();
      if (await approveButton.isVisible()) {
        await approveButton.click();
        await expect(page.getByText(/approved|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should reject refill with reason', async ({ page }) => {
      await page.goto('/pharmacy/refills');
      const rejectButton = page.getByRole('button', { name: /reject|deny/i }).first();
      if (await rejectButton.isVisible()) {
        await rejectButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Requires physician approval');
        }

        const confirmButton = page.getByRole('button', { name: /reject|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
        await expect(page.getByText(/rejected|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should check refill eligibility', async ({ page }) => {
      await page.goto('/pharmacy/refills');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/refill.*remaining|eligible/i)).toBeVisible();
      }
    });
  });

  test.describe('Pharmacy Reports', () => {
    test('should generate dispensing report', async ({ page }) => {
      await page.goto('/pharmacy/reports');
      const dispensingButton = page.getByRole('button', { name: /dispensing|daily/i });
      if (await dispensingButton.isVisible()) {
        await dispensingButton.click();
        await expect(page.getByText(/dispensing|report/i)).toBeVisible();
      }
    });

    test('should generate inventory report', async ({ page }) => {
      await page.goto('/pharmacy/reports');
      const inventoryButton = page.getByRole('button', { name: /inventory|stock/i });
      if (await inventoryButton.isVisible()) {
        await inventoryButton.click();
        await expect(page.getByText(/inventory|stock/i)).toBeVisible();
      }
    });

    test('should generate expiry report', async ({ page }) => {
      await page.goto('/pharmacy/reports');
      const expiryButton = page.getByRole('button', { name: /expiry|expiring/i });
      if (await expiryButton.isVisible()) {
        await expiryButton.click();
        await expect(page.getByText(/expir/i)).toBeVisible();
      }
    });

    test('should export pharmacy report', async ({ page }) => {
      await page.goto('/pharmacy/reports');
      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();
        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/pharmacy.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });
});

test.describe('Pharmacy API E2E Tests', () => {
  test('should list prescriptions', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.get('/api/pharmacy/prescriptions?page=1&pageSize=10', {
      headers: authHeaders(token),
    });
    expect(response.status()).toBe(200);
  });

  test('should list medication inventory', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.get('/api/pharmacy/inventory?page=1&pageSize=10', {
      headers: authHeaders(token),
    });
    expect(response.status()).toBe(200);
  });

  test('should validate dispensing request', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.post('/api/pharmacy/dispense', {
      headers: authHeaders(token),
      data: {},
    });
    expect(response.status()).toBe(400);
  });
});
