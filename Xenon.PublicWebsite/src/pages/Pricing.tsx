import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { Check, ArrowRight, HelpCircle, ChevronDown, RefreshCw } from 'lucide-react';
import { getPricingEstimate, type PricingEstimate } from '@/lib/platform-api';

type BillingCycle = 'monthly' | 'quarterly' | 'yearly';

interface Plan {
  name: string;
  basePrice: number;
  includedBranches: number;
  includedUsers: number;
  features: string[];
  recommended?: boolean;
}

const plans: Record<string, Plan> = {
  STARTER: {
    name: 'Starter',
    basePrice: 499,
    includedBranches: 1,
    includedUsers: 5,
    features: ['Patient management', 'Appointments', 'Basic reporting', 'Email support'],
  },
  GROWTH: {
    name: 'Growth',
    basePrice: 999,
    includedBranches: 3,
    includedUsers: 15,
    recommended: true,
    features: ['All Starter features', 'Multi-branch', 'Advanced analytics', 'Priority support', 'API access'],
  },
  ENTERPRISE: {
    name: 'Enterprise',
    basePrice: 2499,
    includedBranches: 10,
    includedUsers: 50,
    features: ['All Growth features', 'Unlimited branches', 'Custom integrations', 'Dedicated support', 'SLA guarantee'],
  },
};

const discounts: Record<BillingCycle, number> = {
  monthly: 0,
  quarterly: 0.05,
  yearly: 0.15,
};

const billingCycleToApi: Record<BillingCycle, string> = {
  monthly: 'Monthly',
  quarterly: 'Quarterly',
  yearly: 'Annual',
};

