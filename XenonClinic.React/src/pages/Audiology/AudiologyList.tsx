import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ChartBarIcon,
  DevicePhoneMobileIcon,
  ClipboardDocumentListIcon,
  DocumentTextIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  FunnelIcon,
} from "@heroicons/react/24/outline";
import { encounterApi, audiologyStatsApi } from "../../lib/api";
import { Modal } from "../../components/ui/Modal";
import { EncounterForm } from "../../components/audiology/EncounterForm";
import type {
  Encounter,
  AudiologyStatistics,
  CreateEncounterRequest,
  CreateEncounterTaskRequest,
} from "../../types/audiology";
import { EncounterStatus, EncounterType } from "../../types/audiology";
import { format } from "date-fns";

const getEncounterTypeLabel = (type: EncounterType): string => {
  const labels: Record<string, string> = {
    [EncounterType.InitialConsultation]: "Initial Consultation",
    [EncounterType.FollowUp]: "Follow-Up",
    [EncounterType.HearingTest]: "Hearing Test",
    [EncounterType.HearingAidFitting]: "HA Fitting",
    [EncounterType.HearingAidAdjustment]: "HA Adjustment",
    [EncounterType.HearingAidRepair]: "HA Repair",
    [EncounterType.Counseling]: "Counseling",
    [EncounterType.TinnitusEvaluation]: "Tinnitus Eval",
    [EncounterType.BalanceAssessment]: "Balance",
    [EncounterType.CochlearImplantMapping]: "CI Mapping",
  };
  return labels[type] || type;
};

const getStatusBadge = (status: EncounterStatus) => {
  const config: Record<string, { className: string; label: string }> = {
    [EncounterStatus.Scheduled]: {
      className: "bg-blue-100 text-blue-800",
      label: "Scheduled",
    },
    [EncounterStatus.CheckedIn]: {
      className: "bg-purple-100 text-purple-800",
      label: "Checked In",
    },
    [EncounterStatus.InProgress]: {
      className: "bg-yellow-100 text-yellow-800",
      label: "In Progress",
    },
    [EncounterStatus.Completed]: {
      className: "bg-green-100 text-green-800",
      label: "Completed",
    },
    [EncounterStatus.Cancelled]: {
      className: "bg-red-100 text-red-800",
      label: "Cancelled",
    },
    [EncounterStatus.NoShow]: {
      className: "bg-gray-100 text-gray-800",
      label: "No Show",
    },
  };
  const c = config[status] || {
    className: "bg-gray-100 text-gray-800",
    label: status,
  };
  return (
    <span
      className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
    >
      {c.label}
    </span>
  );
};

