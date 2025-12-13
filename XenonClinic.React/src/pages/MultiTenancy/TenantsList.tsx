import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  BuildingOffice2Icon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";
import type { Tenant, TenantStatus } from "../../types/multi-tenancy";

const statusConfig: Record<
  number,
  {
    label: string;
    color: string;
    icon: React.ComponentType<{ className?: string }>;
  }
> = {
  0: {
    label: "Active",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  1: {
    label: "Suspended",
    color: "bg-red-100 text-red-800",
    icon: XCircleIcon,
  },
  2: {
    label: "Pending Activation",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  3: {
    label: "Cancelled",
    color: "bg-gray-100 text-gray-800",
    icon: ExclamationTriangleIcon,
  },
};

const planConfig: Record<number, { label: string; color: string }> = {
  0: { label: "Trial", color: "bg-purple-100 text-purple-800" },
  1: { label: "Basic", color: "bg-blue-100 text-blue-800" },
  2: { label: "Professional", color: "bg-indigo-100 text-indigo-800" },
  3: { label: "Enterprise", color: "bg-amber-100 text-amber-800" },
};

export function TenantsList() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTenant, setSelectedTenant] = useState<Tenant | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<TenantStatus | "all">("all");
  const queryClient = useQueryClient();

  const { data: tenants = [], isLoading } = useQuery({
    queryKey: ["tenants"],
    queryFn: () => api.get<Tenant[]>("/api/multi-tenancy/tenants"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/multi-tenancy/tenants/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tenants"] });
    },
  });

  const filteredTenants = tenants.filter((tenant) => {
    const matchesSearch =
      tenant.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      tenant.subdomain.toLowerCase().includes(searchTerm.toLowerCase()) ||
      tenant.contactEmail.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || tenant.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleEdit = (tenant: Tenant) => {
    setSelectedTenant(tenant);
    setIsModalOpen(true);
  };

  const handleDelete = (tenant: Tenant) => {
    if (confirm(`Are you sure you want to delete tenant "${tenant.name}"?`)) {
      deleteMutation.mutate(tenant.id);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  const formatStorage = (used: number, quota: number) => {
    const percentage = (used / quota) * 100;
    return { used, quota, percentage };
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Tenants</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage tenant organizations and their subscriptions
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedTenant(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Add Tenant
        </button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-blue-100 p-3">
              <BuildingOffice2Icon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Total Tenants</p>
              <p className="text-2xl font-semibold text-gray-900">
                {tenants.length}
              </p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-green-100 p-3">
              <CheckCircleIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Active</p>
              <p className="text-2xl font-semibold text-gray-900">
                {tenants.filter((t) => t.status === 0).length}
              </p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-purple-100 p-3">
              <ClockIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Trial</p>
              <p className="text-2xl font-semibold text-gray-900">
                {tenants.filter((t) => t.plan === 0).length}
              </p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-amber-100 p-3">
              <BuildingOffice2Icon className="h-6 w-6 text-amber-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Enterprise</p>
              <p className="text-2xl font-semibold text-gray-900">
                {tenants.filter((t) => t.plan === 3).length}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search tenants..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2">
          <select
            value={statusFilter}
            onChange={(e) =>
              setStatusFilter(e.target.value as TenantStatus | "all")
            }
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">All Status</option>
            <option value={0}>Active</option>
            <option value={1}>Suspended</option>
            <option value={2}>Pending</option>
            <option value={3}>Cancelled</option>
          </select>
        </div>
      </div>

      {/* Tenants Table */}
      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredTenants.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <BuildingOffice2Icon className="h-12 w-12 mb-2" />
            <p>No tenants found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Tenant
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Plan
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Users / Companies
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Storage
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Subscription
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredTenants.map((tenant) => {
                const status = statusConfig[tenant.status];
                const plan = planConfig[tenant.plan];
                const storage = formatStorage(
                  tenant.usedStorageGB,
                  tenant.storageQuotaGB,
                );
                const StatusIcon = status.icon;

                return (
                  <tr key={tenant.id} className="hover:bg-gray-50">
                    <td className="whitespace-nowrap px-6 py-4">
                      <div>
                        <div className="font-medium text-gray-900">
                          {tenant.name}
                        </div>
                        <div className="text-sm text-gray-500">
                          {tenant.subdomain}.xenonclinic.com
                        </div>
                        <div className="text-xs text-gray-400">
                          {tenant.contactEmail}
                        </div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        <StatusIcon className="mr-1 h-3.5 w-3.5" />
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${plan.color}`}
                      >
                        {plan.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      <div>Max {tenant.maxUsers} users</div>
                      <div>Max {tenant.maxCompanies} companies</div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="w-32">
                        <div className="flex justify-between text-xs text-gray-500 mb-1">
                          <span>{storage.used} GB</span>
                          <span>{storage.quota} GB</span>
                        </div>
                        <div className="h-2 rounded-full bg-gray-200">
                          <div
                            className={`h-2 rounded-full ${
                              storage.percentage > 90
                                ? "bg-red-500"
                                : storage.percentage > 70
                                  ? "bg-yellow-500"
                                  : "bg-green-500"
                            }`}
                            style={{
                              width: `${Math.min(storage.percentage, 100)}%`,
                            }}
                          />
                        </div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      <div>{formatDate(tenant.subscriptionStart)}</div>
                      <div className="text-xs">
                        to {formatDate(tenant.subscriptionEnd)}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <button
                        onClick={() => handleEdit(tenant)}
                        className="mr-3 text-blue-600 hover:text-blue-900"
                      >
                        <PencilIcon className="h-5 w-5" />
                      </button>
                      <button
                        onClick={() => handleDelete(tenant)}
                        className="text-red-600 hover:text-red-900"
                      >
                        <TrashIcon className="h-5 w-5" />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Tenant Modal */}
      <TenantModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        tenant={selectedTenant}
      />
    </div>
  );
}

function TenantModal({
  isOpen,
  onClose,
  tenant,
}: {
  isOpen: boolean;
  onClose: () => void;
  tenant: Tenant | null;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    name: tenant?.name || "",
    subdomain: tenant?.subdomain || "",
    contactEmail: tenant?.contactEmail || "",
    contactPhone: tenant?.contactPhone || "",
    status: tenant?.status || 0,
    plan: tenant?.plan || 0,
    maxUsers: tenant?.maxUsers || 10,
    maxCompanies: tenant?.maxCompanies || 1,
    storageQuotaGB: tenant?.storageQuotaGB || 10,
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      tenant
        ? api.put(`/api/multi-tenancy/tenants/${tenant.id}`, data)
        : api.post("/api/multi-tenancy/tenants", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tenants"] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {tenant ? "Edit Tenant" : "Add New Tenant"}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700">
                  Organization Name
                </label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) =>
                    setFormData({ ...formData, name: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Subdomain
                </label>
                <div className="mt-1 flex rounded-md shadow-sm">
                  <input
                    type="text"
                    required
                    value={formData.subdomain}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        subdomain: e.target.value
                          .toLowerCase()
                          .replace(/[^a-z0-9-]/g, ""),
                      })
                    }
                    className="block w-full rounded-l-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                  <span className="inline-flex items-center rounded-r-md border border-l-0 border-gray-300 bg-gray-50 px-3 text-sm text-gray-500">
                    .xenonclinic.com
                  </span>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Status
                </label>
                <select
                  value={formData.status}
                  onChange={(e) =>
                    setFormData({ ...formData, status: Number(e.target.value) })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value={0}>Active</option>
                  <option value={1}>Suspended</option>
                  <option value={2}>Pending Activation</option>
                  <option value={3}>Cancelled</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contact Email
                </label>
                <input
                  type="email"
                  required
                  value={formData.contactEmail}
                  onChange={(e) =>
                    setFormData({ ...formData, contactEmail: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contact Phone
                </label>
                <input
                  type="tel"
                  value={formData.contactPhone}
                  onChange={(e) =>
                    setFormData({ ...formData, contactPhone: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Plan
                </label>
                <select
                  value={formData.plan}
                  onChange={(e) =>
                    setFormData({ ...formData, plan: Number(e.target.value) })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value={0}>Trial</option>
                  <option value={1}>Basic</option>
                  <option value={2}>Professional</option>
                  <option value={3}>Enterprise</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Max Users
                </label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.maxUsers}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      maxUsers: Number(e.target.value),
                    })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Max Companies
                </label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.maxCompanies}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      maxCompanies: Number(e.target.value),
                    })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Storage Quota (GB)
                </label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.storageQuotaGB}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      storageQuotaGB: Number(e.target.value),
                    })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={onClose}
                className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={mutation.isPending}
                className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {mutation.isPending
                  ? "Saving..."
                  : tenant
                    ? "Update"
                    : "Create"}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
