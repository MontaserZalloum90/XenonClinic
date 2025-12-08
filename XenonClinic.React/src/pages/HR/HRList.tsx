import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';

export const HRList = () => {
  const { data: employees, isLoading } = useQuery({
    queryKey: ['hr-employees'],
    queryFn: async () => {
      const response = await api.get('/api/HRApi/employees');
      return response.data;
    },
  });

  const { data: stats } = useQuery({
    queryKey: ['hr-stats'],
    queryFn: async () => {
      const response = await api.get('/api/HRApi/statistics');
      return response.data;
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Human Resources</h1>
        <p className="text-gray-600 mt-1">Manage employees and HR operations</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Employees</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Active Employees</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.activeEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Listed Below</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{employees?.length || 0}</p>
        </div>
      </div>

      <div className="card">
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading employees...</p>
          </div>
        ) : (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Employee Directory</h3>
            {employees && employees.length > 0 ? (
              <p className="text-gray-600">{employees.length} employees found</p>
            ) : (
              <p className="text-gray-500">No employees found</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
