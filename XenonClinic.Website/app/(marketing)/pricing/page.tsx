'use client';

import React, { useState, useEffect, useCallback } from 'react';
import Link from 'next/link';
import {
  PRICING_TIERS,
  PRICING_ADDONS,
  DURATION_DISCOUNTS,
  calculatePricing,
} from '@/lib/pricing';
import { getPricingEstimate, type PricingEstimate } from '@/lib/platform-api';
import type { PlanCode, Currency } from '@/types';
import { Check, ArrowRight, HelpCircle, ChevronDown, RefreshCw } from 'lucide-react';

type BillingCycle = 'monthly' | 'quarterly' | 'yearly';

const billingCycleToMonths: Record<BillingCycle, 1 | 3 | 6 | 12> = {
  monthly: 1,
  quarterly: 3,
  yearly: 12,
};

const billingCycleToApi: Record<BillingCycle, string> = {
  monthly: 'Monthly',
  quarterly: 'Quarterly',
  yearly: 'Annual',
};

export default function PricingPage() {
  const [billingCycle, setBillingCycle] = useState<BillingCycle>('monthly');
  const [selectedPlan, setSelectedPlan] = useState<PlanCode>('GROWTH');
  const [branches, setBranches] = useState(1);
  const [users, setUsers] = useState(5);
  const [selectedAddOns, setSelectedAddOns] = useState<string[]>([]);
  const [showCalculator, setShowCalculator] = useState(false);

  // Backend pricing state
  const [backendEstimate, setBackendEstimate] = useState<PricingEstimate | null>(null);
  const [isLoadingEstimate, setIsLoadingEstimate] = useState(false);
  const [useBackend, setUseBackend] = useState(true);

  // Local fallback pricing calculation
  const localPricing = calculatePricing({
    plan: selectedPlan,
    durationMonths: billingCycleToMonths[billingCycle],
    branches,
    users,
    addOns: selectedAddOns,
    currency: 'AED' as Currency,
  });

  // Fetch pricing estimate from backend
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
        setUseBackend(true);
      } else {
        // Fall back to local calculation
        setUseBackend(false);
      }
    } catch {
      // Fall back to local calculation on error
      setUseBackend(false);
    } finally {
      setIsLoadingEstimate(false);
    }
  }, [selectedPlan, branches, users, billingCycle, showCalculator]);

  // Fetch estimate when calculator is opened or parameters change
  useEffect(() => {
    if (showCalculator) {
      const debounceTimer = setTimeout(() => {
        fetchEstimate();
      }, 300);
      return () => clearTimeout(debounceTimer);
    }
  }, [fetchEstimate, showCalculator]);

  // Get pricing values (prefer backend, fallback to local)
  const pricing = useBackend && backendEstimate
    ? {
        basePrice: backendEstimate.basePrice,
        extraBranchesPrice: backendEstimate.extraBranchesPrice,
        extraUsersPrice: backendEstimate.extraUsersPrice,
        addOnsPrice: 0, // Backend doesn't include add-ons yet, use local
        discount: backendEstimate.discountAmount,
        discountPercent: backendEstimate.discountPercent,
        total: backendEstimate.total,
        monthlyEquivalent: backendEstimate.monthlyEquivalent,
      }
    : localPricing;

  // Add local add-ons price if using backend
  const addOnsPrice = selectedAddOns.reduce((sum, code) => {
    const addon = PRICING_ADDONS.find(a => a.code === code);
    return sum + (addon?.price.monthly || 0);
  }, 0) * billingCycleToMonths[billingCycle];

  const finalTotal = pricing.total + addOnsPrice;
  const finalMonthlyEquivalent = pricing.monthlyEquivalent + (addOnsPrice / billingCycleToMonths[billingCycle]);

  const toggleAddOn = (addOnCode: string) => {
    setSelectedAddOns((prev) =>
      prev.includes(addOnCode)
        ? prev.filter((code) => code !== addOnCode)
        : [...prev, addOnCode]
    );
  };

  const getDiscountPercent = (cycle: BillingCycle): number => {
    return DURATION_DISCOUNTS[billingCycleToMonths[cycle]] || 0;
  };

  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Pricing</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Simple, transparent pricing
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              Choose the plan that fits your business. All plans include a 30-day free trial.
              No credit card required to start.
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
                  {cycle !== 'monthly' && (
                    <span className="ml-1 text-xs text-primary-600">
                      -{getDiscountPercent(cycle) * 100}%
                    </span>
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
            {(Object.entries(PRICING_TIERS) as [PlanCode, typeof PRICING_TIERS[PlanCode]][]).map(
              ([code, tier]) => {
                const isRecommended = tier.recommended;
                const monthlyPrice = tier.basePrice.monthly;
                const discountedPrice = Math.round(monthlyPrice * (1 - getDiscountPercent(billingCycle)));

                return (
                  <div
                    key={code}
                    className={`card relative ${
                      isRecommended
                        ? 'border-primary-500 ring-2 ring-primary-500'
                        : 'border-gray-200'
                    }`}
                  >
                    {isRecommended && (
                      <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                        <span className="badge bg-primary-600 text-white px-3 py-1">
                          Recommended
                        </span>
                      </div>
                    )}

                    <div className="text-center mb-6">
                      <h3 className="text-xl font-bold text-gray-900 mb-2">
                        {tier.name}
                      </h3>
                      <p className="text-sm text-gray-600 mb-4">
                        {tier.description}
                      </p>
                      <div className="flex items-baseline justify-center gap-1">
                        <span className="text-4xl font-bold text-gray-900">
                          {discountedPrice} AED
                        </span>
                        <span className="text-gray-500">/mo</span>
                      </div>
                      {billingCycle !== 'monthly' && (
                        <p className="text-sm text-gray-500 mt-1">
                          <span className="line-through">{monthlyPrice} AED</span>
                          <span className="text-primary-600 ml-2">
                            Save {getDiscountPercent(billingCycle) * 100}%
                          </span>
                        </p>
                      )}
                    </div>

                    <div className="space-y-3 mb-6">
                      <div className="text-sm text-gray-600">
                        <span className="font-medium text-gray-900">
                          {tier.includedBranches}
                        </span>{' '}
                        {tier.includedBranches === 1 ? 'branch' : 'branches'} included
                      </div>
                      <div className="text-sm text-gray-600">
                        <span className="font-medium text-gray-900">
                          {tier.includedUsers}
                        </span>{' '}
                        users included
                      </div>
                    </div>

                    <div className="border-t pt-6 mb-6">
                      <ul className="space-y-3">
                        {tier.features.slice(0, 6).map((feature) => (
                          <li key={feature} className="flex items-start gap-2 text-sm">
                            <Check className="h-4 w-4 text-primary-600 mt-0.5 flex-shrink-0" />
                            <span className="text-gray-600">{feature}</span>
                          </li>
                        ))}
                        {tier.features.length > 6 && (
                          <li className="text-sm text-gray-500">
                            +{tier.features.length - 6} more features
                          </li>
                        )}
                      </ul>
                    </div>

                    <Link
                      href="/demo"
                      className={`w-full ${
                        isRecommended ? 'btn-primary' : 'btn-secondary'
                      }`}
                    >
                      Start Free Trial
                    </Link>
                  </div>
                );
              }
            )}
          </div>

          {/* Enterprise */}
          <div className="mt-12 max-w-3xl mx-auto">
            <div className="card bg-gray-50 text-center">
              <h3 className="text-xl font-bold text-gray-900 mb-2">
                Need a custom solution?
              </h3>
              <p className="text-gray-600 mb-4">
                Contact us for custom pricing, dedicated support, and on-premises deployment options.
              </p>
              <Link href="/contact" className="btn-primary">
                Contact Sales
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Pricing Calculator */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="max-w-4xl mx-auto">
            <button
              onClick={() => setShowCalculator(!showCalculator)}
              className="w-full flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200 mb-4"
            >
              <div className="text-left">
                <h3 className="font-semibold text-gray-900">Pricing Calculator</h3>
                <p className="text-sm text-gray-600">
                  Calculate your exact monthly cost based on your needs
                </p>
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
                  {/* Configuration */}
                  <div className="space-y-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Plan
                      </label>
                      <div className="grid grid-cols-3 gap-2">
                        {(Object.keys(PRICING_TIERS) as PlanCode[]).map((plan) => (
                          <button
                            key={plan}
                            onClick={() => setSelectedPlan(plan)}
                            className={`py-2 px-3 rounded-lg text-sm font-medium transition-colors ${
                              selectedPlan === plan
                                ? 'bg-primary-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {PRICING_TIERS[plan].name}
                          </button>
                        ))}
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Billing Cycle
                      </label>
                      <div className="grid grid-cols-3 gap-2">
                        {(['monthly', 'quarterly', 'yearly'] as BillingCycle[]).map((cycle) => (
                          <button
                            key={cycle}
                            onClick={() => setBillingCycle(cycle)}
                            className={`py-2 px-3 rounded-lg text-sm font-medium transition-colors ${
                              billingCycle === cycle
                                ? 'bg-primary-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {cycle.charAt(0).toUpperCase() + cycle.slice(1)}
                          </button>
                        ))}
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Number of Branches: {branches}
                      </label>
                      <input
                        type="range"
                        min="1"
                        max="20"
                        value={branches}
                        onChange={(e) => setBranches(Number(e.target.value))}
                        className="w-full"
                      />
                      <div className="flex justify-between text-xs text-gray-500 mt-1">
                        <span>1</span>
                        <span>20</span>
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Number of Users: {users}
                      </label>
                      <input
                        type="range"
                        min="1"
                        max="100"
                        value={users}
                        onChange={(e) => setUsers(Number(e.target.value))}
                        className="w-full"
                      />
                      <div className="flex justify-between text-xs text-gray-500 mt-1">
                        <span>1</span>
                        <span>100</span>
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Add-ons
                      </label>
                      <div className="space-y-2">
                        {PRICING_ADDONS.map((addOn) => (
                          <label
                            key={addOn.code}
                            className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg cursor-pointer hover:bg-gray-100"
                          >
                            <input
                              type="checkbox"
                              checked={selectedAddOns.includes(addOn.code)}
                              onChange={() => toggleAddOn(addOn.code)}
                              className="h-4 w-4 text-primary-600 rounded"
                            />
                            <div className="flex-1">
                              <div className="text-sm font-medium text-gray-900">
                                {addOn.name}
                              </div>
                              <div className="text-xs text-gray-500">
                                {addOn.description}
                              </div>
                            </div>
                            <div className="text-sm font-medium text-gray-900">
                              {addOn.price.monthly} AED/mo
                            </div>
                          </label>
                        ))}
                      </div>
                    </div>
                  </div>

                  {/* Summary */}
                  <div className="bg-gray-50 rounded-lg p-6">
                    <div className="flex items-center justify-between mb-4">
                      <h4 className="font-semibold text-gray-900">
                        Cost Summary
                      </h4>
                      {isLoadingEstimate && (
                        <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                      )}
                    </div>
                    <div className="space-y-3">
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Base Plan ({PRICING_TIERS[selectedPlan].name})</span>
                        <span className="text-gray-900">{Math.round(pricing.basePrice)} AED</span>
                      </div>

                      {pricing.extraBranchesPrice > 0 && (
                        <div className="flex justify-between text-sm">
                          <span className="text-gray-600">
                            Extra Branches ({branches - PRICING_TIERS[selectedPlan].includedBranches})
                          </span>
                          <span className="text-gray-900">{Math.round(pricing.extraBranchesPrice)} AED</span>
                        </div>
                      )}

                      {pricing.extraUsersPrice > 0 && (
                        <div className="flex justify-between text-sm">
                          <span className="text-gray-600">
                            Extra Users ({users - PRICING_TIERS[selectedPlan].includedUsers})
                          </span>
                          <span className="text-gray-900">{Math.round(pricing.extraUsersPrice)} AED</span>
                        </div>
                      )}

                      {addOnsPrice > 0 && (
                        <div className="flex justify-between text-sm">
                          <span className="text-gray-600">Add-ons</span>
                          <span className="text-gray-900">{Math.round(addOnsPrice)} AED</span>
                        </div>
                      )}

                      {pricing.discount > 0 && (
                        <div className="flex justify-between text-sm text-primary-600">
                          <span>Duration Discount ({pricing.discountPercent}%)</span>
                          <span>-{Math.round(pricing.discount)} AED</span>
                        </div>
                      )}

                      <div className="border-t pt-3 mt-3">
                        <div className="flex justify-between">
                          <span className="font-semibold text-gray-900">
                            {billingCycle === 'monthly' ? 'Monthly' : billingCycle === 'quarterly' ? '3-Month' : 'Annual'} Total
                          </span>
                          <span className="text-2xl font-bold text-primary-600">
                            {Math.round(finalTotal)} AED
                          </span>
                        </div>
                        <div className="text-right text-sm text-gray-500 mt-1">
                          ~{Math.round(finalMonthlyEquivalent)} AED/month
                        </div>
                      </div>
                    </div>

                    <Link href="/demo" className="btn-primary w-full mt-6">
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
            <h2 className="heading-2 text-gray-900 mb-8 text-center">
              Frequently Asked Questions
            </h2>
            <div className="space-y-4">
              {[
                {
                  q: 'Is there a free trial?',
                  a: 'Yes! All plans include a 30-day free trial. No credit card required to start. You can explore all features before making a commitment.',
                },
                {
                  q: 'Can I change plans later?',
                  a: 'Absolutely. You can upgrade or downgrade your plan at any time. Changes take effect immediately, and we prorate any differences.',
                },
                {
                  q: 'What payment methods do you accept?',
                  a: 'We accept all major credit cards, bank transfers, and local payment methods popular in the Gulf region.',
                },
                {
                  q: 'Is there a setup fee?',
                  a: 'No setup fees for cloud-hosted plans. For on-premises deployments or custom implementations, please contact our sales team.',
                },
                {
                  q: 'What happens after the trial ends?',
                  a: 'At the end of your trial, you can choose a paid plan to continue. Your data is preserved. If you decide not to continue, your data is retained for 30 days before deletion.',
                },
                {
                  q: 'Do you offer discounts for annual billing?',
                  a: 'Yes! Pay quarterly for 5% off, or annually for 15% off your monthly rate.',
                },
              ].map((faq, index) => (
                <div key={index} className="card">
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

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            Start your free trial today
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Join hundreds of Gulf businesses already using XENON. No credit card required.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/contact" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              Contact Sales
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
