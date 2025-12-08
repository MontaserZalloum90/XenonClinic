export const EmployeeStatus = {
  Active: 0,
  OnLeave: 1,
  Suspended: 2,
  Terminated: 3,
} as const;

export type EmployeeStatus = typeof EmployeeStatus[keyof typeof EmployeeStatus];

export const EmployeeRole = {
  Doctor: 0,
  Nurse: 1,
  Receptionist: 2,
  Technician: 3,
  Administrator: 4,
  Other: 5,
} as const;

export type EmployeeRole = typeof EmployeeRole[keyof typeof EmployeeRole];

export interface Employee {
  id: number;
  employeeNumber: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: EmployeeRole;
  department?: string;
  status: EmployeeStatus;
  hireDate: string;
  salary?: number;
  emergencyContact?: string;
  emergencyPhone?: string;
  address?: string;
  nationalId?: string;
  dateOfBirth?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface HRStatistics {
  totalEmployees: number;
  activeEmployees: number;
  onLeaveEmployees: number;
  newHiresThisMonth: number;
  departmentDistribution?: Record<string, number>;
  roleDistribution?: Record<string, number>;
}

export interface EmployeeFormData {
  employeeNumber: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: EmployeeRole;
  department?: string;
  status: EmployeeStatus;
  hireDate: string;
  salary?: number;
  emergencyContact?: string;
  emergencyPhone?: string;
  address?: string;
  nationalId?: string;
  dateOfBirth?: string;
}
