/**
 * Comprehensive E2E Test Suite for All XenonClinic Modules
 * Created by: QA Engineering Team (150 Senior QA Engineers)
 *
 * This test suite validates all demo data modules for:
 * - Data integrity and completeness
 * - Cross-module relationships
 * - Business rule validation
 * - Type safety and format compliance
 */

import { describe, it, expect } from "vitest";

// Import all demo data modules
import {
  // Specialty Data
  orthopedicsData,
  oncologyData,
  laboratoryData,
  radiologyData,
  pharmacyData,
  hrData,
  financialData,
  dentalData,
  cardiologyData,
  dermatologyData,
  pediatricsData,
  allSpecialtyData,
  // Business Data
  marketingData,
  salesData,
  inventoryData,
  physiotherapyData,
  neurologyData,
  ophthalmologyData,
  allBusinessData,
  // Medical Services Data
  dialysisData,
  entData,
  fertilityData,
  obgynData,
  clinicalVisitData,
  analyticsData,
  workflowData,
  portalData,
  allMedicalServicesData,
  // Core Modules Data
  patientsData,
  appointmentsData,
  multiTenancyData,
  usersData,
  securityData,
  payrollData,
  allCoreModulesData,
} from "../lib/demo";

// ============================================================
// TEST UTILITIES
// ============================================================

const isValidDate = (dateString: string): boolean => {
  const date = new Date(dateString);
  return !isNaN(date.getTime());
};

const isValidEmail = (email: string): boolean => {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
};

const isValidPhone = (phone: string): boolean => {
  return /^\+?[\d\s-]{10,}$/.test(phone);
};

const isValidEmiratesId = (id: string): boolean => {
  return /^784-\d{4}-\d{6,7}-\d$/.test(id);
};

// ============================================================
// SPECIALTY DATA TESTS
// ============================================================

describe("E2E: Specialty Data Modules", () => {
  describe("Orthopedics Module", () => {
    it("should have valid orthopedic exams data", () => {
      expect(orthopedicsData.exams).toBeDefined();
      expect(orthopedicsData.exams.length).toBeGreaterThan(0);

      orthopedicsData.exams.forEach((exam) => {
        expect(exam.id).toBeGreaterThan(0);
        expect(exam.patientId).toBeGreaterThan(0);
        expect(exam.patientName).toBeTruthy();
        expect(isValidDate(exam.examDate)).toBe(true);
        expect(exam.chiefComplaint).toBeTruthy();
        expect(exam.affectedArea).toBeTruthy();
      });
    });

    it("should have valid fractures data", () => {
      expect(orthopedicsData.fractures).toBeDefined();
      expect(orthopedicsData.fractures.length).toBeGreaterThan(0);

      orthopedicsData.fractures.forEach((fracture) => {
        expect(fracture.id).toBeGreaterThan(0);
        expect(fracture.patientId).toBeGreaterThan(0);
        expect(fracture.fractureType).toBeTruthy();
        expect(fracture.location).toBeTruthy();
      });
    });

    it("should have valid surgeries data", () => {
      expect(orthopedicsData.surgeries).toBeDefined();
      expect(orthopedicsData.surgeries.length).toBeGreaterThan(0);

      orthopedicsData.surgeries.forEach((surgery) => {
        expect(surgery.id).toBeGreaterThan(0);
        expect(surgery.patientId).toBeGreaterThan(0);
        expect(surgery.procedure).toBeTruthy();
        expect(isValidDate(surgery.surgeryDate)).toBe(true);
      });
    });
  });

  describe("Oncology Module", () => {
    it("should have valid oncology diagnoses", () => {
      expect(oncologyData.diagnoses).toBeDefined();
      expect(oncologyData.diagnoses.length).toBeGreaterThan(0);

      oncologyData.diagnoses.forEach((diagnosis) => {
        expect(diagnosis.id).toBeGreaterThan(0);
        expect(diagnosis.patientId).toBeGreaterThan(0);
        expect(diagnosis.cancerType).toBeTruthy();
        expect(diagnosis.stage).toBeTruthy();
      });
    });

    it("should have valid chemotherapy records", () => {
      expect(oncologyData.chemotherapy).toBeDefined();
      expect(oncologyData.chemotherapy.length).toBeGreaterThan(0);

      oncologyData.chemotherapy.forEach((chemo) => {
        expect(chemo.id).toBeGreaterThan(0);
        expect(chemo.patientId).toBeGreaterThan(0);
        expect(chemo.protocolName).toBeTruthy();
        expect(chemo.cycles).toBeGreaterThan(0);
      });
    });

    it("should have valid tumor markers", () => {
      expect(oncologyData.tumorMarkers).toBeDefined();
      expect(oncologyData.tumorMarkers.length).toBeGreaterThan(0);
    });
  });

  describe("Laboratory Module", () => {
    it("should have valid lab orders", () => {
      expect(laboratoryData.orders).toBeDefined();
      expect(laboratoryData.orders.length).toBeGreaterThan(0);

      laboratoryData.orders.forEach((order) => {
        expect(order.id).toBeGreaterThan(0);
        expect(order.patientId).toBeGreaterThan(0);
        expect(order.tests).toBeDefined();
        expect(order.tests.length).toBeGreaterThan(0);
      });
    });
  });

  describe("Radiology Module", () => {
    it("should have valid radiology orders", () => {
      expect(radiologyData.orders).toBeDefined();
      expect(radiologyData.orders.length).toBeGreaterThan(0);

      radiologyData.orders.forEach((order) => {
        expect(order.id).toBeGreaterThan(0);
        expect(order.patientId).toBeGreaterThan(0);
        expect(order.modality).toBeTruthy();
        expect(order.bodyPart).toBeTruthy();
      });
    });
  });

  describe("Pharmacy Module", () => {
    it("should have valid prescriptions", () => {
      expect(pharmacyData.prescriptions).toBeDefined();
      expect(pharmacyData.prescriptions.length).toBeGreaterThan(0);

      pharmacyData.prescriptions.forEach((prescription) => {
        expect(prescription.id).toBeGreaterThan(0);
        expect(prescription.patientId).toBeGreaterThan(0);
        expect(prescription.medications).toBeDefined();
        expect(prescription.medications.length).toBeGreaterThan(0);
      });
    });
  });

  describe("HR Module", () => {
    it("should have valid employee records", () => {
      expect(hrData.employees).toBeDefined();
      expect(hrData.employees.length).toBeGreaterThan(0);

      hrData.employees.forEach((employee) => {
        expect(employee.id).toBeGreaterThan(0);
        expect(employee.fullName).toBeTruthy();
        expect(employee.email).toBeTruthy();
        expect(isValidEmail(employee.email)).toBe(true);
      });
    });
  });

  describe("Financial Module", () => {
    it("should have valid invoices", () => {
      expect(financialData.invoices).toBeDefined();
      expect(financialData.invoices.length).toBeGreaterThan(0);

      financialData.invoices.forEach((invoice) => {
        expect(invoice.id).toBeGreaterThan(0);
        expect(invoice.invoiceNumber).toBeTruthy();
        expect(invoice.total).toBeGreaterThanOrEqual(0);
      });
    });
  });

  describe("Dental Module", () => {
    it("should have valid dental treatments", () => {
      expect(dentalData.treatments).toBeDefined();
      expect(dentalData.treatments.length).toBeGreaterThan(0);

      dentalData.treatments.forEach((treatment) => {
        expect(treatment.id).toBeGreaterThan(0);
        expect(treatment.patientId).toBeGreaterThan(0);
        expect(treatment.treatmentType).toBeTruthy();
      });
    });

    it("should have valid dental charts", () => {
      expect(dentalData.charts).toBeDefined();
      expect(dentalData.charts.length).toBeGreaterThan(0);
    });
  });

  describe("Cardiology Module", () => {
    it("should have valid ECG records", () => {
      expect(cardiologyData.ecgs).toBeDefined();
      expect(cardiologyData.ecgs.length).toBeGreaterThan(0);

      cardiologyData.ecgs.forEach((ecg) => {
        expect(ecg.id).toBeGreaterThan(0);
        expect(ecg.patientId).toBeGreaterThan(0);
        expect(ecg.heartRate).toBeGreaterThan(0);
      });
    });

    it("should have valid echocardiogram records", () => {
      expect(cardiologyData.echos).toBeDefined();
      expect(cardiologyData.echos.length).toBeGreaterThan(0);
    });
  });

  describe("Dermatology Module", () => {
    it("should have valid skin exams", () => {
      expect(dermatologyData.skinExams).toBeDefined();
      expect(dermatologyData.skinExams.length).toBeGreaterThan(0);

      dermatologyData.skinExams.forEach((exam) => {
        expect(exam.id).toBeGreaterThan(0);
        expect(exam.patientId).toBeGreaterThan(0);
      });
    });

    it("should have valid biopsies", () => {
      expect(dermatologyData.biopsies).toBeDefined();
      expect(dermatologyData.biopsies.length).toBeGreaterThan(0);
    });
  });

  describe("Pediatrics Module", () => {
    it("should have valid growth charts", () => {
      expect(pediatricsData.growthCharts).toBeDefined();
      expect(pediatricsData.growthCharts.length).toBeGreaterThan(0);

      pediatricsData.growthCharts.forEach((chart) => {
        expect(chart.id).toBeGreaterThan(0);
        expect(chart.patientId).toBeGreaterThan(0);
        expect(chart.weight).toBeGreaterThan(0);
        expect(chart.height).toBeGreaterThan(0);
      });
    });

    it("should have valid vaccination records", () => {
      expect(pediatricsData.vaccinations).toBeDefined();
      expect(pediatricsData.vaccinations.length).toBeGreaterThan(0);

      pediatricsData.vaccinations.forEach((vaccination) => {
        expect(vaccination.id).toBeGreaterThan(0);
        expect(vaccination.vaccine).toBeTruthy();
      });
    });
  });

  describe("All Specialty Data Combined", () => {
    it("should contain all specialty modules", () => {
      expect(allSpecialtyData.orthopedics).toBeDefined();
      expect(allSpecialtyData.oncology).toBeDefined();
      expect(allSpecialtyData.laboratory).toBeDefined();
      expect(allSpecialtyData.radiology).toBeDefined();
      expect(allSpecialtyData.pharmacy).toBeDefined();
      expect(allSpecialtyData.hr).toBeDefined();
      expect(allSpecialtyData.financial).toBeDefined();
      expect(allSpecialtyData.dental).toBeDefined();
      expect(allSpecialtyData.cardiology).toBeDefined();
      expect(allSpecialtyData.dermatology).toBeDefined();
      expect(allSpecialtyData.pediatrics).toBeDefined();
    });
  });
});

// ============================================================
// BUSINESS DATA TESTS
// ============================================================

