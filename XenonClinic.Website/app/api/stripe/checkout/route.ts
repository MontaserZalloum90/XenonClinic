import { NextRequest, NextResponse } from 'next/server';
import { createCheckoutSession } from '@/lib/stripe';
import { PlanCode, BillingCycle } from '@/lib/pricing';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    const {
      tenantId,
      plan,
      billingCycle,
      branches,
      users,
      addOns,
      customerEmail,
    } = body;

    if (!tenantId || !plan || !billingCycle) {
      return NextResponse.json(
        { error: 'Missing required fields: tenantId, plan, billingCycle' },
        { status: 400 }
      );
    }

    // Validate plan
    if (!['STARTER', 'GROWTH', 'ENTERPRISE'].includes(plan)) {
      return NextResponse.json(
        { error: 'Invalid plan' },
        { status: 400 }
      );
    }

    // Validate billing cycle
    if (!['monthly', 'quarterly', 'yearly'].includes(billingCycle)) {
      return NextResponse.json(
        { error: 'Invalid billing cycle' },
        { status: 400 }
      );
    }

    const baseUrl = process.env.NEXT_PUBLIC_BASE_URL || 'http://localhost:3000';

    const session = await createCheckoutSession({
      tenantId,
      plan: plan as PlanCode,
      billingCycle: billingCycle as BillingCycle,
      branches,
      users,
      addOns,
      customerEmail,
      successUrl: `${baseUrl}/dashboard/billing/success?session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${baseUrl}/dashboard/billing?cancelled=true`,
    });

    if (!session) {
      return NextResponse.json(
        { error: 'Failed to create checkout session' },
        { status: 500 }
      );
    }

    return NextResponse.json({ url: session.url });
  } catch (error) {
    console.error('Checkout error:', error);
    return NextResponse.json(
      { error: 'Failed to create checkout session' },
      { status: 500 }
    );
  }
}
