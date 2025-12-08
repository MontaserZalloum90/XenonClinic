import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';

export const PharmacyList = () => {
  const { data: prescriptions, isLoading } = useQuery({
    queryKey: ['pharmacy-prescriptions'],
    queryFn: async () => {
      const response = await api.get('/api/PharmacyApi/prescriptions');
      return response.data;
    },
  });

  const { data: stats } = useQuery({
    queryKey: ['pharmacy-stats'],
    queryFn: async () => {
      const response = await api.get('/api/PharmacyApi/statistics');
      return response.data;
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Pharmacy</h1>
        <p className="text-gray-600 mt-1">Manage prescriptions and medications</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Prescriptions</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{prescriptions?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.pendingPrescriptions || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Dispensed Today</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.dispensedToday || 0}</p>
        </div>
      </div>

      <div className="card">
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading prescriptions...</p>
          </div>
        ) : (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Prescriptions</h3>
            {prescriptions && prescriptions.length > 0 ? (
              <p className="text-gray-600">{prescriptions.length} prescriptions found</p>
            ) : (
              <p className="text-gray-500">No prescriptions found</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
