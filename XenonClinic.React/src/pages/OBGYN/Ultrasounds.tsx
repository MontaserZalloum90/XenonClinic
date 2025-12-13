import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  PlusIcon,
  XMarkIcon,
  PhotoIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";

interface Ultrasound {
  id: number;
  patientId: number;
  patientName: string;
  examDate: string;
  gestationalAge: number;
  examType:
    | "dating"
    | "anatomy"
    | "growth"
    | "biophysical"
    | "cervical"
    | "doppler";
  fetalHeartRate?: number;
  estimatedFetalWeight?: number;
  amnioticFluidIndex?: number;
  placentaLocation: string;
  presentation?: string;
  findings: string;
  impression: string;
  performedBy: string;
  status: "pending" | "completed" | "reviewed";
  images?: number;
  createdAt: string;
}

const examTypes = {
  dating: { label: "Dating Scan", color: "bg-blue-100 text-blue-800" },
  anatomy: { label: "Anatomy Scan", color: "bg-purple-100 text-purple-800" },
  growth: { label: "Growth Scan", color: "bg-green-100 text-green-800" },
  biophysical: {
    label: "Biophysical Profile",
    color: "bg-orange-100 text-orange-800",
  },
  cervical: { label: "Cervical Length", color: "bg-pink-100 text-pink-800" },
  doppler: { label: "Doppler Study", color: "bg-indigo-100 text-indigo-800" },
};

export const Ultrasounds = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedExam, setSelectedExam] = useState<Ultrasound | undefined>(
    undefined,
  );

  const { data: exams, isLoading } = useQuery<Ultrasound[]>({
    queryKey: ["ultrasounds"],
    queryFn: async () => {
      return [
        {
          id: 1,
          patientId: 7001,
          patientName: "Maria Garcia",
          examDate: new Date().toISOString(),
          gestationalAge: 20,
          examType: "anatomy" as const,
          fetalHeartRate: 148,
          estimatedFetalWeight: 350,
          amnioticFluidIndex: 14,
          placentaLocation: "Posterior",
          presentation: "Cephalic",
          findings:
            "Normal anatomy survey. Four chamber heart view normal. Brain structures normal. Spine intact.",
          impression: "Normal 20-week anatomy scan. No abnormalities detected.",
          performedBy: "Tech. Maria Santos",
          status: "reviewed" as const,
          images: 25,
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 7002,
          patientName: "Jennifer Wilson",
          examDate: new Date().toISOString(),
          gestationalAge: 32,
          examType: "growth" as const,
          fetalHeartRate: 145,
          estimatedFetalWeight: 1800,
          amnioticFluidIndex: 12,
          placentaLocation: "Anterior",
          presentation: "Cephalic",
          findings: "EFW 1800g, 50th percentile. Normal AFI.",
          impression: "Appropriate fetal growth for gestational age.",
          performedBy: "Tech. John Davis",
          status: "completed" as const,
          images: 15,
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 7003,
          patientName: "Lisa Anderson",
          examDate: new Date(
            Date.now() + 2 * 24 * 60 * 60 * 1000,
          ).toISOString(),
          gestationalAge: 12,
          examType: "dating" as const,
          placentaLocation: "TBD",
          findings: "",
          impression: "",
          performedBy: "Tech. Maria Santos",
          status: "pending" as const,
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredExams = exams?.filter((exam) => {
    const matchesSearch =
      !searchTerm ||
      exam.patientName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = !typeFilter || exam.examType === typeFilter;
    return matchesSearch && matchesType;
  });

  const handleCreate = () => {
    setSelectedExam(undefined);
    setIsModalOpen(true);
  };

  const handleView = (exam: Ultrasound) => {
    setSelectedExam(exam);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedExam(undefined);
  };

  const getExamTypeBadge = (type: keyof typeof examTypes) => {
    const config = examTypes[type] || {
      label: type,
      color: "bg-gray-100 text-gray-800",
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${config.color}`}
      >
        {config.label}
      </span>
    );
  };

  const getStatusBadge = (status: string) => {
    const config: Record<string, { className: string; label: string }> = {
      pending: { className: "bg-yellow-100 text-yellow-800", label: "Pending" },
      completed: { className: "bg-blue-100 text-blue-800", label: "Completed" },
      reviewed: { className: "bg-green-100 text-green-800", label: "Reviewed" },
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

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Ultrasound Exams</h1>
          <p className="text-gray-600 mt-1">
            Manage obstetric ultrasound examinations
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          Schedule Ultrasound
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <PhotoIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Exams</p>
              <p className="text-2xl font-bold text-gray-900">
                {exams?.length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-yellow-100 flex items-center justify-center">
              <span className="text-yellow-600 font-bold">
                {exams?.filter((e) => e.status === "pending").length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Pending</p>
              <p className="text-2xl font-bold text-yellow-600">
                {exams?.filter((e) => e.status === "pending").length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
              <span className="text-blue-600 font-bold">
                {exams?.filter((e) => e.status === "completed").length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">To Review</p>
              <p className="text-2xl font-bold text-blue-600">
                {exams?.filter((e) => e.status === "completed").length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">
                {exams?.filter((e) => e.status === "reviewed").length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Reviewed</p>
              <p className="text-2xl font-bold text-green-600">
                {exams?.filter((e) => e.status === "reviewed").length || 0}
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
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md"
          >
            <option value="">All Types</option>
            {Object.entries(examTypes).map(([key, value]) => (
              <option key={key} value={key}>
                {value.label}
              </option>
            ))}
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
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  GA
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Exam Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Images
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
              ) : filteredExams && filteredExams.length > 0 ? (
                filteredExams.map((exam) => (
                  <tr key={exam.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap font-medium text-gray-900">
                      {exam.patientName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(exam.examDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {exam.gestationalAge} weeks
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getExamTypeBadge(exam.examType)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(exam.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {exam.images || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <button
                        onClick={() => handleView(exam)}
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
                    No ultrasound exams found.
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
                  {selectedExam ? "Ultrasound Details" : "Schedule Ultrasound"}
                </Dialog.Title>
                <button
                  onClick={handleModalClose}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XMarkIcon className="h-6 w-6" />
                </button>
              </div>
              {selectedExam && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-gray-900">
                        {selectedExam.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Gestational Age
                      </label>
                      <p className="text-gray-900">
                        {selectedExam.gestationalAge} weeks
                      </p>
                    </div>
                    {selectedExam.fetalHeartRate && (
                      <div>
                        <label className="block text-sm font-medium text-gray-500">
                          Fetal Heart Rate
                        </label>
                        <p className="text-gray-900">
                          {selectedExam.fetalHeartRate} bpm
                        </p>
                      </div>
                    )}
                    {selectedExam.estimatedFetalWeight && (
                      <div>
                        <label className="block text-sm font-medium text-gray-500">
                          Estimated Fetal Weight
                        </label>
                        <p className="text-gray-900">
                          {selectedExam.estimatedFetalWeight}g
                        </p>
                      </div>
                    )}
                  </div>
                  {selectedExam.findings && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Findings
                      </label>
                      <p className="text-gray-700 mt-1">
                        {selectedExam.findings}
                      </p>
                    </div>
                  )}
                  {selectedExam.impression && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Impression
                      </label>
                      <p className="text-gray-700 mt-1">
                        {selectedExam.impression}
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
