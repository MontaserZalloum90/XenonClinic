// Multi-Tenancy Types

export const TenantStatus = {
  Active: 0,
  Suspended: 1,
  PendingActivation: 2,
  Cancelled: 3,
} as const;

export type TenantStatus = (typeof TenantStatus)[keyof typeof TenantStatus];

export const SubscriptionPlan = {
  Trial: 0,
  Basic: 1,
  Professional: 2,
  Enterprise: 3,
} as const;

export type SubscriptionPlan = (typeof SubscriptionPlan)[keyof typeof SubscriptionPlan];

export interface Tenant {
  id: number;
  name: string;
  subdomain: string;
  status: TenantStatus;
  plan: SubscriptionPlan;
  contactEmail: string;
  contactPhone?: string;
  maxUsers: number;
  maxCompanies: number;
  storageQuotaGB: number;
  usedStorageGB: number;
  subscriptionStart: string;
  subscriptionEnd: string;
  settings?: TenantSettings;
  createdAt: string;
  updatedAt?: string;
}

export interface TenantSettings {
  logoUrl?: string;
  primaryColor?: string;
  timezone: string;
  dateFormat: string;
  currency: string;
  language: string;
  features: string[];
}

export interface Company {
  id: number;
  tenantId: number;
  name: string;
  nameAr?: string;
  legalName: string;
  registrationNumber: string;
  taxNumber?: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
  logoUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface Branch {
  id: number;
  companyId: number;
  companyName?: string;
  name: string;
  nameAr?: string;
  code: string;
  address: string;
  city: string;
  phone: string;
  email?: string;
  manager?: string;
  workingHours?: WorkingHours;
  isMainBranch: boolean;
  isActive: boolean;
  latitude?: number;
  longitude?: number;
  createdAt: string;
}

export interface WorkingHours {
  monday: DaySchedule;
  tuesday: DaySchedule;
  wednesday: DaySchedule;
  thursday: DaySchedule;
  friday: DaySchedule;
  saturday: DaySchedule;
  sunday: DaySchedule;
}

export interface DaySchedule {
  isOpen: boolean;
  openTime?: string;
  closeTime?: string;
  breakStart?: string;
  breakEnd?: string;
}

export interface TenantUser {
  id: number;
  tenantId: number;
  companyId?: number;
  branchId?: number;
  username: string;
  email: string;
  fullName: string;
  role: string;
  permissions: string[];
  isActive: boolean;
  lastLogin?: string;
  createdAt: string;
}

export interface Subscription {
  id: number;
  tenantId: number;
  plan: SubscriptionPlan;
  status: 'active' | 'expired' | 'cancelled' | 'pending';
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  billingCycle: 'monthly' | 'quarterly' | 'annually';
  amount: number;
  currency: string;
  nextBillingDate?: string;
  paymentMethod?: string;
}

export interface MultiTenancyStatistics {
  totalTenants: number;
  activeTenants: number;
  totalCompanies: number;
  totalBranches: number;
  totalUsers: number;
  activeSubscriptions: number;
  monthlyRevenue: number;
  trialConversions: number;
}
