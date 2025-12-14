import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import type { PayrollRecord, PayrollStatistics, Allowance, Deduction } from '../../types/payroll';

// Mock API - Replace with actual API when backend is ready
const payrollApi = {
  getAll: () => Promise.resolve({ data: [] as PayrollRecord[] }),
  getByPeriod: (period: string) => Promise.resolve({ data: [] as PayrollRecord[] }),
  getById: (id: number) => Promise.resolve({ data: {} as PayrollRecord }),
  create: (data: Partial<PayrollRecord>) => Promise.resolve({ data: {} as PayrollRecord }),
  update: (id: number, data: Partial<PayrollRecord>) => Promise.resolve({ data: {} as PayrollRecord }),
  delete: (id: number) => Promise.resolve({ data: {} }),
  process: (id: number) => Promise.resolve({ data: {} as PayrollRecord }),
  approve: (id: number) => Promise.resolve({ data: {} as PayrollRecord }),
  pay: (id: number, data: { paymentDate: string; paymentMethod: string }) => Promise.resolve({ data: {} as PayrollRecord }),
  cancel: (id: number) => Promise.resolve({ data: {} as PayrollRecord }),
  getStatistics: () => Promise.resolve({
    data: {
      totalPayroll: 0,
      pendingApproval: 0,
      paidThisMonth: 0,
      totalEmployeesInPayroll: 0,
      averageSalary: 0,
      totalAllowances: 0,
      totalDeductions: 0,
    } as PayrollStatistics
  }),
};

