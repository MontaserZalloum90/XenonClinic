import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { SlitLampExam, CreateSlitLampExamRequest } from '../../types/ophthalmology';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

// Mock API - Replace with actual API when backend is ready
const slitLampApi = {
  getAll: () => Promise.resolve({ data: [] as SlitLampExam[] }),
  create: (data: CreateSlitLampExamRequest) => Promise.resolve({ data: { id: 1, ...data } }),
  update: (id: number, data: Partial<CreateSlitLampExamRequest>) =>
    Promise.resolve({ data: { id, ...data } }),
  delete: (id: number) => Promise.resolve({ data: { success: true } }),
};

export const SlitLampExams = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<SlitLampExam | undefined>(undefined);

  const { data: exams, isLoading } = useQuery<SlitLampExam[]>({
    queryKey: ['slit-lamp-exams'],
    queryFn: async () => {
      const response = await slitLampApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateSlitLampExamRequest) => slitLampApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['slit-lamp-exams'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateSlitLampExamRequest> }) =>
      slitLampApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['slit-lamp-exams'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => slitLampApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['slit-lamp-exams'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
    },
  });

  const handleDelete = (exam: SlitLampExam) => {
    if (window.confirm('Are you sure you want to delete this slit lamp exam?')) {
      deleteMutation.mutate(exam.id);
    }
  };

  const handleEdit = (exam: SlitLampExam) => {
    setSelectedExam(exam);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedExam(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedExam(undefined);
  };

  const filteredExams = exams?.filter(
    (exam) =>
      exam.patient?.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exam.performedBy.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Slit Lamp Exams</h1>
          <p className="text-gray-600 mt-1">Anterior segment examination records</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Exam
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Exams</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{exams?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">This Month</p>
          <p className="text-2xl font-bold text-teal-600 mt-1">
            {exams?.filter((e) => {
              const examDate = new Date(e.date);
              const now = new Date();
              return (
                examDate.getMonth() === now.getMonth() && examDate.getFullYear() === now.getFullYear()
              );
            }).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Filtered Results</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{filteredExams?.length || 0}</p>
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
            <p className="text-gray-600 mt-2">Loading exams...</p>
          </div>
        ) : filteredExams && filteredExams.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Findings</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Cornea</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Lens</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Performed By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredExams.map((exam) => (
                  <tr key={exam.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {format(new Date(exam.date), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {exam.patient?.fullNameEn || `Patient #${exam.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {exam.findings.length > 50 ? `${exam.findings.substring(0, 50)}...` : exam.findings}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.cornea || '-'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.lens || '-'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.performedBy}</td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(exam)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(exam)}
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
            {searchTerm ? 'No exams found matching your search.' : 'No slit lamp exams found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedExam ? 'Edit Slit Lamp Exam' : 'New Slit Lamp Exam'}
              </Dialog.Title>
              <SlitLampForm
                exam={selectedExam}
                onSubmit={(data) => {
                  if (selectedExam) {
                    updateMutation.mutate({ id: selectedExam.id, data });
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
interface SlitLampFormProps {
  exam?: SlitLampExam;
  onSubmit: (data: CreateSlitLampExamRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const SlitLampForm = ({ exam, onSubmit, onCancel, isSubmitting }: SlitLampFormProps) => {
  const [formData, setFormData] = useState<CreateSlitLampExamRequest>({
    patientId: exam?.patientId || 0,
    date: exam?.date ? format(new Date(exam.date), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
    findings: exam?.findings || '',
    cornea: exam?.cornea || '',
    lens: exam?.lens || '',
    anteriorChamber: exam?.anteriorChamber || '',
    iris: exam?.iris || '',
    notes: exam?.notes || '',
    performedBy: exam?.performedBy || '',
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
          <label className="block text-sm font-medium text-gray-700 mb-1">Exam Date</label>
          <input
            type="date"
            required
            value={formData.date}
            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">General Findings</label>
        <textarea
          rows={3}
          required
          value={formData.findings}
          onChange={(e) => setFormData({ ...formData, findings: e.target.value })}
          className="input w-full"
          placeholder="Overall examination findings..."
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Cornea</label>
          <textarea
            rows={2}
            value={formData.cornea}
            onChange={(e) => setFormData({ ...formData, cornea: e.target.value })}
            className="input w-full"
            placeholder="Corneal findings..."
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Lens</label>
          <textarea
            rows={2}
            value={formData.lens}
            onChange={(e) => setFormData({ ...formData, lens: e.target.value })}
            className="input w-full"
            placeholder="Lens findings..."
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Anterior Chamber</label>
          <textarea
            rows={2}
            value={formData.anteriorChamber}
            onChange={(e) => setFormData({ ...formData, anteriorChamber: e.target.value })}
            className="input w-full"
            placeholder="Anterior chamber findings..."
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Iris</label>
          <textarea
            rows={2}
            value={formData.iris}
            onChange={(e) => setFormData({ ...formData, iris: e.target.value })}
            className="input w-full"
            placeholder="Iris findings..."
          />
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
        <label className="block text-sm font-medium text-gray-700 mb-1">Additional Notes</label>
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
          {isSubmitting ? 'Saving...' : exam ? 'Update Exam' : 'Create Exam'}
        </button>
      </div>
    </form>
  );
};
