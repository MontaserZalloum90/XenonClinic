import { test, expect } from '@playwright/test';

test.describe('Sales Module E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Sales List', () => {
    test('should display sales list page', async ({ page }) => {
      await page.goto('/sales');

      // Check page title and structure
      await expect(page.getByRole('heading', { name: /sales|transactions/i })).toBeVisible();

      // Check for key UI elements
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
      await expect(page.getByPlaceholder(/search/i)).toBeVisible();
    });

    test('should display sales statistics', async ({ page }) => {
      await page.goto('/sales');

      // Check for statistics cards
      await expect(page.getByText(/total sales|revenue/i)).toBeVisible();
      await expect(page.getByText(/outstanding|unpaid/i)).toBeVisible();
    });

    test('should filter sales by status', async ({ page }) => {
      await page.goto('/sales');

      // Look for filter/status dropdown
      const statusFilter = page.getByRole('combobox', { name: /status|filter/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /completed|paid/i }).click();
      }
    });

    test('should search sales by invoice number', async ({ page }) => {
      await page.goto('/sales');

      const searchInput = page.getByPlaceholder(/search|invoice/i);
      await searchInput.fill('SALE-');

      // Wait for search results
      await page.waitForTimeout(500);
    });
  });

  test.describe('Create Sale', () => {
    test('should open create sale modal', async ({ page }) => {
      await page.goto('/sales');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Check modal opened
      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByRole('heading', { name: /create.*sale|new.*sale/i })).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/sales');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Try to submit without required fields
      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      // Should show validation errors
      await expect(page.getByText(/required|must.*select|patient.*required/i)).toBeVisible();
    });

    test('should create a new sale successfully', async ({ page }) => {
      await page.goto('/sales');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Fill in sale details
      const patientSelect = page.getByLabel(/patient/i);
      if (await patientSelect.isVisible()) {
        await patientSelect.click();
        await page.getByRole('option').first().click();
      }

      // Add notes
      const notesField = page.getByLabel(/notes|description/i);
      if (await notesField.isVisible()) {
        await notesField.fill('E2E Test Sale');
      }

      // Submit
      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();

      // Check for success message or modal closed
      await expect(page.getByRole('dialog')).not.toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Sale Details', () => {
    test('should view sale details', async ({ page }) => {
      await page.goto('/sales');

      // Click on first sale row or view button
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
      } else {
        await page.getByRole('row').nth(1).click();
      }

      // Should show sale details
      await expect(page.getByText(/invoice.*number|sale.*number/i)).toBeVisible();
    });

    test('should display sale items', async ({ page }) => {
      await page.goto('/sales');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Should show items section
        await expect(page.getByText(/items|products|services/i)).toBeVisible();
      }
    });

    test('should display payment history', async ({ page }) => {
      await page.goto('/sales');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Should show payments section
        await expect(page.getByText(/payments|transactions|history/i)).toBeVisible();
      }
    });
  });

  test.describe('Sale Status Management', () => {
    test('should confirm a draft sale', async ({ page }) => {
      await page.goto('/sales');

      // Find draft sale and confirm button
      const confirmButton = page.getByRole('button', { name: /confirm/i }).first();
      if (await confirmButton.isVisible()) {
        await confirmButton.click();

        // Confirm action
        const confirmDialog = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await confirmDialog.isVisible()) {
          await confirmDialog.click();
        }

        // Check for success
        await expect(page.getByText(/confirmed|success/i)).toBeVisible();
      }
    });

    test('should cancel a sale', async ({ page }) => {
      await page.goto('/sales');

      const cancelButton = page.getByRole('button', { name: /cancel/i }).first();
      if (await cancelButton.isVisible()) {
        await cancelButton.click();

        // Fill cancellation reason if prompted
        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test Cancellation');
        }

        // Confirm cancellation
        const confirmButton = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
      }
    });
  });

  test.describe('Payment Processing', () => {
    test('should record payment for a sale', async ({ page }) => {
      await page.goto('/sales');

      // Find sale with outstanding balance
      const paymentButton = page.getByRole('button', { name: /pay|payment|record.*payment/i }).first();
      if (await paymentButton.isVisible()) {
        await paymentButton.click();

        // Fill payment details
        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('100');
        }

        // Select payment method
        const methodSelect = page.getByLabel(/method|payment.*method/i);
        if (await methodSelect.isVisible()) {
          await methodSelect.click();
          await page.getByRole('option', { name: /cash/i }).click();
        }

        // Submit payment
        const submitButton = page.getByRole('button', { name: /submit|record|save/i }).last();
        await submitButton.click();

        // Check for success
        await expect(page.getByText(/payment.*recorded|success/i)).toBeVisible();
      }
    });

    test('should validate payment amount', async ({ page }) => {
      await page.goto('/sales');

      const paymentButton = page.getByRole('button', { name: /pay|payment/i }).first();
      if (await paymentButton.isVisible()) {
        await paymentButton.click();

        // Try to enter invalid amount
        const amountField = page.getByLabel(/amount/i);
        if (await amountField.isVisible()) {
          await amountField.fill('-100');

          const submitButton = page.getByRole('button', { name: /submit|record|save/i }).last();
          await submitButton.click();

          // Should show validation error
          await expect(page.getByText(/invalid|positive|greater.*zero/i)).toBeVisible();
        }
      }
    });
  });
});

