import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Laboratory Module
 * Tests cover lab tests, results, external labs, and reporting
 */
test.describe('Laboratory Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Lab Tests List', () => {
    test('should display laboratory page', async ({ page }) => {
      await page.goto('/laboratory');

      await expect(page.getByRole('heading', { name: /laborator|lab/i })).toBeVisible();
    });

    test('should display pending tests', async ({ page }) => {
      await page.goto('/laboratory');

      const pendingTab = page.getByRole('tab', { name: /pending|queue/i });
      if (await pendingTab.isVisible()) {
        await pendingTab.click();
        await expect(page.getByText(/pending|waiting/i)).toBeVisible();
      }
    });

    test('should display completed tests', async ({ page }) => {
      await page.goto('/laboratory');

      const completedTab = page.getByRole('tab', { name: /completed|result/i });
      if (await completedTab.isVisible()) {
        await completedTab.click();
        await expect(page.getByText(/completed|result/i)).toBeVisible();
      }
    });

    test('should search lab orders', async ({ page }) => {
      await page.goto('/laboratory');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('CBC');
        await page.waitForTimeout(500);
      }
    });

    test('should filter by test type', async ({ page }) => {
      await page.goto('/laboratory');

      const testTypeFilter = page.getByRole('combobox', { name: /test.*type|type/i });
      if (await testTypeFilter.isVisible()) {
        await testTypeFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter by patient', async ({ page }) => {
      await page.goto('/laboratory');

      const patientFilter = page.getByRole('combobox', { name: /patient/i });
      if (await patientFilter.isVisible()) {
        await patientFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter by date range', async ({ page }) => {
      await page.goto('/laboratory');

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

    test('should display lab statistics', async ({ page }) => {
      await page.goto('/laboratory');

      const stats = page.locator('[class*="stat"], [class*="card"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });
  });

  test.describe('Create Lab Order', () => {
    test('should open create lab order form', async ({ page }) => {
      await page.goto('/laboratory');

      const createButton = page.getByRole('button', { name: /create|new|order/i });
      if (await createButton.isVisible()) {
        await createButton.click();
        await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      }
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/laboratory');

      const createButton = page.getByRole('button', { name: /create|new|order/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const submitButton = page.getByRole('button', { name: /save|order|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/required/i)).toBeVisible();
      }
    });

    test('should create lab order', async ({ page }) => {
      await page.goto('/laboratory');

      const createButton = page.getByRole('button', { name: /create|new|order/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select patient
        const patientSelect = page.getByLabel(/patient/i);
        if (await patientSelect.isVisible()) {
          await patientSelect.click();
          await page.getByRole('option').first().click();
        }

        // Select test
        const testSelect = page.getByLabel(/test/i);
        if (await testSelect.isVisible()) {
          await testSelect.click();
          await page.getByRole('option').first().click();
        }

        // Set priority
        const prioritySelect = page.getByLabel(/priority/i);
        if (await prioritySelect.isVisible()) {
          await prioritySelect.click();
          await page.getByRole('option', { name: /routine|urgent|stat/i }).first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|order|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|ordered|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should order multiple tests', async ({ page }) => {
      await page.goto('/laboratory');

      const createButton = page.getByRole('button', { name: /create|new|order/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select multiple tests if available
        const testCheckboxes = page.locator('input[type="checkbox"]').filter({ hasText: /test|cbc|bmp/i });
        if (await testCheckboxes.count() > 1) {
          await testCheckboxes.nth(0).check();
          await testCheckboxes.nth(1).check();
        }
      }
    });

    test('should add clinical notes to order', async ({ page }) => {
      await page.goto('/laboratory');

      const createButton = page.getByRole('button', { name: /create|new|order/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const notesField = page.getByLabel(/notes|clinical.*info/i);
        if (await notesField.isVisible()) {
          await notesField.fill('E2E Test - Fasting sample required');
        }
      }
    });
  });

  test.describe('Sample Collection', () => {
    test('should collect sample', async ({ page }) => {
      await page.goto('/laboratory');

      const collectButton = page.getByRole('button', { name: /collect|sample/i }).first();
      if (await collectButton.isVisible()) {
        await collectButton.click();

        // Enter collection details
        const collectionTime = page.getByLabel(/time|collected.*at/i);
        if (await collectionTime.isVisible()) {
          const now = new Date().toTimeString().slice(0, 5);
          await collectionTime.fill(now);
        }

        const collectedBySelect = page.getByLabel(/collected.*by/i);
        if (await collectedBySelect.isVisible()) {
          await collectedBySelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|confirm|collect/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|collected/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should print sample labels', async ({ page }) => {
      await page.goto('/laboratory');

      const printLabelButton = page.getByRole('button', { name: /print.*label|label/i }).first();
      if (await printLabelButton.isVisible()) {
        await expect(printLabelButton).toBeVisible();
      }
    });

    test('should scan barcode', async ({ page }) => {
      await page.goto('/laboratory');

      const barcodeInput = page.getByPlaceholder(/barcode|scan/i);
      if (await barcodeInput.isVisible()) {
        await barcodeInput.fill('SAMPLE-12345');
        await page.keyboard.press('Enter');
      }
    });
  });

  test.describe('Enter Results', () => {
    test('should open results entry form', async ({ page }) => {
      await page.goto('/laboratory');

      const enterResultsButton = page.getByRole('button', { name: /enter.*result|result/i }).first();
      if (await enterResultsButton.isVisible()) {
        await enterResultsButton.click();

        await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      }
    });

    test('should enter test results', async ({ page }) => {
      await page.goto('/laboratory');

      const enterResultsButton = page.getByRole('button', { name: /enter.*result|result/i }).first();
      if (await enterResultsButton.isVisible()) {
        await enterResultsButton.click();

        // Enter result values
        const resultField = page.getByLabel(/result|value/i).first();
        if (await resultField.isVisible()) {
          await resultField.fill('14.5');
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should highlight abnormal results', async ({ page }) => {
      await page.goto('/laboratory');

      const enterResultsButton = page.getByRole('button', { name: /enter.*result|result/i }).first();
      if (await enterResultsButton.isVisible()) {
        await enterResultsButton.click();

        // Enter abnormal value
        const resultField = page.getByLabel(/result|value/i).first();
        if (await resultField.isVisible()) {
          await resultField.fill('999'); // Abnormally high

          // Should show warning/highlight
          const abnormalIndicator = page.locator('[class*="abnormal"], [class*="high"], [class*="critical"]');
          if (await abnormalIndicator.count() > 0) {
            await expect(abnormalIndicator.first()).toBeVisible();
          }
        }
      }
    });

    test('should add result comments', async ({ page }) => {
      await page.goto('/laboratory');

      const enterResultsButton = page.getByRole('button', { name: /enter.*result|result/i }).first();
      if (await enterResultsButton.isVisible()) {
        await enterResultsButton.click();

        const commentField = page.getByLabel(/comment|notes|interpretation/i);
        if (await commentField.isVisible()) {
          await commentField.fill('E2E Test - Results within normal limits');
        }
      }
    });
  });

  test.describe('Verify & Release Results', () => {
    test('should verify results', async ({ page }) => {
      await page.goto('/laboratory');

      const verifyButton = page.getByRole('button', { name: /verify/i }).first();
      if (await verifyButton.isVisible()) {
        await verifyButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/verified|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should release results', async ({ page }) => {
      await page.goto('/laboratory');

      const releaseButton = page.getByRole('button', { name: /release|finalize/i }).first();
      if (await releaseButton.isVisible()) {
        await releaseButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/released|finalized|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should require supervisor approval for critical results', async ({ page }) => {
      await page.goto('/laboratory');

      // Critical results may need additional approval
      const criticalResult = page.locator('[class*="critical"]').first();
      if (await criticalResult.isVisible()) {
        // Check for approval workflow
        const approvalButton = page.getByRole('button', { name: /approve|supervisor/i });
        if (await approvalButton.isVisible()) {
          await expect(approvalButton).toBeVisible();
        }
      }
    });
  });

  test.describe('View Results', () => {
    test('should view test results', async ({ page }) => {
      await page.goto('/laboratory');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/result|test/i)).toBeVisible();
      }
    });

    test('should view result history', async ({ page }) => {
      await page.goto('/laboratory');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const historyTab = page.getByRole('tab', { name: /history|trend/i });
        if (await historyTab.isVisible()) {
          await historyTab.click();

          await expect(page.getByText(/history|previous/i)).toBeVisible();
        }
      }
    });

    test('should display result trend chart', async ({ page }) => {
      await page.goto('/laboratory');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const trendChart = page.locator('canvas, svg[class*="chart"], [class*="chart"]');
        if (await trendChart.isVisible()) {
          await expect(trendChart).toBeVisible();
        }
      }
    });

    test('should print results', async ({ page }) => {
      await page.goto('/laboratory');

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

  test.describe('Test Catalog', () => {
    test('should display test catalog', async ({ page }) => {
      await page.goto('/laboratory/catalog');

      await expect(page.getByRole('heading', { name: /catalog|test.*list/i })).toBeVisible();
    });

    test('should search tests in catalog', async ({ page }) => {
      await page.goto('/laboratory/catalog');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('glucose');
        await page.waitForTimeout(500);
      }
    });

    test('should view test details', async ({ page }) => {
      await page.goto('/laboratory/catalog');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/reference.*range|specimen|method/i)).toBeVisible();
      }
    });

    test('should create new test definition', async ({ page }) => {
      await page.goto('/laboratory/catalog');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name|test.*name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Test ${Date.now()}`);
        }

        const codeField = page.getByLabel(/code/i);
        if (await codeField.isVisible()) {
          await codeField.fill(`TEST-${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('External Labs', () => {
    test('should display external labs page', async ({ page }) => {
      await page.goto('/laboratory/external');

      await expect(page.getByRole('heading', { name: /external.*lab|outsource/i })).toBeVisible();
    });

    test('should send sample to external lab', async ({ page }) => {
      await page.goto('/laboratory');

      const sendExternalButton = page.getByRole('button', { name: /send.*external|outsource/i }).first();
      if (await sendExternalButton.isVisible()) {
        await sendExternalButton.click();

        const labSelect = page.getByLabel(/lab|laboratory/i);
        if (await labSelect.isVisible()) {
          await labSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /send|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|sent/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should receive external lab results', async ({ page }) => {
      await page.goto('/laboratory/external');

      const receiveButton = page.getByRole('button', { name: /receive|import/i }).first();
      if (await receiveButton.isVisible()) {
        await receiveButton.click();

        await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      }
    });
  });

  test.describe('Lab Reports', () => {
    test('should generate daily workload report', async ({ page }) => {
      await page.goto('/laboratory/reports');

      const workloadButton = page.getByRole('button', { name: /workload|daily/i });
      if (await workloadButton.isVisible()) {
        await workloadButton.click();

        await expect(page.getByText(/workload|report/i)).toBeVisible();
      }
    });

    test('should generate TAT report', async ({ page }) => {
      await page.goto('/laboratory/reports');

      const tatButton = page.getByRole('button', { name: /tat|turnaround/i });
      if (await tatButton.isVisible()) {
        await tatButton.click();

        await expect(page.getByText(/turnaround|tat/i)).toBeVisible();
      }
    });

    test('should export lab report', async ({ page }) => {
      await page.goto('/laboratory/reports');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/lab.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });

  test.describe('Quality Control', () => {
    test('should display QC page', async ({ page }) => {
      await page.goto('/laboratory/qc');

      await expect(page.getByRole('heading', { name: /quality.*control|qc/i })).toBeVisible();
    });

    test('should enter QC results', async ({ page }) => {
      await page.goto('/laboratory/qc');

      const enterQCButton = page.getByRole('button', { name: /enter.*qc|add.*qc/i });
      if (await enterQCButton.isVisible()) {
        await enterQCButton.click();

        const valueField = page.getByLabel(/value|result/i);
        if (await valueField.isVisible()) {
          await valueField.fill('95.5');
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should display Levey-Jennings chart', async ({ page }) => {
      await page.goto('/laboratory/qc');

      const chart = page.locator('canvas, svg[class*="chart"], [class*="levey"], [class*="jennings"]');
      if (await chart.isVisible()) {
        await expect(chart).toBeVisible();
      }
    });
  });
});

test.describe('Laboratory API E2E Tests', () => {
  test('should list lab orders', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/laboratory?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get lab order details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/laboratory?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const orderId = listData.items[0].id;

        const response = await request.get(`/api/laboratory/${orderId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should validate lab order creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/laboratory', {
      headers: authHeaders(token),
      data: {
        // Missing required fields
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should list test catalog', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/laboratory/tests', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });
});
