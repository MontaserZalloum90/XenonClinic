import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import type { SkinExam, CreateSkinExamRequest, SkinType } from '../../types/dermatology';

// Mock API - Replace with actual dermatology API
const skinExamsApi = {
  getAll: async () => {
    // TODO: Implement actual API call
    return { data: [] as SkinExam[] };
  },
  create: async (data: CreateSkinExamRequest) => {
    // TODO: Implement actual API call
    return { data: { id: Date.now(), ...data, createdAt: new Date().toISOString() } };
  },
  update: async (id: number, data: Partial<SkinExam>) => {
    // TODO: Implement actual API call
    return { data: { id, ...data } };
  },
  delete: async (id: number) => {
    // TODO: Implement actual API call
    return { data: { success: true } };
  },
};

export const SkinExams = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<SkinExam | undefined>(undefined);

  const { data: exams, isLoading } = useQuery<SkinExam[]>({
    queryKey: ['skin-exams'],
    queryFn: async () => {
      const response = await skinExamsApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateSkinExamRequest) => skinExamsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-exams'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<SkinExam> }) =>
      skinExamsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-exams'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => skinExamsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-exams'] });
    },
  });

  const handleDelete = (exam: SkinExam) => {
    if (window.confirm('Are you sure you want to delete this skin exam?')) {
      deleteMutation.mutate(exam.id);
    }
  };

  const handleEdit = (exam: SkinExam) => {
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
      exam.diagnosis?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exam.concerns?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getSkinTypeLabel = (skinType?: SkinType) => {
    if (!skinType) return 'Not specified';
    const labels: Record<SkinType, string> = {
      type1: 'Type I - Very Fair',
      type2: 'Type II - Fair',
      type3: 'Type III - Medium',
      type4: 'Type IV - Olive',
      type5: 'Type V - Brown',
      type6: 'Type VI - Dark',
    };
    return labels[skinType];
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Skin Exams</h1>
          <p className="text-gray-600 mt-1">Manage dermatological examinations</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Skin Exam
        </button>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search by patient name, diagnosis, or concerns..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading skin exams...</p>
          </div>
        ) : filteredExams && filteredExams.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Exam Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Skin Type
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Concerns
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Diagnosis
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
                {filteredExams.map((exam) => (
                  <tr key={exam.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {format(new Date(exam.examDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {exam.patient?.fullNameEn || `Patient #${exam.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {getSkinTypeLabel(exam.skinType)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {exam.concerns ? (
                        <span className="line-clamp-2">{exam.concerns}</span>
                      ) : (
                        '-'
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {exam.diagnosis ? (
                        <span className="line-clamp-2">{exam.diagnosis}</span>
                      ) : (
                        '-'
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.performedBy}</td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(exam)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          View
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
            {searchTerm ? 'No skin exams found matching your search.' : 'No skin exams found.'}
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
                {selectedExam ? 'View/Edit Skin Exam' : 'New Skin Exam'}
              </Dialog.Title>
              <SkinExamForm
                exam={selectedExam}
                onSubmit={(data) => {
                  if (selectedExam) {
                    updateMutation.mutate({ id: selectedExam.id, data });
                  } else {
                    createMutation.mutate(data as CreateSkinExamRequest);
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
interface SkinExamFormProps {
  exam?: SkinExam;
  onSubmit: (data: Partial<SkinExam>) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const SkinExamForm = ({ exam, onSubmit, onCancel, isSubmitting }: SkinExamFormProps) => {
  const [formData, setFormData] = useState({
    patientId: exam?.patientId || 0,
    examDate: exam?.examDate ? exam.examDate.split('T')[0] : new Date().toISOString().split('T')[0],
    skinType: exam?.skinType || '',
    concerns: exam?.concerns || '',
    findings: exam?.findings || '',
    bodyAreas: exam?.bodyAreas?.join(', ') || '',
    diagnosis: exam?.diagnosis || '',
    treatment: exam?.treatment || '',
    followUp: exam?.followUp || '',
    followUpDate: exam?.followUpDate ? exam.followUpDate.split('T')[0] : '',
    performedBy: exam?.performedBy || '',
    notes: exam?.notes || '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const bodyAreasArray = formData.bodyAreas
      .split(',')
      .map((area) => area.trim())
      .filter(Boolean);

    onSubmit({
      ...formData,
      bodyAreas: bodyAreasArray.length > 0 ? bodyAreasArray : undefined,
      skinType: formData.skinType || undefined,
      followUpDate: formData.followUpDate || undefined,
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
          <label className="label">Exam Date</label>
          <input
            type="date"
            required
            value={formData.examDate}
            onChange={(e) => setFormData({ ...formData, examDate: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Skin Type</label>
          <select
            value={formData.skinType}
            onChange={(e) => setFormData({ ...formData, skinType: e.target.value })}
            className="input w-full"
          >
            <option value="">Select skin type</option>
            <option value="type1">Type I - Very Fair</option>
            <option value="type2">Type II - Fair</option>
            <option value="type3">Type III - Medium</option>
            <option value="type4">Type IV - Olive</option>
            <option value="type5">Type V - Brown</option>
            <option value="type6">Type VI - Dark</option>
          </select>
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
      </div>

      <div>
        <label className="label">Chief Concerns</label>
        <textarea
          value={formData.concerns}
          onChange={(e) => setFormData({ ...formData, concerns: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Patient's main concerns or reason for visit"
        />
      </div>

      <div>
        <label className="label">Body Areas Examined</label>
        <input
          type="text"
          value={formData.bodyAreas}
          onChange={(e) => setFormData({ ...formData, bodyAreas: e.target.value })}
          className="input w-full"
          placeholder="e.g., face, back, arms (comma-separated)"
        />
      </div>

      <div>
        <label className="label">Clinical Findings</label>
        <textarea
          value={formData.findings}
          onChange={(e) => setFormData({ ...formData, findings: e.target.value })}
          className="input w-full"
          rows={3}
          placeholder="Detailed examination findings"
        />
      </div>

      <div>
        <label className="label">Diagnosis</label>
        <textarea
          value={formData.diagnosis}
          onChange={(e) => setFormData({ ...formData, diagnosis: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Clinical diagnosis"
        />
      </div>

      <div>
        <label className="label">Treatment Plan</label>
        <textarea
          value={formData.treatment}
          onChange={(e) => setFormData({ ...formData, treatment: e.target.value })}
          className="input w-full"
          rows={3}
          placeholder="Prescribed treatments, medications, procedures"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Follow-up Instructions</label>
          <textarea
            value={formData.followUp}
            onChange={(e) => setFormData({ ...formData, followUp: e.target.value })}
            className="input w-full"
            rows={2}
            placeholder="Follow-up care instructions"
          />
        </div>
        <div>
          <label className="label">Follow-up Date</label>
          <input
            type="date"
            value={formData.followUpDate}
            onChange={(e) => setFormData({ ...formData, followUpDate: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div>
        <label className="label">Additional Notes</label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Any additional notes or observations"
        />
      </div>

      <div className="flex items-center justify-end gap-3 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" disabled={isSubmitting} className="btn btn-primary">
          {isSubmitting ? 'Saving...' : exam ? 'Update Exam' : 'Create Exam'}
        </button>
      </div>
    </form>
  );
};
