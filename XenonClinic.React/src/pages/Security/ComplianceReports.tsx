import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  ClipboardDocumentCheckIcon,
  PlusIcon,
  ArrowDownTrayIcon,
  EyeIcon,
  DocumentTextIcon,
  CheckCircleIcon,
  XCircleIcon,
  ExclamationTriangleIcon,
  MinusCircleIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';
import type { ComplianceReport, ComplianceFinding } from '../../types/security';

const reportTypeColors: Record<ComplianceReport['reportType'], string> = {
  HIPAA: 'bg-blue-100 text-blue-800',
  GDPR: 'bg-purple-100 text-purple-800',
  SOC2: 'bg-green-100 text-green-800',
  ISO27001: 'bg-orange-100 text-orange-800',
  Custom: 'bg-gray-100 text-gray-800',
};

const statusConfig = {
  draft: { label: 'Draft', color: 'bg-gray-100 text-gray-800' },
  pending: { label: 'Pending Review', color: 'bg-yellow-100 text-yellow-800' },
  approved: { label: 'Approved', color: 'bg-green-100 text-green-800' },
  published: { label: 'Published', color: 'bg-blue-100 text-blue-800' },
};

const findingStatusConfig = {
  compliant: { label: 'Compliant', color: 'text-green-600', icon: CheckCircleIcon },
  'non-compliant': { label: 'Non-Compliant', color: 'text-red-600', icon: XCircleIcon },
  partial: { label: 'Partial', color: 'text-yellow-600', icon: ExclamationTriangleIcon },
  'not-applicable': { label: 'N/A', color: 'text-gray-400', icon: MinusCircleIcon },
};

