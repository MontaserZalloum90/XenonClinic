import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type {
  GlassesPrescription,
  CreateGlassesPrescriptionRequest,
  PrescriptionStatus,
} from "../../types/ophthalmology";
import { PrescriptionStatus as PrescriptionStatusEnum } from "../../types/ophthalmology";
import { Dialog } from "@headlessui/react";
import { format } from "date-fns";
import { PrinterIcon, PlusIcon } from "@heroicons/react/24/outline";

// Mock API - Replace with actual API when backend is ready
const prescriptionApi = {
  getAll: () => Promise.resolve({ data: [] as GlassesPrescription[] }),
  create: (data: CreateGlassesPrescriptionRequest) =>
    Promise.resolve({ data: { id: 1, ...data } }),
  update: (id: number, data: Partial<CreateGlassesPrescriptionRequest>) =>
    Promise.resolve({ data: { id, ...data } }),
  delete: () => Promise.resolve({ data: { success: true } }),
};

export const GlassesPrescriptions = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isPrintModalOpen, setIsPrintModalOpen] = useState(false);
  const [selectedPrescription, setSelectedPrescription] = useState<
    GlassesPrescription | undefined
  >(undefined);

  const handlePrint = () => {
    window.print();
  };

  const { data: prescriptions, isLoading } = useQuery<GlassesPrescription[]>({
    queryKey: ["glasses-prescriptions"],
    queryFn: async () => {
      const response = await prescriptionApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateGlassesPrescriptionRequest) =>
      prescriptionApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["glasses-prescriptions"] });
      queryClient.invalidateQueries({ queryKey: ["ophthalmology-stats"] });
      setIsModalOpen(false);
      setSelectedPrescription(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: Partial<CreateGlassesPrescriptionRequest>;
    }) => prescriptionApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["glasses-prescriptions"] });
      setIsModalOpen(false);
      setSelectedPrescription(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => prescriptionApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["glasses-prescriptions"] });
      queryClient.invalidateQueries({ queryKey: ["ophthalmology-stats"] });
    },
  });

  const handleDelete = (prescription: GlassesPrescription) => {
    if (window.confirm("Are you sure you want to delete this prescription?")) {
      deleteMutation.mutate(prescription.id);
    }
  };

  const handleEdit = (prescription: GlassesPrescription) => {
    setSelectedPrescription(prescription);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedPrescription(undefined);
    setIsModalOpen(true);
  };

  const handlePrintPrescription = (prescription: GlassesPrescription) => {
    setSelectedPrescription(prescription);
    setIsPrintModalOpen(true);
    // Small delay to ensure modal content is rendered before printing
    setTimeout(() => {
      handlePrint();
    }, 100);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedPrescription(undefined);
  };

  const handlePrintModalClose = () => {
    setIsPrintModalOpen(false);
    setSelectedPrescription(undefined);
  };

  const filteredPrescriptions = prescriptions?.filter(
    (prescription) =>
      prescription.patient?.fullNameEn
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      prescription.prescribedBy
        .toLowerCase()
        .includes(searchTerm.toLowerCase()),
  );

  const getStatusLabel = (status: PrescriptionStatus | undefined) => {
    if (status === undefined) return "Active";
    const statuses = ["Active", "Expired", "Cancelled"];
    return statuses[status] || "Unknown";
  };

  const getStatusColor = (status: PrescriptionStatus | undefined) => {
    if (status === undefined) return "text-green-600 bg-green-100";
    const colors = {
      0: "text-green-600 bg-green-100",
      1: "text-gray-600 bg-gray-100",
      2: "text-red-600 bg-red-100",
    };
    return colors[status as keyof typeof colors] || "text-gray-600 bg-gray-100";
  };

  const formatPrescription = (
    sphere: number,
    cylinder: number,
    axis: number,
    add?: number,
  ) => {
    const sphStr = `${sphere >= 0 ? "+" : ""}${sphere.toFixed(2)}`;
    const cylStr = `${cylinder >= 0 ? "+" : ""}${cylinder.toFixed(2)}`;
    const addStr = add ? ` ADD ${add >= 0 ? "+" : ""}${add.toFixed(2)}` : "";
    return `${sphStr} / ${cylStr} × ${axis}°${addStr}`;
  };

  const activePrescriptions =
    prescriptions?.filter(
      (p) =>
        p.status === undefined || p.status === PrescriptionStatusEnum.Active,
    ).length || 0;

  const expiringSoon =
    prescriptions?.filter((p) => {
      const expiryDate = new Date(p.expiryDate);
      const thirtyDaysFromNow = new Date();
      thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
      return expiryDate <= thirtyDaysFromNow && expiryDate > new Date();
    }).length || 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Glasses Prescriptions
          </h1>
          <p className="text-gray-600 mt-1">
            Manage eyeglass prescriptions and dispensing
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Prescription
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Prescriptions</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {prescriptions?.length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Active</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {activePrescriptions}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Expiring Soon</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">
            {expiringSoon}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">This Month</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">
            {prescriptions?.filter((p) => {
              const prescriptionDate = new Date(p.date);
              const now = new Date();
              return (
                prescriptionDate.getMonth() === now.getMonth() &&
                prescriptionDate.getFullYear() === now.getFullYear()
              );
            }).length || 0}
          </p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search by patient name or prescriber..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading prescriptions...</p>
          </div>
        ) : filteredPrescriptions && filteredPrescriptions.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Right Eye
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Left Eye
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    PD
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Expiry
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredPrescriptions.map((prescription) => (
                  <tr key={prescription.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {format(new Date(prescription.date), "MMM d, yyyy")}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {prescription.patient?.fullNameEn ||
                        `Patient #${prescription.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 font-mono text-xs">
                      {formatPrescription(
                        prescription.rightSphere,
                        prescription.rightCylinder,
                        prescription.rightAxis,
                        prescription.rightAdd,
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 font-mono text-xs">
                      {formatPrescription(
                        prescription.leftSphere,
                        prescription.leftCylinder,
                        prescription.leftAxis,
                        prescription.leftAdd,
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {prescription.pupillaryDistance} mm
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(prescription.expiryDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                          prescription.status,
                        )}`}
                      >
                        {getStatusLabel(prescription.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handlePrintPrescription(prescription)}
                          className="text-blue-600 hover:text-blue-800"
                        >
                          Print
                        </button>
                        <button
                          onClick={() => handleEdit(prescription)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(prescription)}
                          className="text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm
              ? "No prescriptions found matching your search."
              : "No prescriptions found."}
          </div>
        )}
      </div>

      {/* Edit/Create Modal */}
      <Dialog
        open={isModalOpen}
        onClose={handleModalClose}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedPrescription
                  ? "Edit Prescription"
                  : "New Prescription"}
              </Dialog.Title>
              <PrescriptionForm
                prescription={selectedPrescription}
                onSubmit={(data) => {
                  if (selectedPrescription) {
                    updateMutation.mutate({
                      id: selectedPrescription.id,
                      data,
                    });
                  } else {
                    createMutation.mutate(data);
                  }
                }}
                onCancel={handleModalClose}
                isSubmitting={
                  createMutation.isPending || updateMutation.isPending
                }
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* Print Modal */}
      <Dialog
        open={isPrintModalOpen}
        onClose={handlePrintModalClose}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl print:shadow-none">
            <div className="p-6 print:p-0">
              <div className="flex items-center justify-between mb-4 print:hidden">
                <Dialog.Title className="text-lg font-medium text-gray-900">
                  Print Prescription
                </Dialog.Title>
                <button
                  onClick={handlePrintModalClose}
                  className="text-gray-400 hover:text-gray-500"
                >
                  <span className="sr-only">Close</span>
                  <svg
                    className="h-6 w-6"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </div>
              <div>
                {selectedPrescription && (
                  <PrintablePrescription prescription={selectedPrescription} />
                )}
              </div>
              <div className="flex justify-end gap-3 mt-6 print:hidden">
                <button
                  onClick={handlePrintModalClose}
                  className="btn btn-secondary"
                >
                  Close
                </button>
                <button onClick={handlePrint} className="btn btn-primary">
                  <PrinterIcon className="h-5 w-5 mr-2 inline-block" />
                  Print
                </button>
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};

