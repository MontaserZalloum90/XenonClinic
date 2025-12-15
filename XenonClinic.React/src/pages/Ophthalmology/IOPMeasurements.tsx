import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { IOPMeasurement, CreateIOPMeasurementRequest, IOPMethod } from '../../types/ophthalmology';
import { IOPMethod as IOPMethodEnum } from '../../types/ophthalmology';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

// Mock API - Replace with actual API when backend is ready
const iopApi = {
  getAll: () => Promise.resolve({ data: [] as IOPMeasurement[] }),
  create: (data: CreateIOPMeasurementRequest) => Promise.resolve({ data: { id: 1, ...data } }),
  update: (id: number, data: Partial<CreateIOPMeasurementRequest>) =>
    Promise.resolve({ data: { id, ...data } }),
  delete: () => Promise.resolve({ data: { success: true } }),
};

export const IOPMeasurements = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedMeasurement, setSelectedMeasurement] = useState<IOPMeasurement | undefined>(undefined);

  const { data: measurements, isLoading } = useQuery<IOPMeasurement[]>({
    queryKey: ['iop-measurements'],
    queryFn: async () => {
      const response = await iopApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateIOPMeasurementRequest) => iopApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['iop-measurements'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
      setIsModalOpen(false);
      setSelectedMeasurement(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateIOPMeasurementRequest> }) =>
      iopApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['iop-measurements'] });
      setIsModalOpen(false);
      setSelectedMeasurement(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => iopApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['iop-measurements'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
    },
  });

  const handleDelete = (measurement: IOPMeasurement) => {
    if (window.confirm('Are you sure you want to delete this IOP measurement?')) {
      deleteMutation.mutate(measurement.id);
    }
  };

  const handleEdit = (measurement: IOPMeasurement) => {
    setSelectedMeasurement(measurement);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedMeasurement(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedMeasurement(undefined);
  };

  const filteredMeasurements = measurements?.filter(
    (measurement) =>
      measurement.patient?.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase()) ||
      measurement.performedBy.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getIOPStatus = (iop: number) => {
    if (iop < 10) return { label: 'Low', color: 'text-blue-600 bg-blue-100' };
    if (iop <= 21) return { label: 'Normal', color: 'text-green-600 bg-green-100' };
    if (iop <= 25) return { label: 'Borderline', color: 'text-yellow-600 bg-yellow-100' };
    return { label: 'Elevated', color: 'text-red-600 bg-red-100' };
  };

  const getMethodLabel = (method: IOPMethod) => {
    const labels = {
      [IOPMethodEnum.GoldmannApplanation]: 'Goldmann Applanation',
      [IOPMethodEnum.Tonopen]: 'Tonopen',
      [IOPMethodEnum.NonContact]: 'Non-Contact',
      [IOPMethodEnum.Rebound]: 'Rebound',
      [IOPMethodEnum.Other]: 'Other',
    };
    return labels[method] || method;
  };

  const highIOPCount =
    measurements?.filter((m) => m.rightEye > 21 || m.leftEye > 21).length || 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">IOP Measurements</h1>
          <p className="text-gray-600 mt-1">Intraocular pressure monitoring</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Measurement
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Measurements</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{measurements?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">This Month</p>
          <p className="text-2xl font-bold text-indigo-600 mt-1">
            {measurements?.filter((m) => {
              const measurementDate = new Date(m.date);
              const now = new Date();
              return (
                measurementDate.getMonth() === now.getMonth() &&
                measurementDate.getFullYear() === now.getFullYear()
              );
            }).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Elevated IOP Cases</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{highIOPCount}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Filtered Results</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{filteredMeasurements?.length || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search by patient name or examiner..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading measurements...</p>
          </div>
        ) : filteredMeasurements && filteredMeasurements.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Time</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Right Eye (mmHg)
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Left Eye (mmHg)
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Method</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Performed By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredMeasurements.map((measurement) => {
                  const rightStatus = getIOPStatus(measurement.rightEye);
                  const leftStatus = getIOPStatus(measurement.leftEye);
                  return (
                    <tr key={measurement.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm text-gray-900">
                        {format(new Date(measurement.date), 'MMM d, yyyy')}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{measurement.time}</td>
                      <td className="px-4 py-3 text-sm font-medium text-gray-900">
                        {measurement.patient?.fullNameEn || `Patient #${measurement.patientId}`}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-2">
                          <span className="font-semibold">{measurement.rightEye}</span>
                          <span
                            className={`px-2 py-1 rounded-full text-xs font-medium ${rightStatus.color}`}
                          >
                            {rightStatus.label}
                          </span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-2">
                          <span className="font-semibold">{measurement.leftEye}</span>
                          <span
                            className={`px-2 py-1 rounded-full text-xs font-medium ${leftStatus.color}`}
                          >
                            {leftStatus.label}
                          </span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {getMethodLabel(measurement.method)}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{measurement.performedBy}</td>
                      <td className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleEdit(measurement)}
                            className="text-primary-600 hover:text-primary-800"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDelete(measurement)}
                            className="text-red-600 hover:text-red-800"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm ? 'No measurements found matching your search.' : 'No IOP measurements found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedMeasurement ? 'Edit IOP Measurement' : 'New IOP Measurement'}
              </Dialog.Title>
              <IOPForm
                measurement={selectedMeasurement}
                onSubmit={(data) => {
                  if (selectedMeasurement) {
                    updateMutation.mutate({ id: selectedMeasurement.id, data });
                  } else {
                    createMutation.mutate(data);
                  }
                }}
                onCancel={handleModalClose}
                isSubmitting={createMutation.isPending || updateMutation.isPending}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};

// Form Component
interface IOPFormProps {
  measurement?: IOPMeasurement;
  onSubmit: (data: CreateIOPMeasurementRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const IOPForm = ({ measurement, onSubmit, onCancel, isSubmitting }: IOPFormProps) => {
  const [formData, setFormData] = useState<CreateIOPMeasurementRequest>({
    patientId: measurement?.patientId || 0,
    date: measurement?.date ? format(new Date(measurement.date), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
    time: measurement?.time || format(new Date(), 'HH:mm'),
    rightEye: measurement?.rightEye || 0,
    leftEye: measurement?.leftEye || 0,
    method: measurement?.method || IOPMethodEnum.GoldmannApplanation,
    performedBy: measurement?.performedBy || '',
    notes: measurement?.notes || '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Patient ID</label>
          <input
            type="number"
            required
            value={formData.patientId || ''}
            onChange={(e) => setFormData({ ...formData, patientId: parseInt(e.target.value) || 0 })}
            className="input w-full"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Measurement Date</label>
          <input
            type="date"
            required
            value={formData.date}
            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Time</label>
          <input
            type="time"
            required
            value={formData.time}
            onChange={(e) => setFormData({ ...formData, time: e.target.value })}
            className="input w-full"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Method</label>
          <select
            required
            value={formData.method}
            onChange={(e) => setFormData({ ...formData, method: e.target.value as IOPMethod })}
            className="input w-full"
          >
            <option value={IOPMethodEnum.GoldmannApplanation}>Goldmann Applanation</option>
            <option value={IOPMethodEnum.Tonopen}>Tonopen</option>
            <option value={IOPMethodEnum.NonContact}>Non-Contact</option>
            <option value={IOPMethodEnum.Rebound}>Rebound</option>
            <option value={IOPMethodEnum.Other}>Other</option>
          </select>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Right Eye (mmHg)</label>
          <input
            type="number"
            step="0.1"
            min="0"
            max="50"
            required
            value={formData.rightEye}
            onChange={(e) => setFormData({ ...formData, rightEye: parseFloat(e.target.value) || 0 })}
            className="input w-full"
          />
          <p className="text-xs text-gray-500 mt-1">Normal range: 10-21 mmHg</p>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Left Eye (mmHg)</label>
          <input
            type="number"
            step="0.1"
            min="0"
            max="50"
            required
            value={formData.leftEye}
            onChange={(e) => setFormData({ ...formData, leftEye: parseFloat(e.target.value) || 0 })}
            className="input w-full"
          />
          <p className="text-xs text-gray-500 mt-1">Normal range: 10-21 mmHg</p>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Performed By</label>
        <input
          type="text"
          required
          value={formData.performedBy}
          onChange={(e) => setFormData({ ...formData, performedBy: e.target.value })}
          className="input w-full"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
        <textarea
          rows={3}
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="input w-full"
        />
      </div>

      <div className="flex justify-end gap-3 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary" disabled={isSubmitting}>
          Cancel
        </button>
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : measurement ? 'Update Measurement' : 'Create Measurement'}
        </button>
      </div>
    </form>
  );
};
