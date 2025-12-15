import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import type { Pregnancy, PregnancyStatus } from '../../types/obgyn';

// Mock API
const pregnancyApi = {
  getAll: async (): Promise<Pregnancy[]> => [],
  create: async (data: Partial<Pregnancy>): Promise<Pregnancy> => ({ id: 1, ...data } as Pregnancy),
  update: async (id: number, data: Partial<Pregnancy>): Promise<Pregnancy> => ({ id, ...data } as Pregnancy),
  delete: async (): Promise<void> => {},
};

export const Pregnancies = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPregnancy, setSelectedPregnancy] = useState<Pregnancy | undefined>();

  const { data: pregnancies, isLoading } = useQuery({
    queryKey: ['pregnancies'],
    queryFn: pregnancyApi.getAll,
  });

  const deleteMutation = useMutation({
    mutationFn: pregnancyApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['pregnancies'] }),
  });

  const getStatusLabel = (status: PregnancyStatus) => {
    const labels: Record<number, string> = {
      0: 'Active',
      1: 'Delivered',
      2: 'Miscarriage',
      3: 'Ectopic',
      4: 'Terminated',
    };
    return labels[status] || 'Unknown';
  };

  const getStatusColor = (status: PregnancyStatus) => {
    const colors: Record<number, string> = {
      0: 'text-green-600 bg-green-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-red-600 bg-red-100',
      3: 'text-orange-600 bg-orange-100',
      4: 'text-gray-600 bg-gray-100',
    };
    return colors[status] || 'text-gray-600 bg-gray-100';
  };

  const filteredPregnancies = pregnancies?.filter(
    (p) =>
      p.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.patientId.toString().includes(searchTerm)
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Pregnancies</h1>
          <p className="text-gray-600 mt-1">Track and manage pregnancy records</p>
        </div>
        <button onClick={() => setIsModalOpen(true)} className="btn btn-primary">
          New Pregnancy
        </button>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search by patient name or ID..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
          </div>
        ) : filteredPregnancies && filteredPregnancies.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">LMP</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">EDD</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">GA (weeks)</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">G/P</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredPregnancies.map((pregnancy) => (
                  <tr key={pregnancy.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {pregnancy.patientName || `Patient #${pregnancy.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(pregnancy.lmp), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(pregnancy.edd), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{pregnancy.gestationalAge}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      G{pregnancy.gravida}P{pregnancy.para}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(pregnancy.status)}`}>
                        {getStatusLabel(pregnancy.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => {
                            setSelectedPregnancy(pregnancy);
                            setIsModalOpen(true);
                          }}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => {
                            if (window.confirm('Delete this pregnancy record?')) {
                              deleteMutation.mutate(pregnancy.id);
                            }
                          }}
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
          <div className="text-center py-8 text-gray-500">No pregnancies found.</div>
        )}
      </div>

      <Dialog open={isModalOpen} onClose={() => setIsModalOpen(false)} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl p-6">
            <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
              {selectedPregnancy ? 'Edit Pregnancy' : 'New Pregnancy'}
            </Dialog.Title>
            <p className="text-gray-600">Pregnancy form would go here...</p>
            <div className="mt-4 flex justify-end gap-2">
              <button onClick={() => setIsModalOpen(false)} className="btn btn-secondary">
                Cancel
              </button>
              <button onClick={() => setIsModalOpen(false)} className="btn btn-primary">
                Save
              </button>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