export const AudiologyList = () => {
  const [showNewEncounter, setShowNewEncounter] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [typeFilter, setTypeFilter] = useState<string>("");
  const [selectedPatientId, setSelectedPatientId] = useState<number | null>(
    null,
  );
  const queryClient = useQueryClient();

  // Fetch encounters
  const { data: encountersData, isLoading: encountersLoading } = useQuery({
    queryKey: ["encounters"],
    queryFn: () => encounterApi.getAll(),
  });

  // Fetch statistics
  const { data: statsData } = useQuery({
    queryKey: ["audiology-stats"],
    queryFn: () => audiologyStatsApi.getDashboard(),
  });

  const encounters: Encounter[] = encountersData?.data || [];
  const stats: AudiologyStatistics = statsData?.data || {
    totalPatients: 0,
    newPatientsThisMonth: 0,
    encountersToday: 0,
    encountersThisWeek: 0,
    encountersThisMonth: 0,
    hearingAidsFittedThisMonth: 0,
    activeHearingAids: 0,
    warrantyExpiringSoon: 0,
    audiogramsThisMonth: 0,
    pendingTasks: 0,
    overdueTasks: 0,
    encounterTypeDistribution: {},
    hearingLossGradeDistribution: {},
  };

  // Filter encounters
  const filteredEncounters = encounters.filter((enc) => {
    const matchesSearch =
      !searchTerm ||
      enc.patient?.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || enc.status === statusFilter;
    const matchesType = !typeFilter || enc.encounterType === typeFilter;
    return matchesSearch && matchesStatus && matchesType;
  });

  // Mutation for creating encounters
  const createEncounterMutation = useMutation({
    mutationFn: async ({
      data,
      tasks,
    }: {
      data: CreateEncounterRequest;
      tasks: CreateEncounterTaskRequest[];
    }) => {
      const response = await encounterApi.create(data);
      // Create tasks if any
      if (tasks.length > 0 && response.data?.id) {
        await Promise.all(
          tasks.map((task) => encounterApi.createTask(response.data.id, task)),
        );
      }
      return response;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["encounters"] });
      queryClient.invalidateQueries({ queryKey: ["audiology-stats"] });
      setShowNewEncounter(false);
      setSelectedPatientId(null);
    },
  });

  const handleNewEncounter = (
    data: CreateEncounterRequest,
    tasks: CreateEncounterTaskRequest[],
  ) => {
    if (!data.patientId || data.patientId === 0) {
      alert("Please select a patient before creating an encounter");
      return;
    }
    createEncounterMutation.mutate({ data, tasks });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Audiology</h1>
          <p className="text-gray-600">
            Manage encounters, audiograms, and hearing aids
          </p>
        </div>
        <button
          onClick={() => setShowNewEncounter(true)}
          className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Encounter
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg">
              <ClipboardDocumentListIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Today's Encounters</p>
              <p className="text-2xl font-bold text-gray-900">
                {stats.encountersToday}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 rounded-lg">
              <ChartBarIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Audiograms This Month</p>
              <p className="text-2xl font-bold text-gray-900">
                {stats.audiogramsThisMonth}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-purple-100 rounded-lg">
              <DevicePhoneMobileIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">HA Fittings This Month</p>
              <p className="text-2xl font-bold text-gray-900">
                {stats.hearingAidsFittedThisMonth}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-yellow-100 rounded-lg">
              <DocumentTextIcon className="h-6 w-6 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Pending Tasks</p>
              <p className="text-2xl font-bold text-gray-900">
                {stats.pendingTasks}
                {stats.overdueTasks > 0 && (
                  <span className="text-sm text-red-600 ml-2">
                    ({stats.overdueTasks} overdue)
                  </span>
                )}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search patients..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
              />
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Status</option>
              {Object.entries(EncounterStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Types</option>
              {Object.entries(EncounterType).map(([, value]) => (
                <option key={value} value={value}>
                  {getEncounterTypeLabel(value)}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Encounters Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Provider
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Chief Complaint
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {encountersLoading ? (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    Loading encounters...
                  </td>
                </tr>
              ) : filteredEncounters.length === 0 ? (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    No encounters found
                  </td>
                </tr>
              ) : (
                filteredEncounters.map((encounter) => (
                  <tr key={encounter.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {encounter.patient?.fullNameEn || "Unknown Patient"}
                      </div>
                      <div className="text-sm text-gray-500">
                        {encounter.patient?.phoneNumber}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {format(new Date(encounter.encounterDate), "MMM d, yyyy")}
                      {encounter.startTime && (
                        <span className="text-gray-500 ml-1">
                          {format(new Date(encounter.startTime), "h:mm a")}
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {getEncounterTypeLabel(encounter.encounterType)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {encounter.providerName || "Unassigned"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(encounter.status)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">
                      {encounter.chiefComplaint || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-primary-600 hover:text-primary-900 mr-3">
                        View
                      </button>
                      {encounter.status !== EncounterStatus.Completed && (
                        <button className="text-green-600 hover:text-green-900">
                          Continue
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* New Encounter Modal */}
      <Modal
        isOpen={showNewEncounter}
        onClose={() => {
          setShowNewEncounter(false);
          setSelectedPatientId(null);
        }}
        title="New Encounter"
        size="xl"
      >
        {!selectedPatientId ? (
          <div className="p-4">
            <p className="text-gray-600 mb-4">
              Search and select a patient to create an encounter:
            </p>
            <input
              type="text"
              placeholder="Search patient by name or ID..."
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
              onChange={(e) => {
                // In production, this would search patients via API
                const patientId = parseInt(e.target.value, 10);
                if (!isNaN(patientId) && patientId > 0) {
                  setSelectedPatientId(patientId);
                }
              }}
            />
            <p className="text-sm text-gray-500 mt-2">
              Enter patient ID to continue (patient search to be implemented)
            </p>
          </div>
        ) : (
          <EncounterForm
            patientId={selectedPatientId}
            onSubmit={handleNewEncounter}
            onCancel={() => {
              setShowNewEncounter(false);
              setSelectedPatientId(null);
            }}
          />
        )}
      </Modal>
    </div>
  );
};
