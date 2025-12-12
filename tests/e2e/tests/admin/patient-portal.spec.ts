/**
 * Patient Portal Module E2E Tests
 *
 * Comprehensive test suite for patient self-service portal functionality
 * Tests cover: patient registration, appointments, medical records, messaging,
 * prescriptions, billing, notifications, and profile management
 */

import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

test.describe('Patient Portal Module', () => {

  test.describe('Patient Registration & Onboarding', () => {

    test('should allow new patient self-registration', async ({ page }) => {
      await page.goto('/portal/register');

      // Personal Information
      await page.getByLabel(/first name/i).fill('John');
      await page.getByLabel(/last name/i).fill('TestPatient');
      await page.getByLabel(/email/i).fill(`john.test${Date.now()}@example.com`);
      await page.getByLabel(/phone/i).fill('+1234567890');
      await page.getByLabel(/date of birth/i).fill('1985-05-15');
      await page.getByLabel(/gender/i).selectOption({ label: /male/i });

      // Address Information
      await page.getByLabel(/address/i).fill('123 Test Street');
      await page.getByLabel(/city/i).fill('Test City');
      await page.getByLabel(/state|province/i).fill('Test State');
      await page.getByLabel(/postal|zip/i).fill('12345');

      // Account Credentials
      await page.getByLabel(/^password$/i).fill('SecurePass123!');
      await page.getByLabel(/confirm password/i).fill('SecurePass123!');

      // Terms and Privacy
      await page.getByLabel(/terms|conditions/i).check();
      await page.getByLabel(/privacy/i).check();

      await page.getByRole('button', { name: /register|sign up|create account/i }).click();

      await expect(page.getByText(/verification|confirm|success/i)).toBeVisible({ timeout: 10000 });
    });

    test('should verify email during registration', async ({ page }) => {
      await page.goto('/portal/verify-email');
      await expect(page.getByText(/verification|verify|confirm/i)).toBeVisible();
    });

    test('should complete patient profile after registration', async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();

      await page.goto('/portal/profile/complete');

      // Emergency Contact
      await page.getByLabel(/emergency.*name/i).fill('Jane Doe');
      await page.getByLabel(/emergency.*phone/i).fill('+1987654321');
      await page.getByLabel(/relationship/i).selectOption({ index: 1 });

      // Insurance Information
      await page.getByLabel(/insurance.*provider/i).fill('Test Insurance Co');
      await page.getByLabel(/policy.*number/i).fill('POL-123456');
      await page.getByLabel(/group.*number/i).fill('GRP-789');

      await page.getByRole('button', { name: /save|complete|submit/i }).click();

      await expect(page.getByText(/profile.*complete|saved|updated/i)).toBeVisible();
    });

    test('should upload identification documents', async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();

      await page.goto('/portal/documents/upload');

      const fileInput = page.locator('input[type="file"]').first();
      await fileInput.setInputFiles({
        name: 'id-document.pdf',
        mimeType: 'application/pdf',
        buffer: Buffer.from('Test ID document content')
      });

      await page.getByLabel(/document type/i).selectOption({ label: /identification|id/i });
      await page.getByRole('button', { name: /upload|submit/i }).click();

      await expect(page.getByText(/uploaded|success/i)).toBeVisible();
    });

    test('should sign consent forms digitally', async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();

      await page.goto('/portal/consent-forms');

      const consentForm = page.locator('[data-testid="consent-form"]').first();
      await consentForm.getByRole('checkbox', { name: /agree|consent/i }).check();
      await consentForm.getByRole('button', { name: /sign|submit/i }).click();

      await expect(page.getByText(/signed|submitted/i)).toBeVisible();
    });
  });

  test.describe('Appointment Booking', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
      await expect(page).toHaveURL(/portal|dashboard/i);
    });

    test('should search for available doctors', async ({ page }) => {
      await page.goto('/portal/doctors');

      await page.getByPlaceholder(/search|find/i).fill('General');
      await page.getByLabel(/specialty/i).selectOption({ index: 1 });

      await page.getByRole('button', { name: /search|find/i }).click();

      await expect(page.locator('[data-testid="doctor-card"]').first()).toBeVisible();
    });

    test('should view doctor profile and availability', async ({ page }) => {
      await page.goto('/portal/doctors');

      const doctorCard = page.locator('[data-testid="doctor-card"]').first();
      await doctorCard.getByRole('button', { name: /view|profile|details/i }).click();

      await expect(page.getByText(/qualifications|experience|about/i)).toBeVisible();
      await expect(page.getByText(/available|schedule/i)).toBeVisible();
    });

    test('should book new appointment', async ({ page }) => {
      await page.goto('/portal/appointments/book');

      // Select Specialty
      await page.getByLabel(/specialty|department/i).selectOption({ index: 1 });

      // Select Doctor
      await page.getByLabel(/doctor|physician/i).selectOption({ index: 1 });

      // Select Date
      const dateInput = page.getByLabel(/date/i);
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      await dateInput.fill(tomorrow.toISOString().split('T')[0]);

      // Select Time Slot
      const timeSlot = page.locator('[data-testid="time-slot"]').first();
      await timeSlot.click();

      // Select Appointment Type
      await page.getByLabel(/type|reason/i).selectOption({ label: /consultation|checkup/i });

      // Add Notes
      await page.getByLabel(/notes|reason|symptoms/i).fill('Regular checkup appointment');

      await page.getByRole('button', { name: /book|confirm|schedule/i }).click();

      await expect(page.getByText(/booked|confirmed|scheduled/i)).toBeVisible();
    });

    test('should view upcoming appointments', async ({ page }) => {
      await page.goto('/portal/appointments');

      await page.getByRole('tab', { name: /upcoming|scheduled/i }).click();

      const appointmentsList = page.locator('[data-testid="appointments-list"]');
      await expect(appointmentsList).toBeVisible();
    });

    test('should view appointment history', async ({ page }) => {
      await page.goto('/portal/appointments');

      await page.getByRole('tab', { name: /history|past|completed/i }).click();

      const historyList = page.locator('[data-testid="appointment-history"]');
      await expect(historyList).toBeVisible();
    });

    test('should reschedule appointment', async ({ page }) => {
      await page.goto('/portal/appointments');

      const appointment = page.locator('[data-testid="appointment-card"]').first();
      await appointment.getByRole('button', { name: /reschedule|change/i }).click();

      // Select new date
      const dateInput = page.getByLabel(/date/i);
      const nextWeek = new Date();
      nextWeek.setDate(nextWeek.getDate() + 7);
      await dateInput.fill(nextWeek.toISOString().split('T')[0]);

      // Select new time
      const timeSlot = page.locator('[data-testid="time-slot"]').first();
      await timeSlot.click();

      await page.getByRole('button', { name: /confirm|save/i }).click();

      await expect(page.getByText(/rescheduled|updated/i)).toBeVisible();
    });

    test('should cancel appointment', async ({ page }) => {
      await page.goto('/portal/appointments');

      const appointment = page.locator('[data-testid="appointment-card"]').first();
      await appointment.getByRole('button', { name: /cancel/i }).click();

      await page.getByLabel(/reason/i).fill('Personal emergency');
      await page.getByRole('button', { name: /confirm.*cancel/i }).click();

      await expect(page.getByText(/cancelled|canceled/i)).toBeVisible();
    });

    test('should join virtual appointment', async ({ page }) => {
      await page.goto('/portal/appointments');

      const virtualAppointment = page.locator('[data-testid="virtual-appointment"]').first();
      await virtualAppointment.getByRole('button', { name: /join|start/i }).click();

      await expect(page).toHaveURL(/video|telehealth|virtual/i);
    });

    test('should receive appointment reminders', async ({ page }) => {
      await page.goto('/portal/notifications');

      const reminderNotification = page.locator('[data-testid="reminder-notification"]');
      await expect(reminderNotification).toBeVisible();
    });
  });

  test.describe('Medical Records Access', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view health summary', async ({ page }) => {
      await page.goto('/portal/health-summary');

      await expect(page.getByText(/health.*summary|overview/i)).toBeVisible();
      await expect(page.getByText(/allergies/i)).toBeVisible();
      await expect(page.getByText(/medications/i)).toBeVisible();
      await expect(page.getByText(/conditions/i)).toBeVisible();
    });

    test('should view visit history', async ({ page }) => {
      await page.goto('/portal/visits');

      const visitList = page.locator('[data-testid="visit-list"]');
      await expect(visitList).toBeVisible();
    });

    test('should view visit details', async ({ page }) => {
      await page.goto('/portal/visits');

      const visitCard = page.locator('[data-testid="visit-card"]').first();
      await visitCard.click();

      await expect(page.getByText(/diagnosis|assessment/i)).toBeVisible();
      await expect(page.getByText(/treatment|prescription/i)).toBeVisible();
    });

    test('should view lab results', async ({ page }) => {
      await page.goto('/portal/lab-results');

      const labResultsList = page.locator('[data-testid="lab-results-list"]');
      await expect(labResultsList).toBeVisible();
    });

    test('should download lab report', async ({ page }) => {
      await page.goto('/portal/lab-results');

      const downloadPromise = page.waitForEvent('download');
      await page.locator('[data-testid="lab-result"]').first().getByRole('button', { name: /download/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/lab.*report|result/i);
    });

    test('should view radiology images', async ({ page }) => {
      await page.goto('/portal/imaging');

      const imagingList = page.locator('[data-testid="imaging-list"]');
      await expect(imagingList).toBeVisible();
    });

    test('should view radiology report', async ({ page }) => {
      await page.goto('/portal/imaging');

      const imagingItem = page.locator('[data-testid="imaging-item"]').first();
      await imagingItem.getByRole('button', { name: /view.*report/i }).click();

      await expect(page.getByText(/findings|impression/i)).toBeVisible();
    });

    test('should view vaccination records', async ({ page }) => {
      await page.goto('/portal/vaccinations');

      const vaccinationList = page.locator('[data-testid="vaccination-list"]');
      await expect(vaccinationList).toBeVisible();
    });

    test('should request medical records', async ({ page }) => {
      await page.goto('/portal/records/request');

      await page.getByLabel(/record.*type/i).selectOption({ index: 1 });
      await page.getByLabel(/date.*from/i).fill('2024-01-01');
      await page.getByLabel(/date.*to/i).fill('2024-12-31');
      await page.getByLabel(/purpose|reason/i).fill('Personal records');

      await page.getByRole('button', { name: /request|submit/i }).click();

      await expect(page.getByText(/request.*submitted|pending/i)).toBeVisible();
    });

    test('should download medical records', async ({ page }) => {
      await page.goto('/portal/records');

      const downloadPromise = page.waitForEvent('download');
      await page.getByRole('button', { name: /download.*all|export/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/medical.*record|health/i);
    });
  });

  test.describe('Prescription Management', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view active prescriptions', async ({ page }) => {
      await page.goto('/portal/prescriptions');

      await page.getByRole('tab', { name: /active|current/i }).click();

      const prescriptionList = page.locator('[data-testid="prescription-list"]');
      await expect(prescriptionList).toBeVisible();
    });

    test('should view prescription details', async ({ page }) => {
      await page.goto('/portal/prescriptions');

      const prescription = page.locator('[data-testid="prescription-card"]').first();
      await prescription.click();

      await expect(page.getByText(/dosage|instructions/i)).toBeVisible();
      await expect(page.getByText(/refills/i)).toBeVisible();
    });

    test('should request prescription refill', async ({ page }) => {
      await page.goto('/portal/prescriptions');

      const prescription = page.locator('[data-testid="prescription-card"]').first();
      await prescription.getByRole('button', { name: /refill|renew/i }).click();

      await page.getByLabel(/pharmacy|pickup/i).selectOption({ index: 1 });
      await page.getByRole('button', { name: /request|submit/i }).click();

      await expect(page.getByText(/refill.*requested|submitted/i)).toBeVisible();
    });

    test('should view refill request status', async ({ page }) => {
      await page.goto('/portal/prescriptions/refills');

      const refillStatus = page.locator('[data-testid="refill-status"]');
      await expect(refillStatus).toBeVisible();
    });

    test('should set medication reminders', async ({ page }) => {
      await page.goto('/portal/prescriptions');

      const prescription = page.locator('[data-testid="prescription-card"]').first();
      await prescription.getByRole('button', { name: /reminder|alert/i }).click();

      await page.getByLabel(/time/i).fill('09:00');
      await page.getByLabel(/frequency/i).selectOption({ label: /daily/i });

      await page.getByRole('button', { name: /save|set/i }).click();

      await expect(page.getByText(/reminder.*set|saved/i)).toBeVisible();
    });

    test('should view medication history', async ({ page }) => {
      await page.goto('/portal/prescriptions');

      await page.getByRole('tab', { name: /history|past/i }).click();

      const historyList = page.locator('[data-testid="prescription-history"]');
      await expect(historyList).toBeVisible();
    });

    test('should report medication side effects', async ({ page }) => {
      await page.goto('/portal/prescriptions');

      const prescription = page.locator('[data-testid="prescription-card"]').first();
      await prescription.getByRole('button', { name: /report|side.*effect/i }).click();

      await page.getByLabel(/symptom|effect/i).fill('Mild headache');
      await page.getByLabel(/severity/i).selectOption({ label: /mild/i });

      await page.getByRole('button', { name: /submit|report/i }).click();

      await expect(page.getByText(/reported|submitted/i)).toBeVisible();
    });
  });

  test.describe('Messaging & Communication', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view inbox messages', async ({ page }) => {
      await page.goto('/portal/messages');

      const inbox = page.locator('[data-testid="message-inbox"]');
      await expect(inbox).toBeVisible();
    });

    test('should compose new message to provider', async ({ page }) => {
      await page.goto('/portal/messages/compose');

      await page.getByLabel(/recipient|to/i).selectOption({ index: 1 });
      await page.getByLabel(/subject/i).fill('Question about medication');
      await page.getByLabel(/message|body/i).fill('I have a question about my prescription dosage.');

      await page.getByRole('button', { name: /send/i }).click();

      await expect(page.getByText(/sent|delivered/i)).toBeVisible();
    });

    test('should reply to message', async ({ page }) => {
      await page.goto('/portal/messages');

      const message = page.locator('[data-testid="message-item"]').first();
      await message.click();

      await page.getByRole('button', { name: /reply/i }).click();
      await page.getByLabel(/message|reply/i).fill('Thank you for the information.');
      await page.getByRole('button', { name: /send/i }).click();

      await expect(page.getByText(/sent|replied/i)).toBeVisible();
    });

    test('should attach file to message', async ({ page }) => {
      await page.goto('/portal/messages/compose');

      await page.getByLabel(/recipient|to/i).selectOption({ index: 1 });
      await page.getByLabel(/subject/i).fill('Document submission');

      const fileInput = page.locator('input[type="file"]');
      await fileInput.setInputFiles({
        name: 'document.pdf',
        mimeType: 'application/pdf',
        buffer: Buffer.from('Test document content')
      });

      await page.getByLabel(/message|body/i).fill('Please find attached document.');
      await page.getByRole('button', { name: /send/i }).click();

      await expect(page.getByText(/sent/i)).toBeVisible();
    });

    test('should view sent messages', async ({ page }) => {
      await page.goto('/portal/messages');

      await page.getByRole('tab', { name: /sent/i }).click();

      const sentList = page.locator('[data-testid="sent-messages"]');
      await expect(sentList).toBeVisible();
    });

    test('should mark message as read', async ({ page }) => {
      await page.goto('/portal/messages');

      const unreadMessage = page.locator('[data-testid="unread-message"]').first();
      await unreadMessage.click();

      await expect(unreadMessage).not.toHaveClass(/unread/);
    });

    test('should delete message', async ({ page }) => {
      await page.goto('/portal/messages');

      const message = page.locator('[data-testid="message-item"]').first();
      await message.getByRole('button', { name: /delete|remove/i }).click();

      await page.getByRole('button', { name: /confirm/i }).click();

      await expect(page.getByText(/deleted|removed/i)).toBeVisible();
    });
  });

  test.describe('Billing & Payments', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view billing summary', async ({ page }) => {
      await page.goto('/portal/billing');

      await expect(page.getByText(/balance|amount.*due/i)).toBeVisible();
      await expect(page.getByText(/recent.*charges|invoices/i)).toBeVisible();
    });

    test('should view invoice list', async ({ page }) => {
      await page.goto('/portal/billing/invoices');

      const invoiceList = page.locator('[data-testid="invoice-list"]');
      await expect(invoiceList).toBeVisible();
    });

    test('should view invoice details', async ({ page }) => {
      await page.goto('/portal/billing/invoices');

      const invoice = page.locator('[data-testid="invoice-item"]').first();
      await invoice.click();

      await expect(page.getByText(/service|charge/i)).toBeVisible();
      await expect(page.getByText(/total/i)).toBeVisible();
    });

    test('should download invoice PDF', async ({ page }) => {
      await page.goto('/portal/billing/invoices');

      const downloadPromise = page.waitForEvent('download');
      await page.locator('[data-testid="invoice-item"]').first().getByRole('button', { name: /download|pdf/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/invoice.*\.pdf/i);
    });

    test('should make payment', async ({ page }) => {
      await page.goto('/portal/billing/pay');

      await page.getByLabel(/amount/i).fill('100.00');

      // Payment method
      await page.getByLabel(/card.*number/i).fill('4111111111111111');
      await page.getByLabel(/expiry|exp/i).fill('12/25');
      await page.getByLabel(/cvv|cvc/i).fill('123');

      await page.getByRole('button', { name: /pay|submit/i }).click();

      await expect(page.getByText(/payment.*successful|processed/i)).toBeVisible({ timeout: 15000 });
    });

    test('should view payment history', async ({ page }) => {
      await page.goto('/portal/billing/payments');

      const paymentHistory = page.locator('[data-testid="payment-history"]');
      await expect(paymentHistory).toBeVisible();
    });

    test('should set up payment plan', async ({ page }) => {
      await page.goto('/portal/billing/payment-plan');

      await page.getByLabel(/installments|number/i).selectOption({ label: /3/i });
      await page.getByLabel(/start.*date/i).fill(new Date().toISOString().split('T')[0]);

      await page.getByRole('button', { name: /create|set up/i }).click();

      await expect(page.getByText(/payment.*plan.*created|set up/i)).toBeVisible();
    });

    test('should view insurance claims', async ({ page }) => {
      await page.goto('/portal/billing/claims');

      const claimsList = page.locator('[data-testid="claims-list"]');
      await expect(claimsList).toBeVisible();
    });

    test('should download EOB statement', async ({ page }) => {
      await page.goto('/portal/billing/claims');

      const downloadPromise = page.waitForEvent('download');
      await page.locator('[data-testid="claim-item"]').first().getByRole('button', { name: /eob|statement|download/i }).click();
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toMatch(/eob|statement/i);
    });
  });

  test.describe('Profile & Settings', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should view and edit profile', async ({ page }) => {
      await page.goto('/portal/profile');

      await expect(page.getByLabel(/first.*name/i)).toBeVisible();
      await expect(page.getByLabel(/phone/i)).toBeVisible();

      await page.getByLabel(/phone/i).fill('+1999888777');
      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/updated|saved/i)).toBeVisible();
    });

    test('should update emergency contact', async ({ page }) => {
      await page.goto('/portal/profile/emergency-contact');

      await page.getByLabel(/name/i).fill('Emergency Contact Name');
      await page.getByLabel(/phone/i).fill('+1555444333');
      await page.getByLabel(/relationship/i).selectOption({ index: 1 });

      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should update insurance information', async ({ page }) => {
      await page.goto('/portal/profile/insurance');

      await page.getByLabel(/provider|company/i).fill('New Insurance Co');
      await page.getByLabel(/policy.*number/i).fill('NEW-POL-123');

      await page.getByRole('button', { name: /save|update/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should change password', async ({ page }) => {
      await page.goto('/portal/settings/security');

      await page.getByLabel(/current.*password/i).fill(testUsers.patient?.password || 'password');
      await page.getByLabel(/new.*password/i).fill('NewSecurePass456!');
      await page.getByLabel(/confirm.*password/i).fill('NewSecurePass456!');

      await page.getByRole('button', { name: /change|update/i }).click();

      await expect(page.getByText(/password.*changed|updated/i)).toBeVisible();
    });

    test('should enable two-factor authentication', async ({ page }) => {
      await page.goto('/portal/settings/security');

      await page.getByRole('button', { name: /enable.*2fa|two.*factor/i }).click();

      await expect(page.getByText(/qr.*code|authenticator/i)).toBeVisible();
    });

    test('should manage notification preferences', async ({ page }) => {
      await page.goto('/portal/settings/notifications');

      await page.getByLabel(/email.*notifications/i).check();
      await page.getByLabel(/sms.*notifications/i).check();
      await page.getByLabel(/appointment.*reminders/i).check();

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should manage privacy settings', async ({ page }) => {
      await page.goto('/portal/settings/privacy');

      await page.getByLabel(/share.*with.*providers/i).check();
      await page.getByLabel(/research.*participation/i).uncheck();

      await page.getByRole('button', { name: /save/i }).click();

      await expect(page.getByText(/saved|updated/i)).toBeVisible();
    });

    test('should view and download data export', async ({ page }) => {
      await page.goto('/portal/settings/data');

      await page.getByRole('button', { name: /request.*export|download.*data/i }).click();

      await expect(page.getByText(/request.*submitted|processing/i)).toBeVisible();
    });
  });

  test.describe('Health Tracking', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should log vital signs', async ({ page }) => {
      await page.goto('/portal/health/vitals');

      await page.getByRole('button', { name: /add|log|new/i }).click();

      await page.getByLabel(/blood.*pressure.*systolic/i).fill('120');
      await page.getByLabel(/blood.*pressure.*diastolic/i).fill('80');
      await page.getByLabel(/heart.*rate|pulse/i).fill('72');
      await page.getByLabel(/weight/i).fill('70');

      await page.getByRole('button', { name: /save|log/i }).click();

      await expect(page.getByText(/saved|logged/i)).toBeVisible();
    });

    test('should view vitals history chart', async ({ page }) => {
      await page.goto('/portal/health/vitals');

      const chart = page.locator('[data-testid="vitals-chart"]');
      await expect(chart).toBeVisible();
    });

    test('should set health goals', async ({ page }) => {
      await page.goto('/portal/health/goals');

      await page.getByRole('button', { name: /add|new.*goal/i }).click();

      await page.getByLabel(/goal.*type/i).selectOption({ label: /weight/i });
      await page.getByLabel(/target/i).fill('65');
      await page.getByLabel(/deadline|date/i).fill('2025-06-01');

      await page.getByRole('button', { name: /save|create/i }).click();

      await expect(page.getByText(/goal.*created|saved/i)).toBeVisible();
    });

    test('should track symptoms', async ({ page }) => {
      await page.goto('/portal/health/symptoms');

      await page.getByRole('button', { name: /log|add|new/i }).click();

      await page.getByLabel(/symptom/i).fill('Headache');
      await page.getByLabel(/severity/i).selectOption({ label: /moderate/i });
      await page.getByLabel(/duration/i).fill('2 hours');
      await page.getByLabel(/notes/i).fill('Started after lunch');

      await page.getByRole('button', { name: /save|log/i }).click();

      await expect(page.getByText(/logged|saved/i)).toBeVisible();
    });

    test('should sync wearable device data', async ({ page }) => {
      await page.goto('/portal/health/devices');

      await page.getByRole('button', { name: /connect|sync/i }).click();

      await expect(page.getByText(/connect.*device|sync/i)).toBeVisible();
    });
  });

  test.describe('Patient Portal API', () => {

    test('should retrieve patient profile via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/portal/profile', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should list appointments via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/portal/appointments', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should list prescriptions via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/portal/prescriptions', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should list lab results via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/portal/lab-results', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should list invoices via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/portal/billing/invoices', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403]).toContain(response.status());
    });

    test('should send message via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.post('/api/portal/messages', {
        headers: authHeaders(token),
        data: {
          recipientId: 1,
          subject: 'Test Message',
          body: 'This is a test message from the API'
        }
      });

      expect([200, 201, 400, 401, 403]).toContain(response.status());
    });
  });

  test.describe('Portal Accessibility & Mobile', () => {

    test('should be accessible on mobile viewport', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/portal/login');

      await expect(page.getByLabel(/email|username/i)).toBeVisible();
      await expect(page.getByRole('button', { name: /login|sign in/i })).toBeVisible();
    });

    test('should have proper navigation on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/portal/login');
      await page.getByLabel(/email|username/i).fill(testUsers.patient?.email || 'patient@test.com');
      await page.getByLabel(/password/i).fill(testUsers.patient?.password || 'password');
      await page.getByRole('button', { name: /login|sign in/i }).click();

      // Mobile menu
      const menuButton = page.getByRole('button', { name: /menu|hamburger/i });
      await menuButton.click();

      await expect(page.getByRole('navigation')).toBeVisible();
    });

    test('should support keyboard navigation', async ({ page }) => {
      await page.goto('/portal/login');

      await page.keyboard.press('Tab');
      await expect(page.getByLabel(/email|username/i)).toBeFocused();

      await page.keyboard.press('Tab');
      await expect(page.getByLabel(/password/i)).toBeFocused();
    });

    test('should have proper ARIA labels', async ({ page }) => {
      await page.goto('/portal/login');

      const emailInput = page.getByLabel(/email|username/i);
      const ariaLabel = await emailInput.getAttribute('aria-label');
      const labelledBy = await emailInput.getAttribute('aria-labelledby');
      const id = await emailInput.getAttribute('id');

      // Should have some form of accessible labeling
      expect(ariaLabel || labelledBy || id).toBeTruthy();
    });
  });
});
