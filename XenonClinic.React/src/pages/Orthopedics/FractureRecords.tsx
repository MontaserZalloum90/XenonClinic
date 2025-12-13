import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { FractureRecord, CreateFractureRecordRequest, FractureStatus } from '../../types/orthopedics';
import { useT } from '../../contexts/TenantContext';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

const fracturesApi = {
  getAll: () => api.get('/api/OrthopedicsApi/fractures'),
  create: (data: CreateFractureRecordRequest) => api.post('/api/OrthopedicsApi/fractures', data),
  update: (id: number, data: Partial<CreateFractureRecordRequest>) => api.put(`/api/OrthopedicsApi/fractures/${id}`, data),
  delete: (id: number) => api.delete(`/api/OrthopedicsApi/fractures/${id}`),
};

export const FractureRecords = () => {
  const t = useT();
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<number | ''>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedFracture, setSelectedFracture] = useState<FractureRecord | undefined>(undefined);

  const { data: fractures, isLoading } = useQuery<FractureRecord[]>({
    queryKey: ['fracture-records'],
    queryFn: async () => {
      const response = await fracturesApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateFractureRecordRequest) => fracturesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fracture-records'] });
      queryClient.invalidateQueries({ queryKey: ['orthopedics-stats'] });
      setIsModalOpen(false);
      setSelectedFracture(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateFractureRecordRequest> }) =>
      fracturesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fracture-records'] });
      queryClient.invalidateQueries({ queryKey: ['orthopedics-stats'] });
      setIsModalOpen(false);
      setSelectedFracture(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => fracturesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fracture-records'] });
      queryClient.invalidateQueries({ queryKey: ['orthopedics-stats'] });
    },
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const data: any = {
      patientId: parseInt(formData.get('patientId') as string),
      fractureDate: formData.get('fractureDate') as string,
      boneAffected: formData.get('boneAffected') as string,
      fractureType: formData.get('fractureType') as string,
      location: formData.get('location') as string,
      displacement: formData.get('displacement') as string || undefined,
      treatment: formData.get('treatment') as string,
      castType: formData.get('castType') as string || undefined,
      expectedHealingTime: formData.get('expectedHealingTime') ? parseInt(formData.get('expectedHealingTime') as string) : undefined,
      followUpDate: formData.get('followUpDate') as string || undefined,
      status: parseInt(formData.get('status') as string),
      treatedBy: formData.get('treatedBy') as string,
      notes: formData.get('notes') as string || undefined,
    };

    if (selectedFracture) {
      updateMutation.mutate({ id: selectedFracture.id, data });
    } else {
      createMutation.mutate(data);
    }
  };

  const handleDelete = (fracture: FractureRecord) => {
    if (window.confirm(t('message.confirmDelete', `Are you sure you want to delete this fracture record?`))) {
      deleteMutation.mutate(fracture.id);
    }
  };

  const handleEdit = (fracture: FractureRecord) => {
    setSelectedFracture(fracture);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedFracture(undefined);
    setIsModalOpen(true);
  };

  const handleViewDetails = (fracture: FractureRecord) => {
    setSelectedFracture(fracture);
    setIsDetailModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setIsDetailModalOpen(false);
    setSelectedFracture(undefined);
  };

  const getStatusLabel = (status: number) => {
    const labels = ['Active', 'Healing', 'Healed', 'Complicated'];
    return labels[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-yellow-600 bg-yellow-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-green-600 bg-green-100',
      3: 'text-red-600 bg-red-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  const filteredFractures = fractures?.filter((fracture) => {
    const searchLower = searchTerm.toLowerCase();
    const matchesSearch =
      fracture.patientName?.toLowerCase().includes(searchLower) ||
      fracture.boneAffected.toLowerCase().includes(searchLower) ||
      fracture.fractureType.toLowerCase().includes(searchLower) ||
      fracture.location.toLowerCase().includes(searchLower) ||
      fracture.treatedBy.toLowerCase().includes(searchLower);

    const matchesStatus = statusFilter === '' || fracture.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('page.fractureRecords.title', 'Fracture Records')}</h1>
          <p className="text-gray-600 mt-1">{t('page.fractureRecords.description', 'Track fractures and healing progress')}</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <svg className="w-5 h-5 ltr:mr-2 rtl:ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          {t('action.recordFracture', 'Record Fracture')}
        </button>
      </div>

      <div className="card animate-fade-in">
        <div className="mb-4 flex flex-col sm:flex-row gap-3">
          <div className="flex-1">
            <input
              type="text"
              placeholder={t('field.search', 'Search by patient, bone, type, or location...')}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input w-full"
            />
          </div>
          <div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value === '' ? '' : parseInt(e.target.value))}
              className="input"
            >
              <option value="">{t('filter.allStatuses', 'All Statuses')}</option>
              <option value="0">{t('status.active', 'Active')}</option>
              <option value="1">{t('status.healing', 'Healing')}</option>
              <option value="2">{t('status.healed', 'Healed')}</option>
              <option value="3">{t('status.complicated', 'Complicated')}</option>
            </select>
          </div>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">{t('message.loading', 'Loading...')}</p>
          </div>
        ) : filteredFractures && filteredFractures.length > 0 ? (
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
                    {t('field.boneAffected', 'Bone')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.fractureType', 'Type')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.location', 'Location')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.treatment', 'Treatment')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.status', 'Status')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.followUp', 'Follow-up')}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t('field.actions', 'Actions')}
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredFractures.map((fracture) => (
                  <tr key={fracture.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900 whitespace-nowrap">
                      {format(new Date(fracture.fractureDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">{fracture.patientName || `Patient #${fracture.patientId}`}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{fracture.boneAffected}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{fracture.fractureType}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{fracture.location}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{fracture.treatment}</td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(fracture.status)}`}>
                        {getStatusLabel(fracture.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 whitespace-nowrap">
                      {fracture.followUpDate ? format(new Date(fracture.followUpDate), 'MMM d, yyyy') : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewDetails(fracture)}
                          className="text-primary-600 hover:text-primary-800"
                          title={t('action.view', 'View')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleEdit(fracture)}
                          className="text-blue-600 hover:text-blue-800"
                          title={t('action.edit', 'Edit')}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDelete(fracture)}
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
            {searchTerm || statusFilter !== ''
              ? t('message.noResults', 'No fracture records found matching your filters.')
              : t('message.noFractures', 'No fracture records found.')}
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
                {selectedFracture ? t('action.editFracture', 'Edit Fracture') : t('action.recordFracture', 'Record Fracture')}
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
                      defaultValue={selectedFracture?.patientId}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.fractureDate', 'Fracture Date')} *
                    </label>
                    <input
                      type="date"
                      name="fractureDate"
                      defaultValue={selectedFracture?.fractureDate ? format(new Date(selectedFracture.fractureDate), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd')}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.boneAffected', 'Bone Affected')} *
                    </label>
                    <input
                      type="text"
                      name="boneAffected"
                      defaultValue={selectedFracture?.boneAffected}
                      required
                      className="input w-full"
                      placeholder="e.g., Femur, Tibia, Humerus"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.fractureType', 'Fracture Type')} *
                    </label>
                    <input
                      type="text"
                      name="fractureType"
                      defaultValue={selectedFracture?.fractureType}
                      required
                      className="input w-full"
                      placeholder="e.g., Simple, Compound, Comminuted"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.location', 'Location')} *
                    </label>
                    <input
                      type="text"
                      name="location"
                      defaultValue={selectedFracture?.location}
                      required
                      className="input w-full"
                      placeholder="e.g., Proximal, Distal, Mid-shaft"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.displacement', 'Displacement')}
                    </label>
                    <input
                      type="text"
                      name="displacement"
                      defaultValue={selectedFracture?.displacement}
                      className="input w-full"
                      placeholder="e.g., Non-displaced, Minimally displaced"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.treatment', 'Treatment')} *
                    </label>
                    <input
                      type="text"
                      name="treatment"
                      defaultValue={selectedFracture?.treatment}
                      required
                      className="input w-full"
                      placeholder="e.g., Cast, Surgery, Splint"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.castType', 'Cast Type')}
                    </label>
                    <input
                      type="text"
                      name="castType"
                      defaultValue={selectedFracture?.castType}
                      className="input w-full"
                      placeholder="e.g., Short arm cast, Long leg cast"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.expectedHealingTime', 'Expected Healing Time (weeks)')}
                    </label>
                    <input
                      type="number"
                      name="expectedHealingTime"
                      defaultValue={selectedFracture?.expectedHealingTime}
                      className="input w-full"
                      min="1"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.followUpDate', 'Follow-up Date')}
                    </label>
                    <input
                      type="date"
                      name="followUpDate"
                      defaultValue={selectedFracture?.followUpDate ? format(new Date(selectedFracture.followUpDate), 'yyyy-MM-dd') : ''}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.status', 'Status')} *
                    </label>
                    <select
                      name="status"
                      defaultValue={selectedFracture?.status ?? 0}
                      required
                      className="input w-full"
                    >
                      <option value="0">{t('status.active', 'Active')}</option>
                      <option value="1">{t('status.healing', 'Healing')}</option>
                      <option value="2">{t('status.healed', 'Healed')}</option>
                      <option value="3">{t('status.complicated', 'Complicated')}</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('field.treatedBy', 'Treated By')} *
                    </label>
                    <input
                      type="text"
                      name="treatedBy"
                      defaultValue={selectedFracture?.treatedBy}
                      required
                      className="input w-full"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('field.notes', 'Notes')}
                  </label>
                  <textarea
                    name="notes"
                    defaultValue={selectedFracture?.notes}
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
                      : selectedFracture
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
                {t('title.fractureDetails', 'Fracture Details')}
              </Dialog.Title>
              {selectedFracture && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.patient', 'Patient')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.patientName || `Patient #${selectedFracture.patientId}`}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.fractureDate', 'Fracture Date')}</label>
                      <p className="text-sm text-gray-900 mt-1">{format(new Date(selectedFracture.fractureDate), 'MMMM d, yyyy')}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.boneAffected', 'Bone Affected')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.boneAffected}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.fractureType', 'Fracture Type')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.fractureType}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.location', 'Location')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.location}</p>
                    </div>
                    {selectedFracture.displacement && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.displacement', 'Displacement')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedFracture.displacement}</p>
                      </div>
                    )}
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.treatment', 'Treatment')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.treatment}</p>
                    </div>
                    {selectedFracture.castType && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.castType', 'Cast Type')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedFracture.castType}</p>
                      </div>
                    )}
                    {selectedFracture.expectedHealingTime && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.expectedHealingTime', 'Expected Healing Time')}</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedFracture.expectedHealingTime} weeks</p>
                      </div>
                    )}
                    {selectedFracture.followUpDate && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">{t('field.followUpDate', 'Follow-up Date')}</label>
                        <p className="text-sm text-gray-900 mt-1">{format(new Date(selectedFracture.followUpDate), 'MMMM d, yyyy')}</p>
                      </div>
                    )}
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.status', 'Status')}</label>
                      <span className={`inline-block px-2 py-1 rounded-full text-xs font-medium mt-1 ${getStatusColor(selectedFracture.status)}`}>
                        {getStatusLabel(selectedFracture.status)}
                      </span>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.treatedBy', 'Treated By')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.treatedBy}</p>
                    </div>
                  </div>
                  {selectedFracture.notes && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.notes', 'Notes')}</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedFracture.notes}</p>
                    </div>
                  )}
                  {selectedFracture.complications && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">{t('field.complications', 'Complications')}</label>
                      <p className="text-sm text-red-600 mt-1">{selectedFracture.complications}</p>
                    </div>
                  )}
                  <div className="flex justify-end gap-2 pt-4">
                    <button onClick={handleModalClose} className="btn btn-secondary">
                      {t('action.close', 'Close')}
                    </button>
                    <button
                      onClick={() => {
                        handleModalClose();
                        handleEdit(selectedFracture);
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
