import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  PlusIcon,
  MagnifyingGlassIcon,
  SpeakerWaveIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationCircleIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import api from "../../lib/api";

interface AudiometryTest {
  id: number;
  patientId: number;
  patientName: string;
  testDate: string;
  testType: "pure_tone" | "speech" | "tympanometry" | "otoacoustic" | "abr";
  status: "scheduled" | "completed" | "reviewed";
  leftEar: HearingResult;
  rightEar: HearingResult;
  interpretation: string;
  hearingLossType?: "conductive" | "sensorineural" | "mixed" | "normal";
  recommendations?: string;
  performedBy: string;
  reviewedBy?: string;
  notes?: string;
}

interface HearingResult {
  pta?: number; // Pure Tone Average
  srt?: number; // Speech Reception Threshold
  wrs?: number; // Word Recognition Score
  frequencies?: { [key: string]: number }; // Hz to dB mapping
}

const testTypeConfig = {
  pure_tone: {
    label: "Pure Tone Audiometry",
    color: "bg-blue-100 text-blue-800",
  },
  speech: { label: "Speech Audiometry", color: "bg-green-100 text-green-800" },
  tympanometry: {
    label: "Tympanometry",
    color: "bg-purple-100 text-purple-800",
  },
  otoacoustic: { label: "OAE Testing", color: "bg-orange-100 text-orange-800" },
  abr: { label: "ABR Testing", color: "bg-red-100 text-red-800" },
};

