/**
 * Platform API Client
 * Connects the website to the Xenon Platform Backend
 */

const PLATFORM_API_URL = process.env.NEXT_PUBLIC_PLATFORM_API_URL || 'http://localhost:5000';

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

interface PricingPlan {
  code: string;
  name: string;
  description: string;
  monthlyPrice: number;
  annualPrice: number;
  includedBranches: number;
  includedUsers: number;
  extraBranchPrice: number;
  extraUserPrice: number;
  features: string;
  supportLevel: string;
  isRecommended: boolean;
}

interface PricingEstimate {
  planCode: string;
  planName: string;
  billingCycle: string;
  currency: string;
  branches: number;
  users: number;
  includedBranches: number;
  includedUsers: number;
  extraBranches: number;
  extraUsers: number;
  basePrice: number;
  extraBranchesPrice: number;
  extraUsersPrice: number;
  subtotal: number;
  discountPercent: number;
  discountAmount: number;
  total: number;
  monthlyEquivalent: number;
  breakdown: Array<{ label: string; amount: number; isDiscount?: boolean }>;
}

interface TenantSignupRequest {
  companyName: string;
  companyType: string;
  clinicType?: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
  country?: string;
}

interface TenantSignupResponse {
  tenant: {
    id: string;
    name: string;
    slug: string;
    status: string;
    trialEndDate: string;
    trialDaysRemaining: number;
  };
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
  };
  token: string;
}

interface TenantLoginRequest {
  email: string;
  password: string;
}

interface TenantLoginResponse {
  tenant: {
    id: string;
    name: string;
    slug: string;
    status: string;
    trialEndDate: string;
    trialDaysRemaining: number;
    maxBranches: number;
    maxUsers: number;
  };
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
  };
  token: string;
}

interface DemoRequestSubmission {
  name: string;
  email: string;
  phone?: string;
  company: string;
  companyType?: string;
  clinicType?: string;
  estimatedBranches?: number;
  estimatedUsers?: number;
  inquiryType?: string;
  message?: string;
  modulesOfInterest?: string[];
  deploymentPreference?: string;
  source?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  honeypot?: string;
}

async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const url = `${PLATFORM_API_URL}${endpoint}`;

  const defaultHeaders: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  // Add auth token if available
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('tenant_token');
    if (token) {
      defaultHeaders['Authorization'] = `Bearer ${token}`;
    }
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    });

    const data = await response.json();

    if (!response.ok) {
      return {
        success: false,
        error: data.error || `Request failed with status ${response.status}`,
      };
    }

    return data;
  } catch (error) {
    console.error('API request failed:', error);
    return {
      success: false,
      error: 'Network error. Please try again.',
    };
  }
}

// ============================================
// Pricing API
// ============================================

export async function getPlans(): Promise<ApiResponse<PricingPlan[]>> {
  return apiRequest<PricingPlan[]>('/api/public/pricing/plans');
}

export async function getPricingEstimate(params: {
  planId: string;
  branches?: number;
  users?: number;
  billingCycle?: string;
  currency?: string;
}): Promise<ApiResponse<PricingEstimate>> {
  const queryParams = new URLSearchParams({
    planId: params.planId,
    branches: String(params.branches || 1),
    users: String(params.users || 5),
    billingCycle: params.billingCycle || 'Monthly',
    currency: params.currency || 'AED',
  });

  return apiRequest<PricingEstimate>(`/api/public/pricing/estimate?${queryParams}`);
}

export async function getDiscounts(): Promise<ApiResponse<Array<{
  cycle: string;
  months: number;
  discountPercent: number;
}>>> {
  return apiRequest('/api/public/pricing/discounts');
}

// ============================================
// Tenant API
// ============================================

export async function signup(
  data: TenantSignupRequest
): Promise<ApiResponse<TenantSignupResponse>> {
  return apiRequest<TenantSignupResponse>('/api/public/tenants/signup', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function login(
  data: TenantLoginRequest
): Promise<ApiResponse<TenantLoginResponse>> {
  return apiRequest<TenantLoginResponse>('/api/public/tenants/login', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function getCurrentTenant(): Promise<ApiResponse<{
  tenant: {
    id: string;
    name: string;
    slug: string;
    companyType: string;
    clinicType?: string;
    status: string;
    trialStartDate: string;
    trialEndDate: string;
    trialDaysRemaining: number;
    isTrialExpired: boolean;
  };
  license: {
    maxBranches: number;
    maxUsers: number;
    currentBranches: number;
    currentUsers: number;
    canAddBranch: boolean;
    canAddUser: boolean;
  };
  subscription: {
    planCode: string;
    status: string;
    startDate: string;
    endDate: string;
    daysRemaining: number;
    billingCycle: string;
    autoRenew: boolean;
  } | null;
}>> {
  return apiRequest('/api/public/tenants/me');
}

export async function checkSlugAvailability(
  slug: string
): Promise<ApiResponse<{ slug: string; isAvailable: boolean }>> {
  return apiRequest(`/api/public/tenants/check-slug?slug=${encodeURIComponent(slug)}`);
}

// ============================================
// Demo Request API
// ============================================

export async function submitDemoRequest(
  data: DemoRequestSubmission
): Promise<ApiResponse<{ id: string; status: string }>> {
  return apiRequest('/api/public/demo-request', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

// ============================================
// Contact API
// ============================================

interface ContactSubmission {
  name: string;
  email: string;
  company?: string;
  phone?: string;
  inquiryType: string;
  message: string;
}

export async function submitContactInquiry(
  data: ContactSubmission
): Promise<ApiResponse<{ id: string; status: string }>> {
  // Use demo request endpoint with sales/support inquiry types
  return apiRequest('/api/public/demo-request', {
    method: 'POST',
    body: JSON.stringify({
      name: data.name,
      email: data.email,
      company: data.company,
      phone: data.phone,
      inquiryType: data.inquiryType,
      message: data.message,
      source: 'contact-page',
    }),
  });
}

export type { ContactSubmission };

// ============================================
// Auth Helpers
// ============================================

export function setAuthToken(token: string): void {
  if (typeof window !== 'undefined') {
    localStorage.setItem('tenant_token', token);
  }
}

export function getAuthToken(): string | null {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('tenant_token');
  }
  return null;
}

export function clearAuthToken(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('tenant_token');
  }
}

export function isAuthenticated(): boolean {
  return !!getAuthToken();
}

// Export types
export type {
  ApiResponse,
  PricingPlan,
  PricingEstimate,
  TenantSignupRequest,
  TenantSignupResponse,
  TenantLoginRequest,
  TenantLoginResponse,
  DemoRequestSubmission,
};
