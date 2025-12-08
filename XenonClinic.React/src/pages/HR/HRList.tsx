import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { hrApi } from '../../lib/api';
import type { Employee, HRStatistics } from '../../types/hr';
import { HRForm } from '../../components/HRForm';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

export const HRList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | undefined>(undefined);

  const { data: employees, isLoading } = useQuery<Employee[]>({
    queryKey: ['hr-employees'],
    queryFn: async () => {
      const response = await hrApi.getAll();
      return response.data;
    },
  });

  const { data: stats } = useQuery<HRStatistics>({
    queryKey: ['hr-stats'],
    queryFn: async () => {
      const response = await hrApi.getStatistics();
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => hrApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hr-employees'] });
      queryClient.invalidateQueries({ queryKey: ['hr-stats'] });
    },
  });

  const handleDelete = (employee: Employee) => {
    if (window.confirm(`Are you sure you want to delete employee ${employee.fullName}?`)) {
      deleteMutation.mutate(employee.id);
    }
  };

  const handleEdit = (employee: Employee) => {
    setSelectedEmployee(employee);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedEmployee(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedEmployee(undefined);
  };

  const filteredEmployees = employees?.filter(
    (employee) =>
      employee.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.employeeNumber.includes(searchTerm)
  );

  const getRoleLabel = (role: number) => {
    const roles = ['Doctor', 'Nurse', 'Receptionist', 'Technician', 'Administrator', 'Other'];
    return roles[role] || 'Unknown';
  };

  const getStatusLabel = (status: number) => {
    const statuses = ['Active', 'On Leave', 'Suspended', 'Terminated'];
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-green-600 bg-green-100',
      1: 'text-yellow-600 bg-yellow-100',
      2: 'text-orange-600 bg-orange-100',
      3: 'text-red-600 bg-red-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Human Resources</h1>
          <p className="text-gray-600 mt-1">Manage employees and HR operations</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Add Employee
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Employees</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Active Employees</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.activeEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Listed Below</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{filteredEmployees?.length || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search employees by name, email, or employee number..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading employees...</p>
          </div>
        ) : filteredEmployees && filteredEmployees.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Employee #</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Role</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Department</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Hire Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredEmployees.map((employee) => (
                  <tr key={employee.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">{employee.employeeNumber}</td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{employee.fullName}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{getRoleLabel(employee.role)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{employee.department || '-'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{employee.email}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(employee.hireDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(employee.status)}`}>
                        {getStatusLabel(employee.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(employee)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(employee)}
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
            {searchTerm ? 'No employees found matching your search.' : 'No employees found.'}
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
                {selectedEmployee ? 'Edit Employee' : 'Add Employee'}
              </Dialog.Title>
              <HRForm
                employee={selectedEmployee}
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
