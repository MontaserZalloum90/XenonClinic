import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  BanknotesIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon,
  DocumentArrowUpIcon,
  ReceiptPercentIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface Expense {
  id: number;
  expenseNumber: string;
  category:
    | "supplies"
    | "equipment"
    | "utilities"
    | "payroll"
    | "rent"
    | "maintenance"
    | "marketing"
    | "insurance"
    | "other";
  subcategory?: string;
  vendorId?: number;
  vendorName?: string;
  description: string;
  amount: number;
  currency: string;
  expenseDate: string;
  dueDate?: string;
  paymentMethod?:
    | "cash"
    | "check"
    | "bank-transfer"
    | "credit-card"
    | "corporate-card";
  status:
    | "draft"
    | "pending-approval"
    | "approved"
    | "rejected"
    | "paid"
    | "cancelled";
  approvedBy?: string;
  approvedAt?: string;
  paidAt?: string;
  receiptAttached: boolean;
  receiptUrl?: string;
  taxDeductible: boolean;
  taxAmount?: number;
  costCenter?: string;
  departmentId?: number;
  departmentName?: string;
  projectCode?: string;
  notes?: string;
  submittedBy: string;
  submittedAt: string;
}

const categoryConfig = {
  supplies: { label: "Medical Supplies", color: "bg-blue-100 text-blue-800" },
  equipment: { label: "Equipment", color: "bg-purple-100 text-purple-800" },
  utilities: { label: "Utilities", color: "bg-yellow-100 text-yellow-800" },
  payroll: { label: "Payroll", color: "bg-green-100 text-green-800" },
  rent: { label: "Rent/Lease", color: "bg-orange-100 text-orange-800" },
  maintenance: { label: "Maintenance", color: "bg-gray-100 text-gray-800" },
  marketing: { label: "Marketing", color: "bg-pink-100 text-pink-800" },
  insurance: { label: "Insurance", color: "bg-indigo-100 text-indigo-800" },
  other: { label: "Other", color: "bg-gray-100 text-gray-800" },
};

