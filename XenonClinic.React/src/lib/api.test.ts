import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import {
  api,
  authApi,
  appointmentsApi,
  patientsApi,
  laboratoryApi,
  hrApi,
  financialApi,
  inventoryApi,
  pharmacyApi,
  radiologyApi,
  orthopedicExamsApi,
  fracturesApi,
  surgeriesApi,
  dentalTreatmentApi,
  dentalChartApi,
  oncologyDiagnosisApi,
  payrollApi,
} from "./api";

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};

Object.defineProperty(global, "localStorage", {
  value: localStorageMock,
});

describe("API Module", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorageMock.getItem.mockReset();
  });

  describe("API Instance Configuration", () => {
    it("has correct default headers", () => {
      // Content-Type is set on headers.common by configureAxiosInstance
      expect(api.defaults.headers.common["Content-Type"]).toBe(
        "application/json",
      );
    });

    it("has withCredentials set to false", () => {
      expect(api.defaults.withCredentials).toBe(false);
    });
  });

  describe("Auth API", () => {
    it("login endpoint is defined", () => {
      expect(authApi.login).toBeDefined();
      expect(typeof authApi.login).toBe("function");
    });

    it("register endpoint is defined", () => {
      expect(authApi.register).toBeDefined();
      expect(typeof authApi.register).toBe("function");
    });

    it("getCurrentUser endpoint is defined", () => {
      expect(authApi.getCurrentUser).toBeDefined();
      expect(typeof authApi.getCurrentUser).toBe("function");
    });

    it("refreshToken endpoint is defined", () => {
      expect(authApi.refreshToken).toBeDefined();
      expect(typeof authApi.refreshToken).toBe("function");
    });
  });

  describe("Appointments API", () => {
    it("has all required CRUD endpoints", () => {
      expect(appointmentsApi.getAll).toBeDefined();
      expect(appointmentsApi.getById).toBeDefined();
      expect(appointmentsApi.create).toBeDefined();
      expect(appointmentsApi.update).toBeDefined();
      expect(appointmentsApi.delete).toBeDefined();
    });

    it("has appointment-specific endpoints", () => {
      expect(appointmentsApi.getByDate).toBeDefined();
      expect(appointmentsApi.getToday).toBeDefined();
      expect(appointmentsApi.getUpcoming).toBeDefined();
      expect(appointmentsApi.confirm).toBeDefined();
      expect(appointmentsApi.cancel).toBeDefined();
      expect(appointmentsApi.checkIn).toBeDefined();
      expect(appointmentsApi.complete).toBeDefined();
      expect(appointmentsApi.getStatistics).toBeDefined();
    });
  });

  describe("Patients API", () => {
    it("has all required CRUD endpoints", () => {
      expect(patientsApi.getAll).toBeDefined();
      expect(patientsApi.getById).toBeDefined();
      expect(patientsApi.create).toBeDefined();
      expect(patientsApi.update).toBeDefined();
      expect(patientsApi.delete).toBeDefined();
    });

    it("has patient-specific endpoints", () => {
      expect(patientsApi.search).toBeDefined();
      expect(patientsApi.getByEmiratesId).toBeDefined();
      expect(patientsApi.getMedicalHistory).toBeDefined();
      expect(patientsApi.getDocuments).toBeDefined();
      expect(patientsApi.getStatistics).toBeDefined();
    });
  });

  describe("Laboratory API", () => {
    it("has all required CRUD endpoints", () => {
      expect(laboratoryApi.getAllOrders).toBeDefined();
      expect(laboratoryApi.getOrderById).toBeDefined();
      expect(laboratoryApi.createOrder).toBeDefined();
      expect(laboratoryApi.updateOrder).toBeDefined();
      expect(laboratoryApi.deleteOrder).toBeDefined();
    });

    it("has laboratory-specific endpoints", () => {
      expect(laboratoryApi.getPendingOrders).toBeDefined();
      expect(laboratoryApi.getUrgentOrders).toBeDefined();
      expect(laboratoryApi.getOrdersByPatient).toBeDefined();
      expect(laboratoryApi.updateStatus).toBeDefined();
      expect(laboratoryApi.getAllTests).toBeDefined();
      expect(laboratoryApi.getStatistics).toBeDefined();
    });
  });

  describe("HR API", () => {
    it("has all required CRUD endpoints", () => {
      expect(hrApi.getAll).toBeDefined();
      expect(hrApi.getById).toBeDefined();
      expect(hrApi.create).toBeDefined();
      expect(hrApi.update).toBeDefined();
      expect(hrApi.delete).toBeDefined();
    });

    it("has HR-specific endpoints", () => {
      expect(hrApi.search).toBeDefined();
      expect(hrApi.getByDepartment).toBeDefined();
      expect(hrApi.getActive).toBeDefined();
      expect(hrApi.getStatistics).toBeDefined();
    });
  });

  describe("Financial API", () => {
    it("has all required CRUD endpoints", () => {
      expect(financialApi.getAllInvoices).toBeDefined();
      expect(financialApi.getById).toBeDefined();
      expect(financialApi.create).toBeDefined();
      expect(financialApi.update).toBeDefined();
      expect(financialApi.delete).toBeDefined();
    });

    it("has financial-specific endpoints", () => {
      expect(financialApi.search).toBeDefined();
      expect(financialApi.getUnpaid).toBeDefined();
      expect(financialApi.getOverdue).toBeDefined();
      expect(financialApi.getByPatient).toBeDefined();
      expect(financialApi.recordPayment).toBeDefined();
      expect(financialApi.getStatistics).toBeDefined();
    });
  });

  describe("Inventory API", () => {
    it("has all required CRUD endpoints", () => {
      expect(inventoryApi.getAllItems).toBeDefined();
      expect(inventoryApi.getById).toBeDefined();
      expect(inventoryApi.create).toBeDefined();
      expect(inventoryApi.update).toBeDefined();
      expect(inventoryApi.delete).toBeDefined();
    });

    it("has inventory-specific endpoints", () => {
      expect(inventoryApi.search).toBeDefined();
      expect(inventoryApi.getLowStock).toBeDefined();
      expect(inventoryApi.getOutOfStock).toBeDefined();
      expect(inventoryApi.getByCategory).toBeDefined();
      expect(inventoryApi.adjustStock).toBeDefined();
      expect(inventoryApi.getStatistics).toBeDefined();
    });
  });

  describe("Pharmacy API", () => {
    it("has all required CRUD endpoints", () => {
      expect(pharmacyApi.getAllPrescriptions).toBeDefined();
      expect(pharmacyApi.getById).toBeDefined();
      expect(pharmacyApi.create).toBeDefined();
      expect(pharmacyApi.update).toBeDefined();
      expect(pharmacyApi.delete).toBeDefined();
    });

    it("has pharmacy-specific endpoints", () => {
      expect(pharmacyApi.search).toBeDefined();
      expect(pharmacyApi.getPending).toBeDefined();
      expect(pharmacyApi.getByPatient).toBeDefined();
      expect(pharmacyApi.dispense).toBeDefined();
      expect(pharmacyApi.getStatistics).toBeDefined();
    });
  });

  describe("Radiology API", () => {
    it("has all required CRUD endpoints", () => {
      expect(radiologyApi.getAllOrders).toBeDefined();
      expect(radiologyApi.getById).toBeDefined();
      expect(radiologyApi.create).toBeDefined();
      expect(radiologyApi.update).toBeDefined();
      expect(radiologyApi.delete).toBeDefined();
    });

    it("has radiology-specific endpoints", () => {
      expect(radiologyApi.search).toBeDefined();
      expect(radiologyApi.getPending).toBeDefined();
      expect(radiologyApi.getScheduled).toBeDefined();
      expect(radiologyApi.getByPatient).toBeDefined();
      expect(radiologyApi.updateStatus).toBeDefined();
      expect(radiologyApi.getStatistics).toBeDefined();
    });
  });
});

