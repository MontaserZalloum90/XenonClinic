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
import type { StressTest, CreateStressTestRequest } from '../../types/cardiology';
import { StressTestType, StressTestProtocol, StressTestResult } from '../../types/cardiology';

export const StressTests = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [resultFilter, setResultFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTest, setSelectedTest] = useState<StressTest | undefined>(undefined);

  // Mock data - Replace with actual API calls
  const { data: tests, isLoading } = useQuery<StressTest[]>({
    queryKey: ['stress-tests'],
    queryFn: async () => {
      // Mock implementation
      return [
        {
          id: 1,
          patientId: 1001,
          patientName: 'John Smith',
          testType: StressTestType.Exercise,
          protocol: StressTestProtocol.Bruce,
          date: new Date().toISOString(),
          duration: 12,
          maxHeartRate: 165,
          targetHeartRate: 160,
          percentTargetAchieved: 103,
          bloodPressure: { systolic: 130, diastolic: 80 },
          peakBloodPressure: { systolic: 180, diastolic: 90 },
          symptoms: [],
          ecgChanges: 'None',
          testResult: StressTestResult.Negative,
          conclusion: 'Negative stress test. Good exercise capacity.',
          performedBy: 'Dr. Johnson',
          interpretedBy: 'Dr. Williams',
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 1002,
          patientName: 'Mary Williams',
          testType: StressTestType.Pharmacological,
          protocol: StressTestProtocol.Dobutamine,
          date: new Date().toISOString(),
          duration: 15,
          maxHeartRate: 145,
          targetHeartRate: 140,
          percentTargetAchieved: 104,
          bloodPressure: { systolic: 140, diastolic: 85 },
          peakBloodPressure: { systolic: 170, diastolic: 88 },
          symptoms: ['Chest discomfort'],
          ecgChanges: 'ST segment depression in leads V4-V6',
          stSegmentChanges: '2mm horizontal ST depression in V4-V6',
          testResult: StressTestResult.Positive,
          conclusion: 'Positive stress test with inducible ischemia. Recommend coronary angiography.',
          recommendations: 'Coronary angiography, Medical optimization',
          performedBy: 'Dr. Brown',
          interpretedBy: 'Dr. Johnson',
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 1003,
          patientName: 'Robert Davis',
          testType: StressTestType.Exercise,
          protocol: StressTestProtocol.ModifiedBruce,
          date: new Date().toISOString(),
          duration: 8,
          maxHeartRate: 120,
          targetHeartRate: 150,
          percentTargetAchieved: 80,
          bloodPressure: { systolic: 135, diastolic: 82 },
          symptoms: ['Dyspnea', 'Fatigue'],
          testResult: StressTestResult.Equivocal,
          conclusion: 'Equivocal stress test due to submaximal heart rate achievement.',
          recommendations: 'Consider pharmacological stress test',
          performedBy: 'Dr. Williams',
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredTests = tests?.filter((test) => {
    const matchesSearch =
      !searchTerm ||
      test.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      test.conclusion.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesResult = !resultFilter || test.testResult === resultFilter;
    return matchesSearch && matchesResult;
  });

  const handleCreate = () => {
    setSelectedTest(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (test: StressTest) => {
    setSelectedTest(test);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedTest(undefined);
  };

  const getResultBadge = (result: StressTestResult) => {
    const config: Record<string, { className: string; label: string }> = {
      [StressTestResult.Negative]: { className: 'bg-green-100 text-green-800', label: 'Negative' },
      [StressTestResult.Positive]: { className: 'bg-red-100 text-red-800', label: 'Positive' },
      [StressTestResult.Equivocal]: { className: 'bg-yellow-100 text-yellow-800', label: 'Equivocal' },
      [StressTestResult.Uninterpretable]: { className: 'bg-gray-100 text-gray-800', label: 'Uninterpretable' },
    };
    const c = config[result] || { className: 'bg-gray-100 text-gray-800', label: result };
    return (
      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
        {c.label}
      </span>
    );
  };

  const getTestTypeLabel = (type: StressTestType): string => {
    const labels: Record<string, string> = {
      [StressTestType.Exercise]: 'Exercise',
      [StressTestType.Pharmacological]: 'Pharmacological',
      [StressTestType.Nuclear]: 'Nuclear',
      [StressTestType.Echo]: 'Echo',
    };
    return labels[type] || type;
  };

  const getProtocolLabel = (protocol: StressTestProtocol): string => {
    const labels: Record<string, string> = {
      [StressTestProtocol.Bruce]: 'Bruce',
      [StressTestProtocol.ModifiedBruce]: 'Modified Bruce',
      [StressTestProtocol.Naughton]: 'Naughton',
      [StressTestProtocol.Balke]: 'Balke',
      [StressTestProtocol.Dobutamine]: 'Dobutamine',
      [StressTestProtocol.Adenosine]: 'Adenosine',
      [StressTestProtocol.Dipyridamole]: 'Dipyridamole',
    };
    return labels[protocol] || protocol;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Stress Tests</h1>
          <p className="text-gray-600 mt-1">Manage cardiac stress test records</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Stress Test
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
                placeholder="Search by patient name or conclusion..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
              />
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={resultFilter}
              onChange={(e) => setResultFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Results</option>
              {Object.entries(StressTestResult).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Tests Table */}
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
                  Type / Protocol
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Duration
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Max HR
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Peak BP
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Result
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
                  <td colSpan={9} className="px-6 py-12 text-center text-gray-500">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading tests...</p>
                  </td>
                </tr>
              ) : filteredTests && filteredTests.length > 0 ? (
                filteredTests.map((test) => (
                  <tr key={test.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {test.patientName || `Patient #${test.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(test.date), 'MMM d, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      <div>{getTestTypeLabel(test.testType)}</div>
                      <div className="text-xs text-gray-500">{getProtocolLabel(test.protocol)}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {test.duration} min
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {test.maxHeartRate} bpm
                      {test.percentTargetAchieved && (
                        <div className="text-xs text-gray-500">
                          ({test.percentTargetAchieved}% target)
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {test.peakBloodPressure
                        ? `${test.peakBloodPressure.systolic}/${test.peakBloodPressure.diastolic}`
                        : '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">{getResultBadge(test.testResult)}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {test.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleEdit(test)}
                        className="text-primary-600 hover:text-primary-900"
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={9} className="px-6 py-12 text-center text-gray-500">
                    {searchTerm
                      ? 'No tests found matching your search.'
                      : 'No stress tests found.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <StressTestModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        test={selectedTest}
      />
    </div>
  );
};

// Stress Test Form Modal
interface StressTestModalProps {
  isOpen: boolean;
  onClose: () => void;
  test?: StressTest;
}

const StressTestModal = ({ isOpen, onClose, test }: StressTestModalProps) => {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<Partial<CreateStressTestRequest>>({
    patientId: test?.patientId || 0,
    testType: test?.testType || StressTestType.Exercise,
    protocol: test?.protocol || StressTestProtocol.Bruce,
    date: test?.date || new Date().toISOString().split('T')[0],
    duration: test?.duration || 0,
    maxHeartRate: test?.maxHeartRate || 0,
    targetHeartRate: test?.targetHeartRate,
    bloodPressure: test?.bloodPressure || { systolic: 120, diastolic: 80 },
    peakBloodPressure: test?.peakBloodPressure,
    symptoms: test?.symptoms || [],
    ecgChanges: test?.ecgChanges || '',
    stSegmentChanges: test?.stSegmentChanges || '',
    arrhythmias: test?.arrhythmias || '',
    testResult: test?.testResult || StressTestResult.Negative,
    conclusion: test?.conclusion || '',
    recommendations: test?.recommendations || '',
    performedBy: test?.performedBy || '',
    notes: test?.notes || '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Implement actual API call to save stress test
    void formData;
    queryClient.invalidateQueries({ queryKey: ['stress-tests'] });
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: ['duration', 'maxHeartRate', 'targetHeartRate'].includes(name)
        ? Number(value)
        : value,
    }));
  };

  const handleBPChange = (field: 'bloodPressure' | 'peakBloodPressure', type: 'systolic' | 'diastolic', value: number) => {
    setFormData((prev) => ({
      ...prev,
      [field]: {
        ...prev[field],
        [type]: value,
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
                {test ? 'View Stress Test' : 'New Stress Test'}
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

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Test Type
                  </label>
                  <select
                    name="testType"
                    value={formData.testType}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(StressTestType).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Protocol</label>
                  <select
                    name="protocol"
                    value={formData.protocol}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(StressTestProtocol).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Duration (min)
                  </label>
                  <input
                    type="number"
                    name="duration"
                    value={formData.duration}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>
              </div>

              {/* Hemodynamics */}
              <div>
                <h3 className="text-sm font-semibold text-gray-900 mb-3">Hemodynamics</h3>
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Max Heart Rate (bpm)
                    </label>
                    <input
                      type="number"
                      name="maxHeartRate"
                      value={formData.maxHeartRate}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Target HR (bpm)
                    </label>
                    <input
                      type="number"
                      name="targetHeartRate"
                      value={formData.targetHeartRate || ''}
                      onChange={handleChange}
                      className="input w-full"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Baseline BP (mmHg)
                    </label>
                    <div className="flex gap-2">
                      <input
                        type="number"
                        value={formData.bloodPressure?.systolic || ''}
                        onChange={(e) =>
                          handleBPChange('bloodPressure', 'systolic', Number(e.target.value))
                        }
                        placeholder="Systolic"
                        className="input w-full"
                      />
                      <span className="self-center">/</span>
                      <input
                        type="number"
                        value={formData.bloodPressure?.diastolic || ''}
                        onChange={(e) =>
                          handleBPChange('bloodPressure', 'diastolic', Number(e.target.value))
                        }
                        placeholder="Diastolic"
                        className="input w-full"
                      />
                    </div>
                  </div>

                  <div className="col-span-3">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Peak BP (mmHg)
                    </label>
                    <div className="flex gap-2 max-w-md">
                      <input
                        type="number"
                        value={formData.peakBloodPressure?.systolic || ''}
                        onChange={(e) =>
                          handleBPChange('peakBloodPressure', 'systolic', Number(e.target.value))
                        }
                        placeholder="Systolic"
                        className="input w-full"
                      />
                      <span className="self-center">/</span>
                      <input
                        type="number"
                        value={formData.peakBloodPressure?.diastolic || ''}
                        onChange={(e) =>
                          handleBPChange('peakBloodPressure', 'diastolic', Number(e.target.value))
                        }
                        placeholder="Diastolic"
                        className="input w-full"
                      />
                    </div>
                  </div>
                </div>
              </div>

              {/* Findings */}
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    ECG Changes
                  </label>
                  <input
                    type="text"
                    name="ecgChanges"
                    value={formData.ecgChanges}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., None, ST changes"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    ST Segment Changes
                  </label>
                  <input
                    type="text"
                    name="stSegmentChanges"
                    value={formData.stSegmentChanges}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., 2mm ST depression in V4-V6"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Arrhythmias
                  </label>
                  <input
                    type="text"
                    name="arrhythmias"
                    value={formData.arrhythmias}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., Occasional PVCs"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Test Result
                  </label>
                  <select
                    name="testResult"
                    value={formData.testResult}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(StressTestResult).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Conclusion
                  </label>
                  <textarea
                    name="conclusion"
                    value={formData.conclusion}
                    onChange={handleChange}
                    rows={3}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Recommendations
                  </label>
                  <textarea
                    name="recommendations"
                    value={formData.recommendations}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
                  <textarea
                    name="notes"
                    value={formData.notes}
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
                  {test ? 'Update' : 'Create'} Test
                </button>
              </div>
            </form>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