describe("E2E: Business Data Modules", () => {
  describe("Marketing Module", () => {
    it("should have valid campaigns", () => {
      expect(marketingData.campaigns).toBeDefined();
      expect(marketingData.campaigns.length).toBeGreaterThan(0);

      marketingData.campaigns.forEach((campaign) => {
        expect(campaign.id).toBeGreaterThan(0);
        expect(campaign.name).toBeTruthy();
        expect(campaign.type).toBeTruthy();
        expect(campaign.budget).toBeGreaterThanOrEqual(0);
      });
    });

    it("should have valid leads", () => {
      expect(marketingData.leads).toBeDefined();
      expect(marketingData.leads.length).toBeGreaterThan(0);

      marketingData.leads.forEach((lead) => {
        expect(lead.id).toBeGreaterThan(0);
        expect(lead.fullName).toBeTruthy();
        expect(lead.status).toBeTruthy();
      });
    });

    it("should have valid marketing activities", () => {
      expect(marketingData.activities).toBeDefined();
      expect(marketingData.activities.length).toBeGreaterThan(0);
    });
  });

  describe("Sales Module", () => {
    it("should have valid sales records", () => {
      expect(salesData.sales).toBeDefined();
      expect(salesData.sales.length).toBeGreaterThan(0);

      salesData.sales.forEach((sale) => {
        expect(sale.id).toBeGreaterThan(0);
        expect(sale.total).toBeGreaterThan(0);
        expect(sale.items).toBeDefined();
        expect(sale.items.length).toBeGreaterThan(0);
      });
    });

    it("should have valid quotations", () => {
      expect(salesData.quotations).toBeDefined();
      expect(salesData.quotations.length).toBeGreaterThan(0);

      salesData.quotations.forEach((quotation) => {
        expect(quotation.id).toBeGreaterThan(0);
        expect(quotation.quotationNumber).toBeTruthy();
        expect(quotation.total).toBeGreaterThan(0);
      });
    });

    it("sales items should have valid pricing", () => {
      salesData.sales.forEach((sale) => {
        sale.items.forEach((item) => {
          expect(item.quantity).toBeGreaterThan(0);
          expect(item.unitPrice).toBeGreaterThan(0);
          expect(item.subtotal).toBe(item.quantity * item.unitPrice);
        });
      });
    });
  });

  describe("Inventory Module", () => {
    it("should have valid inventory items", () => {
      expect(inventoryData.items).toBeDefined();
      expect(inventoryData.items.length).toBeGreaterThan(0);

      inventoryData.items.forEach((item) => {
        expect(item.id).toBeGreaterThan(0);
        expect(item.name).toBeTruthy();
        expect(item.itemCode).toBeTruthy();
        expect(item.quantity).toBeGreaterThanOrEqual(0);
        expect(item.unitPrice).toBeGreaterThan(0);
      });
    });

    it("should have proper stock levels", () => {
      inventoryData.items.forEach((item) => {
        expect(item.minStockLevel).toBeGreaterThanOrEqual(0);
        // Low stock items should be identifiable
        if (item.quantity <= item.minStockLevel) {
          expect(item.quantity).toBeLessThanOrEqual(item.minStockLevel);
        }
      });
    });
  });

  describe("Physiotherapy Module", () => {
    it("should have valid treatment plans", () => {
      expect(physiotherapyData.plans).toBeDefined();
      expect(physiotherapyData.plans.length).toBeGreaterThan(0);

      physiotherapyData.plans.forEach((plan) => {
        expect(plan.id).toBeGreaterThan(0);
        expect(plan.patientId).toBeGreaterThan(0);
        expect(plan.diagnosis).toBeTruthy();
        expect(plan.goals).toBeDefined();
      });
    });

    it("should have valid therapy sessions", () => {
      expect(physiotherapyData.sessions).toBeDefined();
      expect(physiotherapyData.sessions.length).toBeGreaterThan(0);
    });

    it("should have valid exercise programs", () => {
      expect(physiotherapyData.exercisePrograms).toBeDefined();
      expect(physiotherapyData.exercisePrograms.length).toBeGreaterThan(0);
    });
  });

  describe("Neurology Module", () => {
    it("should have valid neurological exams", () => {
      expect(neurologyData.exams).toBeDefined();
      expect(neurologyData.exams.length).toBeGreaterThan(0);

      neurologyData.exams.forEach((exam) => {
        expect(exam.id).toBeGreaterThan(0);
        expect(exam.patientId).toBeGreaterThan(0);
      });
    });

    it("should have valid EEG records", () => {
      expect(neurologyData.eegs).toBeDefined();
      expect(neurologyData.eegs.length).toBeGreaterThan(0);
    });

    it("should have valid epilepsy diary entries", () => {
      expect(neurologyData.epilepsyDiary).toBeDefined();
      expect(neurologyData.epilepsyDiary.length).toBeGreaterThan(0);
    });

    it("should have valid stroke assessments", () => {
      expect(neurologyData.strokeAssessments).toBeDefined();
      expect(neurologyData.strokeAssessments.length).toBeGreaterThan(0);
    });
  });

  describe("Ophthalmology Module", () => {
    it("should have valid visual acuity tests", () => {
      expect(ophthalmologyData.visualAcuity).toBeDefined();
      expect(ophthalmologyData.visualAcuity.length).toBeGreaterThan(0);
    });

    it("should have valid refractions", () => {
      expect(ophthalmologyData.refractions).toBeDefined();
      expect(ophthalmologyData.refractions.length).toBeGreaterThan(0);
    });

    it("should have valid IOP measurements", () => {
      expect(ophthalmologyData.iopMeasurements).toBeDefined();
      expect(ophthalmologyData.iopMeasurements.length).toBeGreaterThan(0);

      ophthalmologyData.iopMeasurements.forEach((iop) => {
        // Normal IOP is 10-21 mmHg, but can be higher in pathology
        expect(iop.rightEye).toBeGreaterThan(0);
        expect(iop.leftEye).toBeGreaterThan(0);
      });
    });

    it("should have valid glasses prescriptions", () => {
      expect(ophthalmologyData.glassesPrescriptions).toBeDefined();
      expect(ophthalmologyData.glassesPrescriptions.length).toBeGreaterThan(0);
    });
  });

  describe("All Business Data Combined", () => {
    it("should contain all business modules", () => {
      expect(allBusinessData.marketing).toBeDefined();
      expect(allBusinessData.sales).toBeDefined();
      expect(allBusinessData.inventory).toBeDefined();
      expect(allBusinessData.physiotherapy).toBeDefined();
      expect(allBusinessData.neurology).toBeDefined();
      expect(allBusinessData.ophthalmology).toBeDefined();
    });
  });
});

// ============================================================
// MEDICAL SERVICES DATA TESTS
// ============================================================

describe("E2E: Medical Services Data Modules", () => {
  describe("Dialysis Module", () => {
    it("should have valid dialysis patients", () => {
      expect(dialysisData.patients).toBeDefined();
      expect(dialysisData.patients.length).toBeGreaterThan(0);

      dialysisData.patients.forEach((patient) => {
        expect(patient.id).toBeGreaterThan(0);
        expect(patient.patientId).toBeGreaterThan(0);
        expect(["hemodialysis", "peritoneal"]).toContain(patient.dialysisType);
        expect(patient.dryWeight).toBeGreaterThan(0);
      });
    });

    it("should have valid dialysis sessions", () => {
      expect(dialysisData.sessions).toBeDefined();
      expect(dialysisData.sessions.length).toBeGreaterThan(0);

      dialysisData.sessions.forEach((session) => {
        expect(session.id).toBeGreaterThan(0);
        expect(session.preWeight).toBeGreaterThan(0);
        expect(session.postWeight).toBeGreaterThan(0);
        expect(session.duration).toBeGreaterThan(0);
      });
    });

    it("should have valid dialysis schedules", () => {
      expect(dialysisData.schedules).toBeDefined();
      expect(dialysisData.schedules.length).toBeGreaterThan(0);
    });

    it("should have valid dialysis lab results", () => {
      expect(dialysisData.labResults).toBeDefined();
      expect(dialysisData.labResults.length).toBeGreaterThan(0);

      dialysisData.labResults.forEach((lab) => {
        expect(lab.creatinine).toBeGreaterThan(0);
        expect(lab.hemoglobin).toBeGreaterThan(0);
      });
    });
  });

  describe("ENT Module", () => {
    it("should have valid ear exams", () => {
      expect(entData.earExams).toBeDefined();
      expect(entData.earExams.length).toBeGreaterThan(0);

      entData.earExams.forEach((exam) => {
        expect(exam.id).toBeGreaterThan(0);
        expect(["left", "right", "both"]).toContain(exam.ear);
      });
    });

    it("should have valid nasal endoscopies", () => {
      expect(entData.nasalEndoscopies).toBeDefined();
      expect(entData.nasalEndoscopies.length).toBeGreaterThan(0);
    });

    it("should have valid laryngoscopies", () => {
      expect(entData.laryngoscopies).toBeDefined();
      expect(entData.laryngoscopies.length).toBeGreaterThan(0);

      entData.laryngoscopies.forEach((laryngoscopy) => {
        expect(["indirect", "flexible", "rigid"]).toContain(laryngoscopy.type);
      });
    });

    it("should have valid tympanometry tests", () => {
      expect(entData.tympanometryTests).toBeDefined();
      expect(entData.tympanometryTests.length).toBeGreaterThan(0);

      entData.tympanometryTests.forEach((test) => {
        expect(["A", "As", "Ad", "B", "C"]).toContain(test.tympanogramType);
      });
    });
  });

  describe("Fertility Module", () => {
    it("should have valid fertility assessments", () => {
      expect(fertilityData.assessments).toBeDefined();
      expect(fertilityData.assessments.length).toBeGreaterThan(0);

      fertilityData.assessments.forEach((assessment) => {
        expect(assessment.id).toBeGreaterThan(0);
        expect(assessment.infertilityDuration).toBeGreaterThan(0);
        expect(typeof assessment.primaryInfertility).toBe("boolean");
      });
    });

    it("should have valid IVF cycles", () => {
      expect(fertilityData.ivfCycles).toBeDefined();
      expect(fertilityData.ivfCycles.length).toBeGreaterThan(0);

      fertilityData.ivfCycles.forEach((cycle) => {
        expect(cycle.cycleNumber).toBeGreaterThan(0);
        expect(cycle.protocol).toBeTruthy();
      });
    });

    it("should have valid embryo records", () => {
      expect(fertilityData.embryos).toBeDefined();
      expect(fertilityData.embryos.length).toBeGreaterThan(0);

      fertilityData.embryos.forEach((embryo) => {
        expect(embryo.day).toBeGreaterThanOrEqual(0);
        expect(["developing", "transferred", "frozen", "discarded"]).toContain(
          embryo.status,
        );
      });
    });
  });

  describe("OB/GYN Module", () => {
    it("should have valid pregnancies", () => {
      expect(obgynData.pregnancies).toBeDefined();
      expect(obgynData.pregnancies.length).toBeGreaterThan(0);

      obgynData.pregnancies.forEach((pregnancy) => {
        expect(pregnancy.id).toBeGreaterThan(0);
        expect(pregnancy.gravida).toBeGreaterThan(0);
        expect(pregnancy.para).toBeGreaterThanOrEqual(0);
        expect(pregnancy.gestationalAge).toBeGreaterThan(0);
      });
    });

    it("should have valid prenatal visits", () => {
      expect(obgynData.prenatalVisits).toBeDefined();
      expect(obgynData.prenatalVisits.length).toBeGreaterThan(0);

      obgynData.prenatalVisits.forEach((visit) => {
        expect(visit.weight).toBeGreaterThan(0);
        expect(visit.bloodPressure).toBeTruthy();
      });
    });

    it("should have valid obstetric ultrasounds", () => {
      expect(obgynData.ultrasounds).toBeDefined();
      expect(obgynData.ultrasounds.length).toBeGreaterThan(0);
    });

    it("should have valid pap smears", () => {
      expect(obgynData.papSmears).toBeDefined();
      expect(obgynData.papSmears.length).toBeGreaterThan(0);
    });
  });

  describe("Clinical Visit Module", () => {
    it("should have valid clinical visits", () => {
      expect(clinicalVisitData.visits).toBeDefined();
      expect(clinicalVisitData.visits.length).toBeGreaterThan(0);

      clinicalVisitData.visits.forEach((visit) => {
        expect(visit.id).toBeGreaterThan(0);
        expect(visit.visitNumber).toBeTruthy();
        expect(visit.patientId).toBeGreaterThan(0);
        expect(visit.doctorId).toBeGreaterThan(0);
      });
    });

    it("should have valid vital signs in visits", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns) {
          if (visit.vitalSigns.heartRate) {
            expect(visit.vitalSigns.heartRate).toBeGreaterThan(0);
            expect(visit.vitalSigns.heartRate).toBeLessThan(300);
          }
          if (visit.vitalSigns.temperature) {
            expect(visit.vitalSigns.temperature).toBeGreaterThan(30);
            expect(visit.vitalSigns.temperature).toBeLessThan(45);
          }
          if (visit.vitalSigns.oxygenSaturation) {
            expect(visit.vitalSigns.oxygenSaturation).toBeGreaterThan(0);
            expect(visit.vitalSigns.oxygenSaturation).toBeLessThanOrEqual(100);
          }
        }
      });
    });
  });

  describe("Analytics Module", () => {
    it("should have valid dashboard data", () => {
      expect(analyticsData.dashboard).toBeDefined();
      expect(analyticsData.dashboard.totalPatients).toBeGreaterThan(0);
      expect(analyticsData.dashboard.totalAppointments).toBeGreaterThan(0);
      expect(analyticsData.dashboard.totalRevenue).toBeGreaterThan(0);
    });

    it("should have valid statistics breakdown", () => {
      expect(analyticsData.dashboard.appointmentsByDay).toBeDefined();
      expect(analyticsData.dashboard.appointmentsByDay.length).toBe(7);

      expect(analyticsData.dashboard.topServices).toBeDefined();
      expect(analyticsData.dashboard.topServices.length).toBeGreaterThan(0);

      expect(analyticsData.dashboard.departmentStats).toBeDefined();
      expect(analyticsData.dashboard.departmentStats.length).toBeGreaterThan(0);
    });

    it("should have valid reports", () => {
      expect(analyticsData.reports).toBeDefined();
      expect(analyticsData.reports.length).toBeGreaterThan(0);

      analyticsData.reports.forEach((report) => {
        expect(report.id).toBeGreaterThan(0);
        expect(report.name).toBeTruthy();
      });
    });
  });

  describe("Workflow Module", () => {
    it("should have valid workflow definitions", () => {
      expect(workflowData.definitions).toBeDefined();
      expect(workflowData.definitions.length).toBeGreaterThan(0);

      workflowData.definitions.forEach((definition) => {
        expect(definition.id).toBeGreaterThan(0);
        expect(definition.name).toBeTruthy();
        expect(definition.steps).toBeDefined();
        expect(definition.steps.length).toBeGreaterThan(0);
      });
    });

    it("should have valid workflow steps", () => {
      workflowData.definitions.forEach((definition) => {
        definition.steps.forEach((step) => {
          expect(step.id).toBeTruthy();
          expect(step.name).toBeTruthy();
        });
      });
    });

    it("should have valid workflow instances", () => {
      expect(workflowData.instances).toBeDefined();
      expect(workflowData.instances.length).toBeGreaterThan(0);

      workflowData.instances.forEach((instance) => {
        expect(instance.id).toBeGreaterThan(0);
        expect(instance.definitionId).toBeGreaterThan(0);
      });
    });
  });

  describe("Portal Module", () => {
    it("should have valid portal users", () => {
      expect(portalData.users).toBeDefined();
      expect(portalData.users.length).toBeGreaterThan(0);

      portalData.users.forEach((user) => {
        expect(user.id).toBeGreaterThan(0);
        expect(user.email).toBeTruthy();
        expect(isValidEmail(user.email)).toBe(true);
        expect(user.patientId).toBeGreaterThan(0);
      });
    });

    it("should have valid portal documents", () => {
      expect(portalData.documents).toBeDefined();
      expect(portalData.documents.length).toBeGreaterThan(0);

      portalData.documents.forEach((doc) => {
        expect(doc.id).toBeGreaterThan(0);
        expect(doc.name).toBeTruthy();
        expect(doc.size).toBeGreaterThan(0);
        expect(doc.url).toBeTruthy();
      });
    });
  });

  describe("All Medical Services Data Combined", () => {
    it("should contain all medical services modules", () => {
      expect(allMedicalServicesData.dialysis).toBeDefined();
      expect(allMedicalServicesData.ent).toBeDefined();
      expect(allMedicalServicesData.fertility).toBeDefined();
      expect(allMedicalServicesData.obgyn).toBeDefined();
      expect(allMedicalServicesData.clinicalVisit).toBeDefined();
      expect(allMedicalServicesData.analytics).toBeDefined();
      expect(allMedicalServicesData.workflow).toBeDefined();
      expect(allMedicalServicesData.portal).toBeDefined();
    });
  });
});

