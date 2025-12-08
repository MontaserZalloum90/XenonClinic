import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { pharmacyApi } from '../lib/api';
import type { Prescription, PrescriptionFormData, PrescriptionStatus } from '../types/pharmacy';
import { format } from 'date-fns';

interface PharmacyFormProps {
  prescription?: Prescription;
  onSuccess: () => void;
  onCancel: () => void;
}

export const PharmacyForm = ({ prescription, onSuccess, onCancel }: PharmacyFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!prescription;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PrescriptionFormData>({
    defaultValues: prescription
      ? {
          prescriptionNumber: prescription.prescriptionNumber,
          patientId: prescription.patientId,
          doctorId: prescription.doctorId,
          prescriptionDate: format(new Date(prescription.prescriptionDate), 'yyyy-MM-dd'),
          status: prescription.status,
          medications: prescription.medications,
          dosage: prescription.dosage || '',
          duration: prescription.duration || '',
          refills: prescription.refills,
          notes: prescription.notes || '',
          totalAmount: prescription.totalAmount,
          isPaid: prescription.isPaid,
        }
      : {
          prescriptionDate: format(new Date(), 'yyyy-MM-dd'),
          status: 0 as PrescriptionStatus,
          isPaid: false,
          refills: 0,
        },
  });

  const createMutation = useMutation({
    mutationFn: (data: PrescriptionFormData) => pharmacyApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pharmacy-prescriptions'] });
      queryClient.invalidateQueries({ queryKey: ['pharmacy-stats'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: PrescriptionFormData) => pharmacyApi.update(prescription!.id, { ...data, id: prescription!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pharmacy-prescriptions'] });
      queryClient.invalidateQueries({ queryKey: ['pharmacy-stats'] });
      onSuccess();
    },
  });

  const onSubmit = (data: PrescriptionFormData) => {
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
        {/* Prescription Number */}
        <div>
          <label htmlFor="prescriptionNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Prescription Number *
          </label>
          <input
            type="text"
            id="prescriptionNumber"
            {...register('prescriptionNumber', { required: 'Prescription number is required' })}
            className="input"
            placeholder="RX-2024-001"
          />
          {errors.prescriptionNumber && (
            <p className="mt-1 text-sm text-red-600">{errors.prescriptionNumber.message}</p>
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
            Doctor ID
          </label>
          <input
            type="number"
            id="doctorId"
            {...register('doctorId', { valueAsNumber: true })}
            className="input"
            placeholder="1"
          />
        </div>

        {/* Prescription Date */}
        <div>
          <label htmlFor="prescriptionDate" className="block text-sm font-medium text-gray-700 mb-1">
            Prescription Date *
          </label>
          <input
            type="date"
            id="prescriptionDate"
            {...register('prescriptionDate', { required: 'Prescription date is required' })}
            className="input"
          />
          {errors.prescriptionDate && (
            <p className="mt-1 text-sm text-red-600">{errors.prescriptionDate.message}</p>
          )}
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select id="status" {...register('status', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>Pending</option>
            <option value={1}>Approved</option>
            <option value={2}>Dispensed</option>
            <option value={3}>Partially Dispensed</option>
            <option value={4}>Cancelled</option>
            <option value={5}>Rejected</option>
          </select>
        </div>

        {/* Duration */}
        <div>
          <label htmlFor="duration" className="block text-sm font-medium text-gray-700 mb-1">
            Duration
          </label>
          <input
            type="text"
            id="duration"
            {...register('duration')}
            className="input"
            placeholder="7 days"
          />
        </div>

        {/* Refills */}
        <div>
          <label htmlFor="refills" className="block text-sm font-medium text-gray-700 mb-1">
            Refills
          </label>
          <input
            type="number"
            id="refills"
            {...register('refills', { valueAsNumber: true })}
            className="input"
            placeholder="0"
          />
        </div>

        {/* Total Amount */}
        <div>
          <label htmlFor="totalAmount" className="block text-sm font-medium text-gray-700 mb-1">
            Total Amount (AED)
          </label>
          <input
            type="number"
            step="0.01"
            id="totalAmount"
            {...register('totalAmount', { valueAsNumber: true })}
            className="input"
            placeholder="0.00"
          />
        </div>

        {/* Is Paid */}
        <div className="flex items-center pt-6">
          <input
            type="checkbox"
            id="isPaid"
            {...register('isPaid')}
            className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
          />
          <label htmlFor="isPaid" className="ml-2 block text-sm text-gray-700">
            Paid
          </label>
        </div>
      </div>

      {/* Medications */}
      <div>
        <label htmlFor="medications" className="block text-sm font-medium text-gray-700 mb-1">
          Medications *
        </label>
        <textarea
          id="medications"
          {...register('medications', { required: 'Medications are required' })}
          rows={3}
          className="input"
          placeholder="List medications (comma-separated or detailed description)..."
        />
        {errors.medications && (
          <p className="mt-1 text-sm text-red-600">{errors.medications.message}</p>
        )}
      </div>

      {/* Dosage */}
      <div>
        <label htmlFor="dosage" className="block text-sm font-medium text-gray-700 mb-1">
          Dosage Instructions
        </label>
        <textarea
          id="dosage"
          {...register('dosage')}
          rows={2}
          className="input"
          placeholder="Take 1 tablet twice daily..."
        />
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={2}
          className="input"
          placeholder="Additional notes..."
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
            'Update Prescription'
          ) : (
            'Create Prescription'
          )}
        </button>
      </div>
    </form>
  );
};
