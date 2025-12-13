import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { hrApi } from '../../lib/api';
import type { Employee, HRStatistics, EmployeeStatus, EmployeeRole } from '../../types/hr';
import type { PayrollRecord } from '../../types/payroll';
import { HRForm } from '../../components/HRForm';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';

// Mock payroll API - Replace with actual API when backend is ready
const payrollApi = {
  getByEmployee: (employeeId: number) => Promise.resolve({ data: [] as PayrollRecord[] }),
};

export const EmployeesList = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedDepartment, setSelectedDepartment] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<number | ''>('');
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [isPayrollHistoryModalOpen, setIsPayrollHistoryModalOpen] = useState(false);
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

  const { data: employeePayrollHistory } = useQuery<PayrollRecord[]>({
    queryKey: ['employee-payroll', selectedEmployee?.id],
    queryFn: async () => {
      if (!selectedEmployee) return [];
      const response = await payrollApi.getByEmployee(selectedEmployee.id);
      return response.data;
    },
    enabled: !!selectedEmployee && isPayrollHistoryModalOpen,
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
    setIsEditModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedEmployee(undefined);
    setIsEditModalOpen(true);
  };

  const handleViewDetails = (employee: Employee) => {
    setSelectedEmployee(employee);
    setIsDetailModalOpen(true);
  };

  const handleViewPayrollHistory = (employee: Employee) => {
    setSelectedEmployee(employee);
    setIsPayrollHistoryModalOpen(true);
  };

  const handleModalClose = () => {
    setIsEditModalOpen(false);
    setIsDetailModalOpen(false);
    setIsPayrollHistoryModalOpen(false);
    setSelectedEmployee(undefined);
  };

  const filteredEmployees = employees?.filter((employee) => {
    const matchesSearch =
      employee.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.employeeNumber.includes(searchTerm);

    const matchesDepartment = !selectedDepartment || employee.department === selectedDepartment;
    const matchesStatus = selectedStatus === '' || employee.status === selectedStatus;

    return matchesSearch && matchesDepartment && matchesStatus;
  });

  const departments = Array.from(new Set(employees?.map((e) => e.department).filter(Boolean)));

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

  const getPayrollStatusLabel = (status: number) => {
    const statuses = ['Draft', 'Processed', 'Approved', 'Paid', 'Cancelled'];
    return statuses[status] || 'Unknown';
  };

  const getPayrollStatusColor = (status: number) => {
    const colors = {
      0: 'text-gray-600 bg-gray-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-yellow-600 bg-yellow-100',
      3: 'text-green-600 bg-green-100',
      4: 'text-red-600 bg-red-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Employee Management</h1>
          <p className="text-gray-600 mt-1">Detailed employee records and management</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Add Employee
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Employees</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Active Employees</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.activeEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">On Leave</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.onLeaveEmployees || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">New Hires This Month</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">{stats?.newHiresThisMonth || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4 flex flex-col sm:flex-row gap-3">
          <div className="flex-1">
            <input
              type="text"
              placeholder="Search employees by name, email, or employee number..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input w-full"
            />
          </div>
          <div>
            <select
              value={selectedDepartment}
              onChange={(e) => setSelectedDepartment(e.target.value)}
              className="input"
            >
              <option value="">All Departments</option>
              {departments.map((dept) => (
                <option key={dept} value={dept}>
                  {dept}
                </option>
              ))}
            </select>
          </div>
          <div>
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value === '' ? '' : parseInt(e.target.value))}
              className="input"
            >
              <option value="">All Statuses</option>
              <option value="0">Active</option>
              <option value="1">On Leave</option>
              <option value="2">Suspended</option>
              <option value="3">Terminated</option>
            </select>
          </div>
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
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Contact</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Hire Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredEmployees.map((employee) => (
                  <tr key={employee.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">{employee.employeeNumber}</td>
                    <td className="px-4 py-3">
                      <div className="text-sm font-medium text-gray-900">{employee.fullName}</div>
                      <div className="text-xs text-gray-500">{employee.email}</div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{getRoleLabel(employee.role)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{employee.department || '-'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{employee.phoneNumber || '-'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(employee.hireDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(employee.status)}`}>
                        {getStatusLabel(employee.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2 flex-wrap">
                        <button
                          onClick={() => handleViewDetails(employee)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          View
                        </button>
                        <button
                          onClick={() => handleEdit(employee)}
                          className="text-blue-600 hover:text-blue-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleViewPayrollHistory(employee)}
                          className="text-green-600 hover:text-green-800"
                        >
                          Payroll
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
            {searchTerm || selectedDepartment || selectedStatus !== ''
              ? 'No employees found matching your filters.'
              : 'No employees found.'}
          </div>
        )}
      </div>

      {/* Edit Modal */}
      <Dialog open={isEditModalOpen} onClose={handleModalClose} className="relative z-50">
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

      {/* Detail Modal */}
      <Dialog open={isDetailModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Employee Details
              </Dialog.Title>
              {selectedEmployee && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Full Name</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedEmployee.fullName}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Employee Number</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedEmployee.employeeNumber}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Email</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedEmployee.email}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Phone Number</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedEmployee.phoneNumber || '-'}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Role</label>
                      <p className="text-sm text-gray-900 mt-1">{getRoleLabel(selectedEmployee.role)}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Department</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedEmployee.department || '-'}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Status</label>
                      <span className={`inline-block px-2 py-1 rounded-full text-xs font-medium mt-1 ${getStatusColor(selectedEmployee.status)}`}>
                        {getStatusLabel(selectedEmployee.status)}
                      </span>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Hire Date</label>
                      <p className="text-sm text-gray-900 mt-1">
                        {format(new Date(selectedEmployee.hireDate), 'MMMM d, yyyy')}
                      </p>
                    </div>
                    {selectedEmployee.salary && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">Salary</label>
                        <p className="text-sm text-gray-900 mt-1">AED {selectedEmployee.salary.toFixed(2)}</p>
                      </div>
                    )}
                    {selectedEmployee.nationalId && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">National ID</label>
                        <p className="text-sm text-gray-900 mt-1">{selectedEmployee.nationalId}</p>
                      </div>
                    )}
                    {selectedEmployee.dateOfBirth && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">Date of Birth</label>
                        <p className="text-sm text-gray-900 mt-1">
                          {format(new Date(selectedEmployee.dateOfBirth), 'MMMM d, yyyy')}
                        </p>
                      </div>
                    )}
                  </div>

                  {selectedEmployee.address && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Address</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedEmployee.address}</p>
                    </div>
                  )}

                  {(selectedEmployee.emergencyContact || selectedEmployee.emergencyPhone) && (
                    <div className="border-t pt-4">
                      <h3 className="text-sm font-medium text-gray-900 mb-3">Emergency Contact</h3>
                      <div className="grid grid-cols-2 gap-4">
                        {selectedEmployee.emergencyContact && (
                          <div>
                            <label className="block text-sm font-medium text-gray-700">Name</label>
                            <p className="text-sm text-gray-900 mt-1">{selectedEmployee.emergencyContact}</p>
                          </div>
                        )}
                        {selectedEmployee.emergencyPhone && (
                          <div>
                            <label className="block text-sm font-medium text-gray-700">Phone</label>
                            <p className="text-sm text-gray-900 mt-1">{selectedEmployee.emergencyPhone}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  )}

                  <div className="flex justify-end gap-2 pt-4">
                    <button
                      onClick={handleModalClose}
                      className="btn btn-secondary"
                    >
                      Close
                    </button>
                    <button
                      onClick={() => {
                        handleModalClose();
                        handleEdit(selectedEmployee);
                      }}
                      className="btn btn-primary"
                    >
                      Edit Employee
                    </button>
                  </div>
                </div>
              )}
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* Payroll History Modal */}
      <Dialog open={isPayrollHistoryModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Payroll History - {selectedEmployee?.fullName}
              </Dialog.Title>
              {employeePayrollHistory && employeePayrollHistory.length > 0 ? (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Period</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Basic</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Allowances</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Deductions</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Net Salary</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Paid Date</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {employeePayrollHistory.map((record) => (
                        <tr key={record.id} className="hover:bg-gray-50">
                          <td className="px-4 py-3 text-sm text-gray-900">
                            {format(new Date(record.period + '-01'), 'MMM yyyy')}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-900">
                            AED {record.basicSalary.toFixed(2)}
                          </td>
                          <td className="px-4 py-3 text-sm text-green-600">
                            +AED {record.totalAllowances.toFixed(2)}
                          </td>
                          <td className="px-4 py-3 text-sm text-red-600">
                            -AED {record.totalDeductions.toFixed(2)}
                          </td>
                          <td className="px-4 py-3 text-sm font-medium text-gray-900">
                            AED {record.netSalary.toFixed(2)}
                          </td>
                          <td className="px-4 py-3 text-sm">
                            <span className={`px-2 py-1 rounded-full text-xs font-medium ${getPayrollStatusColor(record.status)}`}>
                              {getPayrollStatusLabel(record.status)}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {record.paidDate ? format(new Date(record.paidDate), 'MMM d, yyyy') : '-'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500">
                  No payroll history found for this employee.
                </div>
              )}
              <div className="flex justify-end gap-2 pt-4">
                <button onClick={handleModalClose} className="btn btn-secondary">
                  Close
                </button>
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
