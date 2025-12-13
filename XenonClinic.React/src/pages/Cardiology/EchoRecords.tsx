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
import type { EchocardiogramRecord, CreateEchoRequest } from '../../types/cardiology';
import { EchoStatus, ValveFinding } from '../../types/cardiology';

export const EchoRecords = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<EchocardiogramRecord | undefined>(undefined);

  // Mock data - Replace with actual API calls
  const { data: records, isLoading } = useQuery<EchocardiogramRecord[]>({
    queryKey: ['echo-records'],
    queryFn: async () => {
      // Mock implementation
      return [
        {
          id: 1,
          patientId: 1001,
          patientName: 'John Smith',
          date: new Date().toISOString(),
          ejectionFraction: 55,
          leftVentricleSize: 'Normal',
          rightVentricleSize: 'Normal',
          wallMotion: 'Normal global and regional wall motion',
          valveFindings: {
            mitral: ValveFinding.Normal,
            aortic: ValveFinding.Normal,
            tricuspid: ValveFinding.Normal,
            pulmonary: ValveFinding.Normal,
          },
          conclusions: 'Normal echocardiogram study',
          performedBy: 'Dr. Johnson',
          interpretedBy: 'Dr. Williams',
          status: EchoStatus.Reported,
          diastolicFunction: 'Normal',
          pericardialEffusion: false,
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 1002,
          patientName: 'Mary Williams',
          date: new Date().toISOString(),
          ejectionFraction: 35,
          leftVentricleSize: 'Dilated',
          rightVentricleSize: 'Normal',
          wallMotion: 'Reduced global systolic function with hypokinesis of anterior and anteroseptal walls',
          valveFindings: {
            mitral: ValveFinding.Regurgitation,
            aortic: ValveFinding.Normal,
            tricuspid: ValveFinding.Normal,
          },
          conclusions: 'Reduced left ventricular systolic function with EF 35%. Mild mitral regurgitation.',
          performedBy: 'Dr. Brown',
          status: EchoStatus.Completed,
          diastolicFunction: 'Impaired relaxation',
          pericardialEffusion: false,
          estimatedPAP: 35,
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 1003,
          patientName: 'Robert Davis',
          date: new Date().toISOString(),
          ejectionFraction: 62,
          leftVentricleSize: 'Normal',
          wallMotion: 'Normal',
          valveFindings: {
            mitral: ValveFinding.Normal,
            aortic: ValveFinding.Stenosis,
          },
          conclusions: 'Moderate aortic stenosis. Normal LV systolic function.',
          performedBy: 'Dr. Johnson',
          status: EchoStatus.InProgress,
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredRecords = records?.filter((record) => {
    const matchesSearch =
      !searchTerm ||
      record.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.conclusions.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || record.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedRecord(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (record: EchocardiogramRecord) => {
    setSelectedRecord(record);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedRecord(undefined);
  };

  const getStatusBadge = (status: EchoStatus) => {
    const config: Record<string, { className: string; label: string }> = {
      [EchoStatus.Scheduled]: { className: 'bg-blue-100 text-blue-800', label: 'Scheduled' },
      [EchoStatus.InProgress]: { className: 'bg-yellow-100 text-yellow-800', label: 'In Progress' },
      [EchoStatus.Completed]: { className: 'bg-green-100 text-green-800', label: 'Completed' },
      [EchoStatus.Reported]: { className: 'bg-purple-100 text-purple-800', label: 'Reported' },
    };
    const c = config[status] || { className: 'bg-gray-100 text-gray-800', label: status };
    return (
      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
        {c.label}
      </span>
    );
  };

  const getEFBadge = (ef: number) => {
    if (ef >= 50) {
      return <span className="text-green-600 font-medium">{ef}%</span>;
    } else if (ef >= 40) {
      return <span className="text-yellow-600 font-medium">{ef}%</span>;
    } else {
      return <span className="text-red-600 font-medium">{ef}%</span>;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Echocardiogram Records</h1>
          <p className="text-gray-600 mt-1">Manage echocardiography studies and results</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Echo Record
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
                placeholder="Search by patient name or conclusions..."
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
              {Object.entries(EchoStatus).map(([key, value]) => (
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
                  Ejection Fraction
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Wall Motion
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Valve Findings
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
                      {format(new Date(record.date), 'MMM d, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      {getEFBadge(record.ejectionFraction)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {record.wallMotion}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {Object.entries(record.valveFindings).map(([valve, finding]) => (
                        finding && finding !== ValveFinding.Normal && (
                          <div key={valve} className="text-xs">
                            {valve}: {finding}
                          </div>
                        )
                      ))}
                      {Object.values(record.valveFindings).every(
                        (f) => !f || f === ValveFinding.Normal
                      ) && <span className="text-green-600">All normal</span>}
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
                      {record.status === EchoStatus.Completed && !record.interpretedBy && (
                        <button className="text-green-600 hover:text-green-900">Interpret</button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                    {searchTerm
                      ? 'No records found matching your search.'
                      : 'No echocardiogram records found.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <EchoRecordModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        record={selectedRecord}
      />
    </div>
  );
};

// Echo Record Form Modal
interface EchoRecordModalProps {
  isOpen: boolean;
  onClose: () => void;
  record?: EchocardiogramRecord;
}

const EchoRecordModal = ({ isOpen, onClose, record }: EchoRecordModalProps) => {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<Partial<CreateEchoRequest>>({
    patientId: record?.patientId || 0,
    date: record?.date || new Date().toISOString().split('T')[0],
    ejectionFraction: record?.ejectionFraction || 55,
    leftVentricleSize: record?.leftVentricleSize || '',
    rightVentricleSize: record?.rightVentricleSize || '',
    wallMotion: record?.wallMotion || '',
    valveFindings: record?.valveFindings || {},
    conclusions: record?.conclusions || '',
    performedBy: record?.performedBy || '',
    diastolicFunction: record?.diastolicFunction || '',
    pericardialEffusion: record?.pericardialEffusion || false,
    estimatedPAP: record?.estimatedPAP,
    notes: record?.notes,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Saving echo record:', formData);
    queryClient.invalidateQueries({ queryKey: ['echo-records'] });
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    if (type === 'checkbox') {
      const checked = (e.target as HTMLInputElement).checked;
      setFormData((prev) => ({ ...prev, [name]: checked }));
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: name === 'ejectionFraction' || name === 'estimatedPAP' ? Number(value) : value,
      }));
    }
  };

  const handleValveChange = (valve: string, value: ValveFinding) => {
    setFormData((prev) => ({
      ...prev,
      valveFindings: {
        ...prev.valveFindings,
        [valve]: value,
      },
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
                {record ? 'View Echocardiogram Record' : 'New Echocardiogram Record'}
              </Dialog.Title>
              <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Basic Info */}
              <div className="grid grid-cols-3 gap-4">
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
                  <label className="block text-sm font-medium text-gray-700 mb-1">Date</label>
                  <input
                    type="date"
                    name="date"
                    value={formData.date?.split('T')[0]}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
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
              </div>

              {/* Cardiac Measurements */}
              <div>
                <h3 className="text-sm font-semibold text-gray-900 mb-3">Cardiac Measurements</h3>
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Ejection Fraction (%)
                    </label>
                    <input
                      type="number"
                      name="ejectionFraction"
                      value={formData.ejectionFraction}
                      onChange={handleChange}
                      min="0"
                      max="100"
                      className="input w-full"
                      required
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      LV Size
                    </label>
                    <input
                      type="text"
                      name="leftVentricleSize"
                      value={formData.leftVentricleSize}
                      onChange={handleChange}
                      placeholder="e.g., Normal, Dilated"
                      className="input w-full"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      RV Size
                    </label>
                    <input
                      type="text"
                      name="rightVentricleSize"
                      value={formData.rightVentricleSize}
                      onChange={handleChange}
                      placeholder="e.g., Normal, Dilated"
                      className="input w-full"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Diastolic Function
                    </label>
                    <input
                      type="text"
                      name="diastolicFunction"
                      value={formData.diastolicFunction}
                      onChange={handleChange}
                      placeholder="e.g., Normal, Impaired"
                      className="input w-full"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Estimated PAP (mmHg)
                    </label>
                    <input
                      type="number"
                      name="estimatedPAP"
                      value={formData.estimatedPAP || ''}
                      onChange={handleChange}
                      className="input w-full"
                    />
                  </div>

                  <div className="flex items-center pt-7">
                    <input
                      type="checkbox"
                      name="pericardialEffusion"
                      checked={formData.pericardialEffusion}
                      onChange={handleChange}
                      className="h-4 w-4 text-primary-600 rounded"
                    />
                    <label className="ml-2 text-sm text-gray-700">Pericardial Effusion</label>
                  </div>
                </div>
              </div>

              {/* Valve Findings */}
              <div>
                <h3 className="text-sm font-semibold text-gray-900 mb-3">Valve Findings</h3>
                <div className="grid grid-cols-2 gap-4">
                  {['mitral', 'aortic', 'tricuspid', 'pulmonary'].map((valve) => (
                    <div key={valve}>
                      <label className="block text-sm font-medium text-gray-700 mb-1 capitalize">
                        {valve} Valve
                      </label>
                      <select
                        value={formData.valveFindings?.[valve as keyof typeof formData.valveFindings] || ''}
                        onChange={(e) => handleValveChange(valve, e.target.value as ValveFinding)}
                        className="input w-full"
                      >
                        <option value="">Not assessed</option>
                        {Object.entries(ValveFinding).map(([key, value]) => (
                          <option key={value} value={value}>
                            {key}
                          </option>
                        ))}
                      </select>
                    </div>
                  ))}
                </div>
              </div>

              {/* Wall Motion & Conclusions */}
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Wall Motion
                  </label>
                  <textarea
                    name="wallMotion"
                    value={formData.wallMotion}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="Describe wall motion abnormalities"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Conclusions
                  </label>
                  <textarea
                    name="conclusions"
                    value={formData.conclusions}
                    onChange={handleChange}
                    rows={3}
                    className="input w-full"
                    placeholder="Summary and conclusions"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
                  <textarea
                    name="notes"
                    value={formData.notes || ''}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="Additional notes"
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
