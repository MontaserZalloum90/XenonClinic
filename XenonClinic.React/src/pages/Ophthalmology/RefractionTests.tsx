import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { RefractionTest, CreateRefractionTestRequest } from '../../types/ophthalmology';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

// Mock API - Replace with actual API when backend is ready
const refractionApi = {
  getAll: () => Promise.resolve({ data: [] as RefractionTest[] }),
  create: (data: CreateRefractionTestRequest) => Promise.resolve({ data: { id: 1, ...data } }),
  update: (id: number, data: Partial<CreateRefractionTestRequest>) =>
    Promise.resolve({ data: { id, ...data } }),
  delete: () => Promise.resolve({ data: { success: true } }),
};

export const RefractionTests = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTest, setSelectedTest] = useState<RefractionTest | undefined>(undefined);

  const { data: tests, isLoading } = useQuery<RefractionTest[]>({
    queryKey: ['refraction-tests'],
    queryFn: async () => {
      const response = await refractionApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateRefractionTestRequest) => refractionApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refraction-tests'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
      setIsModalOpen(false);
      setSelectedTest(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateRefractionTestRequest> }) =>
      refractionApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refraction-tests'] });
      setIsModalOpen(false);
      setSelectedTest(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => refractionApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refraction-tests'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
    },
  });

  const handleDelete = (test: RefractionTest) => {
    if (window.confirm('Are you sure you want to delete this refraction test?')) {
      deleteMutation.mutate(test.id);
    }
  };

  const handleEdit = (test: RefractionTest) => {
    setSelectedTest(test);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedTest(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedTest(undefined);
  };

  const filteredTests = tests?.filter(
    (test) =>
      test.patient?.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase()) ||
      test.performedBy.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const formatPrescription = (sphere: number, cylinder: number, axis: number) => {
    return `${sphere >= 0 ? '+' : ''}${sphere.toFixed(2)} / ${cylinder >= 0 ? '+' : ''}${cylinder.toFixed(2)} × ${axis}°`;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Refraction Tests</h1>
          <p className="text-gray-600 mt-1">Manage refraction assessments and measurements</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Test
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Tests</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{tests?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">This Month</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {tests?.filter((t) => {
              const testDate = new Date(t.date);
              const now = new Date();
              return (
                testDate.getMonth() === now.getMonth() && testDate.getFullYear() === now.getFullYear()
              );
            }).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Filtered Results</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{filteredTests?.length || 0}</p>
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
            <p className="text-gray-600 mt-2">Loading tests...</p>
          </div>
        ) : filteredTests && filteredTests.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Right Eye (SPH/CYL/AXIS)
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Left Eye (SPH/CYL/AXIS)
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">PD</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Performed By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredTests.map((test) => (
                  <tr key={test.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {format(new Date(test.date), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {test.patient?.fullNameEn || `Patient #${test.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 font-mono">
                      {formatPrescription(test.rightSphere, test.rightCylinder, test.rightAxis)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 font-mono">
                      {formatPrescription(test.leftSphere, test.leftCylinder, test.leftAxis)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{test.pupillaryDistance} mm</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{test.performedBy}</td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(test)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(test)}
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
            {searchTerm ? 'No tests found matching your search.' : 'No refraction tests found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedTest ? 'Edit Refraction Test' : 'New Refraction Test'}
              </Dialog.Title>
              <RefractionForm
                test={selectedTest}
                onSubmit={(data) => {
                  if (selectedTest) {
                    updateMutation.mutate({ id: selectedTest.id, data });
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
interface RefractionFormProps {
  test?: RefractionTest;
  onSubmit: (data: CreateRefractionTestRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const RefractionForm = ({ test, onSubmit, onCancel, isSubmitting }: RefractionFormProps) => {
  const [formData, setFormData] = useState<CreateRefractionTestRequest>({
    patientId: test?.patientId || 0,
    date: test?.date ? format(new Date(test.date), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
    rightSphere: test?.rightSphere || 0,
    rightCylinder: test?.rightCylinder || 0,
    rightAxis: test?.rightAxis || 0,
    leftSphere: test?.leftSphere || 0,
    leftCylinder: test?.leftCylinder || 0,
    leftAxis: test?.leftAxis || 0,
    pupillaryDistance: test?.pupillaryDistance || 63,
    performedBy: test?.performedBy || '',
    notes: test?.notes || '',
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
          <label className="block text-sm font-medium text-gray-700 mb-1">Test Date</label>
          <input
            type="date"
            required
            value={formData.date}
            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="border-t pt-4">
        <h3 className="text-md font-semibold text-gray-900 mb-3">Right Eye</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Sphere (Diopters)
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.rightSphere}
              onChange={(e) =>
                setFormData({ ...formData, rightSphere: parseFloat(e.target.value) || 0 })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cylinder (Diopters)
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.rightCylinder}
              onChange={(e) =>
                setFormData({ ...formData, rightCylinder: parseFloat(e.target.value) || 0 })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Axis (0-180°)</label>
            <input
              type="number"
              min="0"
              max="180"
              required
              value={formData.rightAxis}
              onChange={(e) =>
                setFormData({ ...formData, rightAxis: parseInt(e.target.value) || 0 })
              }
              className="input w-full"
            />
          </div>
        </div>
      </div>

      <div className="border-t pt-4">
        <h3 className="text-md font-semibold text-gray-900 mb-3">Left Eye</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Sphere (Diopters)
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.leftSphere}
              onChange={(e) =>
                setFormData({ ...formData, leftSphere: parseFloat(e.target.value) || 0 })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cylinder (Diopters)
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.leftCylinder}
              onChange={(e) =>
                setFormData({ ...formData, leftCylinder: parseFloat(e.target.value) || 0 })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Axis (0-180°)</label>
            <input
              type="number"
              min="0"
              max="180"
              required
              value={formData.leftAxis}
              onChange={(e) =>
                setFormData({ ...formData, leftAxis: parseInt(e.target.value) || 0 })
              }
              className="input w-full"
            />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Pupillary Distance (mm)
          </label>
          <input
            type="number"
            step="0.5"
            min="50"
            max="80"
            required
            value={formData.pupillaryDistance}
            onChange={(e) =>
              setFormData({ ...formData, pupillaryDistance: parseFloat(e.target.value) || 63 })
            }
            className="input w-full"
          />
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
          {isSubmitting ? 'Saving...' : test ? 'Update Test' : 'Create Test'}
        </button>
      </div>
    </form>
  );
};
