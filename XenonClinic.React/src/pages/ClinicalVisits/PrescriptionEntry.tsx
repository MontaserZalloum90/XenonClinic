import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  DocumentTextIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  TrashIcon,
  PencilIcon,
  PrinterIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";

interface Prescription {
  id: number;
  visitId: number;
  patientId: number;
  patientName: string;
  patientMRN: string;
  medications: PrescriptionMedication[];
  status: "draft" | "pending" | "dispensed" | "cancelled" | "expired";
  prescribedBy: string;
  prescribedAt: string;
  pharmacyNotes?: string;
  dispensedAt?: string;
  dispensedBy?: string;
  hasInteractions: boolean;
  hasAllergies: boolean;
}

interface PrescriptionMedication {
  id: number;
  medicationId: number;
  medicationName: string;
  genericName: string;
  strength: string;
  form: string;
  dosage: string;
  frequency: string;
  route: string;
  duration: string;
  quantity: number;
  refills: number;
  instructions?: string;
  isControlled: boolean;
}

const statusConfig = {
  draft: { label: "Draft", color: "bg-gray-100 text-gray-800" },
  pending: { label: "Pending", color: "bg-yellow-100 text-yellow-800" },
  dispensed: { label: "Dispensed", color: "bg-green-100 text-green-800" },
  cancelled: { label: "Cancelled", color: "bg-red-100 text-red-800" },
  expired: { label: "Expired", color: "bg-orange-100 text-orange-800" },
};

