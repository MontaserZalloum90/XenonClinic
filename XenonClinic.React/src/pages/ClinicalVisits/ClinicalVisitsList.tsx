import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clinicalVisitsApi } from '../../lib/api';
import type { ClinicalVisit, ClinicalVisitStatistics } from '../../types/clinical-visit';
import { ClinicalVisitForm } from '../../components/ClinicalVisitForm';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

export const ClinicalVisitsList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedVisit, setSelectedVisit] = useState<ClinicalVisit | undefined>(undefined);

  const { data: visits, isLoading } = useQuery<ClinicalVisit[]>({
    queryKey: ['clinical-visits'],
    queryFn: async () => {
      const response = await clinicalVisitsApi.getAll();
      return response.data;
    },
  });

  const { data: stats } = useQuery<ClinicalVisitStatistics>({
    queryKey: ['clinical-visits-stats'],
    queryFn: async () => {
      const response = await clinicalVisitsApi.getStatistics();
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => clinicalVisitsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clinical-visits'] });
      queryClient.invalidateQueries({ queryKey: ['clinical-visits-stats'] });
    },
  });

  const handleDelete = (visit: ClinicalVisit) => {
    if (window.confirm(`Are you sure you want to delete visit ${visit.visitNumber}?`)) {
      deleteMutation.mutate(visit.id);
    }
  };

  const handleEdit = (visit: ClinicalVisit) => {
    setSelectedVisit(visit);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedVisit(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedVisit(undefined);
  };

  const filteredVisits = visits?.filter(
    (visit) =>
      visit.visitNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      visit.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      visit.doctorName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      visit.chiefComplaint?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getVisitTypeLabel = (type: number) => {
    const types = ['Consultation', 'Follow-Up', 'Emergency', 'Procedure', 'Checkup'];
    return types[type] || 'Unknown';
  };

  const getStatusLabel = (status: number) => {
    const statuses = ['Scheduled', 'In Progress', 'Completed', 'Cancelled', 'No Show'];
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-blue-600 bg-blue-100',
      1: 'text-yellow-600 bg-yellow-100',
      2: 'text-green-600 bg-green-100',
      3: 'text-gray-600 bg-gray-100',
      4: 'text-red-600 bg-red-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Clinical Visits</h1>
          <p className="text-gray-600 mt-1">Track and manage patient clinical visits</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Add Visit
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Visits</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalVisits || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Today's Visits</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{stats?.todaysVisits || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">In Progress</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.inProgressVisits || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search visits by visit number, patient, doctor, or chief complaint..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading visits...</p>
          </div>
        ) : filteredVisits && filteredVisits.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Visit #</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Patient</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Doctor</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Chief Complaint</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredVisits.map((visit) => (
                  <tr key={visit.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{visit.visitNumber}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {visit.patientName || `Patient #${visit.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {visit.doctorName || `Doctor #${visit.doctorId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(visit.visitDate), 'MMM d, yyyy HH:mm')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{getVisitTypeLabel(visit.visitType)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {visit.chiefComplaint ? (
                        visit.chiefComplaint.length > 50
                          ? visit.chiefComplaint.substring(0, 50) + '...'
                          : visit.chiefComplaint
                      ) : (
                        '-'
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(visit.status)}`}>
                        {getStatusLabel(visit.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(visit)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(visit)}
                          className="text-red-600 hover:text-red-800"
                        >
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
            {searchTerm ? 'No visits found matching your search.' : 'No visits found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedVisit ? 'Edit Clinical Visit' : 'Add Clinical Visit'}
              </Dialog.Title>
              <ClinicalVisitForm
                visit={selectedVisit}
                onSuccess={handleModalClose}
                onCancel={handleModalClose}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
