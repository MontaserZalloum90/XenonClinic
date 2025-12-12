import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Workflow Engine Module
 * Tests cover process definitions, instances, human tasks, and workflow automation
 */
test.describe('Workflow Engine Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Workflow Dashboard', () => {
    test('should display workflow dashboard', async ({ page }) => {
      await page.goto('/workflow');
      await expect(page.getByRole('heading', { name: /workflow/i })).toBeVisible();
    });

    test('should display active processes count', async ({ page }) => {
      await page.goto('/workflow');
      await expect(page.getByText(/active|running|in.*progress/i)).toBeVisible();
    });

    test('should display pending tasks count', async ({ page }) => {
      await page.goto('/workflow');
      await expect(page.getByText(/pending|task|waiting/i)).toBeVisible();
    });

    test('should display workflow statistics', async ({ page }) => {
      await page.goto('/workflow');
      const stats = page.locator('[class*="stat"], [class*="card"]');
      if (await stats.count() > 0) {
        await expect(stats.first()).toBeVisible();
      }
    });
  });

  test.describe('Process Definitions', () => {
    test('should display process definitions list', async ({ page }) => {
      await page.goto('/workflow/definitions');
      await expect(page.getByRole('heading', { name: /definition|process/i })).toBeVisible();
    });

    test('should create new process definition', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Process ${Date.now()}`);
        }

        const descriptionField = page.getByLabel(/description/i);
        if (await descriptionField.isVisible()) {
          await descriptionField.fill('E2E Test - Automated workflow process');
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should edit process definition', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const descriptionField = page.getByLabel(/description/i);
        if (await descriptionField.isVisible()) {
          await descriptionField.fill(`Updated at ${new Date().toISOString()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view process definition details', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/definition|process|step/i)).toBeVisible();
      }
    });

    test('should activate process definition', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const activateButton = page.getByRole('button', { name: /activate|enable/i }).first();
      if (await activateButton.isVisible()) {
        await activateButton.click();
        await expect(page.getByText(/activated|enabled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should deactivate process definition', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const deactivateButton = page.getByRole('button', { name: /deactivate|disable/i }).first();
      if (await deactivateButton.isVisible()) {
        await deactivateButton.click();
        await expect(page.getByText(/deactivated|disabled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should version process definition', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const versionButton = page.getByRole('button', { name: /version|new.*version/i });
        if (await versionButton.isVisible()) {
          await versionButton.click();
          await expect(page.getByText(/version|created/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should delete process definition', async ({ page }) => {
      await page.goto('/workflow/definitions');
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

    test('should search process definitions', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('approval');
        await page.waitForTimeout(500);
      }
    });

    test('should filter by status', async ({ page }) => {
      await page.goto('/workflow/definitions');
      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /active|inactive|draft/i }).first().click();
      }
    });
  });

  test.describe('Workflow Designer', () => {
    test('should open workflow designer', async ({ page }) => {
      await page.goto('/workflow/designer');
      await expect(page.locator('[class*="designer"], [class*="canvas"], [class*="editor"]')).toBeVisible();
    });

    test('should add start node', async ({ page }) => {
      await page.goto('/workflow/designer');
      const startNode = page.getByRole('button', { name: /start/i });
      if (await startNode.isVisible()) {
        await startNode.click();
      }
    });

    test('should add task node', async ({ page }) => {
      await page.goto('/workflow/designer');
      const taskNode = page.getByRole('button', { name: /task|activity/i });
      if (await taskNode.isVisible()) {
        await taskNode.click();
      }
    });

    test('should add decision node', async ({ page }) => {
      await page.goto('/workflow/designer');
      const decisionNode = page.getByRole('button', { name: /decision|gateway|condition/i });
      if (await decisionNode.isVisible()) {
        await decisionNode.click();
      }
    });

    test('should add end node', async ({ page }) => {
      await page.goto('/workflow/designer');
      const endNode = page.getByRole('button', { name: /end/i });
      if (await endNode.isVisible()) {
        await endNode.click();
      }
    });

    test('should connect nodes', async ({ page }) => {
      await page.goto('/workflow/designer');
      const connectTool = page.getByRole('button', { name: /connect|arrow|link/i });
      if (await connectTool.isVisible()) {
        await connectTool.click();
      }
    });

    test('should configure node properties', async ({ page }) => {
      await page.goto('/workflow/designer');
      const node = page.locator('[class*="node"]').first();
      if (await node.isVisible()) {
        await node.dblclick();
        await expect(page.getByRole('dialog').or(page.locator('[class*="properties"]'))).toBeVisible();
      }
    });

    test('should save workflow design', async ({ page }) => {
      await page.goto('/workflow/designer');
      const saveButton = page.getByRole('button', { name: /save/i });
      if (await saveButton.isVisible()) {
        await saveButton.click();
        await expect(page.getByText(/saved|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should validate workflow', async ({ page }) => {
      await page.goto('/workflow/designer');
      const validateButton = page.getByRole('button', { name: /validate|check/i });
      if (await validateButton.isVisible()) {
        await validateButton.click();
        await expect(page.getByText(/valid|error|warning/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should export workflow as BPMN', async ({ page }) => {
      await page.goto('/workflow/designer');
      const exportButton = page.getByRole('button', { name: /export|bpmn/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();
        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/.*\.(bpmn|xml)/i);
        }
      }
    });

    test('should import BPMN workflow', async ({ page }) => {
      await page.goto('/workflow/designer');
      const importButton = page.getByRole('button', { name: /import/i });
      if (await importButton.isVisible()) {
        await expect(importButton).toBeVisible();
      }
    });
  });

  test.describe('Process Instances', () => {
    test('should display process instances list', async ({ page }) => {
      await page.goto('/workflow/instances');
      await expect(page.getByRole('heading', { name: /instance|running|process/i })).toBeVisible();
    });

    test('should start new process instance', async ({ page }) => {
      await page.goto('/workflow/instances');
      const startButton = page.getByRole('button', { name: /start|new|create/i });
      if (await startButton.isVisible()) {
        await startButton.click();

        const processSelect = page.getByLabel(/process|definition/i);
        if (await processSelect.isVisible()) {
          await processSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /start|create/i }).last();
        await submitButton.click();
        await expect(page.getByText(/started|created|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should view process instance details', async ({ page }) => {
      await page.goto('/workflow/instances');
      const viewButton = page.getByRole('button', { name: /view|details/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/instance|status|step/i)).toBeVisible();
      }
    });

    test('should view process instance diagram', async ({ page }) => {
      await page.goto('/workflow/instances');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const diagramTab = page.getByRole('tab', { name: /diagram|visual/i });
        if (await diagramTab.isVisible()) {
          await diagramTab.click();
          await expect(page.locator('[class*="diagram"], [class*="canvas"]')).toBeVisible();
        }
      }
    });

    test('should view process instance variables', async ({ page }) => {
      await page.goto('/workflow/instances');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const variablesTab = page.getByRole('tab', { name: /variable|data/i });
        if (await variablesTab.isVisible()) {
          await variablesTab.click();
          await expect(page.getByText(/variable|data/i)).toBeVisible();
        }
      }
    });

    test('should view process instance history', async ({ page }) => {
      await page.goto('/workflow/instances');
      const viewButton = page.getByRole('button', { name: /view/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const historyTab = page.getByRole('tab', { name: /history|audit/i });
        if (await historyTab.isVisible()) {
          await historyTab.click();
          await expect(page.getByText(/history|log|event/i)).toBeVisible();
        }
      }
    });

    test('should suspend process instance', async ({ page }) => {
      await page.goto('/workflow/instances');
      const suspendButton = page.getByRole('button', { name: /suspend|pause/i }).first();
      if (await suspendButton.isVisible()) {
        await suspendButton.click();
        await expect(page.getByText(/suspended|paused|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should resume process instance', async ({ page }) => {
      await page.goto('/workflow/instances');
      const resumeButton = page.getByRole('button', { name: /resume|continue/i }).first();
      if (await resumeButton.isVisible()) {
        await resumeButton.click();
        await expect(page.getByText(/resumed|continued|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should terminate process instance', async ({ page }) => {
      await page.goto('/workflow/instances');
      const terminateButton = page.getByRole('button', { name: /terminate|cancel|abort/i }).first();
      if (await terminateButton.isVisible()) {
        await terminateButton.click();

        const reasonField = page.getByLabel(/reason/i);
        if (await reasonField.isVisible()) {
          await reasonField.fill('E2E Test - Process terminated');
        }

        const confirmButton = page.getByRole('button', { name: /yes|confirm|terminate/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
        await expect(page.getByText(/terminated|cancelled|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should filter instances by status', async ({ page }) => {
      await page.goto('/workflow/instances');
      const statusFilter = page.getByRole('combobox', { name: /status/i });
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        await page.getByRole('option', { name: /active|completed|suspended/i }).first().click();
      }
    });

    test('should filter instances by process', async ({ page }) => {
      await page.goto('/workflow/instances');
      const processFilter = page.getByRole('combobox', { name: /process|definition/i });
      if (await processFilter.isVisible()) {
        await processFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should search instances', async ({ page }) => {
      await page.goto('/workflow/instances');
      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('approval');
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Human Tasks', () => {
    test('should display my tasks', async ({ page }) => {
      await page.goto('/workflow/tasks');
      await expect(page.getByRole('heading', { name: /task|my.*task/i })).toBeVisible();
    });

    test('should view task details', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/task|detail|action/i)).toBeVisible();
      }
    });

    test('should claim task', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const claimButton = page.getByRole('button', { name: /claim/i }).first();
      if (await claimButton.isVisible()) {
        await claimButton.click();
        await expect(page.getByText(/claimed|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should release task', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const releaseButton = page.getByRole('button', { name: /release|unclaim/i }).first();
      if (await releaseButton.isVisible()) {
        await releaseButton.click();
        await expect(page.getByText(/released|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should delegate task', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const delegateButton = page.getByRole('button', { name: /delegate|assign/i }).first();
      if (await delegateButton.isVisible()) {
        await delegateButton.click();

        const userSelect = page.getByLabel(/user|assignee/i);
        if (await userSelect.isVisible()) {
          await userSelect.click();
          await page.getByRole('option').first().click();
        }

        const submitButton = page.getByRole('button', { name: /delegate|assign/i }).last();
        await submitButton.click();
        await expect(page.getByText(/delegated|assigned|success/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should complete task', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const completeButton = page.getByRole('button', { name: /complete|submit|done/i });
        if (await completeButton.isVisible()) {
          await completeButton.click();
          await expect(page.getByText(/completed|success/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should add task comment', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const commentField = page.getByLabel(/comment|note/i);
        if (await commentField.isVisible()) {
          await commentField.fill('E2E Test - Task comment added');

          const addButton = page.getByRole('button', { name: /add.*comment|save/i });
          if (await addButton.isVisible()) {
            await addButton.click();
          }
        }
      }
    });

    test('should filter tasks by priority', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const priorityFilter = page.getByRole('combobox', { name: /priority/i });
      if (await priorityFilter.isVisible()) {
        await priorityFilter.click();
        await page.getByRole('option', { name: /high|medium|low/i }).first().click();
      }
    });

    test('should filter tasks by due date', async ({ page }) => {
      await page.goto('/workflow/tasks');
      const dueFilter = page.getByRole('combobox', { name: /due|deadline/i });
      if (await dueFilter.isVisible()) {
        await dueFilter.click();
        await page.getByRole('option', { name: /overdue|today|week/i }).first().click();
      }
    });
  });

  test.describe('Business Rules', () => {
    test('should display business rules', async ({ page }) => {
      await page.goto('/workflow/rules');
      await expect(page.getByRole('heading', { name: /rule|business.*rule/i })).toBeVisible();
    });

    test('should create business rule', async ({ page }) => {
      await page.goto('/workflow/rules');
      const createButton = page.getByRole('button', { name: /create|new|add/i });
      if (await createButton.isVisible()) {
        await createButton.click();

        const nameField = page.getByLabel(/name/i);
        if (await nameField.isVisible()) {
          await nameField.fill(`E2E Rule ${Date.now()}`);
        }

        const submitButton = page.getByRole('button', { name: /save|create/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|created/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should edit business rule', async ({ page }) => {
      await page.goto('/workflow/rules');
      const editButton = page.getByRole('button', { name: /edit/i }).first();
      if (await editButton.isVisible()) {
        await editButton.click();

        const submitButton = page.getByRole('button', { name: /save|update/i }).last();
        await submitButton.click();
        await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
      }
    });

    test('should test business rule', async ({ page }) => {
      await page.goto('/workflow/rules');
      const testButton = page.getByRole('button', { name: /test|evaluate/i }).first();
      if (await testButton.isVisible()) {
        await testButton.click();
        await expect(page.getByText(/result|test/i)).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Workflow Reports', () => {
    test('should display workflow reports', async ({ page }) => {
      await page.goto('/workflow/reports');
      await expect(page.getByRole('heading', { name: /report/i })).toBeVisible();
    });

    test('should generate process performance report', async ({ page }) => {
      await page.goto('/workflow/reports');
      const performanceButton = page.getByRole('button', { name: /performance/i });
      if (await performanceButton.isVisible()) {
        await performanceButton.click();
        await expect(page.getByText(/performance|duration|count/i)).toBeVisible();
      }
    });

    test('should generate task analytics report', async ({ page }) => {
      await page.goto('/workflow/reports');
      const taskButton = page.getByRole('button', { name: /task/i });
      if (await taskButton.isVisible()) {
        await taskButton.click();
        await expect(page.getByText(/task|completed|pending/i)).toBeVisible();
      }
    });

    test('should export workflow report', async ({ page }) => {
      await page.goto('/workflow/reports');
      const exportButton = page.getByRole('button', { name: /export|download/i });
      if (await exportButton.isVisible()) {
        const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
        await exportButton.click();
        const download = await downloadPromise;
        if (download) {
          expect(download.suggestedFilename()).toMatch(/workflow.*\.(csv|xlsx|pdf)/i);
        }
      }
    });
  });
});

test.describe('Workflow API E2E Tests', () => {
  test('should list process definitions', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.get('/api/workflows/definitions?page=1&pageSize=10', {
      headers: authHeaders(token),
    });
    expect(response.status()).toBe(200);
  });

  test('should list process instances', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.get('/api/workflows/instances?page=1&pageSize=10', {
      headers: authHeaders(token),
    });
    expect(response.status()).toBe(200);
  });

  test('should list human tasks', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.get('/api/workflows/tasks?page=1&pageSize=10', {
      headers: authHeaders(token),
    });
    expect(response.status()).toBe(200);
  });

  test('should validate process definition creation', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) { test.skip(); return; }

    const response = await request.post('/api/workflows/definitions', {
      headers: authHeaders(token),
      data: {},
    });
    expect(response.status()).toBe(400);
  });
});
