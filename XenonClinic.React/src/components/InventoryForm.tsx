import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { inventoryApi } from '../lib/api';
import type { InventoryItem, InventoryItemFormData, ItemCategory, StockStatus } from '../types/inventory';
import { format } from 'date-fns';

interface InventoryFormProps {
  item?: InventoryItem;
  onSuccess: () => void;
  onCancel: () => void;
}

export const InventoryForm = ({ item, onSuccess, onCancel }: InventoryFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!item;

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<InventoryItemFormData>({
    defaultValues: item
      ? {
          itemCode: item.itemCode,
          name: item.name,
          description: item.description || '',
          category: item.category,
          quantity: item.quantity,
          minStockLevel: item.minStockLevel,
          maxStockLevel: item.maxStockLevel,
          unitPrice: item.unitPrice,
          supplier: item.supplier || '',
          location: item.location || '',
          expiryDate: item.expiryDate ? format(new Date(item.expiryDate), 'yyyy-MM-dd') : '',
          status: item.status,
        }
      : {
          category: 0 as ItemCategory,
          status: 0 as StockStatus,
          quantity: 0,
          minStockLevel: 10,
        },
  });

  const quantity = watch('quantity');
  const unitPrice = watch('unitPrice');

  const createMutation = useMutation({
    mutationFn: (data: InventoryItemFormData) => inventoryApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inventory-items'] });
      queryClient.invalidateQueries({ queryKey: ['inventory-stats'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: InventoryItemFormData) => inventoryApi.update(item!.id, { ...data, id: item!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inventory-items'] });
      queryClient.invalidateQueries({ queryKey: ['inventory-stats'] });
      onSuccess();
    },
  });

  const onSubmit = (data: InventoryItemFormData) => {
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
        {/* Item Code */}
        <div>
          <label htmlFor="itemCode" className="block text-sm font-medium text-gray-700 mb-1">
            Item Code *
          </label>
          <input
            type="text"
            id="itemCode"
            {...register('itemCode', { required: 'Item code is required' })}
            className="input"
            placeholder="ITEM-001"
          />
          {errors.itemCode && (
            <p className="mt-1 text-sm text-red-600">{errors.itemCode.message}</p>
          )}
        </div>

        {/* Item Name */}
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
            Item Name *
          </label>
          <input
            type="text"
            id="name"
            {...register('name', { required: 'Item name is required' })}
            className="input"
            placeholder="Surgical Gloves"
          />
          {errors.name && (
            <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
          )}
        </div>

        {/* Category */}
        <div>
          <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-1">
            Category *
          </label>
          <select id="category" {...register('category', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>Medical</option>
            <option value={1}>Surgical</option>
            <option value={2}>Laboratory</option>
            <option value={3}>Pharmaceutical</option>
            <option value={4}>Equipment</option>
            <option value={5}>Office</option>
            <option value={6}>Other</option>
          </select>
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select id="status" {...register('status', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>In Stock</option>
            <option value={1}>Low Stock</option>
            <option value={2}>Out of Stock</option>
            <option value={3}>Discontinued</option>
          </select>
        </div>

        {/* Quantity */}
        <div>
          <label htmlFor="quantity" className="block text-sm font-medium text-gray-700 mb-1">
            Quantity *
          </label>
          <input
            type="number"
            id="quantity"
            {...register('quantity', { required: 'Quantity is required', valueAsNumber: true })}
            className="input"
            placeholder="0"
          />
          {errors.quantity && (
            <p className="mt-1 text-sm text-red-600">{errors.quantity.message}</p>
          )}
        </div>

        {/* Min Stock Level */}
        <div>
          <label htmlFor="minStockLevel" className="block text-sm font-medium text-gray-700 mb-1">
            Min Stock Level *
          </label>
          <input
            type="number"
            id="minStockLevel"
            {...register('minStockLevel', { required: 'Min stock level is required', valueAsNumber: true })}
            className="input"
            placeholder="10"
          />
          {errors.minStockLevel && (
            <p className="mt-1 text-sm text-red-600">{errors.minStockLevel.message}</p>
          )}
        </div>

        {/* Max Stock Level */}
        <div>
          <label htmlFor="maxStockLevel" className="block text-sm font-medium text-gray-700 mb-1">
            Max Stock Level
          </label>
          <input
            type="number"
            id="maxStockLevel"
            {...register('maxStockLevel', { valueAsNumber: true })}
            className="input"
            placeholder="100"
          />
        </div>

        {/* Unit Price */}
        <div>
          <label htmlFor="unitPrice" className="block text-sm font-medium text-gray-700 mb-1">
            Unit Price (AED) *
          </label>
          <input
            type="number"
            step="0.01"
            id="unitPrice"
            {...register('unitPrice', { required: 'Unit price is required', valueAsNumber: true })}
            className="input"
            placeholder="0.00"
          />
          {errors.unitPrice && (
            <p className="mt-1 text-sm text-red-600">{errors.unitPrice.message}</p>
          )}
        </div>

        {/* Supplier */}
        <div>
          <label htmlFor="supplier" className="block text-sm font-medium text-gray-700 mb-1">
            Supplier
          </label>
          <input
            type="text"
            id="supplier"
            {...register('supplier')}
            className="input"
            placeholder="Supplier Name"
          />
        </div>

        {/* Location */}
        <div>
          <label htmlFor="location" className="block text-sm font-medium text-gray-700 mb-1">
            Storage Location
          </label>
          <input
            type="text"
            id="location"
            {...register('location')}
            className="input"
            placeholder="Shelf A1"
          />
        </div>

        {/* Expiry Date */}
        <div>
          <label htmlFor="expiryDate" className="block text-sm font-medium text-gray-700 mb-1">
            Expiry Date
          </label>
          <input
            type="date"
            id="expiryDate"
            {...register('expiryDate')}
            className="input"
          />
        </div>
      </div>

      {/* Total Value Display */}
      {quantity !== undefined && unitPrice !== undefined && (
        <div className="p-3 bg-green-50 border border-green-200 rounded-md">
          <p className="text-sm text-green-800">
            <span className="font-medium">Total Value:</span> AED {(quantity * unitPrice).toFixed(2)}
          </p>
        </div>
      )}

      {/* Description */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          id="description"
          {...register('description')}
          rows={2}
          className="input"
          placeholder="Item description..."
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
            'Update Item'
          ) : (
            'Add Item'
          )}
        </button>
      </div>
    </form>
  );
};
