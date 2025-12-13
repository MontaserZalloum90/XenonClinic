import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  BeakerIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type { ChemotherapyAdministration } from "../../types/oncology";
import { AdverseReactionSeverity } from "../../types/oncology";
import { oncologyChemotherapyApi } from "../../lib/api";

const SessionStatus = {
  Scheduled: "scheduled",
  InProgress: "in_progress",
  Completed: "completed",
  Cancelled: "cancelled",
} as const;

type SessionStatusType = (typeof SessionStatus)[keyof typeof SessionStatus];

interface ChemoSession extends ChemotherapyAdministration {
  status: SessionStatusType;
  protocolName: string;
}

export const ChemotherapySessions = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedSession, setSelectedSession] = useState<
    ChemoSession | undefined
  >(undefined);

  const { data: sessionsResponse, isLoading } = useQuery({
    queryKey: ["chemo-sessions"],
    queryFn: () => oncologyChemotherapyApi.getAll(),
  });

  const sessions: ChemoSession[] = sessionsResponse?.data || [];

  const filteredSessions = sessions?.filter((session) => {
    const matchesSearch =
      !searchTerm ||
      session.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      session.protocolName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || session.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedSession(undefined);
    setIsModalOpen(true);
  };

  const handleView = (session: ChemoSession) => {
    setSelectedSession(session);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedSession(undefined);
  };

  const getStatusBadge = (status: SessionStatusType) => {
    const config: Record<string, { className: string; label: string }> = {
      [SessionStatus.Scheduled]: {
        className: "bg-blue-100 text-blue-800",
        label: "Scheduled",
      },
      [SessionStatus.InProgress]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "In Progress",
      },
      [SessionStatus.Completed]: {
        className: "bg-green-100 text-green-800",
        label: "Completed",
      },
      [SessionStatus.Cancelled]: {
        className: "bg-red-100 text-red-800",
        label: "Cancelled",
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

  const sessionsWithReactions =
    sessions?.filter((s) => s.adverseReactions && s.adverseReactions.length > 0)
      .length || 0;
  const todaysSessions =
    sessions?.filter((s) => {
      const today = new Date().toDateString();
      return new Date(s.administeredDate).toDateString() === today;
    }).length || 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Chemotherapy Sessions
          </h1>
          <p className="text-gray-600 mt-1">
            Schedule and track chemotherapy administration
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          Schedule Session
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <BeakerIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Sessions</p>
              <p className="text-2xl font-bold text-gray-900">
                {sessions?.length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
              <span className="text-blue-600 font-bold">{todaysSessions}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Today's Sessions</p>
              <p className="text-2xl font-bold text-blue-600">
                {todaysSessions}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-yellow-100 flex items-center justify-center">
              <span className="text-yellow-600 font-bold">
                {sessions?.filter((s) => s.status === SessionStatus.Scheduled)
                  .length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Scheduled</p>
              <p className="text-2xl font-bold text-yellow-600">
                {sessions?.filter((s) => s.status === SessionStatus.Scheduled)
                  .length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-8 w-8 text-orange-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">With Reactions</p>
              <p className="text-2xl font-bold text-orange-600">
                {sessionsWithReactions}
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
                placeholder="Search by patient name or protocol..."
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
              {Object.entries(SessionStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Sessions Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Protocol
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Cycle
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Duration
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Reactions
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td
                    colSpan={8}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading sessions...</p>
                  </td>
                </tr>
              ) : filteredSessions && filteredSessions.length > 0 ? (
                filteredSessions.map((session) => (
                  <tr key={session.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {session.patientName || `Patient #${session.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="px-2 py-1 rounded bg-purple-100 text-purple-800 text-sm font-medium">
                        {session.protocolName}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      Cycle {session.cycleNumber}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(
                        new Date(session.administeredDate),
                        "MMM d, yyyy",
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {session.duration ? `${session.duration} min` : "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(session.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {session.adverseReactions &&
                      session.adverseReactions.length > 0 ? (
                        <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-orange-100 text-orange-800">
                          <ExclamationTriangleIcon className="h-3 w-3 mr-1" />
                          {session.adverseReactions.length}
                        </span>
                      ) : (
                        <span className="text-gray-400 text-sm">None</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleView(session)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {session.status === SessionStatus.Scheduled && (
                        <button className="text-green-600 hover:text-green-900">
                          Start
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td
                    colSpan={8}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    {searchTerm
                      ? "No sessions found matching your search."
                      : "No chemotherapy sessions found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <ChemoSessionModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        session={selectedSession}
      />
    </div>
  );
};

// Chemo Session Modal
interface ChemoSessionModalProps {
  isOpen: boolean;
  onClose: () => void;
  session?: ChemoSession;
}

const ChemoSessionModal = ({
  isOpen,
  onClose,
  session,
}: ChemoSessionModalProps) => {
  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {session
                  ? "Chemotherapy Session Details"
                  : "Schedule Chemotherapy Session"}
              </Dialog.Title>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            {session ? (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Patient
                    </label>
                    <p className="text-gray-900">{session.patientName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Protocol
                    </label>
                    <p className="text-gray-900">{session.protocolName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Cycle
                    </label>
                    <p className="text-gray-900">Cycle {session.cycleNumber}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Date
                    </label>
                    <p className="text-gray-900">
                      {format(
                        new Date(session.administeredDate),
                        "MMM d, yyyy",
                      )}
                    </p>
                  </div>
                </div>

                {session.premedications &&
                  session.premedications.length > 0 && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500 mb-2">
                        Premedications
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {session.premedications.map((med, i) => (
                          <span
                            key={i}
                            className="px-2 py-1 bg-blue-50 text-blue-700 rounded text-sm"
                          >
                            {med}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                {session.vitalSigns && (
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-2">
                      Vital Signs
                    </label>
                    <div className="grid grid-cols-4 gap-4 bg-gray-50 p-3 rounded">
                      <div>
                        <p className="text-xs text-gray-500">BP</p>
                        <p className="text-sm font-medium">
                          {session.vitalSigns.bloodPressure}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">HR</p>
                        <p className="text-sm font-medium">
                          {session.vitalSigns.heartRate} bpm
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Temp</p>
                        <p className="text-sm font-medium">
                          {session.vitalSigns.temperature}°C
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Weight</p>
                        <p className="text-sm font-medium">
                          {session.vitalSigns.weight} kg
                        </p>
                      </div>
                    </div>
                  </div>
                )}

                {session.adverseReactions &&
                  session.adverseReactions.length > 0 && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500 mb-2">
                        Adverse Reactions
                      </label>
                      <div className="space-y-2">
                        {session.adverseReactions.map((reaction, i) => (
                          <div
                            key={i}
                            className="bg-orange-50 border border-orange-200 p-3 rounded"
                          >
                            <div className="flex justify-between items-start">
                              <div>
                                <p className="font-medium text-orange-800">
                                  {reaction.reaction}
                                </p>
                                {reaction.treatment && (
                                  <p className="text-sm text-orange-600 mt-1">
                                    Treatment: {reaction.treatment}
                                  </p>
                                )}
                              </div>
                              <span
                                className={`px-2 py-0.5 rounded text-xs font-medium ${
                                  reaction.severity ===
                                  AdverseReactionSeverity.Mild
                                    ? "bg-yellow-100 text-yellow-800"
                                    : reaction.severity ===
                                        AdverseReactionSeverity.Moderate
                                      ? "bg-orange-100 text-orange-800"
                                      : "bg-red-100 text-red-800"
                                }`}
                              >
                                {reaction.severity}
                              </span>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                <div>
                  <label className="block text-sm font-medium text-gray-500">
                    Administered By
                  </label>
                  <p className="text-gray-900">{session.performedBy}</p>
                </div>
              </div>
            ) : (
              <p className="text-gray-500">Schedule form would go here</p>
            )}

            <div className="flex justify-end gap-3 pt-4 mt-4 border-t">
              <button onClick={onClose} className="btn btn-outline">
                Close
              </button>
            </div>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
