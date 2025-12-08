import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { appointmentsApi } from '../lib/api';
import type { Appointment, AppointmentType } from '../types/appointment';
import { AppointmentType as AppointmentTypeEnum } from '../types/appointment';
import { format } from 'date-fns';

interface AppointmentFormProps {
  appointment?: Appointment;
  onSuccess: () => void;
  onCancel: () => void;
}

interface AppointmentFormData {
  patientId: number;
  providerId?: number;
  startTime: string;
  endTime: string;
  type: AppointmentType;
  notes?: string;
}

export const AppointmentForm = ({ appointment, onSuccess, onCancel }: AppointmentFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!appointment;

  // Initialize form with default values
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AppointmentFormData>({
    defaultValues: appointment
      ? {
          patientId: appointment.patientId,
          providerId: appointment.providerId,
          startTime: format(new Date(appointment.startTime), "yyyy-MM-dd'T'HH:mm"),
          endTime: format(new Date(appointment.endTime), "yyyy-MM-dd'T'HH:mm"),
          type: appointment.type,
          notes: appointment.notes || '',
        }
      : {
          type: AppointmentTypeEnum.Consultation,
          startTime: format(new Date(), "yyyy-MM-dd'T'HH:mm"),
          endTime: format(new Date(Date.now() + 30 * 60000), "yyyy-MM-dd'T'HH:mm"), // 30 min later
        },
  });

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (data: AppointmentFormData) => appointmentsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      onSuccess();
    },
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: (data: AppointmentFormData) =>
      appointmentsApi.update(appointment!.id, {
        ...data,
        id: appointment!.id,
        branchId: appointment!.branchId,
        status: appointment!.status,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      onSuccess();
    },
  });

  const onSubmit = (data: AppointmentFormData) => {
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

      {/* Patient ID (simplified - in real app, this would be a searchable dropdown) */}
      <div>
        <label htmlFor="patientId" className="block text-sm font-medium text-gray-700 mb-1">
          Patient ID *
        </label>
        <input
          type="number"
          id="patientId"
          {...register('patientId', { required: 'Patient ID is required', min: 1 })}
          className="input"
          placeholder="Enter patient ID"
        />
        {errors.patientId && (
          <p className="mt-1 text-sm text-red-600">{errors.patientId.message}</p>
        )}
        <p className="mt-1 text-xs text-gray-500">
          In production, this would be a searchable patient selector
        </p>
      </div>

      {/* Provider ID (optional) */}
      <div>
        <label htmlFor="providerId" className="block text-sm font-medium text-gray-700 mb-1">
          Provider ID (Optional)
        </label>
        <input
          type="number"
          id="providerId"
          {...register('providerId', { min: 1 })}
          className="input"
          placeholder="Enter provider/employee ID"
        />
        <p className="mt-1 text-xs text-gray-500">Leave empty for unassigned</p>
      </div>

      {/* Start Time */}
      <div>
        <label htmlFor="startTime" className="block text-sm font-medium text-gray-700 mb-1">
          Start Time *
        </label>
        <input
          type="datetime-local"
          id="startTime"
          {...register('startTime', { required: 'Start time is required' })}
          className="input"
        />
        {errors.startTime && (
          <p className="mt-1 text-sm text-red-600">{errors.startTime.message}</p>
        )}
      </div>

      {/* End Time */}
      <div>
        <label htmlFor="endTime" className="block text-sm font-medium text-gray-700 mb-1">
          End Time *
        </label>
        <input
          type="datetime-local"
          id="endTime"
          {...register('endTime', { required: 'End time is required' })}
          className="input"
        />
        {errors.endTime && <p className="mt-1 text-sm text-red-600">{errors.endTime.message}</p>}
      </div>

      {/* Appointment Type */}
      <div>
        <label htmlFor="type" className="block text-sm font-medium text-gray-700 mb-1">
          Appointment Type *
        </label>
        <select id="type" {...register('type', { required: true })} className="input">
          <option value={AppointmentTypeEnum.Consultation}>Consultation</option>
          <option value={AppointmentTypeEnum.FollowUp}>Follow-up</option>
          <option value={AppointmentTypeEnum.Procedure}>Procedure</option>
          <option value={AppointmentTypeEnum.Emergency}>Emergency</option>
        </select>
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={3}
          className="input"
          placeholder="Add any additional notes or special instructions..."
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
            'Update Appointment'
          ) : (
            'Create Appointment'
          )}
        </button>
      </div>
    </form>
  );
};
