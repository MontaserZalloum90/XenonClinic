// ============================================
// Company & Tenant Types
// ============================================

export type CompanyType = 'CLINIC' | 'TRADING' | 'OTHER';

export type ClinicType =
  | 'AUDIOLOGY'
  | 'DENTAL'
  | 'VET'
  | 'DERMATOLOGY'
  | 'OPHTHALMOLOGY'
  | 'GENERAL'
  | 'PHYSIOTHERAPY'
  | 'PEDIATRICS';

export type TenantStatus = 'TRIAL' | 'ACTIVE' | 'EXPIRED' | 'SUSPENDED' | 'CANCELLED';

export interface Tenant {
  id: string;
  name: string;
  slug: string;
  companyType: CompanyType;
  clinicType?: ClinicType;
  status: TenantStatus;
  trialStartDate?: string;
  trialEndDate?: string;
  createdAt: string;
  updatedAt: string;
}

// ============================================
// User & Auth Types
// ============================================

export type SystemRole = 'SYSTEM_ADMIN';

export type TenantRole = 'TENANT_ADMIN' | 'TENANT_MANAGER' | 'TENANT_USER';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  emailVerified: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface TenantMembership {
  id: string;
  userId: string;
  tenantId: string;
  role: TenantRole;
  isActive: boolean;
  createdAt: string;
}

export interface AuthSession {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface ExternalLogin {
  provider: 'google' | 'microsoft';
  providerUserId: string;
  email: string;
}

// ============================================
// Billing & Subscription Types
// ============================================

export type PlanCode = 'STARTER' | 'GROWTH' | 'ENTERPRISE';

export type SubscriptionStatus = 'ACTIVE' | 'PAST_DUE' | 'CANCELLED' | 'EXPIRED';

export type OrderStatus = 'PENDING' | 'PAID' | 'FAILED' | 'CANCELLED' | 'REFUNDED';

export type Currency = 'AED' | 'USD';

export interface PricingTier {
  code: PlanCode;
  name: string;
  description: string;
  basePrice: {
    monthly: number;
    annual: number;
  };
  includedBranches: number;
  includedUsers: number;
  extraBranchPrice: number;
  extraUserPrice: number;
  features: string[];
  supportLevel: string;
  recommended?: boolean;
}

export interface PricingAddOn {
  code: string;
  name: string;
  description: string;
  price: {
    monthly: number;
    annual: number;
  };
}

export interface TenantSubscription {
  id: string;
  tenantId: string;
  planCode: PlanCode;
  status: SubscriptionStatus;
  startDate: string;
  endDate: string;
  branchesLimit: number;
  usersLimit: number;
  addOns: string[];
  stripeSubscriptionId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface Order {
  id: string;
  tenantId: string;
  planCode: PlanCode;
  durationMonths: number;
  branchesPurchased: number;
  usersPurchased: number;
  addOns: string[];
  subtotal: number;
  discount: number;
  tax: number;
  total: number;
  currency: Currency;
  status: OrderStatus;
  stripeSessionId?: string;
  stripePaymentIntentId?: string;
  createdAt: string;
  paidAt?: string;
}

export interface PaymentTransaction {
  id: string;
  orderId: string;
  tenantId: string;
  gatewayEventId: string;
  eventType: string;
  amount: number;
  currency: Currency;
  status: string;
  rawPayload: string;
  processedAt: string;
}

// ============================================
// Lead / Demo Request Types
// ============================================

export interface LeadRequest {
  name: string;
  company: string;
  email: string;
  phone: string;
  companyType: CompanyType;
  clinicType?: ClinicType;
  estimatedBranches: number;
  estimatedUsers: number;
  modulesOfInterest: string[];
  deploymentPreference: 'CLOUD' | 'ON_PREM' | 'HYBRID';
  notes?: string;
  honeypot?: string; // Anti-spam field
}

// ============================================
// Pricing Calculator Types
// ============================================

export interface PricingCalculatorInput {
  plan: PlanCode;
  branches: number;
  users: number;
  durationMonths: 1 | 3 | 6 | 12;
  addOns: string[];
  currency: Currency;
}

export interface PricingCalculatorResult {
  plan: PlanCode;
  basePrice: number;
  extraBranchesPrice: number;
  extraUsersPrice: number;
  addOnsPrice: number;
  subtotal: number;
  discount: number;
  discountPercent: number;
  monthlyEquivalent: number;
  total: number;
  currency: Currency;
  breakdown: {
    label: string;
    amount: number;
  }[];
}

// ============================================
// Content Types
// ============================================

export interface FeatureContent {
  slug: string;
  title: string;
  shortDescription: string;
  fullDescription: string;
  icon: string;
  capabilities: string[];
  whoItsFor: string[];
  screenshotPlaceholder?: string;
  category: 'core' | 'clinical' | 'operations' | 'analytics';
}

export interface DocPage {
  slug: string;
  title: string;
  description: string;
  content: string;
  category: string;
  order: number;
}

// ============================================
// API Response Types
// ============================================

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: Record<string, string[]>;
  };
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