export const PayrollList = () => {
  const queryClient = useQueryClient();
  const [selectedPeriod, setSelectedPeriod] = useState(() => {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedPayroll, setSelectedPayroll] = useState<PayrollRecord | undefined>(undefined);
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState('Bank Transfer');

  const { data: payrollRecords, isLoading } = useQuery<PayrollRecord[]>({
    queryKey: ['payroll-records', selectedPeriod],
    queryFn: async () => {
      const response = await payrollApi.getByPeriod(selectedPeriod);
      return response.data;
    },
  });

  const { data: stats } = useQuery<PayrollStatistics>({
    queryKey: ['payroll-stats'],
    queryFn: async () => {
      const response = await payrollApi.getStatistics();
      return response.data;
    },
  });

  const processMutation = useMutation({
    mutationFn: (id: number) => payrollApi.process(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll-records'] });
      queryClient.invalidateQueries({ queryKey: ['payroll-stats'] });
    },
  });

  const approveMutation = useMutation({
    mutationFn: (id: number) => payrollApi.approve(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll-records'] });
      queryClient.invalidateQueries({ queryKey: ['payroll-stats'] });
    },
  });

  const payMutation = useMutation({
    mutationFn: ({ id, method }: { id: number; method: string }) =>
      payrollApi.pay(id, { paymentMethod: method }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll-records'] });
      queryClient.invalidateQueries({ queryKey: ['payroll-stats'] });
      setIsPaymentModalOpen(false);
      setSelectedPayroll(undefined);
    },
  });

  const cancelMutation = useMutation({
    mutationFn: (id: number) => payrollApi.cancel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll-records'] });
      queryClient.invalidateQueries({ queryKey: ['payroll-stats'] });
    },
  });

  const handleProcess = (payroll: PayrollRecord) => {
    if (window.confirm(`Process payroll for ${payroll.employeeName}?`)) {
      processMutation.mutate(payroll.id);
    }
  };

  const handleApprove = (payroll: PayrollRecord) => {
    if (window.confirm(`Approve payroll for ${payroll.employeeName}?`)) {
      approveMutation.mutate(payroll.id);
    }
  };

  const handlePay = (payroll: PayrollRecord) => {
    setSelectedPayroll(payroll);
    setIsPaymentModalOpen(true);
  };

  const handlePaymentSubmit = () => {
    if (selectedPayroll) {
      payMutation.mutate({ id: selectedPayroll.id, method: paymentMethod });
    }
  };

  const handleCancel = (payroll: PayrollRecord) => {
    if (window.confirm(`Cancel payroll for ${payroll.employeeName}? This action cannot be undone.`)) {
      cancelMutation.mutate(payroll.id);
    }
  };

  const handleViewDetails = (payroll: PayrollRecord) => {
    setSelectedPayroll(payroll);
    setIsDetailModalOpen(true);
  };

  const filteredRecords = payrollRecords?.filter(
    (record) =>
      record.employeeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.employeeNumber?.includes(searchTerm) ||
      record.department?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusLabel = (status: number) => {
    const statuses = ['Draft', 'Processed', 'Approved', 'Paid', 'Cancelled'];
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'text-gray-600 bg-gray-100',
      1: 'text-blue-600 bg-blue-100',
      2: 'text-yellow-600 bg-yellow-100',
      3: 'text-green-600 bg-green-100',
      4: 'text-red-600 bg-red-100',
    };
    return colors[status as keyof typeof colors] || 'text-gray-600 bg-gray-100';
  };

  const calculateTotalAllowances = (allowances: Allowance[]) => {
    return allowances.reduce((sum, a) => sum + a.amount, 0);
  };

  const calculateTotalDeductions = (deductions: Deduction[]) => {
    return deductions.reduce((sum, d) => sum + d.amount, 0);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Payroll Management</h1>
          <p className="text-gray-600 mt-1">Manage employee payroll and salary processing</p>
        </div>
        <button className="btn btn-primary">
          Process Payroll
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Payroll</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            AED {stats?.totalPayroll?.toFixed(2) || '0.00'}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending Approval</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.pendingApproval || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Paid This Month</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats?.paidThisMonth || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4 flex flex-col sm:flex-row gap-3">
          <div className="flex-1">
            <input
              type="text"
              placeholder="Search by employee name, number, or department..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input w-full"
            />
          </div>
          <div>
            <input
              type="month"
              value={selectedPeriod}
              onChange={(e) => setSelectedPeriod(e.target.value)}
              className="input"
            />
          </div>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading payroll records...</p>
          </div>
        ) : filteredRecords && filteredRecords.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Employee</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Period</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Basic</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Allowances</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Deductions</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Net Salary</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredRecords.map((record) => (
                  <tr key={record.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <div className="text-sm font-medium text-gray-900">{record.employeeName}</div>
                      <div className="text-xs text-gray-500">{record.employeeNumber}</div>
                      {record.department && (
                        <div className="text-xs text-gray-500">{record.department}</div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(record.period + '-01'), 'MMM yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      AED {record.basicSalary.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm text-green-600">
                      +AED {calculateTotalAllowances(record.allowances).toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm text-red-600">
                      -AED {calculateTotalDeductions(record.deductions).toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      AED {record.netSalary.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(record.status)}`}>
                        {getStatusLabel(record.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewDetails(record)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          View
                        </button>
                        {record.status === 0 && (
                          <button
                            onClick={() => handleProcess(record)}
                            className="text-blue-600 hover:text-blue-800"
                          >
                            Process
                          </button>
                        )}
                        {record.status === 1 && (
                          <button
                            onClick={() => handleApprove(record)}
                            className="text-green-600 hover:text-green-800"
                          >
                            Approve
                          </button>
                        )}
                        {record.status === 2 && (
                          <button
                            onClick={() => handlePay(record)}
                            className="text-green-600 hover:text-green-800"
                          >
                            Pay
                          </button>
                        )}
                        {record.status < 3 && (
                          <button
                            onClick={() => handleCancel(record)}
                            className="text-red-600 hover:text-red-800"
                          >
                            Cancel
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm ? 'No payroll records found matching your search.' : 'No payroll records found for this period.'}
          </div>
        )}
      </div>

      {/* Detail Modal */}
      <Dialog open={isDetailModalOpen} onClose={() => setIsDetailModalOpen(false)} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Payroll Details
              </Dialog.Title>
              {selectedPayroll && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Employee</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPayroll.employeeName}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Employee Number</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPayroll.employeeNumber || '-'}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Period</label>
                      <p className="text-sm text-gray-900 mt-1">
                        {format(new Date(selectedPayroll.period + '-01'), 'MMMM yyyy')}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Status</label>
                      <span className={`inline-block px-2 py-1 rounded-full text-xs font-medium mt-1 ${getStatusColor(selectedPayroll.status)}`}>
                        {getStatusLabel(selectedPayroll.status)}
                      </span>
                    </div>
                  </div>

                  <div className="border-t pt-4">
                    <h3 className="text-sm font-medium text-gray-900 mb-3">Salary Breakdown</h3>
                    <div className="space-y-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-600">Basic Salary</span>
                        <span className="text-sm font-medium text-gray-900">
                          AED {selectedPayroll.basicSalary.toFixed(2)}
                        </span>
                      </div>
                      {selectedPayroll.allowances.length > 0 && (
                        <>
                          <div className="text-sm font-medium text-gray-700 mt-3">Allowances</div>
                          {selectedPayroll.allowances.map((allowance) => (
                            <div key={allowance.id} className="flex justify-between pl-4">
                              <span className="text-sm text-gray-600">{allowance.name}</span>
                              <span className="text-sm text-green-600">
                                +AED {allowance.amount.toFixed(2)}
                              </span>
                            </div>
                          ))}
                        </>
                      )}
                      {selectedPayroll.deductions.length > 0 && (
                        <>
                          <div className="text-sm font-medium text-gray-700 mt-3">Deductions</div>
                          {selectedPayroll.deductions.map((deduction) => (
                            <div key={deduction.id} className="flex justify-between pl-4">
                              <span className="text-sm text-gray-600">{deduction.name}</span>
                              <span className="text-sm text-red-600">
                                -AED {deduction.amount.toFixed(2)}
                              </span>
                            </div>
                          ))}
                        </>
                      )}
                      <div className="border-t pt-2 flex justify-between">
                        <span className="text-sm font-medium text-gray-900">Gross Salary</span>
                        <span className="text-sm font-medium text-gray-900">
                          AED {selectedPayroll.grossSalary.toFixed(2)}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm font-medium text-gray-900">Net Salary</span>
                        <span className="text-sm font-bold text-primary-600">
                          AED {selectedPayroll.netSalary.toFixed(2)}
                        </span>
                      </div>
                    </div>
                  </div>

                  {selectedPayroll.notes && (
                    <div className="border-t pt-4">
                      <label className="block text-sm font-medium text-gray-700">Notes</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPayroll.notes}</p>
                    </div>
                  )}

                  <div className="border-t pt-4">
                    <div className="grid grid-cols-2 gap-4 text-xs text-gray-500">
                      {selectedPayroll.processedDate && (
                        <div>
                          <span className="font-medium">Processed:</span>{' '}
                          {format(new Date(selectedPayroll.processedDate), 'MMM d, yyyy')}
                        </div>
                      )}
                      {selectedPayroll.approvedDate && (
                        <div>
                          <span className="font-medium">Approved:</span>{' '}
                          {format(new Date(selectedPayroll.approvedDate), 'MMM d, yyyy')}
                          {selectedPayroll.approvedBy && ` by ${selectedPayroll.approvedBy}`}
                        </div>
                      )}
                      {selectedPayroll.paidDate && (
                        <div>
                          <span className="font-medium">Paid:</span>{' '}
                          {format(new Date(selectedPayroll.paidDate), 'MMM d, yyyy')}
                          {selectedPayroll.paymentMethod && ` via ${selectedPayroll.paymentMethod}`}
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="flex justify-end gap-2 pt-4">
                    <button
                      onClick={() => setIsDetailModalOpen(false)}
                      className="btn btn-secondary"
                    >
                      Close
                    </button>
                  </div>
                </div>
              )}
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* Payment Modal */}
      <Dialog open={isPaymentModalOpen} onClose={() => setIsPaymentModalOpen(false)} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-md w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Process Payment
              </Dialog.Title>
              {selectedPayroll && (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Employee
                    </label>
                    <p className="text-sm text-gray-900">{selectedPayroll.employeeName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Net Salary
                    </label>
                    <p className="text-lg font-bold text-gray-900">
                      AED {selectedPayroll.netSalary.toFixed(2)}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Payment Method
                    </label>
                    <select
                      value={paymentMethod}
                      onChange={(e) => setPaymentMethod(e.target.value)}
                      className="input w-full"
                    >
                      <option value="Bank Transfer">Bank Transfer</option>
                      <option value="Cash">Cash</option>
                      <option value="Cheque">Cheque</option>
                    </select>
                  </div>
                  <div className="flex justify-end gap-2 pt-4">
                    <button
                      onClick={() => setIsPaymentModalOpen(false)}
                      className="btn btn-secondary"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handlePaymentSubmit}
                      className="btn btn-primary"
                    >
                      Confirm Payment
                    </button>
                  </div>
                </div>
              )}
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
