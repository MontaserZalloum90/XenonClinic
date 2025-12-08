import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { api, appointmentsApi, patientsApi, laboratoryApi } from '../lib/api';
import { BarChart, Bar, PieChart, Pie, Cell, ResponsiveContainer, XAxis, YAxis, Tooltip, Legend } from 'recharts';

export const Dashboard = () => {
  const { user } = useAuth();

  // Fetch statistics from all modules
  const { data: appointmentStats } = useQuery({
    queryKey: ['dashboard-appointments'],
    queryFn: async () => {
      const response = await appointmentsApi.getStatistics();
      return response.data;
    },
  });

  const { data: patientStats } = useQuery({
    queryKey: ['dashboard-patients'],
    queryFn: async () => {
      const response = await patientsApi.getStatistics();
      return response.data;
    },
  });

  const { data: labStats } = useQuery({
    queryKey: ['dashboard-lab'],
    queryFn: async () => {
      const response = await laboratoryApi.getStatistics();
      return response.data;
    },
  });

  const { data: hrStats } = useQuery({
    queryKey: ['dashboard-hr'],
    queryFn: async () => {
      const response = await api.get('/api/HRApi/statistics');
      return response.data;
    },
  });

  const { data: financialStats } = useQuery({
    queryKey: ['dashboard-financial'],
    queryFn: async () => {
      const response = await api.get('/api/FinancialApi/statistics');
      return response.data;
    },
  });

  const { data: inventoryStats } = useQuery({
    queryKey: ['dashboard-inventory'],
    queryFn: async () => {
      const response = await api.get('/api/InventoryApi/statistics');
      return response.data;
    },
  });

  const { data: pharmacyStats } = useQuery({
    queryKey: ['dashboard-pharmacy'],
    queryFn: async () => {
      const response = await api.get('/api/PharmacyApi/statistics');
      return response.data;
    },
  });

  const { data: radiologyStats } = useQuery({
    queryKey: ['dashboard-radiology'],
    queryFn: async () => {
      const response = await api.get('/api/RadiologyApi/statistics');
      return response.data;
    },
  });

  // Module cards configuration
  const modules = [
    {
      name: 'Appointments',
      path: '/appointments',
      icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
      color: 'blue',
      stats: [
        { label: 'Today', value: appointmentStats?.today || 0 },
        { label: 'Upcoming', value: appointmentStats?.upcoming || 0 },
      ],
    },
    {
      name: 'Patients',
      path: '/patients',
      icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
      color: 'green',
      stats: [
        { label: 'Total', value: patientStats?.totalPatients || 0 },
        { label: 'New (30d)', value: patientStats?.newPatientsThisMonth || 0 },
      ],
    },
    {
      name: 'Laboratory',
      path: '/laboratory',
      icon: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z',
      color: 'purple',
      stats: [
        { label: 'Pending', value: labStats?.pendingOrders || 0 },
        { label: 'Urgent', value: labStats?.urgentOrders || 0 },
      ],
    },
    {
      name: 'HR',
      path: '/hr',
      icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z',
      color: 'indigo',
      stats: [
        { label: 'Total', value: hrStats?.totalEmployees || 0 },
        { label: 'Active', value: hrStats?.activeEmployees || 0 },
      ],
    },
    {
      name: 'Financial',
      path: '/financial',
      icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
      color: 'emerald',
      stats: [
        { label: 'Revenue', value: `AED ${(financialStats?.monthlyRevenue || 0).toFixed(0)}` },
        { label: 'Unpaid', value: financialStats?.unpaidInvoices || 0 },
      ],
    },
    {
      name: 'Inventory',
      path: '/inventory',
      icon: 'M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4',
      color: 'orange',
      stats: [
        { label: 'Items', value: inventoryStats?.totalItems || 0 },
        { label: 'Low Stock', value: inventoryStats?.lowStockItems || 0 },
      ],
    },
    {
      name: 'Pharmacy',
      path: '/pharmacy',
      icon: 'M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z',
      color: 'pink',
      stats: [
        { label: 'Pending', value: pharmacyStats?.pendingPrescriptions || 0 },
        { label: 'Today', value: pharmacyStats?.dispensedToday || 0 },
      ],
    },
    {
      name: 'Radiology',
      path: '/radiology',
      icon: 'M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z',
      color: 'cyan',
      stats: [
        { label: 'Pending', value: radiologyStats?.pendingOrders || 0 },
        { label: 'Today', value: radiologyStats?.completedToday || 0 },
      ],
    },
  ];

  // Chart data
  const appointmentChartData = appointmentStats?.statusDistribution
    ? Object.entries(appointmentStats.statusDistribution).map(([status, count]) => ({
        name: ['Booked', 'Confirmed', 'CheckedIn', 'Completed', 'Cancelled', 'NoShow'][Number(status)],
        value: count,
      }))
    : [];

  const moduleActivityData = [
    { name: 'Appointments', value: appointmentStats?.total || 0 },
    { name: 'Patients', value: patientStats?.totalPatients || 0 },
    { name: 'Lab Orders', value: labStats?.pendingOrders || 0 },
    { name: 'Prescriptions', value: pharmacyStats?.pendingPrescriptions || 0 },
  ];

  const COLORS = ['#3B82F6', '#10B981', '#8B5CF6', '#F59E0B', '#EF4444', '#6366F1'];

  return (
    <div className="space-y-6">
      {/* Welcome Section */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Welcome back, {user?.fullName}! 👋</h1>
        <p className="text-gray-600 mt-1">Here's your clinic overview across all modules</p>
      </div>

      {/* Module Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {modules.map((module) => (
          <Link
            key={module.name}
            to={module.path}
            className="card hover:shadow-lg transition-shadow cursor-pointer"
          >
            <div className="flex items-start justify-between mb-3">
              <div className={`w-12 h-12 bg-${module.color}-100 rounded-lg flex items-center justify-center`}>
                <svg className={`w-6 h-6 text-${module.color}-600`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={module.icon} />
                </svg>
              </div>
              <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-3">{module.name}</h3>
            <div className="space-y-1">
              {module.stats.map((stat, idx) => (
                <div key={idx} className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">{stat.label}</span>
                  <span className="text-sm font-bold text-gray-900">{stat.value}</span>
                </div>
              ))}
            </div>
          </Link>
        ))}
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Appointment Status Distribution */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Appointment Status Distribution</h3>
          {appointmentChartData.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={appointmentChartData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name} ${((percent || 0) * 100).toFixed(0)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {appointmentChartData.map((_entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              No appointment data available
            </div>
          )}
        </div>

        {/* Module Activity Overview */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Module Activity Overview</h3>
          {moduleActivityData.some((d) => d.value > 0) ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={moduleActivityData}>
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="value" fill="#3B82F6" />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              No activity data available
            </div>
          )}
        </div>
      </div>

      {/* System Health Status */}
      <div className="card">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">System Health Status</h3>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="flex items-center gap-3">
            <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
            <div>
              <p className="text-sm font-medium text-gray-900">API Status</p>
              <p className="text-xs text-gray-600">All systems operational</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
            <div>
              <p className="text-sm font-medium text-gray-900">Database</p>
              <p className="text-xs text-gray-600">Connected</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
            <div>
              <p className="text-sm font-medium text-gray-900">Modules</p>
              <p className="text-xs text-gray-600">8/8 Active</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="w-3 h-3 bg-yellow-500 rounded-full animate-pulse"></div>
            <div>
              <p className="text-sm font-medium text-gray-900">Alerts</p>
              <p className="text-xs text-gray-600">{(inventoryStats?.lowStockItems || 0) + (labStats?.urgentOrders || 0)} items need attention</p>
            </div>
          </div>
        </div>
      </div>

      {/* Quick Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card bg-gradient-to-br from-blue-50 to-blue-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-blue-900">Total Patients</p>
              <p className="text-3xl font-bold text-blue-900 mt-2">{patientStats?.totalPatients || 0}</p>
            </div>
            <div className="w-16 h-16 bg-blue-200 rounded-full flex items-center justify-center">
              <svg className="w-8 h-8 text-blue-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
            </div>
          </div>
        </div>

        <div className="card bg-gradient-to-br from-green-50 to-green-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-green-900">Monthly Revenue</p>
              <p className="text-3xl font-bold text-green-900 mt-2">
                AED {(financialStats?.monthlyRevenue || 0).toFixed(0)}
              </p>
            </div>
            <div className="w-16 h-16 bg-green-200 rounded-full flex items-center justify-center">
              <svg className="w-8 h-8 text-green-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>

        <div className="card bg-gradient-to-br from-purple-50 to-purple-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-purple-900">Active Staff</p>
              <p className="text-3xl font-bold text-purple-900 mt-2">{hrStats?.activeEmployees || 0}</p>
            </div>
            <div className="w-16 h-16 bg-purple-200 rounded-full flex items-center justify-center">
              <svg className="w-8 h-8 text-purple-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
