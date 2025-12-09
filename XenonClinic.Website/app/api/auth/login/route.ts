import { NextRequest, NextResponse } from 'next/server';

// Demo users for testing
const demoUsers = [
  {
    id: 'user_demo',
    email: 'demo@xenon.ae',
    password: 'demo123',
    name: 'Demo User',
    role: 'admin',
    tenantId: 'tenant_demo',
  },
];

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const { email, password } = body;

    if (!email || !password) {
      return NextResponse.json(
        { error: 'Email and password are required' },
        { status: 400 }
      );
    }

    // Find user
    const user = demoUsers.find(
      u => u.email === email && u.password === password
    );

    if (!user) {
      return NextResponse.json(
        { error: 'Invalid email or password' },
        { status: 401 }
      );
    }

    // In production, you would:
    // 1. Verify password hash
    // 2. Generate JWT token
    // 3. Set secure HTTP-only cookies
    // 4. Log authentication event

    // For demo, return user info (without password)
    const { password: _, ...userWithoutPassword } = user;

    return NextResponse.json({
      success: true,
      user: userWithoutPassword,
      token: `demo_token_${Date.now()}`,
    });
  } catch (error) {
    console.error('Login error:', error);
    return NextResponse.json(
      { error: 'Authentication failed' },
      { status: 500 }
    );
  }
}
