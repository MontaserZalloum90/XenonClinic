import type { Metadata } from 'next';
import Link from 'next/link';
import {
  CreditCard,
  Download,
  Plus,
  Receipt,
  CheckCircle,
  Clock,
  AlertCircle,
  ArrowUpRight,
  Calendar,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Billing',
  description: 'Manage invoices, payments, and billing settings.',
};

const invoices = [
  {
    id: 'INV-001',
    patient: 'Ahmed Al-Hassan',
    amount: '1,500 AED',
    date: '2024-01-15',
    dueDate: '2024-01-30',
    status: 'paid',
  },
  {
    id: 'INV-002',
    patient: 'Sarah Johnson',
    amount: '2,300 AED',
    date: '2024-01-14',
    dueDate: '2024-01-29',
    status: 'pending',
  },
  {
    id: 'INV-003',
    patient: 'Mohammed Ali',
    amount: '850 AED',
    date: '2024-01-13',
    dueDate: '2024-01-28',
    status: 'pending',
  },
  {
    id: 'INV-004',
    patient: 'Fatima Al-Rashid',
    amount: '3,200 AED',
    date: '2024-01-12',
    dueDate: '2024-01-27',
    status: 'overdue',
  },
  {
    id: 'INV-005',
    patient: 'James Wilson',
    amount: '1,100 AED',
    date: '2024-01-10',
    dueDate: '2024-01-25',
    status: 'paid',
  },
];

const subscription = {
  plan: 'Growth',
  status: 'active',
  price: '999 AED',
  billingCycle: 'monthly',
  nextBilling: '2024-02-01',
  branches: 3,
  users: 12,
};

