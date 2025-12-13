import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { Report, ReportType, CreateReportRequest, AnalyticsStatistics } from '../../types/analytics';
import { format } from 'date-fns';
import { useT } from '../../contexts/TenantContext';
import { Dialog } from '@headlessui/react';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/ui/Toast';

export const ReportsList = () => {
  const t = useT();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<string>('all');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState<CreateReportRequest>({
    name: '',
    type: 0,
    parameters: {
      startDate: format(new Date(), 'yyyy-MM-dd'),
      endDate: format(new Date(), 'yyyy-MM-dd'),
      includeCharts: true,
      includeDetails: true,
    },
    description: '',
  });

  // Fetch all reports
  const { data: reports, isLoading } = useQuery<Report[]>({
    queryKey: ['analytics-reports'],
    queryFn: async () => {
      const response = await api.get('/api/AnalyticsApi/reports');
      return response.data;
    },
  });

  // Fetch statistics
  const { data: stats } = useQuery<AnalyticsStatistics>({
    queryKey: ['analytics-stats'],
    queryFn: async () => {
      const response = await api.get('/api/AnalyticsApi/statistics');
      return response.data;
    },
  });

  // Create report mutation
  const createMutation = useMutation({
    mutationFn: (data: CreateReportRequest) => api.post('/api/AnalyticsApi/reports', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analytics-reports'] });
      queryClient.invalidateQueries({ queryKey: ['analytics-stats'] });
      setIsModalOpen(false);
      resetForm();
      showToast('success', t('message.success.created', 'Report generation started successfully'));
    },
    onError: (error: Error) => {
      showToast('error', `${t('action.create', 'Create')} ${t('message.error.generic', 'failed')}: ${error.message}`);
    },
  });

  // Delete report mutation
  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/AnalyticsApi/reports/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analytics-reports'] });
      queryClient.invalidateQueries({ queryKey: ['analytics-stats'] });
      showToast('success', t('message.success.deleted', 'Report deleted successfully'));
    },
    onError: (error: Error) => {
      showToast('error', `${t('action.delete', 'Delete')} ${t('message.error.generic', 'failed')}: ${error.message}`);
    },
  });

  const resetForm = () => {
    setFormData({
      name: '',
      type: 0,
      parameters: {
        startDate: format(new Date(), 'yyyy-MM-dd'),
        endDate: format(new Date(), 'yyyy-MM-dd'),
        includeCharts: true,
        includeDetails: true,
      },
      description: '',
    });
  };

  const handleOpenModal = () => {
    resetForm();
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    resetForm();
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(formData);
  };

  const handleDelete = (report: Report) => {
    if (window.confirm(t('message.confirm.delete', `Are you sure you want to delete "${report.name}"?`))) {
      deleteMutation.mutate(report.id);
    }
  };

  const handleDownload = (report: Report) => {
    if (report.fileUrl) {
      window.open(report.fileUrl, '_blank');
    } else {
      showToast('error', t('message.error.noFile', 'No file available for download'));
    }
  };

  const getStatusBadge = (status: number) => {
    const statusConfig = {
      0: { label: t('status.pending', 'Pending'), class: 'text-yellow-600 bg-yellow-100' },
      1: { label: t('status.generating', 'Generating'), class: 'text-blue-600 bg-blue-100' },
      2: { label: t('status.completed', 'Completed'), class: 'text-green-600 bg-green-100' },
      3: { label: t('status.failed', 'Failed'), class: 'text-red-600 bg-red-100' },
    };
    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig[0];
    return <span className={`px-2 py-1 text-xs font-medium rounded-full ${config.class}`}>{config.label}</span>;
  };

  const getReportTypeLabel = (type: ReportType) => {
    const labels = [
      t('reportType.daily', 'Daily'),
      t('reportType.weekly', 'Weekly'),
      t('reportType.monthly', 'Monthly'),
      t('reportType.quarterly', 'Quarterly'),
      t('reportType.annual', 'Annual'),
      t('reportType.custom', 'Custom'),
    ];
    return labels[type] || 'Unknown';
  };

  // Filter reports
  const filteredReports = reports?.filter((report) => {
    const matchesSearch = report.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      report.description?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = selectedType === 'all' || report.type.toString() === selectedType;
    return matchesSearch && matchesType;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('page.analytics.reports', 'Analytics Reports')}</h1>
          <p className="text-gray-600 mt-1">{t('page.analytics.reportsDescription', 'Generate and manage analytics reports')}</p>
        </div>
        <button onClick={handleOpenModal} className="btn btn-primary">
          <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          {t('page.analytics.generateReport', 'Generate Report')}
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {[
          { label: t('stats.totalReports', 'Total Reports'), value: stats?.totalReports || 0, color: 'text-blue-600' },
          { label: t('stats.thisMonth', 'This Month'), value: stats?.reportsThisMonth || 0, color: 'text-green-600' },
          { label: t('stats.pending', 'Pending'), value: stats?.pendingReports || 0, color: 'text-yellow-600' },
          { label: t('stats.failed', 'Failed'), value: stats?.failedReports || 0, color: 'text-red-600' },
        ].map((stat, index) => (
          <div key={stat.label} className="card animate-fade-in" style={{ animationDelay: `${index * 50}ms` }}>
            <p className="text-sm text-gray-600">{stat.label}</p>
            <p className={`text-2xl font-bold ${stat.color} mt-1`}>{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div className="card animate-fade-in">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm text-gray-600 mb-1">{t('action.search', 'Search')}</label>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder={t('page.analytics.searchReports', 'Search reports by name or description...')}
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-600 mb-1">{t('field.reportType', 'Report Type')}</label>
            <select
              value={selectedType}
              onChange={(e) => setSelectedType(e.target.value)}
              className="input w-full"
            >
              <option value="all">{t('field.all', 'All Types')}</option>
              <option value="0">{t('reportType.daily', 'Daily')}</option>
              <option value="1">{t('reportType.weekly', 'Weekly')}</option>
              <option value="2">{t('reportType.monthly', 'Monthly')}</option>
              <option value="3">{t('reportType.quarterly', 'Quarterly')}</option>
              <option value="4">{t('reportType.annual', 'Annual')}</option>
              <option value="5">{t('reportType.custom', 'Custom')}</option>
            </select>
          </div>
        </div>
      </div>

      {/* Reports Table */}
      <div className="card overflow-hidden p-0 animate-fade-in">
        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-4">{t('message.loading', 'Loading reports...')}</p>
          </div>
        ) : filteredReports && filteredReports.length > 0 ? (
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
                    {t('field.dateRange', 'Date Range')}
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
                {filteredReports.map((report) => (
                  <tr key={report.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">{report.name}</div>
                      {report.description && (
                        <div className="text-sm text-gray-500">{report.description}</div>
                      )}
                      {report.generatedBy && (
                        <div className="text-xs text-gray-400 mt-1">
                          {t('field.by', 'By')}: {report.generatedBy}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {getReportTypeLabel(report.type)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {report.parameters.startDate && report.parameters.endDate ? (
                        <div>
                          <div>{format(new Date(report.parameters.startDate), 'MMM d, yyyy')}</div>
                          <div className="text-xs text-gray-400">
                            {t('field.to', 'to')} {format(new Date(report.parameters.endDate), 'MMM d, yyyy')}
                          </div>
                        </div>
                      ) : (
                        '-'
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(report.generatedAt), 'MMM d, yyyy HH:mm')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(report.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <div className="flex items-center gap-3">
                        {report.status === 2 && report.fileUrl && (
                          <button
                            onClick={() => handleDownload(report)}
                            className="text-primary-600 hover:text-primary-900 transition-colors"
                            title={t('action.download', 'Download')}
                          >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                            </svg>
                          </button>
                        )}
                        <button
                          className="text-gray-600 hover:text-gray-900 transition-colors"
                          title={t('action.view', 'View')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDelete(report)}
                          disabled={deleteMutation.isPending}
                          className="text-red-600 hover:text-red-900 disabled:opacity-50 transition-colors"
                          title={t('action.delete', 'Delete')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
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
          <div className="text-center py-12">
            <svg className="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <p className="text-gray-600 mb-2">
              {searchTerm || selectedType !== 'all'
                ? t('message.noResults', 'No reports found')
                : t('message.noReports', 'No reports generated yet')}
            </p>
            <p className="text-gray-500 text-sm">
              {t('message.getStarted', 'Get started by generating your first report')}
            </p>
          </div>
        )}
      </div>

      {/* Generate Report Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={t('page.analytics.generateReport', 'Generate New Report')}
        size="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('field.reportName', 'Report Name')} <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="input w-full"
              placeholder={t('field.reportNamePlaceholder', 'e.g., Monthly Patient Statistics')}
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('field.description', 'Description')}
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="input w-full"
              rows={3}
              placeholder={t('field.descriptionPlaceholder', 'Brief description of this report')}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('field.reportType', 'Report Type')} <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: Number(e.target.value) as ReportType })}
              className="input w-full"
              required
            >
              <option value={0}>{t('reportType.daily', 'Daily')}</option>
              <option value={1}>{t('reportType.weekly', 'Weekly')}</option>
              <option value={2}>{t('reportType.monthly', 'Monthly')}</option>
              <option value={3}>{t('reportType.quarterly', 'Quarterly')}</option>
              <option value={4}>{t('reportType.annual', 'Annual')}</option>
              <option value={5}>{t('reportType.custom', 'Custom')}</option>
            </select>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('field.startDate', 'Start Date')} <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                value={formData.parameters.startDate || ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    parameters: { ...formData.parameters, startDate: e.target.value },
                  })
                }
                className="input w-full"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('field.endDate', 'End Date')} <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                value={formData.parameters.endDate || ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    parameters: { ...formData.parameters, endDate: e.target.value },
                  })
                }
                className="input w-full"
                required
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('field.department', 'Department')}
            </label>
            <select
              value={formData.parameters.departmentId || ''}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  parameters: { ...formData.parameters, departmentId: e.target.value ? Number(e.target.value) : undefined },
                })
              }
              className="input w-full"
            >
              <option value="">{t('field.all', 'All Departments')}</option>
              <option value="1">{t('nav.laboratory', 'Laboratory')}</option>
              <option value="2">{t('nav.pharmacy', 'Pharmacy')}</option>
              <option value="3">{t('nav.radiology', 'Radiology')}</option>
              <option value="4">{t('nav.audiology', 'Audiology')}</option>
            </select>
          </div>

          <div className="space-y-2">
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.parameters.includeCharts || false}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    parameters: { ...formData.parameters, includeCharts: e.target.checked },
                  })
                }
                className="rounded border-gray-300 text-primary-600 focus:ring-primary-500 ltr:mr-2 rtl:ml-2"
              />
              <span className="text-sm text-gray-700">{t('field.includeCharts', 'Include Charts')}</span>
            </label>
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.parameters.includeDetails || false}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    parameters: { ...formData.parameters, includeDetails: e.target.checked },
                  })
                }
                className="rounded border-gray-300 text-primary-600 focus:ring-primary-500 ltr:mr-2 rtl:ml-2"
              />
              <span className="text-sm text-gray-700">{t('field.includeDetails', 'Include Detailed Data')}</span>
            </label>
          </div>

          <div className="flex justify-end gap-3 pt-4 border-t">
            <button
              type="button"
              onClick={handleCloseModal}
              className="btn btn-secondary"
              disabled={createMutation.isPending}
            >
              {t('action.cancel', 'Cancel')}
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={createMutation.isPending}
            >
              {createMutation.isPending ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white ltr:mr-2 rtl:ml-2"></div>
                  {t('status.generating', 'Generating...')}
                </>
              ) : (
                t('action.generate', 'Generate Report')
              )}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
