import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { AnalyticsDashboard, Report } from '../../types/analytics';
import { format, subDays } from 'date-fns';
import { useT } from '../../contexts/TenantContext';
import { BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, ResponsiveContainer, XAxis, YAxis, Tooltip, Legend, CartesianGrid } from 'recharts';

export const AnalyticsDashboardPage = () => {
  const t = useT();
  const [startDate, setStartDate] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'));
  const [endDate, setEndDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [selectedDepartment, setSelectedDepartment] = useState<string>('all');

  // Fetch dashboard data
  const { data: dashboardData, isLoading } = useQuery<AnalyticsDashboard>({
    queryKey: ['analytics-dashboard', startDate, endDate, selectedDepartment],
    queryFn: async () => {
      const params: any = { startDate, endDate };
      if (selectedDepartment !== 'all') {
        params.departmentId = selectedDepartment;
      }
      const response = await api.get('/api/AnalyticsApi/dashboard', { params });
      return response.data;
    },
  });

  // Fetch recent reports
  const { data: recentReports } = useQuery<Report[]>({
    queryKey: ['analytics-recent-reports'],
    queryFn: async () => {
      const response = await api.get('/api/AnalyticsApi/reports/recent', { params: { limit: 5 } });
      return response.data;
    },
  });

  const handleExport = () => {
    // Placeholder for export functionality
    alert('Export functionality would be implemented here');
  };

  const COLORS = ['#3B82F6', '#10B981', '#8B5CF6', '#F59E0B', '#EF4444', '#6366F1'];

  const getStatusBadge = (status: number) => {
    const statusConfig = {
      0: { label: 'Pending', class: 'text-yellow-600 bg-yellow-100' },
      1: { label: 'Generating', class: 'text-blue-600 bg-blue-100' },
      2: { label: 'Completed', class: 'text-green-600 bg-green-100' },
      3: { label: 'Failed', class: 'text-red-600 bg-red-100' },
    };
    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig[0];
    return <span className={`px-2 py-1 text-xs font-medium rounded-full ${config.class}`}>{config.label}</span>;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('nav.analytics', 'Analytics Dashboard')}</h1>
          <p className="text-gray-600 mt-1">{t('page.analytics.description', 'View comprehensive analytics and generate reports')}</p>
        </div>
        <button onClick={handleExport} className="btn btn-primary">
          <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          {t('action.export', 'Export Report')}
        </button>
      </div>

      {/* Filters */}
      <div className="card animate-fade-in">
        <h3 className="text-sm font-medium text-gray-700 mb-3">{t('field.filters', 'Filters')}</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm text-gray-600 mb-1">{t('field.startDate', 'Start Date')}</label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-600 mb-1">{t('field.endDate', 'End Date')}</label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-600 mb-1">{t('field.department', 'Department')}</label>
            <select
              value={selectedDepartment}
              onChange={(e) => setSelectedDepartment(e.target.value)}
              className="input w-full"
            >
              <option value="all">{t('field.all', 'All Departments')}</option>
              <option value="1">{t('nav.laboratory', 'Laboratory')}</option>
              <option value="2">{t('nav.pharmacy', 'Pharmacy')}</option>
              <option value="3">{t('nav.radiology', 'Radiology')}</option>
              <option value="4">{t('nav.audiology', 'Audiology')}</option>
            </select>
          </div>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        {[
          {
            label: t('stats.totalPatients', 'Total Patients'),
            value: dashboardData?.totalPatients || 0,
            color: 'text-blue-600',
            bgColor: 'bg-blue-100',
            icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z'
          },
          {
            label: t('stats.totalAppointments', 'Total Appointments'),
            value: dashboardData?.totalAppointments || 0,
            color: 'text-green-600',
            bgColor: 'bg-green-100',
            icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z'
          },
          {
            label: t('stats.totalRevenue', 'Total Revenue'),
            value: `AED ${(dashboardData?.totalRevenue || 0).toFixed(0)}`,
            color: 'text-emerald-600',
            bgColor: 'bg-emerald-100',
            icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
          },
          {
            label: t('stats.averageWaitTime', 'Avg Wait Time'),
            value: `${dashboardData?.averageWaitTime || 0} min`,
            color: 'text-orange-600',
            bgColor: 'bg-orange-100',
            icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z'
          },
          {
            label: t('stats.satisfaction', 'Satisfaction'),
            value: `${((dashboardData?.patientSatisfaction || 0) * 100).toFixed(0)}%`,
            color: 'text-purple-600',
            bgColor: 'bg-purple-100',
            icon: 'M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
          },
        ].map((stat, index) => (
          <div key={stat.label} className="card animate-fade-in" style={{ animationDelay: `${index * 50}ms` }}>
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

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Appointments by Day */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('chart.appointmentsByDay', 'Appointments by Day')}
          </h3>
          {isLoading ? (
            <div className="h-[300px] flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : dashboardData?.appointmentsByDay && dashboardData.appointmentsByDay.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={dashboardData.appointmentsByDay}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="label" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="count" stroke="#3B82F6" strokeWidth={2} name={t('field.appointments', 'Appointments')} />
              </LineChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              {t('message.noData', 'No data available')}
            </div>
          )}
        </div>

        {/* Revenue by Month */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('chart.revenueByMonth', 'Revenue by Month')}
          </h3>
          {isLoading ? (
            <div className="h-[300px] flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : dashboardData?.revenueByMonth && dashboardData.revenueByMonth.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={dashboardData.revenueByMonth}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="label" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="revenue" fill="#10B981" name={t('field.revenue', 'Revenue (AED)')} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              {t('message.noData', 'No data available')}
            </div>
          )}
        </div>

        {/* Top Services */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('chart.topServices', 'Top Services')}
          </h3>
          {isLoading ? (
            <div className="h-[300px] flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : dashboardData?.topServices && dashboardData.topServices.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={dashboardData.topServices}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ serviceName, count }) => `${serviceName} (${count})`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="count"
                >
                  {dashboardData.topServices.map((_entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              {t('message.noData', 'No data available')}
            </div>
          )}
        </div>

        {/* Department Stats */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('chart.departmentStats', 'Department Statistics')}
          </h3>
          {isLoading ? (
            <div className="h-[300px] flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : dashboardData?.departmentStats && dashboardData.departmentStats.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={dashboardData.departmentStats}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="departmentName" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="patientCount" fill="#8B5CF6" name={t('field.patients', 'Patients')} />
                <Bar dataKey="appointmentCount" fill="#3B82F6" name={t('field.appointments', 'Appointments')} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-500">
              {t('message.noData', 'No data available')}
            </div>
          )}
        </div>
      </div>

      {/* Recent Reports Table */}
      <div className="card animate-fade-in">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900">
            {t('page.analytics.recentReports', 'Recent Reports')}
          </h3>
          <a href="/analytics/reports" className="text-sm text-primary-600 hover:text-primary-700">
            {t('action.viewAll', 'View All')} →
          </a>
        </div>

        {recentReports && recentReports.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    {t('field.reportName', 'Report Name')}
                  </th>
                  <th className="px-6 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    {t('field.type', 'Type')}
                  </th>
                  <th className="px-6 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    {t('field.generated', 'Generated')}
                  </th>
                  <th className="px-6 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    {t('field.status', 'Status')}
                  </th>
                  <th className="px-6 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    {t('field.actions', 'Actions')}
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {recentReports.map((report) => (
                  <tr key={report.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{report.name}</div>
                      {report.description && (
                        <div className="text-sm text-gray-500">{report.description}</div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {['Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annual', 'Custom'][report.type] || 'Unknown'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(report.generatedAt), 'MMM d, yyyy HH:mm')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(report.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <div className="flex items-center gap-2">
                        {report.status === 2 && report.fileUrl && (
                          <button
                            className="text-primary-600 hover:text-primary-900"
                            title={t('action.download', 'Download')}
                          >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                            </svg>
                          </button>
                        )}
                        <button
                          className="text-gray-600 hover:text-gray-900"
                          title={t('action.view', 'View')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            <svg className="w-12 h-12 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <p>{t('message.noReports', 'No reports generated yet')}</p>
          </div>
        )}
      </div>
    </div>
  );
};
