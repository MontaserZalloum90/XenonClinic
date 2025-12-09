import { useQuery, useMutation, useQueryClient } from '@tantml:react-query';
import { useState, useEffect } from 'react';
import { patientsApi } from '../../lib/api';
import type { Patient } from '../../types/patient';
import { Modal } from '../../components/ui/Modal';
import { PatientForm } from '../../components/PatientForm';
import { SkeletonTable } from '../../components/ui/LoadingSkeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { ConfirmDialog, useConfirmDialog } from '../../components/ui/ConfirmDialog';
import { useToast } from '../../components/ui/Toast';
import { useKeyboardShortcuts, createCommonShortcuts } from '../../hooks/useKeyboardShortcuts';
import { useRecentItems } from '../../hooks/useRecentItems';
import { exportToCSV, exportToExcel, printTable, formatters, type ExportColumn } from '../../utils/export';

export const PatientsList = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPatient, setSelectedPatient] = useState<Patient | undefined>(undefined);
  const [selectedIds, setSelectedIds] = useState<Set<number>>(new Set());
  const [showExportMenu, setShowExportMenu] = useState(false);
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { dialogState, showConfirm, hideConfirm, handleConfirm } = useConfirmDialog();
  const { recentItems, addRecentItem } = useRecentItems('patient');

  // Fetch patients
  const { data: patients, isLoading, error, refetch } = useQuery<Patient[]>({
    queryKey: ['patients'],
    queryFn: async () => {
      const response = await patientsApi.getAll();
      return response.data;
    },
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: number) => patientsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      showToast('success', 'Patient deleted successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Failed to delete patient: ${error.message}`);
    },
  });

  // Bulk delete mutation
  const bulkDeleteMutation = useMutation({
    mutationFn: async (ids: number[]) => {
      await Promise.all(ids.map((id) => patientsApi.delete(id)));
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      setSelectedIds(new Set());
      showToast('success', 'Selected patients deleted successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Failed to delete patients: ${error.message}`);
    },
  });

  // Keyboard shortcuts
  useKeyboardShortcuts(
    createCommonShortcuts({
      onNew: handleOpenCreateModal,
      onSearch: () => document.getElementById('patient-search')?.focus(),
      onRefresh: () => refetch(),
      onDelete: () => {
        if (selectedIds.size > 0) {
          handleBulkDelete();
        }
      },
      onClose: () => {
        if (isModalOpen) handleCloseModal();
      },
    })
  );

  // Modal handlers
  function handleOpenCreateModal() {
    setSelectedPatient(undefined);
    setIsModalOpen(true);
  }

  const handleOpenEditModal = (patient: Patient) => {
    setSelectedPatient(patient);
    setIsModalOpen(true);
    addRecentItem(patient.id.toString(), 'patient', patient.fullNameEn);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedPatient(undefined);
  };

  const handleFormSuccess = () => {
    handleCloseModal();
    showToast('success', selectedPatient ? 'Patient updated successfully' : 'Patient created successfully');
  };

  const handleDelete = (patient: Patient) => {
    showConfirm(
      'Delete Patient',
      `Are you sure you want to delete "${patient.fullNameEn}"? This action cannot be undone.`,
      () => deleteMutation.mutate(patient.id),
      'danger'
    );
  };

  const handleBulkDelete = () => {
    showConfirm(
      'Delete Multiple Patients',
      `Are you sure you want to delete ${selectedIds.size} patient(s)? This action cannot be undone.`,
      () => bulkDeleteMutation.mutate(Array.from(selectedIds)),
      'danger'
    );
  };

  const calculateAge = (dateOfBirth: string) => {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  };

  // Selection handlers
  const handleSelectAll = () => {
    if (filteredPatients) {
      if (selectedIds.size === filteredPatients.length) {
        setSelectedIds(new Set());
      } else {
        setSelectedIds(new Set(filteredPatients.map((p) => p.id)));
      }
    }
  };

  const handleSelectOne = (id: number) => {
    const newSelected = new Set(selectedIds);
    if (newSelected.has(id)) {
      newSelected.delete(id);
    } else {
      newSelected.add(id);
    }
    setSelectedIds(newSelected);
  };

  // Export handlers
  const exportColumns: ExportColumn[] = [
    { key: 'fullNameEn', label: 'Full Name (English)' },
    { key: 'fullNameAr', label: 'Full Name (Arabic)' },
    { key: 'emiratesId', label: 'Emirates ID' },
    { key: 'dateOfBirth', label: 'Date of Birth', format: formatters.date },
    { key: 'age', label: 'Age' },
    { key: 'gender', label: 'Gender', format: (v) => (v === 'M' ? 'Male' : 'Female') },
    { key: 'phoneNumber', label: 'Phone' },
    { key: 'email', label: 'Email' },
  ];

  const prepareExportData = () => {
    return (filteredPatients || []).map((p) => ({
      ...p,
      age: calculateAge(p.dateOfBirth),
    }));
  };

  const handleExportCSV = () => {
    exportToCSV(prepareExportData(), exportColumns, 'patients.csv');
    setShowExportMenu(false);
    showToast('success', 'Patients exported to CSV');
  };

  const handleExportExcel = () => {
    exportToExcel(prepareExportData(), exportColumns, 'patients.xls');
    setShowExportMenu(false);
    showToast('success', 'Patients exported to Excel');
  };

  const handlePrint = () => {
    printTable(prepareExportData(), exportColumns, 'Patients List');
    setShowExportMenu(false);
  };

  // Filter patients based on search term
  const filteredPatients = patients?.filter(
    (patient) =>
      patient.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase()) ||
      patient.emiratesId.includes(searchTerm) ||
      patient.phoneNumber?.includes(searchTerm) ||
      patient.email?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Clear selection when filter changes
  useEffect(() => {
    setSelectedIds(new Set());
  }, [searchTerm]);

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md animate-fade-in">
        <p className="text-sm text-red-600">Error loading patients: {(error as Error).message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Patients</h1>
          <p className="text-gray-600 mt-1">Manage patient records and information</p>
        </div>
        <div className="flex items-center gap-3">
          {selectedIds.size > 0 && (
            <button
              onClick={handleBulkDelete}
              className="btn bg-red-600 hover:bg-red-700 text-white"
              disabled={bulkDeleteMutation.isPending}
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
              Delete ({selectedIds.size})
            </button>
          )}
          <div className="relative">
            <button
              onClick={() => setShowExportMenu(!showExportMenu)}
              className="btn btn-secondary"
              disabled={!filteredPatients || filteredPatients.length === 0}
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              Export
            </button>
            {showExportMenu && (
              <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg z-10 border border-gray-200 animate-scale-in">
                <div className="py-1">
                  <button
                    onClick={handleExportCSV}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Export as CSV
                  </button>
                  <button
                    onClick={handleExportExcel}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Export as Excel
                  </button>
                  <button
                    onClick={handlePrint}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Print
                  </button>
                </div>
              </div>
            )}
          </div>
          <button onClick={handleOpenCreateModal} className="btn btn-primary">
            <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            New Patient
          </button>
        </div>
      </div>

      {/* Recent Patients */}
      {recentItems.length > 0 && (
        <div className="card animate-fade-in">
          <h3 className="text-sm font-medium text-gray-700 mb-3">Recently Viewed</h3>
          <div className="flex flex-wrap gap-2">
            {recentItems.map((item) => (
              <button
                key={item.id}
                onClick={() => {
                  const patient = patients?.find((p) => p.id.toString() === item.id);
                  if (patient) handleOpenEditModal(patient);
                }}
                className="px-3 py-1 text-sm bg-blue-50 text-blue-700 rounded-full hover:bg-blue-100 transition-colors"
              >
                {item.label}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Search */}
      <div className="card animate-fade-in">
        <label className="block text-sm font-medium text-gray-700 mb-2">Search Patients</label>
        <div className="relative">
          <input
            id="patient-search"
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full md:w-96 pl-10"
            placeholder="Search by name, Emirates ID, phone, or email..."
          />
          <svg
            className="w-5 h-5 text-gray-400 absolute left-3 top-1/2 transform -translate-y-1/2"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <p className="text-xs text-gray-500 mt-2">
          Tip: Press <kbd className="px-2 py-1 bg-gray-100 border border-gray-300 rounded text-xs">Ctrl</kbd> +{' '}
          <kbd className="px-2 py-1 bg-gray-100 border border-gray-300 rounded text-xs">K</kbd> to focus search
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {[
          { label: 'Total Patients', value: patients?.length || 0, color: 'text-gray-900' },
          { label: 'Male', value: patients?.filter((p) => p.gender === 'M').length || 0, color: 'text-blue-600' },
          { label: 'Female', value: patients?.filter((p) => p.gender === 'F').length || 0, color: 'text-pink-600' },
          { label: 'Search Results', value: filteredPatients?.length || 0, color: 'text-primary-600' },
        ].map((stat, index) => (
          <div key={stat.label} className="card animate-fade-in" style={{ animationDelay: `${index * 50}ms` }}>
            <p className="text-sm text-gray-600">{stat.label}</p>
            <p className={`text-2xl font-bold ${stat.color} mt-1`}>{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Table */}
      <div className="card overflow-hidden p-0 animate-fade-in">
        {isLoading ? (
          <div className="p-6">
            <SkeletonTable rows={8} columns={5} />
          </div>
        ) : filteredPatients && filteredPatients.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left">
                    <input
                      type="checkbox"
                      checked={selectedIds.size === filteredPatients.length}
                      onChange={handleSelectAll}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Patient
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Emirates ID
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Age/Gender
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Contact
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredPatients.map((patient) => (
                  <tr
                    key={patient.id}
                    className={`hover:bg-gray-50 transition-colors ${selectedIds.has(patient.id) ? 'bg-blue-50' : ''}`}
                  >
                    <td className="px-6 py-4">
                      <input
                        type="checkbox"
                        checked={selectedIds.has(patient.id)}
                        onChange={() => handleSelectOne(patient.id)}
                        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{patient.fullNameEn}</div>
                      {patient.fullNameAr && <div className="text-sm text-gray-500" dir="rtl">{patient.fullNameAr}</div>}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{patient.emiratesId}</td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">{calculateAge(patient.dateOfBirth)} years old</div>
                      <div className="text-sm text-gray-500">{patient.gender === 'M' ? 'Male' : 'Female'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">{patient.phoneNumber || '-'}</div>
                      <div className="text-sm text-gray-500">{patient.email || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <div className="flex items-center gap-3">
                        <button
                          onClick={() => handleOpenEditModal(patient)}
                          className="text-primary-600 hover:text-primary-900 transition-colors"
                          title="Edit patient"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                            />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDelete(patient)}
                          disabled={deleteMutation.isPending}
                          className="text-red-600 hover:text-red-900 disabled:opacity-50 transition-colors"
                          title="Delete patient"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                            />
                          </svg>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <EmptyState
            icon={
              <svg className="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                />
              </svg>
            }
            title={searchTerm ? 'No patients found' : 'No patients yet'}
            description={searchTerm ? 'Try adjusting your search criteria' : 'Get started by creating your first patient record'}
            action={
              searchTerm
                ? undefined
                : {
                    label: 'Create Patient',
                    onClick: handleOpenCreateModal,
                  }
            }
          />
        )}
      </div>

      {/* Keyboard Shortcuts Help */}
      <div className="text-xs text-gray-500 animate-fade-in">
        <strong>Keyboard shortcuts:</strong> <kbd className="px-1.5 py-0.5 bg-gray-100 border border-gray-300 rounded">Ctrl+N</kbd> New,{' '}
        <kbd className="px-1.5 py-0.5 bg-gray-100 border border-gray-300 rounded">Ctrl+K</kbd> Search,{' '}
        <kbd className="px-1.5 py-0.5 bg-gray-100 border border-gray-300 rounded">Ctrl+R</kbd> Refresh,{' '}
        <kbd className="px-1.5 py-0.5 bg-gray-100 border border-gray-300 rounded">Delete</kbd> Delete selected
      </div>

      {/* Create/Edit Modal */}
      <Modal isOpen={isModalOpen} onClose={handleCloseModal} title={selectedPatient ? 'Edit Patient' : 'New Patient'} size="lg">
        <PatientForm patient={selectedPatient} onSuccess={handleFormSuccess} onCancel={handleCloseModal} />
      </Modal>

      {/* Confirm Dialog */}
      <ConfirmDialog
        isOpen={dialogState.isOpen}
        onClose={hideConfirm}
        onConfirm={handleConfirm}
        title={dialogState.title}
        message={dialogState.message}
        variant={dialogState.variant}
        isLoading={deleteMutation.isPending || bulkDeleteMutation.isPending}
      />
    </div>
  );
};
