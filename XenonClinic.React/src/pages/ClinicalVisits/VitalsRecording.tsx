import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  HeartIcon,
  MagnifyingGlassIcon,
  PlusIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
} from '@heroicons/react/24/outline';
import { Dialog } from '@headlessui/react';
import { api } from '../../lib/api';

interface VitalSign {
  id: number;
  visitId: number;
  patientId: number;
  patientName: string;
  patientMRN: string;
  recordedBy: string;
  recordedAt: string;
  temperature: number;
  temperatureUnit: 'C' | 'F';
  bloodPressureSystolic: number;
  bloodPressureDiastolic: number;
  heartRate: number;
  respiratoryRate: number;
  oxygenSaturation: number;
  weight: number;
  weightUnit: 'kg' | 'lb';
  height: number;
  heightUnit: 'cm' | 'in';
  bmi?: number;
  painLevel?: number;
  bloodGlucose?: number;
  notes?: string;
  alerts: VitalAlert[];
}

interface VitalAlert {
  type: 'warning' | 'critical';
  vital: string;
  message: string;
}

const vitalRanges = {
  temperature: { min: 36.1, max: 37.2, unit: '°C' },
  bloodPressureSystolic: { min: 90, max: 140, unit: 'mmHg' },
  bloodPressureDiastolic: { min: 60, max: 90, unit: 'mmHg' },
  heartRate: { min: 60, max: 100, unit: 'bpm' },
  respiratoryRate: { min: 12, max: 20, unit: '/min' },
  oxygenSaturation: { min: 95, max: 100, unit: '%' },
};

const getVitalStatus = (value: number, min: number, max: number): 'normal' | 'warning' | 'critical' => {
  if (value >= min && value <= max) return 'normal';
  const deviation = value < min ? (min - value) / min : (value - max) / max;
  return deviation > 0.2 ? 'critical' : 'warning';
};

