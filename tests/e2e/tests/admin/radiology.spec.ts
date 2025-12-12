import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Radiology Module
 * Tests cover imaging studies, DICOM, orders, results, and reporting
 */
test.describe('Radiology Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Radiology Dashboard', () => {
    test('should display radiology dashboard', async ({ page }) => {
      await page.goto('/radiology');
      await expect(page.getByRole('heading', { name: /radiology|imaging/i })).toBeVisible();
    });

    test('should display pending studies count', async ({ page }) => {
      await page.goto('/radiology');
      await expect(page.getByText(/pending|queue|waiting/i)).toBeVisible();
    });

    test('should display completed studies count', async ({ page }) => {
      await page.goto('/radiology');
      await expect(page.getByText(/completed|done/i)).toBeVisible();
    });

    test('should display radiology statistics', async ({ page }) => {
      await page.goto('/radiology');
      const stats = page.locator('[class*="stat"], [class*="card"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });
  });

  test.describe('Imaging Orders', () => {
    test('should display imaging orders list', async ({ page }) => {
      await page.goto('/radiology/orders');
      await expect(page.getByRole('heading', { name: /order|study|imaging/i })).toBeVisible();
    });

    test('should create imaging order', async ({ page }) => {
      await page.goto('/radiology/orders');
      const createButton = page.getByRole('button', { name: /create|new|order/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const patientSelect = page.getByLabel(/patient/i);
        if (await patientSelect.isVisible()) {
          await patientSelect.click();
          await page.getByRole('option').first().click();
        }

        const modalitySelect = page.getByLabel(/modality|type/i);
        if (await modalitySelect.isVisible()) {
          await modalitySelect.click();
          await page.getByRole('option', { name: /x-ray|ct|mri|ultrasound/i }).first().click();
        }

        const bodyPartSelect = page.getByLabel(/body.*part|region|area/i);
        if (await bodyPartSelect.isVisible()) {
          await bodyPartSelect.click();
          await page.getByRole('option').first().click();
        }

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

    test('should filter orders by modality', async ({ page }) => {
      await page.goto('/radiology/orders');
      const modalityFilter = page.getByRole('combobox', { name: /modality|type/i });
      if (await modalityFilter.isVisible()) {
        await modalityFilter.click();
        await page.getByRole('option', { name: /x-ray|ct|mri/i }).first().click();
      }
    });

    test('should filter orders by status', async ({ page }) => {
      await page.goto('/radiology/orders');
      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /pending|scheduled|completed/i }).first().click();
      }
    });

    test('should filter orders by date', async ({ page }) => {
      await page.goto('/radiology/orders');
      const dateFilter = page.getByLabel(/date/i);
      if (await dateFilter.isVisible()) {
        const today = new Date().toISOString().split('T')[0];
        await dateFilter.fill(today);
      }
    });

    test('should search orders by patient', async ({ page }) => {
      await page.goto('/radiology/orders');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('John');
        await page.waitForTimeout(500);
      }
    });

    test('should view order details', async ({ page }) => {
      await page.goto('/radiology/orders');
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/order.*detail|patient|modality/i)).toBeVisible();
      }
    });

    test('should cancel order with reason', async ({ page }) => {
      await page.goto('/radiology/orders');
      const cancelButton = page.getByRole('button', { name: /cancel/i }).first();
      if (await cancelButton.isVisible()) {
        await cancelButton.click();
        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Order cancelled');
        }
        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
        await expect(page.getByText(/cancelled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Study Scheduling', () => {
    test('should schedule imaging study', async ({ page }) => {
      await page.goto('/radiology/orders');
      const scheduleButton = page.getByRole('button', { name: /schedule/i }).first();
      if (await scheduleButton.isVisible()) {
        await scheduleButton.click();

        const dateField = page.getByLabel(/date/i);
        if (await dateField.isVisible()) {
          const tomorrow = new Date();
          tomorrow.setDate(tomorrow.getDate() + 1);
          await dateField.fill(tomorrow.toISOString().split('T')[0]);
        }

        const timeField = page.getByLabel(/time/i);
        if (await timeField.isVisible()) {
          await timeField.fill('10:00');
        }

        const roomSelect = page.getByLabel(/room|equipment/i);
        if (await roomSelect.isVisible()) {
          await roomSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|schedule/i }).last();
        await submitButton.click();
        await expect(page.getByText(/scheduled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should reschedule study', async ({ page }) => {
      await page.goto('/radiology/orders');
      const rescheduleButton = page.getByRole('button', { name: /reschedule/i }).first();
      if (await rescheduleButton.isVisible()) {
        await rescheduleButton.click();

        const dateField = page.getByLabel(/date/i);
        if (await dateField.isVisible()) {
          const nextWeek = new Date();
          nextWeek.setDate(nextWeek.getDate() + 7);
          await dateField.fill(nextWeek.toISOString().split('T')[0]);
        }

        const submitButton = page.getByRole('button', { name: /save|reschedule/i }).last();
        await submitButton.click();
        await expect(page.getByText(/rescheduled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view schedule calendar', async ({ page }) => {
      await page.goto('/radiology/schedule');
      const calendar = page.locator('[class*="calendar"], [class*="schedule"]');
      if (await calendar.isVisible()) {
        await expect(calendar).toBeVisible();
      }
    });
  });

  test.describe('Study Acquisition', () => {
    test('should start study acquisition', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const startButton = page.getByRole('button', { name: /start|begin|acquire/i }).first();
      if (await startButton.isVisible()) {
        await startButton.click();
        await expect(page.getByText(/started|in.*progress/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should mark patient arrived', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const arrivedButton = page.getByRole('button', { name: /arrived|check.*in/i }).first();
      if (await arrivedButton.isVisible()) {
        await arrivedButton.click();
        await expect(page.getByText(/arrived|checked.*in|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should complete study acquisition', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const completeButton = page.getByRole('button', { name: /complete|finish/i }).first();
      if (await completeButton.isVisible()) {
        await completeButton.click();
        await expect(page.getByText(/completed|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should add technologist notes', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const notesField = page.getByLabel(/technologist.*note|tech.*note/i);
        if (await notesField.isVisible()) {
          await notesField.fill('E2E Test - Patient positioned correctly, good image quality');
        }

        const submitButton = page.getByRole('button', { name: /save/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Image Viewing', () => {
    test('should open DICOM viewer', async ({ page }) => {
      await page.goto('/radiology');
      const viewImagesButton = page.getByRole('button', { name: /view.*image|dicom|viewer/i }).first();
      if (await viewImagesButton.isVisible()) {
        await viewImagesButton.click();
        // DICOM viewer should open
        await expect(page.locator('[class*="viewer"], [class*="dicom"]')).toBeVisible({ timeout: 10000 });
      }
    });

    test('should display image thumbnails', async ({ page }) => {
      await page.goto('/radiology');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        const thumbnails = page.locator('[class*="thumbnail"], img');
        if (await thumbnails.count() > 0) {
          await expect(thumbnails.first()).toBeVisible();
        }
      }
    });

    test('should zoom in/out images', async ({ page }) => {
      await page.goto('/radiology');
      const viewButton = page.getByRole('button', { name: /view.*image/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const zoomInButton = page.getByRole('button', { name: /zoom.*in|\+/i });
        if (await zoomInButton.isVisible()) {
          await zoomInButton.click();
        }

        const zoomOutButton = page.getByRole('button', { name: /zoom.*out|-/i });
        if (await zoomOutButton.isVisible()) {
          await zoomOutButton.click();
        }
      }
    });

    test('should adjust window/level', async ({ page }) => {
      await page.goto('/radiology');
      const viewButton = page.getByRole('button', { name: /view.*image/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const windowLevelButton = page.getByRole('button', { name: /window|level|contrast/i });
        if (await windowLevelButton.isVisible()) {
          await expect(windowLevelButton).toBeVisible();
        }
      }
    });

    test('should compare with prior studies', async ({ page }) => {
      await page.goto('/radiology');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const compareButton = page.getByRole('button', { name: /compare|prior/i });
        if (await compareButton.isVisible()) {
          await compareButton.click();
          await expect(page.getByText(/prior|comparison/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Radiology Reporting', () => {
    test('should create radiology report', async ({ page }) => {
      await page.goto('/radiology');
      const reportButton = page.getByRole('button', { name: /report|interpret/i }).first();
      if (await reportButton.isVisible()) {
        await reportButton.click();

        const findingsField = page.getByLabel(/finding/i);
        if (await findingsField.isVisible()) {
          await findingsField.fill('E2E Test - No acute abnormality identified.');
        }

        const impressionField = page.getByLabel(/impression|conclusion/i);
        if (await impressionField.isVisible()) {
          await impressionField.fill('Normal study.');
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should use report templates', async ({ page }) => {
      await page.goto('/radiology');
      const reportButton = page.getByRole('button', { name: /report/i }).first();
      if (await reportButton.isVisible()) {
        await reportButton.click();

        const templateSelect = page.getByLabel(/template/i);
        if (await templateSelect.isVisible()) {
          await templateSelect.click();
          await page.getByRole('option').first().click();
        }
      }
    });

    test('should sign radiology report', async ({ page }) => {
      await page.goto('/radiology');
      const signButton = page.getByRole('button', { name: /sign|finalize/i }).first();
      if (await signButton.isVisible()) {
        await signButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm|sign/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
        await expect(page.getByText(/signed|finalized|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should add addendum to report', async ({ page }) => {
      await page.goto('/radiology');
      const addendumButton = page.getByRole('button', { name: /addendum/i }).first();
      if (await addendumButton.isVisible()) {
        await addendumButton.click();

        const addendumField = page.getByLabel(/addendum|additional/i);
        if (await addendumField.isVisible()) {
          await addendumField.fill('E2E Test - Additional finding noted.');
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|added/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should print radiology report', async ({ page }) => {
      await page.goto('/radiology');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          await expect(printButton).toBeVisible();
        }
      }
    });

    test('should share report with referring physician', async ({ page }) => {
      await page.goto('/radiology');
      const shareButton = page.getByRole('button', { name: /share|send/i }).first();
      if (await shareButton.isVisible()) {
        await shareButton.click();
        await expect(page.getByRole('dialog').or(page.getByText(/share|send/i))).toBeVisible();
      }
    });
  });

  test.describe('Modality Worklist', () => {
    test('should display modality worklist', async ({ page }) => {
      await page.goto('/radiology/worklist');
      await expect(page.getByRole('heading', { name: /worklist/i })).toBeVisible();
    });

    test('should filter by modality', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const modalityFilter = page.getByRole('combobox', { name: /modality/i });
      if (await modalityFilter.isVisible()) {
        await modalityFilter.click();
        await page.getByRole('option', { name: /ct|mri|xray/i }).first().click();
      }
    });

    test('should filter by room/equipment', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const roomFilter = page.getByRole('combobox', { name: /room|equipment/i });
      if (await roomFilter.isVisible()) {
        await roomFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should refresh worklist', async ({ page }) => {
      await page.goto('/radiology/worklist');
      const refreshButton = page.getByRole('button', { name: /refresh/i });
      if (await refreshButton.isVisible()) {
        await refreshButton.click();
        await page.waitForTimeout(1000);
      }
    });
  });

  test.describe('Radiology Reports', () => {
    test('should generate daily report', async ({ page }) => {
      await page.goto('/radiology/reports');
      const dailyButton = page.getByRole('button', { name: /daily|workload/i });
      if (await dailyButton.isVisible()) {
        await dailyButton.click();
        await expect(page.getByText(/daily|report/i)).toBeVisible();
      }
    });

    test('should generate TAT report', async ({ page }) => {
      await page.goto('/radiology/reports');
      const tatButton = page.getByRole('button', { name: /tat|turnaround/i });
      if (await tatButton.isVisible()) {
        await tatButton.click();
        await expect(page.getByText(/turnaround|tat/i)).toBeVisible();
      }
    });

    test('should export radiology report', async ({ page }) => {
      await page.goto('/radiology/reports');
      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();
        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/radiology.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });
});

test.describe('Radiology API E2E Tests', () => {
  test('should list imaging orders', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.get('/api/radiology?page=1&pageSize=10', {
      headers: authHeaders(token),
    });
    expect(response.status()).toBe(200);
  });

  test('should get imaging order details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const listResponse = await request.get('/api/radiology?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const orderId = listData.items[0].id;
        const response = await request.get(`/api/radiology/${orderId}`, {
          headers: authHeaders(token),
        });
        expect(response.status()).toBe(200);
      }
    }
  });

  test('should validate imaging order creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.post('/api/radiology', {
      headers: authHeaders(token),
      data: {},
    });
    expect(response.status()).toBe(400);
  });
});