export function PrescriptionEntry() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPrescription, setSelectedPrescription] =
    useState<Prescription | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    Prescription["status"] | "all"
  >("all");
  const queryClient = useQueryClient();

  const { data: prescriptions = [], isLoading } = useQuery({
    queryKey: ["prescriptions"],
    queryFn: () => api.get<Prescription[]>("/api/clinical/prescriptions"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/clinical/prescriptions/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["prescriptions"] });
    },
  });

  const filteredPrescriptions = prescriptions.filter((rx) => {
    const matchesSearch =
      rx.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      rx.patientMRN.toLowerCase().includes(searchTerm.toLowerCase()) ||
      rx.medications.some((m) =>
        m.medicationName.toLowerCase().includes(searchTerm.toLowerCase()),
      );
    const matchesStatus = statusFilter === "all" || rx.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const handleEdit = (prescription: Prescription) => {
    setSelectedPrescription(prescription);
    setIsModalOpen(true);
  };

  const handleDelete = (prescription: Prescription) => {
    if (confirm("Delete this prescription?")) {
      deleteMutation.mutate(prescription.id);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Prescriptions</h1>
          <p className="mt-1 text-sm text-gray-500">
            Write and manage medication prescriptions
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedPrescription(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New Prescription
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {prescriptions.length}
          </div>
          <div className="text-sm text-gray-500">Total</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {prescriptions.filter((p) => p.status === "pending").length}
          </div>
          <div className="text-sm text-gray-500">Pending</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {prescriptions.filter((p) => p.status === "dispensed").length}
          </div>
          <div className="text-sm text-gray-500">Dispensed</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-orange-600">
            {prescriptions.filter((p) => p.hasInteractions).length}
          </div>
          <div className="text-sm text-gray-500">Interactions</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {prescriptions.filter((p) => p.hasAllergies).length}
          </div>
          <div className="text-sm text-gray-500">Allergy Alerts</div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search prescriptions..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2 flex-wrap">
          {(["all", "pending", "dispensed", "draft"] as const).map((status) => (
            <button
              key={status}
              onClick={() => setStatusFilter(status)}
              className={`rounded-md px-3 py-1.5 text-sm font-medium ${
                statusFilter === status
                  ? "bg-blue-600 text-white"
                  : "bg-gray-100 text-gray-700 hover:bg-gray-200"
              }`}
            >
              {status === "all" ? "All" : statusConfig[status].label}
            </button>
          ))}
        </div>
      </div>

      {/* Prescriptions List */}
      <div className="space-y-4">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredPrescriptions.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center rounded-lg bg-white shadow text-gray-500">
            <DocumentTextIcon className="h-12 w-12 mb-2" />
            <p>No prescriptions found</p>
          </div>
        ) : (
          filteredPrescriptions.map((rx) => (
            <div
              key={rx.id}
              className={`rounded-lg bg-white shadow ${
                rx.hasAllergies
                  ? "ring-2 ring-red-500"
                  : rx.hasInteractions
                    ? "ring-2 ring-yellow-500"
                    : ""
              }`}
            >
              <div className="p-4 sm:p-6">
                {/* Header Row */}
                <div className="flex items-center justify-between mb-4">
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                      <DocumentTextIcon className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900">
                        {rx.patientName}
                      </h3>
                      <p className="text-sm text-gray-500">
                        MRN: {rx.patientMRN}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    {rx.hasAllergies && (
                      <span className="inline-flex items-center rounded-full bg-red-100 px-2 py-1 text-xs font-medium text-red-800">
                        <ExclamationTriangleIcon className="h-3.5 w-3.5 mr-1" />
                        Allergy Alert
                      </span>
                    )}
                    {rx.hasInteractions && (
                      <span className="inline-flex items-center rounded-full bg-yellow-100 px-2 py-1 text-xs font-medium text-yellow-800">
                        <ExclamationTriangleIcon className="h-3.5 w-3.5 mr-1" />
                        Drug Interaction
                      </span>
                    )}
                    <span
                      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[rx.status].color}`}
                    >
                      {statusConfig[rx.status].label}
                    </span>
                  </div>
                </div>

                {/* Medications */}
                <div className="space-y-3">
                  {rx.medications.map((med) => (
                    <div
                      key={med.id}
                      className="flex items-start justify-between p-3 bg-gray-50 rounded-lg"
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <span className="font-medium text-gray-900">
                            {med.medicationName}
                          </span>
                          <span className="text-sm text-gray-500">
                            {med.strength}
                          </span>
                          {med.isControlled && (
                            <span className="inline-flex items-center rounded bg-red-100 px-1.5 py-0.5 text-xs font-medium text-red-800">
                              Controlled
                            </span>
                          )}
                        </div>
                        <p className="text-sm text-gray-600">
                          {med.genericName}
                        </p>
                        <div className="mt-1 text-sm text-gray-500">
                          <span>{med.dosage}</span>
                          <span className="mx-2">•</span>
                          <span>{med.frequency}</span>
                          <span className="mx-2">•</span>
                          <span>{med.route}</span>
                          <span className="mx-2">•</span>
                          <span>{med.duration}</span>
                        </div>
                        {med.instructions && (
                          <p className="mt-1 text-sm text-gray-600 italic">
                            {med.instructions}
                          </p>
                        )}
                      </div>
                      <div className="text-right text-sm">
                        <div className="text-gray-900">Qty: {med.quantity}</div>
                        <div className="text-gray-500">
                          Refills: {med.refills}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Footer */}
                <div className="mt-4 pt-4 border-t flex items-center justify-between">
                  <div className="text-sm text-gray-500">
                    <span>
                      Prescribed by {rx.prescribedBy} on{" "}
                      {formatDate(rx.prescribedAt)}
                    </span>
                    {rx.dispensedAt && (
                      <span className="ml-4">
                        • Dispensed on {formatDate(rx.dispensedAt)}
                      </span>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    <button className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50">
                      <PrinterIcon className="h-4 w-4 mr-1" />
                      Print
                    </button>
                    <button
                      onClick={() => handleEdit(rx)}
                      className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      <PencilIcon className="h-4 w-4 mr-1" />
                      Edit
                    </button>
                    {rx.status === "draft" && (
                      <button
                        onClick={() => handleDelete(rx)}
                        className="inline-flex items-center rounded-md border border-red-300 bg-white px-3 py-1.5 text-sm text-red-700 hover:bg-red-50"
                      >
                        <TrashIcon className="h-4 w-4 mr-1" />
                        Delete
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Prescription Modal */}
      <PrescriptionModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        prescription={selectedPrescription}
      />
    </div>
  );
}

function PrescriptionModal({
  isOpen,
  onClose,
  prescription,
}: {
  isOpen: boolean;
  onClose: () => void;
  prescription: Prescription | null;
}) {
  const queryClient = useQueryClient();
  const [medications, setMedications] = useState<
    Partial<PrescriptionMedication>[]
  >(prescription?.medications || [{}]);
  const [formData, setFormData] = useState({
    patientId: prescription?.patientId || 0,
    patientName: prescription?.patientName || "",
    patientMRN: prescription?.patientMRN || "",
    pharmacyNotes: prescription?.pharmacyNotes || "",
  });

  const mutation = useMutation({
    mutationFn: (data: {
      formData: typeof formData;
      medications: typeof medications;
    }) =>
      prescription
        ? api.put(`/api/clinical/prescriptions/${prescription.id}`, data)
        : api.post("/api/clinical/prescriptions", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["prescriptions"] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate({ formData, medications });
  };

  const addMedication = () => {
    setMedications([...medications, {}]);
  };

  const removeMedication = (index: number) => {
    setMedications(medications.filter((_, i) => i !== index));
  };

  const updateMedication = (
    index: number,
    field: string,
    value: string | number | boolean,
  ) => {
    const updated = [...medications];
    updated[index] = { ...updated[index], [field]: value };
    setMedications(updated);
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-3xl w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {prescription ? "Edit Prescription" : "New Prescription"}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Patient Info */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Patient Name
                </label>
                <input
                  type="text"
                  required
                  value={formData.patientName}
                  onChange={(e) =>
                    setFormData({ ...formData, patientName: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  MRN
                </label>
                <input
                  type="text"
                  required
                  value={formData.patientMRN}
                  onChange={(e) =>
                    setFormData({ ...formData, patientMRN: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                />
              </div>
            </div>

            {/* Medications */}
            <div className="border-t pt-4">
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-sm font-medium text-gray-700">
                  Medications
                </h4>
                <button
                  type="button"
                  onClick={addMedication}
                  className="inline-flex items-center rounded-md bg-blue-50 px-3 py-1 text-sm text-blue-700 hover:bg-blue-100"
                >
                  <PlusIcon className="h-4 w-4 mr-1" />
                  Add Medication
                </button>
              </div>

              <div className="space-y-4">
                {medications.map((med, index) => (
                  <div key={index} className="p-4 bg-gray-50 rounded-lg">
                    <div className="flex justify-between items-start mb-3">
                      <span className="text-sm font-medium text-gray-700">
                        Medication #{index + 1}
                      </span>
                      {medications.length > 1 && (
                        <button
                          type="button"
                          onClick={() => removeMedication(index)}
                          className="text-red-600 hover:text-red-800"
                        >
                          <TrashIcon className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <input
                          type="text"
                          placeholder="Medication Name"
                          required
                          value={med.medicationName || ""}
                          onChange={(e) =>
                            updateMedication(
                              index,
                              "medicationName",
                              e.target.value,
                            )
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div>
                        <input
                          type="text"
                          placeholder="Strength (e.g., 500mg)"
                          required
                          value={med.strength || ""}
                          onChange={(e) =>
                            updateMedication(index, "strength", e.target.value)
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div>
                        <input
                          type="text"
                          placeholder="Dosage (e.g., 1 tablet)"
                          required
                          value={med.dosage || ""}
                          onChange={(e) =>
                            updateMedication(index, "dosage", e.target.value)
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div>
                        <select
                          value={med.frequency || ""}
                          onChange={(e) =>
                            updateMedication(index, "frequency", e.target.value)
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        >
                          <option value="">Select Frequency</option>
                          <option value="Once daily">Once daily</option>
                          <option value="Twice daily">Twice daily</option>
                          <option value="Three times daily">
                            Three times daily
                          </option>
                          <option value="Four times daily">
                            Four times daily
                          </option>
                          <option value="Every 4 hours">Every 4 hours</option>
                          <option value="Every 6 hours">Every 6 hours</option>
                          <option value="Every 8 hours">Every 8 hours</option>
                          <option value="As needed">As needed (PRN)</option>
                          <option value="At bedtime">At bedtime</option>
                        </select>
                      </div>
                      <div>
                        <select
                          value={med.route || ""}
                          onChange={(e) =>
                            updateMedication(index, "route", e.target.value)
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        >
                          <option value="">Select Route</option>
                          <option value="Oral">Oral</option>
                          <option value="Sublingual">Sublingual</option>
                          <option value="Topical">Topical</option>
                          <option value="Inhalation">Inhalation</option>
                          <option value="Intramuscular">Intramuscular</option>
                          <option value="Intravenous">Intravenous</option>
                          <option value="Subcutaneous">Subcutaneous</option>
                          <option value="Rectal">Rectal</option>
                          <option value="Ophthalmic">Ophthalmic</option>
                          <option value="Otic">Otic</option>
                        </select>
                      </div>
                      <div>
                        <input
                          type="text"
                          placeholder="Duration (e.g., 7 days)"
                          required
                          value={med.duration || ""}
                          onChange={(e) =>
                            updateMedication(index, "duration", e.target.value)
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div>
                        <input
                          type="number"
                          placeholder="Quantity"
                          required
                          min="1"
                          value={med.quantity || ""}
                          onChange={(e) =>
                            updateMedication(
                              index,
                              "quantity",
                              Number(e.target.value),
                            )
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div>
                        <input
                          type="number"
                          placeholder="Refills"
                          min="0"
                          value={med.refills || 0}
                          onChange={(e) =>
                            updateMedication(
                              index,
                              "refills",
                              Number(e.target.value),
                            )
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div className="col-span-2">
                        <input
                          type="text"
                          placeholder="Special Instructions (optional)"
                          value={med.instructions || ""}
                          onChange={(e) =>
                            updateMedication(
                              index,
                              "instructions",
                              e.target.value,
                            )
                          }
                          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Pharmacy Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Pharmacy Notes
              </label>
              <textarea
                rows={2}
                value={formData.pharmacyNotes}
                onChange={(e) =>
                  setFormData({ ...formData, pharmacyNotes: e.target.value })
                }
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                placeholder="Special instructions for pharmacy..."
              />
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
                  : prescription
                    ? "Update"
                    : "Create Prescription"}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
