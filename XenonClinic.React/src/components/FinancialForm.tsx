import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { financialApi } from '../lib/api';
import type { Invoice, InvoiceFormData, InvoiceStatus } from '../types/financial';
import { format } from 'date-fns';

interface FinancialFormProps {
  invoice?: Invoice;
  onSuccess: () => void;
  onCancel: () => void;
}

export const FinancialForm = ({ invoice, onSuccess, onCancel }: FinancialFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!invoice;

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<InvoiceFormData>({
    defaultValues: invoice
      ? {
          invoiceNumber: invoice.invoiceNumber,
          patientId: invoice.patientId,
          issueDate: format(new Date(invoice.issueDate), 'yyyy-MM-dd'),
          dueDate: format(new Date(invoice.dueDate), 'yyyy-MM-dd'),
          status: invoice.status,
          totalAmount: invoice.totalAmount,
          paidAmount: invoice.paidAmount,
          paymentMethod: invoice.paymentMethod,
          description: invoice.description || '',
          notes: invoice.notes || '',
        }
      : {
          issueDate: format(new Date(), 'yyyy-MM-dd'),
          status: 0 as InvoiceStatus,
          paidAmount: 0,
        },
  });

  const totalAmount = watch('totalAmount');
  const paidAmount = watch('paidAmount');

  const createMutation = useMutation({
    mutationFn: (data: InvoiceFormData) => financialApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-invoices'] });
      queryClient.invalidateQueries({ queryKey: ['financial-stats'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: InvoiceFormData) => financialApi.update(invoice!.id, { ...data, id: invoice!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-invoices'] });
      queryClient.invalidateQueries({ queryKey: ['financial-stats'] });
      onSuccess();
    },
  });

  const onSubmit = (data: InvoiceFormData) => {
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
        {/* Invoice Number */}
        <div>
          <label htmlFor="invoiceNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Invoice Number *
          </label>
          <input
            type="text"
            id="invoiceNumber"
            {...register('invoiceNumber', { required: 'Invoice number is required' })}
            className="input"
            placeholder="INV-2024-001"
          />
          {errors.invoiceNumber && (
            <p className="mt-1 text-sm text-red-600">{errors.invoiceNumber.message}</p>
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

        {/* Issue Date */}
        <div>
          <label htmlFor="issueDate" className="block text-sm font-medium text-gray-700 mb-1">
            Issue Date *
          </label>
          <input
            type="date"
            id="issueDate"
            {...register('issueDate', { required: 'Issue date is required' })}
            className="input"
          />
          {errors.issueDate && (
            <p className="mt-1 text-sm text-red-600">{errors.issueDate.message}</p>
          )}
        </div>

        {/* Due Date */}
        <div>
          <label htmlFor="dueDate" className="block text-sm font-medium text-gray-700 mb-1">
            Due Date *
          </label>
          <input
            type="date"
            id="dueDate"
            {...register('dueDate', { required: 'Due date is required' })}
            className="input"
          />
          {errors.dueDate && (
            <p className="mt-1 text-sm text-red-600">{errors.dueDate.message}</p>
          )}
        </div>

        {/* Total Amount */}
        <div>
          <label htmlFor="totalAmount" className="block text-sm font-medium text-gray-700 mb-1">
            Total Amount (AED) *
          </label>
          <input
            type="number"
            step="0.01"
            id="totalAmount"
            {...register('totalAmount', { required: 'Total amount is required', valueAsNumber: true })}
            className="input"
            placeholder="0.00"
          />
          {errors.totalAmount && (
            <p className="mt-1 text-sm text-red-600">{errors.totalAmount.message}</p>
          )}
        </div>

        {/* Paid Amount */}
        <div>
          <label htmlFor="paidAmount" className="block text-sm font-medium text-gray-700 mb-1">
            Paid Amount (AED) *
          </label>
          <input
            type="number"
            step="0.01"
            id="paidAmount"
            {...register('paidAmount', { required: 'Paid amount is required', valueAsNumber: true })}
            className="input"
            placeholder="0.00"
          />
          {errors.paidAmount && (
            <p className="mt-1 text-sm text-red-600">{errors.paidAmount.message}</p>
          )}
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select id="status" {...register('status', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>Draft</option>
            <option value={1}>Issued</option>
            <option value={2}>Partially Paid</option>
            <option value={3}>Paid</option>
            <option value={4}>Overdue</option>
            <option value={5}>Cancelled</option>
          </select>
        </div>

        {/* Payment Method */}
        <div>
          <label htmlFor="paymentMethod" className="block text-sm font-medium text-gray-700 mb-1">
            Payment Method
          </label>
          <select id="paymentMethod" {...register('paymentMethod', { valueAsNumber: true })} className="input">
            <option value="">Select method...</option>
            <option value={0}>Cash</option>
            <option value={1}>Card</option>
            <option value={2}>Bank Transfer</option>
            <option value={3}>Insurance</option>
            <option value={4}>Other</option>
          </select>
        </div>
      </div>

      {/* Remaining Amount Display */}
      {totalAmount !== undefined && paidAmount !== undefined && (
        <div className="p-3 bg-blue-50 border border-blue-200 rounded-md">
          <p className="text-sm text-blue-800">
            <span className="font-medium">Remaining Amount:</span> AED {(totalAmount - paidAmount).toFixed(2)}
          </p>
        </div>
      )}

      {/* Description */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <input
          type="text"
          id="description"
          {...register('description')}
          className="input"
          placeholder="Service description..."
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
            'Update Invoice'
          ) : (
            'Create Invoice'
          )}
        </button>
      </div>
    </form>
  );
};
