import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  PlusIcon,
  MagnifyingGlassIcon,
  ClipboardDocumentListIcon,
  PlayIcon,
  CheckCircleIcon,
  PauseIcon,
  ArrowPathIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { exerciseProgramsApi } from "../../lib/api";

interface ExerciseProgram {
  id: number;
  patientId: number;
  patientName: string;
  programName: string;
  startDate: string;
  endDate?: string;
  status: "active" | "completed" | "paused" | "modified";
  goal: string;
  exercises: Exercise[];
  frequency: string;
  duration: string;
  precautions?: string;
  prescribedBy: string;
  progressNotes?: string;
  compliance?: number;
}

interface Exercise {
  name: string;
  sets: number;
  reps: number;
  holdTime?: number;
  notes?: string;
}

const statusConfig = {
  active: {
    label: "Active",
    color: "bg-green-100 text-green-800",
    icon: PlayIcon,
  },
  completed: {
    label: "Completed",
    color: "bg-blue-100 text-blue-800",
    icon: CheckCircleIcon,
  },
  paused: {
    label: "Paused",
    color: "bg-yellow-100 text-yellow-800",
    icon: PauseIcon,
  },
  modified: {
    label: "Modified",
    color: "bg-purple-100 text-purple-800",
    icon: ArrowPathIcon,
  },
};

export function ExercisePrograms() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedProgram, setSelectedProgram] =
    useState<ExerciseProgram | null>(null);

  const { data: programs = [], isLoading } = useQuery({
    queryKey: ["exercise-programs", searchTerm, statusFilter],
    queryFn: async () => {
      const response = await exerciseProgramsApi.getAll();
      return response.data;
    },
  });

  const filteredPrograms = programs.filter((program: ExerciseProgram) => {
    const matchesSearch =
      program.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      program.programName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || program.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const stats = {
    active: programs.filter((p: ExerciseProgram) => p.status === "active")
      .length,
    completed: programs.filter((p: ExerciseProgram) => p.status === "completed")
      .length,
    paused: programs.filter((p: ExerciseProgram) => p.status === "paused")
      .length,
    avgCompliance: Math.round(
      programs.reduce(
        (sum: number, p: ExerciseProgram) => sum + (p.compliance || 0),
        0,
      ) / programs.length,
    ),
  };

  const handleViewDetails = (program: ExerciseProgram) => {
    setSelectedProgram(program);
    setIsModalOpen(true);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">
            Exercise Programs
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage home exercise programs and monitor patient compliance
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Program
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <PlayIcon className="h-6 w-6 text-green-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Active Programs
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.active}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <CheckCircleIcon className="h-6 w-6 text-blue-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Completed
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.completed}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <PauseIcon className="h-6 w-6 text-yellow-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Paused
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.paused}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <ClipboardDocumentListIcon className="h-6 w-6 text-indigo-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Avg Compliance
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.avgCompliance}%
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white shadow rounded-lg p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search by patient or program name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              />
            </div>
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Status</option>
            <option value="active">Active</option>
            <option value="completed">Completed</option>
            <option value="paused">Paused</option>
            <option value="modified">Modified</option>
          </select>
        </div>
      </div>

      {/* Programs List */}
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Program
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Duration
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Compliance
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredPrograms.map((program: ExerciseProgram) => {
              const StatusIcon = statusConfig[program.status].icon;
              return (
                <tr key={program.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {program.patientName}
                    </div>
                    <div className="text-sm text-gray-500">
                      ID: {program.patientId}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">
                      {program.programName}
                    </div>
                    <div className="text-sm text-gray-500">
                      {program.exercises.length} exercises | {program.frequency}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[program.status].color}`}
                    >
                      <StatusIcon className="h-3 w-3 mr-1" />
                      {statusConfig[program.status].label}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {program.duration}
                    </div>
                    <div className="text-sm text-gray-500">
                      Started:{" "}
                      {format(new Date(program.startDate), "MMM d, yyyy")}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {program.compliance !== undefined ? (
                      <div>
                        <div className="flex items-center">
                          <div className="w-16 bg-gray-200 rounded-full h-2 mr-2">
                            <div
                              className={`h-2 rounded-full ${
                                program.compliance >= 80
                                  ? "bg-green-500"
                                  : program.compliance >= 60
                                    ? "bg-yellow-500"
                                    : "bg-red-500"
                              }`}
                              style={{ width: `${program.compliance}%` }}
                            ></div>
                          </div>
                          <span className="text-sm text-gray-900">
                            {program.compliance}%
                          </span>
                        </div>
                      </div>
                    ) : (
                      <span className="text-sm text-gray-400">N/A</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleViewDetails(program)}
                      className="text-indigo-600 hover:text-indigo-900 mr-3"
                    >
                      View
                    </button>
                    <button className="text-gray-600 hover:text-gray-900">
                      Edit
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Detail Modal */}
      <Dialog
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {selectedProgram ? "Program Details" : "New Exercise Program"}
              </Dialog.Title>

              {selectedProgram && (
                <div className="mt-4 space-y-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Prescribed By
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.prescribedBy}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Program Name
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.programName}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Status
                      </label>
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[selectedProgram.status].color}`}
                      >
                        {statusConfig[selectedProgram.status].label}
                      </span>
                    </div>
                    <div className="col-span-2">
                      <label className="text-sm font-medium text-gray-500">
                        Goal
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.goal}
                      </p>
                    </div>
                  </div>

                  <div>
                    <h4 className="text-sm font-medium text-gray-900 mb-3">
                      Exercises
                    </h4>
                    <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                      {selectedProgram.exercises.map((exercise, index) => (
                        <div
                          key={index}
                          className="flex justify-between items-center bg-white p-3 rounded border"
                        >
                          <div>
                            <span className="font-medium text-gray-900">
                              {exercise.name}
                            </span>
                            {exercise.notes && (
                              <p className="text-xs text-gray-500">
                                {exercise.notes}
                              </p>
                            )}
                          </div>
                          <div className="text-sm text-gray-600">
                            {exercise.sets} sets x {exercise.reps} reps
                            {exercise.holdTime &&
                              ` (${exercise.holdTime}s hold)`}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Frequency
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.frequency}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Duration
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.duration}
                      </p>
                    </div>
                  </div>

                  {selectedProgram.precautions && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-yellow-800">
                        Precautions
                      </h4>
                      <p className="text-sm text-yellow-700 mt-1">
                        {selectedProgram.precautions}
                      </p>
                    </div>
                  )}

                  {selectedProgram.progressNotes && (
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Progress Notes
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedProgram.progressNotes}
                      </p>
                    </div>
                  )}
                </div>
              )}

              <div className="mt-6 flex justify-end gap-3">
                <button
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                >
                  Close
                </button>
                {selectedProgram && (
                  <button className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700">
                    Print Program
                  </button>
                )}
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
}