export function VitalsRecording() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPatient] = useState<{ id: number; name: string; mrn: string } | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [dateFilter, setDateFilter] = useState(new Date().toISOString().split('T')[0]);

  const { data: vitals = [], isLoading } = useQuery({
    queryKey: ['vitals', dateFilter],
    queryFn: () => api.get<VitalSign[]>(`/api/clinical/vitals?date=${dateFilter}`),
  });

  const filteredVitals = vitals.filter((vital) =>
    vital.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    vital.patientMRN.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const formatTime = (dateString: string) => {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const alertCount = vitals.reduce((acc, v) => acc + v.alerts.length, 0);
  const criticalCount = vitals.reduce((acc, v) => acc + v.alerts.filter((a) => a.type === 'critical').length, 0);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Vitals Recording</h1>
          <p className="mt-1 text-sm text-gray-500">
            Record and monitor patient vital signs
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Record Vitals
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <HeartIcon className="h-8 w-8 text-blue-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">{vitals.length}</div>
              <div className="text-sm text-gray-500">Recorded Today</div>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <CheckCircleIcon className="h-8 w-8 text-green-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">
                {vitals.filter((v) => v.alerts.length === 0).length}
              </div>
              <div className="text-sm text-gray-500">Normal</div>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-8 w-8 text-yellow-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">{alertCount}</div>
              <div className="text-sm text-gray-500">Alerts</div>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-8 w-8 text-red-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">{criticalCount}</div>
              <div className="text-sm text-gray-500">Critical</div>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search patients..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <input
          type="date"
          value={dateFilter}
          onChange={(e) => setDateFilter(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
        />
      </div>

      {/* Vitals List */}
      <div className="space-y-4">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredVitals.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center rounded-lg bg-white shadow text-gray-500">
            <HeartIcon className="h-12 w-12 mb-2" />
            <p>No vitals recorded</p>
          </div>
        ) : (
          filteredVitals.map((vital) => (
            <div
              key={vital.id}
              className={`rounded-lg bg-white shadow ${
                vital.alerts.some((a) => a.type === 'critical')
                  ? 'ring-2 ring-red-500'
                  : vital.alerts.length > 0
                  ? 'ring-2 ring-yellow-500'
                  : ''
              }`}
            >
              <div className="p-4 sm:p-6">
                {/* Patient Info Row */}
                <div className="flex items-center justify-between mb-4">
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                      <HeartIcon className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900">{vital.patientName}</h3>
                      <p className="text-sm text-gray-500">MRN: {vital.patientMRN}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-900">{formatTime(vital.recordedAt)}</p>
                    <p className="text-xs text-gray-500">by {vital.recordedBy}</p>
                  </div>
                </div>

                {/* Alerts */}
                {vital.alerts.length > 0 && (
                  <div className="mb-4 space-y-2">
                    {vital.alerts.map((alert, index) => (
                      <div
                        key={index}
                        className={`flex items-center gap-2 rounded-md px-3 py-2 text-sm ${
                          alert.type === 'critical'
                            ? 'bg-red-50 text-red-800'
                            : 'bg-yellow-50 text-yellow-800'
                        }`}
                      >
                        <ExclamationTriangleIcon className="h-4 w-4" />
                        <span className="font-medium">{alert.vital}:</span>
                        <span>{alert.message}</span>
                      </div>
                    ))}
                  </div>
                )}

                {/* Vitals Grid */}
                <div className="grid grid-cols-2 gap-4 sm:grid-cols-4 lg:grid-cols-6">
                  <VitalCard
                    label="Temperature"
                    value={vital.temperature}
                    unit={vital.temperatureUnit === 'C' ? '°C' : '°F'}
                    status={getVitalStatus(
                      vital.temperatureUnit === 'F' ? (vital.temperature - 32) * 5 / 9 : vital.temperature,
                      vitalRanges.temperature.min,
                      vitalRanges.temperature.max
                    )}
                  />
                  <VitalCard
                    label="Blood Pressure"
                    value={`${vital.bloodPressureSystolic}/${vital.bloodPressureDiastolic}`}
                    unit="mmHg"
                    status={
                      getVitalStatus(vital.bloodPressureSystolic, vitalRanges.bloodPressureSystolic.min, vitalRanges.bloodPressureSystolic.max) === 'critical' ||
                      getVitalStatus(vital.bloodPressureDiastolic, vitalRanges.bloodPressureDiastolic.min, vitalRanges.bloodPressureDiastolic.max) === 'critical'
                        ? 'critical'
                        : getVitalStatus(vital.bloodPressureSystolic, vitalRanges.bloodPressureSystolic.min, vitalRanges.bloodPressureSystolic.max) === 'warning' ||
                          getVitalStatus(vital.bloodPressureDiastolic, vitalRanges.bloodPressureDiastolic.min, vitalRanges.bloodPressureDiastolic.max) === 'warning'
                        ? 'warning'
                        : 'normal'
                    }
                  />
                  <VitalCard
                    label="Heart Rate"
                    value={vital.heartRate}
                    unit="bpm"
                    status={getVitalStatus(vital.heartRate, vitalRanges.heartRate.min, vitalRanges.heartRate.max)}
                  />
                  <VitalCard
                    label="Resp. Rate"
                    value={vital.respiratoryRate}
                    unit="/min"
                    status={getVitalStatus(vital.respiratoryRate, vitalRanges.respiratoryRate.min, vitalRanges.respiratoryRate.max)}
                  />
                  <VitalCard
                    label="SpO2"
                    value={vital.oxygenSaturation}
                    unit="%"
                    status={getVitalStatus(vital.oxygenSaturation, vitalRanges.oxygenSaturation.min, vitalRanges.oxygenSaturation.max)}
                  />
                  {vital.bmi && (
                    <VitalCard
                      label="BMI"
                      value={vital.bmi.toFixed(1)}
                      unit="kg/m²"
                      status="normal"
                    />
                  )}
                </div>

                {/* Additional Info */}
                <div className="mt-4 pt-4 border-t flex flex-wrap gap-4 text-sm text-gray-500">
                  <span>Weight: {vital.weight} {vital.weightUnit}</span>
                  <span>Height: {vital.height} {vital.heightUnit}</span>
                  {vital.painLevel !== undefined && (
                    <span className={vital.painLevel > 5 ? 'text-red-600 font-medium' : ''}>
                      Pain Level: {vital.painLevel}/10
                    </span>
                  )}
                  {vital.bloodGlucose !== undefined && (
                    <span>Blood Glucose: {vital.bloodGlucose} mg/dL</span>
                  )}
                </div>

                {vital.notes && (
                  <div className="mt-2 text-sm text-gray-600">
                    <span className="font-medium">Notes:</span> {vital.notes}
                  </div>
                )}
              </div>
            </div>
          ))
        )}
      </div>

      {/* Record Vitals Modal */}
      <VitalsModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        patient={selectedPatient}
      />
    </div>
  );
}

