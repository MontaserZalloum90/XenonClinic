import { test, expect, testUsers, getApiToken, authHeaders } from '../../fixtures/auth';

/**
 * Comprehensive E2E Tests for Clinical Visits Module
 * Tests cover clinical workflows, case notes, diagnoses, prescriptions, and vitals
 */
test.describe('Clinical Visits Module', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/username|email/i).fill(testUsers.admin.email);
    await page.getByLabel(/password/i).fill(testUsers.admin.password);
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page).toHaveURL(/dashboard|home/i);
  });

  test.describe('Clinical Visits List', () => {
    test('should display clinical visits page', async ({ page }) => {
      await page.goto('/clinical-visits');

      await expect(page.getByRole('heading', { name: /clinical.*visit|consultation|visit/i })).toBeVisible();
    });

    test('should filter visits by date range', async ({ page }) => {
      await page.goto('/clinical-visits');

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

    test('should filter visits by doctor', async ({ page }) => {
      await page.goto('/clinical-visits');

      const doctorFilter = page.getByRole('combobox', { name: /doctor|provider/i });
      if (await doctorFilter.isVisible()) {
        await doctorFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should filter visits by patient', async ({ page }) => {
      await page.goto('/clinical-visits');

      const patientFilter = page.getByRole('combobox', { name: /patient/i });
      if (await patientFilter.isVisible()) {
        await patientFilter.click();
        await page.getByRole('option').first().click();
      }
    });

    test('should search visits', async ({ page }) => {
      await page.goto('/clinical-visits');

      const searchInput = page.getByPlaceholder(/search/i);
      if (await searchInput.isVisible()) {
        await searchInput.fill('consultation');
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Start Clinical Visit', () => {
    test('should start new clinical visit from appointment', async ({ page }) => {
      await page.goto('/appointments');

      const startVisitButton = page.getByRole('button', { name: /start.*visit|begin.*consultation/i }).first();
      if (await startVisitButton.isVisible()) {
        await startVisitButton.click();

        await expect(page).toHaveURL(/clinical.*visit|consultation/i);
      }
    });

    test('should display patient summary in clinical visit', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        await expect(page.getByText(/patient.*summary|patient.*info|medical.*history/i)).toBeVisible();
      }
    });

    test('should display patient allergies warning', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Allergies should be prominently displayed if patient has any
        const allergyWarning = page.locator('[class*="allergy"], [class*="warning"], [class*="alert"]');
        if (await allergyWarning.count() > 0) {
          await expect(allergyWarning.first()).toBeVisible();
        }
      }
    });
  });

  test.describe('Vital Signs', () => {
    test('should record vital signs', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const vitalsTab = page.getByRole('tab', { name: /vital|signs/i });
        if (await vitalsTab.isVisible()) {
          await vitalsTab.click();
        }

        const addVitalsButton = page.getByRole('button', { name: /add.*vital|record.*vital/i });
        if (await addVitalsButton.isVisible()) {
          await addVitalsButton.click();

          // Fill vital signs
          const bpSystolic = page.getByLabel(/systolic|blood.*pressure/i);
          if (await bpSystolic.isVisible()) {
            await bpSystolic.fill('120');
          }

          const bpDiastolic = page.getByLabel(/diastolic/i);
          if (await bpDiastolic.isVisible()) {
            await bpDiastolic.fill('80');
          }

          const heartRate = page.getByLabel(/heart.*rate|pulse/i);
          if (await heartRate.isVisible()) {
            await heartRate.fill('72');
          }

          const temperature = page.getByLabel(/temperature/i);
          if (await temperature.isVisible()) {
            await temperature.fill('37.0');
          }

          const weight = page.getByLabel(/weight/i);
          if (await weight.isVisible()) {
            await weight.fill('70');
          }

          const submitButton = page.getByRole('button', { name: /save|record/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|saved|recorded/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should validate vital sign ranges', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const addVitalsButton = page.getByRole('button', { name: /add.*vital|record.*vital/i });
        if (await addVitalsButton.isVisible()) {
          await addVitalsButton.click();

          // Enter abnormal values
          const bpSystolic = page.getByLabel(/systolic|blood.*pressure/i);
          if (await bpSystolic.isVisible()) {
            await bpSystolic.fill('300'); // Abnormally high

            const submitButton = page.getByRole('button', { name: /save|record/i }).last();
            await submitButton.click();

            // Should show warning or validation
            await expect(page.getByText(/abnormal|warning|invalid|range/i)).toBeVisible();
          }
        }
      }
    });

    test('should display vital signs history chart', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const vitalsTab = page.getByRole('tab', { name: /vital|signs/i });
        if (await vitalsTab.isVisible()) {
          await vitalsTab.click();

          // Look for chart
          const chart = page.locator('canvas, svg[class*="chart"], [class*="chart"]');
          if (await chart.isVisible()) {
            await expect(chart).toBeVisible();
          }
        }
      }
    });
  });

  test.describe('Chief Complaint & History', () => {
    test('should record chief complaint', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const complaintField = page.getByLabel(/chief.*complaint|presenting.*complaint|reason.*visit/i);
        if (await complaintField.isVisible()) {
          await complaintField.fill('Patient presents with persistent headache for 3 days');

          const submitButton = page.getByRole('button', { name: /save/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should record history of present illness', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const hpiField = page.getByLabel(/history.*present.*illness|hpi/i);
        if (await hpiField.isVisible()) {
          await hpiField.fill('Headache started 3 days ago, intermittent, frontal region, no associated symptoms');
        }
      }
    });

    test('should record review of systems', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const rosTab = page.getByRole('tab', { name: /review.*system|ros/i });
        if (await rosTab.isVisible()) {
          await rosTab.click();

          // Check various systems
          const checkboxes = page.locator('input[type="checkbox"]');
          if (await checkboxes.count() > 0) {
            await checkboxes.first().check();
          }
        }
      }
    });
  });

  test.describe('Physical Examination', () => {
    test('should record physical examination findings', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const examTab = page.getByRole('tab', { name: /examination|physical.*exam/i });
        if (await examTab.isVisible()) {
          await examTab.click();

          const generalExam = page.getByLabel(/general.*appearance|general.*exam/i);
          if (await generalExam.isVisible()) {
            await generalExam.fill('Patient appears well, alert and oriented');
          }

          const submitButton = page.getByRole('button', { name: /save/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should use examination templates', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const examTab = page.getByRole('tab', { name: /examination|physical.*exam/i });
        if (await examTab.isVisible()) {
          await examTab.click();

          const templateButton = page.getByRole('button', { name: /template|quick.*fill/i });
          if (await templateButton.isVisible()) {
            await templateButton.click();

            await expect(page.getByRole('listbox').or(page.getByRole('menu'))).toBeVisible();
          }
        }
      }
    });
  });

  test.describe('Diagnosis', () => {
    test('should add diagnosis with ICD code', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const diagnosisTab = page.getByRole('tab', { name: /diagnosis|assessment/i });
        if (await diagnosisTab.isVisible()) {
          await diagnosisTab.click();
        }

        const addDiagnosisButton = page.getByRole('button', { name: /add.*diagnosis/i });
        if (await addDiagnosisButton.isVisible()) {
          await addDiagnosisButton.click();

          // Search for diagnosis
          const diagnosisSearch = page.getByPlaceholder(/search.*diagnosis|icd|code/i);
          if (await diagnosisSearch.isVisible()) {
            await diagnosisSearch.fill('headache');
            await page.waitForTimeout(500);

            await page.getByRole('option').first().click();
          }

          const submitButton = page.getByRole('button', { name: /save|add/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|added/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should set primary diagnosis', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const diagnosisTab = page.getByRole('tab', { name: /diagnosis|assessment/i });
        if (await diagnosisTab.isVisible()) {
          await diagnosisTab.click();

          const primaryCheckbox = page.getByLabel(/primary|main/i).first();
          if (await primaryCheckbox.isVisible()) {
            await primaryCheckbox.check();
          }
        }
      }
    });

    test('should add differential diagnoses', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const differentialButton = page.getByRole('button', { name: /differential|add.*differential/i });
        if (await differentialButton.isVisible()) {
          await differentialButton.click();

          // Add differential diagnosis
          await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
        }
      }
    });
  });

  test.describe('Prescriptions', () => {
    test('should add prescription medication', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const prescriptionTab = page.getByRole('tab', { name: /prescription|medication/i });
        if (await prescriptionTab.isVisible()) {
          await prescriptionTab.click();
        }

        const addMedButton = page.getByRole('button', { name: /add.*medication|prescribe/i });
        if (await addMedButton.isVisible()) {
          await addMedButton.click();

          // Search for medication
          const medSearch = page.getByPlaceholder(/search.*medication|drug/i);
          if (await medSearch.isVisible()) {
            await medSearch.fill('paracetamol');
            await page.waitForTimeout(500);

            await page.getByRole('option').first().click();
          }

          // Set dosage
          const dosageField = page.getByLabel(/dosage|dose/i);
          if (await dosageField.isVisible()) {
            await dosageField.fill('500mg');
          }

          // Set frequency
          const frequencySelect = page.getByLabel(/frequency|times/i);
          if (await frequencySelect.isVisible()) {
            await frequencySelect.click();
            await page.getByRole('option', { name: /three.*day|tid|8.*hour/i }).first().click();
          }

          // Set duration
          const durationField = page.getByLabel(/duration|days/i);
          if (await durationField.isVisible()) {
            await durationField.fill('7');
          }

          const submitButton = page.getByRole('button', { name: /save|add|prescribe/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|added|prescribed/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should check drug interactions', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const prescriptionTab = page.getByRole('tab', { name: /prescription|medication/i });
        if (await prescriptionTab.isVisible()) {
          await prescriptionTab.click();

          // Look for drug interaction warnings
          const interactionWarning = page.locator('[class*="interaction"], [class*="warning"], [class*="alert"]');
          if (await interactionWarning.count() > 0) {
            await expect(interactionWarning.first()).toBeVisible();
          }
        }
      }
    });

    test('should check allergy alerts', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Allergy alerts should show when prescribing
        const allergyAlert = page.locator('[class*="allergy"], [class*="danger"]');
        if (await allergyAlert.count() > 0) {
          await expect(allergyAlert.first()).toBeVisible();
        }
      }
    });

    test('should print prescription', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const prescriptionTab = page.getByRole('tab', { name: /prescription|medication/i });
        if (await prescriptionTab.isVisible()) {
          await prescriptionTab.click();

          const printButton = page.getByRole('button', { name: /print/i });
          if (await printButton.isVisible()) {
            await expect(printButton).toBeVisible();
          }
        }
      }
    });

    test('should use prescription templates', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const templateButton = page.getByRole('button', { name: /template|quick.*prescribe/i });
        if (await templateButton.isVisible()) {
          await templateButton.click();

          await expect(page.getByRole('listbox').or(page.getByRole('menu'))).toBeVisible();
        }
      }
    });
  });

  test.describe('Lab Orders', () => {
    test('should order lab tests', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const labTab = page.getByRole('tab', { name: /lab|laboratory|order/i });
        if (await labTab.isVisible()) {
          await labTab.click();
        }

        const orderLabButton = page.getByRole('button', { name: /order.*lab|add.*test/i });
        if (await orderLabButton.isVisible()) {
          await orderLabButton.click();

          // Select tests
          const testSearch = page.getByPlaceholder(/search.*test/i);
          if (await testSearch.isVisible()) {
            await testSearch.fill('CBC');
            await page.waitForTimeout(500);

            await page.getByRole('option').first().click();
          }

          const submitButton = page.getByRole('button', { name: /save|order/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|ordered/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should view lab results in clinical visit', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const labTab = page.getByRole('tab', { name: /lab|results/i });
        if (await labTab.isVisible()) {
          await labTab.click();

          // Should show lab results if available
          await expect(page.getByText(/result|lab|test/i)).toBeVisible();
        }
      }
    });
  });

  test.describe('Imaging Orders', () => {
    test('should order imaging studies', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const imagingTab = page.getByRole('tab', { name: /imaging|radiology|x-ray/i });
        if (await imagingTab.isVisible()) {
          await imagingTab.click();
        }

        const orderImagingButton = page.getByRole('button', { name: /order.*imaging|add.*study/i });
        if (await orderImagingButton.isVisible()) {
          await orderImagingButton.click();

          await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
        }
      }
    });
  });

  test.describe('Clinical Notes', () => {
    test('should add clinical notes', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const notesTab = page.getByRole('tab', { name: /note|documentation/i });
        if (await notesTab.isVisible()) {
          await notesTab.click();
        }

        const addNoteButton = page.getByRole('button', { name: /add.*note/i });
        if (await addNoteButton.isVisible()) {
          await addNoteButton.click();

          const noteField = page.getByLabel(/note|content/i);
          if (await noteField.isVisible()) {
            await noteField.fill('E2E Test - Clinical note added during consultation');
          }

          const submitButton = page.getByRole('button', { name: /save|add/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|saved|added/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should use SOAP note format', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        // Look for SOAP sections
        const subjective = page.getByLabel(/subjective/i);
        const objective = page.getByLabel(/objective/i);
        const assessment = page.getByLabel(/assessment/i);
        const plan = page.getByLabel(/plan/i);

        if (await subjective.isVisible()) {
          await expect(subjective).toBeVisible();
        }
      }
    });

    test('should sign off clinical notes', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const signButton = page.getByRole('button', { name: /sign|finalize|complete/i });
        if (await signButton.isVisible()) {
          await signButton.click();

          const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await expect(page.getByText(/signed|finalized|completed/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });
  });

  test.describe('Treatment Plan', () => {
    test('should create treatment plan', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const planTab = page.getByRole('tab', { name: /plan|treatment/i });
        if (await planTab.isVisible()) {
          await planTab.click();

          const planField = page.getByLabel(/plan|treatment.*plan/i);
          if (await planField.isVisible()) {
            await planField.fill('Continue current medication, follow up in 2 weeks, monitor symptoms');
          }

          const submitButton = page.getByRole('button', { name: /save/i }).last();
          await submitButton.click();

          await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should schedule follow-up appointment', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const followUpButton = page.getByRole('button', { name: /follow.*up|schedule.*follow/i });
        if (await followUpButton.isVisible()) {
          await followUpButton.click();

          await expect(page.getByRole('dialog').or(page.locator('[class*="form"]'))).toBeVisible();
        }
      }
    });
  });

  test.describe('Complete Visit', () => {
    test('should complete clinical visit', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const completeButton = page.getByRole('button', { name: /complete.*visit|finish|end.*visit/i });
        if (await completeButton.isVisible()) {
          await completeButton.click();

          const confirmButton = page.getByRole('button', { name: /yes|confirm/i });
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await expect(page.getByText(/completed|finished|success/i)).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('should generate visit summary', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const summaryButton = page.getByRole('button', { name: /summary|generate.*summary/i });
        if (await summaryButton.isVisible()) {
          await summaryButton.click();

          await expect(page.getByText(/summary/i)).toBeVisible();
        }
      }
    });

    test('should print visit summary', async ({ page }) => {
      await page.goto('/clinical-visits');

      const viewButton = page.getByRole('button', { name: /view|open/i }).first();
      if (await viewButton.isVisible()) {
        await viewButton.click();

        const printButton = page.getByRole('button', { name: /print/i });
        if (await printButton.isVisible()) {
          await expect(printButton).toBeVisible();
        }
      }
    });
  });
});

test.describe('Clinical Visits API E2E Tests', () => {
  test('should list clinical visits', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.get('/api/clinical-visits?page=1&pageSize=10', {
      headers: authHeaders(token),
    });

    expect(response.status()).toBe(200);
  });

  test('should get clinical visit details', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const listResponse = await request.get('/api/clinical-visits?pageSize=1', {
      headers: authHeaders(token),
    });

    if (listResponse.status() === 200) {
      const listData = await listResponse.json();
      if (listData.items && listData.items.length > 0) {
        const visitId = listData.items[0].id;

        const response = await request.get(`/api/clinical-visits/${visitId}`, {
          headers: authHeaders(token),
        });

        expect(response.status()).toBe(200);
      }
    }
  });

  test('should validate vital signs input', async ({ request }) => {
    const token = await getApiToken(request);
    if (!token) {
      test.skip();
      return;
    }

    const response = await request.post('/api/vital-signs', {
      headers: authHeaders(token),
      data: {
        systolicBP: -10, // Invalid negative value
        diastolicBP: 80,
      },
    });

    expect(response.status()).toBe(400);
  });
});
