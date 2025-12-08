import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { financialApi } from '../../lib/api';
import type { Invoice, FinancialStatistics } from '../../types/financial';
import { FinancialForm } from '../../components/FinancialForm';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

export const FinancialList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | undefined>(undefined);

  const { data: invoices, isLoading } = useQuery<Invoice[]>({
    queryKey: ['financial-invoices'],
    queryFn: async () => {
      const response = await financialApi.getAllInvoices();
      return response.data;
    },
  });

  const { data: stats } = useQuery<FinancialStatistics>({
    queryKey: ['financial-stats'],
    queryFn: async () => {
      const response = await financialApi.getStatistics();
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => financialApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-invoices'] });
      queryClient.invalidateQueries({ queryKey: ['financial-stats'] });
    },
  });

  const handleDelete = (invoice: Invoice) => {
    if (window.confirm(`Are you sure you want to delete invoice ${invoice.invoiceNumber}?`)) {
      deleteMutation.mutate(invoice.id);
    }
  };

  const handleEdit = (invoice: Invoice) => {
    setSelectedInvoice(invoice);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedInvoice(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedInvoice(undefined);
  };

  const filteredInvoices = invoices?.filter((invoice) =>
    invoice.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    invoice.patientName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusLabel = (status: number) => {
    const statuses = ['Draft', 'Issued', 'Partially Paid', 'Paid', 'Overdue', 'Cancelled'];
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-gray-600 bg-gray-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-yellow-600 bg-yellow-100',
      3: 'text-green-600 bg-green-100',
      4: 'text-red-600 bg-red-100',
      5: 'text-gray-600 bg-gray-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Financial Management</h1>
          <p className="text-gray-600 mt-1">Manage invoices and financial operations</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Create Invoice
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Monthly Revenue</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            AED {stats?.monthlyRevenue?.toFixed(2) || '0.00'}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Unpaid Invoices</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.unpaidInvoices || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Overdue Invoices</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{stats?.overdueInvoices || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search invoices by number or patient name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading invoices...</p>
          </div>
        ) : filteredInvoices && filteredInvoices.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Invoice #</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Issue Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Due Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Remaining</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredInvoices.map((invoice) => (
                  <tr key={invoice.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{invoice.invoiceNumber}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{invoice.patientName || `Patient #${invoice.patientId}`}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{format(new Date(invoice.issueDate), 'MMM d, yyyy')}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{format(new Date(invoice.dueDate), 'MMM d, yyyy')}</td>
                    <td className="px-4 py-3 text-sm text-gray-900">AED {invoice.totalAmount.toFixed(2)}</td>
                    <td className="px-4 py-3 text-sm text-gray-900">AED {invoice.remainingAmount.toFixed(2)}</td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(invoice.status)}`}>
                        {getStatusLabel(invoice.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button onClick={() => handleEdit(invoice)} className="text-primary-600 hover:text-primary-800">
                          Edit
                        </button>
                        <button onClick={() => handleDelete(invoice)} className="text-red-600 hover:text-red-800">
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
            {searchTerm ? 'No invoices found matching your search.' : 'No invoices found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedInvoice ? 'Edit Invoice' : 'Create Invoice'}
              </Dialog.Title>
              <FinancialForm invoice={selectedInvoice} onSuccess={handleModalClose} onCancel={handleModalClose} />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
