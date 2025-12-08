import { useQuery } from '@tanstack/react-query';
import { laboratoryApi } from '../../lib/api';
import type { LabOrder } from '../../types/laboratory';
import { LabOrderStatus } from '../../types/laboratory';
import { format } from 'date-fns';

export const LaboratoryList = () => {
  const { data: orders, isLoading, error } = useQuery<LabOrder[]>({
    queryKey: ['lab-orders'],
    queryFn: async () => {
      const response = await laboratoryApi.getAllOrders();
      return response.data;
    },
  });

  const getStatusBadge = (status: number) => {
    const statusConfig = {
      [LabOrderStatus.Pending]: { label: 'Pending', className: 'bg-yellow-100 text-yellow-800' },
      [LabOrderStatus.Collected]: { label: 'Collected', className: 'bg-blue-100 text-blue-800' },
      [LabOrderStatus.InProgress]: { label: 'In Progress', className: 'bg-purple-100 text-purple-800' },
      [LabOrderStatus.Completed]: { label: 'Completed', className: 'bg-green-100 text-green-800' },
      [LabOrderStatus.Cancelled]: { label: 'Cancelled', className: 'bg-red-100 text-red-800' },
    };
    const config = statusConfig[status as keyof typeof statusConfig];
    return (
      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.className}`}>
        {config.label}
      </span>
    );
  };

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md">
        <p className="text-sm text-red-600">Error loading orders: {(error as Error).message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Laboratory</h1>
          <p className="text-gray-600 mt-1">Manage lab orders and tests</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Orders</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{orders?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">
            {orders?.filter((o) => o.status === LabOrderStatus.Pending).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">In Progress</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {orders?.filter((o) => o.status === LabOrderStatus.InProgress).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {orders?.filter((o) => o.status === LabOrderStatus.Completed).length || 0}
          </p>
        </div>
      </div>

      <div className="card overflow-hidden p-0">
        {isLoading ? (
          <div className="p-8 text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading orders...</p>
          </div>
        ) : orders && orders.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order #</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Amount</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {orders.map((order) => (
                  <tr key={order.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {order.orderNumber}
                      {order.isUrgent && (
                        <span className="ml-2 text-red-600 font-bold">URGENT</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {order.patient?.fullNameEn || 'Unknown'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {format(new Date(order.orderDate), 'MMM dd, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(order.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      AED {order.totalAmount.toFixed(2)}
                      {order.isPaid && <span className="ml-2 text-green-600">✓ Paid</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="p-8 text-center">
            <h3 className="mt-2 text-sm font-medium text-gray-900">No orders found</h3>
          </div>
        )}
      </div>
    </div>
  );
};
