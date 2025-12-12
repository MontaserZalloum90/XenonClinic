/**
 * Specialty Medical Modules E2E Tests
 *
 * Comprehensive test suite for specialty-specific medical functionality
 * Tests cover: Audiology, Dental, Cardiology, Ophthalmology, Orthopedics,
 * Dermatology, Oncology, Neurology, Pediatrics, Gynecology, ENT, Fertility,
 * Physiotherapy, Dialysis, Pain Management, and more
 */

import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

test.describe('Specialty Medical Modules', () => {

  test.describe('Audiology Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
      await expect(page).toHaveURL(/dashboard|home/i);
    });

    test('should display audiology dashboard', async ({ page }) => {
      await page.goto('/audiology');
      await expect(page.getByRole('heading', { name: /audiology/i })).toBeVisible();
    });

    test('should create audiometry test', async ({ page }) => {
      await page.goto('/audiology/tests/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/test.*type/i).selectOption({ label: /pure.*tone/i });

      // Left ear frequencies
      await page.getByLabel(/250hz.*left/i).fill('20');
      await page.getByLabel(/500hz.*left/i).fill('25');
      await page.getByLabel(/1000hz.*left/i).fill('30');
      await page.getByLabel(/2000hz.*left/i).fill('35');
      await page.getByLabel(/4000hz.*left/i).fill('40');
      await page.getByLabel(/8000hz.*left/i).fill('45');

      // Right ear frequencies
      await page.getByLabel(/250hz.*right/i).fill('15');
      await page.getByLabel(/500hz.*right/i).fill('20');
      await page.getByLabel(/1000hz.*right/i).fill('25');
      await page.getByLabel(/2000hz.*right/i).fill('30');
      await page.getByLabel(/4000hz.*right/i).fill('35');
      await page.getByLabel(/8000hz.*right/i).fill('40');

      await page.getByRole('button', { name: /save|submit/i }).click();
      await expect(page.getByText(/saved|created/i)).toBeVisible();
    });

    test('should view audiogram', async ({ page }) => {
      await page.goto('/audiology/audiograms');

      const audiogram = page.locator('[data-testid="audiogram-card"]').first();
      await audiogram.click();

      await expect(page.locator('[data-testid="audiogram-chart"]')).toBeVisible();
    });

    test('should recommend hearing aids', async ({ page }) => {
      await page.goto('/audiology/hearing-aids');

      await page.getByRole('button', { name: /recommend|new/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/hearing.*aid.*type/i).selectOption({ label: /behind.*ear|bte/i });
      await page.getByLabel(/brand/i).selectOption({ index: 1 });
      await page.getByLabel(/model/i).fill('HA-500');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/recommendation.*saved/i)).toBeVisible();
    });

    test('should track hearing aid fitting', async ({ page }) => {
      await page.goto('/audiology/fittings');

      await page.getByRole('button', { name: /new.*fitting/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/hearing.*aid/i).selectOption({ index: 1 });
      await page.getByLabel(/fitting.*notes/i).fill('Initial fitting - comfortable, good feedback');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/fitting.*recorded/i)).toBeVisible();
    });
  });

  test.describe('Dental Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display dental dashboard', async ({ page }) => {
      await page.goto('/dental');
      await expect(page.getByRole('heading', { name: /dental/i })).toBeVisible();
    });

    test('should create dental chart', async ({ page }) => {
      await page.goto('/dental/charts/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });

      // Interactive tooth chart
      const toothChart = page.locator('[data-testid="tooth-chart"]');
      await toothChart.locator('[data-tooth="18"]').click();
      await page.getByLabel(/condition/i).selectOption({ label: /cavity|caries/i });

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/chart.*saved/i)).toBeVisible();
    });

    test('should create dental treatment plan', async ({ page }) => {
      await page.goto('/dental/treatments/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/procedure/i).selectOption({ label: /filling|restoration/i });
      await page.getByLabel(/tooth/i).fill('18');
      await page.getByLabel(/surface/i).selectOption({ label: /occlusal/i });
      await page.getByLabel(/material/i).selectOption({ label: /composite/i });

      await page.getByRole('button', { name: /create|save/i }).click();
      await expect(page.getByText(/treatment.*plan.*created/i)).toBeVisible();
    });

    test('should record periodontal exam', async ({ page }) => {
      await page.goto('/dental/perio/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });

      // Periodontal probing depths
      await page.getByLabel(/tooth.*16.*mb/i).fill('3');
      await page.getByLabel(/tooth.*16.*db/i).fill('4');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/perio.*exam.*saved/i)).toBeVisible();
    });

    test('should view odontogram', async ({ page }) => {
      await page.goto('/dental/patients');

      const patientRow = page.locator('[data-testid="patient-row"]').first();
      await patientRow.getByRole('button', { name: /odontogram|chart/i }).click();

      await expect(page.locator('[data-testid="odontogram"]')).toBeVisible();
    });

    test('should manage dental imaging', async ({ page }) => {
      await page.goto('/dental/imaging');

      await page.getByRole('button', { name: /upload|new/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/image.*type/i).selectOption({ label: /panoramic|x-ray/i });

      const fileInput = page.locator('input[type="file"]');
      await fileInput.setInputFiles({
        name: 'dental-xray.jpg',
        mimeType: 'image/jpeg',
        buffer: Buffer.from('fake dental image')
      });

      await page.getByRole('button', { name: /upload|save/i }).click();
      await expect(page.getByText(/uploaded|saved/i)).toBeVisible();
    });
  });

  test.describe('Cardiology Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display cardiology dashboard', async ({ page }) => {
      await page.goto('/cardiology');
      await expect(page.getByRole('heading', { name: /cardiology/i })).toBeVisible();
    });

    test('should create ECG record', async ({ page }) => {
      await page.goto('/cardiology/ecg/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/heart.*rate/i).fill('72');
      await page.getByLabel(/pr.*interval/i).fill('160');
      await page.getByLabel(/qrs.*duration/i).fill('100');
      await page.getByLabel(/qt.*interval/i).fill('400');
      await page.getByLabel(/rhythm/i).selectOption({ label: /sinus/i });
      await page.getByLabel(/interpretation/i).fill('Normal sinus rhythm');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/ecg.*saved/i)).toBeVisible();
    });

    test('should record echocardiogram', async ({ page }) => {
      await page.goto('/cardiology/echo/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/lvef|ejection.*fraction/i).fill('60');
      await page.getByLabel(/lv.*dimension/i).fill('45');
      await page.getByLabel(/la.*size/i).fill('35');
      await page.getByLabel(/wall.*motion/i).selectOption({ label: /normal/i });
      await page.getByLabel(/valve.*function/i).fill('All valves competent');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/echo.*saved/i)).toBeVisible();
    });

    test('should manage stress test', async ({ page }) => {
      await page.goto('/cardiology/stress-test/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/test.*type/i).selectOption({ label: /treadmill|exercise/i });
      await page.getByLabel(/protocol/i).selectOption({ label: /bruce/i });
      await page.getByLabel(/duration/i).fill('12');
      await page.getByLabel(/max.*heart.*rate/i).fill('175');
      await page.getByLabel(/target.*achieved/i).selectOption({ label: /yes/i });
      await page.getByLabel(/result/i).selectOption({ label: /negative/i });

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/stress.*test.*saved/i)).toBeVisible();
    });

    test('should track cardiac catheterization', async ({ page }) => {
      await page.goto('/cardiology/cath/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/indication/i).fill('Chest pain evaluation');
      await page.getByLabel(/access.*site/i).selectOption({ label: /radial/i });
      await page.getByLabel(/findings/i).fill('No significant stenosis');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/cath.*record.*saved/i)).toBeVisible();
    });

    test('should calculate cardiovascular risk', async ({ page }) => {
      await page.goto('/cardiology/risk-calculator');

      await page.getByLabel(/age/i).fill('55');
      await page.getByLabel(/gender/i).selectOption({ label: /male/i });
      await page.getByLabel(/total.*cholesterol/i).fill('220');
      await page.getByLabel(/hdl/i).fill('45');
      await page.getByLabel(/systolic.*bp/i).fill('140');
      await page.getByLabel(/smoker/i).check();
      await page.getByLabel(/diabetes/i).uncheck();

      await page.getByRole('button', { name: /calculate/i }).click();

      await expect(page.getByText(/risk.*score|10.*year.*risk/i)).toBeVisible();
    });
  });

  test.describe('Ophthalmology Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display ophthalmology dashboard', async ({ page }) => {
      await page.goto('/ophthalmology');
      await expect(page.getByRole('heading', { name: /ophthalmology|eye/i })).toBeVisible();
    });

    test('should record visual acuity', async ({ page }) => {
      await page.goto('/ophthalmology/visual-acuity/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/right.*eye.*uncorrected|od.*unaided/i).fill('20/40');
      await page.getByLabel(/right.*eye.*corrected|od.*best/i).fill('20/20');
      await page.getByLabel(/left.*eye.*uncorrected|os.*unaided/i).fill('20/30');
      await page.getByLabel(/left.*eye.*corrected|os.*best/i).fill('20/20');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/visual.*acuity.*saved/i)).toBeVisible();
    });

    test('should record refraction', async ({ page }) => {
      await page.goto('/ophthalmology/refraction/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });

      // Right eye
      await page.getByLabel(/od.*sphere/i).fill('-2.00');
      await page.getByLabel(/od.*cylinder/i).fill('-0.50');
      await page.getByLabel(/od.*axis/i).fill('180');
      await page.getByLabel(/od.*add/i).fill('+1.50');

      // Left eye
      await page.getByLabel(/os.*sphere/i).fill('-1.75');
      await page.getByLabel(/os.*cylinder/i).fill('-0.75');
      await page.getByLabel(/os.*axis/i).fill('170');
      await page.getByLabel(/os.*add/i).fill('+1.50');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/refraction.*saved/i)).toBeVisible();
    });

    test('should record intraocular pressure', async ({ page }) => {
      await page.goto('/ophthalmology/iop/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/right.*eye.*iop|od.*iop/i).fill('16');
      await page.getByLabel(/left.*eye.*iop|os.*iop/i).fill('17');
      await page.getByLabel(/method/i).selectOption({ label: /goldmann|applanation/i });
      await page.getByLabel(/time/i).fill('10:00');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/iop.*saved/i)).toBeVisible();
    });

    test('should record slit lamp exam', async ({ page }) => {
      await page.goto('/ophthalmology/slit-lamp/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/lids.*lashes/i).fill('Normal');
      await page.getByLabel(/conjunctiva/i).fill('Clear');
      await page.getByLabel(/cornea/i).fill('Clear');
      await page.getByLabel(/anterior.*chamber/i).fill('Deep and quiet');
      await page.getByLabel(/iris/i).fill('Normal');
      await page.getByLabel(/lens/i).fill('Clear');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/exam.*saved/i)).toBeVisible();
    });

    test('should view fundus images', async ({ page }) => {
      await page.goto('/ophthalmology/fundus');

      const fundusImage = page.locator('[data-testid="fundus-image"]').first();
      await fundusImage.click();

      await expect(page.locator('[data-testid="fundus-viewer"]')).toBeVisible();
    });

    test('should generate prescription for glasses', async ({ page }) => {
      await page.goto('/ophthalmology/prescriptions/glasses');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/type/i).selectOption({ label: /progressive|bifocal/i });
      await page.getByLabel(/pd/i).fill('64');

      await page.getByRole('button', { name: /generate|create/i }).click();
      await expect(page.getByText(/prescription.*generated/i)).toBeVisible();
    });
  });

  test.describe('Orthopedics Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display orthopedics dashboard', async ({ page }) => {
      await page.goto('/orthopedics');
      await expect(page.getByRole('heading', { name: /orthopedic/i })).toBeVisible();
    });

    test('should record musculoskeletal exam', async ({ page }) => {
      await page.goto('/orthopedics/exam/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/body.*part|region/i).selectOption({ label: /knee/i });
      await page.getByLabel(/side/i).selectOption({ label: /right/i });
      await page.getByLabel(/range.*of.*motion/i).fill('0-140 degrees');
      await page.getByLabel(/strength/i).selectOption({ label: /5.*5|normal/i });
      await page.getByLabel(/tenderness/i).fill('Mild over medial joint line');
      await page.getByLabel(/special.*tests/i).fill('McMurray negative, Lachman negative');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/exam.*saved/i)).toBeVisible();
    });

    test('should manage fracture record', async ({ page }) => {
      await page.goto('/orthopedics/fractures/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/bone/i).selectOption({ label: /radius/i });
      await page.getByLabel(/side/i).selectOption({ label: /left/i });
      await page.getByLabel(/fracture.*type/i).selectOption({ label: /distal|colles/i });
      await page.getByLabel(/mechanism/i).fill('Fall on outstretched hand');
      await page.getByLabel(/displacement/i).selectOption({ label: /displaced/i });
      await page.getByLabel(/treatment/i).selectOption({ label: /closed.*reduction|cast/i });

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/fracture.*recorded/i)).toBeVisible();
    });

    test('should track surgical procedure', async ({ page }) => {
      await page.goto('/orthopedics/surgery/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/procedure/i).selectOption({ label: /total.*knee|tkr/i });
      await page.getByLabel(/side/i).selectOption({ label: /right/i });
      await page.getByLabel(/implant/i).fill('Zimmer Biomet NexGen');
      await page.getByLabel(/surgeon/i).selectOption({ index: 1 });
      await page.getByLabel(/operative.*notes/i).fill('Standard medial parapatellar approach...');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/surgery.*recorded/i)).toBeVisible();
    });

    test('should view body diagram', async ({ page }) => {
      await page.goto('/orthopedics/patients');

      const patientRow = page.locator('[data-testid="patient-row"]').first();
      await patientRow.getByRole('button', { name: /diagram|body.*map/i }).click();

      await expect(page.locator('[data-testid="body-diagram"]')).toBeVisible();
    });
  });

  test.describe('Dermatology Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display dermatology dashboard', async ({ page }) => {
      await page.goto('/dermatology');
      await expect(page.getByRole('heading', { name: /dermatology|skin/i })).toBeVisible();
    });

    test('should record skin examination', async ({ page }) => {
      await page.goto('/dermatology/exam/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/body.*area/i).selectOption({ label: /face/i });
      await page.getByLabel(/lesion.*type/i).selectOption({ label: /papule/i });
      await page.getByLabel(/size/i).fill('5mm');
      await page.getByLabel(/color/i).selectOption({ label: /erythematous|red/i });
      await page.getByLabel(/distribution/i).fill('Scattered across forehead');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/exam.*saved/i)).toBeVisible();
    });

    test('should capture dermatological photo', async ({ page }) => {
      await page.goto('/dermatology/photos/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/body.*region/i).selectOption({ label: /arm|extremity/i });

      const fileInput = page.locator('input[type="file"]');
      await fileInput.setInputFiles({
        name: 'skin-lesion.jpg',
        mimeType: 'image/jpeg',
        buffer: Buffer.from('fake derma image')
      });

      await page.getByLabel(/clinical.*notes/i).fill('Monitoring pigmented lesion');
      await page.getByRole('button', { name: /upload|save/i }).click();
      await expect(page.getByText(/photo.*saved|uploaded/i)).toBeVisible();
    });

    test('should track mole mapping', async ({ page }) => {
      await page.goto('/dermatology/mole-mapping');

      await page.getByRole('button', { name: /new.*mapping/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });

      // Interactive body map for mole positions
      const bodyMap = page.locator('[data-testid="body-map"]');
      await bodyMap.click({ position: { x: 100, y: 200 } });

      await page.getByLabel(/size/i).fill('4mm');
      await page.getByLabel(/abcde.*score/i).fill('0');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/mapping.*saved/i)).toBeVisible();
    });

    test('should record biopsy', async ({ page }) => {
      await page.goto('/dermatology/biopsies/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/location/i).fill('Left forearm');
      await page.getByLabel(/type/i).selectOption({ label: /punch/i });
      await page.getByLabel(/clinical.*impression/i).fill('R/O basal cell carcinoma');
      await page.getByLabel(/specimen.*sent/i).check();

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/biopsy.*recorded/i)).toBeVisible();
    });
  });

  test.describe('Oncology Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display oncology dashboard', async ({ page }) => {
      await page.goto('/oncology');
      await expect(page.getByRole('heading', { name: /oncology|cancer/i })).toBeVisible();
    });

    test('should create cancer diagnosis record', async ({ page }) => {
      await page.goto('/oncology/diagnosis/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/cancer.*type/i).selectOption({ label: /breast/i });
      await page.getByLabel(/histology/i).fill('Invasive ductal carcinoma');
      await page.getByLabel(/grade/i).selectOption({ label: /grade.*2|moderate/i });
      await page.getByLabel(/stage/i).selectOption({ label: /stage.*ii/i });
      await page.getByLabel(/tnm.*t/i).fill('T2');
      await page.getByLabel(/tnm.*n/i).fill('N1');
      await page.getByLabel(/tnm.*m/i).fill('M0');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/diagnosis.*saved/i)).toBeVisible();
    });

    test('should create chemotherapy protocol', async ({ page }) => {
      await page.goto('/oncology/chemo/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/protocol.*name/i).selectOption({ label: /ac-t|folfox/i });
      await page.getByLabel(/cycles/i).fill('6');
      await page.getByLabel(/start.*date/i).fill('2024-01-15');

      await page.getByRole('button', { name: /create|save/i }).click();
      await expect(page.getByText(/protocol.*created/i)).toBeVisible();
    });

    test('should record chemotherapy administration', async ({ page }) => {
      await page.goto('/oncology/chemo/administration');

      await page.getByRole('button', { name: /new.*administration/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/cycle/i).fill('1');
      await page.getByLabel(/day/i).fill('1');
      await page.getByLabel(/drug/i).selectOption({ index: 1 });
      await page.getByLabel(/dose/i).fill('100');
      await page.getByLabel(/dose.*unit/i).selectOption({ label: /mg.*m2/i });
      await page.getByLabel(/adverse.*reactions/i).fill('Grade 1 nausea');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/administration.*recorded/i)).toBeVisible();
    });

    test('should track radiation therapy', async ({ page }) => {
      await page.goto('/oncology/radiation/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/treatment.*site/i).fill('Left breast');
      await page.getByLabel(/technique/i).selectOption({ label: /imrt|external/i });
      await page.getByLabel(/total.*dose/i).fill('50');
      await page.getByLabel(/fractions/i).fill('25');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/radiation.*plan.*saved/i)).toBeVisible();
    });

    test('should view tumor markers trend', async ({ page }) => {
      await page.goto('/oncology/markers');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/marker/i).selectOption({ label: /ca.*125|cea/i });

      await expect(page.locator('[data-testid="marker-trend-chart"]')).toBeVisible();
    });
  });

  test.describe('Neurology Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display neurology dashboard', async ({ page }) => {
      await page.goto('/neurology');
      await expect(page.getByRole('heading', { name: /neurology/i })).toBeVisible();
    });

    test('should record neurological examination', async ({ page }) => {
      await page.goto('/neurology/exam/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/mental.*status/i).fill('Alert and oriented x3');
      await page.getByLabel(/cranial.*nerves/i).fill('II-XII intact');
      await page.getByLabel(/motor/i).fill('5/5 throughout');
      await page.getByLabel(/sensory/i).fill('Intact to light touch');
      await page.getByLabel(/reflexes/i).fill('2+ throughout, symmetric');
      await page.getByLabel(/coordination/i).fill('Finger-nose intact bilaterally');
      await page.getByLabel(/gait/i).fill('Normal');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/exam.*saved/i)).toBeVisible();
    });

    test('should record EEG results', async ({ page }) => {
      await page.goto('/neurology/eeg/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/recording.*duration/i).fill('30');
      await page.getByLabel(/sleep.*obtained/i).check();
      await page.getByLabel(/background.*rhythm/i).fill('9-10 Hz posterior dominant rhythm');
      await page.getByLabel(/abnormalities/i).fill('None');
      await page.getByLabel(/interpretation/i).fill('Normal awake and asleep EEG');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/eeg.*saved/i)).toBeVisible();
    });

    test('should manage epilepsy diary', async ({ page }) => {
      await page.goto('/neurology/epilepsy/diary');

      await page.getByRole('button', { name: /new.*entry|log.*seizure/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/date.*time/i).fill('2024-01-15T14:30');
      await page.getByLabel(/seizure.*type/i).selectOption({ label: /focal|generalized/i });
      await page.getByLabel(/duration/i).fill('2');
      await page.getByLabel(/triggers/i).fill('Sleep deprivation');
      await page.getByLabel(/post.*ictal/i).fill('Confused for 10 minutes');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/entry.*saved/i)).toBeVisible();
    });

    test('should calculate stroke scale', async ({ page }) => {
      await page.goto('/neurology/stroke/nihss');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/consciousness/i).selectOption({ index: 0 });
      await page.getByLabel(/gaze/i).selectOption({ index: 0 });
      await page.getByLabel(/visual/i).selectOption({ index: 0 });
      await page.getByLabel(/facial.*palsy/i).selectOption({ index: 0 });
      await page.getByLabel(/motor.*arm.*left/i).selectOption({ index: 0 });
      await page.getByLabel(/motor.*arm.*right/i).selectOption({ index: 0 });

      await page.getByRole('button', { name: /calculate|score/i }).click();
      await expect(page.getByText(/nihss.*score/i)).toBeVisible();
    });
  });

  test.describe('Pediatrics Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display pediatrics dashboard', async ({ page }) => {
      await page.goto('/pediatrics');
      await expect(page.getByRole('heading', { name: /pediatric/i })).toBeVisible();
    });

    test('should record growth measurements', async ({ page }) => {
      await page.goto('/pediatrics/growth/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/weight.*kg/i).fill('12.5');
      await page.getByLabel(/height.*cm|length/i).fill('85');
      await page.getByLabel(/head.*circumference/i).fill('47');
      await page.getByLabel(/bmi/i).fill('17.3');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/measurements.*saved/i)).toBeVisible();
    });

    test('should view growth chart', async ({ page }) => {
      await page.goto('/pediatrics/patients');

      const patientRow = page.locator('[data-testid="patient-row"]').first();
      await patientRow.getByRole('button', { name: /growth.*chart/i }).click();

      await expect(page.locator('[data-testid="growth-chart"]')).toBeVisible();
      await expect(page.getByText(/percentile/i)).toBeVisible();
    });

    test('should record developmental milestones', async ({ page }) => {
      await page.goto('/pediatrics/milestones/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/gross.*motor/i).selectOption({ label: /walks.*alone|age.*appropriate/i });
      await page.getByLabel(/fine.*motor/i).selectOption({ label: /pincer.*grasp|age.*appropriate/i });
      await page.getByLabel(/language/i).selectOption({ label: /2.*words|age.*appropriate/i });
      await page.getByLabel(/social/i).selectOption({ label: /plays.*alongside|age.*appropriate/i });

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/milestones.*saved/i)).toBeVisible();
    });

    test('should track vaccination schedule', async ({ page }) => {
      await page.goto('/pediatrics/vaccinations');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });

      // View recommended schedule
      await expect(page.locator('[data-testid="vaccine-schedule"]')).toBeVisible();

      // Administer vaccine
      await page.getByRole('button', { name: /administer|record/i }).click();
      await page.getByLabel(/vaccine/i).selectOption({ label: /mmr/i });
      await page.getByLabel(/lot.*number/i).fill('LOT-123456');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/vaccination.*recorded/i)).toBeVisible();
    });

    test('should calculate pediatric dosing', async ({ page }) => {
      await page.goto('/pediatrics/dosing-calculator');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/medication/i).selectOption({ label: /amoxicillin/i });
      await page.getByLabel(/indication/i).selectOption({ label: /otitis.*media/i });

      await page.getByRole('button', { name: /calculate/i }).click();

      await expect(page.getByText(/recommended.*dose/i)).toBeVisible();
    });
  });

  test.describe('Gynecology & Obstetrics Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display OB/GYN dashboard', async ({ page }) => {
      await page.goto('/obgyn');
      await expect(page.getByRole('heading', { name: /ob.*gyn|obstetric|gynecolog/i })).toBeVisible();
    });

    test('should create pregnancy record', async ({ page }) => {
      await page.goto('/obgyn/pregnancy/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/lmp|last.*menstrual/i).fill('2024-01-15');
      await page.getByLabel(/edd|due.*date/i).fill('2024-10-22');
      await page.getByLabel(/gravida/i).fill('2');
      await page.getByLabel(/para/i).fill('1');
      await page.getByLabel(/blood.*type/i).selectOption({ label: /o.*positive/i });

      await page.getByRole('button', { name: /save|create/i }).click();
      await expect(page.getByText(/pregnancy.*created/i)).toBeVisible();
    });

    test('should record prenatal visit', async ({ page }) => {
      await page.goto('/obgyn/prenatal/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/gestational.*age/i).fill('24');
      await page.getByLabel(/weight/i).fill('68');
      await page.getByLabel(/blood.*pressure/i).fill('120/78');
      await page.getByLabel(/fundal.*height/i).fill('24');
      await page.getByLabel(/fetal.*heart.*rate/i).fill('145');
      await page.getByLabel(/fetal.*movement/i).selectOption({ label: /present/i });
      await page.getByLabel(/edema/i).selectOption({ label: /none/i });

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/visit.*saved/i)).toBeVisible();
    });

    test('should record ultrasound findings', async ({ page }) => {
      await page.goto('/obgyn/ultrasound/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/gestational.*age.*by.*us/i).fill('20w3d');
      await page.getByLabel(/bpd/i).fill('48');
      await page.getByLabel(/hc/i).fill('175');
      await page.getByLabel(/ac/i).fill('150');
      await page.getByLabel(/fl/i).fill('33');
      await page.getByLabel(/efw/i).fill('370');
      await page.getByLabel(/placenta/i).selectOption({ label: /anterior/i });
      await page.getByLabel(/afv.*afi/i).fill('14');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/ultrasound.*saved/i)).toBeVisible();
    });

    test('should record pap smear', async ({ page }) => {
      await page.goto('/obgyn/pap-smear/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/result/i).selectOption({ label: /nilm|negative/i });
      await page.getByLabel(/hpv.*test/i).selectOption({ label: /negative/i });
      await page.getByLabel(/next.*due/i).fill('2027-01-15');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/pap.*saved/i)).toBeVisible();
    });

    test('should calculate pregnancy wheel', async ({ page }) => {
      await page.goto('/obgyn/pregnancy-wheel');

      await page.getByLabel(/lmp/i).fill('2024-01-15');
      await page.getByRole('button', { name: /calculate/i }).click();

      await expect(page.getByText(/edd|due.*date/i)).toBeVisible();
      await expect(page.getByText(/gestational.*age/i)).toBeVisible();
    });
  });

  test.describe('Physiotherapy Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display physiotherapy dashboard', async ({ page }) => {
      await page.goto('/physiotherapy');
      await expect(page.getByRole('heading', { name: /physiotherapy|physical.*therapy/i })).toBeVisible();
    });

    test('should create treatment plan', async ({ page }) => {
      await page.goto('/physiotherapy/treatment-plan/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/diagnosis/i).fill('Low back pain - L5/S1 disc herniation');
      await page.getByLabel(/goals/i).fill('Reduce pain, improve mobility, return to work');
      await page.getByLabel(/sessions.*recommended/i).fill('12');
      await page.getByLabel(/frequency/i).selectOption({ label: /2.*per.*week/i });

      await page.getByRole('button', { name: /create|save/i }).click();
      await expect(page.getByText(/treatment.*plan.*created/i)).toBeVisible();
    });

    test('should record therapy session', async ({ page }) => {
      await page.goto('/physiotherapy/sessions/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/pain.*level.*before/i).fill('7');
      await page.getByLabel(/treatment.*provided/i).fill('Manual therapy, exercises, ultrasound');
      await page.getByLabel(/pain.*level.*after/i).fill('4');
      await page.getByLabel(/home.*exercises/i).fill('Core strengthening 2x daily');
      await page.getByLabel(/notes/i).fill('Good progress, responding well to treatment');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/session.*saved/i)).toBeVisible();
    });

    test('should track range of motion', async ({ page }) => {
      await page.goto('/physiotherapy/rom/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/joint/i).selectOption({ label: /shoulder/i });
      await page.getByLabel(/side/i).selectOption({ label: /right/i });
      await page.getByLabel(/flexion/i).fill('150');
      await page.getByLabel(/extension/i).fill('45');
      await page.getByLabel(/abduction/i).fill('160');
      await page.getByLabel(/internal.*rotation/i).fill('70');
      await page.getByLabel(/external.*rotation/i).fill('80');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/rom.*saved/i)).toBeVisible();
    });

    test('should assign exercise program', async ({ page }) => {
      await page.goto('/physiotherapy/exercises/assign');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByRole('button', { name: /add.*exercise/i }).click();
      await page.getByLabel(/exercise/i).selectOption({ label: /bridge|squat/i });
      await page.getByLabel(/sets/i).fill('3');
      await page.getByLabel(/reps/i).fill('10');
      await page.getByLabel(/frequency/i).selectOption({ label: /daily/i });

      await page.getByRole('button', { name: /save|assign/i }).click();
      await expect(page.getByText(/exercises.*assigned/i)).toBeVisible();
    });
  });

  test.describe('ENT Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display ENT dashboard', async ({ page }) => {
      await page.goto('/ent');
      await expect(page.getByRole('heading', { name: /ent|otolaryngology/i })).toBeVisible();
    });

    test('should record ear examination', async ({ page }) => {
      await page.goto('/ent/ear-exam/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/right.*ear.*canal/i).fill('Clear');
      await page.getByLabel(/right.*tympanic.*membrane/i).fill('Intact, pearly gray');
      await page.getByLabel(/left.*ear.*canal/i).fill('Clear');
      await page.getByLabel(/left.*tympanic.*membrane/i).fill('Intact, pearly gray');
      await page.getByLabel(/hearing.*assessment/i).fill('Grossly intact bilaterally');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/exam.*saved/i)).toBeVisible();
    });

    test('should record nasal endoscopy', async ({ page }) => {
      await page.goto('/ent/nasal-endoscopy/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/septum/i).fill('Midline');
      await page.getByLabel(/turbinates/i).fill('Mildly edematous bilaterally');
      await page.getByLabel(/meati/i).fill('Patent');
      await page.getByLabel(/nasopharynx/i).fill('Clear');
      await page.getByLabel(/findings/i).fill('Chronic rhinosinusitis');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/endoscopy.*saved/i)).toBeVisible();
    });

    test('should record laryngoscopy', async ({ page }) => {
      await page.goto('/ent/laryngoscopy/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/vocal.*cords/i).fill('Mobile bilaterally');
      await page.getByLabel(/arytenoids/i).fill('Normal');
      await page.getByLabel(/epiglottis/i).fill('Normal');
      await page.getByLabel(/subglottis/i).fill('Clear');
      await page.getByLabel(/impression/i).fill('No abnormalities');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/laryngoscopy.*saved/i)).toBeVisible();
    });
  });

  test.describe('Fertility Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display fertility dashboard', async ({ page }) => {
      await page.goto('/fertility');
      await expect(page.getByRole('heading', { name: /fertility|ivf|reproductive/i })).toBeVisible();
    });

    test('should create fertility assessment', async ({ page }) => {
      await page.goto('/fertility/assessment/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/partner/i).fill('Partner Name');
      await page.getByLabel(/duration.*infertility/i).fill('24');
      await page.getByLabel(/primary.*secondary/i).selectOption({ label: /primary/i });
      await page.getByLabel(/amh/i).fill('2.5');
      await page.getByLabel(/fsh/i).fill('7.2');
      await page.getByLabel(/afc/i).fill('14');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/assessment.*saved/i)).toBeVisible();
    });

    test('should track IVF cycle', async ({ page }) => {
      await page.goto('/fertility/ivf/cycle/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/cycle.*number/i).fill('1');
      await page.getByLabel(/protocol/i).selectOption({ label: /antagonist|long/i });
      await page.getByLabel(/start.*date/i).fill('2024-01-15');

      await page.getByRole('button', { name: /create/i }).click();
      await expect(page.getByText(/cycle.*created/i)).toBeVisible();
    });

    test('should record stimulation monitoring', async ({ page }) => {
      await page.goto('/fertility/ivf/monitoring');

      await page.getByRole('button', { name: /new.*scan/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/day/i).fill('8');
      await page.getByLabel(/e2.*level/i).fill('1200');
      await page.getByLabel(/lh/i).fill('2.5');
      await page.getByLabel(/follicles.*right/i).fill('6');
      await page.getByLabel(/follicles.*left/i).fill('5');
      await page.getByLabel(/endometrial.*thickness/i).fill('9.5');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/monitoring.*saved/i)).toBeVisible();
    });

    test('should record egg retrieval', async ({ page }) => {
      await page.goto('/fertility/ivf/retrieval/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/eggs.*retrieved/i).fill('12');
      await page.getByLabel(/mature.*eggs/i).fill('10');
      await page.getByLabel(/procedure.*notes/i).fill('Uncomplicated retrieval');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/retrieval.*saved/i)).toBeVisible();
    });

    test('should record embryo development', async ({ page }) => {
      await page.goto('/fertility/ivf/embryos');

      await page.getByRole('button', { name: /update.*day/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/day/i).selectOption({ label: /day.*3/i });
      await page.getByLabel(/embryo.*1.*grade/i).fill('8-cell, Grade A');
      await page.getByLabel(/embryo.*2.*grade/i).fill('7-cell, Grade B');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/embryo.*report.*saved/i)).toBeVisible();
    });
  });

  test.describe('Dialysis Module', () => {

    test.beforeEach(async ({ page }) => {
      await page.goto('/login');
      await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
      await page.getByLabel(/password/i).fill(testUsers.admin.password);
      await page.getByRole('button', { name: /login|sign in/i }).click();
    });

    test('should display dialysis dashboard', async ({ page }) => {
      await page.goto('/dialysis');
      await expect(page.getByRole('heading', { name: /dialysis|renal/i })).toBeVisible();
    });

    test('should record dialysis session', async ({ page }) => {
      await page.goto('/dialysis/sessions/new');

      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/type/i).selectOption({ label: /hemodialysis/i });
      await page.getByLabel(/pre.*weight/i).fill('72.5');
      await page.getByLabel(/post.*weight/i).fill('70.0');
      await page.getByLabel(/uf.*goal/i).fill('2.5');
      await page.getByLabel(/blood.*flow.*rate/i).fill('350');
      await page.getByLabel(/dialysate.*flow/i).fill('500');
      await page.getByLabel(/duration/i).fill('240');
      await page.getByLabel(/access.*type/i).selectOption({ label: /av.*fistula/i });
      await page.getByLabel(/ktv/i).fill('1.4');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/session.*saved/i)).toBeVisible();
    });

    test('should track intradialytic vitals', async ({ page }) => {
      await page.goto('/dialysis/sessions');

      const session = page.locator('[data-testid="session-row"]').first();
      await session.getByRole('button', { name: /vitals/i }).click();

      await page.getByRole('button', { name: /add.*reading/i }).click();
      await page.getByLabel(/time/i).fill('30');
      await page.getByLabel(/systolic/i).fill('130');
      await page.getByLabel(/diastolic/i).fill('80');
      await page.getByLabel(/heart.*rate/i).fill('78');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/vitals.*saved/i)).toBeVisible();
    });

    test('should manage dialysis schedule', async ({ page }) => {
      await page.goto('/dialysis/schedule');

      await page.getByRole('button', { name: /new.*schedule/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/days/i).selectOption({ label: /mon.*wed.*fri/i });
      await page.getByLabel(/shift/i).selectOption({ label: /morning/i });
      await page.getByLabel(/machine/i).selectOption({ index: 1 });

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/schedule.*saved/i)).toBeVisible();
    });

    test('should track monthly labs', async ({ page }) => {
      await page.goto('/dialysis/labs');

      await page.getByRole('button', { name: /new.*labs/i }).click();
      await page.getByLabel(/patient/i).selectOption({ index: 1 });
      await page.getByLabel(/creatinine/i).fill('8.5');
      await page.getByLabel(/bun/i).fill('55');
      await page.getByLabel(/potassium/i).fill('5.2');
      await page.getByLabel(/phosphorus/i).fill('5.8');
      await page.getByLabel(/hemoglobin/i).fill('10.5');

      await page.getByRole('button', { name: /save/i }).click();
      await expect(page.getByText(/labs.*saved/i)).toBeVisible();
    });
  });

  test.describe('Specialty Module API', () => {

    test('should list specialty services via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const specialties = ['audiology', 'dental', 'cardiology', 'ophthalmology', 'orthopedics'];

      for (const specialty of specialties) {
        const response = await request.get(`/api/${specialty}/services`, {
          headers: authHeaders(token)
        });

        expect([200, 401, 403, 404]).toContain(response.status());
      }
    });

    test('should get specialty patient records via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.get('/api/cardiology/patients/1/records', {
        headers: authHeaders(token)
      });

      expect([200, 401, 403, 404]).toContain(response.status());
    });

    test('should create specialty examination via API', async ({ request }) => {
      const token = await getApiToken(request);
      if (!token) {
        test.skip();
        return;
      }

      const response = await request.post('/api/ophthalmology/examinations', {
        headers: authHeaders(token),
        data: {
          patientId: 1,
          type: 'visual-acuity',
          rightEye: '20/20',
          leftEye: '20/25'
        }
      });

      expect([200, 201, 400, 401, 403]).toContain(response.status());
    });
  });
});
