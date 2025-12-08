import { useQuery } from '@tanstack/react-query';
import { api } from '../../lib/api';

export const InventoryList = () => {
  const { data: items, isLoading } = useQuery({
    queryKey: ['inventory-items'],
    queryFn: async () => {
      const response = await api.get('/api/InventoryApi/items');
      return response.data;
    },
  });

  const { data: stats } = useQuery({
    queryKey: ['inventory-stats'],
    queryFn: async () => {
      const response = await api.get('/api/InventoryApi/statistics');
      return response.data;
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Inventory Management</h1>
        <p className="text-gray-600 mt-1">Manage stock and inventory items</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Items</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalItems || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Low Stock Items</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{stats?.lowStockItems || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Total Value</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            AED {stats?.totalValue?.toFixed(2) || '0.00'}
          </p>
        </div>
      </div>

      <div className="card">
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading inventory...</p>
          </div>
        ) : (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Inventory Items</h3>
            {items && items.length > 0 ? (
              <p className="text-gray-600">{items.length} items in stock</p>
            ) : (
              <p className="text-gray-500">No items found</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
