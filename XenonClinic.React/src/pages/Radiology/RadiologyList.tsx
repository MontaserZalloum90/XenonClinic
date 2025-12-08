import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';

export const RadiologyList = () => {
  const { data: orders, isLoading } = useQuery({
    queryKey: ['radiology-orders'],
    queryFn: async () => {
      const response = await api.get('/api/RadiologyApi/orders');
      return response.data;
    },
  });

  const { data: stats } = useQuery({
    queryKey: ['radiology-stats'],
    queryFn: async () => {
      const response = await api.get('/api/RadiologyApi/statistics');
      return response.data;
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Radiology</h1>
        <p className="text-gray-600 mt-1">Manage radiology orders and imaging</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Orders</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{orders?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending Orders</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.pendingOrders || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed Today</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.completedToday || 0}</p>
        </div>
      </div>

      <div className="card">
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading orders...</p>
          </div>
        ) : (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Radiology Orders</h3>
            {orders && orders.length > 0 ? (
              <p className="text-gray-600">{orders.length} orders found</p>
            ) : (
              <p className="text-gray-500">No orders found</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
