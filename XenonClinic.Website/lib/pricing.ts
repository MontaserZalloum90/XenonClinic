import type {
  PlanCode,
  PricingTier,
  PricingAddOn,
  PricingCalculatorInput,
  PricingCalculatorResult,
  Currency,
} from '@/types';

// ============================================
// Pricing Configuration
// ============================================

export const PRICING_TIERS: Record<PlanCode, PricingTier> = {
  STARTER: {
    code: 'STARTER',
    name: 'Starter',
    description: 'Perfect for small clinics or trading companies just getting started',
    basePrice: {
      monthly: 499,  // AED
      annual: 4490,  // ~10% discount
    },
    includedBranches: 1,
    includedUsers: 5,
    extraBranchPrice: 199,
    extraUserPrice: 49,
    features: [
      'Patient/Customer Management',
      'Appointments & Scheduling',
      'Basic Billing & Invoicing',
      'Inventory Tracking',
      'Standard Reports',
      'Email Support',
      '99.5% Uptime SLA',
    ],
    supportLevel: 'Email support (48h response)',
  },
  GROWTH: {
    code: 'GROWTH',
    name: 'Growth',
    description: 'For growing practices with multiple locations and staff',
    basePrice: {
      monthly: 999,
      annual: 8990,
    },
    includedBranches: 3,
    includedUsers: 15,
    extraBranchPrice: 149,
    extraUserPrice: 39,
    features: [
      'Everything in Starter',
      'Multi-Branch Management',
      'Advanced RBAC',
      'Laboratory Module',
      'Analytics Dashboard',
      'Audit Trail',
      'API Access',
      'Priority Email & Chat Support',
      '99.9% Uptime SLA',
    ],
    supportLevel: 'Priority support (24h response)',
    recommended: true,
  },
  ENTERPRISE: {
    code: 'ENTERPRISE',
    name: 'Enterprise',
    description: 'For large organizations requiring advanced features and dedicated support',
    basePrice: {
      monthly: 2499,
      annual: 22490,
    },
    includedBranches: 10,
    includedUsers: 50,
    extraBranchPrice: 99,
    extraUserPrice: 29,
    features: [
      'Everything in Growth',
      'Unlimited Branches',
      'Custom Integrations',
      'SSO/SAML Support',
      'Advanced Analytics',
      'Custom Reports',
      'Dedicated Account Manager',
      'On-Prem Deployment Option',
      'Phone & Priority Support',
      '99.95% Uptime SLA',
      'Custom SLA Available',
    ],
    supportLevel: 'Dedicated support (4h response)',
  },
};

export const PRICING_ADDONS: PricingAddOn[] = [
  {
    code: 'ANALYTICS_PRO',
    name: 'Analytics Pro',
    description: 'Advanced analytics with custom dashboards, forecasting, and data export',
    price: { monthly: 199, annual: 1790 },
  },
  {
    code: 'INTEGRATIONS_PACK',
    name: 'Integrations Pack',
    description: 'Connect with payment gateways, SMS providers, and third-party systems',
    price: { monthly: 149, annual: 1340 },
  },
  {
    code: 'ADVANCED_AUDIT',
    name: 'Advanced Audit',
    description: 'Extended audit trail with compliance reporting and data retention',
    price: { monthly: 99, annual: 890 },
  },
  {
    code: 'SSO_ADDON',
    name: 'SSO Add-on',
    description: 'Single Sign-On with SAML 2.0 and Azure AD integration',
    price: { monthly: 249, annual: 2240 },
  },
];

// ============================================
// On-Prem Pricing
// ============================================

export const ON_PREM_PRICING = {
  baseSubscription: {
    monthly: 1999,
    annual: 19990,
  },
  perBranch: {
    monthly: 299,
    annual: 2690,
  },
  perUser: {
    monthly: 59,
    annual: 530,
  },
  implementationFee: {
    small: 9999,    // Up to 3 branches
    medium: 24999,  // 4-10 branches
    large: 49999,   // 10+ branches (custom quote)
  },
  supportContract: {
    standard: { monthly: 499, annual: 4490 },
    premium: { monthly: 999, annual: 8990 },
  },
};

// ============================================
// Currency Conversion
// ============================================

export const CURRENCY_RATES: Record<Currency, number> = {
  AED: 1,
  USD: 0.27, // Approximate AED to USD
};