// Printable Prescription Component
const PrintablePrescription = ({
  prescription,
}: {
  prescription: GlassesPrescription;
}) => {
  const formatValue = (value: number) =>
    `${value >= 0 ? "+" : ""}${value.toFixed(2)}`;

  return (
    <div className="bg-white p-8 print:p-0">
      <div className="border-2 border-gray-800 p-8">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            EYEGLASS PRESCRIPTION
          </h1>
          <p className="text-sm text-gray-600 mt-2">
            XenonClinic Ophthalmology Department
          </p>
        </div>

        {/* Patient Info */}
        <div className="mb-6 pb-4 border-b-2 border-gray-300">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-gray-600">Patient Name:</p>
              <p className="text-lg font-semibold text-gray-900">
                {prescription.patient?.fullNameEn || "N/A"}
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Date of Birth:</p>
              <p className="text-lg font-semibold text-gray-900">
                {prescription.patient?.dateOfBirth
                  ? format(
                      new Date(prescription.patient.dateOfBirth),
                      "MMM d, yyyy",
                    )
                  : "N/A"}
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Prescription Date:</p>
              <p className="text-lg font-semibold text-gray-900">
                {format(new Date(prescription.date), "MMM d, yyyy")}
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Expiry Date:</p>
              <p className="text-lg font-semibold text-gray-900">
                {format(new Date(prescription.expiryDate), "MMM d, yyyy")}
              </p>
            </div>
          </div>
        </div>

        {/* Prescription Table */}
        <div className="mb-6">
          <table className="w-full border-2 border-gray-800">
            <thead>
              <tr className="bg-gray-100">
                <th className="border border-gray-800 px-4 py-3 text-left font-semibold">
                  Eye
                </th>
                <th className="border border-gray-800 px-4 py-3 text-center font-semibold">
                  Sphere (SPH)
                </th>
                <th className="border border-gray-800 px-4 py-3 text-center font-semibold">
                  Cylinder (CYL)
                </th>
                <th className="border border-gray-800 px-4 py-3 text-center font-semibold">
                  Axis
                </th>
                <th className="border border-gray-800 px-4 py-3 text-center font-semibold">
                  Add (Reading)
                </th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td className="border border-gray-800 px-4 py-3 font-semibold">
                  Right Eye (OD)
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {formatValue(prescription.rightSphere)}
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {formatValue(prescription.rightCylinder)}
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {prescription.rightAxis}°
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {prescription.rightAdd
                    ? formatValue(prescription.rightAdd)
                    : "-"}
                </td>
              </tr>
              <tr>
                <td className="border border-gray-800 px-4 py-3 font-semibold">
                  Left Eye (OS)
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {formatValue(prescription.leftSphere)}
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {formatValue(prescription.leftCylinder)}
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {prescription.leftAxis}°
                </td>
                <td className="border border-gray-800 px-4 py-3 text-center font-mono text-lg">
                  {prescription.leftAdd
                    ? formatValue(prescription.leftAdd)
                    : "-"}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        {/* PD */}
        <div className="mb-6">
          <p className="text-sm text-gray-600">Pupillary Distance (PD):</p>
          <p className="text-xl font-bold text-gray-900">
            {prescription.pupillaryDistance} mm
          </p>
        </div>

        {/* Notes */}
        {prescription.notes && (
          <div className="mb-6">
            <p className="text-sm text-gray-600 mb-1">Additional Notes:</p>
            <p className="text-gray-900">{prescription.notes}</p>
          </div>
        )}

        {/* Signature */}
        <div className="mt-8 pt-6 border-t-2 border-gray-300">
          <div className="flex justify-between items-end">
            <div>
              <p className="text-sm text-gray-600">Prescribed By:</p>
              <p className="text-lg font-semibold text-gray-900 mt-1">
                {prescription.prescribedBy}
              </p>
            </div>
            <div className="text-right">
              <div className="border-t-2 border-gray-800 w-48 mb-1"></div>
              <p className="text-sm text-gray-600">Signature</p>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="mt-6 text-center text-xs text-gray-500">
          <p>
            This prescription is valid until{" "}
            {format(new Date(prescription.expiryDate), "MMMM d, yyyy")}
          </p>
          <p className="mt-1">XenonClinic - Quality Eye Care</p>
        </div>
      </div>
    </div>
  );
};

