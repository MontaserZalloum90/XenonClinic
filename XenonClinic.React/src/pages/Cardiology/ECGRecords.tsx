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
import type { ECGRecord, CreateECGRequest } from '../../types/cardiology';
import { ECGStatus, HeartRhythm } from '../../types/cardiology';

export const ECGRecords = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<ECGRecord | undefined>(undefined);

  // Mock data - Replace with actual API calls
  const { data: records, isLoading } = useQuery<ECGRecord[]>({
    queryKey: ['ecg-records'],
    queryFn: async () => {
      // Mock implementation
      return [
        {
          id: 1,
          patientId: 1001,
          patientName: 'John Smith',
          recordDate: new Date().toISOString(),
          heartRate: 72,
          rhythm: HeartRhythm.Sinus,
          interpretation: 'Normal sinus rhythm',
          abnormalities: [],
          performedBy: 'Dr. Johnson',
          status: ECGStatus.Completed,
          prInterval: 160,
          qrsDuration: 90,
          qtInterval: 380,
          qtcInterval: 410,
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 1002,
          patientName: 'Mary Williams',
          recordDate: new Date().toISOString(),
          heartRate: 105,
          rhythm: HeartRhythm.AFib,
          interpretation: 'Atrial fibrillation with rapid ventricular response',
          abnormalities: ['Irregular rhythm', 'Absent P waves'],
          performedBy: 'Dr. Brown',
          status: ECGStatus.Reviewed,
          reviewedBy: 'Dr. Johnson',
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 1003,
          patientName: 'Robert Davis',
          recordDate: new Date().toISOString(),
          heartRate: 58,
          rhythm: HeartRhythm.Bradycardia,
          interpretation: 'Sinus bradycardia',
          abnormalities: ['Low heart rate'],
          performedBy: 'Dr. Williams',
          status: ECGStatus.Pending,
          prInterval: 170,
          qrsDuration: 85,
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredRecords = records?.filter((record) => {
    const matchesSearch =
      !searchTerm ||
      record.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.interpretation.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || record.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedRecord(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (record: ECGRecord) => {
    setSelectedRecord(record);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedRecord(undefined);
  };

  const getStatusBadge = (status: ECGStatus) => {
    const config: Record<string, { className: string; label: string }> = {
      [ECGStatus.Pending]: { className: 'bg-yellow-100 text-yellow-800', label: 'Pending' },
      [ECGStatus.InProgress]: { className: 'bg-blue-100 text-blue-800', label: 'In Progress' },
      [ECGStatus.Completed]: { className: 'bg-green-100 text-green-800', label: 'Completed' },
      [ECGStatus.Reviewed]: { className: 'bg-purple-100 text-purple-800', label: 'Reviewed' },
    };
    const c = config[status] || { className: 'bg-gray-100 text-gray-800', label: status };
    return (
      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
        {c.label}
      </span>
    );
  };

  const getRhythmLabel = (rhythm: HeartRhythm): string => {
    const labels: Record<string, string> = {
      [HeartRhythm.Normal]: 'Normal',
      [HeartRhythm.Sinus]: 'Sinus Rhythm',
      [HeartRhythm.AFib]: 'Atrial Fibrillation',
      [HeartRhythm.AFlutter]: 'Atrial Flutter',
      [HeartRhythm.SVT]: 'SVT',
      [HeartRhythm.VTach]: 'V-Tach',
      [HeartRhythm.VFib]: 'V-Fib',
      [HeartRhythm.Bradycardia]: 'Bradycardia',
      [HeartRhythm.Tachycardia]: 'Tachycardia',
      [HeartRhythm.Other]: 'Other',
    };
    return labels[rhythm] || rhythm;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">ECG Records</h1>
          <p className="text-gray-600 mt-1">Manage electrocardiogram records and interpretations</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New ECG Record
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
                placeholder="Search by patient name or interpretation..."
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
              {Object.entries(ECGStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Records Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Heart Rate
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Rhythm
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Interpretation
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
                  <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading records...</p>
                  </td>
                </tr>
              ) : filteredRecords && filteredRecords.length > 0 ? (
                filteredRecords.map((record) => (
                  <tr key={record.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {record.patientName || `Patient #${record.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(record.recordDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {record.heartRate} bpm
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {getRhythmLabel(record.rhythm)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {record.interpretation}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">{getStatusBadge(record.status)}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {record.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleEdit(record)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {record.status === ECGStatus.Pending && (
                        <button className="text-green-600 hover:text-green-900">Review</button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                    {searchTerm ? 'No records found matching your search.' : 'No ECG records found.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <ECGRecordModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        record={selectedRecord}
      />
    </div>
  );
};

// ECG Record Form Modal
interface ECGRecordModalProps {
  isOpen: boolean;
  onClose: () => void;
  record?: ECGRecord;
}

const ECGRecordModal = ({ isOpen, onClose, record }: ECGRecordModalProps) => {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<Partial<CreateECGRequest>>({
    patientId: record?.patientId || 0,
    recordDate: record?.recordDate || new Date().toISOString().split('T')[0],
    heartRate: record?.heartRate || 70,
    rhythm: record?.rhythm || HeartRhythm.Sinus,
    interpretation: record?.interpretation || '',
    abnormalities: record?.abnormalities || [],
    performedBy: record?.performedBy || '',
    prInterval: record?.prInterval,
    qrsDuration: record?.qrsDuration,
    qtInterval: record?.qtInterval,
    qtcInterval: record?.qtcInterval,
    stChanges: record?.stChanges,
    tWaveChanges: record?.tWaveChanges,
    notes: record?.notes,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // In production, call API to save
    console.log('Saving ECG record:', formData);
    queryClient.invalidateQueries({ queryKey: ['ecg-records'] });
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'heartRate' || name.includes('Interval') ? Number(value) : value,
    }));
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {record ? 'View ECG Record' : 'New ECG Record'}
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
                    Record Date
                  </label>
                  <input
                    type="date"
                    name="recordDate"
                    value={formData.recordDate?.split('T')[0]}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Heart Rate (bpm)
                  </label>
                  <input
                    type="number"
                    name="heartRate"
                    value={formData.heartRate}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Rhythm</label>
                  <select
                    name="rhythm"
                    value={formData.rhythm}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(HeartRhythm).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    PR Interval (ms)
                  </label>
                  <input
                    type="number"
                    name="prInterval"
                    value={formData.prInterval || ''}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    QRS Duration (ms)
                  </label>
                  <input
                    type="number"
                    name="qrsDuration"
                    value={formData.qrsDuration || ''}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    QT Interval (ms)
                  </label>
                  <input
                    type="number"
                    name="qtInterval"
                    value={formData.qtInterval || ''}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    QTc Interval (ms)
                  </label>
                  <input
                    type="number"
                    name="qtcInterval"
                    value={formData.qtcInterval || ''}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>

                <div className="col-span-2">
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
                    Interpretation
                  </label>
                  <textarea
                    name="interpretation"
                    value={formData.interpretation}
                    onChange={handleChange}
                    rows={3}
                    className="input w-full"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    ST Changes
                  </label>
                  <input
                    type="text"
                    name="stChanges"
                    value={formData.stChanges || ''}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., ST elevation in leads II, III, aVF"
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    T Wave Changes
                  </label>
                  <input
                    type="text"
                    name="tWaveChanges"
                    value={formData.tWaveChanges || ''}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., T wave inversion in V1-V3"
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
                  <textarea
                    name="notes"
                    value={formData.notes || ''}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <button type="button" onClick={onClose} className="btn btn-outline">
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  {record ? 'Update' : 'Create'} Record
                </button>
              </div>
            </form>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
