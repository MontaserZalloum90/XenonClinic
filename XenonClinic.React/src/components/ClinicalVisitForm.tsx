import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { clinicalVisitsApi } from '../lib/api';
import type {
  ClinicalVisit,
  ClinicalVisitFormData,
  ClinicalVisitStatus,
  VisitType,
} from '../types/clinical-visit';
import { format } from 'date-fns';

interface ClinicalVisitFormProps {
  visit?: ClinicalVisit;
  onSuccess: () => void;
  onCancel: () => void;
}

export const ClinicalVisitForm = ({ visit, onSuccess, onCancel }: ClinicalVisitFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!visit;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ClinicalVisitFormData>({
    defaultValues: visit
      ? {
          visitNumber: visit.visitNumber,
          patientId: visit.patientId,
          doctorId: visit.doctorId,
          visitDate: format(new Date(visit.visitDate), "yyyy-MM-dd'T'HH:mm"),
          visitType: visit.visitType,
          chiefComplaint: visit.chiefComplaint || '',
          diagnosis: visit.diagnosis || '',
          treatmentPlan: visit.treatmentPlan || '',
          notes: visit.notes || '',
          vitalSigns: visit.vitalSigns || {},
          status: visit.status,
          followUpDate: visit.followUpDate ? format(new Date(visit.followUpDate), 'yyyy-MM-dd') : '',
        }
      : {
          status: 0 as ClinicalVisitStatus,
          visitType: 0 as VisitType,
          vitalSigns: {},
        },
  });

  const createMutation = useMutation({
    mutationFn: (data: ClinicalVisitFormData) => clinicalVisitsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clinical-visits'] });
      queryClient.invalidateQueries({ queryKey: ['clinical-visits-stats'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: ClinicalVisitFormData) =>
      clinicalVisitsApi.update(visit!.id, { ...data, id: visit!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clinical-visits'] });
      queryClient.invalidateQueries({ queryKey: ['clinical-visits-stats'] });
      onSuccess();
    },
  });

  const onSubmit = (data: ClinicalVisitFormData) => {
    const formData = {
      ...data,
      vitalSigns: {
        bloodPressure: data.vitalSigns?.bloodPressure || undefined,
        heartRate: data.vitalSigns?.heartRate ? Number(data.vitalSigns.heartRate) : undefined,
        temperature: data.vitalSigns?.temperature ? Number(data.vitalSigns.temperature) : undefined,
        weight: data.vitalSigns?.weight ? Number(data.vitalSigns.weight) : undefined,
        height: data.vitalSigns?.height ? Number(data.vitalSigns.height) : undefined,
        oxygenSaturation: data.vitalSigns?.oxygenSaturation
          ? Number(data.vitalSigns.oxygenSaturation)
          : undefined,
      },
    };

    if (isEditing) {
      updateMutation.mutate(formData);
    } else {
      createMutation.mutate(formData);
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
        {/* Visit Number */}
        <div>
          <label htmlFor="visitNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Visit Number *
          </label>
          <input
            type="text"
            id="visitNumber"
            {...register('visitNumber', { required: 'Visit number is required' })}
            className="input"
            placeholder="VIS-001"
          />
          {errors.visitNumber && (
            <p className="mt-1 text-sm text-red-600">{errors.visitNumber.message}</p>
          )}
        </div>

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

        {/* Doctor ID */}
        <div>
          <label htmlFor="doctorId" className="block text-sm font-medium text-gray-700 mb-1">
            Doctor ID *
          </label>
          <input
            type="number"
            id="doctorId"
            {...register('doctorId', { required: 'Doctor ID is required', valueAsNumber: true })}
            className="input"
            placeholder="1"
          />
          {errors.doctorId && (
            <p className="mt-1 text-sm text-red-600">{errors.doctorId.message}</p>
          )}
        </div>

        {/* Visit Date */}
        <div>
          <label htmlFor="visitDate" className="block text-sm font-medium text-gray-700 mb-1">
            Visit Date & Time *
          </label>
          <input
            type="datetime-local"
            id="visitDate"
            {...register('visitDate', { required: 'Visit date is required' })}
            className="input"
          />
          {errors.visitDate && (
            <p className="mt-1 text-sm text-red-600">{errors.visitDate.message}</p>
          )}
        </div>

        {/* Visit Type */}
        <div>
          <label htmlFor="visitType" className="block text-sm font-medium text-gray-700 mb-1">
            Visit Type *
          </label>
          <select
            id="visitType"
            {...register('visitType', { required: true, valueAsNumber: true })}
            className="input"
          >
            <option value={0}>Consultation</option>
            <option value={1}>Follow-Up</option>
            <option value={2}>Emergency</option>
            <option value={3}>Procedure</option>
            <option value={4}>Checkup</option>
          </select>
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select
            id="status"
            {...register('status', { required: true, valueAsNumber: true })}
            className="input"
          >
            <option value={0}>Scheduled</option>
            <option value={1}>In Progress</option>
            <option value={2}>Completed</option>
            <option value={3}>Cancelled</option>
            <option value={4}>No Show</option>
          </select>
        </div>

        {/* Follow-up Date */}
        <div>
          <label htmlFor="followUpDate" className="block text-sm font-medium text-gray-700 mb-1">
            Follow-up Date
          </label>
          <input
            type="date"
            id="followUpDate"
            {...register('followUpDate')}
            className="input"
          />
        </div>
      </div>

      {/* Chief Complaint */}
      <div>
        <label htmlFor="chiefComplaint" className="block text-sm font-medium text-gray-700 mb-1">
          Chief Complaint
        </label>
        <textarea
          id="chiefComplaint"
          {...register('chiefComplaint')}
          rows={2}
          className="input"
          placeholder="Patient's main reason for visit..."
        />
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
          placeholder="Medical diagnosis..."
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
          placeholder="Recommended treatment and medications..."
        />
      </div>

      {/* Vital Signs Section */}
      <div className="border-t pt-4">
        <h3 className="text-sm font-semibold text-gray-900 mb-3">Vital Signs</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Blood Pressure */}
          <div>
            <label htmlFor="bloodPressure" className="block text-sm font-medium text-gray-700 mb-1">
              Blood Pressure
            </label>
            <input
              type="text"
              id="bloodPressure"
              {...register('vitalSigns.bloodPressure')}
              className="input"
              placeholder="120/80"
            />
          </div>

          {/* Heart Rate */}
          <div>
            <label htmlFor="heartRate" className="block text-sm font-medium text-gray-700 mb-1">
              Heart Rate (bpm)
            </label>
            <input
              type="number"
              id="heartRate"
              {...register('vitalSigns.heartRate', { valueAsNumber: true })}
              className="input"
              placeholder="72"
            />
          </div>

          {/* Temperature */}
          <div>
            <label htmlFor="temperature" className="block text-sm font-medium text-gray-700 mb-1">
              Temperature (°C)
            </label>
            <input
              type="number"
              step="0.1"
              id="temperature"
              {...register('vitalSigns.temperature', { valueAsNumber: true })}
              className="input"
              placeholder="37.0"
            />
          </div>

          {/* Weight */}
          <div>
            <label htmlFor="weight" className="block text-sm font-medium text-gray-700 mb-1">
              Weight (kg)
            </label>
            <input
              type="number"
              step="0.1"
              id="weight"
              {...register('vitalSigns.weight', { valueAsNumber: true })}
              className="input"
              placeholder="70.5"
            />
          </div>

          {/* Height */}
          <div>
            <label htmlFor="height" className="block text-sm font-medium text-gray-700 mb-1">
              Height (cm)
            </label>
            <input
              type="number"
              step="0.1"
              id="height"
              {...register('vitalSigns.height', { valueAsNumber: true })}
              className="input"
              placeholder="175"
            />
          </div>

          {/* Oxygen Saturation */}
          <div>
            <label htmlFor="oxygenSaturation" className="block text-sm font-medium text-gray-700 mb-1">
              Oxygen Saturation (%)
            </label>
            <input
              type="number"
              id="oxygenSaturation"
              {...register('vitalSigns.oxygenSaturation', { valueAsNumber: true })}
              className="input"
              placeholder="98"
            />
          </div>
        </div>
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          Additional Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={3}
          className="input"
          placeholder="Any additional notes or observations..."
        />
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-3 pt-4 border-t">
        <button
          type="button"
          onClick={onCancel}
          disabled={isPending}
          className="btn btn-secondary"
        >
          Cancel
        </button>
        <button type="submit" disabled={isPending} className="btn btn-primary">
          {isPending ? (
            <div className="flex items-center">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              {isEditing ? 'Updating...' : 'Creating...'}
            </div>
          ) : isEditing ? (
            'Update Visit'
          ) : (
            'Add Visit'
          )}
        </button>
      </div>
    </form>
  );
};