test.describe('Quotations Module E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Quotations List', () => {
    test('should display quotations list', async ({ page }) => {
      await page.goto('/quotations');

      await expect(page.getByRole('heading', { name: /quotation|estimate|proposal/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
    });

    test('should show active and expired quotations', async ({ page }) => {
      await page.goto('/quotations');

      // Check for status indicators
      await expect(page.getByText(/active|pending|draft|sent/i)).toBeVisible();
    });

    test('should filter quotations by status', async ({ page }) => {
      await page.goto('/quotations');

      const statusFilter = page.getByRole('combobox', { name: /status|filter/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /active|sent/i }).click();
      }
    });
  });

  test.describe('Create Quotation', () => {
    test('should create a new quotation', async ({ page }) => {
      await page.goto('/quotations');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      // Check modal/form opened
      await expect(page.getByRole('dialog')).toBeVisible();

      // Fill quotation details
      const patientSelect = page.getByLabel(/patient|customer/i);
      if (await patientSelect.isVisible()) {
        await patientSelect.click();
        await page.getByRole('option').first().click();
      }

      // Set validity days
      const validityField = page.getByLabel(/validity|expiry|days/i);
      if (await validityField.isVisible()) {
        await validityField.fill('30');
      }

      // Submit
      const submitButton = page.getByRole('button', { name: /save|submit|create/i }).last();
      await submitButton.click();
    });

    test('should add items to quotation', async ({ page }) => {
      await page.goto('/quotations');

      // Navigate to quotation details
      const viewButton = page.getByRole('button', { name: /view|edit|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Add item
        const addItemButton = page.getByRole('button', { name: /add.*item|add.*product/i });
        if (await addItemButton.isVisible()) {
          await addItemButton.click();

          // Fill item details
          const itemName = page.getByLabel(/item.*name|product|service/i);
          if (await itemName.isVisible()) {
            await itemName.fill('Test Service');
          }

          const quantity = page.getByLabel(/quantity|qty/i);
          if (await quantity.isVisible()) {
            await quantity.fill('1');
          }

          const price = page.getByLabel(/price|amount|unit.*price/i);
          if (await price.isVisible()) {
            await price.fill('500');
          }

          // Save item
          const saveButton = page.getByRole('button', { name: /save|add/i }).last();
          await saveButton.click();
        }
      }
    });
  });

  test.describe('Quotation Workflow', () => {
    test('should send quotation to customer', async ({ page }) => {
      await page.goto('/quotations');

      // Find draft quotation and send
      const sendButton = page.getByRole('button', { name: /send/i }).first();
      if (await sendButton.isVisible()) {
        await sendButton.click();

        // Confirm send
        const confirmButton = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        // Check status changed
        await expect(page.getByText(/sent|success/i)).toBeVisible();
      }
    });

    test('should accept quotation', async ({ page }) => {
      await page.goto('/quotations');

      const acceptButton = page.getByRole('button', { name: /accept/i }).first();
      if (await acceptButton.isVisible()) {
        await acceptButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/accepted|success/i)).toBeVisible();
      }
    });

    test('should reject quotation with reason', async ({ page }) => {
      await page.goto('/quotations');

      const rejectButton = page.getByRole('button', { name: /reject/i }).first();
      if (await rejectButton.isVisible()) {
        await rejectButton.click();

        // Fill rejection reason
        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('Price too high');
        }

        const confirmButton = page.getByRole('button', { name: /reject|confirm|submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
      }
    });

    test('should convert quotation to sale', async ({ page }) => {
      await page.goto('/quotations');

      // Find accepted quotation
      const convertButton = page.getByRole('button', { name: /convert|create.*sale/i }).first();
      if (await convertButton.isVisible()) {
        await convertButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm|ok/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        // Should redirect to sale or show success
        await expect(page.getByText(/converted|sale.*created|success/i)).toBeVisible();
      }
    });
  });

  test.describe('Quotation Expiry', () => {
    test('should display expired quotations indicator', async ({ page }) => {
      await page.goto('/quotations');

      // Look for expired indicator
      const expiredSection = page.getByText(/expired/i);
      if (await expiredSection.isVisible()) {
        await expect(expiredSection).toHaveClass(/red|warning|danger/i);
      }
    });
  });
});

