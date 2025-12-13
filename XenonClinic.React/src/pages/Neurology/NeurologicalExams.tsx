import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
} from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import type { NeurologicalExam, CreateNeurologicalExamRequest } from '../../types/neurology';
import { ExamStatus, MentalStatus } from '../../types/neurology';

export const NeurologicalExams = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<NeurologicalExam | undefined>(undefined);

  // Mock data - Replace with actual API calls
  const { data: exams, isLoading } = useQuery<NeurologicalExam[]>({
    queryKey: ['neurological-exams'],
    queryFn: async () => {
      // Mock implementation
      return [
        {
          id: 1,
          patientId: 2001,
          patientName: 'Sarah Johnson',
          examDate: new Date().toISOString(),
          mentalStatus: MentalStatus.Alert,
          cranialNerves: 'II-XII intact',
          motorFunction: '5/5 strength all extremities',
          sensory: 'Intact to light touch and pinprick',
          reflexes: '2+ symmetric throughout',
          coordination: 'Finger-to-nose intact, no dysmetria',
          gait: 'Normal, tandem gait intact',
          diagnosis: 'Migraine without aura',
          performedBy: 'Dr. Martinez',
          status: ExamStatus.Completed,
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 2002,
          patientName: 'Michael Brown',
          examDate: new Date().toISOString(),
          mentalStatus: MentalStatus.Alert,
          cranialNerves: 'CN VII weakness on right side',
          motorFunction: '4/5 right upper and lower extremity',
          sensory: 'Decreased on right side',
          reflexes: '3+ on right, 2+ on left',
          coordination: 'Impaired on right side',
          gait: 'Hemiplegic gait',
          diagnosis: 'Post-stroke residual deficit',
          performedBy: 'Dr. Chen',
          status: ExamStatus.Reviewed,
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 2003,
          patientName: 'Emily Davis',
          examDate: new Date().toISOString(),
          mentalStatus: MentalStatus.Confused,
          cranialNerves: 'Difficult to assess',
          motorFunction: '3/5 bilateral lower extremities',
          sensory: 'Intact',
          reflexes: '1+ throughout',
          coordination: 'Unable to assess',
          gait: 'Unable to assess',
          diagnosis: 'Encephalopathy, etiology unclear',
          performedBy: 'Dr. Williams',
          status: ExamStatus.Pending,
          notes: 'Patient confused, requires further workup',
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredExams = exams?.filter((exam) => {
    const matchesSearch =
      !searchTerm ||
      exam.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exam.diagnosis?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || exam.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedExam(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (exam: NeurologicalExam) => {
    setSelectedExam(exam);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedExam(undefined);
  };

  const getStatusBadge = (status: ExamStatus) => {
    const config: Record<string, { className: string; label: string }> = {
      [ExamStatus.Pending]: { className: 'bg-yellow-100 text-yellow-800', label: 'Pending' },
      [ExamStatus.InProgress]: { className: 'bg-blue-100 text-blue-800', label: 'In Progress' },
      [ExamStatus.Completed]: { className: 'bg-green-100 text-green-800', label: 'Completed' },
      [ExamStatus.Reviewed]: { className: 'bg-purple-100 text-purple-800', label: 'Reviewed' },
    };
    const c = config[status] || { className: 'bg-gray-100 text-gray-800', label: status };
    return (
      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
        {c.label}
      </span>
    );
  };

  const getMentalStatusLabel = (status: MentalStatus): string => {
    const labels: Record<string, string> = {
      [MentalStatus.Alert]: 'Alert',
      [MentalStatus.Confused]: 'Confused',
      [MentalStatus.Drowsy]: 'Drowsy',
      [MentalStatus.Lethargic]: 'Lethargic',
      [MentalStatus.Stuporous]: 'Stuporous',
      [MentalStatus.Comatose]: 'Comatose',
    };
    return labels[status] || status;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Neurological Examinations</h1>
          <p className="text-gray-600 mt-1">Comprehensive neurological assessment records</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Examination
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search by patient name or diagnosis..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
              />
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Status</option>
              {Object.entries(ExamStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Exams Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Exam Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Mental Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Diagnosis
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Performed By
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading exams...</p>
                  </td>
                </tr>
              ) : filteredExams && filteredExams.length > 0 ? (
                filteredExams.map((exam) => (
                  <tr key={exam.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {exam.patientName || `Patient #${exam.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(exam.examDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {getMentalStatusLabel(exam.mentalStatus)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {exam.diagnosis || 'No diagnosis'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(exam.status || ExamStatus.Pending)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {exam.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleEdit(exam)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {exam.status === ExamStatus.Pending && (
                        <button className="text-green-600 hover:text-green-900">Review</button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                    {searchTerm ? 'No exams found matching your search.' : 'No neurological exams found.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <NeurologicalExamModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        exam={selectedExam}
      />
    </div>
  );
};

// Neurological Exam Form Modal
interface NeurologicalExamModalProps {
  isOpen: boolean;
  onClose: () => void;
  exam?: NeurologicalExam;
}

const NeurologicalExamModal = ({ isOpen, onClose, exam }: NeurologicalExamModalProps) => {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<Partial<CreateNeurologicalExamRequest>>({
    patientId: exam?.patientId || 0,
    examDate: exam?.examDate || new Date().toISOString().split('T')[0],
    mentalStatus: exam?.mentalStatus || MentalStatus.Alert,
    cranialNerves: exam?.cranialNerves || '',
    motorFunction: exam?.motorFunction || '',
    sensory: exam?.sensory || '',
    reflexes: exam?.reflexes || '',
    coordination: exam?.coordination || '',
    gait: exam?.gait || '',
    diagnosis: exam?.diagnosis || '',
    performedBy: exam?.performedBy || '',
    notes: exam?.notes || '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // In production, call API to save
    console.log('Saving neurological exam:', formData);
    queryClient.invalidateQueries({ queryKey: ['neurological-exams'] });
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'patientId' ? Number(value) : value,
    }));
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {exam ? 'View Neurological Exam' : 'New Neurological Exam'}
              </Dialog.Title>
              <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Patient ID
                  </label>
                  <input
                    type="number"
                    name="patientId"
                    value={formData.patientId}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Exam Date
                  </label>
                  <input
                    type="date"
                    name="examDate"
                    value={formData.examDate?.split('T')[0]}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Mental Status
                  </label>
                  <select
                    name="mentalStatus"
                    value={formData.mentalStatus}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(MentalStatus).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Performed By
                  </label>
                  <input
                    type="text"
                    name="performedBy"
                    value={formData.performedBy}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Cranial Nerves
                  </label>
                  <textarea
                    name="cranialNerves"
                    value={formData.cranialNerves}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., II-XII intact"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Motor Function
                  </label>
                  <textarea
                    name="motorFunction"
                    value={formData.motorFunction}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., 5/5 strength all extremities"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Sensory Exam
                  </label>
                  <textarea
                    name="sensory"
                    value={formData.sensory}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., Intact to light touch and pinprick"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Reflexes
                  </label>
                  <textarea
                    name="reflexes"
                    value={formData.reflexes}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., 2+ symmetric throughout"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Coordination
                  </label>
                  <textarea
                    name="coordination"
                    value={formData.coordination}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., Finger-to-nose intact, no dysmetria"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Gait
                  </label>
                  <textarea
                    name="gait"
                    value={formData.gait}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., Normal, tandem gait intact"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Diagnosis
                  </label>
                  <input
                    type="text"
                    name="diagnosis"
                    value={formData.diagnosis}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
                  <textarea
                    name="notes"
                    value={formData.notes}
                    onChange={handleChange}
                    rows={3}
                    className="input w-full"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <button type="button" onClick={onClose} className="btn btn-outline">
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  {exam ? 'Update' : 'Create'} Exam
                </button>
              </div>
            </form>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
