import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type {
  CardiacCatheterization,
  CreateCathRequest,
} from "../../types/cardiology";
import { CathProcedureType, AccessSite } from "../../types/cardiology";

export const CathLab = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [procedureFilter, setProcedureFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCath, setSelectedCath] = useState<
    CardiacCatheterization | undefined
  >(undefined);

  // Mock data - Replace with actual API calls
  const { data: procedures, isLoading } = useQuery<CardiacCatheterization[]>({
    queryKey: ["cath-procedures"],
    queryFn: async () => {
      // Mock implementation
      return [
        {
          id: 1,
          patientId: 1001,
          patientName: "John Smith",
          procedure: CathProcedureType.Diagnostic,
          date: new Date().toISOString(),
          accessSite: AccessSite.Radial,
          coronaryFindings: "Normal coronary arteries",
          findings: "No significant stenosis in LAD, LCx, or RCA. LMCA patent.",
          interventions: [],
          complications: [],
          contrast: 150,
          fluoroscopyTime: 8,
          conclusions: "Normal diagnostic coronary angiography",
          performedBy: "Dr. Johnson",
          assistedBy: "Dr. Williams",
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          patientId: 1002,
          patientName: "Mary Williams",
          procedure: CathProcedureType.PCI,
          date: new Date().toISOString(),
          accessSite: AccessSite.Femoral,
          coronaryFindings: "90% stenosis in proximal LAD",
          findings:
            "Severe stenosis in proximal LAD. LCx and RCA with mild disease.",
          interventions: ["Drug-eluting stent placement in LAD"],
          stentsPlaced: 1,
          complications: [],
          contrast: 220,
          fluoroscopyTime: 25,
          conclusions:
            "Successful PCI to LAD with drug-eluting stent. TIMI 3 flow achieved.",
          recommendations: "Dual antiplatelet therapy for 12 months",
          performedBy: "Dr. Brown",
          assistedBy: "Dr. Johnson",
          createdAt: new Date().toISOString(),
        },
        {
          id: 3,
          patientId: 1003,
          patientName: "Robert Davis",
          procedure: CathProcedureType.Stent,
          date: new Date().toISOString(),
          accessSite: AccessSite.Radial,
          coronaryFindings: "In-stent restenosis in RCA",
          findings: "Severe in-stent restenosis in mid RCA.",
          interventions: [
            "Balloon angioplasty",
            "Drug-eluting stent placement",
          ],
          stentsPlaced: 1,
          complications: ["Minor bleeding at access site"],
          contrast: 180,
          fluoroscopyTime: 18,
          conclusions:
            "Successful treatment of in-stent restenosis with additional stent.",
          recommendations: "Continue dual antiplatelet therapy",
          performedBy: "Dr. Williams",
          createdAt: new Date().toISOString(),
        },
      ];
    },
  });

  const filteredProcedures = procedures?.filter((proc) => {
    const matchesSearch =
      !searchTerm ||
      proc.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      proc.findings.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesProcedure =
      !procedureFilter || proc.procedure === procedureFilter;
    return matchesSearch && matchesProcedure;
  });

  const handleCreate = () => {
    setSelectedCath(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (cath: CardiacCatheterization) => {
    setSelectedCath(cath);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedCath(undefined);
  };

  const getProcedureBadge = (procedure: CathProcedureType) => {
    const config: Record<string, { className: string; label: string }> = {
      [CathProcedureType.Diagnostic]: {
        className: "bg-blue-100 text-blue-800",
        label: "Diagnostic",
      },
      [CathProcedureType.PCI]: {
        className: "bg-green-100 text-green-800",
        label: "PCI",
      },
      [CathProcedureType.Angioplasty]: {
        className: "bg-purple-100 text-purple-800",
        label: "Angioplasty",
      },
      [CathProcedureType.Stent]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Stent",
      },
      [CathProcedureType.Ablation]: {
        className: "bg-red-100 text-red-800",
        label: "Ablation",
      },
      [CathProcedureType.Biopsy]: {
        className: "bg-gray-100 text-gray-800",
        label: "Biopsy",
      },
    };
    const c = config[procedure] || {
      className: "bg-gray-100 text-gray-800",
      label: procedure,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const getAccessSiteLabel = (site: AccessSite): string => {
    const labels: Record<string, string> = {
      [AccessSite.Radial]: "Radial",
      [AccessSite.Femoral]: "Femoral",
      [AccessSite.Brachial]: "Brachial",
    };
    return labels[site] || site;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Catheterization Lab
          </h1>
          <p className="text-gray-600 mt-1">
            Manage cardiac catheterization procedures
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Procedure
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search by patient name or findings..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
              />
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={procedureFilter}
              onChange={(e) => setProcedureFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Procedures</option>
              {Object.entries(CathProcedureType).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Procedures Table */}
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
                  Procedure Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Access Site
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Findings
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Interventions
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Complications
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Performed By
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
                    colSpan={9}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading procedures...</p>
                  </td>
                </tr>
              ) : filteredProcedures && filteredProcedures.length > 0 ? (
                filteredProcedures.map((proc) => (
                  <tr key={proc.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {proc.patientName || `Patient #${proc.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(proc.date), "MMM d, yyyy")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getProcedureBadge(proc.procedure)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {proc.accessSite
                        ? getAccessSiteLabel(proc.accessSite)
                        : "-"}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {proc.coronaryFindings}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {proc.interventions && proc.interventions.length > 0 ? (
                        <div>
                          {proc.interventions.slice(0, 2).map((int, idx) => (
                            <div key={idx} className="text-xs">
                              {int}
                            </div>
                          ))}
                          {proc.interventions.length > 2 && (
                            <div className="text-xs text-gray-500">
                              +{proc.interventions.length - 2} more
                            </div>
                          )}
                        </div>
                      ) : (
                        <span className="text-gray-400">None</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      {proc.complications && proc.complications.length > 0 ? (
                        <div className="flex items-center text-red-600">
                          <ExclamationTriangleIcon className="h-4 w-4 mr-1" />
                          {proc.complications.length}
                        </div>
                      ) : (
                        <span className="text-green-600">None</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {proc.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleEdit(proc)}
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
                    colSpan={9}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    {searchTerm
                      ? "No procedures found matching your search."
                      : "No catheterization procedures found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <CathLabModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        cath={selectedCath}
      />
    </div>
  );
};

// Cath Lab Form Modal
interface CathLabModalProps {
  isOpen: boolean;
  onClose: () => void;
  cath?: CardiacCatheterization;
}

const CathLabModal = ({ isOpen, onClose, cath }: CathLabModalProps) => {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<Partial<CreateCathRequest>>({
    patientId: cath?.patientId || 0,
    procedure: cath?.procedure || CathProcedureType.Diagnostic,
    date: cath?.date || new Date().toISOString().split("T")[0],
    accessSite: cath?.accessSite || AccessSite.Radial,
    coronaryFindings: cath?.coronaryFindings || "",
    findings: cath?.findings || "",
    interventions: cath?.interventions || [],
    stentsPlaced: cath?.stentsPlaced || 0,
    complications: cath?.complications || [],
    contrast: cath?.contrast,
    fluoroscopyTime: cath?.fluoroscopyTime,
    conclusions: cath?.conclusions || "",
    recommendations: cath?.recommendations || "",
    performedBy: cath?.performedBy || "",
    assistedBy: cath?.assistedBy || "",
    notes: cath?.notes || "",
  });

  const [interventionInput, setInterventionInput] = useState("");
  const [complicationInput, setComplicationInput] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Saving cath procedure:", formData);
    queryClient.invalidateQueries({ queryKey: ["cath-procedures"] });
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >,
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: ["stentsPlaced", "contrast", "fluoroscopyTime"].includes(name)
        ? Number(value)
        : value,
    }));
  };

  const addIntervention = () => {
    if (interventionInput.trim()) {
      setFormData((prev) => ({
        ...prev,
        interventions: [
          ...(prev.interventions || []),
          interventionInput.trim(),
        ],
      }));
      setInterventionInput("");
    }
  };

  const removeIntervention = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      interventions: prev.interventions?.filter((_, i) => i !== index),
    }));
  };

  const addComplication = () => {
    if (complicationInput.trim()) {
      setFormData((prev) => ({
        ...prev,
        complications: [
          ...(prev.complications || []),
          complicationInput.trim(),
        ],
      }));
      setComplicationInput("");
    }
  };

  const removeComplication = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      complications: prev.complications?.filter((_, i) => i !== index),
    }));
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {cath
                  ? "View Catheterization Procedure"
                  : "New Catheterization Procedure"}
              </Dialog.Title>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Basic Info */}
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Patient ID
                  </label>
                  <input
                    type="number"
                    name="patientId"
                    value={formData.patientId}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Date
                  </label>
                  <input
                    type="date"
                    name="date"
                    value={formData.date?.split("T")[0]}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Procedure Type
                  </label>
                  <select
                    name="procedure"
                    value={formData.procedure}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(CathProcedureType).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Access Site
                  </label>
                  <select
                    name="accessSite"
                    value={formData.accessSite}
                    onChange={handleChange}
                    className="input w-full"
                  >
                    <option value="">Select...</option>
                    {Object.entries(AccessSite).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Performed By
                  </label>
                  <input
                    type="text"
                    name="performedBy"
                    value={formData.performedBy}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Assisted By
                  </label>
                  <input
                    type="text"
                    name="assistedBy"
                    value={formData.assistedBy}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>
              </div>

              {/* Procedure Details */}
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Stents Placed
                  </label>
                  <input
                    type="number"
                    name="stentsPlaced"
                    value={formData.stentsPlaced}
                    onChange={handleChange}
                    min="0"
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Contrast (ml)
                  </label>
                  <input
                    type="number"
                    name="contrast"
                    value={formData.contrast || ""}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Fluoroscopy Time (min)
                  </label>
                  <input
                    type="number"
                    name="fluoroscopyTime"
                    value={formData.fluoroscopyTime || ""}
                    onChange={handleChange}
                    className="input w-full"
                  />
                </div>
              </div>

              {/* Findings */}
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Coronary Findings
                  </label>
                  <textarea
                    name="coronaryFindings"
                    value={formData.coronaryFindings}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                    placeholder="Brief summary of coronary findings"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Detailed Findings
                  </label>
                  <textarea
                    name="findings"
                    value={formData.findings}
                    onChange={handleChange}
                    rows={3}
                    className="input w-full"
                    placeholder="Detailed findings for each vessel"
                    required
                  />
                </div>
              </div>

              {/* Interventions */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Interventions Performed
                </label>
                <div className="flex gap-2 mb-2">
                  <input
                    type="text"
                    value={interventionInput}
                    onChange={(e) => setInterventionInput(e.target.value)}
                    onKeyPress={(e) =>
                      e.key === "Enter" &&
                      (e.preventDefault(), addIntervention())
                    }
                    className="input flex-1"
                    placeholder="Add intervention..."
                  />
                  <button
                    type="button"
                    onClick={addIntervention}
                    className="btn btn-outline px-4"
                  >
                    Add
                  </button>
                </div>
                {formData.interventions &&
                  formData.interventions.length > 0 && (
                    <div className="space-y-1">
                      {formData.interventions.map((intervention, index) => (
                        <div
                          key={index}
                          className="flex items-center justify-between bg-gray-50 px-3 py-2 rounded"
                        >
                          <span className="text-sm text-gray-700">
                            {intervention}
                          </span>
                          <button
                            type="button"
                            onClick={() => removeIntervention(index)}
                            className="text-red-600 hover:text-red-800"
                          >
                            <XMarkIcon className="h-4 w-4" />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
              </div>

              {/* Complications */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Complications
                </label>
                <div className="flex gap-2 mb-2">
                  <input
                    type="text"
                    value={complicationInput}
                    onChange={(e) => setComplicationInput(e.target.value)}
                    onKeyPress={(e) =>
                      e.key === "Enter" &&
                      (e.preventDefault(), addComplication())
                    }
                    className="input flex-1"
                    placeholder="Add complication..."
                  />
                  <button
                    type="button"
                    onClick={addComplication}
                    className="btn btn-outline px-4"
                  >
                    Add
                  </button>
                </div>
                {formData.complications &&
                  formData.complications.length > 0 && (
                    <div className="space-y-1">
                      {formData.complications.map((complication, index) => (
                        <div
                          key={index}
                          className="flex items-center justify-between bg-red-50 px-3 py-2 rounded"
                        >
                          <span className="text-sm text-red-700">
                            {complication}
                          </span>
                          <button
                            type="button"
                            onClick={() => removeComplication(index)}
                            className="text-red-600 hover:text-red-800"
                          >
                            <XMarkIcon className="h-4 w-4" />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
              </div>

              {/* Conclusions & Recommendations */}
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Conclusions
                  </label>
                  <textarea
                    name="conclusions"
                    value={formData.conclusions}
                    onChange={handleChange}
                    rows={3}
                    className="input w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Recommendations
                  </label>
                  <textarea
                    name="recommendations"
                    value={formData.recommendations}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Notes
                  </label>
                  <textarea
                    name="notes"
                    value={formData.notes}
                    onChange={handleChange}
                    rows={2}
                    className="input w-full"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <button
                  type="button"
                  onClick={onClose}
                  className="btn btn-outline"
                >
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  {cath ? "Update" : "Create"} Procedure
                </button>
              </div>
            </form>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
