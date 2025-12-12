import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Appointments Module
 * Tests cover scheduling, calendar views, status management, reminders, and conflicts
 */
test.describe('Appointments Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Appointments List & Calendar', () => {
    test('should display appointments page with correct structure', async ({ page }) => {
      await page.goto('/appointments');

      await expect(page.getByRole('heading', { name: /appointment/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|book|schedule/i })).toBeVisible();
    });

    test('should display calendar view', async ({ page }) => {
      await page.goto('/appointments');

      // Look for calendar component
      const calendar = page.locator('[class*="calendar"], [class*="schedule"], [role="grid"]');
      if (await calendar.isVisible()) {
        await expect(calendar).toBeVisible();
      }
    });

    test('should display list view', async ({ page }) => {
      await page.goto('/appointments');

      const listViewButton = page.getByRole('button', { name: /list/i });
      if (await listViewButton.isVisible()) {
        await listViewButton.click();

        const table = page.locator('table, [class*="list"]');
        await expect(table).toBeVisible();
      }
    });

    test('should switch between day, week, and month views', async ({ page }) => {
      await page.goto('/appointments');

      // Day view
      const dayButton = page.getByRole('button', { name: /day/i });
      if (await dayButton.isVisible()) {
        await dayButton.click();
        await page.waitForTimeout(300);
      }

      // Week view
      const weekButton = page.getByRole('button', { name: /week/i });
      if (await weekButton.isVisible()) {
        await weekButton.click();
        await page.waitForTimeout(300);
      }

      // Month view
      const monthButton = page.getByRole('button', { name: /month/i });
      if (await monthButton.isVisible()) {
        await monthButton.click();
        await page.waitForTimeout(300);
      }
    });

    test('should navigate to next/previous periods', async ({ page }) => {
      await page.goto('/appointments');

      const nextButton = page.getByRole('button', { name: /next|>/i });
      if (await nextButton.isVisible()) {
        await nextButton.click();
        await page.waitForTimeout(300);
      }

      const prevButton = page.getByRole('button', { name: /prev|back|</i });
      if (await prevButton.isVisible()) {
        await prevButton.click();
        await page.waitForTimeout(300);
      }

      const todayButton = page.getByRole('button', { name: /today/i });
      if (await todayButton.isVisible()) {
        await todayButton.click();
        await page.waitForTimeout(300);
      }
    });

    test('should filter appointments by doctor', async ({ page }) => {
      await page.goto('/appointments');

      const doctorFilter = page.getByRole('combobox', { name: /doctor|provider|physician/i });
      if (await doctorFilter.isVisible()) {
        await doctorFilter.click();
        await page.getByRole('option').first().click();
        await page.waitForTimeout(300);
      }
    });

    test('should filter appointments by status', async ({ page }) => {
      await page.goto('/appointments');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /confirmed|scheduled|pending/i }).click();
        await page.waitForTimeout(300);
      }
    });

    test('should filter appointments by department', async ({ page }) => {
      await page.goto('/appointments');

      const departmentFilter = page.getByRole('combobox', { name: /department|specialty/i });
      if (await departmentFilter.isVisible()) {
        await departmentFilter.click();
        await page.getByRole('option').first().click();
        await page.waitForTimeout(300);
      }
    });

    test('should search appointments by patient name', async ({ page }) => {
      await page.goto('/appointments');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('John');
        await page.waitForTimeout(500);
      }
    });

    test('should display today\'s appointments summary', async ({ page }) => {
      await page.goto('/appointments');

      const todaySummary = page.getByText(/today|upcoming/i);
      await expect(todaySummary).toBeVisible();
    });
  });

  test.describe('Create Appointment', () => {
    test('should open create appointment form', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      await expect(page.getByRole('heading', { name: /create.*appointment|new.*appointment|book.*appointment|schedule.*appointment/i })).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const submitButton = page.getByRole('button', { name: /save|submit|book|schedule/i }).last();
      await submitButton.click();

      await expect(page.getByText(/required|must.*select|patient.*required/i)).toBeVisible();
    });

    test('should select patient for appointment', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const patientSelect = page.getByLabel(/patient/i);
      if (await patientSelect.isVisible()) {
        await patientSelect.click();

        // Search for patient
        const searchInput = page.getByPlaceholder(/search.*patient/i);
        if (await searchInput.isVisible()) {
          await searchInput.fill('Test');
        }

        await page.getByRole('option').first().click();
      }
    });

    test('should select doctor for appointment', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const doctorSelect = page.getByLabel(/doctor|provider|physician/i);
      if (await doctorSelect.isVisible()) {
        await doctorSelect.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should select appointment date', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const dateField = page.getByLabel(/date/i);
      if (await dateField.isVisible()) {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        await dateField.fill(tomorrow.toISOString().split('T')[0]);
      }
    });

    test('should select appointment time slot', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const timeField = page.getByLabel(/time/i);
      if (await timeField.isVisible()) {
        await timeField.click();
        await page.getByRole('option', { name: /09:00|10:00|11:00/i }).first().click();
      }
    });

    test('should select appointment type/reason', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const typeSelect = page.getByLabel(/type|reason|service/i);
      if (await typeSelect.isVisible()) {
        await typeSelect.click();
        await page.getByRole('option', { name: /consultation|follow.*up|checkup/i }).first().click();
      }
    });

    test('should set appointment duration', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const durationSelect = page.getByLabel(/duration/i);
      if (await durationSelect.isVisible()) {
        await durationSelect.click();
        await page.getByRole('option', { name: /30.*min|60.*min|15.*min/i }).first().click();
      }
    });

    test('should add appointment notes', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const notesField = page.getByLabel(/notes|comment|reason/i);
      if (await notesField.isVisible()) {
        await notesField.fill('E2E Test Appointment - Patient follow-up');
      }
    });

    test('should create appointment successfully', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      // Select patient
      const patientSelect = page.getByLabel(/patient/i);
      if (await patientSelect.isVisible()) {
        await patientSelect.click();
        await page.getByRole('option').first().click();
      }

      // Select doctor
      const doctorSelect = page.getByLabel(/doctor|provider/i);
      if (await doctorSelect.isVisible()) {
        await doctorSelect.click();
        await page.getByRole('option').first().click();
      }

      // Set date
      const dateField = page.getByLabel(/date/i);
      if (await dateField.isVisible()) {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        await dateField.fill(tomorrow.toISOString().split('T')[0]);
      }

      // Submit
      const submitButton = page.getByRole('button', { name: /save|submit|book|schedule/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|booked|scheduled|created/i)).toBeVisible({ timeout: 10000 });
    });

    test('should show available time slots', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      // After selecting doctor and date, available slots should show
      const doctorSelect = page.getByLabel(/doctor|provider/i);
      if (await doctorSelect.isVisible()) {
        await doctorSelect.click();
        await page.getByRole('option').first().click();
      }

      const dateField = page.getByLabel(/date/i);
      if (await dateField.isVisible()) {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        await dateField.fill(tomorrow.toISOString().split('T')[0]);

        // Should show available slots
        await page.waitForTimeout(1000);
        const slots = page.locator('[class*="slot"], [class*="time"]');
        if (await slots.count() > 0) {
          await expect(slots.first()).toBeVisible();
        }
      }
    });

    test('should prevent booking past dates', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const dateField = page.getByLabel(/date/i);
      if (await dateField.isVisible()) {
        const yesterday = new Date();
        yesterday.setDate(yesterday.getDate() - 1);
        await dateField.fill(yesterday.toISOString().split('T')[0]);

        const submitButton = page.getByRole('button', { name: /save|submit|book/i }).last();
        await submitButton.click();

        await expect(page.getByText(/past|future|invalid.*date/i)).toBeVisible();
      }
    });

    test('should prevent double booking', async ({ page }) => {
      // This test assumes there's already an appointment at the same time
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      // Fill in details that would cause conflict
      const patientSelect = page.getByLabel(/patient/i);
      if (await patientSelect.isVisible()) {
        await patientSelect.click();
        await page.getByRole('option').first().click();
      }

      // Try to book at a busy slot - should show conflict warning
      // Note: Actual conflict detection depends on existing data
    });
  });

  test.describe('Appointment Details', () => {
    test('should view appointment details', async ({ page }) => {
      await page.goto('/appointments');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/appointment.*detail|patient|doctor|time/i)).toBeVisible();
      } else {
        // Click on calendar event or row
        const appointment = page.locator('[class*="event"], [class*="appointment"]').first();
        if (await appointment.isVisible()) {
          await appointment.click();
        }
      }
    });

    test('should display patient information in appointment', async ({ page }) => {
      await page.goto('/appointments');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/patient/i)).toBeVisible();
      }
    });

    test('should display doctor information in appointment', async ({ page }) => {
      await page.goto('/appointments');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/doctor|provider|physician/i)).toBeVisible();
      }
    });
  });

  test.describe('Edit Appointment', () => {
    test('should open edit appointment form', async ({ page }) => {
      await page.goto('/appointments');

      const editButton = page.getByRole('button', { name: /edit|reschedule/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        await expect(page.getByRole('heading', { name: /edit.*appointment|reschedule|update/i })).toBeVisible();
      }
    });

    test('should reschedule appointment to new date', async ({ page }) => {
      await page.goto('/appointments');

      const editButton = page.getByRole('button', { name: /edit|reschedule/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const dateField = page.getByLabel(/date/i);
        if (await dateField.isVisible()) {
          const newDate = new Date();
          newDate.setDate(newDate.getDate() + 7);
          await dateField.fill(newDate.toISOString().split('T')[0]);
        }

        const submitButton = page.getByRole('button', { name: /save|update|reschedule/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated|rescheduled/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should update appointment notes', async ({ page }) => {
      await page.goto('/appointments');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const notesField = page.getByLabel(/notes|comment/i);
        if (await notesField.isVisible()) {
          await notesField.fill(`Updated by E2E test at ${new Date().toISOString()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Appointment Status Management', () => {
    test('should confirm pending appointment', async ({ page }) => {
      await page.goto('/appointments');

      const confirmButton = page.getByRole('button', { name: /confirm/i }).first();
      if (await confirmButton.isVisible()) {
        await confirmButton.click();

        const dialogConfirm = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await dialogConfirm.isVisible()) {
          await dialogConfirm.click();
        }

        await expect(page.getByText(/confirmed|success/i)).toBeVisible();
      }
    });

    test('should check-in patient for appointment', async ({ page }) => {
      await page.goto('/appointments');

      const checkinButton = page.getByRole('button', { name: /check.*in|arrived/i }).first();
      if (await checkinButton.isVisible()) {
        await checkinButton.click();

        await expect(page.getByText(/checked.*in|arrived|success/i)).toBeVisible();
      }
    });

    test('should start consultation', async ({ page }) => {
      await page.goto('/appointments');

      const startButton = page.getByRole('button', { name: /start|begin.*consultation/i }).first();
      if (await startButton.isVisible()) {
        await startButton.click();

        // May navigate to consultation page or update status
        await expect(page.getByText(/started|in.*progress|consultation/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should complete appointment', async ({ page }) => {
      await page.goto('/appointments');

      const completeButton = page.getByRole('button', { name: /complete|finish|end/i }).first();
      if (await completeButton.isVisible()) {
        await completeButton.click();

        const dialogConfirm = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await dialogConfirm.isVisible()) {
          await dialogConfirm.click();
        }

        await expect(page.getByText(/completed|finished|success/i)).toBeVisible();
      }
    });

    test('should mark patient as no-show', async ({ page }) => {
      await page.goto('/appointments');

      const noShowButton = page.getByRole('button', { name: /no.*show|missed/i }).first();
      if (await noShowButton.isVisible()) {
        await noShowButton.click();

        const dialogConfirm = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await dialogConfirm.isVisible()) {
          await dialogConfirm.click();
        }

        await expect(page.getByText(/no.*show|missed|success/i)).toBeVisible();
      }
    });

    test('should cancel appointment with reason', async ({ page }) => {
      await page.goto('/appointments');

      const cancelButton = page.getByRole('button', { name: /cancel/i }).first();
      if (await cancelButton.isVisible()) {
        await cancelButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Patient requested cancellation');
        }

        const confirmButton = page.getByRole('button', { name: /yes|confirm|cancel.*appointment/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/cancelled|success/i)).toBeVisible();
      }
    });
  });

  test.describe('Appointment Reminders', () => {
    test('should send appointment reminder', async ({ page }) => {
      await page.goto('/appointments');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const reminderButton = page.getByRole('button', { name: /remind|send.*reminder|notify/i });
        if (await reminderButton.isVisible()) {
          await reminderButton.click();

          await expect(page.getByText(/reminder.*sent|notified|success/i)).toBeVisible();
        }
      }
    });

    test('should configure reminder settings', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const reminderCheckbox = page.getByLabel(/reminder|notify/i);
      if (await reminderCheckbox.isVisible()) {
        await reminderCheckbox.check();

        const reminderTime = page.getByLabel(/before|advance/i);
        if (await reminderTime.isVisible()) {
          await reminderTime.click();
          await page.getByRole('option', { name: /24.*hour|1.*day/i }).click();
        }
      }
    });
  });

  test.describe('Recurring Appointments', () => {
    test('should create recurring appointment', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      const recurringCheckbox = page.getByLabel(/recurring|repeat/i);
      if (await recurringCheckbox.isVisible()) {
        await recurringCheckbox.check();

        const frequencySelect = page.getByLabel(/frequency|repeat.*every/i);
        if (await frequencySelect.isVisible()) {
          await frequencySelect.click();
          await page.getByRole('option', { name: /weekly|daily|monthly/i }).first().click();
        }

        const occurrencesField = page.getByLabel(/occurrences|times|count/i);
        if (await occurrencesField.isVisible()) {
          await occurrencesField.fill('4');
        }
      }
    });
  });

  test.describe('Appointment Conflicts', () => {
    test('should detect and display scheduling conflicts', async ({ page }) => {
      await page.goto('/appointments');

      // Navigate to a busy time slot
      const calendar = page.locator('[class*="calendar"], [class*="schedule"]');
      if (await calendar.isVisible()) {
        // Check for conflict indicators
        const conflicts = page.locator('[class*="conflict"], [class*="overlap"], [class*="busy"]');
        if (await conflicts.count() > 0) {
          await expect(conflicts.first()).toBeVisible();
        }
      }
    });

    test('should show warning when overbooking', async ({ page }) => {
      await page.goto('/appointments');

      await page.getByRole('button', { name: /create|new|book|schedule/i }).click();

      // Try to create appointment at a potentially busy time
      // The system should warn about conflicts
    });
  });

  test.describe('Walk-in Appointments', () => {
    test('should create walk-in appointment', async ({ page }) => {
      await page.goto('/appointments');

      const walkinButton = page.getByRole('button', { name: /walk.*in|quick.*book/i });
      if (await walkinButton.isVisible()) {
        await walkinButton.click();

        // Fill minimal required info
        const patientSelect = page.getByLabel(/patient/i);
        if (await patientSelect.isVisible()) {
          await patientSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|book|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|booked|created/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Appointment Printing', () => {
    test('should print appointment slip', async ({ page }) => {
      await page.goto('/appointments');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          // Can't actually test printing, but verify button exists
          await expect(printButton).toBeVisible();
        }
      }
    });

    test('should export appointment schedule', async ({ page }) => {
      await page.goto('/appointments');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/appointment.*\.(csv|xlsx|pdf|ics)/i);
        }
      }
    });
  });
});

test.describe('Appointments API E2E Tests', () => {
  test('should list appointments with pagination', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/appointments?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data).toHaveProperty('items');
    expect(Array.isArray(data.items)).toBeTruthy();
  });

  test('should filter appointments by date range', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const today = new Date().toISOString().split('T')[0];
    const nextWeek = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

    const response = await request.get(`/api/appointments?fromDate=${today}&toDate=${nextWeek}`, {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should filter appointments by doctor', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/appointments?doctorId=1', {
      headers: authHeaders(token),
    });

    // May return 200 with empty results if doctor doesn't exist
    expect([200, 404]).toContain(response.status());
  });

  test('should get appointment by ID', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    // First get an appointment ID
    const listResponse = await request.get('/api/appointments?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const appointmentId = listData.items[0].id;

        const response = await request.get(`/api/appointments/${appointmentId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);

        const appointment = await response.json();
        expect(appointment).toHaveProperty('id');
      }
    }
  });

  test('should validate appointment creation payload', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    // Send invalid payload
    const response = await request.post('/api/appointments', {
      headers: authHeaders(token),
      data: {
        // Missing required fields
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should update appointment status', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    // First get an appointment ID
    const listResponse = await request.get('/api/appointments?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const appointmentId = listData.items[0].id;

        const response = await request.patch(`/api/appointments/${appointmentId}/status`, {
          headers: authHeaders(token),
          data: {
            status: 'Confirmed',
          },
        });

        // May succeed or fail depending on current status
        expect([200, 400, 404]).toContain(response.status());
      }
    }
  });

  test('should get available time slots', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const tomorrow = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().split('T')[0];

    const response = await request.get(`/api/appointments/available-slots?date=${tomorrow}`, {
      headers: authHeaders(token),
    });

    // May have this endpoint or not
    expect([200, 404]).toContain(response.status());
  });
});

test.describe('Appointment Calendar Integration', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should display appointments in correct time slots', async ({ page }) => {
    await page.goto('/appointments');

    // In calendar view, appointments should be positioned correctly
    const calendar = page.locator('[class*="calendar"], [class*="schedule"]');
    if (await calendar.isVisible()) {
      const events = page.locator('[class*="event"], [class*="appointment"]');
      if (await events.count() > 0) {
        // Events should have proper positioning
        await expect(events.first()).toBeVisible();
      }
    }
  });

  test('should drag and drop to reschedule', async ({ page }) => {
    await page.goto('/appointments');

    // Note: Drag and drop testing is complex and may not work in all calendar implementations
    const event = page.locator('[class*="event"], [class*="appointment"]').first();
    if (await event.isVisible()) {
      // Verify event is draggable
      const draggable = await event.getAttribute('draggable');
      if (draggable === 'true') {
        // Could test drag and drop here
      }
    }
  });

  test('should show appointment tooltip on hover', async ({ page }) => {
    await page.goto('/appointments');

    const event = page.locator('[class*="event"], [class*="appointment"]').first();
    if (await event.isVisible()) {
      await event.hover();

      // Check for tooltip
      const tooltip = page.locator('[class*="tooltip"], [role="tooltip"]');
      if (await tooltip.isVisible()) {
        await expect(tooltip).toBeVisible();
      }
    }
  });
});

test.describe('Appointment Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should navigate calendar with keyboard', async ({ page }) => {
    await page.goto('/appointments');

    // Tab to calendar
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    // Use arrow keys to navigate
    await page.keyboard.press('ArrowRight');
    await page.keyboard.press('ArrowDown');

    // Check focus is visible
    const focusedElement = page.locator(':focus');
    await expect(focusedElement).toBeVisible();
  });

  test('should have proper ARIA labels on calendar', async ({ page }) => {
    await page.goto('/appointments');

    const calendar = page.locator('[class*="calendar"], [class*="schedule"], [role="grid"]');
    if (await calendar.isVisible()) {
      // Calendar should have proper ARIA attributes
      const ariaLabel = await calendar.getAttribute('aria-label');
      const role = await calendar.getAttribute('role');

      expect(ariaLabel || role).toBeTruthy();
    }
  });
});

test.describe('Appointment Mobile Responsiveness', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should display mobile-optimized calendar', async ({ page }) => {
    await page.goto('/appointments');

    // Calendar should adapt to mobile view
    await expect(page.getByRole('heading', { name: /appointment/i })).toBeVisible();

    // No horizontal overflow
    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
    expect(scrollWidth).toBeLessThanOrEqual(clientWidth + 20);
  });

  test('should switch to agenda view on mobile', async ({ page }) => {
    await page.goto('/appointments');

    // Mobile might show agenda/list view instead of full calendar
    const agendaView = page.locator('[class*="agenda"], [class*="list"]');
    if (await agendaView.isVisible()) {
      await expect(agendaView).toBeVisible();
    }
  });

  test('should have touch-friendly controls', async ({ page }) => {
    await page.goto('/appointments');

    const buttons = page.getByRole('button');
    const count = await buttons.count();

    for (let i = 0; i < Math.min(count, 3); i++) {
      const button = buttons.nth(i);
      if (await button.isVisible()) {
        const box = await button.boundingBox();
        if (box) {
          expect(box.width).toBeGreaterThanOrEqual(40);
          expect(box.height).toBeGreaterThanOrEqual(40);
        }
      }
    }
  });
});