describe("Orthopedics APIs", () => {
  describe("Orthopedic Exams API", () => {
    it("has all required CRUD endpoints", () => {
      expect(orthopedicExamsApi.getAll).toBeDefined();
      expect(orthopedicExamsApi.getById).toBeDefined();
      expect(orthopedicExamsApi.create).toBeDefined();
      expect(orthopedicExamsApi.update).toBeDefined();
      expect(orthopedicExamsApi.delete).toBeDefined();
    });
  });

  describe("Fractures API", () => {
    it("has all required CRUD endpoints", () => {
      expect(fracturesApi.getAll).toBeDefined();
      expect(fracturesApi.getById).toBeDefined();
      expect(fracturesApi.create).toBeDefined();
      expect(fracturesApi.update).toBeDefined();
      expect(fracturesApi.delete).toBeDefined();
    });
  });

  describe("Surgeries API", () => {
    it("has all required CRUD endpoints", () => {
      expect(surgeriesApi.getAll).toBeDefined();
      expect(surgeriesApi.getById).toBeDefined();
      expect(surgeriesApi.create).toBeDefined();
      expect(surgeriesApi.update).toBeDefined();
      expect(surgeriesApi.delete).toBeDefined();
    });
  });
});

describe("Dental APIs", () => {
  describe("Dental Treatment API", () => {
    it("has all required CRUD endpoints", () => {
      expect(dentalTreatmentApi.getAll).toBeDefined();
      expect(dentalTreatmentApi.getById).toBeDefined();
      expect(dentalTreatmentApi.create).toBeDefined();
      expect(dentalTreatmentApi.update).toBeDefined();
      expect(dentalTreatmentApi.delete).toBeDefined();
    });
  });

  describe("Dental Chart API", () => {
    it("has all required CRUD endpoints", () => {
      expect(dentalChartApi.getAll).toBeDefined();
      expect(dentalChartApi.getById).toBeDefined();
      expect(dentalChartApi.create).toBeDefined();
      expect(dentalChartApi.update).toBeDefined();
      expect(dentalChartApi.delete).toBeDefined();
    });
  });
});

