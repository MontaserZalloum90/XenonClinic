import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { radiologyApi } from '../lib/api';
import type { RadiologyOrder, RadiologyOrderFormData, RadiologyOrderStatus, ImagingType } from '../types/radiology';
import { format } from 'date-fns';

interface RadiologyFormProps {
  order?: RadiologyOrder;
  onSuccess: () => void;
  onCancel: () => void;
}

export const RadiologyForm = ({ order, onSuccess, onCancel }: RadiologyFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!order;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RadiologyOrderFormData>({
    defaultValues: order
      ? {
          orderNumber: order.orderNumber,
          patientId: order.patientId,
          doctorId: order.doctorId,
          imagingType: order.imagingType,
          bodyPart: order.bodyPart || '',
          orderDate: format(new Date(order.orderDate), 'yyyy-MM-dd'),
          scheduledDate: order.scheduledDate ? format(new Date(order.scheduledDate), 'yyyy-MM-dd') : '',
          status: order.status,
          priority: order.priority,
          clinicalNotes: order.clinicalNotes || '',
          totalAmount: order.totalAmount,
          isPaid: order.isPaid,
        }
      : {
          orderDate: format(new Date(), 'yyyy-MM-dd'),
          status: 0 as RadiologyOrderStatus,
          imagingType: 0 as ImagingType,
          priority: false,
          isPaid: false,
        },
  });

  const createMutation = useMutation({
    mutationFn: (data: RadiologyOrderFormData) => radiologyApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['radiology-orders'] });
      queryClient.invalidateQueries({ queryKey: ['radiology-stats'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: RadiologyOrderFormData) => radiologyApi.update(order!.id, { ...data, id: order!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['radiology-orders'] });
      queryClient.invalidateQueries({ queryKey: ['radiology-stats'] });
      onSuccess();
    },
  });

  const onSubmit = (data: RadiologyOrderFormData) => {
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
        {/* Order Number */}
        <div>
          <label htmlFor="orderNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Order Number *
          </label>
          <input
            type="text"
            id="orderNumber"
            {...register('orderNumber', { required: 'Order number is required' })}
            className="input"
            placeholder="RAD-2024-001"
          />
          {errors.orderNumber && (
            <p className="mt-1 text-sm text-red-600">{errors.orderNumber.message}</p>
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

        {/* Imaging Type */}
        <div>
          <label htmlFor="imagingType" className="block text-sm font-medium text-gray-700 mb-1">
            Imaging Type *
          </label>
          <select id="imagingType" {...register('imagingType', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>X-Ray</option>
            <option value={1}>CT Scan</option>
            <option value={2}>MRI</option>
            <option value={3}>Ultrasound</option>
            <option value={4}>Mammography</option>
            <option value={5}>Fluoroscopy</option>
            <option value={6}>Other</option>
          </select>
        </div>

        {/* Body Part */}
        <div>
          <label htmlFor="bodyPart" className="block text-sm font-medium text-gray-700 mb-1">
            Body Part
          </label>
          <input
            type="text"
            id="bodyPart"
            {...register('bodyPart')}
            className="input"
            placeholder="Chest, Abdomen, etc."
          />
        </div>

        {/* Order Date */}
        <div>
          <label htmlFor="orderDate" className="block text-sm font-medium text-gray-700 mb-1">
            Order Date *
          </label>
          <input
            type="date"
            id="orderDate"
            {...register('orderDate', { required: 'Order date is required' })}
            className="input"
          />
          {errors.orderDate && (
            <p className="mt-1 text-sm text-red-600">{errors.orderDate.message}</p>
          )}
        </div>

        {/* Scheduled Date */}
        <div>
          <label htmlFor="scheduledDate" className="block text-sm font-medium text-gray-700 mb-1">
            Scheduled Date
          </label>
          <input
            type="date"
            id="scheduledDate"
            {...register('scheduledDate')}
            className="input"
          />
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select id="status" {...register('status', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>Pending</option>
            <option value={1}>Scheduled</option>
            <option value={2}>In Progress</option>
            <option value={3}>Completed</option>
            <option value={4}>Reported</option>
            <option value={5}>Cancelled</option>
          </select>
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

        {/* Priority */}
        <div className="flex items-center pt-6">
          <input
            type="checkbox"
            id="priority"
            {...register('priority')}
            className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
          />
          <label htmlFor="priority" className="ml-2 block text-sm text-gray-700">
            Priority / Urgent
          </label>
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

      {/* Clinical Notes */}
      <div>
        <label htmlFor="clinicalNotes" className="block text-sm font-medium text-gray-700 mb-1">
          Clinical Notes
        </label>
        <textarea
          id="clinicalNotes"
          {...register('clinicalNotes')}
          rows={3}
          className="input"
          placeholder="Clinical indication for imaging..."
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
            'Update Order'
          ) : (
            'Create Order'
          )}
        </button>
      </div>
    </form>
  );
};
