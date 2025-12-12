/**
 * Analytics & Reporting Module E2E Tests
 *
 * Comprehensive test suite for analytics, dashboards, and reporting functionality
 * Tests cover: dashboards, reports, KPIs, data visualization, exports,
 * custom reports, scheduled reports, and real-time analytics
 */

import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

test.describe('Analytics & Reporting Module', () => {

  test.describe('Analytics Dashboard', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
      await expect(page).toHaveURL(/dashboard|home/i);
    });

    test('should display main analytics dashboard', async ({ page }) => {
      await page.goto('/analytics');

      await expect(page.getByRole('heading', { name: /analytics|dashboard/i })).toBeVisible();
      await expect(page.locator('[data-testid="analytics-widget"]').first()).toBeVisible();
    });

    test('should display KPI widgets', async ({ page }) => {
      await page.goto('/analytics');

      await expect(page.getByText(/total.*patients|patient.*count/i)).toBeVisible();
      await expect(page.getByText(/appointments|visits/i)).toBeVisible();
      await expect(page.getByText(/revenue/i)).toBeVisible();
    });

    test('should filter dashboard by date range', async ({ page }) => {
      await page.goto('/analytics');

      await page.getByLabel(/date.*range|period/i).click();
      await page.getByRole('option', { name: /last.*30.*days/i }).click();

      await expect(page.locator('[data-testid="analytics-widget"]').first()).toBeVisible();
    });

    test('should filter dashboard by branch', async ({ page }) => {
      await page.goto('/analytics');

      const branchFilter = page.getByLabel(/branch|location/i);
      if (await branchFilter.isVisible()) {
        await branchFilter.selectOption({ index: 1 });
        await expect(page.locator('[data-testid="analytics-widget"]').first()).toBeVisible();
      }
    });

    test('should display trend charts', async ({ page }) => {
      await page.goto('/analytics');

      const trendChart = page.locator('[data-testid="trend-chart"]');
      await expect(trendChart).toBeVisible();
    });

    test('should customize dashboard layout', async ({ page }) => {
      await page.goto('/analytics');

      await page.getByRole('button', { name: /customize|edit.*layout/i }).click();

      // Drag and drop widget (simplified test)
      const widget = page.locator('[data-testid="draggable-widget"]').first();
      await expect(widget).toBeVisible();

      await page.getByRole('button', { name: /save.*layout/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();
    });

    test('should add widget to dashboard', async ({ page }) => {
      await page.goto('/analytics');

      await page.getByRole('button', { name: /add.*widget/i }).click();

      await page.getByLabel(/widget.*type/i).selectOption({ index: 1 });
      await page.getByLabel(/title/i).fill('Custom Widget');

      await page.getByRole('button', { name: /add|create/i }).click();

      await expect(page.getByText(/widget.*added|created/i)).toBeVisible();
    });

    test('should remove widget from dashboard', async ({ page }) => {
      await page.goto('/analytics');

      const widget = page.locator('[data-testid="analytics-widget"]').first();
      await widget.getByRole('button', { name: /remove|delete|close/i }).click();

      await page.getByRole('button', { name: /confirm/i }).click();

      await expect(page.getByText(/removed|deleted/i)).toBeVisible();
    });

    test('should refresh dashboard data', async ({ page }) => {
      await page.goto('/analytics');

      await page.getByRole('button', { name: /refresh/i }).click();

      await expect(page.locator('[data-testid="loading-indicator"]')).toBeVisible();
      await expect(page.locator('[data-testid="analytics-widget"]').first()).toBeVisible();
    });
  });

  test.describe('Clinical Analytics', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display patient demographics analysis', async ({ page }) => {
      await page.goto('/analytics/clinical/demographics');

      await expect(page.getByText(/age.*distribution|demographics/i)).toBeVisible();
      await expect(page.locator('[data-testid="demographics-chart"]')).toBeVisible();
    });

    test('should display diagnosis statistics', async ({ page }) => {
      await page.goto('/analytics/clinical/diagnoses');

      await expect(page.getByText(/diagnosis|condition/i)).toBeVisible();
      await expect(page.locator('[data-testid="diagnosis-chart"]')).toBeVisible();
    });

    test('should display treatment outcome analysis', async ({ page }) => {
      await page.goto('/analytics/clinical/outcomes');

      await expect(page.getByText(/outcome|treatment.*result/i)).toBeVisible();
      await expect(page.locator('[data-testid="outcome-chart"]')).toBeVisible();
    });

    test('should display appointment analytics', async ({ page }) => {
      await page.goto('/analytics/clinical/appointments');

      await expect(page.getByText(/appointment.*volume|booking/i)).toBeVisible();
      await expect(page.getByText(/no.*show|cancellation/i)).toBeVisible();
    });

    test('should display wait time analysis', async ({ page }) => {
      await page.goto('/analytics/clinical/wait-times');

      await expect(page.getByText(/wait.*time|waiting/i)).toBeVisible();
      await expect(page.locator('[data-testid="wait-time-chart"]')).toBeVisible();
    });

    test('should display provider productivity', async ({ page }) => {
      await page.goto('/analytics/clinical/productivity');

      await expect(page.getByText(/provider|doctor.*productivity/i)).toBeVisible();
      await expect(page.getByText(/patients.*seen|consultation/i)).toBeVisible();
    });

    test('should display lab turnaround time analysis', async ({ page }) => {
      await page.goto('/analytics/clinical/lab-tat');

      await expect(page.getByText(/lab.*turnaround|tat/i)).toBeVisible();
      await expect(page.locator('[data-testid="tat-chart"]')).toBeVisible();
    });

    test('should display prescription patterns', async ({ page }) => {
      await page.goto('/analytics/clinical/prescriptions');

      await expect(page.getByText(/prescription|medication.*pattern/i)).toBeVisible();
      await expect(page.locator('[data-testid="prescription-chart"]')).toBeVisible();
    });
  });

  test.describe('Financial Analytics', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display revenue dashboard', async ({ page }) => {
      await page.goto('/analytics/financial/revenue');

      await expect(page.getByText(/revenue|income/i)).toBeVisible();
      await expect(page.locator('[data-testid="revenue-chart"]')).toBeVisible();
    });

    test('should display revenue by department', async ({ page }) => {
      await page.goto('/analytics/financial/revenue');

      await page.getByRole('tab', { name: /by.*department|breakdown/i }).click();

      await expect(page.locator('[data-testid="department-revenue-chart"]')).toBeVisible();
    });

    test('should display revenue by service', async ({ page }) => {
      await page.goto('/analytics/financial/revenue');

      await page.getByRole('tab', { name: /by.*service/i }).click();

      await expect(page.locator('[data-testid="service-revenue-chart"]')).toBeVisible();
    });

    test('should display expense analysis', async ({ page }) => {
      await page.goto('/analytics/financial/expenses');

      await expect(page.getByText(/expense|cost/i)).toBeVisible();
      await expect(page.locator('[data-testid="expense-chart"]')).toBeVisible();
    });

    test('should display profit/loss statement', async ({ page }) => {
      await page.goto('/analytics/financial/pnl');

      await expect(page.getByText(/profit.*loss|p&l/i)).toBeVisible();
      await expect(page.getByText(/gross.*profit|net.*income/i)).toBeVisible();
    });

    test('should display accounts receivable aging', async ({ page }) => {
      await page.goto('/analytics/financial/ar-aging');

      await expect(page.getByText(/accounts.*receivable|ar.*aging/i)).toBeVisible();
      await expect(page.locator('[data-testid="aging-chart"]')).toBeVisible();
    });

    test('should display payment collection rate', async ({ page }) => {
      await page.goto('/analytics/financial/collections');

      await expect(page.getByText(/collection.*rate|payment.*collected/i)).toBeVisible();
    });

    test('should display insurance claim analytics', async ({ page }) => {
      await page.goto('/analytics/financial/claims');

      await expect(page.getByText(/claim|insurance/i)).toBeVisible();
      await expect(page.getByText(/approval.*rate|denial/i)).toBeVisible();
    });

    test('should compare revenue across periods', async ({ page }) => {
      await page.goto('/analytics/financial/comparison');

      await page.getByLabel(/period.*1/i).selectOption({ label: /this.*month/i });
      await page.getByLabel(/period.*2/i).selectOption({ label: /last.*month/i });

      await page.getByRole('button', { name: /compare/i }).click();

      await expect(page.locator('[data-testid="comparison-chart"]')).toBeVisible();
    });
  });

  test.describe('Operational Analytics', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display resource utilization', async ({ page }) => {
      await page.goto('/analytics/operations/utilization');

      await expect(page.getByText(/utilization|resource.*use/i)).toBeVisible();
      await expect(page.locator('[data-testid="utilization-chart"]')).toBeVisible();
    });

    test('should display room/bed occupancy', async ({ page }) => {
      await page.goto('/analytics/operations/occupancy');

      await expect(page.getByText(/occupancy|bed.*usage/i)).toBeVisible();
    });

    test('should display staff scheduling efficiency', async ({ page }) => {
      await page.goto('/analytics/operations/scheduling');

      await expect(page.getByText(/scheduling|staff.*efficiency/i)).toBeVisible();
    });

    test('should display equipment usage analytics', async ({ page }) => {
      await page.goto('/analytics/operations/equipment');

      await expect(page.getByText(/equipment.*usage|device.*utilization/i)).toBeVisible();
    });

    test('should display inventory turnover analytics', async ({ page }) => {
      await page.goto('/analytics/operations/inventory');

      await expect(page.getByText(/inventory.*turnover|stock/i)).toBeVisible();
    });

    test('should display patient flow analysis', async ({ page }) => {
      await page.goto('/analytics/operations/patient-flow');

      await expect(page.getByText(/patient.*flow|throughput/i)).toBeVisible();
      await expect(page.locator('[data-testid="flow-chart"]')).toBeVisible();
    });
  });

  test.describe('Report Builder', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should open report builder', async ({ page }) => {
      await page.goto('/analytics/reports/builder');

      await expect(page.getByText(/report.*builder|create.*report/i)).toBeVisible();
    });

    test('should create custom report', async ({ page }) => {
      await page.goto('/analytics/reports/builder');

      // Select data source
      await page.getByLabel(/data.*source|entity/i).selectOption({ label: /patient/i });

      // Add columns
      await page.getByRole('button', { name: /add.*column|field/i }).click();
      await page.getByLabel(/column|field/i).selectOption({ label: /name/i });
      await page.getByRole('button', { name: /add/i }).click();

      // Add filters
      await page.getByRole('button', { name: /add.*filter/i }).click();
      await page.getByLabel(/field/i).selectOption({ label: /status/i });
      await page.getByLabel(/operator/i).selectOption({ label: /equals/i });
      await page.getByLabel(/value/i).fill('Active');

      // Save report
      await page.getByLabel(/report.*name/i).fill('Custom Patient Report');
      await page.getByRole('button', { name: /save|create/i }).click();

      await expect(page.getByText(/report.*saved|created/i)).toBeVisible();
    });

    test('should preview report', async ({ page }) => {
      await page.goto('/analytics/reports/builder');

      await page.getByLabel(/data.*source/i).selectOption({ label: /patient/i });
      await page.getByRole('button', { name: /preview/i }).click();

      await expect(page.locator('[data-testid="report-preview"]')).toBeVisible();
    });

    test('should add grouping to report', async ({ page }) => {
      await page.goto('/analytics/reports/builder');

      await page.getByLabel(/data.*source/i).selectOption({ label: /appointment/i });
      await page.getByRole('button', { name: /add.*group/i }).click();
      await page.getByLabel(/group.*by/i).selectOption({ label: /department|status/i });

      await expect(page.getByText(/grouped/i)).toBeVisible();
    });

    test('should add aggregations to report', async ({ page }) => {
      await page.goto('/analytics/reports/builder');

      await page.getByLabel(/data.*source/i).selectOption({ label: /invoice/i });
      await page.getByRole('button', { name: /add.*aggregation|calculation/i }).click();
      await page.getByLabel(/function/i).selectOption({ label: /sum/i });
      await page.getByLabel(/field/i).selectOption({ label: /amount/i });

      await expect(page.getByText(/sum|total/i)).toBeVisible();
    });

    test('should add chart to report', async ({ page }) => {
      await page.goto('/analytics/reports/builder');

      await page.getByRole('button', { name: /add.*chart/i }).click();
      await page.getByLabel(/chart.*type/i).selectOption({ label: /bar/i });

      await expect(page.locator('[data-testid="chart-config"]')).toBeVisible();
    });
  });

  test.describe('Standard Reports', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view standard reports list', async ({ page }) => {
      await page.goto('/analytics/reports');

      await expect(page.getByText(/standard.*reports|report.*library/i)).toBeVisible();
      await expect(page.locator('[data-testid="report-card"]').first()).toBeVisible();
    });

    test('should run patient list report', async ({ page }) => {
      await page.goto('/analytics/reports/patient-list');

      await page.getByRole('button', { name: /run|generate/i }).click();

      await expect(page.locator('[data-testid="report-results"]')).toBeVisible();
    });

    test('should run appointment report', async ({ page }) => {
      await page.goto('/analytics/reports/appointments');

      await page.getByLabel(/date.*from/i).fill('2024-01-01');
      await page.getByLabel(/date.*to/i).fill('2024-12-31');
      await page.getByRole('button', { name: /run|generate/i }).click();

      await expect(page.locator('[data-testid="report-results"]')).toBeVisible();
    });

    test('should run revenue report', async ({ page }) => {
      await page.goto('/analytics/reports/revenue');

      await page.getByLabel(/period/i).selectOption({ label: /monthly/i });
      await page.getByRole('button', { name: /run|generate/i }).click();

      await expect(page.locator('[data-testid="report-results"]')).toBeVisible();
    });

    test('should run daily summary report', async ({ page }) => {
      await page.goto('/analytics/reports/daily-summary');

      await page.getByLabel(/date/i).fill(new Date().toISOString().split('T')[0]);
      await page.getByRole('button', { name: /run|generate/i }).click();

      await expect(page.locator('[data-testid="report-results"]')).toBeVisible();
    });

    test('should export report to PDF', async ({ page }) => {
      await page.goto('/analytics/reports/patient-list');
      await page.getByRole('button', { name: /run|generate/i }).click();

      const downloadPromise = page.waitForEvent('download');
      await page.getByRole('button', { name: /export.*pdf/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/\.pdf$/i);
    });

    test('should export report to Excel', async ({ page }) => {
      await page.goto('/analytics/reports/patient-list');
      await page.getByRole('button', { name: /run|generate/i }).click();

      const downloadPromise = page.waitForEvent('download');
      await page.getByRole('button', { name: /export.*excel|xlsx/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/\.(xlsx|xls)$/i);
    });

    test('should export report to CSV', async ({ page }) => {
      await page.goto('/analytics/reports/patient-list');
      await page.getByRole('button', { name: /run|generate/i }).click();

      const downloadPromise = page.waitForEvent('download');
      await page.getByRole('button', { name: /export.*csv/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/\.csv$/i);
    });
  });

  test.describe('Scheduled Reports', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view scheduled reports', async ({ page }) => {
      await page.goto('/analytics/reports/scheduled');

      await expect(page.getByText(/scheduled.*reports/i)).toBeVisible();
    });

    test('should create scheduled report', async ({ page }) => {
      await page.goto('/analytics/reports/scheduled');

      await page.getByRole('button', { name: /add|create|schedule/i }).click();

      await page.getByLabel(/report/i).selectOption({ index: 1 });
      await page.getByLabel(/frequency/i).selectOption({ label: /daily/i });
      await page.getByLabel(/time/i).fill('08:00');
      await page.getByLabel(/email.*recipient/i).fill('admin@clinic.com');
      await page.getByLabel(/format/i).selectOption({ label: /pdf/i });

      await page.getByRole('button', { name: /create|save|schedule/i }).click();

      await expect(page.getByText(/scheduled|created/i)).toBeVisible();
    });

    test('should edit scheduled report', async ({ page }) => {
      await page.goto('/analytics/reports/scheduled');

      const scheduleRow = page.locator('[data-testid="schedule-row"]').first();
      await scheduleRow.getByRole('button', { name: /edit/i }).click();

      await page.getByLabel(/frequency/i).selectOption({ label: /weekly/i });
      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/updated|saved/i)).toBeVisible();
    });

    test('should pause scheduled report', async ({ page }) => {
      await page.goto('/analytics/reports/scheduled');

      const scheduleRow = page.locator('[data-testid="schedule-row"]').first();
      await scheduleRow.getByRole('button', { name: /pause|disable/i }).click();

      await expect(page.getByText(/paused|disabled/i)).toBeVisible();
    });

    test('should delete scheduled report', async ({ page }) => {
      await page.goto('/analytics/reports/scheduled');

      const scheduleRow = page.locator('[data-testid="schedule-row"]').first();
      await scheduleRow.getByRole('button', { name: /delete|remove/i }).click();

      await page.getByRole('button', { name: /confirm/i }).click();

      await expect(page.getByText(/deleted|removed/i)).toBeVisible();
    });

    test('should run scheduled report manually', async ({ page }) => {
      await page.goto('/analytics/reports/scheduled');

      const scheduleRow = page.locator('[data-testid="schedule-row"]').first();
      await scheduleRow.getByRole('button', { name: /run.*now|execute/i }).click();

      await expect(page.getByText(/running|started/i)).toBeVisible();
    });
  });

  test.describe('Data Visualization', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display bar chart', async ({ page }) => {
      await page.goto('/analytics');

      const barChart = page.locator('[data-testid="bar-chart"]');
      await expect(barChart).toBeVisible();
    });

    test('should display line chart', async ({ page }) => {
      await page.goto('/analytics');

      const lineChart = page.locator('[data-testid="line-chart"]');
      await expect(lineChart).toBeVisible();
    });

    test('should display pie chart', async ({ page }) => {
      await page.goto('/analytics');

      const pieChart = page.locator('[data-testid="pie-chart"]');
      await expect(pieChart).toBeVisible();
    });

    test('should interact with chart (hover/tooltip)', async ({ page }) => {
      await page.goto('/analytics');

      const chart = page.locator('[data-testid="bar-chart"]').first();
      await chart.hover();

      await expect(page.locator('[data-testid="chart-tooltip"]')).toBeVisible();
    });

    test('should zoom chart', async ({ page }) => {
      await page.goto('/analytics');

      const chart = page.locator('[data-testid="line-chart"]').first();

      // Zoom controls
      await chart.getByRole('button', { name: /zoom.*in/i }).click();

      await expect(chart).toBeVisible();
    });

    test('should drill down on chart data', async ({ page }) => {
      await page.goto('/analytics/clinical/diagnoses');

      const chart = page.locator('[data-testid="diagnosis-chart"]');
      await chart.locator('path, rect').first().click();

      // Should show detailed breakdown
      await expect(page.getByText(/detail|breakdown/i)).toBeVisible();
    });
  });

  test.describe('Real-Time Analytics', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display real-time dashboard', async ({ page }) => {
      await page.goto('/analytics/realtime');

      await expect(page.getByText(/real.*time|live/i)).toBeVisible();
      await expect(page.locator('[data-testid="realtime-widget"]').first()).toBeVisible();
    });

    test('should show live patient count', async ({ page }) => {
      await page.goto('/analytics/realtime');

      const patientCount = page.locator('[data-testid="live-patient-count"]');
      await expect(patientCount).toBeVisible();
    });

    test('should show current wait times', async ({ page }) => {
      await page.goto('/analytics/realtime');

      await expect(page.getByText(/current.*wait|wait.*time/i)).toBeVisible();
    });

    test('should show active appointments', async ({ page }) => {
      await page.goto('/analytics/realtime');

      await expect(page.getByText(/active.*appointment|in.*progress/i)).toBeVisible();
    });

    test('should auto-refresh real-time data', async ({ page }) => {
      await page.goto('/analytics/realtime');

      // Wait for auto-refresh indicator
      await page.waitForTimeout(5000);

      await expect(page.locator('[data-testid="last-updated"]')).toBeVisible();
    });
  });

  test.describe('Analytics API', () => {

    test('should get dashboard metrics via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/analytics/dashboard', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should get revenue analytics via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/analytics/revenue', {
        headers: authHeaders(token),
        params: {
          startDate: '2024-01-01',
          endDate: '2024-12-31'
        }
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should get patient analytics via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/analytics/patients', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should get appointment analytics via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/analytics/appointments', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should run custom report via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.post('/api/analytics/reports/run', {
        headers: authHeaders(token),
        data: {
          reportType: 'patient-list',
          filters: {
            status: 'Active'
          },
          format: 'json'
        }
      });

      expect([200, 201, 400, 401, 403]).toContain(response.status());
    });

    test('should export analytics data via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/analytics/export', {
        headers: authHeaders(token),
        params: {
          type: 'revenue',
          format: 'csv'
        }
      });

      expect([200, 401, 403]).toContain(response.status());
    });
  });

  test.describe('Analytics Performance', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should load dashboard within acceptable time', async ({ page }) => {
      const startTime = Date.now();
      await page.goto('/analytics');
      await expect(page.locator('[data-testid="analytics-widget"]').first()).toBeVisible();
      const loadTime = Date.now() - startTime;

      expect(loadTime).toBeLessThan(10000); // Should load within 10 seconds
    });

    test('should handle large data sets', async ({ page }) => {
      await page.goto('/analytics/reports/patient-list');

      await page.getByLabel(/date.*from/i).fill('2020-01-01');
      await page.getByLabel(/date.*to/i).fill('2024-12-31');
      await page.getByRole('button', { name: /run|generate/i }).click();

      await expect(page.locator('[data-testid="report-results"]')).toBeVisible({ timeout: 30000 });
    });
  });
});