// Form Component (similar to previous forms)
interface PrescriptionFormProps {
  prescription?: GlassesPrescription;
  onSubmit: (data: CreateGlassesPrescriptionRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const PrescriptionForm = ({
  prescription,
  onSubmit,
  onCancel,
  isSubmitting,
}: PrescriptionFormProps) => {
  const [formData, setFormData] = useState<CreateGlassesPrescriptionRequest>({
    patientId: prescription?.patientId || 0,
    date: prescription?.date
      ? format(new Date(prescription.date), "yyyy-MM-dd")
      : format(new Date(), "yyyy-MM-dd"),
    rightSphere: prescription?.rightSphere || 0,
    rightCylinder: prescription?.rightCylinder || 0,
    rightAxis: prescription?.rightAxis || 0,
    rightAdd: prescription?.rightAdd || undefined,
    leftSphere: prescription?.leftSphere || 0,
    leftCylinder: prescription?.leftCylinder || 0,
    leftAxis: prescription?.leftAxis || 0,
    leftAdd: prescription?.leftAdd || undefined,
    pupillaryDistance: prescription?.pupillaryDistance || 63,
    prescribedBy: prescription?.prescribedBy || "",
    expiryDate: prescription?.expiryDate
      ? format(new Date(prescription.expiryDate), "yyyy-MM-dd")
      : format(
          new Date(new Date().setFullYear(new Date().getFullYear() + 1)),
          "yyyy-MM-dd",
        ),
    notes: prescription?.notes || "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Patient ID
          </label>
          <input
            type="number"
            required
            value={formData.patientId || ""}
            onChange={(e) =>
              setFormData({
                ...formData,
                patientId: parseInt(e.target.value) || 0,
              })
            }
            className="input w-full"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Prescription Date
          </label>
          <input
            type="date"
            required
            value={formData.date}
            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
            className="input w-full"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Expiry Date
          </label>
          <input
            type="date"
            required
            value={formData.expiryDate}
            onChange={(e) =>
              setFormData({ ...formData, expiryDate: e.target.value })
            }
            className="input w-full"
          />
        </div>
      </div>

      <div className="border-t pt-4">
        <h3 className="text-md font-semibold text-gray-900 mb-3">
          Right Eye (OD)
        </h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Sphere
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.rightSphere}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  rightSphere: parseFloat(e.target.value) || 0,
                })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cylinder
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.rightCylinder}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  rightCylinder: parseFloat(e.target.value) || 0,
                })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Axis (0-180°)
            </label>
            <input
              type="number"
              min="0"
              max="180"
              required
              value={formData.rightAxis}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  rightAxis: parseInt(e.target.value) || 0,
                })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Add (Optional)
            </label>
            <input
              type="number"
              step="0.25"
              value={formData.rightAdd || ""}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  rightAdd: e.target.value
                    ? parseFloat(e.target.value)
                    : undefined,
                })
              }
              className="input w-full"
            />
          </div>
        </div>
      </div>

      <div className="border-t pt-4">
        <h3 className="text-md font-semibold text-gray-900 mb-3">
          Left Eye (OS)
        </h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Sphere
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.leftSphere}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  leftSphere: parseFloat(e.target.value) || 0,
                })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cylinder
            </label>
            <input
              type="number"
              step="0.25"
              required
              value={formData.leftCylinder}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  leftCylinder: parseFloat(e.target.value) || 0,
                })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Axis (0-180°)
            </label>
            <input
              type="number"
              min="0"
              max="180"
              required
              value={formData.leftAxis}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  leftAxis: parseInt(e.target.value) || 0,
                })
              }
              className="input w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Add (Optional)
            </label>
            <input
              type="number"
              step="0.25"
              value={formData.leftAdd || ""}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  leftAdd: e.target.value
                    ? parseFloat(e.target.value)
                    : undefined,
                })
              }
              className="input w-full"
            />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Pupillary Distance (mm)
          </label>
          <input
            type="number"
            step="0.5"
            min="50"
            max="80"
            required
            value={formData.pupillaryDistance}
            onChange={(e) =>
              setFormData({
                ...formData,
                pupillaryDistance: parseFloat(e.target.value) || 63,
              })
            }
            className="input w-full"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Prescribed By
          </label>
          <input
            type="text"
            required
            value={formData.prescribedBy}
            onChange={(e) =>
              setFormData({ ...formData, prescribedBy: e.target.value })
            }
            className="input w-full"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          rows={3}
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="input w-full"
          placeholder="Additional notes or recommendations..."
        />
      </div>

      <div className="flex justify-end gap-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          className="btn btn-secondary"
          disabled={isSubmitting}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting
            ? "Saving..."
            : prescription
              ? "Update Prescription"
              : "Create Prescription"}
        </button>
      </div>
    </form>
  );
};