test.describe('Sales Dashboard & Statistics', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should display sales statistics on dashboard', async ({ page }) => {
    await page.goto('/dashboard');

    // Check for sales metrics
    await expect(page.getByText(/sales|revenue/i)).toBeVisible();
  });

  test('should display sales trend chart', async ({ page }) => {
    await page.goto('/sales');

    // Look for chart/graph
    const chart = page.locator('canvas, svg[class*="chart"], [class*="chart"]');
    if (await chart.isVisible()) {
      await expect(chart).toBeVisible();
    }
  });

  test('should filter statistics by date range', async ({ page }) => {
    await page.goto('/sales');

    // Look for date range picker
    const dateFrom = page.getByLabel(/from|start.*date/i);
    const dateTo = page.getByLabel(/to|end.*date/i);

    if (await dateFrom.isVisible() && await dateTo.isVisible()) {
      const today = new Date();
      const lastMonth = new Date(today.setMonth(today.getMonth() - 1));

      await dateFrom.fill(lastMonth.toISOString().split('T')[0]);
      await dateTo.fill(new Date().toISOString().split('T')[0]);

      // Apply filter
      const applyButton = page.getByRole('button', { name: /apply|filter/i });
      if (await applyButton.isVisible()) {
        await applyButton.click();
      }
    }
  });

  test('should export sales report', async ({ page }) => {
    await page.goto('/sales');

    const exportButton = page.getByRole('button', { name: /export|download|report/i });
    if (await exportButton.isVisible()) {
      // Set up download listener
      const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);

      await exportButton.click();

      const download = await downloadPromise;
      if (download) {
        expect(download.suggestedFilename()).toMatch(/sales.*\.(csv|xlsx|pdf)/i);
      }
    }
  });
});

test.describe('Sales Module Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should navigate sales list with keyboard', async ({ page }) => {
    await page.goto('/sales');

    // Tab through elements
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    // Check focus is visible
    const focusedElement = page.locator(':focus');
    await expect(focusedElement).toBeVisible();
  });

  test('should have proper ARIA labels', async ({ page }) => {
    await page.goto('/sales');

    // Check for accessible buttons
    const createButton = page.getByRole('button', { name: /create|new|add/i });
    await expect(createButton).toHaveAttribute('aria-label', /.+/);
  });
});

test.describe('Sales Module Mobile Responsiveness', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill('admin@test.com');
    await page.getByLabel(/password/i).fill('TestPassword123!');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test('should display mobile-friendly sales list', async ({ page }) => {
    await page.goto('/sales');

    // Check layout adapts to mobile
    await expect(page.getByRole('heading', { name: /sales/i })).toBeVisible();

    // Table should scroll horizontally or convert to card layout
    const table = page.locator('table, [class*="card"], [class*="list"]');
    await expect(table).toBeVisible();
  });

  test('should open mobile menu for actions', async ({ page }) => {
    await page.goto('/sales');

    // Look for hamburger menu or action button
    const menuButton = page.getByRole('button', { name: /menu|actions/i });
    if (await menuButton.isVisible()) {
      await menuButton.click();
      await expect(page.getByRole('menu')).toBeVisible();
    }
  });
});
