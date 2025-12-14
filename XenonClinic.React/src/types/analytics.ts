// Analytics Types

export const ReportType = {
  Daily: 0,
  Weekly: 1,
  Monthly: 2,
  Quarterly: 3,
  Annual: 4,
  Custom: 5,
} as const;

export type ReportType = typeof ReportType[keyof typeof ReportType];

export const ReportStatus = {
  Pending: 0,
  Generating: 1,
  Completed: 2,
  Failed: 3,
} as const;

export type ReportStatus = typeof ReportStatus[keyof typeof ReportStatus];

export interface AnalyticsDashboard {
  totalPatients: number;
  totalAppointments: number;
  totalRevenue: number;
  averageWaitTime: number;
  patientSatisfaction: number;
  appointmentsByDay: DayStatistic[];
  revenueByMonth: MonthStatistic[];
  topServices: ServiceStatistic[];
  departmentStats: DepartmentStatistic[];
}

export interface DayStatistic {
  date: string;
  count: number;
  label: string;
}

export interface MonthStatistic {
  month: string;
  revenue: number;
  label: string;
}

export interface ServiceStatistic {
  serviceName: string;
  count: number;
  revenue: number;
}

export interface DepartmentStatistic {
  departmentName: string;
  patientCount: number;
  revenue: number;
  appointmentCount: number;
}

export interface Report {
  id: number;
  name: string;
  type: ReportType;
  parameters: ReportParameters;
  generatedAt: string;
  generatedBy?: string;
  data: Record<string, unknown>;
  status: ReportStatus;
  fileUrl?: string;
  description?: string;
}

export interface ReportParameters {
  startDate?: string;
  endDate?: string;
  departmentId?: number;
  branchId?: number;
  serviceType?: string;
  includeCharts?: boolean;
  includeDetails?: boolean;
  [key: string]: string | number | boolean | undefined;
}

export interface ChartData {
  name: string;
  value: number;
  label?: string;
  color?: string;
}

export interface AnalyticsFilter {
  startDate?: string;
  endDate?: string;
  departmentId?: number;
  branchId?: number;
  serviceType?: string;
  reportType?: ReportType;
}

export interface CreateReportRequest {
  name: string;
  type: ReportType;
  parameters: ReportParameters;
  description?: string;
}

export interface AnalyticsStatistics {
  totalReports: number;
  reportsThisMonth: number;
  pendingReports: number;
  failedReports: number;
  lastGenerated?: string;
}
