import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pharmacyApi } from '../../lib/api';
import type { Prescription, PharmacyStatistics } from '../../types/pharmacy';
import { PharmacyForm } from '../../components/PharmacyForm';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

export const PharmacyList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPrescription, setSelectedPrescription] = useState<Prescription | undefined>(undefined);

  const { data: prescriptions, isLoading } = useQuery<Prescription[]>({
    queryKey: ['pharmacy-prescriptions'],
    queryFn: async () => {
      const response = await pharmacyApi.getAllPrescriptions();
      return response.data;
    },
  });

  const { data: stats } = useQuery<PharmacyStatistics>({
    queryKey: ['pharmacy-stats'],
    queryFn: async () => {
      const response = await pharmacyApi.getStatistics();
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => pharmacyApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pharmacy-prescriptions'] });
      queryClient.invalidateQueries({ queryKey: ['pharmacy-stats'] });
    },
  });

  const handleDelete = (prescription: Prescription) => {
    if (window.confirm(`Are you sure you want to delete prescription ${prescription.prescriptionNumber}?`)) {
      deleteMutation.mutate(prescription.id);
    }
  };

  const handleEdit = (prescription: Prescription) => {
    setSelectedPrescription(prescription);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedPrescription(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedPrescription(undefined);
  };

  const filteredPrescriptions = prescriptions?.filter((prescription) =>
    prescription.prescriptionNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    prescription.patientName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusLabel = (status: number) => {
    const statuses = ['Pending', 'Approved', 'Dispensed', 'Partially Dispensed', 'Cancelled', 'Rejected'];
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-yellow-600 bg-yellow-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-green-600 bg-green-100',
      3: 'text-orange-600 bg-orange-100',
      4: 'text-gray-600 bg-gray-100',
      5: 'text-red-600 bg-red-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Pharmacy</h1>
          <p className="text-gray-600 mt-1">Manage prescriptions and medications</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Prescription
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Prescriptions</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{prescriptions?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.pendingPrescriptions || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Dispensed Today</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.dispensedToday || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search prescriptions by number or patient name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading prescriptions...</p>
          </div>
        ) : filteredPrescriptions && filteredPrescriptions.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Prescription #</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Doctor</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Medications</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredPrescriptions.map((prescription) => (
                  <tr key={prescription.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{prescription.prescriptionNumber}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {prescription.patientName || `Patient #${prescription.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {prescription.doctorName || (prescription.doctorId ? `Dr. #${prescription.doctorId}` : '-')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(prescription.prescriptionDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 max-w-xs truncate">
                      {prescription.medications}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(prescription.status)}`}>
                        {getStatusLabel(prescription.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button onClick={() => handleEdit(prescription)} className="text-primary-600 hover:text-primary-800">
                          Edit
                        </button>
                        <button onClick={() => handleDelete(prescription)} className="text-red-600 hover:text-red-800">
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
            {searchTerm ? 'No prescriptions found matching your search.' : 'No prescriptions found.'}
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
                {selectedPrescription ? 'Edit Prescription' : 'New Prescription'}
              </Dialog.Title>
              <PharmacyForm prescription={selectedPrescription} onSuccess={handleModalClose} onCancel={handleModalClose} />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