const statusConfig = {
  draft: {
    label: "Draft",
    color: "bg-gray-100 text-gray-800",
    icon: ClockIcon,
  },
  "pending-approval": {
    label: "Pending",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  approved: {
    label: "Approved",
    color: "bg-blue-100 text-blue-800",
    icon: CheckCircleIcon,
  },
  rejected: {
    label: "Rejected",
    color: "bg-red-100 text-red-800",
    icon: XCircleIcon,
  },
  paid: {
    label: "Paid",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  cancelled: {
    label: "Cancelled",
    color: "bg-gray-100 text-gray-800",
    icon: XCircleIcon,
  },
};

export function ExpenseManagement() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<Expense["status"] | "all">(
    "all",
  );
  const [categoryFilter, setCategoryFilter] = useState<
    Expense["category"] | "all"
  >("all");
  const [dateRange, setDateRange] = useState<{ start: string; end: string }>({
    start: new Date(new Date().setMonth(new Date().getMonth() - 1))
      .toISOString()
      .split("T")[0],
    end: new Date().toISOString().split("T")[0],
  });
  const [isModalOpen, setIsModalOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: expenses = [], isLoading } = useQuery({
    queryKey: ["expenses", dateRange],
    queryFn: () =>
      api.get<Expense[]>(
        `/api/financial/expenses?start=${dateRange.start}&end=${dateRange.end}`,
      ),
  });

  const approveExpenseMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/financial/expenses/${id}/approve`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["expenses"] }),
  });

  const rejectExpenseMutation = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason: string }) =>
      api.post(`/api/financial/expenses/${id}/reject`, { reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["expenses"] }),
  });

  const markPaidMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/financial/expenses/${id}/pay`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["expenses"] }),
  });

  const createExpenseMutation = useMutation({
    mutationFn: (data: {
      category: string;
      description: string;
      amount: number;
      expenseDate: string;
      vendorName?: string;
      dueDate?: string;
      paymentMethod?: string;
      taxDeductible: boolean;
      costCenter?: string;
      notes?: string;
    }) => api.post("/api/financial/expenses", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expenses"] });
      setIsModalOpen(false);
    },
  });

  const filteredExpenses = expenses.filter((expense) => {
    const matchesSearch =
      expense.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
      expense.expenseNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (expense.vendorName &&
        expense.vendorName.toLowerCase().includes(searchTerm.toLowerCase()));
    const matchesStatus =
      statusFilter === "all" || expense.status === statusFilter;
    const matchesCategory =
      categoryFilter === "all" || expense.category === categoryFilter;
    return matchesSearch && matchesStatus && matchesCategory;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const formatCurrency = (amount: number, currency: string = "USD") => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency,
    }).format(amount);
  };

  const totalExpenses = expenses.reduce((sum, e) => sum + e.amount, 0);
  const pendingApproval = expenses.filter(
    (e) => e.status === "pending-approval",
  ).length;
  const approvedUnpaid = expenses
    .filter((e) => e.status === "approved")
    .reduce((sum, e) => sum + e.amount, 0);
  const paidThisMonth = expenses
    .filter((e) => e.status === "paid")
    .reduce((sum, e) => sum + e.amount, 0);

  const categoryTotals = expenses.reduce(
    (acc, exp) => {
      acc[exp.category] = (acc[exp.category] || 0) + exp.amount;
      return acc;
    },
    {} as Record<string, number>,
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Expense Management
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Track and manage clinic expenses and vendor payments
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Record Expense
        </button>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {formatCurrency(totalExpenses)}
          </div>
          <div className="text-sm text-gray-500">Total Expenses</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {pendingApproval}
          </div>
          <div className="text-sm text-gray-500">Pending Approval</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {formatCurrency(approvedUnpaid)}
          </div>
          <div className="text-sm text-gray-500">Approved (Unpaid)</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {formatCurrency(paidThisMonth)}
          </div>
          <div className="text-sm text-gray-500">Paid This Period</div>
        </div>
      </div>

      <div className="rounded-lg bg-white p-4 shadow">
        <h3 className="text-sm font-medium text-gray-700 mb-3">
          Expenses by Category
        </h3>
        <div className="grid grid-cols-3 gap-4 sm:grid-cols-5">
          {Object.entries(categoryTotals)
            .sort(([, a], [, b]) => b - a)
            .slice(0, 5)
            .map(([category, amount]) => (
              <div key={category} className="text-center">
                <div
                  className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${categoryConfig[category as Expense["category"]]?.color || "bg-gray-100"}`}
                >
                  {categoryConfig[category as Expense["category"]]?.label ||
                    category}
                </div>
                <div className="mt-1 text-sm font-medium text-gray-900">
                  {formatCurrency(amount)}
                </div>
              </div>
            ))}
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center flex-wrap">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search expenses..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <div className="flex gap-2 items-center">
          <input
            type="date"
            value={dateRange.start}
            onChange={(e) =>
              setDateRange({ ...dateRange, start: e.target.value })
            }
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
          />
          <span className="text-gray-500">to</span>
          <input
            type="date"
            value={dateRange.end}
            onChange={(e) =>
              setDateRange({ ...dateRange, end: e.target.value })
            }
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as Expense["status"] | "all")
          }
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Status</option>
          {Object.entries(statusConfig).map(([key, value]) => (
            <option key={key} value={key}>
              {value.label}
            </option>
          ))}
        </select>
        <select
          value={categoryFilter}
          onChange={(e) =>
            setCategoryFilter(e.target.value as Expense["category"] | "all")
          }
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Categories</option>
          {Object.entries(categoryConfig).map(([key, value]) => (
            <option key={key} value={key}>
              {value.label}
            </option>
          ))}
        </select>
      </div>

      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredExpenses.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <BanknotesIcon className="h-12 w-12 mb-2" />
            <p>No expenses found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Expense
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Category
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Vendor
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Amount
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredExpenses.map((expense) => {
                const status = statusConfig[expense.status];
                const category = categoryConfig[expense.category];
                const StatusIcon = status.icon;

                return (
                  <tr key={expense.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="flex items-center">
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {expense.description}
                          </div>
                          <div className="font-mono text-xs text-gray-500">
                            {expense.expenseNumber}
                          </div>
                        </div>
                        {expense.receiptAttached && (
                          <DocumentArrowUpIcon
                            className="ml-2 h-4 w-4 text-green-500"
                            title="Receipt attached"
                          />
                        )}
                        {expense.taxDeductible && (
                          <ReceiptPercentIcon
                            className="ml-1 h-4 w-4 text-blue-500"
                            title="Tax deductible"
                          />
                        )}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${category.color}`}
                      >
                        {category.label}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900">
                      {expense.vendorName || "-"}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {formatCurrency(expense.amount, expense.currency)}
                      </div>
                      {expense.taxAmount && (
                        <div className="text-xs text-gray-500">
                          Tax: {formatCurrency(expense.taxAmount)}
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatDate(expense.expenseDate)}
                      {expense.dueDate && (
                        <div className="text-xs">
                          Due: {formatDate(expense.dueDate)}
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        <StatusIcon className="h-3.5 w-3.5 mr-1" />
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        {expense.status === "pending-approval" && (
                          <>
                            <button
                              onClick={() =>
                                approveExpenseMutation.mutate(expense.id)
                              }
                              className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                            >
                              Approve
                            </button>
                            <button
                              onClick={() =>
                                rejectExpenseMutation.mutate({
                                  id: expense.id,
                                  reason: "Rejected by manager",
                                })
                              }
                              className="rounded bg-red-600 px-3 py-1 text-xs font-medium text-white hover:bg-red-700"
                            >
                              Reject
                            </button>
                          </>
                        )}
                        {expense.status === "approved" && (
                          <button
                            onClick={() => markPaidMutation.mutate(expense.id)}
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Mark Paid
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Record Expense Modal */}
      <Dialog
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="border-b px-6 py-4">
              <Dialog.Title className="text-lg font-semibold text-gray-900">
                Record New Expense
              </Dialog.Title>
            </div>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                createExpenseMutation.mutate({
                  category: formData.get("category") as string,
                  description: formData.get("description") as string,
                  amount: parseFloat(formData.get("amount") as string),
                  expenseDate: formData.get("expenseDate") as string,
                  vendorName:
                    (formData.get("vendorName") as string) || undefined,
                  dueDate: (formData.get("dueDate") as string) || undefined,
                  paymentMethod:
                    (formData.get("paymentMethod") as string) || undefined,
                  taxDeductible: formData.get("taxDeductible") === "on",
                  costCenter:
                    (formData.get("costCenter") as string) || undefined,
                  notes: (formData.get("notes") as string) || undefined,
                });
              }}
              className="p-6 space-y-4"
            >
              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Description *
                  </label>
                  <input
                    type="text"
                    name="description"
                    required
                    placeholder="e.g., Office supplies purchase"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Category *
                  </label>
                  <select
                    name="category"
                    required
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  >
                    {Object.entries(categoryConfig).map(([key, value]) => (
                      <option key={key} value={key}>
                        {value.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Amount *
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500">
                      $
                    </span>
                    <input
                      type="number"
                      name="amount"
                      required
                      min="0.01"
                      step="0.01"
                      className="w-full rounded-md border border-gray-300 py-2 pl-8 pr-3 focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Expense Date *
                  </label>
                  <input
                    type="date"
                    name="expenseDate"
                    required
                    defaultValue={new Date().toISOString().split("T")[0]}
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Due Date
                  </label>
                  <input
                    type="date"
                    name="dueDate"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Vendor Name
                  </label>
                  <input
                    type="text"
                    name="vendorName"
                    placeholder="Vendor or supplier"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Payment Method
                  </label>
                  <select
                    name="paymentMethod"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="">Select method</option>
                    <option value="cash">Cash</option>
                    <option value="check">Check</option>
                    <option value="bank-transfer">Bank Transfer</option>
                    <option value="credit-card">Credit Card</option>
                    <option value="corporate-card">Corporate Card</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Cost Center
                  </label>
                  <input
                    type="text"
                    name="costCenter"
                    placeholder="e.g., Operations, Marketing"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    name="taxDeductible"
                    id="taxDeductible"
                    className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <label
                    htmlFor="taxDeductible"
                    className="ml-2 text-sm text-gray-700"
                  >
                    Tax Deductible
                  </label>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Notes
                </label>
                <textarea
                  name="notes"
                  rows={2}
                  className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                />
              </div>

              <div className="flex justify-end gap-3 pt-4 border-t">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createExpenseMutation.isPending}
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                >
                  {createExpenseMutation.isPending
                    ? "Recording..."
                    : "Record Expense"}
                </button>
              </div>
            </form>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
}
