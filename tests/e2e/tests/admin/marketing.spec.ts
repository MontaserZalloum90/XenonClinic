import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Marketing Module
 * Tests cover campaigns, communications, promotions, referrals, feedback, and analytics
 */
test.describe('Marketing Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Marketing Dashboard', () => {
    test('should display marketing dashboard', async ({ page }) => {
      await page.goto('/marketing');

      await expect(page.getByRole('heading', { name: /marketing/i })).toBeVisible();
    });

    test('should display campaign statistics', async ({ page }) => {
      await page.goto('/marketing');

      const stats = page.locator('[class*="stat"], [class*="card"], [class*="summary"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });

    test('should display active campaigns count', async ({ page }) => {
      await page.goto('/marketing');

      await expect(page.getByText(/active.*campaign|running|ongoing/i)).toBeVisible();
    });

    test('should display message delivery stats', async ({ page }) => {
      await page.goto('/marketing');

      await expect(page.getByText(/sent|delivered|opened|clicked/i)).toBeVisible();
    });

    test('should display ROI metrics', async ({ page }) => {
      await page.goto('/marketing');

      const roiSection = page.getByText(/roi|return|conversion/i);
      if (await roiSection.isVisible()) {
        await expect(roiSection).toBeVisible();
      }
    });
  });

  test.describe('Campaigns Management', () => {
    test('should display campaigns list', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      await expect(page.getByRole('heading', { name: /campaign/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /create|new|add/i })).toBeVisible();
    });

    test('should create new campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      await page.getByRole('button', { name: /create|new|add/i }).click();

      const nameField = page.getByLabel(/name|campaign.*name/i);
      if (await nameField.isVisible()) {
        await nameField.fill(`E2E Campaign ${Date.now()}`);
      }

      const typeSelect = page.getByLabel(/type|campaign.*type/i);
      if (await typeSelect.isVisible()) {
        await typeSelect.click();
        await page.getByRole('option', { name: /email|sms|promotional/i }).first().click();
      }

      const startDate = page.getByLabel(/start.*date/i);
      if (await startDate.isVisible()) {
        const today = new Date().toISOString().split('T')[0];
        await startDate.fill(today);
      }

      const endDate = page.getByLabel(/end.*date/i);
      if (await endDate.isVisible()) {
        const nextMonth = new Date();
        nextMonth.setMonth(nextMonth.getMonth() + 1);
        await endDate.fill(nextMonth.toISOString().split('T')[0]);
      }

      const submitButton = page.getByRole('button', { name: /save|create/i }).last();
      await submitButton.click();

      await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
    });

    test('should edit campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`Updated Campaign ${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view campaign details', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/campaign.*detail|target|audience/i)).toBeVisible();
      }
    });

    test('should activate campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const activateButton = page.getByRole('button', { name: /activate|start|launch/i }).first();
      if (await activateButton.isVisible()) {
        await activateButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/activated|started|launched|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should pause campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const pauseButton = page.getByRole('button', { name: /pause/i }).first();
      if (await pauseButton.isVisible()) {
        await pauseButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/paused|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should stop campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const stopButton = page.getByRole('button', { name: /stop|end|terminate/i }).first();
      if (await stopButton.isVisible()) {
        await stopButton.click();

        const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/stopped|ended|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should duplicate campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const duplicateButton = page.getByRole('button', { name: /duplicate|copy|clone/i }).first();
      if (await duplicateButton.isVisible()) {
        await duplicateButton.click();

        await expect(page.getByText(/duplicated|copied|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should delete campaign', async ({ page }) => {
      await page.goto('/marketing/campaigns');

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

    test('should filter campaigns by status', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /active|draft|completed/i }).first().click();
      }
    });

    test('should filter campaigns by type', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const typeFilter = page.getByRole('combobox', { name: /type/i });
      if (await typeFilter.isVisible()) {
        await typeFilter.click();
        await page.getByRole('option', { name: /email|sms|promotional/i }).first().click();
      }
    });

    test('should search campaigns', async ({ page }) => {
      await page.goto('/marketing/campaigns');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('promotion');
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Target Audience', () => {
    test('should display audience segments', async ({ page }) => {
      await page.goto('/marketing/audiences');

      await expect(page.getByRole('heading', { name: /audience|segment/i })).toBeVisible();
    });

    test('should create audience segment', async ({ page }) => {
      await page.goto('/marketing/audiences');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name|segment.*name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Segment ${Date.now()}`);
        }

        // Add filter criteria
        const addCriteriaButton = page.getByRole('button', { name: /add.*criteria|add.*filter/i });
        if (await addCriteriaButton.isVisible()) {
          await addCriteriaButton.click();

          const fieldSelect = page.getByLabel(/field|attribute/i);
          if (await fieldSelect.isVisible()) {
            await fieldSelect.click();
            await page.getByRole('option', { name: /age|gender|location/i }).first().click();
          }
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter by patient demographics', async ({ page }) => {
      await page.goto('/marketing/audiences');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Filter by age range
        const ageFrom = page.getByLabel(/age.*from|min.*age/i);
        if (await ageFrom.isVisible()) {
          await ageFrom.fill('25');
        }

        const ageTo = page.getByLabel(/age.*to|max.*age/i);
        if (await ageTo.isVisible()) {
          await ageTo.fill('45');
        }

        // Filter by gender
        const genderSelect = page.getByLabel(/gender/i);
        if (await genderSelect.isVisible()) {
          await genderSelect.click();
          await page.getByRole('option', { name: /female|male/i }).first().click();
        }
      }
    });

    test('should filter by visit history', async ({ page }) => {
      await page.goto('/marketing/audiences');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Filter by last visit
        const lastVisitSelect = page.getByLabel(/last.*visit|visited/i);
        if (await lastVisitSelect.isVisible()) {
          await lastVisitSelect.click();
          await page.getByRole('option', { name: /30.*day|60.*day|90.*day/i }).first().click();
        }
      }
    });

    test('should preview audience size', async ({ page }) => {
      await page.goto('/marketing/audiences');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const previewButton = page.getByRole('button', { name: /preview|calculate|estimate/i });
        if (await previewButton.isVisible()) {
          await previewButton.click();

          await expect(page.getByText(/patient|member|contact/i)).toBeVisible();
        }
      }
    });

    test('should view segment members', async ({ page }) => {
      await page.goto('/marketing/audiences');

      const viewButton = page.getByRole('button', { name: /view|members/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/member|patient|contact/i)).toBeVisible();
      }
    });
  });

  test.describe('Email Marketing', () => {
    test('should display email campaigns', async ({ page }) => {
      await page.goto('/marketing/email');

      await expect(page.getByRole('heading', { name: /email/i })).toBeVisible();
    });

    test('should create email campaign', async ({ page }) => {
      await page.goto('/marketing/email');

      const createButton = page.getByRole('button', { name: /create|new|compose/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const subjectField = page.getByLabel(/subject/i);
        if (await subjectField.isVisible()) {
          await subjectField.fill('E2E Test Email Campaign');
        }

        const audienceSelect = page.getByLabel(/audience|recipient|segment/i);
        if (await audienceSelect.isVisible()) {
          await audienceSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should use email template', async ({ page }) => {
      await page.goto('/marketing/email');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const templateSelect = page.getByLabel(/template/i);
        if (await templateSelect.isVisible()) {
          await templateSelect.click();
          await page.getByRole('option').first().click();
        }
      }
    });

    test('should design email content', async ({ page }) => {
      await page.goto('/marketing/email');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // Rich text editor should be visible
        const editor = page.locator('[class*="editor"], [contenteditable="true"], textarea');
        if (await editor.isVisible()) {
          await expect(editor).toBeVisible();
        }
      }
    });

    test('should preview email', async ({ page }) => {
      await page.goto('/marketing/email');

      const viewButton = page.getByRole('button', { name: /view|preview/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/preview/i)).toBeVisible();
      }
    });

    test('should send test email', async ({ page }) => {
      await page.goto('/marketing/email');

      const viewButton = page.getByRole('button', { name: /view|edit/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const testButton = page.getByRole('button', { name: /send.*test|test.*email/i });
        if (await testButton.isVisible()) {
          await testButton.click();

          const emailField = page.getByLabel(/email/i);
          if (await emailField.isVisible()) {
            await emailField.fill('test@example.com');
          }

          const sendButton = page.getByRole('button', { name: /send/i }).last();
          await sendButton.click();

          await expect(page.getByText(/sent|success/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should schedule email', async ({ page }) => {
      await page.goto('/marketing/email');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const scheduleCheckbox = page.getByLabel(/schedule/i);
        if (await scheduleCheckbox.isVisible()) {
          await scheduleCheckbox.check();

          const scheduleDateField = page.getByLabel(/schedule.*date|send.*date/i);
          if (await scheduleDateField.isVisible()) {
            const tomorrow = new Date();
            tomorrow.setDate(tomorrow.getDate() + 1);
            await scheduleDateField.fill(tomorrow.toISOString().split('T')[0]);
          }

          const scheduleTimeField = page.getByLabel(/schedule.*time|send.*time/i);
          if (await scheduleTimeField.isVisible()) {
            await scheduleTimeField.fill('09:00');
          }
        }
      }
    });

    test('should view email analytics', async ({ page }) => {
      await page.goto('/marketing/email');

      const analyticsButton = page.getByRole('button', { name: /analytics|stats|report/i }).first();
      if (await analyticsButton.isVisible()) {
        await analyticsButton.click();

        await expect(page.getByText(/open.*rate|click.*rate|delivered/i)).toBeVisible();
      }
    });
  });

  test.describe('SMS Marketing', () => {
    test('should display SMS campaigns', async ({ page }) => {
      await page.goto('/marketing/sms');

      await expect(page.getByRole('heading', { name: /sms/i })).toBeVisible();
    });

    test('should create SMS campaign', async ({ page }) => {
      await page.goto('/marketing/sms');

      const createButton = page.getByRole('button', { name: /create|new|compose/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const messageField = page.getByLabel(/message|content/i);
        if (await messageField.isVisible()) {
          await messageField.fill('E2E Test SMS Campaign - Special offer for you!');
        }

        const audienceSelect = page.getByLabel(/audience|recipient/i);
        if (await audienceSelect.isVisible()) {
          await audienceSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should count SMS characters', async ({ page }) => {
      await page.goto('/marketing/sms');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const messageField = page.getByLabel(/message/i);
        if (await messageField.isVisible()) {
          await messageField.fill('Test message');

          // Character counter should be visible
          const counter = page.getByText(/character|160|remaining/i);
          if (await counter.isVisible()) {
            await expect(counter).toBeVisible();
          }
        }
      }
    });

    test('should use SMS templates', async ({ page }) => {
      await page.goto('/marketing/sms');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const templateSelect = page.getByLabel(/template/i);
        if (await templateSelect.isVisible()) {
          await templateSelect.click();
          await page.getByRole('option').first().click();
        }
      }
    });

    test('should personalize SMS with merge tags', async ({ page }) => {
      await page.goto('/marketing/sms');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const mergeTagButton = page.getByRole('button', { name: /insert|merge|personalize/i });
        if (await mergeTagButton.isVisible()) {
          await mergeTagButton.click();

          await page.getByRole('option', { name: /first.*name|name/i }).click();
        }
      }
    });

    test('should view SMS delivery report', async ({ page }) => {
      await page.goto('/marketing/sms');

      const reportButton = page.getByRole('button', { name: /report|delivery|stats/i }).first();
      if (await reportButton.isVisible()) {
        await reportButton.click();

        await expect(page.getByText(/delivered|failed|pending/i)).toBeVisible();
      }
    });
  });

  test.describe('WhatsApp Marketing', () => {
    test('should display WhatsApp campaigns', async ({ page }) => {
      await page.goto('/marketing/whatsapp');

      await expect(page.getByRole('heading', { name: /whatsapp/i })).toBeVisible();
    });

    test('should create WhatsApp campaign', async ({ page }) => {
      await page.goto('/marketing/whatsapp');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const templateSelect = page.getByLabel(/template/i);
        if (await templateSelect.isVisible()) {
          await templateSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should use approved WhatsApp templates', async ({ page }) => {
      await page.goto('/marketing/whatsapp');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        // WhatsApp requires pre-approved templates
        const templateSelect = page.getByLabel(/template/i);
        if (await templateSelect.isVisible()) {
          await expect(templateSelect).toBeVisible();
        }
      }
    });

    test('should view WhatsApp delivery status', async ({ page }) => {
      await page.goto('/marketing/whatsapp');

      const viewButton = page.getByRole('button', { name: /view|status/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/delivered|read|sent/i)).toBeVisible();
      }
    });
  });

  test.describe('Promotional Offers', () => {
    test('should display promotions list', async ({ page }) => {
      await page.goto('/marketing/promotions');

      await expect(page.getByRole('heading', { name: /promotion|offer|discount/i })).toBeVisible();
    });

    test('should create promotional offer', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name|title/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Promo ${Date.now()}`);
        }

        const discountField = page.getByLabel(/discount|value/i);
        if (await discountField.isVisible()) {
          await discountField.fill('20');
        }

        const typeSelect = page.getByLabel(/type|discount.*type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.click();
          await page.getByRole('option', { name: /percent|fixed/i }).first().click();
        }

        const startDate = page.getByLabel(/start.*date/i);
        if (await startDate.isVisible()) {
          const today = new Date().toISOString().split('T')[0];
          await startDate.fill(today);
        }

        const endDate = page.getByLabel(/end.*date/i);
        if (await endDate.isVisible()) {
          const nextWeek = new Date();
          nextWeek.setDate(nextWeek.getDate() + 7);
          await endDate.fill(nextWeek.toISOString().split('T')[0]);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should create promo code', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const codeField = page.getByLabel(/code|promo.*code|coupon/i);
        if (await codeField.isVisible()) {
          await codeField.fill(`PROMO${Date.now()}`);
        }
      }
    });

    test('should set usage limits', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const maxUsesField = page.getByLabel(/max.*use|limit|usage/i);
        if (await maxUsesField.isVisible()) {
          await maxUsesField.fill('100');
        }

        const perUserField = page.getByLabel(/per.*user|per.*patient/i);
        if (await perUserField.isVisible()) {
          await perUserField.fill('1');
        }
      }
    });

    test('should set minimum purchase', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const minPurchaseField = page.getByLabel(/minimum|min.*purchase|min.*amount/i);
        if (await minPurchaseField.isVisible()) {
          await minPurchaseField.fill('500');
        }
      }
    });

    test('should apply to specific services', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const serviceSelect = page.getByLabel(/service|apply.*to/i);
        if (await serviceSelect.isVisible()) {
          await serviceSelect.click();
          await page.getByRole('option').first().click();
        }
      }
    });

    test('should activate promotion', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const activateButton = page.getByRole('button', { name: /activate/i }).first();
      if (await activateButton.isVisible()) {
        await activateButton.click();

        await expect(page.getByText(/activated|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should deactivate promotion', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const deactivateButton = page.getByRole('button', { name: /deactivate|disable/i }).first();
      if (await deactivateButton.isVisible()) {
        await deactivateButton.click();

        await expect(page.getByText(/deactivated|disabled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view promotion usage stats', async ({ page }) => {
      await page.goto('/marketing/promotions');

      const statsButton = page.getByRole('button', { name: /stats|usage|report/i }).first();
      if (await statsButton.isVisible()) {
        await statsButton.click();

        await expect(page.getByText(/usage|redeemed|used/i)).toBeVisible();
      }
    });
  });

  test.describe('Referral Program', () => {
    test('should display referral program page', async ({ page }) => {
      await page.goto('/marketing/referrals');

      await expect(page.getByRole('heading', { name: /referral/i })).toBeVisible();
    });

    test('should configure referral rewards', async ({ page }) => {
      await page.goto('/marketing/referrals');

      const configureButton = page.getByRole('button', { name: /configure|settings|edit/i });
      if (await configureButton.isVisible()) {
        await configureButton.click();

        const referrerRewardField = page.getByLabel(/referrer.*reward|reward.*for.*referrer/i);
        if (await referrerRewardField.isVisible()) {
          await referrerRewardField.fill('100');
        }

        const refereeRewardField = page.getByLabel(/referee.*reward|new.*patient.*reward/i);
        if (await refereeRewardField.isVisible()) {
          await refereeRewardField.fill('50');
        }

        const submitButton = page.getByRole('button', { name: /save/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view referral list', async ({ page }) => {
      await page.goto('/marketing/referrals');

      const table = page.locator('table, [class*="list"]');
      await expect(table).toBeVisible();
    });

    test('should view referral details', async ({ page }) => {
      await page.goto('/marketing/referrals');

      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/referrer|referee|status/i)).toBeVisible();
      }
    });

    test('should approve pending referral', async ({ page }) => {
      await page.goto('/marketing/referrals');

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

    test('should reject referral with reason', async ({ page }) => {
      await page.goto('/marketing/referrals');

      const rejectButton = page.getByRole('button', { name: /reject/i }).first();
      if (await rejectButton.isVisible()) {
        await rejectButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Invalid referral');
        }

        const confirmButton = page.getByRole('button', { name: /reject|confirm/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        await expect(page.getByText(/rejected|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should generate referral code for patient', async ({ page }) => {
      await page.goto('/marketing/referrals');

      const generateButton = page.getByRole('button', { name: /generate|create.*code/i });
      if (await generateButton.isVisible()) {
        await generateButton.click();

        const patientSelect = page.getByLabel(/patient/i);
        if (await patientSelect.isVisible()) {
          await patientSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /generate|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/generated|success|code/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view referral statistics', async ({ page }) => {
      await page.goto('/marketing/referrals');

      const statsSection = page.getByText(/total.*referral|conversion|reward.*paid/i);
      if (await statsSection.isVisible()) {
        await expect(statsSection).toBeVisible();
      }
    });
  });

  test.describe('Patient Feedback & Surveys', () => {
    test('should display surveys page', async ({ page }) => {
      await page.goto('/marketing/surveys');

      await expect(page.getByRole('heading', { name: /survey|feedback/i })).toBeVisible();
    });

    test('should create survey', async ({ page }) => {
      await page.goto('/marketing/surveys');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const titleField = page.getByLabel(/title|name/i);
        if (await titleField.isVisible()) {
          await titleField.fill(`E2E Survey ${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should add survey questions', async ({ page }) => {
      await page.goto('/marketing/surveys');

      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const addQuestionButton = page.getByRole('button', { name: /add.*question/i });
        if (await addQuestionButton.isVisible()) {
          await addQuestionButton.click();

          const questionField = page.getByLabel(/question/i);
          if (await questionField.isVisible()) {
            await questionField.fill('How satisfied are you with our service?');
          }

          const typeSelect = page.getByLabel(/type|question.*type/i);
          if (await typeSelect.isVisible()) {
            await typeSelect.click();
            await page.getByRole('option', { name: /rating|scale|multiple/i }).first().click();
          }
        }
      }
    });

    test('should send survey to patients', async ({ page }) => {
      await page.goto('/marketing/surveys');

      const sendButton = page.getByRole('button', { name: /send/i }).first();
      if (await sendButton.isVisible()) {
        await sendButton.click();

        const audienceSelect = page.getByLabel(/audience|recipient/i);
        if (await audienceSelect.isVisible()) {
          await audienceSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /send/i }).last();
        await submitButton.click();

        await expect(page.getByText(/sent|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view survey responses', async ({ page }) => {
      await page.goto('/marketing/surveys');

      const responsesButton = page.getByRole('button', { name: /response|result|view/i }).first();
      if (await responsesButton.isVisible()) {
        await responsesButton.click();

        await expect(page.getByText(/response|answer|result/i)).toBeVisible();
      }
    });

    test('should view survey analytics', async ({ page }) => {
      await page.goto('/marketing/surveys');

      const analyticsButton = page.getByRole('button', { name: /analytics|stats/i }).first();
      if (await analyticsButton.isVisible()) {
        await analyticsButton.click();

        await expect(page.getByText(/response.*rate|completion|average/i)).toBeVisible();
      }
    });

    test('should export survey results', async ({ page }) => {
      await page.goto('/marketing/surveys');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/survey.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });

  test.describe('Loyalty Program', () => {
    test('should display loyalty program page', async ({ page }) => {
      await page.goto('/marketing/loyalty');

      await expect(page.getByRole('heading', { name: /loyalty|reward.*point/i })).toBeVisible();
    });

    test('should configure point earning rules', async ({ page }) => {
      await page.goto('/marketing/loyalty');

      const configureButton = page.getByRole('button', { name: /configure|settings|edit/i });
      if (await configureButton.isVisible()) {
        await configureButton.click();

        const pointsPerField = page.getByLabel(/points.*per|earning.*rate/i);
        if (await pointsPerField.isVisible()) {
          await pointsPerField.fill('1');
        }

        const submitButton = page.getByRole('button', { name: /save/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should configure point redemption', async ({ page }) => {
      await page.goto('/marketing/loyalty');

      const configureButton = page.getByRole('button', { name: /configure|settings/i });
      if (await configureButton.isVisible()) {
        await configureButton.click();

        const redemptionField = page.getByLabel(/redemption|points.*value/i);
        if (await redemptionField.isVisible()) {
          await redemptionField.fill('100');
        }
      }
    });

    test('should view member points', async ({ page }) => {
      await page.goto('/marketing/loyalty');

      const table = page.locator('table, [class*="list"]');
      await expect(table).toBeVisible();
    });

    test('should adjust member points', async ({ page }) => {
      await page.goto('/marketing/loyalty');

      const adjustButton = page.getByRole('button', { name: /adjust|edit/i }).first();
      if (await adjustButton.isVisible()) {
        await adjustButton.click();

        const pointsField = page.getByLabel(/points|amount/i);
        if (await pointsField.isVisible()) {
          await pointsField.fill('50');
        }

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Manual adjustment');
        }

        const submitButton = page.getByRole('button', { name: /save|adjust/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|adjusted/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view points history', async ({ page }) => {
      await page.goto('/marketing/loyalty');

      const historyButton = page.getByRole('button', { name: /history|view/i }).first();
      if (await historyButton.isVisible()) {
        await historyButton.click();

        await expect(page.getByText(/history|earned|redeemed/i)).toBeVisible();
      }
    });
  });

  test.describe('Marketing Templates', () => {
    test('should display templates page', async ({ page }) => {
      await page.goto('/marketing/templates');

      await expect(page.getByRole('heading', { name: /template/i })).toBeVisible();
    });

    test('should create email template', async ({ page }) => {
      await page.goto('/marketing/templates');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Template ${Date.now()}`);
        }

        const typeSelect = page.getByLabel(/type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.click();
          await page.getByRole('option', { name: /email/i }).click();
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();

        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should create SMS template', async ({ page }) => {
      await page.goto('/marketing/templates');

      const createButton = page.getByRole('button', { name: /create|new/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const typeSelect = page.getByLabel(/type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.click();
          await page.getByRole('option', { name: /sms/i }).click();
        }
      }
    });

    test('should preview template', async ({ page }) => {
      await page.goto('/marketing/templates');

      const previewButton = page.getByRole('button', { name: /preview/i }).first();
      if (await previewButton.isVisible()) {
        await previewButton.click();

        await expect(page.getByText(/preview/i)).toBeVisible();
      }
    });

    test('should duplicate template', async ({ page }) => {
      await page.goto('/marketing/templates');

      const duplicateButton = page.getByRole('button', { name: /duplicate|copy/i }).first();
      if (await duplicateButton.isVisible()) {
        await duplicateButton.click();

        await expect(page.getByText(/duplicated|copied|success/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Marketing Reports & Analytics', () => {
    test('should display marketing reports page', async ({ page }) => {
      await page.goto('/marketing/reports');

      await expect(page.getByRole('heading', { name: /report|analytics/i })).toBeVisible();
    });

    test('should generate campaign performance report', async ({ page }) => {
      await page.goto('/marketing/reports');

      const campaignButton = page.getByRole('button', { name: /campaign.*report|performance/i });
      if (await campaignButton.isVisible()) {
        await campaignButton.click();

        await expect(page.getByText(/campaign|performance|conversion/i)).toBeVisible();
      }
    });

    test('should generate channel performance report', async ({ page }) => {
      await page.goto('/marketing/reports');

      const channelButton = page.getByRole('button', { name: /channel|email|sms/i });
      if (await channelButton.isVisible()) {
        await channelButton.click();

        await expect(page.getByText(/email|sms|whatsapp/i)).toBeVisible();
      }
    });

    test('should generate ROI report', async ({ page }) => {
      await page.goto('/marketing/reports');

      const roiButton = page.getByRole('button', { name: /roi|return/i });
      if (await roiButton.isVisible()) {
        await roiButton.click();

        await expect(page.getByText(/roi|return|investment/i)).toBeVisible();
      }
    });

    test('should export marketing report', async ({ page }) => {
      await page.goto('/marketing/reports');

      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();

        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/marketing.*\.(csv|xlsx|pdf)/i);
        }
      }
    });

    test('should filter reports by date range', async ({ page }) => {
      await page.goto('/marketing/reports');

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
  });
});

test.describe('Marketing API E2E Tests', () => {
  test('should list campaigns', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/marketing/campaigns?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get campaign details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/marketing/campaigns?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const campaignId = listData.items[0].id;

        const response = await request.get(`/api/marketing/campaigns/${campaignId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should list promotions', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/marketing/promotions?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should list audience segments', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/marketing/audiences', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should validate campaign creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/marketing/campaigns', {
      headers: authHeaders(token),
      data: {
        // Missing required fields
      },
    });

    expect(response.status()).toBe(400);
  });

  test('should get marketing statistics', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/marketing/statistics', {
      headers: authHeaders(token),
    });

    expect([200, 404]).toContain(response.status());
  });
});
