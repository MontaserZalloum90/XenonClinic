export const PayrollStatus = {
  Draft: 0,
  Processed: 1,
  Approved: 2,
  Paid: 3,
  Cancelled: 4,
} as const;

export type PayrollStatus = typeof PayrollStatus[keyof typeof PayrollStatus];

export interface Allowance {
  id: number;
  name: string;
  amount: number;
  description?: string;
}

export interface Deduction {
  id: number;
  name: string;
  amount: number;
  description?: string;
}

export interface PayrollRecord {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeNumber?: string;
  department?: string;
  period: string;
  basicSalary: number;
  allowances: Allowance[];
  deductions: Deduction[];
  totalAllowances: number;
  totalDeductions: number;
  grossSalary: number;
  netSalary: number;
  status: PayrollStatus;
  processedDate?: string;
  approvedDate?: string;
  approvedBy?: string;
  paidDate?: string;
  paidBy?: string;
  paymentMethod?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface SalaryStructure {
  id: number;
  name: string;
  basicSalary: number;
  housingAllowance: number;
  transportAllowance: number;
  otherAllowances: number;
  effectiveDate: string;
  endDate?: string;
  isActive: boolean;
  description?: string;
  employeeCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface PayrollStatistics {
  totalPayroll: number;
  pendingApproval: number;
  paidThisMonth: number;
  totalEmployeesInPayroll: number;
  averageSalary: number;
  totalAllowances: number;
  totalDeductions: number;
  monthlyPayrollTrend?: Record<string, number>;
  departmentPayroll?: Record<string, number>;
}

export interface PayrollFormData {
  employeeId: number;
  period: string;
  basicSalary: number;
  allowances: Allowance[];
  deductions: Deduction[];
  notes?: string;
}

export interface SalaryStructureFormData {
  name: string;
  basicSalary: number;
  housingAllowance: number;
  transportAllowance: number;
  otherAllowances: number;
  effectiveDate: string;
  endDate?: string;
  description?: string;
}
