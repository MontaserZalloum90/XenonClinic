import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  PlusIcon,
  XMarkIcon,
  UserGroupIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { physiotherapyApi } from "../../lib/api";

interface TherapySession {
  id: number;
  patientId: number;
  patientName: string;
  sessionDate: string;
  sessionType: "initial" | "follow-up" | "discharge";
  treatmentArea: string;
  diagnosis: string;
  interventions: string[];
  duration: number;
  painLevelBefore: number;
  painLevelAfter?: number;
  progress: "improving" | "stable" | "declining";
  goals: string;
  homeExercises?: string;
  nextSessionDate?: string;
  therapist: string;
  status: "scheduled" | "completed" | "cancelled" | "no-show";
  createdAt: string;
}

interface TherapySessionsProps {
  patientId?: number;
}

export const TherapySessions = ({ patientId }: TherapySessionsProps = {}) => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedSession, setSelectedSession] = useState<
    TherapySession | undefined
  >(undefined);

  const { data: sessions, isLoading } = useQuery<TherapySession[]>({
    queryKey: ["therapy-sessions", patientId],
    queryFn: async () => {
      if (patientId) {
        const response = await physiotherapyApi.getSessionsByPatient(patientId);
        return response.data?.data ?? response.data ?? [];
      }
      const response = await physiotherapyApi.getAllSessions();
      return response.data?.data ?? response.data ?? [];
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: Record<string, unknown>) => physiotherapyApi.createSession(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["therapy-sessions"] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Record<string, unknown> }) =>
      physiotherapyApi.updateSession(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["therapy-sessions"] }),
  });

  void createMutation;
  void updateMutation;

  const filteredSessions = sessions?.filter((session) => {
    const matchesSearch =
      !searchTerm ||
      session.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      session.treatmentArea.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || session.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedSession(undefined);
    setIsModalOpen(true);
  };

  const handleView = (session: TherapySession) => {
    setSelectedSession(session);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedSession(undefined);
  };

  const getStatusBadge = (status: string) => {
    const config: Record<string, { className: string; label: string }> = {
      scheduled: { className: "bg-blue-100 text-blue-800", label: "Scheduled" },
      completed: {
        className: "bg-green-100 text-green-800",
        label: "Completed",
      },
      cancelled: { className: "bg-gray-100 text-gray-800", label: "Cancelled" },
      "no-show": { className: "bg-red-100 text-red-800", label: "No Show" },
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

  const getProgressBadge = (progress: string) => {
    const config: Record<string, { className: string; label: string }> = {
      improving: {
        className: "bg-green-100 text-green-800",
        label: "Improving",
      },
      stable: { className: "bg-yellow-100 text-yellow-800", label: "Stable" },
      declining: { className: "bg-red-100 text-red-800", label: "Declining" },
    };
    const c = config[progress] || {
      className: "bg-gray-100 text-gray-800",
      label: progress,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const scheduledCount =
    sessions?.filter((s) => s.status === "scheduled").length || 0;
  const completedToday =
    sessions?.filter(
      (s) =>
        s.status === "completed" &&
        new Date(s.sessionDate).toDateString() === new Date().toDateString(),
    ).length || 0;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Therapy Sessions</h1>
          <p className="text-gray-600 mt-1">
            Manage physiotherapy treatment sessions
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Session
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <UserGroupIcon className="h-8 w-8 text-blue-500" />
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
              <span className="text-blue-600 font-bold">{scheduledCount}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Scheduled</p>
              <p className="text-2xl font-bold text-blue-600">
                {scheduledCount}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">{completedToday}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Completed Today</p>
              <p className="text-2xl font-bold text-green-600">
                {completedToday}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">
                {sessions?.filter((s) => s.progress === "improving").length ||
                  0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Improving</p>
              <p className="text-2xl font-bold text-green-600">
                {sessions?.filter((s) => s.progress === "improving").length ||
                  0}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search by patient or treatment area..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md"
              />
            </div>
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md"
          >
            <option value="">All Status</option>
            <option value="scheduled">Scheduled</option>
            <option value="completed">Completed</option>
            <option value="cancelled">Cancelled</option>
            <option value="no-show">No Show</option>
          </select>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Date/Time
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Treatment Area
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Pain Level
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Progress
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                  </td>
                </tr>
              ) : filteredSessions && filteredSessions.length > 0 ? (
                filteredSessions.map((session) => (
                  <tr key={session.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap font-medium text-gray-900">
                      {session.patientName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(session.sessionDate), "MMM d, h:mm a")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {session.treatmentArea}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <span className="text-gray-600">
                        {session.painLevelBefore}/10
                      </span>
                      {session.painLevelAfter !== undefined && (
                        <span className="text-green-600 ml-1">
                          → {session.painLevelAfter}/10
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getProgressBadge(session.progress)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(session.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <button
                        onClick={() => handleView(session)}
                        className="text-primary-600 hover:text-primary-900"
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    No therapy sessions found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Dialog
        open={isModalOpen}
        onClose={handleModalClose}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-center mb-4">
                <Dialog.Title className="text-lg font-medium text-gray-900">
                  {selectedSession ? "Session Details" : "New Therapy Session"}
                </Dialog.Title>
                <button
                  onClick={handleModalClose}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XMarkIcon className="h-6 w-6" />
                </button>
              </div>
              {selectedSession && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-gray-900">
                        {selectedSession.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Therapist
                      </label>
                      <p className="text-gray-900">
                        {selectedSession.therapist}
                      </p>
                    </div>
                    <div className="col-span-2">
                      <label className="block text-sm font-medium text-gray-500">
                        Diagnosis
                      </label>
                      <p className="text-gray-900">
                        {selectedSession.diagnosis}
                      </p>
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Interventions
                    </label>
                    <div className="flex flex-wrap gap-2 mt-1">
                      {selectedSession.interventions.map((intervention, i) => (
                        <span
                          key={i}
                          className="px-2 py-1 bg-blue-50 text-blue-700 rounded text-sm"
                        >
                          {intervention}
                        </span>
                      ))}
                    </div>
                  </div>
                  {selectedSession.homeExercises && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Home Exercises
                      </label>
                      <p className="text-gray-700 mt-1">
                        {selectedSession.homeExercises}
                      </p>
                    </div>
                  )}
                </div>
              )}
              <div className="flex justify-end gap-3 pt-4 mt-4 border-t">
                <button onClick={handleModalClose} className="btn btn-outline">
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
