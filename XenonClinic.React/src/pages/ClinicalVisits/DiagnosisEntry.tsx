import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  ClipboardDocumentCheckIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  TrashIcon,
  PencilIcon,
  BookOpenIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';

interface Diagnosis {
  id: number;
  visitId: number;
  patientId: number;
  patientName: string;
  patientMRN: string;
  icdCode: string;
  icdDescription: string;
  diagnosisType: 'primary' | 'secondary' | 'admission' | 'discharge';
  severity: 'mild' | 'moderate' | 'severe' | 'critical';
  status: 'active' | 'resolved' | 'chronic' | 'in-remission';
  onsetDate?: string;
  clinicalNotes?: string;
  diagnosedBy: string;
  diagnosedAt: string;
  updatedAt?: string;
}

interface ICDCode {
  code: string;
  description: string;
  category: string;
}

const diagnosisTypeConfig = {
  primary: { label: 'Primary', color: 'bg-blue-100 text-blue-800' },
  secondary: { label: 'Secondary', color: 'bg-gray-100 text-gray-800' },
  admission: { label: 'Admission', color: 'bg-purple-100 text-purple-800' },
  discharge: { label: 'Discharge', color: 'bg-green-100 text-green-800' },
};

const severityConfig = {
  mild: { label: 'Mild', color: 'bg-green-100 text-green-800' },
  moderate: { label: 'Moderate', color: 'bg-yellow-100 text-yellow-800' },
  severe: { label: 'Severe', color: 'bg-orange-100 text-orange-800' },
  critical: { label: 'Critical', color: 'bg-red-100 text-red-800' },
};

const statusConfig = {
  active: { label: 'Active', color: 'bg-red-100 text-red-800' },
  resolved: { label: 'Resolved', color: 'bg-green-100 text-green-800' },
  chronic: { label: 'Chronic', color: 'bg-purple-100 text-purple-800' },
  'in-remission': { label: 'In Remission', color: 'bg-blue-100 text-blue-800' },
};

