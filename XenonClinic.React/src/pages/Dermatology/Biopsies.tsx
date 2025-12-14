import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import type {
  SkinBiopsy,
  CreateSkinBiopsyRequest,
  UpdateBiopsyResultRequest,
  BiopsyStatus,
  BiopsyTechnique,
} from '../../types/dermatology';
import { dermatologyApi } from '../../lib/api';

interface BiopsiesProps {
  patientId?: number;
}

export const Biopsies = ({ patientId }: BiopsiesProps = {}) => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isResultModalOpen, setIsResultModalOpen] = useState(false);
  const [selectedBiopsy, setSelectedBiopsy] = useState<SkinBiopsy | undefined>(undefined);

  const { data: biopsies, isLoading } = useQuery<SkinBiopsy[]>({
    queryKey: ['skin-biopsies', patientId],
    queryFn: async () => {
      if (patientId) {
        const response = await dermatologyApi.getBiopsiesByPatient(patientId);
        return response.data?.data ?? response.data ?? [];
      }
      return [];
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateSkinBiopsyRequest) => dermatologyApi.createBiopsy(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-biopsies'] });
      setIsModalOpen(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<SkinBiopsy> }) =>
      dermatologyApi.updateBiopsy(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-biopsies'] });
      setIsResultModalOpen(false);
    },
  });

  const updateResultMutation = useMutation({
    mutationFn: (data: UpdateBiopsyResultRequest) => dermatologyApi.updateBiopsy(data.biopsyId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-biopsies'] });
      setIsResultModalOpen(false);
      setSelectedBiopsy(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dermatologyApi.deleteBiopsy(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-biopsies'] });
    },
  });

  const handleDelete = (biopsy: SkinBiopsy) => {
    if (window.confirm('Are you sure you want to delete this biopsy record?')) {
      deleteMutation.mutate(biopsy.id);
    }
  };

  const handleAddResult = (biopsy: SkinBiopsy) => {
    setSelectedBiopsy(biopsy);
    setIsResultModalOpen(true);
  };

  const handleCreate = () => {
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
  };

  const handleResultModalClose = () => {
    setIsResultModalOpen(false);
    setSelectedBiopsy(undefined);
  };

  const filteredBiopsies = biopsies
    ?.filter((biopsy) => {
      const matchesSearch =
        biopsy.patient?.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase()) ||
        biopsy.site.toLowerCase().includes(searchTerm.toLowerCase()) ||
        biopsy.indication.toLowerCase().includes(searchTerm.toLowerCase());

      const matchesStatus = statusFilter === 'all' || biopsy.status === statusFilter;

      return matchesSearch && matchesStatus;
    })
    .sort((a, b) => new Date(b.biopsyDate).getTime() - new Date(a.biopsyDate).getTime());

  const getStatusLabel = (status: BiopsyStatus) => {
    const labels: Record<BiopsyStatus, string> = {
      pending: 'Pending',
      processing: 'Processing',
      completed: 'Completed',
      requires_followup: 'Requires Follow-up',
    };
    return labels[status];
  };

  const getStatusColor = (status: BiopsyStatus) => {
    const colors: Record<BiopsyStatus, string> = {
      pending: 'text-yellow-600 bg-yellow-100',
      processing: 'text-blue-600 bg-blue-100',
      completed: 'text-green-600 bg-green-100',
      requires_followup: 'text-red-600 bg-red-100',
    };
    return colors[status];
  };

  const getTechniqueLabel = (technique: BiopsyTechnique) => {
    const labels: Record<BiopsyTechnique, string> = {
      shave: 'Shave Biopsy',
      punch: 'Punch Biopsy',
      excisional: 'Excisional Biopsy',
      incisional: 'Incisional Biopsy',
    };
    return labels[technique];
  };

  const pendingCount = biopsies?.filter((b) => b.status === 'pending').length || 0;
  const processingCount = biopsies?.filter((b) => b.status === 'processing').length || 0;
  const followUpCount = biopsies?.filter((b) => b.status === 'requires_followup').length || 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Skin Biopsies</h1>
          <p className="text-gray-600 mt-1">Manage biopsy procedures and pathology results</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Biopsy
        </button>
      </div>

      {/* Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Pending Results</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{pendingCount}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Processing</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{processingCount}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Requires Follow-up</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{followUpCount}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4 flex flex-col sm:flex-row gap-3">
          <input
            type="text"
            placeholder="Search by patient name, site, or indication..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input flex-1"
          />
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="input sm:w-48"
          >
            <option value="all">All Statuses</option>
            <option value="pending">Pending</option>
            <option value="processing">Processing</option>
            <option value="completed">Completed</option>
            <option value="requires_followup">Requires Follow-up</option>
          </select>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading biopsies...</p>
          </div>
        ) : filteredBiopsies && filteredBiopsies.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Biopsy Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Site
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Technique
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Result Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Performed By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredBiopsies.map((biopsy) => (
                  <tr key={biopsy.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {format(new Date(biopsy.biopsyDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {biopsy.patient?.fullNameEn || `Patient #${biopsy.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{biopsy.site}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {getTechniqueLabel(biopsy.technique)}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                          biopsy.status
                        )}`}
                      >
                        {getStatusLabel(biopsy.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {biopsy.resultDate
                        ? format(new Date(biopsy.resultDate), 'MMM d, yyyy')
                        : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{biopsy.performedBy}</td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        {biopsy.status !== 'completed' && (
                          <button
                            onClick={() => handleAddResult(biopsy)}
                            className="text-primary-600 hover:text-primary-800"
                          >
                            Add Result
                          </button>
                        )}
                        <button
                          onClick={() => handleDelete(biopsy)}
                          className="text-red-600 hover:text-red-800"
                        >
                          Delete
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
            {searchTerm || statusFilter !== 'all'
              ? 'No biopsies found matching your filters.'
              : 'No biopsies found.'}
          </div>
        )}
      </div>

      {/* Create Biopsy Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                New Biopsy
              </Dialog.Title>
              <BiopsyForm
                onSubmit={(data) => createMutation.mutate(data)}
                onCancel={handleModalClose}
                isSubmitting={createMutation.isPending}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* Add Result Modal */}
      <Dialog open={isResultModalOpen} onClose={handleResultModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Add Pathology Result
              </Dialog.Title>
              {selectedBiopsy && (
                <BiopsyResultForm
                  biopsy={selectedBiopsy}
                  onSubmit={(data) => updateResultMutation.mutate(data)}
                  onCancel={handleResultModalClose}
                  isSubmitting={updateResultMutation.isPending}
                />
              )}
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};

// Create Biopsy Form Component
interface BiopsyFormProps {
  onSubmit: (data: CreateSkinBiopsyRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const BiopsyForm = ({ onSubmit, onCancel, isSubmitting }: BiopsyFormProps) => {
  const [formData, setFormData] = useState({
    patientId: 0,
    biopsyDate: new Date().toISOString().split('T')[0],
    site: '',
    indication: '',
    technique: 'punch' as BiopsyTechnique,
    specimenDescription: '',
    performedBy: '',
    notes: '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      ...formData,
      specimenDescription: formData.specimenDescription || undefined,
      notes: formData.notes || undefined,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Patient ID</label>
          <input
            type="number"
            required
            value={formData.patientId || ''}
            onChange={(e) => setFormData({ ...formData, patientId: parseInt(e.target.value) })}
            className="input w-full"
          />
        </div>
        <div>
          <label className="label">Biopsy Date</label>
          <input
            type="date"
            required
            value={formData.biopsyDate}
            onChange={(e) => setFormData({ ...formData, biopsyDate: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Site</label>
          <input
            type="text"
            required
            value={formData.site}
            onChange={(e) => setFormData({ ...formData, site: e.target.value })}
            className="input w-full"
            placeholder="e.g., Left upper arm"
          />
        </div>
        <div>
          <label className="label">Technique</label>
          <select
            required
            value={formData.technique}
            onChange={(e) => setFormData({ ...formData, technique: e.target.value as BiopsyTechnique })}
            className="input w-full"
          >
            <option value="shave">Shave Biopsy</option>
            <option value="punch">Punch Biopsy</option>
            <option value="excisional">Excisional Biopsy</option>
            <option value="incisional">Incisional Biopsy</option>
          </select>
        </div>
      </div>

      <div>
        <label className="label">Clinical Indication</label>
        <textarea
          required
          value={formData.indication}
          onChange={(e) => setFormData({ ...formData, indication: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Reason for biopsy and clinical findings"
        />
      </div>

      <div>
        <label className="label">Specimen Description</label>
        <textarea
          value={formData.specimenDescription}
          onChange={(e) => setFormData({ ...formData, specimenDescription: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Description of specimen collected"
        />
      </div>

      <div>
        <label className="label">Performed By</label>
        <input
          type="text"
          required
          value={formData.performedBy}
          onChange={(e) => setFormData({ ...formData, performedBy: e.target.value })}
          className="input w-full"
          placeholder="Provider name"
        />
      </div>

      <div>
        <label className="label">Additional Notes</label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Any additional notes"
        />
      </div>

      <div className="flex items-center justify-end gap-3 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" disabled={isSubmitting} className="btn btn-primary">
          {isSubmitting ? 'Creating...' : 'Create Biopsy'}
        </button>
      </div>
    </form>
  );
};

// Biopsy Result Form Component
interface BiopsyResultFormProps {
  biopsy: SkinBiopsy;
  onSubmit: (data: UpdateBiopsyResultRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const BiopsyResultForm = ({ biopsy, onSubmit, onCancel, isSubmitting }: BiopsyResultFormProps) => {
  const [formData, setFormData] = useState({
    pathologyResult: biopsy.pathologyResult || '',
    diagnosis: biopsy.diagnosis || '',
    status: biopsy.status,
    resultDate: biopsy.resultDate
      ? biopsy.resultDate.split('T')[0]
      : new Date().toISOString().split('T')[0],
    followUpRequired: biopsy.followUpRequired || false,
    followUpDate: biopsy.followUpDate ? biopsy.followUpDate.split('T')[0] : '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      biopsyId: biopsy.id,
      ...formData,
      diagnosis: formData.diagnosis || undefined,
      followUpRequired: formData.followUpRequired || undefined,
      followUpDate: formData.followUpDate || undefined,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="bg-gray-50 p-3 rounded-lg mb-4">
        <p className="text-sm text-gray-600">Patient: <span className="font-medium text-gray-900">{biopsy.patient?.fullNameEn || `#${biopsy.patientId}`}</span></p>
        <p className="text-sm text-gray-600">Site: <span className="font-medium text-gray-900">{biopsy.site}</span></p>
        <p className="text-sm text-gray-600">Biopsy Date: <span className="font-medium text-gray-900">{format(new Date(biopsy.biopsyDate), 'MMMM d, yyyy')}</span></p>
      </div>

      <div>
        <label className="label">Pathology Result</label>
        <textarea
          required
          value={formData.pathologyResult}
          onChange={(e) => setFormData({ ...formData, pathologyResult: e.target.value })}
          className="input w-full"
          rows={4}
          placeholder="Detailed pathology findings"
        />
      </div>

      <div>
        <label className="label">Diagnosis</label>
        <input
          type="text"
          value={formData.diagnosis}
          onChange={(e) => setFormData({ ...formData, diagnosis: e.target.value })}
          className="input w-full"
          placeholder="Final diagnosis"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Status</label>
          <select
            required
            value={formData.status}
            onChange={(e) => setFormData({ ...formData, status: e.target.value as BiopsyStatus })}
            className="input w-full"
          >
            <option value="processing">Processing</option>
            <option value="completed">Completed</option>
            <option value="requires_followup">Requires Follow-up</option>
          </select>
        </div>
        <div>
          <label className="label">Result Date</label>
          <input
            type="date"
            required
            value={formData.resultDate}
            onChange={(e) => setFormData({ ...formData, resultDate: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="flex items-center">
        <input
          type="checkbox"
          id="followUpRequired"
          checked={formData.followUpRequired}
          onChange={(e) => setFormData({ ...formData, followUpRequired: e.target.checked })}
          className="mr-2"
        />
        <label htmlFor="followUpRequired" className="text-sm text-gray-700">
          Follow-up required
        </label>
      </div>

      {formData.followUpRequired && (
        <div>
          <label className="label">Follow-up Date</label>
          <input
            type="date"
            value={formData.followUpDate}
            onChange={(e) => setFormData({ ...formData, followUpDate: e.target.value })}
            className="input w-full"
          />
        </div>
      )}

      <div className="flex items-center justify-end gap-3 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" disabled={isSubmitting} className="btn btn-primary">
          {isSubmitting ? 'Saving...' : 'Save Result'}
        </button>
      </div>
    </form>
  );
};
