import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { radiologyApi } from '../../lib/api';
import type { RadiologyOrder, RadiologyStatistics } from '../../types/radiology';
import { RadiologyForm } from '../../components/RadiologyForm';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

export const RadiologyList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<RadiologyOrder | undefined>(undefined);

  const { data: orders, isLoading } = useQuery<RadiologyOrder[]>({
    queryKey: ['radiology-orders'],
    queryFn: async () => {
      const response = await radiologyApi.getAllOrders();
      return response.data;
    },
  });

  const { data: stats } = useQuery<RadiologyStatistics>({
    queryKey: ['radiology-stats'],
    queryFn: async () => {
      const response = await radiologyApi.getStatistics();
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => radiologyApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['radiology-orders'] });
      queryClient.invalidateQueries({ queryKey: ['radiology-stats'] });
    },
  });

  const handleDelete = (order: RadiologyOrder) => {
    if (window.confirm(`Are you sure you want to delete order ${order.orderNumber}?`)) {
      deleteMutation.mutate(order.id);
    }
  };

  const handleEdit = (order: RadiologyOrder) => {
    setSelectedOrder(order);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedOrder(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedOrder(undefined);
  };

  const filteredOrders = orders?.filter((order) =>
    order.orderNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    order.patientName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getImagingTypeLabel = (type: number) => {
    const types = ['X-Ray', 'CT Scan', 'MRI', 'Ultrasound', 'Mammography', 'Fluoroscopy', 'Other'];
    return types[type] || 'Unknown';
  };

  const getStatusLabel = (status: number) => {
    const statuses = ['Pending', 'Scheduled', 'In Progress', 'Completed', 'Reported', 'Cancelled'];
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-yellow-600 bg-yellow-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-orange-600 bg-orange-100',
      3: 'text-green-600 bg-green-100',
      4: 'text-purple-600 bg-purple-100',
      5: 'text-gray-600 bg-gray-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Radiology</h1>
          <p className="text-gray-600 mt-1">Manage radiology orders and imaging</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Order
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Orders</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{orders?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending Orders</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.pendingOrders || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed Today</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.completedToday || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search orders by number or patient name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading orders...</p>
          </div>
        ) : filteredOrders && filteredOrders.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order #</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Imaging Type</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Body Part</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Priority</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredOrders.map((order) => (
                  <tr key={order.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{order.orderNumber}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {order.patientName || `Patient #${order.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{getImagingTypeLabel(order.imagingType)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{order.bodyPart || '-'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(order.orderDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      {order.priority && (
                        <span className="px-2 py-1 rounded-full text-xs font-medium text-red-600 bg-red-100">
                          Urgent
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(order.status)}`}>
                        {getStatusLabel(order.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button onClick={() => handleEdit(order)} className="text-primary-600 hover:text-primary-800">
                          Edit
                        </button>
                        <button onClick={() => handleDelete(order)} className="text-red-600 hover:text-red-800">
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
            {searchTerm ? 'No orders found matching your search.' : 'No orders found.'}
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
                {selectedOrder ? 'Edit Order' : 'New Order'}
              </Dialog.Title>
              <RadiologyForm order={selectedOrder} onSuccess={handleModalClose} onCancel={handleModalClose} />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