export function DiagnosisEntry() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedDiagnosis, setSelectedDiagnosis] = useState<Diagnosis | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<Diagnosis['status'] | 'all'>('all');
  const queryClient = useQueryClient();

  const { data: diagnoses = [], isLoading } = useQuery({
    queryKey: ['diagnoses'],
    queryFn: () => api.get<Diagnosis[]>('/api/clinical/diagnoses'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/clinical/diagnoses/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diagnoses'] });
    },
  });

  const filteredDiagnoses = diagnoses.filter((dx) => {
    const matchesSearch =
      dx.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      dx.icdCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
      dx.icdDescription.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || dx.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const handleEdit = (diagnosis: Diagnosis) => {
    setSelectedDiagnosis(diagnosis);
    setIsModalOpen(true);
  };

  const handleDelete = (diagnosis: Diagnosis) => {
    if (confirm(`Delete diagnosis "${diagnosis.icdCode}: ${diagnosis.icdDescription}"?`)) {
      deleteMutation.mutate(diagnosis.id);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Diagnosis Entry</h1>
          <p className="mt-1 text-sm text-gray-500">
            Record and manage patient diagnoses with ICD-10 coding
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedDiagnosis(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Add Diagnosis
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">{diagnoses.length}</div>
          <div className="text-sm text-gray-500">Total Diagnoses</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {diagnoses.filter((d) => d.status === 'active').length}
          </div>
          <div className="text-sm text-gray-500">Active</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-purple-600">
            {diagnoses.filter((d) => d.status === 'chronic').length}
          </div>
          <div className="text-sm text-gray-500">Chronic</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {diagnoses.filter((d) => d.status === 'resolved').length}
          </div>
          <div className="text-sm text-gray-500">Resolved</div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by patient, ICD code, or description..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2 flex-wrap">
          {(['all', 'active', 'chronic', 'resolved', 'in-remission'] as const).map((status) => (
            <button
              key={status}
              onClick={() => setStatusFilter(status)}
              className={`rounded-md px-3 py-1.5 text-sm font-medium ${
                statusFilter === status
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {status === 'all' ? 'All' : statusConfig[status].label}
            </button>
          ))}
        </div>
      </div>

      {/* Diagnoses Table */}
      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredDiagnoses.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ClipboardDocumentCheckIcon className="h-12 w-12 mb-2" />
            <p>No diagnoses found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  ICD Code / Description
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Severity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Date
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredDiagnoses.map((dx) => (
                <tr key={dx.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4">
                    <div>
                      <div className="font-medium text-gray-900">{dx.patientName}</div>
                      <div className="text-sm text-gray-500">MRN: {dx.patientMRN}</div>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div>
                      <div className="font-mono font-medium text-blue-600">{dx.icdCode}</div>
                      <div className="text-sm text-gray-900 max-w-xs truncate" title={dx.icdDescription}>
                        {dx.icdDescription}
                      </div>
                    </div>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4">
                    <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${diagnosisTypeConfig[dx.diagnosisType].color}`}>
                      {diagnosisTypeConfig[dx.diagnosisType].label}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4">
                    <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${severityConfig[dx.severity].color}`}>
                      {severityConfig[dx.severity].label}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4">
                    <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[dx.status].color}`}>
                      {statusConfig[dx.status].label}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    <div>{formatDate(dx.diagnosedAt)}</div>
                    <div className="text-xs text-gray-400">by {dx.diagnosedBy}</div>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                    <button
                      onClick={() => handleEdit(dx)}
                      className="mr-3 text-blue-600 hover:text-blue-900"
                    >
                      <PencilIcon className="h-5 w-5" />
                    </button>
                    <button
                      onClick={() => handleDelete(dx)}
                      className="text-red-600 hover:text-red-900"
                    >
                      <TrashIcon className="h-5 w-5" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Diagnosis Modal */}
      <DiagnosisModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        diagnosis={selectedDiagnosis}
      />
    </div>
  );
}

function DiagnosisModal({
  isOpen,
  onClose,
  diagnosis,
}: {
  isOpen: boolean;
  onClose: () => void;
  diagnosis: Diagnosis | null;
}) {
  const queryClient = useQueryClient();
  const [icdSearch, setIcdSearch] = useState('');
  const [showIcdSearch, setShowIcdSearch] = useState(false);
  const [formData, setFormData] = useState({
    patientId: diagnosis?.patientId || 0,
    patientName: diagnosis?.patientName || '',
    patientMRN: diagnosis?.patientMRN || '',
    icdCode: diagnosis?.icdCode || '',
    icdDescription: diagnosis?.icdDescription || '',
    diagnosisType: diagnosis?.diagnosisType || 'primary',
    severity: diagnosis?.severity || 'moderate',
    status: diagnosis?.status || 'active',
    onsetDate: diagnosis?.onsetDate?.split('T')[0] || '',
    clinicalNotes: diagnosis?.clinicalNotes || '',
  });

  const { data: icdCodes = [] } = useQuery({
    queryKey: ['icd-codes', icdSearch],
    queryFn: () => api.get<ICDCode[]>(`/api/reference/icd-codes?search=${icdSearch}`),
    enabled: icdSearch.length >= 2,
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      diagnosis
        ? api.put(`/api/clinical/diagnoses/${diagnosis.id}`, data)
        : api.post('/api/clinical/diagnoses', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diagnoses'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  const selectIcdCode = (code: ICDCode) => {
    setFormData({ ...formData, icdCode: code.code, icdDescription: code.description });
    setShowIcdSearch(false);
    setIcdSearch('');
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {diagnosis ? 'Edit Diagnosis' : 'Add New Diagnosis'}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Patient Info */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Patient Name</label>
                <input
                  type="text"
                  required
                  value={formData.patientName}
                  onChange={(e) => setFormData({ ...formData, patientName: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">MRN</label>
                <input
                  type="text"
                  required
                  value={formData.patientMRN}
                  onChange={(e) => setFormData({ ...formData, patientMRN: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                />
              </div>
            </div>

            {/* ICD Code */}
            <div>
              <label className="block text-sm font-medium text-gray-700">ICD-10 Code</label>
              <div className="relative mt-1">
                <div className="flex gap-2">
                  <input
                    type="text"
                    required
                    value={formData.icdCode}
                    onChange={(e) => setFormData({ ...formData, icdCode: e.target.value })}
                    className="block w-32 rounded-md border border-gray-300 px-3 py-2 font-mono focus:border-blue-500 focus:outline-none"
                    placeholder="E11.9"
                  />
                  <input
                    type="text"
                    required
                    value={formData.icdDescription}
                    onChange={(e) => setFormData({ ...formData, icdDescription: e.target.value })}
                    className="block flex-1 rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                    placeholder="Description"
                  />
                  <button
                    type="button"
                    onClick={() => setShowIcdSearch(!showIcdSearch)}
                    className="rounded-md border border-gray-300 px-3 py-2 text-gray-600 hover:bg-gray-50"
                  >
                    <BookOpenIcon className="h-5 w-5" />
                  </button>
                </div>

                {showIcdSearch && (
                  <div className="absolute z-10 mt-1 w-full rounded-md bg-white border border-gray-300 shadow-lg">
                    <input
                      type="text"
                      value={icdSearch}
                      onChange={(e) => setIcdSearch(e.target.value)}
                      placeholder="Search ICD codes..."
                      className="w-full border-b px-3 py-2 focus:outline-none"
                    />
                    <div className="max-h-48 overflow-y-auto">
                      {icdCodes.map((code) => (
                        <button
                          key={code.code}
                          type="button"
                          onClick={() => selectIcdCode(code)}
                          className="w-full px-3 py-2 text-left hover:bg-gray-100"
                        >
                          <span className="font-mono text-blue-600">{code.code}</span>
                          <span className="ml-2 text-sm text-gray-700">{code.description}</span>
                        </button>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Classification */}
            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Type</label>
                <select
                  value={formData.diagnosisType}
                  onChange={(e) => setFormData({ ...formData, diagnosisType: e.target.value as Diagnosis['diagnosisType'] })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                >
                  <option value="primary">Primary</option>
                  <option value="secondary">Secondary</option>
                  <option value="admission">Admission</option>
                  <option value="discharge">Discharge</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Severity</label>
                <select
                  value={formData.severity}
                  onChange={(e) => setFormData({ ...formData, severity: e.target.value as Diagnosis['severity'] })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                >
                  <option value="mild">Mild</option>
                  <option value="moderate">Moderate</option>
                  <option value="severe">Severe</option>
                  <option value="critical">Critical</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Status</label>
                <select
                  value={formData.status}
                  onChange={(e) => setFormData({ ...formData, status: e.target.value as Diagnosis['status'] })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                >
                  <option value="active">Active</option>
                  <option value="chronic">Chronic</option>
                  <option value="resolved">Resolved</option>
                  <option value="in-remission">In Remission</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Onset Date</label>
              <input
                type="date"
                value={formData.onsetDate}
                onChange={(e) => setFormData({ ...formData, onsetDate: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Clinical Notes</label>
              <textarea
                rows={3}
                value={formData.clinicalNotes}
                onChange={(e) => setFormData({ ...formData, clinicalNotes: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                placeholder="Additional clinical observations..."
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
                {mutation.isPending ? 'Saving...' : diagnosis ? 'Update' : 'Add Diagnosis'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
