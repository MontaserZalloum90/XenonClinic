import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { OrthopedicExam, CreateOrthopedicExamRequest } from '../../types/orthopedics';
import { useT } from '../../contexts/TenantContext';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

const orthopedicsApi = {
  getAllExams: () => api.get('/api/OrthopedicsApi/exams'),
  createExam: (data: CreateOrthopedicExamRequest) => api.post('/api/OrthopedicsApi/exams', data),
  updateExam: (id: number, data: Partial<CreateOrthopedicExamRequest>) => api.put(`/api/OrthopedicsApi/exams/${id}`, data),
  deleteExam: (id: number) => api.delete(`/api/OrthopedicsApi/exams/${id}`),
};

export const OrthopedicExams = () => {
  const t = useT();
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<OrthopedicExam | undefined>(undefined);

  const { data: exams, isLoading } = useQuery<OrthopedicExam[]>({
    queryKey: ['orthopedic-exams'],
    queryFn: async () => {
      const response = await orthopedicsApi.getAllExams();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateOrthopedicExamRequest) => orthopedicsApi.createExam(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orthopedic-exams'] });
      queryClient.invalidateQueries({ queryKey: ['orthopedics-stats'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateOrthopedicExamRequest> }) =>
      orthopedicsApi.updateExam(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orthopedic-exams'] });
      queryClient.invalidateQueries({ queryKey: ['orthopedics-stats'] });
      setIsModalOpen(false);
      setSelectedExam(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => orthopedicsApi.deleteExam(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orthopedic-exams'] });
      queryClient.invalidateQueries({ queryKey: ['orthopedics-stats'] });
    },
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const data: any = {
      patientId: parseInt(formData.get('patientId') as string),
      examDate: formData.get('examDate') as string,
      chiefComplaint: formData.get('chiefComplaint') as string,
      affectedArea: formData.get('affectedArea') as string,
      rangeOfMotion: formData.get('rangeOfMotion') as string || undefined,
      strength: formData.get('strength') as string || undefined,
      stability: formData.get('stability') as string || undefined,
      specialTests: formData.get('specialTests') as string || undefined,
      diagnosis: formData.get('diagnosis') as string,
      plan: formData.get('plan') as string,
      performedBy: formData.get('performedBy') as string,
      notes: formData.get('notes') as string || undefined,
    };

    if (selectedExam) {
      updateMutation.mutate({ id: selectedExam.id, data });
    } else {
      createMutation.mutate(data);
    }
  };

  const handleDelete = (exam: OrthopedicExam) => {
    if (window.confirm(t('message.confirmDelete', `Are you sure you want to delete this exam?`))) {
      deleteMutation.mutate(exam.id);
    }
  };

  const handleEdit = (exam: OrthopedicExam) => {
    setSelectedExam(exam);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedExam(undefined);
    setIsModalOpen(true);
  };

  const handleViewDetails = (exam: OrthopedicExam) => {
    setSelectedExam(exam);
    setIsDetailModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setIsDetailModalOpen(false);
    setSelectedExam(undefined);
  };

  const filteredExams = exams?.filter((exam) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      exam.patientName?.toLowerCase().includes(searchLower) ||
      exam.chiefComplaint.toLowerCase().includes(searchLower) ||
      exam.affectedArea.toLowerCase().includes(searchLower) ||
      exam.diagnosis.toLowerCase().includes(searchLower) ||
      exam.performedBy.toLowerCase().includes(searchLower)
    );
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('page.orthopedicExams.title', 'Orthopedic Exams')}</h1>
          <p className="text-gray-600 mt-1">{t('page.orthopedicExams.description', 'Manage orthopedic examinations')}</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          {t('action.newExam', 'New Exam')}
        </button>
      </div>

      <div className="card animate-fade-in">
        <div className="mb-4">
          <input
            type="text"
            placeholder={t('field.search', 'Search by patient, complaint, area, or diagnosis...')}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">{t('message.loading', 'Loading...')}</p>
          </div>
        ) : filteredExams && filteredExams.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.date', 'Date')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.patient', 'Patient')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.complaint', 'Chief Complaint')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.affectedArea', 'Affected Area')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.diagnosis', 'Diagnosis')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.performedBy', 'Performed By')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.actions', 'Actions')}
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredExams.map((exam) => (
                  <tr key={exam.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900 whitespace-nowrap">
                      {format(new Date(exam.examDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">{exam.patientName || `Patient #${exam.patientId}`}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.chiefComplaint}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.affectedArea}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.diagnosis}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{exam.performedBy}</td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewDetails(exam)}
                          className="text-primary-600 hover:text-primary-800"
                          title={t('action.view', 'View')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleEdit(exam)}
                          className="text-blue-600 hover:text-blue-800"
                          title={t('action.edit', 'Edit')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDelete(exam)}
                          className="text-red-600 hover:text-red-800"
                          title={t('action.delete', 'Delete')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
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
            {searchTerm ? t('message.noResults', 'No exams found matching your search.') : t('message.noExams', 'No exams recorded yet.')}
          </div>
        )}
      </div>

      {/* Edit/Create Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedExam ? t('action.editExam', 'Edit Exam') : t('action.newExam', 'New Exam')}
              </Dialog.Title>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.patientId', 'Patient ID')} *
                    </label>
                    <input
                      type="number"
                      name="patientId"
                      defaultValue={selectedExam?.patientId}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.examDate', 'Exam Date')} *
                    </label>
                    <input
                      type="date"
                      name="examDate"
                      defaultValue={selectedExam?.examDate ? format(new Date(selectedExam.examDate), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd')}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.chiefComplaint', 'Chief Complaint')} *
                    </label>
                    <input
                      type="text"
                      name="chiefComplaint"
                      defaultValue={selectedExam?.chiefComplaint}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.affectedArea', 'Affected Area')} *
                    </label>
                    <input
                      type="text"
                      name="affectedArea"
                      defaultValue={selectedExam?.affectedArea}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.rangeOfMotion', 'Range of Motion')}
                    </label>
                    <input
                      type="text"
                      name="rangeOfMotion"
                      defaultValue={selectedExam?.rangeOfMotion}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.strength', 'Strength')}
                    </label>
                    <input
                      type="text"
                      name="strength"
                      defaultValue={selectedExam?.strength}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.stability', 'Stability')}
                    </label>
                    <input
                      type="text"
                      name="stability"
                      defaultValue={selectedExam?.stability}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.specialTests', 'Special Tests')}
                    </label>
                    <input
                      type="text"
                      name="specialTests"
                      defaultValue={selectedExam?.specialTests}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.diagnosis', 'Diagnosis')} *
                    </label>
                    <input
                      type="text"
                      name="diagnosis"
                      defaultValue={selectedExam?.diagnosis}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.performedBy', 'Performed By')} *
                    </label>
                    <input
                      type="text"
                      name="performedBy"
                      defaultValue={selectedExam?.performedBy}
                      required
                      className="input w-full"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('field.plan', 'Treatment Plan')} *
                  </label>
                  <textarea
                    name="plan"
                    defaultValue={selectedExam?.plan}
                    required
                    rows={3}
                    className="input w-full"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('field.notes', 'Notes')}
                  </label>
                  <textarea
                    name="notes"
                    defaultValue={selectedExam?.notes}
                    rows={3}
                    className="input w-full"
                  />
                </div>
                <div className="flex justify-end gap-2 pt-4">
                  <button type="button" onClick={handleModalClose} className="btn btn-secondary">
                    {t('action.cancel', 'Cancel')}
                  </button>
                  <button
                    type="submit"
                    disabled={createMutation.isPending || updateMutation.isPending}
                    className="btn btn-primary"
                  >
                    {createMutation.isPending || updateMutation.isPending
                      ? t('action.saving', 'Saving...')
                      : selectedExam
                      ? t('action.update', 'Update')
                      : t('action.create', 'Create')}
                  </button>
                </div>
              </form>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* Detail Modal */}
      <Dialog open={isDetailModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {t('title.examDetails', 'Exam Details')}
              </Dialog.Title>
              {selectedExam && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.patient', 'Patient')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedExam.patientName || `Patient #${selectedExam.patientId}`}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.examDate', 'Exam Date')}</label>
                      <p className="text-sm text-gray-900 mt-1">{format(new Date(selectedExam.examDate), 'MMMM d, yyyy')}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.chiefComplaint', 'Chief Complaint')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedExam.chiefComplaint}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.affectedArea', 'Affected Area')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedExam.affectedArea}</p>
                    </div>
                    {selectedExam.rangeOfMotion && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.rangeOfMotion', 'Range of Motion')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedExam.rangeOfMotion}</p>
                      </div>
                    )}
                    {selectedExam.strength && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.strength', 'Strength')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedExam.strength}</p>
                      </div>
                    )}
                    {selectedExam.stability && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.stability', 'Stability')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedExam.stability}</p>
                      </div>
                    )}
                    {selectedExam.specialTests && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.specialTests', 'Special Tests')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedExam.specialTests}</p>
                      </div>
                    )}
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.diagnosis', 'Diagnosis')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedExam.diagnosis}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.performedBy', 'Performed By')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedExam.performedBy}</p>
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">{t('field.plan', 'Treatment Plan')}</label>
                    <p className="text-sm text-gray-900 mt-1">{selectedExam.plan}</p>
                  </div>
                  {selectedExam.notes && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.notes', 'Notes')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedExam.notes}</p>
                    </div>
                  )}
                  <div className="flex justify-end gap-2 pt-4">
                    <button onClick={handleModalClose} className="btn btn-secondary">
                      {t('action.close', 'Close')}
                    </button>
                    <button
                      onClick={() => {
                        handleModalClose();
                        handleEdit(selectedExam);
                      }}
                      className="btn btn-primary"
                    >
                      {t('action.edit', 'Edit')}
                    </button>
                  </div>
                </div>
              )}
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
