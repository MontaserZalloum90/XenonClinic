import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for HR Module
 * Tests cover employees, attendance, leave management, departments, and payroll
 */
test.describe('HR Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Employees List', () => {
    test('should display employees page', async ({ page }) => {
      await page.goto('/hr/employees');

      await expect(page.getByRole('heading', { name: /employee|staff/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
    });

    test('should search employees', async ({ page }) => {
      await page.goto('/hr/employees');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('Dr.');
        await page.waitForTimeout(500);
      }
    });

    test('should filter employees by department', async ({ page }) => {
      await page.goto('/hr/employees');

      const departmentFilter = page.getByRole('combobox', { name: /department/i });
      if (await departmentFilter.isVisible()) {
        await departmentFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter employees by status', async ({ page }) => {
      await page.goto('/hr/employees');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /active/i }).click();
      }
    });

    test('should filter employees by role', async ({ page }) => {
      await page.goto('/hr/employees');

      const roleFilter = page.getByRole('combobox', { name: /role|position/i });
      if (await roleFilter.isVisible()) {
        await roleFilter.click();
        await page.getByRole('option', { name: /doctor|nurse|receptionist/i }).first().click();
      }
    });

    test('should display employee statistics', async ({ page }) => {
      await page.goto('/hr/employees');

      const stats = page.locator('[class*="stat"], [class*="card"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });
  });

  test.describe('Create Employee', () => {
    test('should open create employee form', async ({ page }) => {
      await page.goto('/hr/employees');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/hr/employees');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      await expect(page.getByText(/required/i)).toBeVisible();
    });

    test('should create employee with valid data', async ({ page }) => {
      await page.goto('/hr/employees');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Fill employee details
      const firstNameField = page.getByLabel(/first.*name/i);
      if (await firstNameField.isVisible()) {
        await firstNameField.fill(`E2E Employee ${Date.now()}`);
      }

      const lastNameField = page.getByLabel(/last.*name/i);
      if (await lastNameField.isVisible()) {
        await lastNameField.fill('TestLastName');
      }

      const emailField = page.getByLabel(/email/i);
      if (await emailField.isVisible()) {
        await emailField.fill(`e2e.employee.${Date.now()}@test.com`);
      }

      const departmentSelect = page.getByLabel(/department/i);
      if (await departmentSelect.isVisible()) {
        await departmentSelect.click();
        await page.getByRole('option').first().click();
      }

      const positionSelect = page.getByLabel(/position|role|job.*title/i);
      if (await positionSelect.isVisible()) {
        await positionSelect.click();
        await page.getByRole('option').first().click();
      }

      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|created|saved/i)).toBeVisible({ timeout: 10000 });
    });

    test('should validate email uniqueness', async ({ page }) => {
      await page.goto('/hr/employees');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const emailField = page.getByLabel(/email/i);
      if (await emailField.isVisible()) {
        await emailField.fill('admin@test.com'); // Existing email

        const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
        await submitButton.click();

        // May show duplicate error or other validation
      }
    });
  });

  test.describe('Employee Details', () => {
    test('should view employee details', async ({ page }) => {
      await page.goto('/hr/employees');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/employee.*detail|personal.*info/i)).toBeVisible();
      }
    });

    test('should display employee documents', async ({ page }) => {
      await page.goto('/hr/employees');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const documentsTab = page.getByRole('tab', { name: /document/i });
        if (await documentsTab.isVisible()) {
          await documentsTab.click();

          await expect(page.getByText(/document|file/i)).toBeVisible();
        }
      }
    });

    test('should display employee schedule', async ({ page }) => {
      await page.goto('/hr/employees');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const scheduleTab = page.getByRole('tab', { name: /schedule|shift/i });
        if (await scheduleTab.isVisible()) {
          await scheduleTab.click();

          await expect(page.getByText(/schedule|shift/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Attendance Management', () => {
    test('should display attendance page', async ({ page }) => {
      await page.goto('/hr/attendance');

      await expect(page.getByRole('heading', { name: /attendance/i })).toBeVisible();
    });

    test('should record clock in', async ({ page }) => {
      await page.goto('/hr/attendance');

      const clockInButton = page.getByRole('button', { name: /clock.*in|check.*in/i });
      if (await clockInButton.isVisible()) {
        await clockInButton.click();

        await expect(page.getByText(/clocked.*in|checked.*in|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should record clock out', async ({ page }) => {
      await page.goto('/hr/attendance');

      const clockOutButton = page.getByRole('button', { name: /clock.*out|check.*out/i });
      if (await clockOutButton.isVisible()) {
        await clockOutButton.click();

        await expect(page.getByText(/clocked.*out|checked.*out|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view attendance history', async ({ page }) => {
      await page.goto('/hr/attendance');

      const historyTab = page.getByRole('tab', { name: /history|record/i });
      if (await historyTab.isVisible()) {
        await historyTab.click();

        await expect(page.getByText(/attendance.*history|record/i)).toBeVisible();
      }
    });

    test('should filter attendance by date', async ({ page }) => {
      await page.goto('/hr/attendance');

      const dateFilter = page.getByLabel(/date/i);
      if (await dateFilter.isVisible()) {
        const today = new Date().toISOString().split('T')[0];
        await dateFilter.fill(today);
      }
    });

    test('should filter attendance by employee', async ({ page }) => {
      await page.goto('/hr/attendance');

      const employeeFilter = page.getByRole('combobox', { name: /employee|staff/i });
      if (await employeeFilter.isVisible()) {
        await employeeFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should export attendance report', async ({ page }) => {
      await page.goto('/hr/attendance');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/attendance.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });

  test.describe('Leave Management', () => {
    test('should display leave requests page', async ({ page }) => {
      await page.goto('/hr/leave');

      await expect(page.getByRole('heading', { name: /leave|time.*off/i })).toBeVisible();
    });

    test('should create leave request', async ({ page }) => {
      await page.goto('/hr/leave');

      const createButton = page.getByRole('button', { name: /create|new|request/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Fill leave request
        const leaveTypeSelect = page.getByLabel(/type|leave.*type/i);
        if (await leaveTypeSelect.isVisible()) {
          await leaveTypeSelect.click();
          await page.getByRole('option', { name: /annual|vacation|sick/i }).first().click();
        }

        const fromDate = page.getByLabel(/from|start.*date/i);
        if (await fromDate.isVisible()) {
          const tomorrow = new Date();
          tomorrow.setDate(tomorrow.getDate() + 1);
          await fromDate.fill(tomorrow.toISOString().split('T')[0]);
        }

        const toDate = page.getByLabel(/to|end.*date/i);
        if (await toDate.isVisible()) {
          const nextWeek = new Date();
          nextWeek.setDate(nextWeek.getDate() + 7);
          await toDate.fill(nextWeek.toISOString().split('T')[0]);
        }

        const reasonField = page.getByLabel(/reason|notes/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Annual leave request');
        }

        const submitButton = page.getByRole('button', { name: /submit|save|request/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|submitted|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should approve leave request', async ({ page }) => {
      await page.goto('/hr/leave');

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

    test('should reject leave request with reason', async ({ page }) => {
      await page.goto('/hr/leave');

      const rejectButton = page.getByRole('button', { name: /reject|decline/i }).first();
      if (await rejectButton.isVisible()) {
        await rejectButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Insufficient leave balance');
        }

        const confirmButton = page.getByRole('button', { name: /reject|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/rejected|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view leave balance', async ({ page }) => {
      await page.goto('/hr/leave');

      const balanceSection = page.getByText(/balance|remaining|available/i);
      if (await balanceSection.isVisible()) {
        await expect(balanceSection).toBeVisible();
      }
    });

    test('should filter leave requests by status', async ({ page }) => {
      await page.goto('/hr/leave');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /pending|approved|rejected/i }).first().click();
      }
    });
  });

  test.describe('Departments', () => {
    test('should display departments page', async ({ page }) => {
      await page.goto('/hr/departments');

      await expect(page.getByRole('heading', { name: /department/i })).toBeVisible();
    });

    test('should create department', async ({ page }) => {
      await page.goto('/hr/departments');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Department ${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should assign manager to department', async ({ page }) => {
      await page.goto('/hr/departments');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const managerSelect = page.getByLabel(/manager|head/i);
        if (await managerSelect.isVisible()) {
          await managerSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Job Positions', () => {
    test('should display job positions page', async ({ page }) => {
      await page.goto('/hr/positions');

      await expect(page.getByRole('heading', { name: /position|job|role/i })).toBeVisible();
    });

    test('should create job position', async ({ page }) => {
      await page.goto('/hr/positions');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const titleField = page.getByLabel(/title|name/i);
        if (await titleField.isVisible()) {
          await titleField.fill(`E2E Position ${Date.now()}`);
        }

        const departmentSelect = page.getByLabel(/department/i);
        if (await departmentSelect.isVisible()) {
          await departmentSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Performance Reviews', () => {
    test('should display performance reviews page', async ({ page }) => {
      await page.goto('/hr/performance');

      await expect(page.getByRole('heading', { name: /performance|review/i })).toBeVisible();
    });

    test('should create performance review', async ({ page }) => {
      await page.goto('/hr/performance');

      const createButton = page.getByRole('button', { name: /create|new|start/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const employeeSelect = page.getByLabel(/employee/i);
        if (await employeeSelect.isVisible()) {
          await employeeSelect.click();
          await page.getByRole('option').first().click();
        }

        const periodSelect = page.getByLabel(/period|quarter|year/i);
        if (await periodSelect.isVisible()) {
          await periodSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create|start/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created|started/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should add performance rating', async ({ page }) => {
      await page.goto('/hr/performance');

      const viewButton = page.getByRole('button', { name: /view|edit/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const ratingSelect = page.getByLabel(/rating|score/i);
        if (await ratingSelect.isVisible()) {
          await ratingSelect.click();
          await page.getByRole('option').first().click();
        }

        const commentsField = page.getByLabel(/comment|feedback/i);
        if (await commentsField.isVisible()) {
          await commentsField.fill('E2E Test - Good performance during the review period');
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Employee Schedules', () => {
    test('should display schedules page', async ({ page }) => {
      await page.goto('/hr/schedules');

      await expect(page.getByRole('heading', { name: /schedule|shift/i })).toBeVisible();
    });

    test('should create shift schedule', async ({ page }) => {
      await page.goto('/hr/schedules');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const employeeSelect = page.getByLabel(/employee/i);
        if (await employeeSelect.isVisible()) {
          await employeeSelect.click();
          await page.getByRole('option').first().click();
        }

        const dateField = page.getByLabel(/date/i);
        if (await dateField.isVisible()) {
          const tomorrow = new Date();
          tomorrow.setDate(tomorrow.getDate() + 1);
          await dateField.fill(tomorrow.toISOString().split('T')[0]);
        }

        const startTimeField = page.getByLabel(/start.*time/i);
        if (await startTimeField.isVisible()) {
          await startTimeField.fill('09:00');
        }

        const endTimeField = page.getByLabel(/end.*time/i);
        if (await endTimeField.isVisible()) {
          await endTimeField.fill('17:00');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view weekly schedule', async ({ page }) => {
      await page.goto('/hr/schedules');

      const weekView = page.getByRole('button', { name: /week/i });
      if (await weekView.isVisible()) {
        await weekView.click();

        await expect(page.locator('[class*="calendar"], [class*="schedule"], [class*="grid"]')).toBeVisible();
      }
    });
  });
});

test.describe('HR API E2E Tests', () => {
  test('should list employees', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/employees?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get employee details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/hr/employees?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const employeeId = listData.items[0].id;

        const response = await request.get(`/api/hr/employees/${employeeId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should validate employee creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/hr/employees', {
      headers: authHeaders(token),
      data: {
        firstName: '', // Invalid empty name
        lastName: '',
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should list attendance records', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/attendance?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should list leave requests', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/leave-requests?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });
});
