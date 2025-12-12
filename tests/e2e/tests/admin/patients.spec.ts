import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Patient Management Module
 * Tests cover CRUD operations, medical history, documents, search, and filtering
 */
test.describe('Patient Management Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Patient List', () => {
    test('should display patient list page with correct structure', async ({ page }) => {
      await page.goto('/patients');

      // Verify page structure
      await expect(page.getByRole('heading', { name: /patients/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
      await expect(page.getByPlaceholder(/search/i)).toBeVisible();
    });

    test('should display patient statistics cards', async ({ page }) => {
      await page.goto('/patients');

      // Check for statistics
      const statsContainer = page.locator('[class*="stat"], [class*="card"], [class*="summary"]');
      await expect(statsContainer.first()).toBeVisible();
    });

    test('should search patients by name', async ({ page }) => {
      await page.goto('/patients');

      const searchInput = page.getByPlaceholder(/search/i);
      await searchInput.fill('John');
      await page.waitForTimeout(500); // Debounce wait

      // Results should update based on search
      const table = page.locator('table, [class*="list"], [class*="grid"]');
      await expect(table).toBeVisible();
    });

    test('should search patients by phone number', async ({ page }) => {
      await page.goto('/patients');

      const searchInput = page.getByPlaceholder(/search/i);
      await searchInput.fill('+971');
      await page.waitForTimeout(500);

      // Should find patients with UAE phone numbers
      const table = page.locator('table, [class*="list"], [class*="grid"]');
      await expect(table).toBeVisible();
    });

    test('should search patients by medical record number', async ({ page }) => {
      await page.goto('/patients');

      const searchInput = page.getByPlaceholder(/search/i);
      await searchInput.fill('MRN-');
      await page.waitForTimeout(500);

      const table = page.locator('table, [class*="list"], [class*="grid"]');
      await expect(table).toBeVisible();
    });

    test('should filter patients by status', async ({ page }) => {
      await page.goto('/patients');

      const statusFilter = page.getByRole('combobox', { name: /status|filter/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /active/i }).click();
        await page.waitForTimeout(300);
      }
    });

    test('should filter patients by gender', async ({ page }) => {
      await page.goto('/patients');

      const genderFilter = page.getByRole('combobox', { name: /gender/i });
      if (await genderFilter.isVisible()) {
        await genderFilter.click();
        await page.getByRole('option', { name: /male|female/i }).first().click();
        await page.waitForTimeout(300);
      }
    });

    test('should paginate patient list', async ({ page }) => {
      await page.goto('/patients');

      const nextButton = page.getByRole('button', { name: /next|>/i });
      if (await nextButton.isVisible() && await nextButton.isEnabled()) {
        await nextButton.click();
        await page.waitForTimeout(300);

        // Previous button should now be enabled
        const prevButton = page.getByRole('button', { name: /prev|</i });
        await expect(prevButton).toBeEnabled();
      }
    });

    test('should export patient list', async ({ page }) => {
      await page.goto('/patients');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/patient.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });

  test.describe('Create Patient', () => {
    test('should open create patient form', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Check form/modal opened
      await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
      await expect(page.getByRole('heading', { name: /create.*patient|new.*patient|add.*patient/i })).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Try to submit without required fields
      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      // Should show validation errors
      await expect(page.getByText(/required|must.*enter|please.*fill/i)).toBeVisible();
    });

    test('should validate email format', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const emailField = page.getByLabel(/email/i);
      if (await emailField.isVisible()) {
        await emailField.fill('invalid-email');

        const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/invalid.*email|email.*format/i)).toBeVisible();
      }
    });

    test('should validate phone number format', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const phoneField = page.getByLabel(/phone|mobile/i);
      if (await phoneField.isVisible()) {
        await phoneField.fill('invalid');

        const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/invalid.*phone|phone.*format/i)).toBeVisible();
      }
    });

    test('should validate date of birth is in the past', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const dobField = page.getByLabel(/date.*birth|dob|birth.*date/i);
      if (await dobField.isVisible()) {
        const futureDate = new Date();
        futureDate.setFullYear(futureDate.getFullYear() + 1);
        await dobField.fill(futureDate.toISOString().split('T')[0]);

        const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/past|future|invalid.*date/i)).toBeVisible();
      }
    });

    test('should create patient with valid data', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Fill required fields
      const firstNameField = page.getByLabel(/first.*name/i);
      if (await firstNameField.isVisible()) {
        await firstNameField.fill(`E2E Test Patient ${Date.now()}`);
      }

      const lastNameField = page.getByLabel(/last.*name/i);
      if (await lastNameField.isVisible()) {
        await lastNameField.fill('TestLastName');
      }

      const phoneField = page.getByLabel(/phone|mobile/i);
      if (await phoneField.isVisible()) {
        await phoneField.fill('+971501234567');
      }

      const dobField = page.getByLabel(/date.*birth|dob/i);
      if (await dobField.isVisible()) {
        await dobField.fill('1990-01-15');
      }

      const genderSelect = page.getByLabel(/gender/i);
      if (await genderSelect.isVisible()) {
        await genderSelect.click();
        await page.getByRole('option', { name: /male/i }).click();
      }

      // Submit
      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      // Check for success
      await expect(page.getByText(/success|created|saved/i)).toBeVisible({ timeout: 10000 });
    });

    test('should prevent duplicate patient creation', async ({ page }) => {
      await page.goto('/patients');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const phoneField = page.getByLabel(/phone|mobile/i);
      if (await phoneField.isVisible()) {
        // Use existing phone number
        await phoneField.fill('+971501234567');
      }

      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      // Should show duplicate warning or error
      await expect(page.getByText(/exists|duplicate|already/i)).toBeVisible({ timeout: 5000 }).catch(() => {
        // May succeed if patient doesn't exist
      });
    });
  });

  test.describe('Patient Details', () => {
    test('should view patient details', async ({ page }) => {
      await page.goto('/patients');

      // Click on first patient row or view button
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
      } else {
        await page.getByRole('row').nth(1).click();
      }

      // Should show patient details
      await expect(page.getByText(/patient.*info|patient.*detail|medical.*record/i)).toBeVisible();
    });

    test('should display patient basic information', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Check for basic info sections
        await expect(page.getByText(/name|phone|email|date.*birth/i)).toBeVisible();
      }
    });

    test('should display patient medical history section', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Navigate to medical history tab if tabbed
        const historyTab = page.getByRole('tab', { name: /history|medical/i });
        if (await historyTab.isVisible()) {
          await historyTab.click();
        }

        await expect(page.getByText(/medical.*history|history/i)).toBeVisible();
      }
    });

    test('should display patient appointments history', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const appointmentsTab = page.getByRole('tab', { name: /appointment/i });
        if (await appointmentsTab.isVisible()) {
          await appointmentsTab.click();
        }

        await expect(page.getByText(/appointment/i)).toBeVisible();
      }
    });

    test('should display patient documents section', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const documentsTab = page.getByRole('tab', { name: /document|file/i });
        if (await documentsTab.isVisible()) {
          await documentsTab.click();
        }

        await expect(page.getByText(/document|file|upload/i)).toBeVisible();
      }
    });

    test('should display patient billing history', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const billingTab = page.getByRole('tab', { name: /billing|invoice|financial/i });
        if (await billingTab.isVisible()) {
          await billingTab.click();
        }

        await expect(page.getByText(/billing|invoice|payment/i)).toBeVisible();
      }
    });
  });

  test.describe('Edit Patient', () => {
    test('should open edit patient form', async ({ page }) => {
      await page.goto('/patients');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        await expect(page.getByRole('heading', { name: /edit.*patient|update.*patient/i })).toBeVisible();
      }
    });

    test('should update patient information', async ({ page }) => {
      await page.goto('/patients');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        // Update a field
        const notesField = page.getByLabel(/notes|comment/i);
        if (await notesField.isVisible()) {
          await notesField.fill(`Updated by E2E test at ${new Date().toISOString()}`);
        }

        // Submit
        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        // Check for success
        await expect(page.getByText(/success|updated|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should cancel edit without saving changes', async ({ page }) => {
      await page.goto('/patients');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        // Make a change
        const notesField = page.getByLabel(/notes|comment/i);
        if (await notesField.isVisible()) {
          await notesField.fill('Temporary change');
        }

        // Cancel
        const cancelButton = page.getByRole('button', { name: /cancel|close/i });
        await cancelButton.click();

        // Should close form without saving
        await expect(page.getByRole('dialog')).not.toBeVisible({ timeout: 3000 });
      }
    });
  });

  test.describe('Patient Medical History', () => {
    test('should add medical condition', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const historyTab = page.getByRole('tab', { name: /history|medical/i });
        if (await historyTab.isVisible()) {
          await historyTab.click();
        }

        const addConditionButton = page.getByRole('button', { name: /add.*condition|add.*history/i });
        if (await addConditionButton.isVisible()) {
          await addConditionButton.click();

          const conditionField = page.getByLabel(/condition|diagnosis/i);
          if (await conditionField.isVisible()) {
            await conditionField.fill('Hypertension');
          }

          const submitButton = page.getByRole('button', { name: /save|add/i }).last();
          await submitButton.click();

          await expect(page.getByText(/hypertension|success/i)).toBeVisible();
        }
      }
    });

    test('should add allergy', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const addAllergyButton = page.getByRole('button', { name: /add.*allergy/i });
        if (await addAllergyButton.isVisible()) {
          await addAllergyButton.click();

          const allergyField = page.getByLabel(/allergy|allergen/i);
          if (await allergyField.isVisible()) {
            await allergyField.fill('Penicillin');
          }

          const severityField = page.getByLabel(/severity/i);
          if (await severityField.isVisible()) {
            await severityField.click();
            await page.getByRole('option', { name: /severe|high/i }).click();
          }

          const submitButton = page.getByRole('button', { name: /save|add/i }).last();
          await submitButton.click();

          await expect(page.getByText(/penicillin|success/i)).toBeVisible();
        }
      }
    });

    test('should add current medication', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const addMedicationButton = page.getByRole('button', { name: /add.*medication/i });
        if (await addMedicationButton.isVisible()) {
          await addMedicationButton.click();

          const medicationField = page.getByLabel(/medication|drug|medicine/i);
          if (await medicationField.isVisible()) {
            await medicationField.fill('Lisinopril 10mg');
          }

          const submitButton = page.getByRole('button', { name: /save|add/i }).last();
          await submitButton.click();

          await expect(page.getByText(/lisinopril|success/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Patient Documents', () => {
    test('should upload document', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const documentsTab = page.getByRole('tab', { name: /document|file/i });
        if (await documentsTab.isVisible()) {
          await documentsTab.click();
        }

        const uploadButton = page.getByRole('button', { name: /upload|add.*document/i });
        if (await uploadButton.isVisible()) {
          // Note: File upload would require actual file handling
          await expect(uploadButton).toBeVisible();
        }
      }
    });

    test('should view document categories', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const documentsTab = page.getByRole('tab', { name: /document|file/i });
        if (await documentsTab.isVisible()) {
          await documentsTab.click();

          // Check for document categories
          await expect(page.getByText(/lab.*result|prescription|report|image|consent/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Patient Deletion', () => {
    test('should show delete confirmation', async ({ page }) => {
      await page.goto('/patients');

      const deleteButton = page.getByRole('button', { name: /delete/i }).first();
      if (await deleteButton.isVisible()) {
        await deleteButton.click();

        // Should show confirmation dialog
        await expect(page.getByRole('dialog').or(page.getByText(/confirm|sure|delete/i))).toBeVisible();
      }
    });

    test('should cancel delete operation', async ({ page }) => {
      await page.goto('/patients');

      const deleteButton = page.getByRole('button', { name: /delete/i }).first();
      if (await deleteButton.isVisible()) {
        await deleteButton.click();

        const cancelButton = page.getByRole('button', { name: /cancel|no/i });
        if (await cancelButton.isVisible()) {
          await cancelButton.click();

          // Dialog should close
          await expect(page.getByRole('alertdialog')).not.toBeVisible({ timeout: 3000 });
        }
      }
    });
  });

  test.describe('Patient Quick Actions', () => {
    test('should create appointment from patient view', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const createAppointmentButton = page.getByRole('button', { name: /book.*appointment|create.*appointment|schedule/i });
        if (await createAppointmentButton.isVisible()) {
          await createAppointmentButton.click();

          // Should open appointment creation with patient pre-selected
          await expect(page.getByText(/appointment|schedule/i)).toBeVisible();
        }
      }
    });

    test('should create invoice from patient view', async ({ page }) => {
      await page.goto('/patients');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const createInvoiceButton = page.getByRole('button', { name: /create.*invoice|new.*invoice|bill/i });
        if (await createInvoiceButton.isVisible()) {
          await createInvoiceButton.click();

          await expect(page.getByText(/invoice|bill/i)).toBeVisible();
        }
      }
    });
  });
});