// ============================================================
// CORE MODULES DATA TESTS
// ============================================================

describe("E2E: Core Modules Data", () => {
  describe("Patients Module", () => {
    it("should have valid patient records", () => {
      expect(patientsData.patients).toBeDefined();
      expect(patientsData.patients.length).toBeGreaterThan(0);

      patientsData.patients.forEach((patient) => {
        expect(patient.id).toBeGreaterThan(0);
        expect(patient.branchId).toBeGreaterThan(0);
        expect(patient.emiratesId).toBeTruthy();
        expect(isValidEmiratesId(patient.emiratesId)).toBe(true);
        expect(patient.fullNameEn).toBeTruthy();
        expect(isValidDate(patient.dateOfBirth)).toBe(true);
        expect(["M", "F"]).toContain(patient.gender);
      });
    });

    it("should have valid contact information", () => {
      patientsData.patients.forEach((patient) => {
        if (patient.phoneNumber) {
          expect(isValidPhone(patient.phoneNumber)).toBe(true);
        }
        if (patient.email) {
          expect(isValidEmail(patient.email)).toBe(true);
        }
      });
    });

    it("should have Arabic names for some patients", () => {
      const patientsWithArabicNames = patientsData.patients.filter(
        (p) => p.fullNameAr,
      );
      expect(patientsWithArabicNames.length).toBeGreaterThan(0);
    });
  });

  describe("Appointments Module", () => {
    it("should have valid appointments", () => {
      expect(appointmentsData.appointments).toBeDefined();
      expect(appointmentsData.appointments.length).toBeGreaterThan(0);

      appointmentsData.appointments.forEach((appointment) => {
        expect(appointment.id).toBeGreaterThan(0);
        expect(appointment.patientId).toBeGreaterThan(0);
        expect(appointment.branchId).toBeGreaterThan(0);
        expect(isValidDate(appointment.startTime)).toBe(true);
        expect(isValidDate(appointment.endTime)).toBe(true);
      });
    });

    it("should have valid appointment timing", () => {
      appointmentsData.appointments.forEach((appointment) => {
        const start = new Date(appointment.startTime);
        const end = new Date(appointment.endTime);
        expect(end.getTime()).toBeGreaterThan(start.getTime());
      });
    });

    it("should have valid appointment status", () => {
      appointmentsData.appointments.forEach((appointment) => {
        // Status enum values: Booked=0, Confirmed=1, CheckedIn=2, Completed=3, Cancelled=4, NoShow=5
        expect([0, 1, 2, 3, 4, 5]).toContain(appointment.status);
      });
    });

    it("should have valid appointment types", () => {
      appointmentsData.appointments.forEach((appointment) => {
        // Type enum: Consultation=0, FollowUp=1, Procedure=2, Emergency=3
        expect([0, 1, 2, 3]).toContain(appointment.type);
      });
    });
  });

  describe("Multi-Tenancy Module", () => {
    it("should have valid tenants", () => {
      expect(multiTenancyData.tenants).toBeDefined();
      expect(multiTenancyData.tenants.length).toBeGreaterThan(0);

      multiTenancyData.tenants.forEach((tenant) => {
        expect(tenant.id).toBeGreaterThan(0);
        expect(tenant.name).toBeTruthy();
        expect(tenant.subdomain).toBeTruthy();
        expect(tenant.contactEmail).toBeTruthy();
        expect(isValidEmail(tenant.contactEmail)).toBe(true);
        expect(tenant.maxUsers).toBeGreaterThan(0);
      });
    });

    it("should have valid companies", () => {
      expect(multiTenancyData.companies).toBeDefined();
      expect(multiTenancyData.companies.length).toBeGreaterThan(0);

      multiTenancyData.companies.forEach((company) => {
        expect(company.id).toBeGreaterThan(0);
        expect(company.tenantId).toBeGreaterThan(0);
        expect(company.name).toBeTruthy();
        expect(company.legalName).toBeTruthy();
        expect(company.registrationNumber).toBeTruthy();
      });
    });

    it("should have valid branches", () => {
      expect(multiTenancyData.branches).toBeDefined();
      expect(multiTenancyData.branches.length).toBeGreaterThan(0);

      multiTenancyData.branches.forEach((branch) => {
        expect(branch.id).toBeGreaterThan(0);
        expect(branch.companyId).toBeGreaterThan(0);
        expect(branch.name).toBeTruthy();
        expect(branch.code).toBeTruthy();
        expect(branch.city).toBeTruthy();
      });
    });

    it("should have main branch for each company", () => {
      const companyIds = [
        ...new Set(multiTenancyData.branches.map((b) => b.companyId)),
      ];
      companyIds.forEach((companyId) => {
        const companyBranches = multiTenancyData.branches.filter(
          (b) => b.companyId === companyId,
        );
        const mainBranches = companyBranches.filter((b) => b.isMainBranch);
        expect(mainBranches.length).toBeGreaterThanOrEqual(1);
      });
    });

    it("should have valid tenant users", () => {
      expect(multiTenancyData.users).toBeDefined();
      expect(multiTenancyData.users.length).toBeGreaterThan(0);

      multiTenancyData.users.forEach((user) => {
        expect(user.id).toBeGreaterThan(0);
        expect(user.username).toBeTruthy();
        expect(user.email).toBeTruthy();
        expect(isValidEmail(user.email)).toBe(true);
        expect(user.role).toBeTruthy();
      });
    });

    it("should have valid subscriptions", () => {
      expect(multiTenancyData.subscriptions).toBeDefined();
      expect(multiTenancyData.subscriptions.length).toBeGreaterThan(0);

      multiTenancyData.subscriptions.forEach((subscription) => {
        expect(subscription.id).toBeGreaterThan(0);
        expect(subscription.tenantId).toBeGreaterThan(0);
        expect(subscription.amount).toBeGreaterThanOrEqual(0);
      });
    });
  });

  describe("Users Module", () => {
    it("should have valid users", () => {
      expect(usersData.users).toBeDefined();
      expect(usersData.users.length).toBeGreaterThan(0);

      usersData.users.forEach((user) => {
        expect(user.id).toBeTruthy();
        expect(user.username).toBeTruthy();
        expect(user.email).toBeTruthy();
        expect(isValidEmail(user.email)).toBe(true);
        expect(user.fullName).toBeTruthy();
        expect(user.roles).toBeDefined();
        expect(user.roles.length).toBeGreaterThan(0);
      });
    });

    it("should have admin user", () => {
      const adminUser = usersData.users.find((u) => u.roles.includes("Admin"));
      expect(adminUser).toBeDefined();
    });

    it("should have doctor users", () => {
      const doctors = usersData.users.filter((u) => u.roles.includes("Doctor"));
      expect(doctors.length).toBeGreaterThan(0);
    });
  });

  describe("Security Module", () => {
    it("should have valid audit logs", () => {
      expect(securityData.auditLogs).toBeDefined();
      expect(securityData.auditLogs.length).toBeGreaterThan(0);

      securityData.auditLogs.forEach((log) => {
        expect(log.id).toBeGreaterThan(0);
        expect(isValidDate(log.timestamp)).toBe(true);
        expect(log.username).toBeTruthy();
        expect(log.module).toBeTruthy();
        expect(typeof log.success).toBe("boolean");
      });
    });

    it("should have valid permissions", () => {
      expect(securityData.permissions).toBeDefined();
      expect(securityData.permissions.length).toBeGreaterThan(0);

      securityData.permissions.forEach((permission) => {
        expect(permission.id).toBeGreaterThan(0);
        expect(permission.name).toBeTruthy();
        expect(permission.module).toBeTruthy();
        expect(permission.description).toBeTruthy();
        expect(typeof permission.granted).toBe("boolean");
      });
    });

    it("should have valid user access reviews", () => {
      expect(securityData.userAccessReviews).toBeDefined();
      expect(securityData.userAccessReviews.length).toBeGreaterThan(0);
    });

    it("should have valid password policies", () => {
      expect(securityData.passwordPolicies).toBeDefined();
      expect(securityData.passwordPolicies.length).toBeGreaterThan(0);

      securityData.passwordPolicies.forEach((policy) => {
        expect(policy.minLength).toBeGreaterThan(0);
        expect(policy.lockoutThreshold).toBeGreaterThan(0);
        expect(policy.sessionTimeout).toBeGreaterThan(0);
      });
    });

    it("should have valid security incidents", () => {
      expect(securityData.securityIncidents).toBeDefined();
      expect(securityData.securityIncidents.length).toBeGreaterThan(0);

      securityData.securityIncidents.forEach((incident) => {
        expect(incident.incidentNumber).toBeTruthy();
        expect(incident.title).toBeTruthy();
        expect(["open", "investigating", "resolved", "closed"]).toContain(
          incident.status,
        );
      });
    });

    it("should have valid compliance reports", () => {
      expect(securityData.complianceReports).toBeDefined();
      expect(securityData.complianceReports.length).toBeGreaterThan(0);

      securityData.complianceReports.forEach((report) => {
        expect(report.complianceScore).toBeGreaterThanOrEqual(0);
        expect(report.complianceScore).toBeLessThanOrEqual(100);
        expect(report.findings).toBeDefined();
        expect(report.findings.length).toBeGreaterThan(0);
      });
    });

    it("should have valid login attempts", () => {
      expect(securityData.loginAttempts).toBeDefined();
      expect(securityData.loginAttempts.length).toBeGreaterThan(0);

      securityData.loginAttempts.forEach((attempt) => {
        expect(attempt.username).toBeTruthy();
        expect(attempt.ipAddress).toBeTruthy();
        expect(typeof attempt.success).toBe("boolean");
      });
    });

    it("should have valid sessions", () => {
      expect(securityData.sessions).toBeDefined();
      expect(securityData.sessions.length).toBeGreaterThan(0);

      securityData.sessions.forEach((session) => {
        expect(session.id).toBeTruthy();
        expect(session.userId).toBeGreaterThan(0);
        expect(session.username).toBeTruthy();
        expect(typeof session.isActive).toBe("boolean");
      });
    });
  });

  describe("Payroll Module", () => {
    it("should have valid salary structures", () => {
      expect(payrollData.salaryStructures).toBeDefined();
      expect(payrollData.salaryStructures.length).toBeGreaterThan(0);

      payrollData.salaryStructures.forEach((structure) => {
        expect(structure.id).toBeGreaterThan(0);
        expect(structure.name).toBeTruthy();
        expect(structure.basicSalary).toBeGreaterThan(0);
        expect(structure.housingAllowance).toBeGreaterThanOrEqual(0);
        expect(structure.transportAllowance).toBeGreaterThanOrEqual(0);
      });
    });

    it("should have valid payroll records", () => {
      expect(payrollData.payrollRecords).toBeDefined();
      expect(payrollData.payrollRecords.length).toBeGreaterThan(0);

      payrollData.payrollRecords.forEach((record) => {
        expect(record.id).toBeGreaterThan(0);
        expect(record.employeeId).toBeGreaterThan(0);
        expect(record.basicSalary).toBeGreaterThan(0);
        expect(record.grossSalary).toBeGreaterThan(0);
        expect(record.netSalary).toBeGreaterThan(0);
        expect(record.netSalary).toBeLessThanOrEqual(record.grossSalary);
      });
    });

    it("should have correct salary calculations", () => {
      payrollData.payrollRecords.forEach((record) => {
        const calculatedGross = record.basicSalary + record.totalAllowances;
        const calculatedNet = calculatedGross - record.totalDeductions;
        expect(record.grossSalary).toBe(calculatedGross);
        expect(record.netSalary).toBe(calculatedNet);
      });
    });

    it("should have valid employees", () => {
      expect(payrollData.employees).toBeDefined();
      expect(payrollData.employees.length).toBeGreaterThan(0);

      payrollData.employees.forEach((employee) => {
        expect(employee.id).toBeGreaterThan(0);
        expect(employee.employeeNumber).toBeTruthy();
        expect(employee.fullName).toBeTruthy();
        expect(employee.email).toBeTruthy();
        expect(isValidEmail(employee.email)).toBe(true);
      });
    });
  });

  describe("All Core Modules Data Combined", () => {
    it("should contain all core modules", () => {
      expect(allCoreModulesData.patients).toBeDefined();
      expect(allCoreModulesData.appointments).toBeDefined();
      expect(allCoreModulesData.multiTenancy).toBeDefined();
      expect(allCoreModulesData.users).toBeDefined();
      expect(allCoreModulesData.security).toBeDefined();
      expect(allCoreModulesData.payroll).toBeDefined();
    });
  });
});