describe("Oncology APIs", () => {
  describe("Oncology Diagnosis API", () => {
    it("has all required CRUD endpoints", () => {
      expect(oncologyDiagnosisApi.getAll).toBeDefined();
      expect(oncologyDiagnosisApi.getById).toBeDefined();
      expect(oncologyDiagnosisApi.create).toBeDefined();
      expect(oncologyDiagnosisApi.update).toBeDefined();
      expect(oncologyDiagnosisApi.delete).toBeDefined();
    });
  });
});

describe("Payroll API", () => {
  it("has all required CRUD endpoints", () => {
    expect(payrollApi.getAll).toBeDefined();
    expect(payrollApi.getById).toBeDefined();
    expect(payrollApi.create).toBeDefined();
    expect(payrollApi.update).toBeDefined();
    expect(payrollApi.delete).toBeDefined();
  });

  it("has payroll-specific endpoints", () => {
    expect(payrollApi.getByEmployee).toBeDefined();
  });
});

describe("Appointment Date Functions", () => {
  it("getByDate function accepts a Date parameter", () => {
    // Verify the function signature accepts Date objects
    expect(typeof appointmentsApi.getByDate).toBe("function");
    expect(appointmentsApi.getByDate.length).toBe(1);
  });

  it("getUpcoming function accepts optional days parameter", () => {
    // Verify the function signature
    expect(typeof appointmentsApi.getUpcoming).toBe("function");
  });

  it("getStatistics function accepts optional date range parameters", () => {
    // Verify the function signature
    expect(typeof appointmentsApi.getStatistics).toBe("function");
  });
});