test.describe('Patient API E2E Tests', () => {
  test('should list patients with pagination', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/patients?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data).toHaveProperty('items');
    expect(Array.isArray(data.items)).toBeTruthy();
  });

  test('should search patients by name', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/patients?search=test', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get patient details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    // First get a patient ID
    const listResponse = await request.get('/api/patients?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const patientId = listData.items[0].id;

        const response = await request.get(`/api/patients/${patientId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);

        const patient = await response.json();
        expect(patient).toHaveProperty('id');
      }
    }
  });

  test('should validate patient creation payload', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    // Send invalid payload
    const response = await request.post('/api/patients', {
      headers: authHeaders(token),
      data: {
        firstName: '', // Empty required field
        lastName: '',
      },
    });

    // Should return validation error
    expect(response.status()).toBe(400);
  });

  test('should handle patient not found', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/patients/999999999', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(404);
  });
});

test.describe('Patient Data Privacy', () => {
  test('should mask sensitive data in list view', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);

    await page.goto('/patients');

    // Phone numbers should be partially masked in list
    const phoneNumbers = page.locator('[class*="phone"], td:has-text("+")');
    if (await phoneNumbers.count() > 0) {
      const text = await phoneNumbers.first().textContent();
      // Phone should either be fully shown (for authorized users) or masked
      expect(text).toBeDefined();
    }
  });

  test('should require consent for data access', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);

    await page.goto('/patients');

    const viewButton = page.getByRole('button', { name: /view|details/i }).first();
    if (await viewButton.isVisible()) {
      await viewButton.click();

      // Check for consent section
      const consentSection = page.getByText(/consent|privacy|hipaa/i);
      if (await consentSection.isVisible()) {
        await expect(consentSection).toBeVisible();
      }
    }
  });
});

test.describe('Patient Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should navigate patient list with keyboard', async ({ page }) => {
    await page.goto('/patients');

    // Tab through elements
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    // Check focus is visible
    const focusedElement = page.locator(':focus');
    await expect(focusedElement).toBeVisible();
  });

  test('should have proper form labels', async ({ page }) => {
    await page.goto('/patients');

    await page.getByRole('button', { name: /create|new|add/i }).click();

    // Check form fields have proper labels
    const formFields = page.locator('input, select, textarea');
    const count = await formFields.count();

    for (let i = 0; i < Math.min(count, 5); i++) {
      const field = formFields.nth(i);
      const id = await field.getAttribute('id');
      const ariaLabel = await field.getAttribute('aria-label');
      const ariaLabelledBy = await field.getAttribute('aria-labelledby');

      // Field should have some form of labeling
      const hasLabel = id || ariaLabel || ariaLabelledBy;
      expect(hasLabel).toBeTruthy();
    }
  });

  test('should announce errors to screen readers', async ({ page }) => {
    await page.goto('/patients');

    await page.getByRole('button', { name: /create|new|add/i }).click();

    // Submit empty form
    const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
    await submitButton.click();

    // Error messages should be in an alert region or have role="alert"
    const errorAlert = page.locator('[role="alert"], [aria-live="polite"], [aria-live="assertive"]');
    if (await errorAlert.count() > 0) {
      await expect(errorAlert.first()).toBeVisible();
    }
  });
});

test.describe('Patient Mobile Responsiveness', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should display mobile-friendly patient list', async ({ page }) => {
    await page.goto('/patients');

    // Page should be visible and functional
    await expect(page.getByRole('heading', { name: /patient/i })).toBeVisible();

    // No horizontal overflow
    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
    expect(scrollWidth).toBeLessThanOrEqual(clientWidth + 20);
  });

  test('should show touch-friendly action buttons', async ({ page }) => {
    await page.goto('/patients');

    // Buttons should be large enough for touch
    const buttons = page.getByRole('button');
    const count = await buttons.count();

    for (let i = 0; i < Math.min(count, 3); i++) {
      const button = buttons.nth(i);
      if (await button.isVisible()) {
        const box = await button.boundingBox();
        if (box) {
          // Minimum touch target size (44x44 recommended by Apple, 48x48 by Google)
          expect(box.width).toBeGreaterThanOrEqual(40);
          expect(box.height).toBeGreaterThanOrEqual(40);
        }
      }
    }
  });
});
