import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  PlusIcon,
  XMarkIcon,
  BeakerIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";

interface DialysisSession {
  id: number;
  patientId: number;
  patientName: string;
  sessionDate: string;
  dialysisType: "hemodialysis" | "peritoneal";
  accessType: "AVF" | "AVG" | "catheter" | "PD_catheter";
  machine?: string;
  duration: number;
  preWeight: number;
  postWeight: number;
  fluidRemoved: number;
  bloodFlow: number;
  dialysateFlow?: number;
  heparin?: number;
  preVitals: { bp: string; hr: number; temp: number };
  postVitals: { bp: string; hr: number; temp: number };
  intradialyticEvents?: string[];
  ktv?: number;
  urr?: number;
  performedBy: string;
  status: "scheduled" | "in-progress" | "completed" | "cancelled";
  notes?: string;
  createdAt: string;
}

export const DialysisSessions = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedSession, setSelectedSession] = useState<
    DialysisSession | undefined
  >(undefined);

  const { data: sessions, isLoading } = useQuery<DialysisSession[]>({
    queryKey: ["dialysis-sessions"],
    queryFn: async () => {
      return [
        {
          id: 1,
          patientId: 9001,
          patientName: "George Thompson",
          sessionDate: new Date().toISOString(),
          dialysisType: "hemodialysis" as const,
          accessType: "AVF" as const,
          machine: "Fresenius 5008S #3",
          duration: 240,
          preWeight: 78.5,
          postWeight: 75.2,
          fluidRemoved: 3.3,
          bloodFlow: 350,
          dialysateFlow: 500,
          heparin: 4000,
          preVitals: { bp: "158/92", hr: 82, temp: 36.5 },
          postVitals: { bp: "138/82", hr: 78, temp: 36.4 },
          ktv: 1.45,
          urr: 72,
          performedBy: "RN Maria Garcia",
          status: "completed" as const,
          notes: "Uneventful session",
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 9002,
          patientName: "Patricia Wilson",
          sessionDate: new Date().toISOString(),
          dialysisType: "hemodialysis" as const,
          accessType: "catheter" as const,
          machine: "Fresenius 5008S #5",
          duration: 180,
          preWeight: 65.2,
          postWeight: 63.0,
          fluidRemoved: 2.2,
          bloodFlow: 300,
          dialysateFlow: 500,
          heparin: 3000,
          preVitals: { bp: "145/88", hr: 76, temp: 36.6 },
          postVitals: { bp: "130/78", hr: 72, temp: 36.5 },
          intradialyticEvents: [
            "Mild cramping at 120 min - resolved with saline",
          ],
          performedBy: "RN John Davis",
          status: "completed" as const,
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 9003,
          patientName: "Robert Martinez",
          sessionDate: new Date(Date.now() + 4 * 60 * 60 * 1000).toISOString(),
          dialysisType: "hemodialysis" as const,
          accessType: "AVG" as const,
          machine: "Fresenius 5008S #1",
          duration: 240,
          preWeight: 82.0,
          postWeight: 82.0,
          fluidRemoved: 0,
          bloodFlow: 350,
          preVitals: { bp: "150/90", hr: 80, temp: 36.5 },
          postVitals: { bp: "150/90", hr: 80, temp: 36.5 },
          performedBy: "RN Maria Garcia",
          status: "scheduled" as const,
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredSessions = sessions?.filter((session) => {
    const matchesSearch =
      !searchTerm ||
      session.patientName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || session.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedSession(undefined);
    setIsModalOpen(true);
  };

  const handleView = (session: DialysisSession) => {
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
      "in-progress": {
        className: "bg-yellow-100 text-yellow-800",
        label: "In Progress",
      },
      completed: {
        className: "bg-green-100 text-green-800",
        label: "Completed",
      },
      cancelled: { className: "bg-red-100 text-red-800", label: "Cancelled" },
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

  const scheduledCount =
    sessions?.filter((s) => s.status === "scheduled").length || 0;
  const inProgressCount =
    sessions?.filter((s) => s.status === "in-progress").length || 0;
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
          <h1 className="text-2xl font-bold text-gray-900">
            Dialysis Sessions
          </h1>
          <p className="text-gray-600 mt-1">
            Manage hemodialysis and peritoneal dialysis treatments
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
            <div className="h-8 w-8 rounded-full bg-yellow-100 flex items-center justify-center">
              <span className="text-yellow-600 font-bold">
                {inProgressCount}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">In Progress</p>
              <p className="text-2xl font-bold text-yellow-600">
                {inProgressCount}
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
      </div>

      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search by patient name..."
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
            <option value="in-progress">In Progress</option>
            <option value="completed">Completed</option>
            <option value="cancelled">Cancelled</option>
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
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Access
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Duration
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  UF
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
                    colSpan={8}
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
                      {session.dialysisType === "hemodialysis" ? "HD" : "PD"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {session.accessType}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {session.duration} min
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {session.fluidRemoved} L
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
                    colSpan={8}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    No dialysis sessions found.
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
                  {selectedSession
                    ? "Dialysis Session Details"
                    : "New Dialysis Session"}
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
                        Machine
                      </label>
                      <p className="text-gray-900">
                        {selectedSession.machine || "-"}
                      </p>
                    </div>
                  </div>
                  <div className="grid grid-cols-4 gap-4 bg-gray-50 p-3 rounded">
                    <div>
                      <p className="text-xs text-gray-500">Pre-Weight</p>
                      <p className="font-medium">
                        {selectedSession.preWeight} kg
                      </p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Post-Weight</p>
                      <p className="font-medium">
                        {selectedSession.postWeight} kg
                      </p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">UF Goal</p>
                      <p className="font-medium">
                        {selectedSession.fluidRemoved} L
                      </p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Blood Flow</p>
                      <p className="font-medium">
                        {selectedSession.bloodFlow} mL/min
                      </p>
                    </div>
                  </div>
                  {selectedSession.ktv && (
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-500">
                          Kt/V
                        </label>
                        <p className="text-gray-900">{selectedSession.ktv}</p>
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-500">
                          URR
                        </label>
                        <p className="text-gray-900">{selectedSession.urr}%</p>
                      </div>
                    </div>
                  )}
                  {selectedSession.intradialyticEvents &&
                    selectedSession.intradialyticEvents.length > 0 && (
                      <div>
                        <label className="block text-sm font-medium text-gray-500">
                          Events
                        </label>
                        <ul className="mt-1 text-sm text-gray-700">
                          {selectedSession.intradialyticEvents.map(
                            (event, i) => (
                              <li key={i} className="text-orange-600">
                                • {event}
                              </li>
                            ),
                          )}
                        </ul>
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
