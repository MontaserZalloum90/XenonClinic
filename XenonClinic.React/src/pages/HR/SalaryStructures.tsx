import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import type { SalaryStructure, SalaryStructureFormData } from '../../types/payroll';

// Mock API - Replace with actual API when backend is ready
const salaryStructureApi = {
  getAll: () => Promise.resolve({ data: [] as SalaryStructure[] }),
  getById: (_id: number) => Promise.resolve({ data: {} as SalaryStructure }),
  getActive: () => Promise.resolve({ data: [] as SalaryStructure[] }),
  create: () => Promise.resolve({ data: {} as SalaryStructure }),
  update: () => Promise.resolve({ data: {} as SalaryStructure }),
  delete: () => Promise.resolve({ data: {} }),
  activate: () => Promise.resolve({ data: {} as SalaryStructure }),
  deactivate: () => Promise.resolve({ data: {} as SalaryStructure }),
};

export const SalaryStructures = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedStructure, setSelectedStructure] = useState<SalaryStructure | undefined>(undefined);

  const { data: structures, isLoading } = useQuery<SalaryStructure[]>({
    queryKey: ['salary-structures'],
    queryFn: async () => {
      const response = await salaryStructureApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: SalaryStructureFormData) => salaryStructureApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salary-structures'] });
      handleModalClose();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: SalaryStructureFormData }) =>
      salaryStructureApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salary-structures'] });
      handleModalClose();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => salaryStructureApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salary-structures'] });
    },
  });

  const handleCreate = () => {
    setSelectedStructure(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (structure: SalaryStructure) => {
    setSelectedStructure(structure);
    setIsModalOpen(true);
  };

  const handleDelete = (structure: SalaryStructure) => {
    if (window.confirm(`Are you sure you want to delete salary structure "${structure.name}"?`)) {
      deleteMutation.mutate(structure.id);
    }
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedStructure(undefined);
  };

  const filteredStructures = structures?.filter(
    (structure) =>
      structure.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      structure.description?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const calculateTotalCompensation = (structure: SalaryStructure) => {
    return (
      structure.basicSalary +
      structure.housingAllowance +
      structure.transportAllowance +
      structure.otherAllowances
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Salary Structures</h1>
          <p className="text-gray-600 mt-1">Manage salary structure templates</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Create Structure
        </button>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search salary structures..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading salary structures...</p>
          </div>
        ) : filteredStructures && filteredStructures.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Structure Name</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Basic Salary</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Housing</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Transport</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Other</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Effective Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredStructures.map((structure) => (
                  <tr key={structure.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <div className="text-sm font-medium text-gray-900">{structure.name}</div>
                      {structure.description && (
                        <div className="text-xs text-gray-500">{structure.description}</div>
                      )}
                      {structure.employeeCount !== undefined && structure.employeeCount > 0 && (
                        <div className="text-xs text-gray-500">{structure.employeeCount} employees</div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      AED {structure.basicSalary.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      AED {structure.housingAllowance.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      AED {structure.transportAllowance.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      AED {structure.otherAllowances.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      AED {calculateTotalCompensation(structure).toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(structure.effectiveDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${
                          structure.isActive
                            ? 'text-green-600 bg-green-100'
                            : 'text-gray-600 bg-gray-100'
                        }`}
                      >
                        {structure.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(structure)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(structure)}
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
            {searchTerm ? 'No salary structures found matching your search.' : 'No salary structures found.'}
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedStructure ? 'Edit Salary Structure' : 'Create Salary Structure'}
              </Dialog.Title>
              <SalaryStructureForm
                structure={selectedStructure}
                onSubmit={(data) => {
                  if (selectedStructure) {
                    updateMutation.mutate({ id: selectedStructure.id, data });
                  } else {
                    createMutation.mutate(data);
                  }
                }}
                onCancel={handleModalClose}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};

interface SalaryStructureFormProps {
  structure?: SalaryStructure;
  onSubmit: (data: SalaryStructureFormData) => void;
  onCancel: () => void;
}

const SalaryStructureForm = ({ structure, onSubmit, onCancel }: SalaryStructureFormProps) => {
  const [formData, setFormData] = useState<SalaryStructureFormData>({
    name: structure?.name || '',
    basicSalary: structure?.basicSalary || 0,
    housingAllowance: structure?.housingAllowance || 0,
    transportAllowance: structure?.transportAllowance || 0,
    otherAllowances: structure?.otherAllowances || 0,
    effectiveDate: structure?.effectiveDate || new Date().toISOString().split('T')[0],
    endDate: structure?.endDate || '',
    description: structure?.description || '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const calculateTotal = () => {
    return (
      formData.basicSalary +
      formData.housingAllowance +
      formData.transportAllowance +
      formData.otherAllowances
    );
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Structure Name <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          className="input w-full"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          className="input w-full"
          rows={2}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Basic Salary (AED) <span className="text-red-500">*</span>
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={formData.basicSalary}
            onChange={(e) => setFormData({ ...formData, basicSalary: parseFloat(e.target.value) || 0 })}
            className="input w-full"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Housing Allowance (AED)
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={formData.housingAllowance}
            onChange={(e) => setFormData({ ...formData, housingAllowance: parseFloat(e.target.value) || 0 })}
            className="input w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Transport Allowance (AED)
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={formData.transportAllowance}
            onChange={(e) => setFormData({ ...formData, transportAllowance: parseFloat(e.target.value) || 0 })}
            className="input w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Other Allowances (AED)
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={formData.otherAllowances}
            onChange={(e) => setFormData({ ...formData, otherAllowances: parseFloat(e.target.value) || 0 })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="bg-gray-50 p-4 rounded-lg">
        <div className="flex justify-between items-center">
          <span className="text-sm font-medium text-gray-700">Total Compensation</span>
          <span className="text-lg font-bold text-primary-600">AED {calculateTotal().toFixed(2)}</span>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Effective Date <span className="text-red-500">*</span>
          </label>
          <input
            type="date"
            value={formData.effectiveDate}
            onChange={(e) => setFormData({ ...formData, effectiveDate: e.target.value })}
            className="input w-full"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            End Date (Optional)
          </label>
          <input
            type="date"
            value={formData.endDate}
            onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" className="btn btn-primary">
          {structure ? 'Update' : 'Create'} Structure
        </button>
      </div>
    </form>
  );
};