export const CURRENCY_SYMBOLS: Record<Currency, string> = {
  AED: 'AED',
  USD: '$',
};

// ============================================
// Duration Discounts
// ============================================

export const DURATION_DISCOUNTS: Record<number, number> = {
  1: 0,      // No discount for monthly
  3: 0.05,   // 5% discount for quarterly
  6: 0.10,   // 10% discount for semi-annual
  12: 0.15,  // 15% discount for annual
};

// ============================================
// Pricing Calculator
// ============================================

export function calculatePricing(input: PricingCalculatorInput): PricingCalculatorResult {
  const tier = PRICING_TIERS[input.plan];
  const currencyRate = CURRENCY_RATES[input.currency];
  const discount = DURATION_DISCOUNTS[input.durationMonths] || 0;

  // Base price calculation
  const monthlyBase = tier.basePrice.monthly;

  // Extra branches calculation
  const extraBranches = Math.max(0, input.branches - tier.includedBranches);
  const extraBranchesMonthly = extraBranches * tier.extraBranchPrice;

  // Extra users calculation
  const extraUsers = Math.max(0, input.users - tier.includedUsers);
  const extraUsersMonthly = extraUsers * tier.extraUserPrice;

  // Add-ons calculation
  let addOnsMonthly = 0;
  const addOnDetails: { label: string; amount: number }[] = [];

  for (const addOnCode of input.addOns) {
    const addOn = PRICING_ADDONS.find(a => a.code === addOnCode);
    if (addOn) {
      addOnsMonthly += addOn.price.monthly;
      addOnDetails.push({
        label: addOn.name,
        amount: addOn.price.monthly * input.durationMonths * currencyRate,
      });
    }
  }

  // Subtotal (monthly)
  const monthlySubtotal = monthlyBase + extraBranchesMonthly + extraUsersMonthly + addOnsMonthly;

  // Total for duration
  const subtotalForDuration = monthlySubtotal * input.durationMonths;
  const discountAmount = subtotalForDuration * discount;
  const total = (subtotalForDuration - discountAmount) * currencyRate;

  // Build breakdown
  const breakdown: { label: string; amount: number }[] = [
    { label: `${tier.name} Plan (${input.durationMonths} mo)`, amount: monthlyBase * input.durationMonths * currencyRate },
  ];

  if (extraBranches > 0) {
    breakdown.push({
      label: `Extra Branches (${extraBranches} x ${input.durationMonths} mo)`,
      amount: extraBranchesMonthly * input.durationMonths * currencyRate,
    });
  }

  if (extraUsers > 0) {
    breakdown.push({
      label: `Extra Users (${extraUsers} x ${input.durationMonths} mo)`,
      amount: extraUsersMonthly * input.durationMonths * currencyRate,
    });
  }

  breakdown.push(...addOnDetails);

  if (discountAmount > 0) {
    breakdown.push({
      label: `Duration Discount (${discount * 100}%)`,
      amount: -discountAmount * currencyRate,
    });
  }

  return {
    plan: input.plan,
    basePrice: monthlyBase * input.durationMonths * currencyRate,
    extraBranchesPrice: extraBranchesMonthly * input.durationMonths * currencyRate,
    extraUsersPrice: extraUsersMonthly * input.durationMonths * currencyRate,
    addOnsPrice: addOnsMonthly * input.durationMonths * currencyRate,
    subtotal: subtotalForDuration * currencyRate,
    discount: discountAmount * currencyRate,
    discountPercent: discount * 100,
    monthlyEquivalent: (total / input.durationMonths),
    total,
    currency: input.currency,
    breakdown,
  };
}

// ============================================
// Format Helpers
// ============================================

export function formatPrice(amount: number, currency: Currency): string {
  const symbol = CURRENCY_SYMBOLS[currency];
  const formatted = amount.toLocaleString('en-AE', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  });

  if (currency === 'USD') {
    return `${symbol}${formatted}`;
  }
  return `${formatted} ${symbol}`;
}

export function formatPriceWithDecimals(amount: number, currency: Currency): string {
  const symbol = CURRENCY_SYMBOLS[currency];
  const formatted = amount.toLocaleString('en-AE', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  if (currency === 'USD') {
    return `${symbol}${formatted}`;
  }
  return `${formatted} ${symbol}`;
}
