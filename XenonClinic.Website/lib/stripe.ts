import Stripe from 'stripe';
import { PlanCode, BillingCycle, PRICING_TIERS, DURATION_DISCOUNTS, calculatePricing } from './pricing';

// Initialize Stripe
const stripeSecretKey = process.env.STRIPE_SECRET_KEY;
if (!stripeSecretKey) {
  console.warn('STRIPE_SECRET_KEY is not set. Stripe functionality will be limited.');
}

export const stripe = stripeSecretKey
  ? new Stripe(stripeSecretKey, {
      apiVersion: '2023-10-16',
      typescript: true,
    })
  : null;

// Price IDs mapped from Stripe Dashboard
// These would be created in Stripe and stored here
export const STRIPE_PRICE_IDS: Record<PlanCode, Record<BillingCycle, string>> = {
  STARTER: {
    monthly: process.env.STRIPE_PRICE_STARTER_MONTHLY || '',
    quarterly: process.env.STRIPE_PRICE_STARTER_QUARTERLY || '',
    yearly: process.env.STRIPE_PRICE_STARTER_YEARLY || '',
  },
  GROWTH: {
    monthly: process.env.STRIPE_PRICE_GROWTH_MONTHLY || '',
    quarterly: process.env.STRIPE_PRICE_GROWTH_QUARTERLY || '',
    yearly: process.env.STRIPE_PRICE_GROWTH_YEARLY || '',
  },
  ENTERPRISE: {
    monthly: process.env.STRIPE_PRICE_ENTERPRISE_MONTHLY || '',
    quarterly: process.env.STRIPE_PRICE_ENTERPRISE_QUARTERLY || '',
    yearly: process.env.STRIPE_PRICE_ENTERPRISE_YEARLY || '',
  },
};

// Metered pricing for extra branches and users
export const STRIPE_METERED_PRICES = {
  extraBranch: process.env.STRIPE_PRICE_EXTRA_BRANCH || '',
  extraUser: process.env.STRIPE_PRICE_EXTRA_USER || '',
};

/**
 * Create a Stripe Checkout session for a new subscription
 */
export async function createCheckoutSession(params: {
  tenantId: string;
  plan: PlanCode;
  billingCycle: BillingCycle;
  branches?: number;
  users?: number;
  addOns?: string[];
  successUrl: string;
  cancelUrl: string;
  customerEmail?: string;
}): Promise<Stripe.Checkout.Session | null> {
  if (!stripe) {
    console.error('Stripe is not initialized');
    return null;
  }

  const tier = PRICING_TIERS[params.plan];
  const priceId = STRIPE_PRICE_IDS[params.plan][params.billingCycle];

  if (!priceId) {
    console.error(`No price ID found for ${params.plan} ${params.billingCycle}`);
    return null;
  }

  const lineItems: Stripe.Checkout.SessionCreateParams.LineItem[] = [
    {
      price: priceId,
      quantity: 1,
    },
  ];

  // Add extra branches if needed
  const extraBranches = (params.branches || 1) - tier.includedBranches;
  if (extraBranches > 0 && STRIPE_METERED_PRICES.extraBranch) {
    lineItems.push({
      price: STRIPE_METERED_PRICES.extraBranch,
      quantity: extraBranches,
    });
  }

  // Add extra users if needed
  const extraUsers = (params.users || tier.includedUsers) - tier.includedUsers;
  if (extraUsers > 0 && STRIPE_METERED_PRICES.extraUser) {
    lineItems.push({
      price: STRIPE_METERED_PRICES.extraUser,
      quantity: extraUsers,
    });
  }

  try {
    const session = await stripe.checkout.sessions.create({
      mode: 'subscription',
      line_items: lineItems,
      success_url: params.successUrl,
      cancel_url: params.cancelUrl,
      customer_email: params.customerEmail,
      metadata: {
        tenantId: params.tenantId,
        plan: params.plan,
        billingCycle: params.billingCycle,
        branches: String(params.branches || 1),
        users: String(params.users || tier.includedUsers),
      },
      subscription_data: {
        metadata: {
          tenantId: params.tenantId,
          plan: params.plan,
        },
      },
      allow_promotion_codes: true,
      billing_address_collection: 'required',
      tax_id_collection: {
        enabled: true,
      },
    });

    return session;
  } catch (error) {
    console.error('Error creating checkout session:', error);
    throw error;
  }
}

/**
 * Create a Stripe Customer Portal session
 */
export async function createCustomerPortalSession(params: {
  customerId: string;
  returnUrl: string;
}): Promise<Stripe.BillingPortal.Session | null> {
  if (!stripe) {
    console.error('Stripe is not initialized');
    return null;
  }

  try {
    const session = await stripe.billingPortal.sessions.create({
      customer: params.customerId,
      return_url: params.returnUrl,
    });

    return session;
  } catch (error) {
    console.error('Error creating customer portal session:', error);
    throw error;
  }
}

