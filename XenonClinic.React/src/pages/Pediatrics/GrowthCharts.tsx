import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  MagnifyingGlassIcon,
  PlusIcon,
  ChartBarIcon,
} from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import type { GrowthMeasurement, CreateGrowthMeasurementRequest } from '../../types/pediatrics';

// Mock API functions - Replace with actual API calls
const growthApi = {
  getAll: async () => ({
    data: [] as GrowthMeasurement[],
  }),
  create: async (data: CreateGrowthMeasurementRequest) => ({
    data: { id: Date.now(), ...data, bmi: 0, createdAt: new Date().toISOString() },
  }),
  update: async (id: number, data: Partial<GrowthMeasurement>) => ({
    data: { id, ...data },
  }),
  delete: async () => ({
    data: { success: true },
  }),
};

export const GrowthCharts = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedMeasurement, setSelectedMeasurement] = useState<
    GrowthMeasurement | undefined
  >(undefined);

  // Fetch growth measurements
  const { data: measurementsData, isLoading } = useQuery({
    queryKey: ['growth-measurements'],
    queryFn: () => growthApi.getAll(),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => growthApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['growth-measurements'] });
    },
  });

  const measurements = measurementsData?.data || [];

  // Filter measurements
  const filteredMeasurements = measurements.filter((measurement) => {
    const matchesSearch =
      !searchTerm ||
      measurement.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      measurement.patientId.toString().includes(searchTerm);
    return matchesSearch;
  });

  const handleDelete = (measurement: GrowthMeasurement) => {
    if (
      window.confirm(
        `Are you sure you want to delete the measurement for ${measurement.patientName}?`
      )
    ) {
      deleteMutation.mutate(measurement.id);
    }
  };

  const handleEdit = (measurement: GrowthMeasurement) => {
    setSelectedMeasurement(measurement);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedMeasurement(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedMeasurement(undefined);
  };

  const getPercentileColor = (percentile?: number) => {
    if (!percentile) return 'text-gray-600';
    if (percentile < 5 || percentile > 95) return 'text-red-600';
    if (percentile < 10 || percentile > 90) return 'text-orange-600';
    return 'text-green-600';
  };

  const getGrowthTrend = (measurements: GrowthMeasurement[], patientId: number) => {
    const patientMeasurements = measurements
      .filter((m) => m.patientId === patientId)
      .sort((a, b) => new Date(a.measurementDate).getTime() - new Date(b.measurementDate).getTime());

    if (patientMeasurements.length < 2) return null;

    const latest = patientMeasurements[patientMeasurements.length - 1];
    const previous = patientMeasurements[patientMeasurements.length - 2];

    const weightChange = latest.weight - previous.weight;
    const heightChange = latest.height - previous.height;

    return { weightChange, heightChange };
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Growth Charts</h1>
          <p className="text-gray-600 mt-1">
            Track pediatric growth measurements and percentiles
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2 inline" />
          New Measurement
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Measurements</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{measurements.length}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">This Month</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {
              measurements.filter(
                (m) =>
                  new Date(m.measurementDate).getMonth() === new Date().getMonth() &&
                  new Date(m.measurementDate).getFullYear() === new Date().getFullYear()
              ).length
            }
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Growth Concerns</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">
            {
              measurements.filter(
                (m) =>
                  (m.weightPercentile && (m.weightPercentile < 5 || m.weightPercentile > 95)) ||
                  (m.heightPercentile && (m.heightPercentile < 5 || m.heightPercentile > 95))
              ).length
            }
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Unique Patients</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {new Set(measurements.map((m) => m.patientId)).size}
          </p>
        </div>
      </div>

      {/* Search and Filters */}
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

        {/* Measurements List */}
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading measurements...</p>
          </div>
        ) : filteredMeasurements.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Age
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Weight
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Height
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    BMI
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Percentiles
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredMeasurements.map((measurement) => {
                  const trend = getGrowthTrend(measurements, measurement.patientId);
                  return (
                    <tr key={measurement.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm">
                        <div className="font-medium text-gray-900">
                          {measurement.patientName || `Patient #${measurement.patientId}`}
                        </div>
                        <div className="text-gray-500">ID: {measurement.patientId}</div>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {format(new Date(measurement.measurementDate), 'MMM d, yyyy')}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {Math.floor(measurement.age / 12)}y {measurement.age % 12}m
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="text-gray-900 font-medium">{measurement.weight} kg</div>
                        {trend && (
                          <div
                            className={`text-xs ${
                              trend.weightChange > 0 ? 'text-green-600' : 'text-gray-500'
                            }`}
                          >
                            {trend.weightChange > 0 ? '+' : ''}
                            {trend.weightChange.toFixed(1)} kg
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="text-gray-900 font-medium">{measurement.height} cm</div>
                        {trend && (
                          <div
                            className={`text-xs ${
                              trend.heightChange > 0 ? 'text-green-600' : 'text-gray-500'
                            }`}
                          >
                            {trend.heightChange > 0 ? '+' : ''}
                            {trend.heightChange.toFixed(1)} cm
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {measurement.bmi.toFixed(1)}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="space-y-1">
                          {measurement.weightPercentile !== undefined && (
                            <div className="flex items-center gap-1">
                              <span className="text-gray-500 text-xs">W:</span>
                              <span
                                className={`font-medium ${getPercentileColor(
                                  measurement.weightPercentile
                                )}`}
                              >
                                {measurement.weightPercentile}%
                              </span>
                            </div>
                          )}
                          {measurement.heightPercentile !== undefined && (
                            <div className="flex items-center gap-1">
                              <span className="text-gray-500 text-xs">H:</span>
                              <span
                                className={`font-medium ${getPercentileColor(
                                  measurement.heightPercentile
                                )}`}
                              >
                                {measurement.heightPercentile}%
                              </span>
                            </div>
                          )}
                          {measurement.bmiPercentile !== undefined && (
                            <div className="flex items-center gap-1">
                              <span className="text-gray-500 text-xs">BMI:</span>
                              <span
                                className={`font-medium ${getPercentileColor(
                                  measurement.bmiPercentile
                                )}`}
                              >
                                {measurement.bmiPercentile}%
                              </span>
                            </div>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleEdit(measurement)}
                            className="text-primary-600 hover:text-primary-800"
                          >
                            View
                          </button>
                          <button
                            onClick={() => handleDelete(measurement)}
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
            <ChartBarIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-500">
              {searchTerm ? 'No measurements found matching your search.' : 'No growth measurements found.'}
            </p>
            <button
              onClick={handleCreate}
              className="mt-4 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
            >
              <PlusIcon className="h-5 w-5 mr-2" />
              Add First Measurement
            </button>
          </div>
        )}
      </div>

      {/* Growth Chart Visualization Placeholder */}
      <div className="card">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Growth Chart Visualization</h3>
        <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
          <ChartBarIcon className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">Growth chart visualization will be displayed here</p>
          <p className="text-sm text-gray-400 mt-2">
            Interactive charts showing weight, height, and BMI percentiles over time
          </p>
        </div>
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedMeasurement ? 'View/Edit Growth Measurement' : 'New Growth Measurement'}
              </Dialog.Title>
              <GrowthMeasurementForm
                measurement={selectedMeasurement}
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

// Growth Measurement Form Component
interface GrowthMeasurementFormProps {
  measurement?: GrowthMeasurement;
  onSuccess: () => void;
  onCancel: () => void;
}

const GrowthMeasurementForm = ({
  measurement,
  onSuccess,
  onCancel,
}: GrowthMeasurementFormProps) => {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<CreateGrowthMeasurementRequest>({
    patientId: measurement?.patientId || 0,
    measurementDate: measurement?.measurementDate || new Date().toISOString().split('T')[0],
    age: measurement?.age || 0,
    weight: measurement?.weight || 0,
    height: measurement?.height || 0,
    headCircumference: measurement?.headCircumference,
    recordedBy: measurement?.recordedBy || '',
    notes: measurement?.notes,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateGrowthMeasurementRequest) => growthApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['growth-measurements'] });
      onSuccess();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(formData);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'patientId' || name === 'age' || name === 'weight' || name === 'height' || name === 'headCircumference'
        ? parseFloat(value) || 0
        : value,
    }));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Patient ID</label>
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
          <label className="block text-sm font-medium text-gray-700 mb-1">Measurement Date</label>
          <input
            type="date"
            name="measurementDate"
            value={formData.measurementDate}
            onChange={handleChange}
            className="input w-full"
            required
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Age (months)</label>
          <input
            type="number"
            name="age"
            value={formData.age}
            onChange={handleChange}
            className="input w-full"
            required
            min="0"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Weight (kg)</label>
          <input
            type="number"
            name="weight"
            value={formData.weight}
            onChange={handleChange}
            className="input w-full"
            required
            step="0.1"
            min="0"
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Height (cm)</label>
          <input
            type="number"
            name="height"
            value={formData.height}
            onChange={handleChange}
            className="input w-full"
            required
            step="0.1"
            min="0"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Head Circumference (cm)
          </label>
          <input
            type="number"
            name="headCircumference"
            value={formData.headCircumference || ''}
            onChange={handleChange}
            className="input w-full"
            step="0.1"
            min="0"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Recorded By</label>
        <input
          type="text"
          name="recordedBy"
          value={formData.recordedBy}
          onChange={handleChange}
          className="input w-full"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
        <textarea
          name="notes"
          value={formData.notes || ''}
          onChange={handleChange}
          className="input w-full"
          rows={3}
        />
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-outline">
          Cancel
        </button>
        <button
          type="submit"
          className="btn btn-primary"
          disabled={createMutation.isPending}
        >
          {createMutation.isPending ? 'Saving...' : measurement ? 'Update' : 'Create'}
        </button>
      </div>
    </form>
  );
};
