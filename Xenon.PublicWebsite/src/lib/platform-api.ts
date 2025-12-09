/**
 * Platform API Client
 * Connects to Xenon.Platform backend
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  try {
    const token = localStorage.getItem('tenant_token');
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      return {
        success: false,
        error: errorData.message || `Request failed with status ${response.status}`,
      };
    }

    const data = await response.json();
    return { success: true, data };
  } catch (error) {
    return {
      success: false,
      error: error instanceof Error ? error.message : 'Network error',
    };
  }
}

// ============================================
// Pricing API
// ============================================

export interface PricingPlan {
  id: string;
  name: string;
  code: string;
  basePrice: number;
  includedBranches: number;
  includedUsers: number;
  features: string[];
}

export interface PricingEstimate {
  planId: string;
  planName: string;
  basePrice: number;
  extraBranchesPrice: number;
  extraUsersPrice: number;
  subtotal: number;
  discountPercent: number;
  discountAmount: number;
  total: number;
  monthlyEquivalent: number;
  currency: string;
}

export interface PricingEstimateRequest {
  planId: string;
  branches: number;
  users: number;
  billingCycle: string;
  currency?: string;
}

export async function getPlans(): Promise<ApiResponse<PricingPlan[]>> {
  return apiRequest('/api/public/pricing/plans');
}

export async function getPricingEstimate(
  request: PricingEstimateRequest
): Promise<ApiResponse<PricingEstimate>> {
  const params = new URLSearchParams({
    planId: request.planId,
    branches: request.branches.toString(),
    users: request.users.toString(),
    billingCycle: request.billingCycle,
    currency: request.currency || 'AED',
  });
  return apiRequest(`/api/public/pricing/estimate?${params}`);
}

// ============================================
// Tenant API
// ============================================

export interface TenantSignupRequest {
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

export interface TenantSignupResponse {
  tenantId: string;
  slug: string;
  token: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
  };
}

export interface TenantLoginRequest {
  email: string;
  password: string;
}

export interface TenantLoginResponse {
  token: string;
  tenant: {
    id: string;
    companyName: string;
    slug: string;
  };
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
  };
}

export async function signup(
  data: TenantSignupRequest
): Promise<ApiResponse<TenantSignupResponse>> {
  return apiRequest('/api/public/tenants/signup', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function login(
  data: TenantLoginRequest
): Promise<ApiResponse<TenantLoginResponse>> {
  return apiRequest('/api/public/tenants/login', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function getCurrentTenant(): Promise<ApiResponse<{ tenant: unknown; user: unknown }>> {
  return apiRequest('/api/public/tenants/me');
}

// ============================================
// Demo Request API
// ============================================

export interface DemoRequestSubmission {
  name: string;
  email: string;
  phone?: string;
  company?: string;
  companyType?: string;
  clinicType?: string;
  inquiryType?: string;
  message?: string;
  source?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
}

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

export interface ContactSubmission {
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
  return apiRequest('/api/public/demo-request', {
    method: 'POST',
    body: JSON.stringify({
      ...data,
      source: 'contact-page',
    }),
  });
}

// ============================================
// Auth Helpers
// ============================================

export function setAuthToken(token: string): void {
  localStorage.setItem('tenant_token', token);
}

export function getAuthToken(): string | null {
  return localStorage.getItem('tenant_token');
}

export function clearAuthToken(): void {
  localStorage.removeItem('tenant_token');
  localStorage.removeItem('tenant_info');
  localStorage.removeItem('user_info');
}

export function isAuthenticated(): boolean {
  return !!getAuthToken();
}