function VitalCard({
  label,
  value,
  unit,
  status,
}: {
  label: string;
  value: number | string;
  unit: string;
  status: 'normal' | 'warning' | 'critical';
}) {
  const statusColors = {
    normal: 'bg-green-50 border-green-200',
    warning: 'bg-yellow-50 border-yellow-200',
    critical: 'bg-red-50 border-red-200',
  };

  const textColors = {
    normal: 'text-green-700',
    warning: 'text-yellow-700',
    critical: 'text-red-700',
  };

  return (
    <div className={`rounded-lg border p-3 ${statusColors[status]}`}>
      <div className="text-xs text-gray-500 mb-1">{label}</div>
      <div className={`text-lg font-bold ${textColors[status]}`}>
        {value}
        <span className="text-xs font-normal ml-1">{unit}</span>
      </div>
    </div>
  );
}

function VitalsModal({
  isOpen,
  onClose,
  patient,
}: {
  isOpen: boolean;
  onClose: () => void;
  patient: { id: number; name: string; mrn: string } | null;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    patientId: patient?.id || 0,
    patientName: patient?.name || '',
    patientMRN: patient?.mrn || '',
    temperature: 37.0,
    temperatureUnit: 'C' as const,
    bloodPressureSystolic: 120,
    bloodPressureDiastolic: 80,
    heartRate: 72,
    respiratoryRate: 16,
    oxygenSaturation: 98,
    weight: 70,
    weightUnit: 'kg' as const,
    height: 170,
    heightUnit: 'cm' as const,
    painLevel: 0,
    bloodGlucose: undefined as number | undefined,
    notes: '',
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) => api.post('/api/clinical/vitals', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vitals'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  const calculateBMI = () => {
    const weightKg = formData.weightUnit === 'lb' ? formData.weight * 0.453592 : formData.weight;
    const heightM = formData.heightUnit === 'in' ? formData.height * 0.0254 : formData.height / 100;
    return (weightKg / (heightM * heightM)).toFixed(1);
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            Record Vital Signs
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Patient Selection */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Patient Name</label>
                <input
                  type="text"
                  required
                  value={formData.patientName}
                  onChange={(e) => setFormData({ ...formData, patientName: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">MRN</label>
                <input
                  type="text"
                  required
                  value={formData.patientMRN}
                  onChange={(e) => setFormData({ ...formData, patientMRN: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>

            {/* Vital Signs */}
            <div className="border-t pt-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Vital Signs</h4>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Temperature</label>
                  <div className="mt-1 flex">
                    <input
                      type="number"
                      step="0.1"
                      required
                      value={formData.temperature}
                      onChange={(e) => setFormData({ ...formData, temperature: Number(e.target.value) })}
                      className="block w-full rounded-l-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                    />
                    <select
                      value={formData.temperatureUnit}
                      onChange={(e) => setFormData({ ...formData, temperatureUnit: e.target.value as 'C' | 'F' })}
                      className="rounded-r-md border border-l-0 border-gray-300 bg-gray-50 px-3 py-2"
                    >
                      <option value="C">°C</option>
                      <option value="F">°F</option>
                    </select>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Blood Pressure</label>
                  <div className="mt-1 flex items-center gap-2">
                    <input
                      type="number"
                      required
                      value={formData.bloodPressureSystolic}
                      onChange={(e) => setFormData({ ...formData, bloodPressureSystolic: Number(e.target.value) })}
                      className="block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                      placeholder="Systolic"
                    />
                    <span>/</span>
                    <input
                      type="number"
                      required
                      value={formData.bloodPressureDiastolic}
                      onChange={(e) => setFormData({ ...formData, bloodPressureDiastolic: Number(e.target.value) })}
                      className="block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                      placeholder="Diastolic"
                    />
                    <span className="text-gray-500">mmHg</span>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Heart Rate (bpm)</label>
                  <input
                    type="number"
                    required
                    value={formData.heartRate}
                    onChange={(e) => setFormData({ ...formData, heartRate: Number(e.target.value) })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Respiratory Rate (/min)</label>
                  <input
                    type="number"
                    required
                    value={formData.respiratoryRate}
                    onChange={(e) => setFormData({ ...formData, respiratoryRate: Number(e.target.value) })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">SpO2 (%)</label>
                  <input
                    type="number"
                    required
                    min="0"
                    max="100"
                    value={formData.oxygenSaturation}
                    onChange={(e) => setFormData({ ...formData, oxygenSaturation: Number(e.target.value) })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Pain Level (0-10)</label>
                  <input
                    type="number"
                    min="0"
                    max="10"
                    value={formData.painLevel}
                    onChange={(e) => setFormData({ ...formData, painLevel: Number(e.target.value) })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
              </div>
            </div>

            {/* Measurements */}
            <div className="border-t pt-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Body Measurements</h4>
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Weight</label>
                  <div className="mt-1 flex">
                    <input
                      type="number"
                      step="0.1"
                      required
                      value={formData.weight}
                      onChange={(e) => setFormData({ ...formData, weight: Number(e.target.value) })}
                      className="block w-full rounded-l-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                    />
                    <select
                      value={formData.weightUnit}
                      onChange={(e) => setFormData({ ...formData, weightUnit: e.target.value as 'kg' | 'lb' })}
                      className="rounded-r-md border border-l-0 border-gray-300 bg-gray-50 px-3 py-2"
                    >
                      <option value="kg">kg</option>
                      <option value="lb">lb</option>
                    </select>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Height</label>
                  <div className="mt-1 flex">
                    <input
                      type="number"
                      step="0.1"
                      required
                      value={formData.height}
                      onChange={(e) => setFormData({ ...formData, height: Number(e.target.value) })}
                      className="block w-full rounded-l-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                    />
                    <select
                      value={formData.heightUnit}
                      onChange={(e) => setFormData({ ...formData, heightUnit: e.target.value as 'cm' | 'in' })}
                      className="rounded-r-md border border-l-0 border-gray-300 bg-gray-50 px-3 py-2"
                    >
                      <option value="cm">cm</option>
                      <option value="in">in</option>
                    </select>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">BMI (calculated)</label>
                  <div className="mt-1 rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-gray-700">
                    {calculateBMI()} kg/m²
                  </div>
                </div>
              </div>
            </div>

            {/* Additional */}
            <div>
              <label className="block text-sm font-medium text-gray-700">Blood Glucose (optional)</label>
              <input
                type="number"
                value={formData.bloodGlucose || ''}
                onChange={(e) => setFormData({ ...formData, bloodGlucose: e.target.value ? Number(e.target.value) : undefined })}
                placeholder="mg/dL"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Notes</label>
              <textarea
                rows={2}
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
              />
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={onClose}
                className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={mutation.isPending}
                className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {mutation.isPending ? 'Saving...' : 'Record Vitals'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