export default function PricingPage() {
  const [billingCycle, setBillingCycle] = useState<BillingCycle>('monthly');
  const [selectedPlan, setSelectedPlan] = useState<string>('GROWTH');
  const [branches, setBranches] = useState(1);
  const [users, setUsers] = useState(5);
  const [showCalculator, setShowCalculator] = useState(false);
  const [backendEstimate, setBackendEstimate] = useState<PricingEstimate | null>(null);
  const [isLoadingEstimate, setIsLoadingEstimate] = useState(false);

  const fetchEstimate = useCallback(async () => {
    if (!showCalculator) return;
    setIsLoadingEstimate(true);
    try {
      const response = await getPricingEstimate({
        planId: selectedPlan,
        branches,
        users,
        billingCycle: billingCycleToApi[billingCycle],
        currency: 'AED',
      });
      if (response.success && response.data) {
        setBackendEstimate(response.data);
      }
    } catch {
      // Fall back to local calculation
    } finally {
      setIsLoadingEstimate(false);
    }
  }, [selectedPlan, branches, users, billingCycle, showCalculator]);

  useEffect(() => {
    if (showCalculator) {
      const timer = setTimeout(fetchEstimate, 300);
      return () => clearTimeout(timer);
    }
  }, [fetchEstimate, showCalculator]);

  const getDiscountedPrice = (basePrice: number) => {
    return Math.round(basePrice * (1 - discounts[billingCycle]));
  };

  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Pricing</div>
            <h1 className="heading-1 text-gray-900 mb-6">Simple, transparent pricing</h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              Choose the plan that fits your business. All plans include a 30-day free trial.
            </p>

            {/* Billing toggle */}
            <div className="inline-flex items-center gap-4 p-1 bg-gray-100 rounded-lg">
              {(['monthly', 'quarterly', 'yearly'] as BillingCycle[]).map((cycle) => (
                <button
                  key={cycle}
                  onClick={() => setBillingCycle(cycle)}
                  className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                    billingCycle === cycle
                      ? 'bg-white shadow-sm text-gray-900'
                      : 'text-gray-600 hover:text-gray-900'
                  }`}
                >
                  {cycle.charAt(0).toUpperCase() + cycle.slice(1)}
                  {discounts[cycle] > 0 && (
                    <span className="ml-1 text-xs text-primary-600">-{discounts[cycle] * 100}%</span>
                  )}
                </button>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Pricing Cards */}
      <section className="pb-16 bg-white">
        <div className="container-marketing">
          <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
            {Object.entries(plans).map(
              ([code, plan]) => {
                const discountedPrice = getDiscountedPrice(plan.basePrice);
                return (
                  <div
                    key={code}
                    className={`card relative ${
                      plan.recommended ? 'border-primary-500 ring-2 ring-primary-500' : ''
                    }`}
                  >
                    {plan.recommended && (
                      <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                        <span className="badge bg-primary-600 text-white px-3 py-1">Recommended</span>
                      </div>
                    )}
                    <div className="text-center mb-6">
                      <h3 className="text-xl font-bold text-gray-900 mb-2">{plan.name}</h3>
                      <div className="flex items-baseline justify-center gap-1">
                        <span className="text-4xl font-bold text-gray-900">{discountedPrice}</span>
                        <span className="text-gray-500">AED/mo</span>
                      </div>
                      {billingCycle !== 'monthly' && (
                        <p className="text-sm text-gray-500 mt-1">
                          <span className="line-through">{plan.basePrice} AED</span>
                          <span className="text-primary-600 ml-2">
                            Save {discounts[billingCycle] * 100}%
                          </span>
                        </p>
                      )}
                    </div>
                    <div className="space-y-3 mb-6 text-sm text-gray-600">
                      <div>
                        <span className="font-medium text-gray-900">{plan.includedBranches}</span>{' '}
                        {plan.includedBranches === 1 ? 'branch' : 'branches'} included
                      </div>
                      <div>
                        <span className="font-medium text-gray-900">{plan.includedUsers}</span> users
                        included
                      </div>
                    </div>
                    <div className="border-t pt-6 mb-6">
                      <ul className="space-y-3">
                        {plan.features.map((feature) => (
                          <li key={feature} className="flex items-start gap-2 text-sm">
                            <Check className="h-4 w-4 text-primary-600 mt-0.5 flex-shrink-0" />
                            <span className="text-gray-600">{feature}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                    <Link
                      to="/demo"
                      className={`w-full ${plan.recommended ? 'btn-primary' : 'btn-secondary'}`}
                    >
                      Start Free Trial
                    </Link>
                  </div>
                );
              }
            )}
          </div>
        </div>
      </section>

      {/* Calculator */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="max-w-4xl mx-auto">
            <button
              onClick={() => setShowCalculator(!showCalculator)}
              className="w-full flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200 mb-4"
            >
              <div className="text-left">
                <h3 className="font-semibold text-gray-900">Pricing Calculator</h3>
                <p className="text-sm text-gray-600">Calculate your exact monthly cost</p>
              </div>
              <ChevronDown
                className={`h-5 w-5 text-gray-400 transition-transform ${
                  showCalculator ? 'rotate-180' : ''
                }`}
              />
            </button>

            {showCalculator && (
              <div className="card">
                <div className="grid md:grid-cols-2 gap-8">
                  <div className="space-y-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">Plan</label>
                      <div className="grid grid-cols-3 gap-2">
                        {Object.keys(plans).map((planKey) => (
                          <button
                            key={planKey}
                            onClick={() => setSelectedPlan(planKey)}
                            className={`py-2 px-3 rounded-lg text-sm font-medium transition-colors ${
                              selectedPlan === planKey
                                ? 'bg-primary-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {plans[planKey].name}
                          </button>
                        ))}
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Branches: {branches}
                      </label>
                      <input
                        type="range"
                        min="1"
                        max="20"
                        value={branches}
                        onChange={(e) => setBranches(Number(e.target.value))}
                        className="w-full"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Users: {users}
                      </label>
                      <input
                        type="range"
                        min="1"
                        max="100"
                        value={users}
                        onChange={(e) => setUsers(Number(e.target.value))}
                        className="w-full"
                      />
                    </div>
                  </div>
                  <div className="bg-gray-50 rounded-lg p-6">
                    <div className="flex items-center justify-between mb-4">
                      <h4 className="font-semibold text-gray-900">Cost Summary</h4>
                      {isLoadingEstimate && <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />}
                    </div>
                    {backendEstimate ? (
                      <div className="space-y-3">
                        <div className="flex justify-between text-sm">
                          <span className="text-gray-600">Base Plan</span>
                          <span>{backendEstimate.basePrice} AED</span>
                        </div>
                        {backendEstimate.extraBranchesPrice > 0 && (
                          <div className="flex justify-between text-sm">
                            <span className="text-gray-600">Extra Branches</span>
                            <span>{backendEstimate.extraBranchesPrice} AED</span>
                          </div>
                        )}
                        {backendEstimate.extraUsersPrice > 0 && (
                          <div className="flex justify-between text-sm">
                            <span className="text-gray-600">Extra Users</span>
                            <span>{backendEstimate.extraUsersPrice} AED</span>
                          </div>
                        )}
                        {backendEstimate.discountAmount > 0 && (
                          <div className="flex justify-between text-sm text-primary-600">
                            <span>Discount ({backendEstimate.discountPercent}%)</span>
                            <span>-{backendEstimate.discountAmount} AED</span>
                          </div>
                        )}
                        <div className="border-t pt-3">
                          <div className="flex justify-between">
                            <span className="font-semibold">Total</span>
                            <span className="text-2xl font-bold text-primary-600">
                              {backendEstimate.total} AED
                            </span>
                          </div>
                        </div>
                      </div>
                    ) : (
                      <p className="text-sm text-gray-500">Loading estimate...</p>
                    )}
                    <Link to="/demo" className="btn-primary w-full mt-6">
                      Start Free Trial
                      <ArrowRight className="ml-2 h-5 w-5" />
                    </Link>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </section>

      {/* FAQ */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto">
            <h2 className="heading-2 text-gray-900 mb-8 text-center">Frequently Asked Questions</h2>
            <div className="space-y-4">
              {[
                {
                  q: 'Is there a free trial?',
                  a: 'Yes! All plans include a 30-day free trial. No credit card required.',
                },
                {
                  q: 'Can I change plans later?',
                  a: 'Absolutely. You can upgrade or downgrade at any time.',
                },
                {
                  q: 'What payment methods do you accept?',
                  a: 'We accept all major credit cards and bank transfers.',
                },
              ].map((faq, i) => (
                <div key={i} className="card">
                  <h3 className="font-semibold text-gray-900 mb-2 flex items-start gap-2">
                    <HelpCircle className="h-5 w-5 text-primary-600 flex-shrink-0 mt-0.5" />
                    {faq.q}
                  </h3>
                  <p className="text-gray-600 text-sm pl-7">{faq.a}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
    </>
  );
}
