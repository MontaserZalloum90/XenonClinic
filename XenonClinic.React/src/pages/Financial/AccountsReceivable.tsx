import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  CurrencyDollarIcon,
  MagnifyingGlassIcon,
  PhoneIcon,
  EnvelopeIcon,
  ExclamationTriangleIcon,
  DocumentTextIcon,
  ChartBarIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface ARAccount {
  id: number;
  accountNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  patientEmail?: string;
  patientPhone?: string;
  totalBalance: number;
  currentBalance: number;
  balance30Days: number;
  balance60Days: number;
  balance90Days: number;
  balance120PlusDays: number;
  lastPaymentDate?: string;
  lastPaymentAmount?: number;
  lastStatementDate?: string;
  paymentPlanActive: boolean;
  paymentPlanAmount?: number;
  paymentPlanFrequency?: "weekly" | "biweekly" | "monthly";
  collectionStatus:
    | "current"
    | "delinquent"
    | "payment-plan"
    | "collections"
    | "write-off"
    | "hardship";
  notes?: string;
  lastContactDate?: string;
  lastContactMethod?: string;
  insuranceBalance: number;
  selfPayBalance: number;
}

interface PaymentRecord {
  id: number;
  accountId: number;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber?: string;
  appliedTo: string;
  receivedBy: string;
}

const collectionStatusConfig = {
  current: { label: "Current", color: "bg-green-100 text-green-800" },
  delinquent: { label: "Delinquent", color: "bg-yellow-100 text-yellow-800" },
  "payment-plan": { label: "Payment Plan", color: "bg-blue-100 text-blue-800" },
  collections: { label: "Collections", color: "bg-red-100 text-red-800" },
  "write-off": { label: "Write-Off", color: "bg-gray-100 text-gray-800" },
  hardship: { label: "Hardship", color: "bg-purple-100 text-purple-800" },
};

