import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import type { OBGYNStatistics } from '../../types/obgyn';

// Mock API - replace with actual API
const obgynApi = {
  getStatistics: async (): Promise<OBGYNStatistics> => ({
    activePregnancies: 45,
    prenatalVisitsToday: 12,
    ultrasoundsThisWeek: 28,
    papSmearsThisMonth: 65,
    deliveriesThisMonth: 8,
    highRiskPregnancies: 7,
  }),
};

export const OBGYNDashboard = () => {
  const { data: stats, isLoading } = useQuery({
    queryKey: ['obgyn-stats'],
    queryFn: obgynApi.getStatistics,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">OB/GYN Department</h1>
          <p className="text-gray-600 mt-1">Obstetrics and Gynecology Management</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Active Pregnancies</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{stats?.activePregnancies || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Prenatal Visits Today</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{stats?.prenatalVisitsToday || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Ultrasounds This Week</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.ultrasoundsThisWeek || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pap Smears This Month</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">{stats?.papSmearsThisMonth || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Deliveries This Month</p>
          <p className="text-2xl font-bold text-pink-600 mt-1">{stats?.deliveriesThisMonth || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">High Risk Pregnancies</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{stats?.highRiskPregnancies || 0}</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Link to="/obgyn/pregnancies" className="card hover:shadow-lg transition-shadow">
          <h3 className="font-medium text-gray-900">Pregnancies</h3>
          <p className="text-sm text-gray-600 mt-1">Track and manage pregnancies</p>
        </Link>
        <Link to="/obgyn/prenatal" className="card hover:shadow-lg transition-shadow">
          <h3 className="font-medium text-gray-900">Prenatal Visits</h3>
          <p className="text-sm text-gray-600 mt-1">Schedule and record prenatal visits</p>
        </Link>
        <Link to="/obgyn/ultrasounds" className="card hover:shadow-lg transition-shadow">
          <h3 className="font-medium text-gray-900">Ultrasounds</h3>
          <p className="text-sm text-gray-600 mt-1">Obstetric ultrasound records</p>
        </Link>
        <Link to="/obgyn/pap-smears" className="card hover:shadow-lg transition-shadow">
          <h3 className="font-medium text-gray-900">Pap Smears</h3>
          <p className="text-sm text-gray-600 mt-1">Cervical screening management</p>
        </Link>
      </div>
    </div>
  );
};