const statusConfig = {
  scheduled: {
    label: "Scheduled",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  completed: {
    label: "Completed",
    color: "bg-blue-100 text-blue-800",
    icon: CheckCircleIcon,
  },
  reviewed: {
    label: "Reviewed",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
};

const hearingLossConfig = {
  normal: { label: "Normal Hearing", color: "bg-green-100 text-green-800" },
  conductive: {
    label: "Conductive Loss",
    color: "bg-yellow-100 text-yellow-800",
  },
  sensorineural: {
    label: "Sensorineural Loss",
    color: "bg-orange-100 text-orange-800",
  },
  mixed: { label: "Mixed Loss", color: "bg-red-100 text-red-800" },
};

const mockTests: AudiometryTest[] = [
  {
    id: 1,
    patientId: 101,
    patientName: "John Smith",
    testDate: "2024-01-22",
    testType: "pure_tone",
    status: "reviewed",
    leftEar: {
      pta: 35,
      frequencies: {
        "250": 30,
        "500": 35,
        "1000": 35,
        "2000": 40,
        "4000": 45,
        "8000": 50,
      },
    },
    rightEar: {
      pta: 25,
      frequencies: {
        "250": 20,
        "500": 25,
        "1000": 25,
        "2000": 30,
        "4000": 35,
        "8000": 40,
      },
    },
    interpretation:
      "Mild sensorineural hearing loss in left ear, slight high-frequency loss in right ear",
    hearingLossType: "sensorineural",
    recommendations:
      "Recommend hearing aid evaluation for left ear. Retest in 6 months.",
    performedBy: "Sarah Johnson, AuD",
    reviewedBy: "Dr. Michael Brown",
  },
  {
    id: 2,
    patientId: 102,
    patientName: "Mary Johnson",
    testDate: "2024-01-21",
    testType: "speech",
    status: "completed",
    leftEar: {
      srt: 30,
      wrs: 88,
    },
    rightEar: {
      srt: 25,
      wrs: 92,
    },
    interpretation:
      "Speech reception thresholds slightly elevated bilaterally. Word recognition scores good.",
    hearingLossType: "sensorineural",
    performedBy: "Sarah Johnson, AuD",
    notes: "Patient reports difficulty hearing in noisy environments.",
  },
  {
    id: 3,
    patientId: 103,
    patientName: "Robert Davis",
    testDate: "2024-01-23",
    testType: "tympanometry",
    status: "scheduled",
    leftEar: {},
    rightEar: {},
    interpretation: "Pending",
    performedBy: "Sarah Johnson, AuD",
    notes: "Follow-up for otitis media treatment",
  },
  {
    id: 4,
    patientId: 104,
    patientName: "Lisa Wilson",
    testDate: "2024-01-20",
    testType: "pure_tone",
    status: "reviewed",
    leftEar: {
      pta: 15,
      frequencies: {
        "250": 15,
        "500": 15,
        "1000": 15,
        "2000": 15,
        "4000": 20,
        "8000": 20,
      },
    },
    rightEar: {
      pta: 15,
      frequencies: {
        "250": 10,
        "500": 15,
        "1000": 15,
        "2000": 15,
        "4000": 15,
        "8000": 20,
      },
    },
    interpretation: "Hearing within normal limits bilaterally",
    hearingLossType: "normal",
    recommendations: "Routine follow-up in 1 year",
    performedBy: "Sarah Johnson, AuD",
    reviewedBy: "Dr. Michael Brown",
  },
  {
    id: 5,
    patientId: 105,
    patientName: "Thomas Brown",
    testDate: "2024-01-19",
    testType: "abr",
    status: "reviewed",
    leftEar: {
      pta: 55,
    },
    rightEar: {
      pta: 60,
    },
    interpretation:
      "ABR thresholds elevated bilaterally consistent with moderate sensorineural hearing loss",
    hearingLossType: "sensorineural",
    recommendations:
      "Binaural hearing aids recommended. Refer to hearing aid specialist.",
    performedBy: "Dr. Emily Chen",
    reviewedBy: "Dr. Michael Brown",
  },
];

export function AudiometryTests() {
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTest, setSelectedTest] = useState<AudiometryTest | null>(null);

  const { data: tests = mockTests, isLoading } = useQuery({
    queryKey: ["audiometry-tests", searchTerm, typeFilter, statusFilter],
    queryFn: async () => {
      const response = await api.get("/api/ent/audiometry-tests", {
        params: {
          search: searchTerm,
          type: typeFilter !== "all" ? typeFilter : undefined,
          status: statusFilter !== "all" ? statusFilter : undefined,
        },
      });
      return response.data;
    },
    placeholderData: mockTests,
  });

  const filteredTests = tests.filter((test: AudiometryTest) => {
    const matchesSearch = test.patientName
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesType = typeFilter === "all" || test.testType === typeFilter;
    const matchesStatus =
      statusFilter === "all" || test.status === statusFilter;
    return matchesSearch && matchesType && matchesStatus;
  });

  const stats = {
    scheduled: tests.filter((t: AudiometryTest) => t.status === "scheduled")
      .length,
    completed: tests.filter((t: AudiometryTest) => t.status === "completed")
      .length,
    reviewed: tests.filter((t: AudiometryTest) => t.status === "reviewed")
      .length,
    abnormal: tests.filter(
      (t: AudiometryTest) =>
        t.hearingLossType && t.hearingLossType !== "normal",
    ).length,
  };

  const handleViewDetails = (test: AudiometryTest) => {
    setSelectedTest(test);
    setIsModalOpen(true);
  };

  const getHearingLevelDescription = (pta: number | undefined): string => {
    if (pta === undefined) return "N/A";
    if (pta <= 25) return "Normal";
    if (pta <= 40) return "Mild Loss";
    if (pta <= 55) return "Moderate Loss";
    if (pta <= 70) return "Moderately Severe";
    if (pta <= 90) return "Severe Loss";
    return "Profound Loss";
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
            Audiometry Tests
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage hearing assessments and audiological evaluations
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Test
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <ClockIcon className="h-6 w-6 text-yellow-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Scheduled
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.scheduled}
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
                <SpeakerWaveIcon className="h-6 w-6 text-blue-600" />
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
                <CheckCircleIcon className="h-6 w-6 text-green-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Reviewed
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.reviewed}
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
                <ExclamationCircleIcon className="h-6 w-6 text-orange-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Abnormal Results
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.abnormal}
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
                placeholder="Search by patient name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              />
            </div>
          </div>
          <select
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Test Types</option>
            <option value="pure_tone">Pure Tone</option>
            <option value="speech">Speech</option>
            <option value="tympanometry">Tympanometry</option>
            <option value="otoacoustic">OAE</option>
            <option value="abr">ABR</option>
          </select>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Status</option>
            <option value="scheduled">Scheduled</option>
            <option value="completed">Completed</option>
            <option value="reviewed">Reviewed</option>
          </select>
        </div>
      </div>

      {/* Tests List */}
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Test Type
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Date
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Results
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredTests.map((test: AudiometryTest) => {
              const StatusIcon = statusConfig[test.status].icon;
              return (
                <tr key={test.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {test.patientName}
                    </div>
                    <div className="text-sm text-gray-500">
                      ID: {test.patientId}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${testTypeConfig[test.testType].color}`}
                    >
                      {testTypeConfig[test.testType].label}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {format(new Date(test.testDate), "MMM d, yyyy")}
                    </div>
                    <div className="text-sm text-gray-500">
                      {test.performedBy}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[test.status].color}`}
                    >
                      <StatusIcon className="h-3 w-3 mr-1" />
                      {statusConfig[test.status].label}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    {test.hearingLossType ? (
                      <div>
                        <span
                          className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${hearingLossConfig[test.hearingLossType].color}`}
                        >
                          {hearingLossConfig[test.hearingLossType].label}
                        </span>
                        <div className="mt-1 text-xs text-gray-500">
                          L: {getHearingLevelDescription(test.leftEar.pta)} | R:{" "}
                          {getHearingLevelDescription(test.rightEar.pta)}
                        </div>
                      </div>
                    ) : (
                      <span className="text-sm text-gray-400">Pending</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleViewDetails(test)}
                      className="text-indigo-600 hover:text-indigo-900 mr-3"
                    >
                      View
                    </button>
                    <button className="text-gray-600 hover:text-gray-900">
                      {test.status === "completed" ? "Review" : "Edit"}
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
                {selectedTest
                  ? "Audiometry Test Results"
                  : "New Audiometry Test"}
              </Dialog.Title>

              {selectedTest && (
                <div className="mt-4 space-y-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedTest.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Test Date
                      </label>
                      <p className="text-sm text-gray-900">
                        {format(
                          new Date(selectedTest.testDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Test Type
                      </label>
                      <span
                        className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${testTypeConfig[selectedTest.testType].color}`}
                      >
                        {testTypeConfig[selectedTest.testType].label}
                      </span>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Status
                      </label>
                      <span
                        className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[selectedTest.status].color}`}
                      >
                        {statusConfig[selectedTest.status].label}
                      </span>
                    </div>
                  </div>

                  {/* Hearing Results */}
                  <div className="grid grid-cols-2 gap-4">
                    <div className="bg-blue-50 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-blue-800 mb-3">
                        Left Ear
                      </h4>
                      {selectedTest.leftEar.pta !== undefined && (
                        <div className="mb-2">
                          <span className="text-xs text-gray-500">PTA:</span>
                          <span className="ml-2 font-medium">
                            {selectedTest.leftEar.pta} dB
                          </span>
                          <span className="ml-2 text-xs text-gray-600">
                            (
                            {getHearingLevelDescription(
                              selectedTest.leftEar.pta,
                            )}
                            )
                          </span>
                        </div>
                      )}
                      {selectedTest.leftEar.srt !== undefined && (
                        <div className="mb-2">
                          <span className="text-xs text-gray-500">SRT:</span>
                          <span className="ml-2 font-medium">
                            {selectedTest.leftEar.srt} dB
                          </span>
                        </div>
                      )}
                      {selectedTest.leftEar.wrs !== undefined && (
                        <div className="mb-2">
                          <span className="text-xs text-gray-500">WRS:</span>
                          <span className="ml-2 font-medium">
                            {selectedTest.leftEar.wrs}%
                          </span>
                        </div>
                      )}
                      {selectedTest.leftEar.frequencies && (
                        <div className="mt-3">
                          <span className="text-xs text-gray-500">
                            Frequencies (Hz → dB):
                          </span>
                          <div className="mt-1 text-xs">
                            {Object.entries(
                              selectedTest.leftEar.frequencies,
                            ).map(([hz, db]) => (
                              <span key={hz} className="mr-2">
                                {hz}: {db}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                    <div className="bg-red-50 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-red-800 mb-3">
                        Right Ear
                      </h4>
                      {selectedTest.rightEar.pta !== undefined && (
                        <div className="mb-2">
                          <span className="text-xs text-gray-500">PTA:</span>
                          <span className="ml-2 font-medium">
                            {selectedTest.rightEar.pta} dB
                          </span>
                          <span className="ml-2 text-xs text-gray-600">
                            (
                            {getHearingLevelDescription(
                              selectedTest.rightEar.pta,
                            )}
                            )
                          </span>
                        </div>
                      )}
                      {selectedTest.rightEar.srt !== undefined && (
                        <div className="mb-2">
                          <span className="text-xs text-gray-500">SRT:</span>
                          <span className="ml-2 font-medium">
                            {selectedTest.rightEar.srt} dB
                          </span>
                        </div>
                      )}
                      {selectedTest.rightEar.wrs !== undefined && (
                        <div className="mb-2">
                          <span className="text-xs text-gray-500">WRS:</span>
                          <span className="ml-2 font-medium">
                            {selectedTest.rightEar.wrs}%
                          </span>
                        </div>
                      )}
                      {selectedTest.rightEar.frequencies && (
                        <div className="mt-3">
                          <span className="text-xs text-gray-500">
                            Frequencies (Hz → dB):
                          </span>
                          <div className="mt-1 text-xs">
                            {Object.entries(
                              selectedTest.rightEar.frequencies,
                            ).map(([hz, db]) => (
                              <span key={hz} className="mr-2">
                                {hz}: {db}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  </div>

                  {selectedTest.hearingLossType && (
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Hearing Loss Classification
                      </label>
                      <div className="mt-1">
                        <span
                          className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${hearingLossConfig[selectedTest.hearingLossType].color}`}
                        >
                          {
                            hearingLossConfig[selectedTest.hearingLossType]
                              .label
                          }
                        </span>
                      </div>
                    </div>
                  )}

                  <div>
                    <label className="text-sm font-medium text-gray-500">
                      Interpretation
                    </label>
                    <p className="text-sm text-gray-900 mt-1">
                      {selectedTest.interpretation}
                    </p>
                  </div>

                  {selectedTest.recommendations && (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-green-800">
                        Recommendations
                      </h4>
                      <p className="text-sm text-green-700 mt-1">
                        {selectedTest.recommendations}
                      </p>
                    </div>
                  )}

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Performed By
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedTest.performedBy}
                      </p>
                    </div>
                    {selectedTest.reviewedBy && (
                      <div>
                        <label className="text-sm font-medium text-gray-500">
                          Reviewed By
                        </label>
                        <p className="text-sm text-gray-900">
                          {selectedTest.reviewedBy}
                        </p>
                      </div>
                    )}
                  </div>

                  {selectedTest.notes && (
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Notes
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedTest.notes}
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
                {selectedTest && (
                  <button className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700">
                    Print Report
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
