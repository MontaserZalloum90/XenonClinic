import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';

export const FinancialList = () => {
  const { data: invoices, isLoading } = useQuery({
    queryKey: ['financial-invoices'],
    queryFn: async () => {
      const response = await api.get('/api/FinancialApi/invoices');
      return response.data;
    },
  });

  const { data: stats } = useQuery({
    queryKey: ['financial-stats'],
    queryFn: async () => {
      const response = await api.get('/api/FinancialApi/statistics');
      return response.data;
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Financial Management</h1>
        <p className="text-gray-600 mt-1">Manage invoices and financial operations</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Monthly Revenue</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            AED {stats?.monthlyRevenue?.toFixed(2) || '0.00'}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Unpaid Invoices</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.unpaidInvoices || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Overdue Invoices</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{stats?.overdueInvoices || 0}</p>
        </div>
      </div>

      <div className="card">
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading invoices...</p>
          </div>
        ) : (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Invoices</h3>
            {invoices && invoices.length > 0 ? (
              <p className="text-gray-600">{invoices.length} invoices found</p>
            ) : (
              <p className="text-gray-500">No invoices found</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