export function AccountsReceivable() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    ARAccount["collectionStatus"] | "all"
  >("all");
  const [agingFilter, setAgingFilter] = useState<
    "all" | "0-30" | "31-60" | "61-90" | "90+"
  >("all");
  const [selectedAccount, setSelectedAccount] = useState<ARAccount | null>(
    null,
  );
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: accounts = [], isLoading } = useQuery({
    queryKey: ["ar-accounts"],
    queryFn: () => api.get<ARAccount[]>("/api/financial/accounts-receivable"),
  });

  const { data: recentPayments = [] } = useQuery({
    queryKey: ["ar-recent-payments", selectedAccount?.id],
    queryFn: () =>
      api.get<PaymentRecord[]>(
        `/api/financial/accounts-receivable/${selectedAccount?.id}/payments`,
      ),
    enabled: !!selectedAccount,
  });

  const recordPaymentMutation = useMutation({
    mutationFn: (data: {
      accountId: number;
      amount: number;
      paymentMethod: string;
      referenceNumber?: string;
    }) => api.post("/api/financial/payments", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ar-accounts"] });
      setIsPaymentModalOpen(false);
    },
  });

  const sendStatementMutation = useMutation({
    mutationFn: (accountId: number) =>
      api.post(
        `/api/financial/accounts-receivable/${accountId}/send-statement`,
      ),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["ar-accounts"] }),
  });

  const filteredAccounts = accounts.filter((account) => {
    const matchesSearch =
      account.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      account.accountNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      account.patientMRN.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || account.collectionStatus === statusFilter;

    let matchesAging = true;
    if (agingFilter !== "all") {
      if (agingFilter === "0-30") matchesAging = account.currentBalance > 0;
      else if (agingFilter === "31-60")
        matchesAging = account.balance30Days > 0;
      else if (agingFilter === "61-90")
        matchesAging = account.balance60Days > 0;
      else if (agingFilter === "90+")
        matchesAging = account.balance90Days + account.balance120PlusDays > 0;
    }

    return matchesSearch && matchesStatus && matchesAging;
  });

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const totalAR = accounts.reduce((sum, a) => sum + a.totalBalance, 0);
  const currentAR = accounts.reduce((sum, a) => sum + a.currentBalance, 0);
  const over30 = accounts.reduce((sum, a) => sum + a.balance30Days, 0);
  const over60 = accounts.reduce((sum, a) => sum + a.balance60Days, 0);
  const over90 = accounts.reduce(
    (sum, a) => sum + a.balance90Days + a.balance120PlusDays,
    0,
  );
  const delinquentCount = accounts.filter(
    (a) =>
      a.collectionStatus === "delinquent" ||
      a.collectionStatus === "collections",
  ).length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Accounts Receivable
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage patient balances and collections
          </p>
        </div>
        <button className="inline-flex items-center rounded-md bg-white border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
          <ChartBarIcon className="mr-2 h-5 w-5" />
          AR Report
        </button>
      </div>

      {delinquentCount > 0 && (
        <div className="rounded-lg bg-yellow-50 border border-yellow-200 p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-5 w-5 text-yellow-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-yellow-800">
                Collection Attention Required
              </h3>
              <p className="text-sm text-yellow-700 mt-1">
                {delinquentCount} account(s) require follow-up. Total overdue:{" "}
                {formatCurrency(over60 + over90)}
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {formatCurrency(totalAR)}
          </div>
          <div className="text-sm text-gray-500">Total A/R</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {formatCurrency(currentAR)}
          </div>
          <div className="text-sm text-gray-500">Current (0-30)</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {formatCurrency(over30)}
          </div>
          <div className="text-sm text-gray-500">31-60 Days</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-orange-600">
            {formatCurrency(over60)}
          </div>
          <div className="text-sm text-gray-500">61-90 Days</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {formatCurrency(over90)}
          </div>
          <div className="text-sm text-gray-500">90+ Days</div>
        </div>
      </div>

      <div className="rounded-lg bg-white p-4 shadow">
        <h3 className="text-sm font-medium text-gray-700 mb-3">
          Aging Summary
        </h3>
        <div className="flex h-4 rounded-full overflow-hidden">
          {totalAR > 0 && (
            <>
              <div
                className="bg-green-500"
                style={{ width: `${(currentAR / totalAR) * 100}%` }}
                title={`Current: ${formatCurrency(currentAR)}`}
              />
              <div
                className="bg-yellow-500"
                style={{ width: `${(over30 / totalAR) * 100}%` }}
                title={`31-60: ${formatCurrency(over30)}`}
              />
              <div
                className="bg-orange-500"
                style={{ width: `${(over60 / totalAR) * 100}%` }}
                title={`61-90: ${formatCurrency(over60)}`}
              />
              <div
                className="bg-red-500"
                style={{ width: `${(over90 / totalAR) * 100}%` }}
                title={`90+: ${formatCurrency(over90)}`}
              />
            </>
          )}
        </div>
        <div className="flex justify-between mt-2 text-xs text-gray-500">
          <span>Current</span>
          <span>31-60</span>
          <span>61-90</span>
          <span>90+</span>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by name, account, or MRN..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(
              e.target.value as ARAccount["collectionStatus"] | "all",
            )
          }
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Status</option>
          {Object.entries(collectionStatusConfig).map(([key, value]) => (
            <option key={key} value={key}>
              {value.label}
            </option>
          ))}
        </select>
        <select
          value={agingFilter}
          onChange={(e) => setAgingFilter(e.target.value as typeof agingFilter)}
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Aging</option>
          <option value="0-30">0-30 Days</option>
          <option value="31-60">31-60 Days</option>
          <option value="61-90">61-90 Days</option>
          <option value="90+">90+ Days</option>
        </select>
      </div>

      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredAccounts.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <CurrencyDollarIcon className="h-12 w-12 mb-2" />
            <p>No accounts found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Total Balance
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Current
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  31-60
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  61-90
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  90+
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
              {filteredAccounts.map((account) => {
                const status = collectionStatusConfig[account.collectionStatus];
                const hasOverdue =
                  account.balance60Days +
                    account.balance90Days +
                    account.balance120PlusDays >
                  0;

                return (
                  <tr
                    key={account.id}
                    className={`hover:bg-gray-50 ${hasOverdue ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {account.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {account.patientMRN} | Acct:{" "}
                        {account.accountNumber}
                      </div>
                      {account.paymentPlanActive && (
                        <div className="text-xs text-blue-600 font-medium mt-1">
                          Payment Plan:{" "}
                          {formatCurrency(account.paymentPlanAmount || 0)}/
                          {account.paymentPlanFrequency}
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="text-sm font-bold text-gray-900">
                        {formatCurrency(account.totalBalance)}
                      </div>
                      <div className="text-xs text-gray-500">
                        Ins: {formatCurrency(account.insuranceBalance)} | Self:{" "}
                        {formatCurrency(account.selfPayBalance)}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {account.currentBalance > 0
                        ? formatCurrency(account.currentBalance)
                        : "-"}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-yellow-600">
                      {account.balance30Days > 0
                        ? formatCurrency(account.balance30Days)
                        : "-"}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-orange-600">
                      {account.balance60Days > 0
                        ? formatCurrency(account.balance60Days)
                        : "-"}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-red-600 font-medium">
                      {account.balance90Days + account.balance120PlusDays > 0
                        ? formatCurrency(
                            account.balance90Days + account.balance120PlusDays,
                          )
                        : "-"}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setSelectedAccount(account)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          View
                        </button>
                        <button
                          onClick={() => {
                            setSelectedAccount(account);
                            setIsPaymentModalOpen(true);
                          }}
                          className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                        >
                          Payment
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {selectedAccount && !isPaymentModalOpen && (
        <Dialog
          open={!!selectedAccount}
          onClose={() => setSelectedAccount(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
              <div className="border-b px-6 py-4">
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${collectionStatusConfig[selectedAccount.collectionStatus].color}`}
                  >
                    {
                      collectionStatusConfig[selectedAccount.collectionStatus]
                        .label
                    }
                  </span>
                </div>
                <Dialog.Title className="text-xl font-semibold text-gray-900 mt-2">
                  {selectedAccount.patientName}
                </Dialog.Title>
                <p className="text-sm text-gray-500">
                  Account: {selectedAccount.accountNumber} | MRN:{" "}
                  {selectedAccount.patientMRN}
                </p>
              </div>
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div className="bg-gray-50 rounded-lg p-4 text-center">
                    <div className="text-3xl font-bold text-gray-900">
                      {formatCurrency(selectedAccount.totalBalance)}
                    </div>
                    <div className="text-sm text-gray-500">Total Balance</div>
                  </div>
                  <div className="space-y-2">
                    {selectedAccount.patientPhone && (
                      <div className="flex items-center text-sm">
                        <PhoneIcon className="h-4 w-4 mr-2 text-gray-400" />
                        <a
                          href={`tel:${selectedAccount.patientPhone}`}
                          className="text-blue-600 hover:underline"
                        >
                          {selectedAccount.patientPhone}
                        </a>
                      </div>
                    )}
                    {selectedAccount.patientEmail && (
                      <div className="flex items-center text-sm">
                        <EnvelopeIcon className="h-4 w-4 mr-2 text-gray-400" />
                        <a
                          href={`mailto:${selectedAccount.patientEmail}`}
                          className="text-blue-600 hover:underline"
                        >
                          {selectedAccount.patientEmail}
                        </a>
                      </div>
                    )}
                  </div>
                </div>

                <div className="grid grid-cols-4 gap-4 text-center">
                  <div className="bg-green-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-green-700">
                      {formatCurrency(selectedAccount.currentBalance)}
                    </div>
                    <div className="text-xs text-green-600">Current</div>
                  </div>
                  <div className="bg-yellow-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-yellow-700">
                      {formatCurrency(selectedAccount.balance30Days)}
                    </div>
                    <div className="text-xs text-yellow-600">31-60 Days</div>
                  </div>
                  <div className="bg-orange-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-orange-700">
                      {formatCurrency(selectedAccount.balance60Days)}
                    </div>
                    <div className="text-xs text-orange-600">61-90 Days</div>
                  </div>
                  <div className="bg-red-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-red-700">
                      {formatCurrency(
                        selectedAccount.balance90Days +
                          selectedAccount.balance120PlusDays,
                      )}
                    </div>
                    <div className="text-xs text-red-600">90+ Days</div>
                  </div>
                </div>

                {selectedAccount.lastPaymentDate && (
                  <div className="text-sm text-gray-600">
                    Last Payment:{" "}
                    {formatCurrency(selectedAccount.lastPaymentAmount || 0)} on{" "}
                    {formatDate(selectedAccount.lastPaymentDate)}
                  </div>
                )}

                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-2">
                    Recent Payments
                  </h4>
                  {recentPayments.length > 0 ? (
                    <div className="space-y-2">
                      {recentPayments.slice(0, 5).map((payment) => (
                        <div
                          key={payment.id}
                          className="flex justify-between items-center text-sm bg-gray-50 p-2 rounded"
                        >
                          <div>
                            <span className="font-medium">
                              {formatCurrency(payment.amount)}
                            </span>
                            <span className="text-gray-500 ml-2">
                              {payment.paymentMethod}
                            </span>
                          </div>
                          <div className="text-gray-500">
                            {formatDate(payment.paymentDate)}
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-gray-500">No recent payments</p>
                  )}
                </div>
              </div>
              <div className="border-t px-6 py-4 flex justify-between">
                <button
                  onClick={() =>
                    sendStatementMutation.mutate(selectedAccount.id)
                  }
                  className="inline-flex items-center rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  <DocumentTextIcon className="h-4 w-4 mr-2" />
                  Send Statement
                </button>
                <div className="flex gap-3">
                  <button
                    onClick={() => setSelectedAccount(null)}
                    className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                  >
                    Close
                  </button>
                  <button
                    onClick={() => setIsPaymentModalOpen(true)}
                    className="rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
                  >
                    Record Payment
                  </button>
                </div>
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}

      {selectedAccount && isPaymentModalOpen && (
        <Dialog
          open={isPaymentModalOpen}
          onClose={() => setIsPaymentModalOpen(false)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-md w-full rounded-lg bg-white shadow-xl">
              <div className="border-b px-6 py-4">
                <Dialog.Title className="text-lg font-semibold text-gray-900">
                  Record Payment
                </Dialog.Title>
                <p className="text-sm text-gray-500">
                  {selectedAccount.patientName} - Balance:{" "}
                  {formatCurrency(selectedAccount.totalBalance)}
                </p>
              </div>
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  const formData = new FormData(e.currentTarget);
                  recordPaymentMutation.mutate({
                    accountId: selectedAccount.id,
                    amount: parseFloat(formData.get("amount") as string),
                    paymentMethod: formData.get("paymentMethod") as string,
                    referenceNumber:
                      (formData.get("referenceNumber") as string) || undefined,
                  });
                }}
                className="p-6 space-y-4"
              >
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Amount
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
                      className="w-full rounded-md border border-gray-300 py-2 pl-8 pr-4 focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Payment Method
                  </label>
                  <select
                    name="paymentMethod"
                    required
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="cash">Cash</option>
                    <option value="check">Check</option>
                    <option value="credit-card">Credit Card</option>
                    <option value="debit-card">Debit Card</option>
                    <option value="bank-transfer">Bank Transfer</option>
                    <option value="online-portal">Online Portal</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Reference Number (Optional)
                  </label>
                  <input
                    type="text"
                    name="referenceNumber"
                    placeholder="Check number, transaction ID, etc."
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div className="flex justify-end gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => setIsPaymentModalOpen(false)}
                    className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
                  >
                    Record Payment
                  </button>
                </div>
              </form>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}
    </div>
  );
}