export function ComplianceReports() {
  const [selectedReport, setSelectedReport] = useState<ComplianceReport | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: reports = [], isLoading } = useQuery({
    queryKey: ['compliance-reports'],
    queryFn: () => api.get<ComplianceReport[]>('/api/security/compliance-reports'),
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getScoreColor = (score: number) => {
    if (score >= 90) return 'text-green-600';
    if (score >= 70) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getScoreBgColor = (score: number) => {
    if (score >= 90) return 'bg-green-500';
    if (score >= 70) return 'bg-yellow-500';
    return 'bg-red-500';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Compliance Reports</h1>
          <p className="mt-1 text-sm text-gray-500">
            Generate and manage regulatory compliance reports
          </p>
        </div>
        <button
          onClick={() => setIsCreateModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Generate Report
        </button>
      </div>

      {/* Compliance Overview Cards */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {['HIPAA', 'GDPR', 'SOC2', 'ISO27001'].map((type) => {
          const typeReports = reports.filter((r) => r.reportType === type);
          const latestReport = typeReports[0];

          return (
            <div key={type} className="rounded-lg bg-white p-6 shadow">
              <div className="flex items-center justify-between">
                <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${reportTypeColors[type as ComplianceReport['reportType']]}`}>
                  {type}
                </span>
                {latestReport && (
                  <span className={`text-2xl font-bold ${getScoreColor(latestReport.complianceScore)}`}>
                    {latestReport.complianceScore}%
                  </span>
                )}
              </div>
              {latestReport ? (
                <div className="mt-4">
                  <div className="h-2 w-full bg-gray-200 rounded-full">
                    <div
                      className={`h-2 rounded-full ${getScoreBgColor(latestReport.complianceScore)}`}
                      style={{ width: `${latestReport.complianceScore}%` }}
                    />
                  </div>
                  <p className="mt-2 text-xs text-gray-500">
                    Last assessed: {formatDate(latestReport.generatedAt)}
                  </p>
                </div>
              ) : (
                <p className="mt-4 text-sm text-gray-400">No reports yet</p>
              )}
            </div>
          );
        })}
      </div>

      {/* Reports List */}
      <div className="rounded-lg bg-white shadow">
        <div className="border-b border-gray-200 px-6 py-4">
          <h3 className="text-lg font-medium text-gray-900">All Reports</h3>
        </div>

        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : reports.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ClipboardDocumentCheckIcon className="h-12 w-12 mb-2" />
            <p>No compliance reports generated yet</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Report
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Period
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Score
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Generated
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {reports.map((report) => {
                const status = statusConfig[report.status];

                return (
                  <tr key={report.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="flex items-center">
                        <DocumentTextIcon className="h-5 w-5 text-gray-400 mr-3" />
                        <div className="font-medium text-gray-900">{report.reportName}</div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${reportTypeColors[report.reportType]}`}>
                        {report.reportType}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {report.period}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="flex items-center">
                        <span className={`text-lg font-semibold ${getScoreColor(report.complianceScore)}`}>
                          {report.complianceScore}%
                        </span>
                        <div className="ml-2 w-16 h-2 bg-gray-200 rounded-full">
                          <div
                            className={`h-2 rounded-full ${getScoreBgColor(report.complianceScore)}`}
                            style={{ width: `${report.complianceScore}%` }}
                          />
                        </div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}>
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      <div>{formatDate(report.generatedAt)}</div>
                      <div className="text-xs text-gray-400">by {report.generatedBy}</div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <button
                        onClick={() => setSelectedReport(report)}
                        className="mr-3 text-blue-600 hover:text-blue-900"
                      >
                        <EyeIcon className="h-5 w-5" />
                      </button>
                      <button className="text-gray-600 hover:text-gray-900">
                        <ArrowDownTrayIcon className="h-5 w-5" />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Report Detail Modal */}
      <ReportDetailModal
        report={selectedReport}
        onClose={() => setSelectedReport(null)}
      />

      {/* Create Report Modal */}
      <CreateReportModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      />
    </div>
  );
}

function ReportDetailModal({
  report,
  onClose,
}: {
  report: ComplianceReport | null;
  onClose: () => void;
}) {
  if (!report) return null;

  const groupedFindings = report.findings.reduce((acc, finding) => {
    if (!acc[finding.category]) {
      acc[finding.category] = [];
    }
    acc[finding.category].push(finding);
    return acc;
  }, {} as Record<string, ComplianceFinding[]>);

  const compliantCount = report.findings.filter((f) => f.status === 'compliant').length;
  const nonCompliantCount = report.findings.filter((f) => f.status === 'non-compliant').length;
  const partialCount = report.findings.filter((f) => f.status === 'partial').length;

  return (
    <Dialog open={!!report} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-4xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
          {/* Header */}
          <div className="border-b border-gray-200 px-6 py-4">
            <div className="flex items-start justify-between">
              <div>
                <div className="flex items-center gap-2">
                  <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${reportTypeColors[report.reportType]}`}>
                    {report.reportType}
                  </span>
                  <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[report.status].color}`}>
                    {statusConfig[report.status].label}
                  </span>
                </div>
                <Dialog.Title className="text-xl font-semibold text-gray-900 mt-2">
                  {report.reportName}
                </Dialog.Title>
                <p className="text-sm text-gray-500">Period: {report.period}</p>
              </div>
              <div className="text-right">
                <div className={`text-4xl font-bold ${report.complianceScore >= 90 ? 'text-green-600' : report.complianceScore >= 70 ? 'text-yellow-600' : 'text-red-600'}`}>
                  {report.complianceScore}%
                </div>
                <p className="text-sm text-gray-500">Compliance Score</p>
              </div>
            </div>
          </div>

          <div className="p-6">
            {/* Summary Stats */}
            <div className="grid grid-cols-4 gap-4 mb-6">
              <div className="bg-gray-50 rounded-lg p-4 text-center">
                <div className="text-2xl font-bold text-gray-900">{report.findings.length}</div>
                <div className="text-sm text-gray-500">Total Requirements</div>
              </div>
              <div className="bg-green-50 rounded-lg p-4 text-center">
                <div className="text-2xl font-bold text-green-600">{compliantCount}</div>
                <div className="text-sm text-green-700">Compliant</div>
              </div>
              <div className="bg-yellow-50 rounded-lg p-4 text-center">
                <div className="text-2xl font-bold text-yellow-600">{partialCount}</div>
                <div className="text-sm text-yellow-700">Partial</div>
              </div>
              <div className="bg-red-50 rounded-lg p-4 text-center">
                <div className="text-2xl font-bold text-red-600">{nonCompliantCount}</div>
                <div className="text-sm text-red-700">Non-Compliant</div>
              </div>
            </div>

            {/* Findings by Category */}
            <div className="space-y-6">
              {Object.entries(groupedFindings).map(([category, findings]) => (
                <div key={category} className="border rounded-lg">
                  <div className="bg-gray-50 px-4 py-3 border-b">
                    <h4 className="font-medium text-gray-900">{category}</h4>
                  </div>
                  <div className="divide-y">
                    {findings.map((finding) => {
                      const statusInfo = findingStatusConfig[finding.status];
                      const StatusIcon = statusInfo.icon;

                      return (
                        <div key={finding.id} className="px-4 py-3">
                          <div className="flex items-start justify-between">
                            <div className="flex-1">
                              <div className="flex items-center gap-2">
                                <StatusIcon className={`h-5 w-5 ${statusInfo.color}`} />
                                <span className="font-medium text-gray-900">{finding.requirement}</span>
                                <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                                  finding.priority === 'critical' ? 'bg-red-100 text-red-800' :
                                  finding.priority === 'high' ? 'bg-orange-100 text-orange-800' :
                                  finding.priority === 'medium' ? 'bg-yellow-100 text-yellow-800' :
                                  'bg-gray-100 text-gray-800'
                                }`}>
                                  {finding.priority}
                                </span>
                              </div>
                              {finding.evidence && (
                                <p className="mt-1 text-sm text-gray-600 ml-7">{finding.evidence}</p>
                              )}
                              {finding.remediation && finding.status !== 'compliant' && (
                                <div className="mt-2 ml-7 bg-blue-50 rounded p-2">
                                  <p className="text-sm text-blue-800">
                                    <span className="font-medium">Remediation:</span> {finding.remediation}
                                  </p>
                                  {finding.dueDate && (
                                    <p className="text-xs text-blue-600 mt-1">
                                      Due: {new Date(finding.dueDate).toLocaleDateString()}
                                    </p>
                                  )}
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="border-t border-gray-200 px-6 py-4 flex justify-between">
            <div className="text-sm text-gray-500">
              Generated by {report.generatedBy} on {new Date(report.generatedAt).toLocaleString()}
              {report.approvedBy && (
                <span className="ml-4">• Approved by {report.approvedBy}</span>
              )}
            </div>
            <button
              onClick={onClose}
              className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
            >
              Close
            </button>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}

function CreateReportModal({
  isOpen,
  onClose,
}: {
  isOpen: boolean;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    reportName: '',
    reportType: 'HIPAA' as ComplianceReport['reportType'],
    period: '',
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      api.post('/api/security/compliance-reports/generate', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['compliance-reports'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-md w-full rounded-lg bg-white p-6 shadow-xl">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            Generate Compliance Report
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Report Name</label>
              <input
                type="text"
                required
                value={formData.reportName}
                onChange={(e) => setFormData({ ...formData, reportName: e.target.value })}
                placeholder="Q4 2024 HIPAA Assessment"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Compliance Framework</label>
              <select
                value={formData.reportType}
                onChange={(e) => setFormData({ ...formData, reportType: e.target.value as ComplianceReport['reportType'] })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                <option value="HIPAA">HIPAA</option>
                <option value="GDPR">GDPR</option>
                <option value="SOC2">SOC2</option>
                <option value="ISO27001">ISO 27001</option>
                <option value="Custom">Custom</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Assessment Period</label>
              <input
                type="text"
                required
                value={formData.period}
                onChange={(e) => setFormData({ ...formData, period: e.target.value })}
                placeholder="Q4 2024"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={onClose}
                className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={mutation.isPending}
                className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {mutation.isPending ? 'Generating...' : 'Generate Report'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
