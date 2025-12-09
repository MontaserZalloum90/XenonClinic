import { NextRequest, NextResponse } from 'next/server';

// In-memory store for demo purposes
const tenants: Array<{
  id: string;
  companyName: string;
  companyType: string;
  clinicType?: string;
  ownerName: string;
  ownerEmail: string;
  phone?: string;
  country: string;
  status: 'trial' | 'active' | 'suspended' | 'cancelled';
  trialEndsAt: string;
  createdAt: string;
}> = [];

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    const {
      companyName,
      companyType,
      clinicType,
      fullName,
      email,
      phone,
      country = 'UAE',
    } = body;

    // Validation
    if (!companyName || !companyType || !fullName || !email) {
      return NextResponse.json(
        { error: 'Missing required fields' },
        { status: 400 }
      );
    }

    // Check if email already exists
    const existingTenant = tenants.find(t => t.ownerEmail === email);
    if (existingTenant) {
      return NextResponse.json(
        { error: 'An account with this email already exists' },
        { status: 409 }
      );
    }

    // Calculate trial end date (30 days from now)
    const trialEndsAt = new Date();
    trialEndsAt.setDate(trialEndsAt.getDate() + 30);

    const tenant = {
      id: `tenant_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      companyName,
      companyType,
      clinicType: companyType === 'CLINIC' ? clinicType : undefined,
      ownerName: fullName,
      ownerEmail: email,
      phone,
      country,
      status: 'trial' as const,
      trialEndsAt: trialEndsAt.toISOString(),
      createdAt: new Date().toISOString(),
    };

    tenants.push(tenant);

    // In production, you would:
    // 1. Create tenant record in database
    // 2. Create owner user account
    // 3. Send verification email
    // 4. Set up default configuration based on company/clinic type
    // 5. Create default branch

    console.log('New tenant created:', tenant);

    return NextResponse.json({
      success: true,
      tenant: {
        id: tenant.id,
        companyName: tenant.companyName,
        status: tenant.status,
        trialEndsAt: tenant.trialEndsAt,
      },
      message: 'Your trial account has been created. Please check your email to verify your account.',
    });
  } catch (error) {
    console.error('Tenant creation error:', error);
    return NextResponse.json(
      { error: 'Failed to create account. Please try again.' },
      { status: 500 }
    );
  }
}

export async function GET(request: NextRequest) {
  // This endpoint would be protected in production
  const authHeader = request.headers.get('authorization');

  if (authHeader !== `Bearer ${process.env.API_SECRET_KEY}`) {
    return NextResponse.json(
      { error: 'Unauthorized' },
      { status: 401 }
    );
  }

  const url = new URL(request.url);
  const status = url.searchParams.get('status');
  const search = url.searchParams.get('search');

  let filteredTenants = [...tenants];

  if (status) {
    filteredTenants = filteredTenants.filter(t => t.status === status);
  }

  if (search) {
    const searchLower = search.toLowerCase();
    filteredTenants = filteredTenants.filter(
      t =>
        t.companyName.toLowerCase().includes(searchLower) ||
        t.ownerEmail.toLowerCase().includes(searchLower)
    );
  }

  return NextResponse.json({
    tenants: filteredTenants,
    total: filteredTenants.length,
  });
}
