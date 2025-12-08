import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { inventoryApi } from '../../lib/api';
import type { InventoryItem, InventoryStatistics } from '../../types/inventory';
import { InventoryForm } from '../../components/InventoryForm';
import { Dialog } from '@headlessui/react';

export const InventoryList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<InventoryItem | undefined>(undefined);

  const { data: items, isLoading } = useQuery<InventoryItem[]>({
    queryKey: ['inventory-items'],
    queryFn: async () => {
      const response = await inventoryApi.getAllItems();
      return response.data;
    },
  });

  const { data: stats } = useQuery<InventoryStatistics>({
    queryKey: ['inventory-stats'],
    queryFn: async () => {
      const response = await inventoryApi.getStatistics();
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => inventoryApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inventory-items'] });
      queryClient.invalidateQueries({ queryKey: ['inventory-stats'] });
    },
  });

  const handleDelete = (item: InventoryItem) => {
    if (window.confirm(`Are you sure you want to delete item ${item.name}?`)) {
      deleteMutation.mutate(item.id);
    }
  };

  const handleEdit = (item: InventoryItem) => {
    setSelectedItem(item);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedItem(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedItem(undefined);
  };

  const filteredItems = items?.filter((item) =>
    item.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.itemCode.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getCategoryLabel = (category: number) => {
    const categories = ['Medical', 'Surgical', 'Laboratory', 'Pharmaceutical', 'Equipment', 'Office', 'Other'];
    return categories[category] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-green-600 bg-green-100',
      1: 'text-yellow-600 bg-yellow-100',
      2: 'text-red-600 bg-red-100',
      3: 'text-gray-600 bg-gray-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Inventory Management</h1>
          <p className="text-gray-600 mt-1">Manage stock and inventory items</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Add Item
        </button>
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
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search items by name or code..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading inventory...</p>
          </div>
        ) : filteredItems && filteredItems.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Item Code</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Category</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Quantity</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Min Level</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Unit Price</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredItems.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">{item.itemCode}</td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{item.name}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{getCategoryLabel(item.category)}</td>
                    <td className="px-4 py-3 text-sm text-gray-900">{item.quantity}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{item.minStockLevel}</td>
                    <td className="px-4 py-3 text-sm text-gray-900">AED {item.unitPrice.toFixed(2)}</td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(item.status)}`}>
                        {item.quantity === 0 ? 'Out of Stock' : item.quantity <= item.minStockLevel ? 'Low Stock' : 'In Stock'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button onClick={() => handleEdit(item)} className="text-primary-600 hover:text-primary-800">
                          Edit
                        </button>
                        <button onClick={() => handleDelete(item)} className="text-red-600 hover:text-red-800">
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm ? 'No items found matching your search.' : 'No items found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedItem ? 'Edit Item' : 'Add Item'}
              </Dialog.Title>
              <InventoryForm item={selectedItem} onSuccess={handleModalClose} onCancel={handleModalClose} />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
