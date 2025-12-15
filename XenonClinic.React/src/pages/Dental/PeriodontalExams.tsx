import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  BeakerIcon,
} from '@heroicons/react/24/outline';
import { Dialog } from '@headlessui/react';
import type { PeriodontalExam, CreatePeriodontalExamRequest } from '../../types/dental';
import { format } from 'date-fns';

// Mock API functions - Replace with actual API calls
const periodontalExamApi = {
  getAll: async () => ({
    data: [] as PeriodontalExam[],
  }),
  create: async (data: CreatePeriodontalExamRequest) => ({
    data: { id: Date.now(), ...data },
  }),
  update: async (id: number, data: Partial<PeriodontalExam>) => ({
    data: { id, ...data },
  }),
  delete: async () => ({
    data: { success: true },
  }),
};

interface ExamFormData {
  patientId: number;
  examDate: string;
  plaqueScore?: number;
  gingivalIndex?: number;
  notes?: string;
  diagnosis?: string;
  treatmentPlan?: string;
}

const PeriodontalExamForm = ({
  exam,
  onSuccess,
  onCancel,
}: {
  exam?: PeriodontalExam;
  onSuccess: () => void;
  onCancel: () => void;
}) => {
  const queryClient = useQueryClient();
  const isEditing = !!exam;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ExamFormData>({
    defaultValues: exam
      ? {
          patientId: exam.patientId,
          examDate: format(new Date(exam.examDate), 'yyyy-MM-dd'),
          plaqueScore: exam.plaqueScore,
          gingivalIndex: exam.gingivalIndex,
          notes: exam.notes || '',
          diagnosis: exam.diagnosis || '',
          treatmentPlan: exam.treatmentPlan || '',
        }
      : {
          examDate: format(new Date(), 'yyyy-MM-dd'),
        },
  });

  const createMutation = useMutation({
    mutationFn: (data: ExamFormData) =>
      periodontalExamApi.create({
        ...data,
        pocketDepths: [],
        bleedingPoints: [],
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['periodontal-exams'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: ExamFormData) =>
      periodontalExamApi.update(exam!.id, { ...data, id: exam!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['periodontal-exams'] });
      onSuccess();
    },
  });

  const onSubmit = (data: ExamFormData) => {
    if (isEditing) {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(data);
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;
  const error = createMutation.error || updateMutation.error;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {error && (
        <div className="p-3 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-600">
            Error: {error instanceof Error ? error.message : 'An error occurred'}
          </p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Patient ID */}
        <div>
          <label htmlFor="patientId" className="block text-sm font-medium text-gray-700 mb-1">
            Patient ID *
          </label>
          <input
            type="number"
            id="patientId"
            {...register('patientId', { required: 'Patient ID is required', valueAsNumber: true })}
            className="input"
            placeholder="1"
          />
          {errors.patientId && (
            <p className="mt-1 text-sm text-red-600">{errors.patientId.message}</p>
          )}
        </div>

        {/* Exam Date */}
        <div>
          <label htmlFor="examDate" className="block text-sm font-medium text-gray-700 mb-1">
            Exam Date *
          </label>
          <input
            type="date"
            id="examDate"
            {...register('examDate', { required: 'Exam date is required' })}
            className="input"
          />
          {errors.examDate && (
            <p className="mt-1 text-sm text-red-600">{errors.examDate.message}</p>
          )}
        </div>

        {/* Plaque Score */}
        <div>
          <label htmlFor="plaqueScore" className="block text-sm font-medium text-gray-700 mb-1">
            Plaque Score (%)
          </label>
          <input
            type="number"
            step="0.1"
            id="plaqueScore"
            {...register('plaqueScore', { valueAsNumber: true, min: 0, max: 100 })}
            className="input"
            placeholder="0-100"
          />
        </div>

        {/* Gingival Index */}
        <div>
          <label htmlFor="gingivalIndex" className="block text-sm font-medium text-gray-700 mb-1">
            Gingival Index (0-3)
          </label>
          <input
            type="number"
            step="0.1"
            id="gingivalIndex"
            {...register('gingivalIndex', { valueAsNumber: true, min: 0, max: 3 })}
            className="input"
            placeholder="0-3 scale"
          />
        </div>
      </div>

      {/* Diagnosis */}
      <div>
        <label htmlFor="diagnosis" className="block text-sm font-medium text-gray-700 mb-1">
          Diagnosis
        </label>
        <textarea
          id="diagnosis"
          {...register('diagnosis')}
          rows={2}
          className="input"
          placeholder="Clinical diagnosis..."
        />
      </div>

      {/* Treatment Plan */}
      <div>
        <label htmlFor="treatmentPlan" className="block text-sm font-medium text-gray-700 mb-1">
          Treatment Plan
        </label>
        <textarea
          id="treatmentPlan"
          {...register('treatmentPlan')}
          rows={3}
          className="input"
          placeholder="Recommended treatment plan..."
        />
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          Clinical Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={3}
          className="input"
          placeholder="Additional clinical notes..."
        />
      </div>

      {/* Pocket Depths Placeholder */}
      <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center">
        <BeakerIcon className="h-12 w-12 text-gray-400 mx-auto mb-2" />
        <p className="text-sm text-gray-500">Pocket Depths Measurement Interface</p>
        <p className="text-xs text-gray-400 mt-1">
          Interactive charting for recording probing depths and bleeding points
        </p>
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-3 pt-4 border-t">
        <button type="button" onClick={onCancel} disabled={isPending} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" disabled={isPending} className="btn btn-primary">
          {isPending ? (
            <div className="flex items-center">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              {isEditing ? 'Updating...' : 'Creating...'}
            </div>
          ) : isEditing ? (
            'Update Exam'
          ) : (
            'Create Exam'
          )}
        </button>
      </div>
    </form>
  );
};

export const PeriodontalExams = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<PeriodontalExam | undefined>(undefined);

  // Fetch exams
  const { data: examsData, isLoading } = useQuery({
    queryKey: ['periodontal-exams'],
    queryFn: () => periodontalExamApi.getAll(),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => periodontalExamApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['periodontal-exams'] });
    },
  });

  const exams = examsData?.data || [];

  // Filter exams
  const filteredExams = exams.filter((exam) => {
    const matchesSearch =
      !searchTerm ||
      exam.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exam.patientId.toString().includes(searchTerm);
    return matchesSearch;
  });

  const handleDelete = (exam: PeriodontalExam) => {
    if (
      window.confirm(
        `Are you sure you want to delete the periodontal exam for ${exam.patientName}?`
      )
    ) {
      deleteMutation.mutate(exam.id);
    }
  };

  const handleEdit = (exam: PeriodontalExam) => {
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

  const getGingivalStatus = (index?: number) => {
    if (!index) return { label: 'N/A', className: 'bg-gray-100 text-gray-800' };
    if (index <= 1) return { label: 'Mild', className: 'bg-green-100 text-green-800' };
    if (index <= 2) return { label: 'Moderate', className: 'bg-yellow-100 text-yellow-800' };
    return { label: 'Severe', className: 'bg-red-100 text-red-800' };
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Periodontal Exams</h1>
          <p className="text-gray-600 mt-1">Manage periodontal examinations and assessments</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2 inline" />
          New Exam
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Exams</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{exams.length}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Exams This Month</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {
              exams.filter(
                (e) =>
                  new Date(e.examDate).getMonth() === new Date().getMonth() &&
                  new Date(e.examDate).getFullYear() === new Date().getFullYear()
              ).length
            }
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Avg Gingival Index</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {exams.length > 0
              ? (
                  exams.reduce((sum, e) => sum + (e.gingivalIndex || 0), 0) / exams.length
                ).toFixed(1)
              : '0.0'}
          </p>
        </div>
      </div>

      {/* Search and List */}
      <div className="card">
        <div className="mb-4">
          <div className="relative">
            <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
            <input
              type="text"
              placeholder="Search by patient name or ID..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input w-full pl-10"
            />
          </div>
        </div>

        {/* Exams Table */}
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading exams...</p>
          </div>
        ) : filteredExams.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Exam Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Plaque Score
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Gingival Index
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Examined By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredExams.map((exam) => {
                  const status = getGingivalStatus(exam.gingivalIndex);
                  return (
                    <tr key={exam.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm">
                        <div className="font-medium text-gray-900">
                          {exam.patientName || `Patient #${exam.patientId}`}
                        </div>
                        <div className="text-gray-500">ID: {exam.patientId}</div>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {format(new Date(exam.examDate), 'MMM d, yyyy')}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {exam.plaqueScore !== undefined ? `${exam.plaqueScore}%` : '-'}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {exam.gingivalIndex !== undefined ? exam.gingivalIndex.toFixed(1) : '-'}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <span
                          className={`px-2 py-0.5 rounded-full text-xs font-medium ${status.className}`}
                        >
                          {status.label}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{exam.examinedBy}</td>
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
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-12">
            <BeakerIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-500">
              {searchTerm ? 'No exams found matching your search.' : 'No periodontal exams found.'}
            </p>
            <button
              onClick={handleCreate}
              className="mt-4 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
            >
              <PlusIcon className="h-5 w-5 mr-2" />
              Create First Exam
            </button>
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
                {selectedExam ? 'View/Edit Periodontal Exam' : 'New Periodontal Exam'}
              </Dialog.Title>
              <PeriodontalExamForm
                exam={selectedExam}
                onSuccess={handleModalClose}
                onCancel={handleModalClose}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
