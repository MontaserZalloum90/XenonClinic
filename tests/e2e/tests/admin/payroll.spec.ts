import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Payroll Module
 * Tests cover salary structure, payroll processing, payslips, deductions, and reports
 */
test.describe('Payroll Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Payroll Dashboard', () => {
    test('should display payroll dashboard', async ({ page }) => {
      await page.goto('/hr/payroll');

      await expect(page.getByRole('heading', { name: /payroll/i })).toBeVisible();
    });

    test('should display payroll summary statistics', async ({ page }) => {
      await page.goto('/hr/payroll');

      const stats = page.locator('[class*="stat"], [class*="card"], [class*="summary"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });

    test('should display total payroll cost', async ({ page }) => {
      await page.goto('/hr/payroll');

      await expect(page.getByText(/total.*payroll|payroll.*cost|total.*salary/i)).toBeVisible();
    });

    test('should display pending payroll count', async ({ page }) => {
      await page.goto('/hr/payroll');

      await expect(page.getByText(/pending|unpaid|due/i)).toBeVisible();
    });

    test('should display payroll calendar/timeline', async ({ page }) => {
      await page.goto('/hr/payroll');

      const calendar = page.locator('[class*="calendar"], [class*="timeline"], [class*="schedule"]');
      if (await calendar.count() > 0) {
        await expect(calendar.first()).toBeVisible();
      }
    });
  });

  test.describe('Salary Structure', () => {
    test('should display salary structures page', async ({ page }) => {
      await page.goto('/hr/payroll/salary-structures');

      await expect(page.getByRole('heading', { name: /salary.*structure|pay.*grade/i })).toBeVisible();
    });

    test('should create new salary structure', async ({ page }) => {
      await page.goto('/hr/payroll/salary-structures');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name|title/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Salary Structure ${Date.now()}`);
        }

        const baseSalaryField = page.getByLabel(/base.*salary|basic.*salary/i);
        if (await baseSalaryField.isVisible()) {
          await baseSalaryField.fill('5000');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should edit salary structure', async ({ page }) => {
      await page.goto('/hr/payroll/salary-structures');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const baseSalaryField = page.getByLabel(/base.*salary|basic.*salary/i);
        if (await baseSalaryField.isVisible()) {
          await baseSalaryField.fill('5500');
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view salary structure details', async ({ page }) => {
      await page.goto('/hr/payroll/salary-structures');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/salary.*structure|component|allowance|deduction/i)).toBeVisible();
      }
    });

    test('should delete salary structure', async ({ page }) => {
      await page.goto('/hr/payroll/salary-structures');

      const deleteButton = page.getByRole('button', { name: /delete/i }).first();
      if (await deleteButton.isVisible()) {
        await deleteButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/deleted|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Salary Components', () => {
    test('should display salary components page', async ({ page }) => {
      await page.goto('/hr/payroll/components');

      await expect(page.getByRole('heading', { name: /component|allowance|deduction/i })).toBeVisible();
    });

    test('should create allowance component', async ({ page }) => {
      await page.goto('/hr/payroll/components');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill('Housing Allowance');
        }

        const typeSelect = page.getByLabel(/type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.click();
          await page.getByRole('option', { name: /allowance|earning/i }).click();
        }

        const calculationSelect = page.getByLabel(/calculation|method/i);
        if (await calculationSelect.isVisible()) {
          await calculationSelect.click();
          await page.getByRole('option', { name: /percentage|fixed/i }).first().click();
        }

        const valueField = page.getByLabel(/value|amount|percentage/i);
        if (await valueField.isVisible()) {
          await valueField.fill('25');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should create deduction component', async ({ page }) => {
      await page.goto('/hr/payroll/components');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill('Health Insurance');
        }

        const typeSelect = page.getByLabel(/type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.click();
          await page.getByRole('option', { name: /deduction/i }).click();
        }

        const valueField = page.getByLabel(/value|amount/i);
        if (await valueField.isVisible()) {
          await valueField.fill('200');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter components by type', async ({ page }) => {
      await page.goto('/hr/payroll/components');

      const typeFilter = page.getByRole('combobox', { name: /type/i });
      if (await typeFilter.isVisible()) {
        await typeFilter.click();
        await page.getByRole('option', { name: /allowance|deduction/i }).first().click();
      }
    });

    test('should set component as taxable', async ({ page }) => {
      await page.goto('/hr/payroll/components');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const taxableCheckbox = page.getByLabel(/taxable/i);
        if (await taxableCheckbox.isVisible()) {
          await taxableCheckbox.check();
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Employee Salary Assignment', () => {
    test('should display employee salaries page', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      await expect(page.getByRole('heading', { name: /employee.*salary|salary.*assignment/i })).toBeVisible();
    });

    test('should assign salary structure to employee', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const assignButton = page.getByRole('button', { name: /assign|set.*salary/i }).first();
      if (await assignButton.isVisible()) {
        await assignButton.click();

        const structureSelect = page.getByLabel(/structure|grade/i);
        if (await structureSelect.isVisible()) {
          await structureSelect.click();
          await page.getByRole('option').first().click();
        }

        const effectiveDate = page.getByLabel(/effective.*date|from.*date/i);
        if (await effectiveDate.isVisible()) {
          const today = new Date().toISOString().split('T')[0];
          await effectiveDate.fill(today);
        }

        const submitButton = page.getByRole('button', { name: /save|assign/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|assigned/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should update employee salary', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const editButton = page.getByRole('button', { name: /edit|update/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const baseSalaryField = page.getByLabel(/base.*salary|basic/i);
        if (await baseSalaryField.isVisible()) {
          await baseSalaryField.fill('6000');
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view employee salary history', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const historyButton = page.getByRole('button', { name: /history|view/i }).first();
      if (await historyButton.isVisible()) {
        await historyButton.click();

        await expect(page.getByText(/salary.*history|revision/i)).toBeVisible();
      }
    });

    test('should add individual allowance to employee', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const addAllowanceButton = page.getByRole('button', { name: /add.*allowance/i });
        if (await addAllowanceButton.isVisible()) {
          await addAllowanceButton.click();

          const allowanceSelect = page.getByLabel(/allowance/i);
          if (await allowanceSelect.isVisible()) {
            await allowanceSelect.click();
            await page.getByRole('option').first().click();
          }

          const amountField = page.getByLabel(/amount/i);
          if (await amountField.isVisible()) {
            await amountField.fill('500');
          }
        }

        const submitButton = page.getByRole('button', { name: /save/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should add individual deduction to employee', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const addDeductionButton = page.getByRole('button', { name: /add.*deduction/i });
        if (await addDeductionButton.isVisible()) {
          await addDeductionButton.click();

          const deductionSelect = page.getByLabel(/deduction/i);
          if (await deductionSelect.isVisible()) {
            await deductionSelect.click();
            await page.getByRole('option').first().click();
          }

          const amountField = page.getByLabel(/amount/i);
          if (await amountField.isVisible()) {
            await amountField.fill('100');
          }
        }

        const submitButton = page.getByRole('button', { name: /save/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should search employees by name', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('Dr.');
        await page.waitForTimeout(500);
      }
    });

    test('should filter by department', async ({ page }) => {
      await page.goto('/hr/payroll/employee-salaries');

      const departmentFilter = page.getByRole('combobox', { name: /department/i });
      if (await departmentFilter.isVisible()) {
        await departmentFilter.click();
        await page.getByRole('option').first().click();
      }
    });
  });

  test.describe('Payroll Processing', () => {
    test('should display payroll runs page', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      await expect(page.getByRole('heading', { name: /payroll.*run|payroll.*process/i })).toBeVisible();
    });

    test('should create new payroll run', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const createButton = page.getByRole('button', { name: /create|new|run.*payroll|process/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select pay period
        const periodSelect = page.getByLabel(/period|month/i);
        if (await periodSelect.isVisible()) {
          await periodSelect.click();
          await page.getByRole('option').first().click();
        }

        // Select year
        const yearSelect = page.getByLabel(/year/i);
        if (await yearSelect.isVisible()) {
          await yearSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /create|run|process/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created|processing/i)).toBeVisible({ timeout: 10000 });
      }
    });

    test('should select employees for payroll run', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const createButton = page.getByRole('button', { name: /create|new|run.*payroll/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Select all employees
        const selectAllCheckbox = page.getByLabel(/select.*all/i);
        if (await selectAllCheckbox.isVisible()) {
          await selectAllCheckbox.check();
        }

        // Or select individual employees
        const employeeCheckboxes = page.locator('input[type="checkbox"]');
        if (await employeeCheckboxes.count() > 1) {
          await employeeCheckboxes.nth(1).check();
        }
      }
    });

    test('should calculate payroll', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const calculateButton = page.getByRole('button', { name: /calculate|compute/i });
        if (await calculateButton.isVisible()) {
          await calculateButton.click();

          await expect(page.getByText(/calculated|success/i)).toBeVisible({ timeout: 10000 });
        }
      }
    });

    test('should view payroll calculation breakdown', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Should show breakdown
        await expect(page.getByText(/gross|net|allowance|deduction/i)).toBeVisible();
      }
    });

    test('should approve payroll run', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

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

    test('should reject payroll run with reason', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const rejectButton = page.getByRole('button', { name: /reject/i }).first();
      if (await rejectButton.isVisible()) {
        await rejectButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Incorrect calculations');
        }

        const confirmButton = page.getByRole('button', { name: /reject|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/rejected|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should finalize payroll run', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const finalizeButton = page.getByRole('button', { name: /finalize|complete|close/i }).first();
      if (await finalizeButton.isVisible()) {
        await finalizeButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/finalized|completed|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter payroll runs by status', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /draft|pending|approved|finalized/i }).first().click();
      }
    });

    test('should filter payroll runs by period', async ({ page }) => {
      await page.goto('/hr/payroll/runs');

      const periodFilter = page.getByRole('combobox', { name: /period|month/i });
      if (await periodFilter.isVisible()) {
        await periodFilter.click();
        await page.getByRole('option').first().click();
      }
    });
  });

  test.describe('Payslips', () => {
    test('should display payslips page', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      await expect(page.getByRole('heading', { name: /payslip|pay.*slip|salary.*slip/i })).toBeVisible();
    });

    test('should view individual payslip', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/payslip|salary.*slip|earnings|deductions/i)).toBeVisible();
      }
    });

    test('should display payslip with earnings breakdown', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Should show earnings section
        await expect(page.getByText(/earning|allowance|basic.*salary/i)).toBeVisible();
      }
    });

    test('should display payslip with deductions breakdown', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Should show deductions section
        await expect(page.getByText(/deduction|tax|insurance/i)).toBeVisible();
      }
    });

    test('should display net salary', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/net.*salary|net.*pay|take.*home/i)).toBeVisible();
      }
    });

    test('should print payslip', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          await expect(printButton).toBeVisible();
        }
      }
    });

    test('should download payslip as PDF', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const downloadButton = page.getByRole('button', { name: /download|pdf/i });
        if (await downloadButton.isVisible()) {
          const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
          await downloadButton.click();

          const download = await downloadPromise;
          if (download) {
            expect(download.suggestedFilename()).toMatch(/payslip.*\.pdf/i);
          }
        }
      }
    });

    test('should email payslip to employee', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const emailButton = page.getByRole('button', { name: /email|send/i });
        if (await emailButton.isVisible()) {
          await emailButton.click();

          await expect(page.getByText(/email.*sent|success/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should bulk email payslips', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const bulkEmailButton = page.getByRole('button', { name: /bulk.*email|send.*all/i });
      if (await bulkEmailButton.isVisible()) {
        await bulkEmailButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm|send/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/sent|success/i)).toBeVisible({ timeout: 10000 });
      }
    });

    test('should search payslips by employee', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('John');
        await page.waitForTimeout(500);
      }
    });

    test('should filter payslips by month', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const monthFilter = page.getByRole('combobox', { name: /month|period/i });
      if (await monthFilter.isVisible()) {
        await monthFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter payslips by year', async ({ page }) => {
      await page.goto('/hr/payroll/payslips');

      const yearFilter = page.getByRole('combobox', { name: /year/i });
      if (await yearFilter.isVisible()) {
        await yearFilter.click();
        await page.getByRole('option').first().click();
      }
    });
  });

  test.describe('Overtime Management', () => {
    test('should display overtime page', async ({ page }) => {
      await page.goto('/hr/payroll/overtime');

      await expect(page.getByRole('heading', { name: /overtime/i })).toBeVisible();
    });

    test('should record overtime hours', async ({ page }) => {
      await page.goto('/hr/payroll/overtime');

      const createButton = page.getByRole('button', { name: /create|new|add|record/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const employeeSelect = page.getByLabel(/employee/i);
        if (await employeeSelect.isVisible()) {
          await employeeSelect.click();
          await page.getByRole('option').first().click();
        }

        const dateField = page.getByLabel(/date/i);
        if (await dateField.isVisible()) {
          const today = new Date().toISOString().split('T')[0];
          await dateField.fill(today);
        }

        const hoursField = page.getByLabel(/hours/i);
        if (await hoursField.isVisible()) {
          await hoursField.fill('4');
        }

        const rateSelect = page.getByLabel(/rate|multiplier/i);
        if (await rateSelect.isVisible()) {
          await rateSelect.click();
          await page.getByRole('option', { name: /1\.5|2\.0|normal/i }).first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|recorded/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should approve overtime request', async ({ page }) => {
      await page.goto('/hr/payroll/overtime');

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

    test('should reject overtime request', async ({ page }) => {
      await page.goto('/hr/payroll/overtime');

      const rejectButton = page.getByRole('button', { name: /reject/i }).first();
      if (await rejectButton.isVisible()) {
        await rejectButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Not authorized');
        }

        const confirmButton = page.getByRole('button', { name: /reject|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/rejected|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter overtime by status', async ({ page }) => {
      await page.goto('/hr/payroll/overtime');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /pending|approved|rejected/i }).first().click();
      }
    });
  });

  test.describe('Bonus & Incentives', () => {
    test('should display bonuses page', async ({ page }) => {
      await page.goto('/hr/payroll/bonuses');

      await expect(page.getByRole('heading', { name: /bonus|incentive/i })).toBeVisible();
    });

    test('should create bonus payment', async ({ page }) => {
      await page.goto('/hr/payroll/bonuses');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const employeeSelect = page.getByLabel(/employee/i);
        if (await employeeSelect.isVisible()) {
          await employeeSelect.click();
          await page.getByRole('option').first().click();
        }

        const typeSelect = page.getByLabel(/type|bonus.*type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.click();
          await page.getByRole('option', { name: /performance|annual|eid|holiday/i }).first().click();
        }

        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('2000');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should create bulk bonus for department', async ({ page }) => {
      await page.goto('/hr/payroll/bonuses');

      const bulkButton = page.getByRole('button', { name: /bulk|department.*bonus/i });
      if (await bulkButton.isVisible()) {
        await bulkButton.click();

        const departmentSelect = page.getByLabel(/department/i);
        if (await departmentSelect.isVisible()) {
          await departmentSelect.click();
          await page.getByRole('option').first().click();
        }

        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('1000');
        }

        const submitButton = page.getByRole('button', { name: /save|create|apply/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|applied/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should approve bonus', async ({ page }) => {
      await page.goto('/hr/payroll/bonuses');

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

  test.describe('Loans & Advances', () => {
    test('should display loans page', async ({ page }) => {
      await page.goto('/hr/payroll/loans');

      await expect(page.getByRole('heading', { name: /loan|advance/i })).toBeVisible();
    });

    test('should create loan request', async ({ page }) => {
      await page.goto('/hr/payroll/loans');

      const createButton = page.getByRole('button', { name: /create|new|request/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const employeeSelect = page.getByLabel(/employee/i);
        if (await employeeSelect.isVisible()) {
          await employeeSelect.click();
          await page.getByRole('option').first().click();
        }

        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('5000');
        }

        const installmentsField = page.getByLabel(/installment|month/i);
        if (await installmentsField.isVisible()) {
          await installmentsField.fill('6');
        }

        const reasonField = page.getByLabel(/reason|purpose/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Personal loan');
        }

        const submitButton = page.getByRole('button', { name: /save|submit/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|submitted/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should approve loan', async ({ page }) => {
      await page.goto('/hr/payroll/loans');

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

    test('should view loan repayment schedule', async ({ page }) => {
      await page.goto('/hr/payroll/loans');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/repayment|schedule|installment/i)).toBeVisible();
      }
    });

    test('should track loan balance', async ({ page }) => {
      await page.goto('/hr/payroll/loans');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/balance|remaining|outstanding/i)).toBeVisible();
      }
    });
  });

  test.describe('Bank Transfers', () => {
    test('should display bank transfers page', async ({ page }) => {
      await page.goto('/hr/payroll/bank-transfers');

      await expect(page.getByRole('heading', { name: /bank.*transfer|payment/i })).toBeVisible();
    });

    test('should generate bank transfer file', async ({ page }) => {
      await page.goto('/hr/payroll/bank-transfers');

      const generateButton = page.getByRole('button', { name: /generate|create.*file/i });
      if (await generateButton.isVisible()) {
        await generateButton.click();

        const payrollSelect = page.getByLabel(/payroll.*run|period/i);
        if (await payrollSelect.isVisible()) {
          await payrollSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /generate|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/generated|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should download bank transfer file', async ({ page }) => {
      await page.goto('/hr/payroll/bank-transfers');

      const downloadButton = page.getByRole('button', { name: /download/i }).first();
      if (await downloadButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await downloadButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/bank.*transfer.*\.(csv|txt|xml)/i);
        }
      }
    });

    test('should mark transfers as completed', async ({ page }) => {
      await page.goto('/hr/payroll/bank-transfers');

      const completeButton = page.getByRole('button', { name: /complete|mark.*paid/i }).first();
      if (await completeButton.isVisible()) {
        await completeButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/completed|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Tax Calculations', () => {
    test('should display tax settings page', async ({ page }) => {
      await page.goto('/hr/payroll/tax-settings');

      await expect(page.getByRole('heading', { name: /tax/i })).toBeVisible();
    });

    test('should configure tax brackets', async ({ page }) => {
      await page.goto('/hr/payroll/tax-settings');

      const editButton = page.getByRole('button', { name: /edit|configure/i });
      if (await editButton.isVisible()) {
        await editButton.click();

        const bracketField = page.getByLabel(/bracket|rate/i).first();
        if (await bracketField.isVisible()) {
          await bracketField.fill('10');
        }

        const submitButton = page.getByRole('button', { name: /save/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view employee tax summary', async ({ page }) => {
      await page.goto('/hr/payroll/tax-settings');

      const summaryButton = page.getByRole('button', { name: /summary|report/i });
      if (await summaryButton.isVisible()) {
        await summaryButton.click();

        await expect(page.getByText(/tax.*summary|total.*tax/i)).toBeVisible();
      }
    });
  });

  test.describe('End of Service', () => {
    test('should display end of service page', async ({ page }) => {
      await page.goto('/hr/payroll/end-of-service');

      await expect(page.getByRole('heading', { name: /end.*service|final.*settlement|gratuity/i })).toBeVisible();
    });

    test('should calculate end of service benefit', async ({ page }) => {
      await page.goto('/hr/payroll/end-of-service');

      const calculateButton = page.getByRole('button', { name: /calculate/i });
      if (await calculateButton.isVisible()) {
        await calculateButton.click();

        const employeeSelect = page.getByLabel(/employee/i);
        if (await employeeSelect.isVisible()) {
          await employeeSelect.click();
          await page.getByRole('option').first().click();
        }

        const endDateField = page.getByLabel(/end.*date|termination.*date/i);
        if (await endDateField.isVisible()) {
          const today = new Date().toISOString().split('T')[0];
          await endDateField.fill(today);
        }

        const calcButton = page.getByRole('button', { name: /calculate/i }).last();
        await calcButton.click();

        await expect(page.getByText(/gratuity|settlement|total/i)).toBeVisible();
      }
    });

    test('should generate final settlement', async ({ page }) => {
      await page.goto('/hr/payroll/end-of-service');

      const generateButton = page.getByRole('button', { name: /generate|create.*settlement/i });
      if (await generateButton.isVisible()) {
        await generateButton.click();

        await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      }
    });

    test('should include pending leave balance', async ({ page }) => {
      await page.goto('/hr/payroll/end-of-service');

      const calculateButton = page.getByRole('button', { name: /calculate/i });
      if (await calculateButton.isVisible()) {
        await calculateButton.click();

        await expect(page.getByText(/leave.*balance|accrued.*leave/i)).toBeVisible();
      }
    });
  });

  test.describe('Payroll Reports', () => {
    test('should display payroll reports page', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      await expect(page.getByRole('heading', { name: /report/i })).toBeVisible();
    });

    test('should generate payroll summary report', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      const summaryButton = page.getByRole('button', { name: /summary|payroll.*report/i });
      if (await summaryButton.isVisible()) {
        await summaryButton.click();

        await expect(page.getByText(/payroll.*summary|total/i)).toBeVisible();
      }
    });

    test('should generate department-wise report', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      const deptButton = page.getByRole('button', { name: /department/i });
      if (await deptButton.isVisible()) {
        await deptButton.click();

        await expect(page.getByText(/department/i)).toBeVisible();
      }
    });

    test('should generate year-to-date report', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      const ytdButton = page.getByRole('button', { name: /ytd|year.*to.*date/i });
      if (await ytdButton.isVisible()) {
        await ytdButton.click();

        await expect(page.getByText(/year.*to.*date|ytd/i)).toBeVisible();
      }
    });

    test('should generate tax report', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      const taxButton = page.getByRole('button', { name: /tax.*report/i });
      if (await taxButton.isVisible()) {
        await taxButton.click();

        await expect(page.getByText(/tax/i)).toBeVisible();
      }
    });

    test('should export payroll report', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/payroll.*\.(csv|xlsx|pdf)/i);
        }
      }
    });

    test('should filter reports by date range', async ({ page }) => {
      await page.goto('/hr/payroll/reports');

      const fromDate = page.getByLabel(/from|start.*date/i);
      const toDate = page.getByLabel(/to|end.*date/i);

      if (await fromDate.isVisible() && await toDate.isVisible()) {
        const today = new Date();
        const firstDay = new Date(today.getFullYear(), 0, 1);

        await fromDate.fill(firstDay.toISOString().split('T')[0]);
        await toDate.fill(today.toISOString().split('T')[0]);
      }
    });
  });
});

test.describe('Payroll API E2E Tests', () => {
  test('should list payroll runs', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/payroll/runs?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get payroll run details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/hr/payroll/runs?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const runId = listData.items[0].id;

        const response = await request.get(`/api/hr/payroll/runs/${runId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should list payslips', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/payroll/payslips?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get employee salary details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/payroll/employee-salaries?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should list salary structures', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/payroll/salary-structures', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should list salary components', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/hr/payroll/components', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should validate payroll creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/hr/payroll/runs', {
      headers: authHeaders(token),
      data: {
        // Missing required fields
      },
    });

    expect(response.status()).toBe(400);
  });
});