/**
 * Retrieve subscription details
 */
export async function getSubscription(subscriptionId: string): Promise<Stripe.Subscription | null> {
  if (!stripe) {
    console.error('Stripe is not initialized');
    return null;
  }

  try {
    const subscription = await stripe.subscriptions.retrieve(subscriptionId);
    return subscription;
  } catch (error) {
    console.error('Error retrieving subscription:', error);
    return null;
  }
}

/**
 * Cancel a subscription
 */
export async function cancelSubscription(subscriptionId: string, immediately = false): Promise<Stripe.Subscription | null> {
  if (!stripe) {
    console.error('Stripe is not initialized');
    return null;
  }

  try {
    if (immediately) {
      const subscription = await stripe.subscriptions.cancel(subscriptionId);
      return subscription;
    } else {
      const subscription = await stripe.subscriptions.update(subscriptionId, {
        cancel_at_period_end: true,
      });
      return subscription;
    }
  } catch (error) {
    console.error('Error canceling subscription:', error);
    throw error;
  }
}

/**
 * Update subscription (upgrade/downgrade)
 */
export async function updateSubscription(params: {
  subscriptionId: string;
  plan: PlanCode;
  billingCycle: BillingCycle;
}): Promise<Stripe.Subscription | null> {
  if (!stripe) {
    console.error('Stripe is not initialized');
    return null;
  }

  const priceId = STRIPE_PRICE_IDS[params.plan][params.billingCycle];
  if (!priceId) {
    console.error(`No price ID found for ${params.plan} ${params.billingCycle}`);
    return null;
  }

  try {
    const subscription = await stripe.subscriptions.retrieve(params.subscriptionId);

    const updatedSubscription = await stripe.subscriptions.update(params.subscriptionId, {
      items: [
        {
          id: subscription.items.data[0].id,
          price: priceId,
        },
      ],
      proration_behavior: 'create_prorations',
      metadata: {
        plan: params.plan,
      },
    });

    return updatedSubscription;
  } catch (error) {
    console.error('Error updating subscription:', error);
    throw error;
  }
}

/**
 * Verify Stripe webhook signature
 */
export function verifyWebhookSignature(
  payload: string | Buffer,
  signature: string
): Stripe.Event | null {
  if (!stripe) {
    console.error('Stripe is not initialized');
    return null;
  }

  const webhookSecret = process.env.STRIPE_WEBHOOK_SECRET;
  if (!webhookSecret) {
    console.error('STRIPE_WEBHOOK_SECRET is not set');
    return null;
  }

  try {
    const event = stripe.webhooks.constructEvent(payload, signature, webhookSecret);
    return event;
  } catch (error) {
    console.error('Error verifying webhook signature:', error);
    return null;
  }
}

/**
 * Handle Stripe webhook events
 */
export async function handleWebhookEvent(event: Stripe.Event): Promise<{ success: boolean; message: string }> {
  switch (event.type) {
    case 'checkout.session.completed': {
      const session = event.data.object as Stripe.Checkout.Session;
      // Activate the tenant's subscription
      console.log('Checkout completed:', session.metadata);
      // TODO: Update tenant status in database
      return { success: true, message: 'Checkout completed' };
    }

    case 'customer.subscription.created': {
      const subscription = event.data.object as Stripe.Subscription;
      console.log('Subscription created:', subscription.metadata);
      // TODO: Create subscription record in database
      return { success: true, message: 'Subscription created' };
    }

    case 'customer.subscription.updated': {
      const subscription = event.data.object as Stripe.Subscription;
      console.log('Subscription updated:', subscription.id, subscription.status);
      // TODO: Update subscription record in database
      return { success: true, message: 'Subscription updated' };
    }

    case 'customer.subscription.deleted': {
      const subscription = event.data.object as Stripe.Subscription;
      console.log('Subscription deleted:', subscription.id);
      // TODO: Deactivate tenant or mark as cancelled
      return { success: true, message: 'Subscription deleted' };
    }

    case 'invoice.paid': {
      const invoice = event.data.object as Stripe.Invoice;
      console.log('Invoice paid:', invoice.id);
      // TODO: Record payment in database
      return { success: true, message: 'Invoice paid' };
    }

    case 'invoice.payment_failed': {
      const invoice = event.data.object as Stripe.Invoice;
      console.log('Invoice payment failed:', invoice.id);
      // TODO: Send notification, possibly suspend access
      return { success: true, message: 'Payment failed handled' };
    }

    default:
      console.log('Unhandled event type:', event.type);
      return { success: true, message: `Unhandled event type: ${event.type}` };
  }
}
