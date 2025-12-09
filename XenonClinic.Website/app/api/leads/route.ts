import { NextRequest, NextResponse } from 'next/server';

// In-memory store for demo purposes
// In production, this would be stored in a database
const leads: Array<{
  id: string;
  email: string;
  name: string;
  company?: string;
  phone?: string;
  inquiryType: string;
  message: string;
  source: string;
  createdAt: string;
}> = [];

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    const {
      name,
      email,
      company,
      phone,
      inquiryType = 'general',
      message,
      source = 'website',
    } = body;

    if (!name || !email) {
      return NextResponse.json(
        { error: 'Name and email are required' },
        { status: 400 }
      );
    }

    // Basic email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      return NextResponse.json(
        { error: 'Invalid email format' },
        { status: 400 }
      );
    }

    const lead = {
      id: `lead_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      name,
      email,
      company,
      phone,
      inquiryType,
      message: message || '',
      source,
      createdAt: new Date().toISOString(),
    };

    leads.push(lead);

    // In production, you would:
    // 1. Save to database
    // 2. Send notification email to sales team
    // 3. Add to CRM
    // 4. Send confirmation email to lead

    console.log('New lead captured:', lead);

    return NextResponse.json({
      success: true,
      message: 'Thank you for your interest. We will be in touch shortly.',
      leadId: lead.id,
    });
  } catch (error) {
    console.error('Lead capture error:', error);
    return NextResponse.json(
      { error: 'Failed to process your request. Please try again.' },
      { status: 500 }
    );
  }
}

export async function GET(request: NextRequest) {
  // This endpoint would be protected in production
  const authHeader = request.headers.get('authorization');

  // Simple API key check for demo
  if (authHeader !== `Bearer ${process.env.API_SECRET_KEY}`) {
    return NextResponse.json(
      { error: 'Unauthorized' },
      { status: 401 }
    );
  }

  return NextResponse.json({
    leads,
    total: leads.length,
  });
}
