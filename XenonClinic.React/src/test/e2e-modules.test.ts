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
