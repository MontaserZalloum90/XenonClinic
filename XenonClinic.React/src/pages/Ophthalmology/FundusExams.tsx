import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { FundusExam, CreateFundusExamRequest } from '../../types/ophthalmology';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

// Mock API - Replace with actual API when backend is ready
const fundusApi = {
  getAll: () => Promise.resolve({ data: [] as FundusExam[] }),
  create: (data: CreateFundusExamRequest) => Promise.resolve({ data: { id: 1, ...data } }),
  update: (id: number, data: Partial<CreateFundusExamRequest>) =>
    Promise.resolve({ data: { id, ...data } }),
  delete: (id: number) => Promise.resolve({ data: { success: true } }),
};

export const FundusExams = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<FundusExam | undefined>(undefined);

  const { data: exams, isLoading } = useQuery<FundusExam[]>({
    queryKey: ['fundus-exams'],
    queryFn: async () => {
      const response = await fundusApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateFundusExamRequest) => fundusApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fundus-exams'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateFundusExamRequest> }) =>
      fundusApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fundus-exams'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => fundusApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fundus-exams'] });
      queryClient.invalidateQueries({ queryKey: ['ophthalmology-stats'] });
    },
  });

  const handleDelete = (exam: FundusExam) => {
    if (window.confirm('Are you sure you want to delete this fundus exam?')) {
      deleteMutation.mutate(exam.id);
    }
  };

  const handleEdit = (exam: FundusExam) => {
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
          <h1 className="text-2xl font-bold text-gray-900">Fundus Exams</h1>
          <p className="text-gray-600 mt-1">Posterior segment and retinal examination records</p>
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
          <p className="text-2xl font-bold text-cyan-600 mt-1">
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
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Right Eye Findings
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Left Eye Findings
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Optic Disc
                  </th>
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
                      {exam.rightEyeFindings.length > 40
                        ? `${exam.rightEyeFindings.substring(0, 40)}...`
                        : exam.rightEyeFindings}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {exam.leftEyeFindings.length > 40
                        ? `${exam.leftEyeFindings.substring(0, 40)}...`
                        : exam.leftEyeFindings}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.opticDisc || '-'}</td>
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
            {searchTerm ? 'No exams found matching your search.' : 'No fundus exams found.'}
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
                {selectedExam ? 'Edit Fundus Exam' : 'New Fundus Exam'}
              </Dialog.Title>
              <FundusForm
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
interface FundusFormProps {
  exam?: FundusExam;
  onSubmit: (data: CreateFundusExamRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const FundusForm = ({ exam, onSubmit, onCancel, isSubmitting }: FundusFormProps) => {
  const [formData, setFormData] = useState<CreateFundusExamRequest>({
    patientId: exam?.patientId || 0,
    date: exam?.date ? format(new Date(exam.date), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
    rightEyeFindings: exam?.rightEyeFindings || '',
    leftEyeFindings: exam?.leftEyeFindings || '',
    opticDisc: exam?.opticDisc || '',
    macula: exam?.macula || '',
    vessels: exam?.vessels || '',
    performedBy: exam?.performedBy || '',
    notes: exam?.notes || '',
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

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Right Eye Findings
          </label>
          <textarea
            rows={4}
            required
            value={formData.rightEyeFindings}
            onChange={(e) => setFormData({ ...formData, rightEyeFindings: e.target.value })}
            className="input w-full"
            placeholder="Describe right eye fundus findings..."
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Left Eye Findings
          </label>
          <textarea
            rows={4}
            required
            value={formData.leftEyeFindings}
            onChange={(e) => setFormData({ ...formData, leftEyeFindings: e.target.value })}
            className="input w-full"
            placeholder="Describe left eye fundus findings..."
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Optic Disc</label>
          <textarea
            rows={2}
            value={formData.opticDisc}
            onChange={(e) => setFormData({ ...formData, opticDisc: e.target.value })}
            className="input w-full"
            placeholder="Optic disc findings..."
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Macula</label>
          <textarea
            rows={2}
            value={formData.macula}
            onChange={(e) => setFormData({ ...formData, macula: e.target.value })}
            className="input w-full"
            placeholder="Macular findings..."
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Vessels</label>
          <textarea
            rows={2}
            value={formData.vessels}
            onChange={(e) => setFormData({ ...formData, vessels: e.target.value })}
            className="input w-full"
            placeholder="Vascular findings..."
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
          placeholder="Any additional observations or recommendations..."
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
