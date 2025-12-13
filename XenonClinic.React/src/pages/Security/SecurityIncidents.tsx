import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  ShieldExclamationIcon,
  PlusIcon,
  PencilIcon,
  MagnifyingGlassIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  ClockIcon,
  EyeIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";
import type {
  SecurityIncident,
  SecurityAlertSeverity,
} from "../../types/security";

const severityConfig: Record<
  number,
  { label: string; color: string; bgColor: string }
> = {
  0: { label: "Low", color: "text-blue-800", bgColor: "bg-blue-100" },
  1: { label: "Medium", color: "text-yellow-800", bgColor: "bg-yellow-100" },
  2: { label: "High", color: "text-orange-800", bgColor: "bg-orange-100" },
  3: { label: "Critical", color: "text-red-800", bgColor: "bg-red-100" },
};

const statusConfig = {
  open: {
    label: "Open",
    color: "bg-red-100 text-red-800",
    icon: ExclamationTriangleIcon,
  },
  investigating: {
    label: "Investigating",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  resolved: {
    label: "Resolved",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  closed: {
    label: "Closed",
    color: "bg-gray-100 text-gray-800",
    icon: CheckCircleIcon,
  },
};

export function SecurityIncidents() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedIncident, setSelectedIncident] =
    useState<SecurityIncident | null>(null);
  const [viewMode, setViewMode] = useState<"view" | "edit">("view");
  const [searchTerm, setSearchTerm] = useState("");
  const [severityFilter, setSeverityFilter] = useState<
    SecurityAlertSeverity | "all"
  >("all");
  const [statusFilter, setStatusFilter] = useState<
    SecurityIncident["status"] | "all"
  >("all");

  const { data: incidents = [], isLoading } = useQuery({
    queryKey: ["security-incidents"],
    queryFn: () => api.get<SecurityIncident[]>("/api/security/incidents"),
  });

  const filteredIncidents = incidents.filter((incident) => {
    const matchesSearch =
      incident.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      incident.incidentNumber
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      incident.description.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesSeverity =
      severityFilter === "all" || incident.severity === severityFilter;
    const matchesStatus =
      statusFilter === "all" || incident.status === statusFilter;
    return matchesSearch && matchesSeverity && matchesStatus;
  });

  const openIncidents = incidents.filter(
    (i) => i.status === "open" || i.status === "investigating",
  );
  const criticalIncidents = incidents.filter(
    (i) => i.severity === 3 && i.status !== "closed",
  );

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const handleView = (incident: SecurityIncident) => {
    setSelectedIncident(incident);
    setViewMode("view");
    setIsModalOpen(true);
  };

  const handleEdit = (incident: SecurityIncident) => {
    setSelectedIncident(incident);
    setViewMode("edit");
    setIsModalOpen(true);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Security Incidents
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Track and manage security incidents and breaches
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedIncident(null);
            setViewMode("edit");
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Report Incident
        </button>
      </div>

      {/* Alert Banner for Critical Incidents */}
      {criticalIncidents.length > 0 && (
        <div className="rounded-lg bg-red-50 border border-red-200 p-4">
          <div className="flex">
            <ExclamationTriangleIcon className="h-5 w-5 text-red-600 mr-3 mt-0.5" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                {criticalIncidents.length} Critical Incident
                {criticalIncidents.length !== 1 ? "s" : ""} Require Attention
              </h3>
              <div className="mt-2 text-sm text-red-700">
                <ul className="list-disc list-inside space-y-1">
                  {criticalIncidents.slice(0, 3).map((incident) => (
                    <li key={incident.id}>
                      {incident.incidentNumber}: {incident.title}
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-blue-100 p-3">
              <ShieldExclamationIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">
                Total Incidents
              </p>
              <p className="text-2xl font-semibold text-gray-900">
                {incidents.length}
              </p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-red-100 p-3">
              <ExclamationTriangleIcon className="h-6 w-6 text-red-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Open</p>
              <p className="text-2xl font-semibold text-gray-900">
                {openIncidents.length}
              </p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-orange-100 p-3">
              <ShieldExclamationIcon className="h-6 w-6 text-orange-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Critical</p>
              <p className="text-2xl font-semibold text-gray-900">
                {criticalIncidents.length}
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
              <p className="text-sm font-medium text-gray-500">
                Resolved (30d)
              </p>
              <p className="text-2xl font-semibold text-gray-900">
                {
                  incidents.filter(
                    (i) => i.status === "resolved" || i.status === "closed",
                  ).length
                }
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
            placeholder="Search incidents..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2">
          <select
            value={severityFilter}
            onChange={(e) =>
              setSeverityFilter(
                e.target.value === "all"
                  ? "all"
                  : (Number(e.target.value) as SecurityAlertSeverity),
              )
            }
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">All Severities</option>
            <option value={0}>Low</option>
            <option value={1}>Medium</option>
            <option value={2}>High</option>
            <option value={3}>Critical</option>
          </select>
          <select
            value={statusFilter}
            onChange={(e) =>
              setStatusFilter(
                e.target.value as SecurityIncident["status"] | "all",
              )
            }
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">All Status</option>
            <option value="open">Open</option>
            <option value="investigating">Investigating</option>
            <option value="resolved">Resolved</option>
            <option value="closed">Closed</option>
          </select>
        </div>
      </div>

      {/* Incidents Table */}
      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredIncidents.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ShieldExclamationIcon className="h-12 w-12 mb-2" />
            <p>No incidents found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Incident
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Severity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Detected
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Assigned To
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredIncidents.map((incident) => {
                const severity = severityConfig[incident.severity];
                const status = statusConfig[incident.status];
                const StatusIcon = status.icon;

                return (
                  <tr key={incident.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div>
                        <div className="font-medium text-gray-900">
                          {incident.title}
                        </div>
                        <div className="text-sm text-gray-500">
                          {incident.incidentNumber}
                        </div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${severity.bgColor} ${severity.color}`}
                      >
                        {severity.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        <StatusIcon className="mr-1 h-3.5 w-3.5" />
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {incident.type}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatDate(incident.detectedAt)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {incident.assignedTo || "-"}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <button
                        onClick={() => handleView(incident)}
                        className="mr-3 text-gray-600 hover:text-gray-900"
                      >
                        <EyeIcon className="h-5 w-5" />
                      </button>
                      <button
                        onClick={() => handleEdit(incident)}
                        className="text-blue-600 hover:text-blue-900"
                      >
                        <PencilIcon className="h-5 w-5" />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Incident Modal */}
      <IncidentModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        incident={selectedIncident}
        mode={viewMode}
      />
    </div>
  );
}

function IncidentModal({
  isOpen,
  onClose,
  incident,
  mode,
}: {
  isOpen: boolean;
  onClose: () => void;
  incident: SecurityIncident | null;
  mode: "view" | "edit";
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    title: incident?.title || "",
    description: incident?.description || "",
    severity: incident?.severity || 1,
    status: incident?.status || "open",
    type: incident?.type || "",
    affectedUsers: incident?.affectedUsers?.join(", ") || "",
    affectedSystems: incident?.affectedSystems?.join(", ") || "",
    assignedTo: incident?.assignedTo || "",
    resolution: incident?.resolution || "",
    preventiveMeasures: incident?.preventiveMeasures || "",
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) => {
      const payload = {
        ...data,
        affectedUsers: data.affectedUsers
          ? data.affectedUsers.split(",").map((s) => s.trim())
          : [],
        affectedSystems: data.affectedSystems
          ? data.affectedSystems.split(",").map((s) => s.trim())
          : [],
      };
      return incident
        ? api.put(`/api/security/incidents/${incident.id}`, payload)
        : api.post("/api/security/incidents", payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["security-incidents"] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  if (mode === "view" && incident) {
    const severity = severityConfig[incident.severity];
    const status = statusConfig[incident.status];

    return (
      <Dialog open={isOpen} onClose={onClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-start justify-between mb-4">
              <div>
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${severity.bgColor} ${severity.color}`}
                  >
                    {severity.label}
                  </span>
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                  >
                    {status.label}
                  </span>
                </div>
                <Dialog.Title className="text-lg font-semibold text-gray-900 mt-2">
                  {incident.title}
                </Dialog.Title>
                <p className="text-sm text-gray-500">
                  {incident.incidentNumber}
                </p>
              </div>
            </div>

            <div className="space-y-4">
              <div>
                <h4 className="text-sm font-medium text-gray-700">
                  Description
                </h4>
                <p className="mt-1 text-sm text-gray-900">
                  {incident.description}
                </p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <h4 className="text-sm font-medium text-gray-700">Type</h4>
                  <p className="mt-1 text-sm text-gray-900">{incident.type}</p>
                </div>
                <div>
                  <h4 className="text-sm font-medium text-gray-700">
                    Detected At
                  </h4>
                  <p className="mt-1 text-sm text-gray-900">
                    {new Date(incident.detectedAt).toLocaleString()}
                  </p>
                </div>
                <div>
                  <h4 className="text-sm font-medium text-gray-700">
                    Reported By
                  </h4>
                  <p className="mt-1 text-sm text-gray-900">
                    {incident.reportedBy}
                  </p>
                </div>
                <div>
                  <h4 className="text-sm font-medium text-gray-700">
                    Assigned To
                  </h4>
                  <p className="mt-1 text-sm text-gray-900">
                    {incident.assignedTo || "-"}
                  </p>
                </div>
              </div>

              {incident.affectedUsers && incident.affectedUsers.length > 0 && (
                <div>
                  <h4 className="text-sm font-medium text-gray-700">
                    Affected Users
                  </h4>
                  <div className="mt-1 flex flex-wrap gap-1">
                    {incident.affectedUsers.map((user, i) => (
                      <span
                        key={i}
                        className="inline-flex rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700"
                      >
                        {user}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {incident.affectedSystems &&
                incident.affectedSystems.length > 0 && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-700">
                      Affected Systems
                    </h4>
                    <div className="mt-1 flex flex-wrap gap-1">
                      {incident.affectedSystems.map((system, i) => (
                        <span
                          key={i}
                          className="inline-flex rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700"
                        >
                          {system}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

              {incident.resolution && (
                <div className="bg-green-50 p-3 rounded-md">
                  <h4 className="text-sm font-medium text-green-800">
                    Resolution
                  </h4>
                  <p className="mt-1 text-sm text-green-700">
                    {incident.resolution}
                  </p>
                </div>
              )}

              {incident.preventiveMeasures && (
                <div className="bg-blue-50 p-3 rounded-md">
                  <h4 className="text-sm font-medium text-blue-800">
                    Preventive Measures
                  </h4>
                  <p className="mt-1 text-sm text-blue-700">
                    {incident.preventiveMeasures}
                  </p>
                </div>
              )}
            </div>

            <div className="mt-6 flex justify-end">
              <button
                onClick={onClose}
                className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
              >
                Close
              </button>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    );
  }

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {incident ? "Update Incident" : "Report New Incident"}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Title
              </label>
              <input
                type="text"
                required
                value={formData.title}
                onChange={(e) =>
                  setFormData({ ...formData, title: e.target.value })
                }
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">
                Description
              </label>
              <textarea
                required
                rows={3}
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Severity
                </label>
                <select
                  value={formData.severity}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      severity: Number(e.target.value),
                    })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value={0}>Low</option>
                  <option value={1}>Medium</option>
                  <option value={2}>High</option>
                  <option value={3}>Critical</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Status
                </label>
                <select
                  value={formData.status}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      status: e.target.value as SecurityIncident["status"],
                    })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="open">Open</option>
                  <option value="investigating">Investigating</option>
                  <option value="resolved">Resolved</option>
                  <option value="closed">Closed</option>
                </select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Type
                </label>
                <input
                  type="text"
                  required
                  value={formData.type}
                  onChange={(e) =>
                    setFormData({ ...formData, type: e.target.value })
                  }
                  placeholder="e.g., Unauthorized Access"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Assigned To
                </label>
                <input
                  type="text"
                  value={formData.assignedTo}
                  onChange={(e) =>
                    setFormData({ ...formData, assignedTo: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">
                Affected Users (comma-separated)
              </label>
              <input
                type="text"
                value={formData.affectedUsers}
                onChange={(e) =>
                  setFormData({ ...formData, affectedUsers: e.target.value })
                }
                placeholder="user1, user2, user3"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">
                Affected Systems (comma-separated)
              </label>
              <input
                type="text"
                value={formData.affectedSystems}
                onChange={(e) =>
                  setFormData({ ...formData, affectedSystems: e.target.value })
                }
                placeholder="System A, System B"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            {(formData.status === "resolved" ||
              formData.status === "closed") && (
              <>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Resolution
                  </label>
                  <textarea
                    rows={2}
                    value={formData.resolution}
                    onChange={(e) =>
                      setFormData({ ...formData, resolution: e.target.value })
                    }
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Preventive Measures
                  </label>
                  <textarea
                    rows={2}
                    value={formData.preventiveMeasures}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        preventiveMeasures: e.target.value,
                      })
                    }
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
              </>
            )}

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
                  : incident
                    ? "Update"
                    : "Report"}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