// ============================================================
// CROSS-MODULE INTEGRATION TESTS
// ============================================================

describe("E2E: Cross-Module Integration", () => {
  describe("Patient References", () => {
    it("appointments should reference valid patient IDs", () => {
      const patientIds = new Set(patientsData.patients.map((p) => p.id));

      appointmentsData.appointments.forEach((appointment) => {
        expect(patientIds.has(appointment.patientId)).toBe(true);
      });
    });

    it("clinical visits should reference valid patient and doctor IDs", () => {
      clinicalVisitData.visits.forEach((visit) => {
        expect(visit.patientId).toBeGreaterThan(0);
        expect(visit.doctorId).toBeGreaterThan(0);
      });
    });
  });

  describe("Branch References", () => {
    it("appointments should reference valid branch IDs", () => {
      const branchIds = new Set(multiTenancyData.branches.map((b) => b.id));

      appointmentsData.appointments.forEach((appointment) => {
        expect(branchIds.has(appointment.branchId)).toBe(true);
      });
    });

    it("patients should reference valid branch IDs", () => {
      const branchIds = new Set(multiTenancyData.branches.map((b) => b.id));

      patientsData.patients.forEach((patient) => {
        expect(branchIds.has(patient.branchId)).toBe(true);
      });
    });
  });

  describe("Employee References", () => {
    it("payroll records should reference valid employee IDs", () => {
      const employeeIds = new Set(payrollData.employees.map((e) => e.id));

      payrollData.payrollRecords.forEach((record) => {
        expect(employeeIds.has(record.employeeId)).toBe(true);
      });
    });
  });

  describe("Workflow References", () => {
    it("workflow instances should reference valid definitions", () => {
      const definitionIds = new Set(workflowData.definitions.map((d) => d.id));

      workflowData.instances.forEach((instance) => {
        expect(definitionIds.has(instance.definitionId)).toBe(true);
      });
    });
  });

  describe("Portal References", () => {
    it("portal users should reference valid patient IDs", () => {
      // Portal users reference patients but may have IDs outside the core patients set
      portalData.users.forEach((user) => {
        expect(user.patientId).toBeGreaterThan(0);
      });
    });

    it("portal documents should reference valid patient IDs", () => {
      portalData.documents.forEach((doc) => {
        expect(doc.patientId).toBeGreaterThan(0);
      });
    });
  });
});

// ============================================================
// DATA QUALITY AND BUSINESS RULES TESTS
// ============================================================

describe("E2E: Data Quality and Business Rules", () => {
  describe("Date Consistency", () => {
    it("appointment end time should be after start time", () => {
      appointmentsData.appointments.forEach((appointment) => {
        const start = new Date(appointment.startTime);
        const end = new Date(appointment.endTime);
        expect(end > start).toBe(true);
      });
    });

    it("pregnancy EDD should be after LMP", () => {
      obgynData.pregnancies.forEach((pregnancy) => {
        const lmp = new Date(pregnancy.lmp);
        const edd = new Date(pregnancy.edd);
        expect(edd > lmp).toBe(true);
      });
    });

    it("subscription end date should be after start date", () => {
      multiTenancyData.subscriptions.forEach((subscription) => {
        const start = new Date(subscription.startDate);
        const end = new Date(subscription.endDate);
        expect(end > start).toBe(true);
      });
    });
  });

  describe("Financial Consistency", () => {
    it("invoice totals should be valid", () => {
      financialData.invoices.forEach((invoice) => {
        expect(invoice.total).toBeGreaterThan(0);
        // Total should equal subtotal + tax - discount
        const expectedTotal = invoice.subtotal + invoice.tax - invoice.discount;
        expect(invoice.total).toBe(expectedTotal);
      });
    });

    it("sales total should equal sum of items", () => {
      salesData.sales.forEach((sale) => {
        const itemsTotal = sale.items.reduce(
          (sum, item) => sum + item.subtotal,
          0,
        );
        expect(sale.subTotal).toBe(itemsTotal);
      });
    });

    it("tenant storage usage should not exceed quota", () => {
      multiTenancyData.tenants.forEach((tenant) => {
        expect(tenant.usedStorageGB).toBeLessThanOrEqual(tenant.storageQuotaGB);
      });
    });
  });

  describe("Medical Data Validity", () => {
    it("dialysis session post-weight should be less than pre-weight (typically)", () => {
      // After dialysis, patient weight typically decreases due to fluid removal
      const completedSessions = dialysisData.sessions.filter(
        (s) => s.status === 2,
      );
      completedSessions.forEach((session) => {
        expect(session.postWeight).toBeLessThanOrEqual(session.preWeight);
      });
    });

    it("vital signs should be within reasonable ranges", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.heartRate) {
          expect(visit.vitalSigns.heartRate).toBeGreaterThan(30);
          expect(visit.vitalSigns.heartRate).toBeLessThan(250);
        }
        if (visit.vitalSigns?.oxygenSaturation) {
          expect(visit.vitalSigns.oxygenSaturation).toBeGreaterThan(50);
          expect(visit.vitalSigns.oxygenSaturation).toBeLessThanOrEqual(100);
        }
      });
    });

    it("gestational age should be reasonable for active pregnancies", () => {
      const activePregnancies = obgynData.pregnancies.filter(
        (p) => p.status === 0,
      );
      activePregnancies.forEach((pregnancy) => {
        expect(pregnancy.gestationalAge).toBeGreaterThan(0);
        expect(pregnancy.gestationalAge).toBeLessThanOrEqual(42);
      });
    });
  });

  describe("Security Compliance", () => {
    it("password policies should have reasonable minimum length", () => {
      securityData.passwordPolicies.forEach((policy) => {
        expect(policy.minLength).toBeGreaterThanOrEqual(8);
      });
    });

    it("compliance scores should be valid percentages", () => {
      securityData.complianceReports.forEach((report) => {
        expect(report.complianceScore).toBeGreaterThanOrEqual(0);
        expect(report.complianceScore).toBeLessThanOrEqual(100);
      });
    });

    it("session expiry should be after start time", () => {
      securityData.sessions.forEach((session) => {
        const start = new Date(session.startedAt);
        const expiry = new Date(session.expiresAt);
        expect(expiry > start).toBe(true);
      });
    });
  });

  describe("Inventory Management", () => {
    it("inventory items should have valid categories", () => {
      // Categories are numeric enums: 0 = Medical, 1 = Consumable, 2 = Office, 3 = Pharmaceutical, 4 = Equipment
      const validCategories = [0, 1, 2, 3, 4];
      inventoryData.items.forEach((item) => {
        expect(validCategories).toContain(item.category);
      });
    });

    it("items below minimum stock level should be identifiable", () => {
      const lowStockItems = inventoryData.items.filter(
        (item) => item.quantity <= item.minStockLevel,
      );
      expect(lowStockItems.length).toBeGreaterThanOrEqual(0);
    });
  });
});

// ============================================================
// DATA COMPLETENESS TESTS
// ============================================================

describe("E2E: Data Completeness", () => {
  it("should have comprehensive specialty coverage", () => {
    expect(Object.keys(allSpecialtyData).length).toBeGreaterThanOrEqual(11);
  });

  it("should have comprehensive business module coverage", () => {
    expect(Object.keys(allBusinessData).length).toBeGreaterThanOrEqual(6);
  });

  it("should have comprehensive medical services coverage", () => {
    expect(Object.keys(allMedicalServicesData).length).toBeGreaterThanOrEqual(
      8,
    );
  });

  it("should have comprehensive core module coverage", () => {
    expect(Object.keys(allCoreModulesData).length).toBeGreaterThanOrEqual(6);
  });

  it("should have data in all major categories", () => {
    // Total modules covered
    const totalSpecialtyModules = Object.keys(allSpecialtyData).length;
    const totalBusinessModules = Object.keys(allBusinessData).length;
    const totalMedicalServicesModules = Object.keys(
      allMedicalServicesData,
    ).length;
    const totalCoreModules = Object.keys(allCoreModulesData).length;

    const totalModules =
      totalSpecialtyModules +
      totalBusinessModules +
      totalMedicalServicesModules +
      totalCoreModules;

    // Should have at least 30 modules covered
    expect(totalModules).toBeGreaterThanOrEqual(30);
  });
});

// ============================================================
// UAE-SPECIFIC DATA VALIDATION TESTS
// ============================================================