export default function BillingPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Billing</h1>
          <p className="text-gray-600">Manage invoices and payment settings.</p>
        </div>
        <div className="flex gap-3">
          <Link href="/dashboard/billing/new" className="btn-primary">
            <Plus className="h-4 w-4 mr-2" />
            Create Invoice
          </Link>
        </div>
      </div>

      {/* Stats */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="card">
          <div className="flex items-center gap-3 mb-2">
            <div className="h-10 w-10 rounded-lg bg-green-100 text-green-600 flex items-center justify-center">
              <CheckCircle className="h-5 w-5" />
            </div>
            <div className="text-2xl font-bold text-gray-900">8,450 AED</div>
          </div>
          <div className="text-sm text-gray-600">Paid this month</div>
        </div>
        <div className="card">
          <div className="flex items-center gap-3 mb-2">
            <div className="h-10 w-10 rounded-lg bg-yellow-100 text-yellow-600 flex items-center justify-center">
              <Clock className="h-5 w-5" />
            </div>
            <div className="text-2xl font-bold text-gray-900">3,150 AED</div>
          </div>
          <div className="text-sm text-gray-600">Pending payment</div>
        </div>
        <div className="card">
          <div className="flex items-center gap-3 mb-2">
            <div className="h-10 w-10 rounded-lg bg-red-100 text-red-600 flex items-center justify-center">
              <AlertCircle className="h-5 w-5" />
            </div>
            <div className="text-2xl font-bold text-gray-900">3,200 AED</div>
          </div>
          <div className="text-sm text-gray-600">Overdue</div>
        </div>
        <div className="card">
          <div className="flex items-center gap-3 mb-2">
            <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center">
              <Receipt className="h-5 w-5" />
            </div>
            <div className="text-2xl font-bold text-gray-900">23</div>
          </div>
          <div className="text-sm text-gray-600">Invoices this month</div>
        </div>
      </div>

      {/* Subscription & Invoices Grid */}
      <div className="grid lg:grid-cols-3 gap-6">
        {/* Invoices List */}
        <div className="lg:col-span-2 card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-900">Recent Invoices</h2>
            <Link href="/dashboard/billing/invoices" className="text-sm text-primary-600 hover:text-primary-700">
              View all
            </Link>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b text-left text-sm text-gray-500">
                  <th className="pb-3 font-medium">Invoice</th>
                  <th className="pb-3 font-medium">Patient</th>
                  <th className="pb-3 font-medium">Amount</th>
                  <th className="pb-3 font-medium">Status</th>
                  <th className="pb-3 font-medium"></th>
                </tr>
              </thead>
              <tbody className="text-sm">
                {invoices.map((invoice) => (
                  <tr key={invoice.id} className="border-b last:border-0">
                    <td className="py-3">
                      <div className="font-medium text-gray-900">{invoice.id}</div>
                      <div className="text-gray-500 text-xs">{invoice.date}</div>
                    </td>
                    <td className="py-3 text-gray-700">{invoice.patient}</td>
                    <td className="py-3 font-medium text-gray-900">{invoice.amount}</td>
                    <td className="py-3">
                      <span
                        className={`badge ${
                          invoice.status === 'paid'
                            ? 'bg-green-100 text-green-700'
                            : invoice.status === 'pending'
                            ? 'bg-yellow-100 text-yellow-700'
                            : 'bg-red-100 text-red-700'
                        }`}
                      >
                        {invoice.status}
                      </span>
                    </td>
                    <td className="py-3">
                      <div className="flex items-center gap-2">
                        <button className="p-1 hover:bg-gray-100 rounded" title="Download">
                          <Download className="h-4 w-4 text-gray-400" />
                        </button>
                        <Link
                          href={`/dashboard/billing/${invoice.id}`}
                          className="p-1 hover:bg-gray-100 rounded"
                          title="View"
                        >
                          <ArrowUpRight className="h-4 w-4 text-gray-400" />
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Subscription Card */}
        <div className="space-y-4">
          <div className="card">
            <div className="flex items-center justify-between mb-4">
              <h2 className="font-semibold text-gray-900">Your Subscription</h2>
              <span className="badge bg-green-100 text-green-700">Active</span>
            </div>
            <div className="space-y-4">
              <div>
                <div className="text-2xl font-bold text-gray-900">
                  {subscription.plan} Plan
                </div>
                <div className="text-gray-600">
                  {subscription.price}/{subscription.billingCycle}
                </div>
              </div>
              <div className="pt-4 border-t space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Branches</span>
                  <span className="font-medium">{subscription.branches} of 3</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Users</span>
                  <span className="font-medium">{subscription.users} of 15</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Next billing</span>
                  <span className="font-medium">{subscription.nextBilling}</span>
                </div>
              </div>
              <div className="pt-4 border-t flex gap-2">
                <Link href="/dashboard/billing/subscription" className="btn-secondary flex-1 text-center">
                  Manage
                </Link>
                <Link href="/pricing" className="btn-primary flex-1 text-center">
                  Upgrade
                </Link>
              </div>
            </div>
          </div>

          {/* Payment Method */}
          <div className="card">
            <div className="flex items-center justify-between mb-4">
              <h2 className="font-semibold text-gray-900">Payment Method</h2>
            </div>
            <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
              <div className="h-10 w-14 bg-gradient-to-r from-blue-600 to-blue-400 rounded flex items-center justify-center">
                <CreditCard className="h-5 w-5 text-white" />
              </div>
              <div>
                <div className="font-medium text-gray-900">Visa ending in 4242</div>
                <div className="text-sm text-gray-500">Expires 12/25</div>
              </div>
            </div>
            <button className="w-full mt-4 text-sm text-primary-600 hover:text-primary-700">
              Update payment method
            </button>
          </div>

          {/* Billing History */}
          <div className="card">
            <div className="flex items-center justify-between mb-4">
              <h2 className="font-semibold text-gray-900">Billing History</h2>
            </div>
            <div className="space-y-3">
              {[
                { date: 'Jan 1, 2024', amount: '999 AED' },
                { date: 'Dec 1, 2023', amount: '999 AED' },
                { date: 'Nov 1, 2023', amount: '999 AED' },
              ].map((payment, i) => (
                <div key={i} className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-gray-400" />
                    <span className="text-gray-600">{payment.date}</span>
                  </div>
                  <span className="font-medium text-gray-900">{payment.amount}</span>
                </div>
              ))}
            </div>
            <Link
              href="/dashboard/billing/history"
              className="block w-full mt-4 text-center text-sm text-primary-600 hover:text-primary-700"
            >
              View full history
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
