import type { Metadata } from 'next';
import Link from 'next/link';
import {
  Users,
  Calendar,
  DollarSign,
  TrendingUp,
  ArrowRight,
  Clock,
  CheckCircle,
  AlertCircle,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Dashboard',
  description: 'Your XENON dashboard - overview of your business operations.',
};

const stats = [
  {
    name: 'Total Patients',
    value: '1,234',
    change: '+12%',
    changeType: 'positive' as const,
    icon: Users,
  },
  {
    name: "Today's Appointments",
    value: '18',
    change: '+3',
    changeType: 'positive' as const,
    icon: Calendar,
  },
  {
    name: 'Revenue (MTD)',
    value: '45,230 AED',
    change: '+8%',
    changeType: 'positive' as const,
    icon: DollarSign,
  },
  {
    name: 'Pending Invoices',
    value: '12',
    change: '-2',
    changeType: 'negative' as const,
    icon: TrendingUp,
  },
];

const upcomingAppointments = [
  { id: 1, patient: 'Ahmed Al-Hassan', time: '09:00', type: 'Hearing Test', status: 'confirmed' },
  { id: 2, patient: 'Sarah Johnson', time: '09:30', type: 'Follow-up', status: 'confirmed' },
  { id: 3, patient: 'Mohammed Ali', time: '10:00', type: 'Fitting', status: 'pending' },
  { id: 4, patient: 'Fatima Al-Rashid', time: '10:30', type: 'Consultation', status: 'confirmed' },
  { id: 5, patient: 'James Wilson', time: '11:00', type: 'Annual Check', status: 'confirmed' },
];

const recentActivity = [
  { id: 1, action: 'New patient registered', patient: 'Ahmed Al-Hassan', time: '5 mins ago' },
  { id: 2, action: 'Appointment completed', patient: 'Sarah Johnson', time: '15 mins ago' },
  { id: 3, action: 'Invoice paid', patient: 'Mohammed Ali', time: '1 hour ago' },
  { id: 4, action: 'Lab results received', patient: 'Fatima Al-Rashid', time: '2 hours ago' },
];

const alerts = [
  { id: 1, type: 'warning', message: 'Low stock: Hearing Aid Batteries (5 remaining)' },
  { id: 2, type: 'info', message: '3 appointments need confirmation for tomorrow' },
];

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600">Welcome back! Here's what's happening today.</p>
        </div>
        <div className="flex gap-3">
          <Link href="/dashboard/appointments/new" className="btn-secondary">
            <Calendar className="h-4 w-4 mr-2" />
            New Appointment
          </Link>
          <Link href="/dashboard/patients/new" className="btn-primary">
            <Users className="h-4 w-4 mr-2" />
            Add Patient
          </Link>
        </div>
      </div>

      {/* Alerts */}
      {alerts.length > 0 && (
        <div className="space-y-2">
          {alerts.map((alert) => (
            <div
              key={alert.id}
              className={`flex items-center gap-3 p-3 rounded-lg ${
                alert.type === 'warning'
                  ? 'bg-yellow-50 text-yellow-800 border border-yellow-200'
                  : 'bg-blue-50 text-blue-800 border border-blue-200'
              }`}
            >
              <AlertCircle className="h-4 w-4 flex-shrink-0" />
              <span className="text-sm">{alert.message}</span>
            </div>
          ))}
        </div>
      )}

      {/* Stats Grid */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat) => (
          <div key={stat.name} className="card">
            <div className="flex items-center justify-between mb-3">
              <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center">
                <stat.icon className="h-5 w-5" />
              </div>
              <span
                className={`text-sm font-medium ${
                  stat.changeType === 'positive' ? 'text-green-600' : 'text-red-600'
                }`}
              >
                {stat.change}
              </span>
            </div>
            <div className="text-2xl font-bold text-gray-900">{stat.value}</div>
            <div className="text-sm text-gray-600">{stat.name}</div>
          </div>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid lg:grid-cols-3 gap-6">
        {/* Upcoming Appointments */}
        <div className="lg:col-span-2 card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-900">Today's Appointments</h2>
            <Link href="/dashboard/appointments" className="text-sm text-primary-600 hover:text-primary-700">
              View all <ArrowRight className="inline h-4 w-4" />
            </Link>
          </div>
          <div className="space-y-3">
            {upcomingAppointments.map((apt) => (
              <div
                key={apt.id}
                className="flex items-center justify-between p-3 rounded-lg bg-gray-50 hover:bg-gray-100 transition-colors"
              >
                <div className="flex items-center gap-4">
                  <div className="text-center">
                    <Clock className="h-4 w-4 text-gray-400 mx-auto mb-1" />
                    <span className="text-sm font-medium text-gray-900">{apt.time}</span>
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">{apt.patient}</div>
                    <div className="text-sm text-gray-500">{apt.type}</div>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <span
                    className={`badge ${
                      apt.status === 'confirmed'
                        ? 'bg-green-100 text-green-700'
                        : 'bg-yellow-100 text-yellow-700'
                    }`}
                  >
                    {apt.status === 'confirmed' ? (
                      <CheckCircle className="h-3 w-3 mr-1" />
                    ) : (
                      <Clock className="h-3 w-3 mr-1" />
                    )}
                    {apt.status}
                  </span>
                  <Link
                    href={`/dashboard/appointments/${apt.id}`}
                    className="text-sm text-primary-600 hover:text-primary-700"
                  >
                    View
                  </Link>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Recent Activity */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-900">Recent Activity</h2>
          </div>
          <div className="space-y-4">
            {recentActivity.map((activity) => (
              <div key={activity.id} className="flex items-start gap-3">
                <div className="h-8 w-8 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0 text-sm font-medium">
                  {activity.patient.charAt(0)}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm text-gray-900">{activity.action}</p>
                  <p className="text-sm text-gray-500">{activity.patient}</p>
                  <p className="text-xs text-gray-400 mt-0.5">{activity.time}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card bg-primary-50 border-primary-100">
        <h2 className="font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <Link
            href="/dashboard/patients/new"
            className="p-4 bg-white rounded-lg border border-gray-200 hover:border-primary-300 hover:shadow-sm transition-all"
          >
            <Users className="h-6 w-6 text-primary-600 mb-2" />
            <div className="font-medium text-gray-900">Add Patient</div>
            <div className="text-sm text-gray-500">Register a new patient</div>
          </Link>
          <Link
            href="/dashboard/appointments/new"
            className="p-4 bg-white rounded-lg border border-gray-200 hover:border-primary-300 hover:shadow-sm transition-all"
          >
            <Calendar className="h-6 w-6 text-primary-600 mb-2" />
            <div className="font-medium text-gray-900">Schedule Appointment</div>
            <div className="text-sm text-gray-500">Book a new appointment</div>
          </Link>
          <Link
            href="/dashboard/billing/new"
            className="p-4 bg-white rounded-lg border border-gray-200 hover:border-primary-300 hover:shadow-sm transition-all"
          >
            <DollarSign className="h-6 w-6 text-primary-600 mb-2" />
            <div className="font-medium text-gray-900">Create Invoice</div>
            <div className="text-sm text-gray-500">Generate a new invoice</div>
          </Link>
          <Link
            href="/dashboard/reports"
            className="p-4 bg-white rounded-lg border border-gray-200 hover:border-primary-300 hover:shadow-sm transition-all"
          >
            <TrendingUp className="h-6 w-6 text-primary-600 mb-2" />
            <div className="font-medium text-gray-900">View Reports</div>
            <div className="text-sm text-gray-500">Analytics and insights</div>
          </Link>
        </div>
      </div>
    </div>
  );
}