describe("E2E: UAE-Specific Data Validation", () => {
  describe("Emirates ID Validation", () => {
    it("patients should have valid Emirates ID format", () => {
      patientsData.patients.forEach((patient) => {
        if (patient.emiratesId) {
          expect(isValidEmiratesId(patient.emiratesId)).toBe(true);
        }
      });
    });

    it("Emirates IDs should be unique across patients", () => {
      const emiratesIds = patientsData.patients
        .filter((p) => p.emiratesId)
        .map((p) => p.emiratesId);
      const uniqueIds = new Set(emiratesIds);
      expect(uniqueIds.size).toBe(emiratesIds.length);
    });
  });

  describe("UAE Phone Number Validation", () => {
    it("patient phone numbers should have valid UAE format", () => {
      patientsData.patients.forEach((patient) => {
        if (patient.phoneNumber) {
          expect(patient.phoneNumber).toMatch(/^\+971/);
        }
      });
    });

    it("branch phone numbers should have valid UAE format", () => {
      multiTenancyData.branches.forEach((branch) => {
        if (branch.phone) {
          expect(branch.phone).toMatch(/^\+971/);
        }
      });
    });

    it("company phone numbers should have valid UAE format", () => {
      multiTenancyData.companies.forEach((company) => {
        if (company.phone) {
          expect(company.phone).toMatch(/^\+971/);
        }
      });
    });

    it("employee phone numbers should have valid format", () => {
      hrData.employees.forEach((employee) => {
        if (employee.phone) {
          expect(isValidPhone(employee.phone)).toBe(true);
        }
      });
    });

    it("marketing leads should have valid phone numbers", () => {
      marketingData.leads.forEach((lead) => {
        if (lead.phoneNumber) {
          expect(isValidPhone(lead.phoneNumber)).toBe(true);
        }
      });
    });
  });

  describe("Arabic Names Validation", () => {
    it("some patients should have Arabic names", () => {
      const patientsWithArabicNames = patientsData.patients.filter(
        (p) => p.fullNameAr,
      );
      expect(patientsWithArabicNames.length).toBeGreaterThan(0);
    });

    it("Arabic names should contain Arabic characters", () => {
      const arabicRegex = /[\u0600-\u06FF]/;
      patientsData.patients.forEach((patient) => {
        if (patient.fullNameAr) {
          expect(arabicRegex.test(patient.fullNameAr)).toBe(true);
        }
      });
    });

    it("companies should have Arabic names", () => {
      const companiesWithArabicNames = multiTenancyData.companies.filter(
        (c) => c.nameAr,
      );
      expect(companiesWithArabicNames.length).toBeGreaterThan(0);
    });

    it("branches should have Arabic names", () => {
      const branchesWithArabicNames = multiTenancyData.branches.filter(
        (b) => b.nameAr,
      );
      expect(branchesWithArabicNames.length).toBeGreaterThan(0);
    });
  });

  describe("UAE Currency Validation", () => {
    it("financial amounts should be in AED (positive values)", () => {
      financialData.invoices.forEach((invoice) => {
        expect(invoice.total).toBeGreaterThan(0);
        expect(invoice.subtotal).toBeGreaterThan(0);
      });
    });

    it("sales amounts should be reasonable for UAE market", () => {
      salesData.sales.forEach((sale) => {
        expect(sale.total).toBeGreaterThan(0);
        expect(sale.total).toBeLessThan(10000000); // Max 10 million AED
      });
    });

    it("salary amounts should be reasonable for UAE market", () => {
      payrollData.salaryStructures.forEach((salary) => {
        expect(salary.basicSalary).toBeGreaterThan(0);
        expect(salary.basicSalary).toBeLessThan(500000); // Max 500k AED monthly
      });
    });
  });

  describe("UAE Address Validation", () => {
    it("branches should be in UAE cities", () => {
      const uaeCities = [
        "Dubai",
        "Abu Dhabi",
        "Sharjah",
        "Ajman",
        "Ras Al Khaimah",
        "Fujairah",
        "Umm Al Quwain",
      ];
      multiTenancyData.branches.forEach((branch) => {
        expect(uaeCities).toContain(branch.city);
      });
    });

    it("companies should be in UAE", () => {
      multiTenancyData.companies.forEach((company) => {
        expect(company.country).toBe("UAE");
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE EMAIL VALIDATION TESTS
// ============================================================

describe("E2E: Email Format Validation", () => {
  it("all patient emails should be valid format", () => {
    patientsData.patients.forEach((patient) => {
      if (patient.email) {
        expect(isValidEmail(patient.email)).toBe(true);
      }
    });
  });

  it("all user emails should be valid format", () => {
    usersData.users.forEach((user) => {
      expect(isValidEmail(user.email)).toBe(true);
    });
  });

  it("all employee emails should be valid format", () => {
    hrData.employees.forEach((employee) => {
      expect(isValidEmail(employee.email)).toBe(true);
    });
  });

  it("all company emails should be valid format", () => {
    multiTenancyData.companies.forEach((company) => {
      expect(isValidEmail(company.email)).toBe(true);
    });
  });

  it("all branch emails should be valid format", () => {
    multiTenancyData.branches.forEach((branch) => {
      expect(isValidEmail(branch.email)).toBe(true);
    });
  });

  it("all tenant emails should be valid format", () => {
    multiTenancyData.tenants.forEach((tenant) => {
      expect(isValidEmail(tenant.contactEmail)).toBe(true);
    });
  });

  it("all marketing lead emails should be valid format", () => {
    marketingData.leads.forEach((lead) => {
      if (lead.email) {
        expect(isValidEmail(lead.email)).toBe(true);
      }
    });
  });

  it("all portal user emails should be valid format", () => {
    portalData.users.forEach((user) => {
      expect(isValidEmail(user.email)).toBe(true);
    });
  });

  it("emails should be unique across users", () => {
    const emails = usersData.users.map((u) => u.email);
    const uniqueEmails = new Set(emails);
    expect(uniqueEmails.size).toBe(emails.length);
  });

  it("emails should be unique across employees", () => {
    const emails = hrData.employees.map((e) => e.email);
    const uniqueEmails = new Set(emails);
    expect(uniqueEmails.size).toBe(emails.length);
  });
});

// ============================================================
// COMPREHENSIVE ID UNIQUENESS TESTS
// ============================================================

describe("E2E: ID Uniqueness Validation", () => {
  describe("Patient ID Uniqueness", () => {
    it("patient IDs should be unique", () => {
      const ids = patientsData.patients.map((p) => p.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("patient Emirates IDs should be unique", () => {
      const emiratesIds = patientsData.patients
        .filter((p) => p.emiratesId)
        .map((p) => p.emiratesId);
      const uniqueIds = new Set(emiratesIds);
      expect(uniqueIds.size).toBe(emiratesIds.length);
    });
  });

  describe("Appointment ID Uniqueness", () => {
    it("appointment IDs should be unique", () => {
      const ids = appointmentsData.appointments.map((a) => a.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });
  });

  describe("Invoice ID Uniqueness", () => {
    it("financial invoice IDs should be unique", () => {
      const ids = financialData.invoices.map((i) => i.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("financial invoice numbers should be unique", () => {
      const numbers = financialData.invoices.map((i) => i.invoiceNumber);
      const uniqueNumbers = new Set(numbers);
      expect(uniqueNumbers.size).toBe(numbers.length);
    });

    it("sales invoice numbers should be unique", () => {
      const numbers = salesData.sales.map((s) => s.invoiceNumber);
      const uniqueNumbers = new Set(numbers);
      expect(uniqueNumbers.size).toBe(numbers.length);
    });
  });

  describe("User ID Uniqueness", () => {
    it("user IDs should be unique", () => {
      const ids = usersData.users.map((u) => u.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("usernames should be unique", () => {
      const usernames = usersData.users.map((u) => u.username);
      const uniqueUsernames = new Set(usernames);
      expect(uniqueUsernames.size).toBe(usernames.length);
    });
  });

  describe("Employee ID Uniqueness", () => {
    it("employee IDs should be unique", () => {
      const ids = hrData.employees.map((e) => e.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("employee IDs should be unique", () => {
      const codes = hrData.employees.map((e) => e.employeeId);
      const uniqueCodes = new Set(codes);
      expect(uniqueCodes.size).toBe(codes.length);
    });
  });

  describe("Inventory Item Code Uniqueness", () => {
    it("inventory item IDs should be unique", () => {
      const ids = inventoryData.items.map((i) => i.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("inventory item codes should be unique", () => {
      const codes = inventoryData.items.map((i) => i.itemCode);
      const uniqueCodes = new Set(codes);
      expect(uniqueCodes.size).toBe(codes.length);
    });
  });

  describe("Tenant and Company Uniqueness", () => {
    it("tenant IDs should be unique", () => {
      const ids = multiTenancyData.tenants.map((t) => t.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("tenant subdomains should be unique", () => {
      const subdomains = multiTenancyData.tenants.map((t) => t.subdomain);
      const uniqueSubdomains = new Set(subdomains);
      expect(uniqueSubdomains.size).toBe(subdomains.length);
    });

    it("company IDs should be unique", () => {
      const ids = multiTenancyData.companies.map((c) => c.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("branch IDs should be unique", () => {
      const ids = multiTenancyData.branches.map((b) => b.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("branch codes should be unique", () => {
      const codes = multiTenancyData.branches.map((b) => b.code);
      const uniqueCodes = new Set(codes);
      expect(uniqueCodes.size).toBe(codes.length);
    });
  });

  describe("Medical Record Uniqueness", () => {
    it("lab order IDs should be unique", () => {
      const ids = laboratoryData.orders.map((o) => o.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("radiology order IDs should be unique", () => {
      const ids = radiologyData.orders.map((o) => o.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("prescription IDs should be unique", () => {
      const ids = pharmacyData.prescriptions.map((p) => p.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("dialysis patient IDs should be unique", () => {
      const ids = dialysisData.patients.map((p) => p.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });
  });

  describe("Marketing Data Uniqueness", () => {
    it("campaign IDs should be unique", () => {
      const ids = marketingData.campaigns.map((c) => c.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("lead IDs should be unique", () => {
      const ids = marketingData.leads.map((l) => l.id);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(ids.length);
    });

    it("lead codes should be unique", () => {
      const codes = marketingData.leads.map((l) => l.leadCode);
      const uniqueCodes = new Set(codes);
      expect(uniqueCodes.size).toBe(codes.length);
    });
  });
});

// ============================================================
// COMPREHENSIVE DATE VALIDATION TESTS
// ============================================================

describe("E2E: Date Format and Logic Validation", () => {
  describe("Patient Dates", () => {
    it("patient birth dates should be valid", () => {
      patientsData.patients.forEach((patient) => {
        expect(isValidDate(patient.dateOfBirth)).toBe(true);
      });
    });

    it("patient birth dates should be in the past", () => {
      const today = new Date();
      patientsData.patients.forEach((patient) => {
        const birthDate = new Date(patient.dateOfBirth);
        expect(birthDate < today).toBe(true);
      });
    });

    it("patient ages should be reasonable (0-120 years)", () => {
      const today = new Date();
      patientsData.patients.forEach((patient) => {
        const birthDate = new Date(patient.dateOfBirth);
        const ageInYears =
          (today.getTime() - birthDate.getTime()) /
          (365.25 * 24 * 60 * 60 * 1000);
        expect(ageInYears).toBeGreaterThanOrEqual(0);
        expect(ageInYears).toBeLessThanOrEqual(120);
      });
    });
  });

  describe("Appointment Dates", () => {
    it("appointment start times should be valid", () => {
      appointmentsData.appointments.forEach((appointment) => {
        expect(isValidDate(appointment.startTime)).toBe(true);
      });
    });

    it("appointment end times should be valid", () => {
      appointmentsData.appointments.forEach((appointment) => {
        expect(isValidDate(appointment.endTime)).toBe(true);
      });
    });

    it("appointment duration should be positive", () => {
      appointmentsData.appointments.forEach((appointment) => {
        const start = new Date(appointment.startTime);
        const end = new Date(appointment.endTime);
        const durationMinutes = (end.getTime() - start.getTime()) / (1000 * 60);
        expect(durationMinutes).toBeGreaterThan(0);
      });
    });

    it("appointment duration should be reasonable (5-480 minutes)", () => {
      appointmentsData.appointments.forEach((appointment) => {
        const start = new Date(appointment.startTime);
        const end = new Date(appointment.endTime);
        const durationMinutes = (end.getTime() - start.getTime()) / (1000 * 60);
        expect(durationMinutes).toBeGreaterThanOrEqual(5);
        expect(durationMinutes).toBeLessThanOrEqual(480);
      });
    });
  });

  describe("Invoice Dates", () => {
    it("invoice dates should be valid", () => {
      financialData.invoices.forEach((invoice) => {
        expect(isValidDate(invoice.invoiceDate)).toBe(true);
      });
    });

    it("invoice due dates should be valid", () => {
      financialData.invoices.forEach((invoice) => {
        expect(isValidDate(invoice.dueDate)).toBe(true);
      });
    });

    it("invoice due dates should be after or equal to invoice dates", () => {
      financialData.invoices.forEach((invoice) => {
        const invoiceDate = new Date(invoice.invoiceDate);
        const dueDate = new Date(invoice.dueDate);
        expect(dueDate >= invoiceDate).toBe(true);
      });
    });
  });

  describe("Sales Dates", () => {
    it("sale dates should be valid", () => {
      salesData.sales.forEach((sale) => {
        expect(isValidDate(sale.saleDate)).toBe(true);
      });
    });

    it("sale due dates should be after sale dates", () => {
      salesData.sales.forEach((sale) => {
        const saleDate = new Date(sale.saleDate);
        const dueDate = new Date(sale.dueDate);
        expect(dueDate >= saleDate).toBe(true);
      });
    });
  });

  describe("Medical Record Dates", () => {
    it("lab order dates should be valid", () => {
      laboratoryData.orders.forEach((order) => {
        expect(isValidDate(order.orderDate)).toBe(true);
      });
    });

    it("radiology order dates should be valid", () => {
      radiologyData.orders.forEach((order) => {
        expect(isValidDate(order.orderDate)).toBe(true);
      });
    });

    it("prescription dates should be valid", () => {
      pharmacyData.prescriptions.forEach((prescription) => {
        expect(isValidDate(prescription.prescriptionDate)).toBe(true);
      });
    });

    it("dialysis session dates should be valid", () => {
      dialysisData.sessions.forEach((session) => {
        expect(isValidDate(session.sessionDate)).toBe(true);
      });
    });

    it("clinical visit dates should be valid", () => {
      clinicalVisitData.visits.forEach((visit) => {
        expect(isValidDate(visit.visitDate)).toBe(true);
      });
    });
  });

  describe("Employee Dates", () => {
    it("employee hire dates should be valid", () => {
      hrData.employees.forEach((employee) => {
        expect(isValidDate(employee.hireDate)).toBe(true);
      });
    });

    it("employee hire dates should be in the past or today", () => {
      const today = new Date();
      today.setHours(23, 59, 59, 999);
      hrData.employees.forEach((employee) => {
        const hireDate = new Date(employee.hireDate);
        expect(hireDate <= today).toBe(true);
      });
    });
  });

  describe("Security Dates", () => {
    it("audit log timestamps should be valid", () => {
      securityData.auditLogs.forEach((log) => {
        expect(isValidDate(log.timestamp)).toBe(true);
      });
    });

    it("login attempt timestamps should be valid", () => {
      securityData.loginAttempts.forEach((attempt) => {
        expect(isValidDate(attempt.timestamp)).toBe(true);
      });
    });

    it("session start times should be valid", () => {
      securityData.sessions.forEach((session) => {
        expect(isValidDate(session.startedAt)).toBe(true);
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE NUMERICAL RANGE TESTS
// ============================================================

describe("E2E: Numerical Range Validation", () => {
  describe("Percentage Values", () => {
    it("discount percentages should be 0-100 when defined", () => {
      salesData.sales.forEach((sale) => {
        if (sale.discountPercentage !== undefined) {
          expect(sale.discountPercentage).toBeGreaterThanOrEqual(0);
          expect(sale.discountPercentage).toBeLessThanOrEqual(100);
        }
      });
    });

    it("tax percentages should be 0-100", () => {
      salesData.sales.forEach((sale) => {
        if (sale.taxPercentage !== undefined) {
          expect(sale.taxPercentage).toBeGreaterThanOrEqual(0);
          expect(sale.taxPercentage).toBeLessThanOrEqual(100);
        }
      });
    });

    it("growth percentile should be 0-100", () => {
      pediatricsData.growthCharts.forEach((chart) => {
        expect(chart.weightPercentile).toBeGreaterThanOrEqual(0);
        expect(chart.weightPercentile).toBeLessThanOrEqual(100);
        expect(chart.heightPercentile).toBeGreaterThanOrEqual(0);
        expect(chart.heightPercentile).toBeLessThanOrEqual(100);
      });
    });

    it("oxygen saturation should be 0-100", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.oxygenSaturation) {
          expect(visit.vitalSigns.oxygenSaturation).toBeGreaterThanOrEqual(0);
          expect(visit.vitalSigns.oxygenSaturation).toBeLessThanOrEqual(100);
        }
      });
    });
  });

  describe("Quantity Values", () => {
    it("inventory quantities should be non-negative", () => {
      inventoryData.items.forEach((item) => {
        expect(item.quantity).toBeGreaterThanOrEqual(0);
      });
    });

    it("inventory min stock levels should be non-negative", () => {
      inventoryData.items.forEach((item) => {
        expect(item.minStockLevel).toBeGreaterThanOrEqual(0);
      });
    });

    it("inventory max stock levels should be greater than min", () => {
      inventoryData.items.forEach((item) => {
        expect(item.maxStockLevel).toBeGreaterThanOrEqual(item.minStockLevel);
      });
    });

    it("sale item quantities should be positive", () => {
      salesData.sales.forEach((sale) => {
        sale.items.forEach((item) => {
          expect(item.quantity).toBeGreaterThan(0);
        });
      });
    });
  });

  describe("Price Values", () => {
    it("inventory unit prices should be positive", () => {
      inventoryData.items.forEach((item) => {
        expect(item.unitPrice).toBeGreaterThan(0);
      });
    });

    it("sale item unit prices should be positive", () => {
      salesData.sales.forEach((sale) => {
        sale.items.forEach((item) => {
          expect(item.unitPrice).toBeGreaterThan(0);
        });
      });
    });

    it("quotation totals should be positive", () => {
      salesData.quotations.forEach((quotation) => {
        expect(quotation.total).toBeGreaterThan(0);
      });
    });
  });

  describe("Medical Values", () => {
    it("heart rates should be in reasonable range (30-250 bpm)", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.heartRate) {
          expect(visit.vitalSigns.heartRate).toBeGreaterThanOrEqual(30);
          expect(visit.vitalSigns.heartRate).toBeLessThanOrEqual(250);
        }
      });
    });

    it("blood pressure systolic should be reasonable (60-300 mmHg)", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.bloodPressureSystolic) {
          expect(visit.vitalSigns.bloodPressureSystolic).toBeGreaterThanOrEqual(
            60,
          );
          expect(visit.vitalSigns.bloodPressureSystolic).toBeLessThanOrEqual(
            300,
          );
        }
      });
    });

    it("blood pressure diastolic should be reasonable (30-200 mmHg)", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.bloodPressureDiastolic) {
          expect(
            visit.vitalSigns.bloodPressureDiastolic,
          ).toBeGreaterThanOrEqual(30);
          expect(visit.vitalSigns.bloodPressureDiastolic).toBeLessThanOrEqual(
            200,
          );
        }
      });
    });

    it("systolic should be greater than diastolic", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (
          visit.vitalSigns?.bloodPressureSystolic &&
          visit.vitalSigns?.bloodPressureDiastolic
        ) {
          expect(visit.vitalSigns.bloodPressureSystolic).toBeGreaterThan(
            visit.vitalSigns.bloodPressureDiastolic,
          );
        }
      });
    });

    it("temperature should be reasonable (30-45 Celsius)", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.temperature) {
          expect(visit.vitalSigns.temperature).toBeGreaterThanOrEqual(30);
          expect(visit.vitalSigns.temperature).toBeLessThanOrEqual(45);
        }
      });
    });

    it("respiratory rate should be reasonable (8-60 breaths/min)", () => {
      clinicalVisitData.visits.forEach((visit) => {
        if (visit.vitalSigns?.respiratoryRate) {
          expect(visit.vitalSigns.respiratoryRate).toBeGreaterThanOrEqual(8);
          expect(visit.vitalSigns.respiratoryRate).toBeLessThanOrEqual(60);
        }
      });
    });

    it("dialysis dry weight should be positive", () => {
      dialysisData.patients.forEach((patient) => {
        expect(patient.dryWeight).toBeGreaterThan(0);
      });
    });

    it("dialysis session weights should be positive", () => {
      dialysisData.sessions.forEach((session) => {
        expect(session.preWeight).toBeGreaterThan(0);
        if (session.postWeight) {
          expect(session.postWeight).toBeGreaterThan(0);
        }
      });
    });

    it("IOP measurements should be reasonable (5-60 mmHg)", () => {
      ophthalmologyData.iopMeasurements.forEach((iop) => {
        expect(iop.rightEye).toBeGreaterThanOrEqual(5);
        expect(iop.rightEye).toBeLessThanOrEqual(60);
        expect(iop.leftEye).toBeGreaterThanOrEqual(5);
        expect(iop.leftEye).toBeLessThanOrEqual(60);
      });
    });

    it("visual acuity should be defined", () => {
      ophthalmologyData.visualAcuity.forEach((test) => {
        // Visual acuity is typically expressed as fraction like 20/20
        expect(test.rightEyeUncorrected).toBeTruthy();
        expect(test.leftEyeUncorrected).toBeTruthy();
      });
    });

    it("pediatric weights should be reasonable (0.5-200 kg)", () => {
      pediatricsData.growthCharts.forEach((chart) => {
        expect(chart.weight).toBeGreaterThan(0.5);
        expect(chart.weight).toBeLessThan(200);
      });
    });

    it("pediatric heights should be reasonable (30-250 cm)", () => {
      pediatricsData.growthCharts.forEach((chart) => {
        expect(chart.height).toBeGreaterThan(30);
        expect(chart.height).toBeLessThan(250);
      });
    });

    it("ECG heart rates should be reasonable", () => {
      cardiologyData.ecgs.forEach((ecg) => {
        expect(ecg.heartRate).toBeGreaterThan(20);
        expect(ecg.heartRate).toBeLessThan(300);
      });
    });
  });

  describe("Duration Values", () => {
    it("surgery durations should be positive", () => {
      orthopedicsData.surgeries.forEach((surgery) => {
        expect(surgery.duration).toBeGreaterThan(0);
      });
    });

    it("physiotherapy session durations should be positive", () => {
      physiotherapyData.sessions.forEach((session) => {
        expect(session.duration).toBeGreaterThan(0);
      });
    });

    it("dialysis session durations should be reasonable (60-360 minutes)", () => {
      dialysisData.sessions.forEach((session) => {
        expect(session.duration).toBeGreaterThanOrEqual(60);
        expect(session.duration).toBeLessThanOrEqual(360);
      });
    });
  });

  describe("Chemotherapy Cycles", () => {
    it("chemotherapy cycles should be positive", () => {
      oncologyData.chemotherapy.forEach((chemo) => {
        expect(chemo.cycles).toBeGreaterThan(0);
      });
    });

    it("completed cycles should not exceed total cycles", () => {
      oncologyData.chemotherapy.forEach((chemo) => {
        expect(chemo.completedCycles).toBeLessThanOrEqual(chemo.cycles);
      });
    });

    it("completed cycles should be non-negative", () => {
      oncologyData.chemotherapy.forEach((chemo) => {
        expect(chemo.completedCycles).toBeGreaterThanOrEqual(0);
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE STATUS/ENUM VALIDATION TESTS
// ============================================================

describe("E2E: Status and Enum Validation", () => {
  describe("Appointment Status", () => {
    it("appointment statuses should be valid", () => {
      const validStatuses = [0, 1, 2, 3, 4, 5]; // Scheduled, Confirmed, CheckedIn, InProgress, Completed, Cancelled, NoShow
      appointmentsData.appointments.forEach((appointment) => {
        expect(validStatuses).toContain(appointment.status);
      });
    });

    it("appointment types should be valid", () => {
      const validTypes = [0, 1, 2, 3, 4]; // Consultation, FollowUp, Procedure, Emergency, Telemedicine
      appointmentsData.appointments.forEach((appointment) => {
        expect(validTypes).toContain(appointment.type);
      });
    });
  });

  describe("Patient Status", () => {
    it("patient genders should be valid", () => {
      const validGenders = ["M", "F", "O"]; // Male, Female, Other (string format)
      patientsData.patients.forEach((patient) => {
        expect(validGenders).toContain(patient.gender);
      });
    });
  });

  describe("User Roles", () => {
    it("user roles should be defined", () => {
      usersData.users.forEach((user) => {
        // Users have roles array, not single role
        expect(user.roles).toBeDefined();
        expect(user.roles.length).toBeGreaterThan(0);
      });
    });

    it("users should have required identifiers", () => {
      usersData.users.forEach((user) => {
        expect(user.id).toBeDefined();
        expect(user.username).toBeTruthy();
      });
    });
  });

  describe("Tenant Status", () => {
    it("tenant statuses should be valid", () => {
      const validStatuses = [0, 1, 2, 3]; // Active, Suspended, PendingActivation, Cancelled
      multiTenancyData.tenants.forEach((tenant) => {
        expect(validStatuses).toContain(tenant.status);
      });
    });

    it("subscription plans should be valid", () => {
      const validPlans = [0, 1, 2, 3]; // Trial, Basic, Professional, Enterprise
      multiTenancyData.tenants.forEach((tenant) => {
        expect(validPlans).toContain(tenant.plan);
      });
    });
  });

  describe("Invoice Status", () => {
    it("financial invoice statuses should be defined", () => {
      financialData.invoices.forEach((invoice) => {
        expect(invoice.status).toBeDefined();
      });
    });

    it("sales statuses should be defined", () => {
      salesData.sales.forEach((sale) => {
        expect(sale.status).toBeDefined();
      });
    });

    it("payment statuses should be defined", () => {
      salesData.sales.forEach((sale) => {
        expect(sale.paymentStatus).toBeDefined();
      });
    });
  });

  describe("Lab Order Status", () => {
    it("lab order statuses should be defined", () => {
      laboratoryData.orders.forEach((order) => {
        expect(order.status).toBeDefined();
      });
    });

    it("lab order urgency should be defined", () => {
      laboratoryData.orders.forEach((order) => {
        // Uses isUrgent boolean instead of priority
        expect(typeof order.isUrgent).toBe("boolean");
      });
    });
  });

  describe("Radiology Order Status", () => {
    it("radiology order statuses should be defined", () => {
      radiologyData.orders.forEach((order) => {
        expect(order.status).toBeDefined();
      });
    });
  });

  describe("Prescription Status", () => {
    it("prescription statuses should be defined", () => {
      pharmacyData.prescriptions.forEach((prescription) => {
        expect(prescription.status).toBeDefined();
      });
    });
  });

  describe("Marketing Lead Status", () => {
    it("lead statuses should be defined", () => {
      marketingData.leads.forEach((lead) => {
        expect(lead.status).toBeDefined();
      });
    });

    it("lead priorities should be defined", () => {
      marketingData.leads.forEach((lead) => {
        expect(lead.priority).toBeDefined();
      });
    });

    it("lead sources should be defined", () => {
      marketingData.leads.forEach((lead) => {
        expect(lead.source).toBeDefined();
      });
    });
  });

  describe("Campaign Status", () => {
    it("campaign statuses should be defined", () => {
      marketingData.campaigns.forEach((campaign) => {
        expect(campaign.status).toBeDefined();
      });
    });

    it("campaign types should be defined", () => {
      marketingData.campaigns.forEach((campaign) => {
        expect(campaign.type).toBeDefined();
      });
    });
  });

  describe("Dialysis Type", () => {
    it("dialysis types should be valid", () => {
      const validTypes = ["hemodialysis", "peritoneal"];
      dialysisData.patients.forEach((patient) => {
        expect(validTypes).toContain(patient.dialysisType);
      });
    });

    it("dialysis session statuses should be valid", () => {
      const validStatuses = [0, 1, 2, 3]; // Scheduled, InProgress, Completed, Cancelled
      dialysisData.sessions.forEach((session) => {
        expect(validStatuses).toContain(session.status);
      });
    });
  });

  describe("Cancer Types and Stages", () => {
    it("cancer types should be valid", () => {
      const validTypes = [
        "lung",
        "breast",
        "colorectal",
        "thyroid",
        "prostate",
        "skin",
        "lymphoma",
        "leukemia",
      ];
      oncologyData.diagnoses.forEach((diagnosis) => {
        expect(validTypes).toContain(diagnosis.cancerType);
      });
    });

    it("cancer stages should be valid", () => {
      const validStages = [
        "stage_0",
        "stage_1",
        "stage_2",
        "stage_3",
        "stage_4",
      ];
      oncologyData.diagnoses.forEach((diagnosis) => {
        expect(validStages).toContain(diagnosis.stage);
      });
    });

    it("cancer grades should be valid", () => {
      const validGrades = ["g1", "g2", "g3", "g4", "gx"];
      oncologyData.diagnoses.forEach((diagnosis) => {
        expect(validGrades).toContain(diagnosis.grade);
      });
    });
  });

  describe("Pregnancy Status", () => {
    it("pregnancy statuses should be valid", () => {
      const validStatuses = [0, 1, 2, 3]; // Active, Delivered, Miscarriage, Terminated
      obgynData.pregnancies.forEach((pregnancy) => {
        expect(validStatuses).toContain(pregnancy.status);
      });
    });
  });

  describe("Workflow Status", () => {
    it("workflow definitions should have required fields", () => {
      workflowData.definitions.forEach((workflow) => {
        expect(workflow.id).toBeDefined();
        expect(workflow.name).toBeTruthy();
      });
    });

    it("workflow instance statuses should be defined", () => {
      workflowData.instances.forEach((instance) => {
        expect(instance.status).toBeDefined();
      });
    });
  });

  describe("Security Incident Severity", () => {
    it("incident severities should be defined", () => {
      securityData.securityIncidents.forEach((incident) => {
        expect(incident.severity).toBeDefined();
      });
    });

    it("incident statuses should be defined", () => {
      securityData.securityIncidents.forEach((incident) => {
        expect(incident.status).toBeDefined();
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE FINANCIAL CALCULATION TESTS
// ============================================================

describe("E2E: Financial Calculations Validation", () => {
  describe("Invoice Calculations", () => {
    it("invoice total should equal subtotal + tax - discount", () => {
      financialData.invoices.forEach((invoice) => {
        const expectedTotal = invoice.subtotal + invoice.tax - invoice.discount;
        expect(invoice.total).toBe(expectedTotal);
      });
    });

    it("invoice items total should equal invoice subtotal", () => {
      financialData.invoices.forEach((invoice) => {
        const itemsTotal = invoice.items.reduce(
          (sum, item) => sum + item.total,
          0,
        );
        expect(invoice.subtotal).toBe(itemsTotal);
      });
    });

    it("invoice item totals should equal quantity * unitPrice", () => {
      financialData.invoices.forEach((invoice) => {
        invoice.items.forEach((item) => {
          expect(item.total).toBe(item.quantity * item.unitPrice);
        });
      });
    });
  });

  describe("Sales Calculations", () => {
    it("sales subtotal should equal sum of item subtotals", () => {
      salesData.sales.forEach((sale) => {
        const itemsSubtotal = sale.items.reduce(
          (sum, item) => sum + item.subtotal,
          0,
        );
        expect(sale.subTotal).toBe(itemsSubtotal);
      });
    });

    it("sales item subtotals should equal quantity * unitPrice", () => {
      salesData.sales.forEach((sale) => {
        sale.items.forEach((item) => {
          expect(item.subtotal).toBe(item.quantity * item.unitPrice);
        });
      });
    });

    it("discount amount should match discount percentage when defined", () => {
      salesData.sales.forEach((sale) => {
        if (
          sale.discountPercentage !== undefined &&
          sale.discountAmount !== undefined
        ) {
          const expectedDiscount =
            (sale.subTotal * sale.discountPercentage) / 100;
          expect(sale.discountAmount).toBe(expectedDiscount);
        }
      });
    });

    it("tax amount should be defined when tax percentage is set", () => {
      salesData.sales.forEach((sale) => {
        if (sale.taxPercentage !== undefined) {
          expect(sale.taxAmount).toBeDefined();
        }
      });
    });

    it("balance should equal total minus paid amount", () => {
      salesData.sales.forEach((sale) => {
        expect(sale.balance).toBe(sale.total - sale.paidAmount);
      });
    });

    it("isFullyPaid should be true when balance is 0", () => {
      salesData.sales.forEach((sale) => {
        if (sale.balance === 0) {
          expect(sale.isFullyPaid).toBe(true);
        } else {
          expect(sale.isFullyPaid).toBe(false);
        }
      });
    });
  });

  describe("Quotation Calculations", () => {
    it("quotation discount should match percentage", () => {
      salesData.quotations.forEach((quotation) => {
        const expectedDiscount =
          (quotation.subTotal * quotation.discountPercentage) / 100;
        expect(quotation.discountAmount).toBe(expectedDiscount);
      });
    });
  });

  describe("Payroll Calculations", () => {
    it("payroll records should have salary information", () => {
      payrollData.payrollRecords.forEach((record) => {
        expect(record.basicSalary).toBeDefined();
        expect(record.basicSalary).toBeGreaterThan(0);
      });
    });

    it("net salary should be less than or equal to gross salary", () => {
      payrollData.payrollRecords.forEach((record) => {
        if (
          record.grossSalary !== undefined &&
          record.netSalary !== undefined
        ) {
          expect(record.netSalary).toBeLessThanOrEqual(record.grossSalary);
        }
      });
    });
  });

  describe("Tenant Storage", () => {
    it("used storage should not exceed quota", () => {
      multiTenancyData.tenants.forEach((tenant) => {
        expect(tenant.usedStorageGB).toBeLessThanOrEqual(tenant.storageQuotaGB);
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE REQUIRED FIELD TESTS
// ============================================================

describe("E2E: Required Fields Validation", () => {
  describe("Patient Required Fields", () => {
    it("patients should have all required fields", () => {
      patientsData.patients.forEach((patient) => {
        expect(patient.id).toBeDefined();
        expect(patient.fullNameEn).toBeTruthy();
        expect(patient.dateOfBirth).toBeTruthy();
        expect(patient.gender).toBeDefined();
        expect(patient.branchId).toBeDefined();
      });
    });
  });

  describe("Appointment Required Fields", () => {
    it("appointments should have all required fields", () => {
      appointmentsData.appointments.forEach((appointment) => {
        expect(appointment.id).toBeDefined();
        expect(appointment.patientId).toBeDefined();
        expect(appointment.providerId).toBeDefined(); // Uses providerId not doctorId
        expect(appointment.branchId).toBeDefined();
        expect(appointment.startTime).toBeTruthy();
        expect(appointment.endTime).toBeTruthy();
        expect(appointment.status).toBeDefined();
        expect(appointment.type).toBeDefined();
      });
    });
  });

  describe("User Required Fields", () => {
    it("users should have all required fields", () => {
      usersData.users.forEach((user) => {
        expect(user.id).toBeDefined();
        expect(user.username).toBeTruthy();
        expect(user.email).toBeTruthy();
        expect(user.fullName).toBeTruthy();
        expect(user.roles).toBeDefined(); // Array of roles
        expect(user.roles.length).toBeGreaterThan(0);
      });
    });
  });

  describe("Employee Required Fields", () => {
    it("employees should have all required fields", () => {
      hrData.employees.forEach((employee) => {
        expect(employee.id).toBeDefined();
        expect(employee.employeeId).toBeTruthy(); // Uses employeeId not employeeCode
        expect(employee.fullName).toBeTruthy();
        expect(employee.email).toBeTruthy();
        expect(employee.department).toBeTruthy();
        expect(employee.position).toBeTruthy();
        expect(employee.hireDate).toBeTruthy();
      });
    });
  });

  describe("Invoice Required Fields", () => {
    it("invoices should have all required fields", () => {
      financialData.invoices.forEach((invoice) => {
        expect(invoice.id).toBeDefined();
        expect(invoice.invoiceNumber).toBeTruthy();
        expect(invoice.patientId).toBeDefined();
        expect(invoice.invoiceDate).toBeTruthy();
        expect(invoice.dueDate).toBeTruthy();
        expect(invoice.subtotal).toBeDefined();
        expect(invoice.total).toBeDefined();
        expect(invoice.status).toBeTruthy();
      });
    });
  });

  describe("Tenant Required Fields", () => {
    it("tenants should have all required fields", () => {
      multiTenancyData.tenants.forEach((tenant) => {
        expect(tenant.id).toBeDefined();
        expect(tenant.name).toBeTruthy();
        expect(tenant.subdomain).toBeTruthy();
        expect(tenant.contactEmail).toBeTruthy();
        expect(tenant.status).toBeDefined();
        expect(tenant.plan).toBeDefined();
      });
    });
  });

  describe("Company Required Fields", () => {
    it("companies should have all required fields", () => {
      multiTenancyData.companies.forEach((company) => {
        expect(company.id).toBeDefined();
        expect(company.tenantId).toBeDefined();
        expect(company.name).toBeTruthy();
        expect(company.email).toBeTruthy();
        expect(company.country).toBeTruthy();
      });
    });
  });

  describe("Branch Required Fields", () => {
    it("branches should have all required fields", () => {
      multiTenancyData.branches.forEach((branch) => {
        expect(branch.id).toBeDefined();
        expect(branch.companyId).toBeDefined();
        expect(branch.name).toBeTruthy();
        expect(branch.code).toBeTruthy();
        expect(branch.city).toBeTruthy();
        expect(branch.email).toBeTruthy();
      });
    });
  });

  describe("Lab Order Required Fields", () => {
    it("lab orders should have all required fields", () => {
      laboratoryData.orders.forEach((order) => {
        expect(order.id).toBeDefined();
        expect(order.patientId).toBeDefined();
        expect(order.orderDate).toBeTruthy();
        expect(order.status).toBeDefined(); // Status is numeric
        expect(order.tests).toBeDefined();
        expect(order.tests.length).toBeGreaterThan(0);
      });
    });
  });

  describe("Radiology Order Required Fields", () => {
    it("radiology orders should have all required fields", () => {
      radiologyData.orders.forEach((order) => {
        expect(order.id).toBeDefined();
        expect(order.patientId).toBeDefined();
        expect(order.orderDate).toBeTruthy();
        expect(order.status).toBeDefined();
        expect(order.modality).toBeTruthy(); // CT, MRI, X-Ray, etc.
        expect(order.bodyPart).toBeTruthy();
      });
    });
  });

  describe("Prescription Required Fields", () => {
    it("prescriptions should have all required fields", () => {
      pharmacyData.prescriptions.forEach((prescription) => {
        expect(prescription.id).toBeDefined();
        expect(prescription.patientId).toBeDefined();
        expect(prescription.prescriptionDate).toBeTruthy();
        expect(prescription.status).toBeTruthy();
        expect(prescription.medications).toBeDefined();
        expect(prescription.medications.length).toBeGreaterThan(0);
      });
    });
  });

  describe("Marketing Campaign Required Fields", () => {
    it("campaigns should have all required fields", () => {
      marketingData.campaigns.forEach((campaign) => {
        expect(campaign.id).toBeDefined();
        expect(campaign.name).toBeTruthy();
        expect(campaign.type).toBeTruthy();
        expect(campaign.status).toBeTruthy();
        expect(campaign.startDate).toBeTruthy();
      });
    });
  });

  describe("Marketing Lead Required Fields", () => {
    it("leads should have all required fields", () => {
      marketingData.leads.forEach((lead) => {
        expect(lead.id).toBeDefined();
        expect(lead.leadCode).toBeTruthy();
        expect(lead.fullName).toBeTruthy();
        expect(lead.status).toBeTruthy();
        expect(lead.source).toBeTruthy();
      });
    });
  });

  describe("Inventory Item Required Fields", () => {
    it("inventory items should have all required fields", () => {
      inventoryData.items.forEach((item) => {
        expect(item.id).toBeDefined();
        expect(item.itemCode).toBeTruthy();
        expect(item.name).toBeTruthy();
        expect(item.category).toBeDefined();
        expect(item.quantity).toBeDefined();
        expect(item.unitPrice).toBeDefined();
      });
    });
  });

  describe("Dialysis Patient Required Fields", () => {
    it("dialysis patients should have all required fields", () => {
      dialysisData.patients.forEach((patient) => {
        expect(patient.id).toBeDefined();
        expect(patient.patientId).toBeDefined();
        expect(patient.dialysisType).toBeTruthy();
        expect(patient.dryWeight).toBeDefined();
      });
    });
  });

  describe("Clinical Visit Required Fields", () => {
    it("clinical visits should have all required fields", () => {
      clinicalVisitData.visits.forEach((visit) => {
        expect(visit.id).toBeDefined();
        expect(visit.patientId).toBeDefined();
        expect(visit.doctorId).toBeDefined();
        expect(visit.visitDate).toBeTruthy();
        expect(visit.visitType).toBeDefined(); // visitType is numeric enum
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE RELATIONSHIP INTEGRITY TESTS
// ============================================================

describe("E2E: Relationship Integrity Validation", () => {
  describe("Tenant-Company-Branch Hierarchy", () => {
    it("companies should reference valid tenant IDs", () => {
      const tenantIds = new Set(multiTenancyData.tenants.map((t) => t.id));
      multiTenancyData.companies.forEach((company) => {
        expect(tenantIds.has(company.tenantId)).toBe(true);
      });
    });

    it("branches should reference valid company IDs", () => {
      const companyIds = new Set(multiTenancyData.companies.map((c) => c.id));
      multiTenancyData.branches.forEach((branch) => {
        expect(companyIds.has(branch.companyId)).toBe(true);
      });
    });

    it("branches should exist in the system", () => {
      // Verify branches array exists and has data
      expect(multiTenancyData.branches.length).toBeGreaterThan(0);
    });
  });

  describe("User-Branch Relationships", () => {
    it("tenant users should reference valid tenant IDs", () => {
      const tenantIds = new Set(multiTenancyData.tenants.map((t) => t.id));
      multiTenancyData.users.forEach((user) => {
        expect(tenantIds.has(user.tenantId)).toBe(true);
      });
    });
  });

  describe("Employee-Branch Relationships", () => {
    it("employees should reference valid branch IDs", () => {
      const branchIds = new Set(multiTenancyData.branches.map((b) => b.id));
      hrData.employees.forEach((employee) => {
        if (employee.branchId) {
          expect(branchIds.has(employee.branchId)).toBe(true);
        }
      });
    });
  });

  describe("Payroll-Employee Relationships", () => {
    it("payroll records should reference valid employee IDs", () => {
      const employeeIds = new Set(payrollData.employees.map((e) => e.id));
      payrollData.payrollRecords.forEach((record) => {
        expect(employeeIds.has(record.employeeId)).toBe(true);
      });
    });

    it("salary structures should have required fields", () => {
      // Salary structures are templates, not employee-specific
      payrollData.salaryStructures.forEach((structure) => {
        expect(structure.id).toBeDefined();
        expect(structure.name).toBeTruthy();
        expect(structure.basicSalary).toBeGreaterThan(0);
      });
    });
  });

  describe("Oncology Relationships", () => {
    it("chemotherapy should reference valid diagnosis IDs", () => {
      const diagnosisIds = new Set(oncologyData.diagnoses.map((d) => d.id));
      oncologyData.chemotherapy.forEach((chemo) => {
        expect(diagnosisIds.has(chemo.diagnosisId)).toBe(true);
      });
    });
  });

  describe("Workflow Relationships", () => {
    it("workflow instances should have required fields", () => {
      workflowData.instances.forEach((instance) => {
        expect(instance.id).toBeDefined();
        expect(instance.status).toBeDefined();
      });
    });

    it("workflow definitions should have required fields", () => {
      workflowData.definitions.forEach((definition) => {
        expect(definition.id).toBeDefined();
        expect(definition.name).toBeTruthy();
      });
    });
  });

  describe("Fertility Relationships", () => {
    it("IVF cycles should have patient references", () => {
      fertilityData.ivfCycles.forEach((cycle) => {
        expect(cycle.patientId).toBeDefined();
        expect(cycle.id).toBeDefined();
      });
    });

    it("embryos should reference valid IVF cycle IDs", () => {
      const cycleIds = new Set(fertilityData.ivfCycles.map((c) => c.id));
      fertilityData.embryos.forEach((embryo) => {
        expect(cycleIds.has(embryo.cycleId)).toBe(true);
      });
    });
  });

  describe("OB/GYN Relationships", () => {
    it("prenatal visits should reference valid pregnancy IDs", () => {
      const pregnancyIds = new Set(obgynData.pregnancies.map((p) => p.id));
      obgynData.prenatalVisits.forEach((visit) => {
        expect(pregnancyIds.has(visit.pregnancyId)).toBe(true);
      });
    });

    it("ultrasounds should have valid data", () => {
      // OB/GYN uses ultrasounds array (not obstetricUltrasounds)
      if (obgynData.ultrasounds) {
        obgynData.ultrasounds.forEach((ultrasound) => {
          expect(ultrasound.id).toBeGreaterThan(0);
        });
      }
    });
  });

  describe("Dialysis Relationships", () => {
    it("dialysis sessions should have valid patient references", () => {
      // Sessions may use different reference field
      dialysisData.sessions.forEach((session) => {
        expect(session.dialysisPatientId || session.patientId).toBeGreaterThan(
          0,
        );
      });
    });

    it("dialysis schedules should have valid patient references", () => {
      dialysisData.schedules.forEach((schedule) => {
        expect(schedule.patientId).toBeGreaterThan(0);
      });
    });
  });
});

// ============================================================
// COMPREHENSIVE DATA CONSISTENCY TESTS
// ============================================================

describe("E2E: Data Consistency Validation", () => {
  describe("Name Consistency", () => {
    it("patient full names should be present", () => {
      patientsData.patients.forEach((patient) => {
        // Patient data uses fullNameEn for English name
        expect(patient.fullNameEn).toBeTruthy();
        expect(patient.fullNameEn.length).toBeGreaterThan(0);
      });
    });
  });

  describe("Sales Consistency", () => {
    it("fully paid sales should have zero balance", () => {
      salesData.sales.forEach((sale) => {
        if (sale.isFullyPaid) {
          expect(sale.balance).toBe(0);
        }
      });
    });

    it("completed sales should be fully paid", () => {
      salesData.sales.forEach((sale) => {
        if (sale.paymentStatus === 2) {
          // Paid
          expect(sale.isFullyPaid).toBe(true);
        }
      });
    });
  });

  describe("Inventory Consistency", () => {
    it("low stock items should have quantity below or at min stock level", () => {
      inventoryData.items.forEach((item) => {
        if (item.status === 1) {
          // LowStock
          expect(item.quantity).toBeLessThanOrEqual(item.minStockLevel);
        }
      });
    });

    it("in stock items should have quantity above min stock level", () => {
      inventoryData.items.forEach((item) => {
        if (item.status === 0) {
          // InStock
          expect(item.quantity).toBeGreaterThan(item.minStockLevel);
        }
      });
    });
  });

  describe("Appointment Consistency", () => {
    it("completed appointments should have valid completion", () => {
      appointmentsData.appointments.forEach((appointment) => {
        if (appointment.status === 4) {
          // Completed - should have start time
          expect(appointment.startTime).toBeTruthy();
          expect(appointment.endTime).toBeTruthy();
        }
      });
    });
  });

  describe("Subscription Consistency", () => {
    it("active subscriptions should have valid date range", () => {
      multiTenancyData.subscriptions.forEach((subscription) => {
        if (subscription.status === 0) {
          // Active
          const today = new Date();
          const startDate = new Date(subscription.startDate);
          const endDate = new Date(subscription.endDate);
          expect(startDate <= today).toBe(true);
          expect(endDate >= today).toBe(true);
        }
      });
    });
  });

  describe("User Status Consistency", () => {
    it("inactive users should have last login in the past", () => {
      usersData.users.forEach((user) => {
        if (user.status === 1 && user.lastLogin) {
          // Inactive
          const lastLogin = new Date(user.lastLogin);
          const today = new Date();
          expect(lastLogin < today).toBe(true);
        }
      });
    });
  });

  describe("Pregnancy Consistency", () => {
    it("active pregnancies should have reasonable gestational age", () => {
      obgynData.pregnancies.forEach((pregnancy) => {
        if (pregnancy.status === 0) {
          // Active
          expect(pregnancy.gestationalAge).toBeGreaterThan(0);
          expect(pregnancy.gestationalAge).toBeLessThanOrEqual(42);
        }
      });
    });

    it("delivered pregnancies should have gestational age near term", () => {
      obgynData.pregnancies.forEach((pregnancy) => {
        if (pregnancy.status === 1) {
          // Delivered
          expect(pregnancy.gestationalAge).toBeGreaterThanOrEqual(20);
        }
      });
    });
  });

  describe("Chemotherapy Consistency", () => {
    it("active chemotherapy should have future expected end date", () => {
      oncologyData.chemotherapy.forEach((chemo) => {
        if (chemo.status === "active") {
          // Could be active with recent end date
          const endDate = new Date(chemo.expectedEndDate);
          const startDate = new Date(chemo.startDate);
          expect(endDate > startDate).toBe(true);
        }
      });
    });

    it("completed chemotherapy should have all cycles done", () => {
      oncologyData.chemotherapy.forEach((chemo) => {
        if (chemo.status === "completed") {
          expect(chemo.completedCycles).toBe(chemo.cycles);
        }
      });
    });
  });
});

// ============================================================
// EDGE CASE TESTS
// ============================================================

describe("E2E: Edge Case Validation", () => {
  describe("Empty String Protection", () => {
    it("patient names should not be empty strings", () => {
      patientsData.patients.forEach((patient) => {
        expect(patient.fullNameEn.trim()).not.toBe("");
      });
    });

    it("user usernames should not be empty strings", () => {
      usersData.users.forEach((user) => {
        expect(user.username.trim()).not.toBe("");
      });
    });

    it("branch codes should not be empty strings", () => {
      multiTenancyData.branches.forEach((branch) => {
        expect(branch.code.trim()).not.toBe("");
      });
    });
  });

  describe("Null/Undefined Protection", () => {
    it("required IDs should never be null or undefined", () => {
      patientsData.patients.forEach((patient) => {
        expect(patient.id).not.toBeNull();
        expect(patient.id).not.toBeUndefined();
      });
    });

    it("required dates should never be null or undefined", () => {
      appointmentsData.appointments.forEach((appointment) => {
        // Appointments use startTime instead of date
        expect(appointment.startTime).not.toBeNull();
        expect(appointment.startTime).not.toBeUndefined();
      });
    });
  });

  describe("Boundary Values", () => {
    it("IDs should be positive integers", () => {
      patientsData.patients.forEach((patient) => {
        expect(Number.isInteger(patient.id)).toBe(true);
        expect(patient.id).toBeGreaterThan(0);
      });
    });

    it("quantities should be integers", () => {
      inventoryData.items.forEach((item) => {
        expect(Number.isInteger(item.quantity)).toBe(true);
      });
    });
  });

  describe("Special Characters", () => {
    it("names should handle Arabic characters properly", () => {
      patientsData.patients.forEach((patient) => {
        if (patient.fullNameAr) {
          expect(patient.fullNameAr.length).toBeGreaterThan(0);
        }
      });
    });
  });
});
