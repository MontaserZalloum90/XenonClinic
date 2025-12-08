import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { patientsApi } from '../lib/api';
import type { Patient } from '../types/patient';
import { format } from 'date-fns';

interface PatientFormProps {
  patient?: Patient;
  onSuccess: () => void;
  onCancel: () => void;
}

interface PatientFormData {
  emiratesId: string;
  fullNameEn: string;
  fullNameAr?: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber?: string;
  email?: string;
  hearingLossType?: string;
  notes?: string;
}

export const PatientForm = ({ patient, onSuccess, onCancel }: PatientFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!patient;

  // Initialize form with default values
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PatientFormData>({
    defaultValues: patient
      ? {
          emiratesId: patient.emiratesId,
          fullNameEn: patient.fullNameEn,
          fullNameAr: patient.fullNameAr || '',
          dateOfBirth: format(new Date(patient.dateOfBirth), 'yyyy-MM-dd'),
          gender: patient.gender,
          phoneNumber: patient.phoneNumber || '',
          email: patient.email || '',
          hearingLossType: patient.hearingLossType || '',
          notes: patient.notes || '',
        }
      : {
          gender: 'M',
        },
  });

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (data: PatientFormData) => patientsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      onSuccess();
    },
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: (data: PatientFormData) =>
      patientsApi.update(patient!.id, {
        ...data,
        id: patient!.id,
        branchId: patient!.branchId,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      onSuccess();
    },
  });

  const onSubmit = (data: PatientFormData) => {
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
        {/* Emirates ID */}
        <div>
          <label htmlFor="emiratesId" className="block text-sm font-medium text-gray-700 mb-1">
            Emirates ID *
          </label>
          <input
            type="text"
            id="emiratesId"
            {...register('emiratesId', { required: 'Emirates ID is required' })}
            className="input"
            placeholder="784-1234-1234567-1"
          />
          {errors.emiratesId && (
            <p className="mt-1 text-sm text-red-600">{errors.emiratesId.message}</p>
          )}
        </div>

        {/* Full Name (English) */}
        <div>
          <label htmlFor="fullNameEn" className="block text-sm font-medium text-gray-700 mb-1">
            Full Name (English) *
          </label>
          <input
            type="text"
            id="fullNameEn"
            {...register('fullNameEn', { required: 'Full name is required' })}
            className="input"
            placeholder="John Doe"
          />
          {errors.fullNameEn && (
            <p className="mt-1 text-sm text-red-600">{errors.fullNameEn.message}</p>
          )}
        </div>

        {/* Full Name (Arabic) */}
        <div>
          <label htmlFor="fullNameAr" className="block text-sm font-medium text-gray-700 mb-1">
            Full Name (Arabic)
          </label>
          <input
            type="text"
            id="fullNameAr"
            {...register('fullNameAr')}
            className="input"
            placeholder="الاسم الكامل"
            dir="rtl"
          />
        </div>

        {/* Date of Birth */}
        <div>
          <label htmlFor="dateOfBirth" className="block text-sm font-medium text-gray-700 mb-1">
            Date of Birth *
          </label>
          <input
            type="date"
            id="dateOfBirth"
            {...register('dateOfBirth', { required: 'Date of birth is required' })}
            className="input"
          />
          {errors.dateOfBirth && (
            <p className="mt-1 text-sm text-red-600">{errors.dateOfBirth.message}</p>
          )}
        </div>

        {/* Gender */}
        <div>
          <label htmlFor="gender" className="block text-sm font-medium text-gray-700 mb-1">
            Gender *
          </label>
          <select id="gender" {...register('gender', { required: true })} className="input">
            <option value="M">Male</option>
            <option value="F">Female</option>
          </select>
        </div>

        {/* Phone Number */}
        <div>
          <label htmlFor="phoneNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Phone Number
          </label>
          <input
            type="tel"
            id="phoneNumber"
            {...register('phoneNumber')}
            className="input"
            placeholder="+971 50 123 4567"
          />
        </div>

        {/* Email */}
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
            Email
          </label>
          <input
            type="email"
            id="email"
            {...register('email')}
            className="input"
            placeholder="john@example.com"
          />
        </div>

        {/* Hearing Loss Type */}
        <div>
          <label htmlFor="hearingLossType" className="block text-sm font-medium text-gray-700 mb-1">
            Hearing Loss Type
          </label>
          <input
            type="text"
            id="hearingLossType"
            {...register('hearingLossType')}
            className="input"
            placeholder="E.g., Sensorineural"
          />
        </div>
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
            'Update Patient'
          ) : (
            'Create Patient'
          )}
        </button>
      </div>
    </form>
  );
};
