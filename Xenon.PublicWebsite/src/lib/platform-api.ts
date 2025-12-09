/**
 * Platform API Client
 * Connects to Xenon.Platform backend
 * Uses shared API utilities from @xenon/ui
 */

import {
  createApiClient,
  buildQueryString,
  type ApiResponse,
} from '@xenon/ui';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// Create API client instance
const api = createApiClient(API_BASE_URL);

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
  return api.get('/api/public/pricing/plans');
}

export async function getPricingEstimate(
  request: PricingEstimateRequest
): Promise<ApiResponse<PricingEstimate>> {
  const queryString = buildQueryString({
    planId: request.planId,
    branches: request.branches,
    users: request.users,
    billingCycle: request.billingCycle,
    currency: request.currency || 'AED',
  });
  return api.get(`/api/public/pricing/estimate${queryString}`);
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
  return api.post('/api/public/tenants/signup', data);
}

export async function login(
  data: TenantLoginRequest
): Promise<ApiResponse<TenantLoginResponse>> {
  return api.post('/api/public/tenants/login', data);
}

export async function getCurrentTenant(): Promise<ApiResponse<{ tenant: unknown; user: unknown }>> {
  return api.get('/api/public/tenants/me');
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
  return api.post('/api/public/demo-request', data);
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
  return api.post('/api/public/demo-request', {
    ...data,
    source: 'contact-page',
  });
}

// ============================================
// Auth Helpers
// Re-export from shared library with custom token key
// ============================================

const TENANT_TOKEN_KEY = 'tenant_token';
const TENANT_INFO_KEY = 'tenant_info';
const USER_INFO_KEY = 'user_info';

export function setAuthToken(token: string): void {
  localStorage.setItem(TENANT_TOKEN_KEY, token);
}

export function getAuthToken(): string | null {
  return localStorage.getItem(TENANT_TOKEN_KEY);
}

export function clearAuthToken(): void {
  localStorage.removeItem(TENANT_TOKEN_KEY);
  localStorage.removeItem(TENANT_INFO_KEY);
  localStorage.removeItem(USER_INFO_KEY);
}

export function isAuthenticated(): boolean {
  return !!getAuthToken();
}
