import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { OrthopedicStatistics } from '../../types/orthopedics';
import { useT } from '../../contexts/TenantContext';
import { useNavigate } from 'react-router-dom';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';

export const OrthopedicsDashboard = () => {
  const t = useT();
  const navigate = useNavigate();

  const { data: stats, isLoading } = useQuery<OrthopedicStatistics>({
    queryKey: ['orthopedics-stats'],
    queryFn: async () => {
      const response = await api.get('/api/OrthopedicsApi/statistics');
      return response.data;
    },
  });

  const COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#6366F1'];

  const fractureTypeChartData = stats?.fractureTypeDistribution
    ? Object.entries(stats.fractureTypeDistribution).map(([name, value]) => ({ name, value }))
    : [];

  const surgeryTypeChartData = stats?.surgeryTypeDistribution
    ? Object.entries(stats.surgeryTypeDistribution).map(([name, value]) => ({ name, value }))
    : [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('nav.orthopedics', 'Orthopedics Dashboard')}</h1>
          <p className="text-gray-600 mt-1">{t('page.orthopedics.description', 'Manage orthopedic exams, fractures, and surgeries')}</p>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {[
          {
            label: t('stats.totalExams', 'Total Exams'),
            value: stats?.totalExams || 0,
            color: 'text-blue-600',
            bgColor: 'bg-blue-100',
            icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2',
            action: () => navigate('/orthopedics/exams')
          },
          {
            label: t('stats.examsThisMonth', 'Exams This Month'),
            value: stats?.examsThisMonth || 0,
            color: 'text-green-600',
            bgColor: 'bg-green-100',
            icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
            action: () => navigate('/orthopedics/exams')
          },
          {
            label: t('stats.activeFractures', 'Active Fractures'),
            value: stats?.activeFractures || 0,
            color: 'text-orange-600',
            bgColor: 'bg-orange-100',
            icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
            action: () => navigate('/orthopedics/fractures')
          },
          {
            label: t('stats.healedFractures', 'Healed Fractures'),
            value: stats?.healedFractures || 0,
            color: 'text-emerald-600',
            bgColor: 'bg-emerald-100',
            icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
            action: () => navigate('/orthopedics/fractures')
          },
          {
            label: t('stats.totalSurgeries', 'Total Surgeries'),
            value: stats?.totalSurgeries || 0,
            color: 'text-purple-600',
            bgColor: 'bg-purple-100',
            icon: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z',
            action: () => navigate('/orthopedics/surgeries')
          },
          {
            label: t('stats.surgeriesThisMonth', 'Surgeries This Month'),
            value: stats?.surgeriesThisMonth || 0,
            color: 'text-indigo-600',
            bgColor: 'bg-indigo-100',
            icon: 'M12 4v16m8-8H4',
            action: () => navigate('/orthopedics/surgeries')
          },
          {
            label: t('stats.pendingFollowUps', 'Pending Follow-ups'),
            value: stats?.pendingFollowUps || 0,
            color: 'text-yellow-600',
            bgColor: 'bg-yellow-100',
            icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',
            action: () => navigate('/orthopedics/fractures')
          },
          {
            label: t('stats.complicatedCases', 'Complicated Cases'),
            value: stats?.complicatedCases || 0,
            color: 'text-red-600',
            bgColor: 'bg-red-100',
            icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
            action: () => navigate('/orthopedics/fractures')
          },
        ].map((stat, index) => (
          <div
            key={stat.label}
            className="card animate-fade-in cursor-pointer hover:shadow-lg transition-shadow"
            style={{ animationDelay: `${index * 50}ms` }}
            onClick={stat.action}
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">{stat.label}</p>
                <p className={`text-2xl font-bold ${stat.color} mt-1`}>{stat.value}</p>
              </div>
              <div className={`w-12 h-12 ${stat.bgColor} rounded-lg flex items-center justify-center`}>
                <svg className={`w-6 h-6 ${stat.color}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={stat.icon} />
                </svg>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Quick Actions */}
      <div className="card animate-fade-in">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">{t('section.quickActions', 'Quick Actions')}</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button
            onClick={() => navigate('/orthopedics/exams')}
            className="btn btn-primary justify-center"
          >
            <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            {t('action.newExam', 'New Exam')}
          </button>
          <button
            onClick={() => navigate('/orthopedics/fractures')}
            className="btn btn-primary justify-center"
          >
            <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            {t('action.recordFracture', 'Record Fracture')}
          </button>
          <button
            onClick={() => navigate('/orthopedics/surgeries')}
            className="btn btn-primary justify-center"
          >
            <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            {t('action.recordSurgery', 'Record Surgery')}
          </button>
        </div>
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Fracture Type Distribution */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('chart.fractureTypes', 'Fracture Type Distribution')}
          </h3>
          {isLoading ? (
            <div className="h-[300px] flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : fractureTypeChartData.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={fractureTypeChartData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, value }) => `${name} (${value})`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {fractureTypeChartData.map((_entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              {t('message.noData', 'No data available')}
            </div>
          )}
        </div>

        {/* Surgery Type Distribution */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('chart.surgeryTypes', 'Surgery Type Distribution')}
          </h3>
          {isLoading ? (
            <div className="h-[300px] flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : surgeryTypeChartData.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={surgeryTypeChartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="value" fill="#8B5CF6" name={t('field.count', 'Count')} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              {t('message.noData', 'No data available')}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
