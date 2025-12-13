import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import { format } from "date-fns";
import type {
  MoleMapping,
  CreateMoleMappingRequest,
  MoleLocation,
  RiskLevel,
} from "../../types/dermatology";
import { moleMappingsApi } from "../../lib/api";

export const MoleMappings = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedMapping, setSelectedMapping] = useState<
    MoleMapping | undefined
  >(undefined);

  const { data: mappings, isLoading } = useQuery<MoleMapping[]>({
    queryKey: ["mole-mappings"],
    queryFn: async () => {
      const response = await moleMappingsApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateMoleMappingRequest) =>
      moleMappingsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["mole-mappings"] });
      setIsModalOpen(false);
      setSelectedMapping(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<MoleMapping> }) =>
      moleMappingsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["mole-mappings"] });
      setIsModalOpen(false);
      setSelectedMapping(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => moleMappingsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["mole-mappings"] });
    },
  });

  const handleDelete = (mapping: MoleMapping) => {
    if (window.confirm("Are you sure you want to delete this mole mapping?")) {
      deleteMutation.mutate(mapping.id);
    }
  };

  const handleEdit = (mapping: MoleMapping) => {
    setSelectedMapping(mapping);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedMapping(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedMapping(undefined);
  };

  const filteredMappings = mappings?.filter((mapping) =>
    mapping.patient?.fullNameEn
      .toLowerCase()
      .includes(searchTerm.toLowerCase()),
  );

  const getRiskLevelLabel = (risk?: RiskLevel) => {
    if (!risk) return "Not assessed";
    const labels: Record<RiskLevel, string> = {
      low: "Low Risk",
      moderate: "Moderate Risk",
      high: "High Risk",
    };
    return labels[risk];
  };

  const getRiskLevelColor = (risk?: RiskLevel) => {
    if (!risk) return "text-gray-600 bg-gray-100";
    const colors: Record<RiskLevel, string> = {
      low: "text-green-600 bg-green-100",
      moderate: "text-yellow-600 bg-yellow-100",
      high: "text-red-600 bg-red-100",
    };
    return colors[risk];
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mole Mapping</h1>
          <p className="text-gray-600 mt-1">
            Track and monitor mole locations and characteristics
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          New Mole Mapping
        </button>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search by patient name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading mole mappings...</p>
          </div>
        ) : filteredMappings && filteredMappings.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Map Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Total Moles
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Atypical Moles
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Risk Level
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Next Mapping
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Performed By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredMappings.map((mapping) => (
                  <tr key={mapping.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {format(new Date(mapping.mapDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      {mapping.patient?.fullNameEn ||
                        `Patient #${mapping.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      <span className="font-medium">{mapping.totalMoles}</span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {mapping.atypicalMoles ? (
                        <span className="font-medium text-orange-600">
                          {mapping.atypicalMoles}
                        </span>
                      ) : (
                        "0"
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getRiskLevelColor(
                          mapping.riskAssessment,
                        )}`}
                      >
                        {getRiskLevelLabel(mapping.riskAssessment)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {mapping.nextMappingDate
                        ? format(
                            new Date(mapping.nextMappingDate),
                            "MMM d, yyyy",
                          )
                        : "-"}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {mapping.performedBy}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(mapping)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          View
                        </button>
                        <button
                          onClick={() => handleDelete(mapping)}
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
              ? "No mole mappings found matching your search."
              : "No mole mappings found."}
          </div>
        )}
      </div>

      {/* Modal */}
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
                {selectedMapping
                  ? "View/Edit Mole Mapping"
                  : "New Mole Mapping"}
              </Dialog.Title>
              <MoleMappingForm
                mapping={selectedMapping}
                onSubmit={(data) => {
                  if (selectedMapping) {
                    updateMutation.mutate({ id: selectedMapping.id, data });
                  } else {
                    createMutation.mutate(data as CreateMoleMappingRequest);
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
    </div>
  );
};

// Form Component
interface MoleMappingFormProps {
  mapping?: MoleMapping;
  onSubmit: (data: Partial<MoleMapping>) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const MoleMappingForm = ({
  mapping,
  onSubmit,
  onCancel,
  isSubmitting,
}: MoleMappingFormProps) => {
  const [formData, setFormData] = useState({
    patientId: mapping?.patientId || 0,
    mapDate: mapping?.mapDate
      ? mapping.mapDate.split("T")[0]
      : new Date().toISOString().split("T")[0],
    totalMoles: mapping?.totalMoles || 0,
    atypicalMoles: mapping?.atypicalMoles || 0,
    riskAssessment: mapping?.riskAssessment || "",
    recommendations: mapping?.recommendations || "",
    nextMappingDate: mapping?.nextMappingDate
      ? mapping.nextMappingDate.split("T")[0]
      : "",
    performedBy: mapping?.performedBy || "",
    notes: mapping?.notes || "",
  });

  const [locations, setLocations] = useState<MoleLocation[]>(
    mapping?.locations || [
      { bodyArea: "", count: 0, hasAtypical: false, description: "" },
    ],
  );

  const handleAddLocation = () => {
    setLocations([
      ...locations,
      { bodyArea: "", count: 0, hasAtypical: false, description: "" },
    ]);
  };

  const handleRemoveLocation = (index: number) => {
    setLocations(locations.filter((_, i) => i !== index));
  };

  const handleLocationChange = (
    index: number,
    field: keyof MoleLocation,
    value: string | number | boolean,
  ) => {
    const newLocations = [...locations];
    newLocations[index] = { ...newLocations[index], [field]: value };
    setLocations(newLocations);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const validLocations = locations.filter(
      (loc) => loc.bodyArea && loc.count > 0,
    );

    onSubmit({
      ...formData,
      atypicalMoles: formData.atypicalMoles || undefined,
      locations: validLocations.length > 0 ? validLocations : undefined,
      riskAssessment: formData.riskAssessment || undefined,
      nextMappingDate: formData.nextMappingDate || undefined,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Patient ID</label>
          <input
            type="number"
            required
            value={formData.patientId || ""}
            onChange={(e) =>
              setFormData({ ...formData, patientId: parseInt(e.target.value) })
            }
            className="input w-full"
          />
        </div>
        <div>
          <label className="label">Map Date</label>
          <input
            type="date"
            required
            value={formData.mapDate}
            onChange={(e) =>
              setFormData({ ...formData, mapDate: e.target.value })
            }
            className="input w-full"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="label">Total Moles</label>
          <input
            type="number"
            required
            min="0"
            value={formData.totalMoles}
            onChange={(e) =>
              setFormData({ ...formData, totalMoles: parseInt(e.target.value) })
            }
            className="input w-full"
          />
        </div>
        <div>
          <label className="label">Atypical Moles</label>
          <input
            type="number"
            min="0"
            value={formData.atypicalMoles}
            onChange={(e) =>
              setFormData({
                ...formData,
                atypicalMoles: parseInt(e.target.value),
              })
            }
            className="input w-full"
          />
        </div>
        <div>
          <label className="label">Risk Assessment</label>
          <select
            value={formData.riskAssessment}
            onChange={(e) =>
              setFormData({ ...formData, riskAssessment: e.target.value })
            }
            className="input w-full"
          >
            <option value="">Select risk level</option>
            <option value="low">Low Risk</option>
            <option value="moderate">Moderate Risk</option>
            <option value="high">High Risk</option>
          </select>
        </div>
      </div>

      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="label">Mole Locations</label>
          <button
            type="button"
            onClick={handleAddLocation}
            className="text-sm text-primary-600 hover:text-primary-800"
          >
            + Add Location
          </button>
        </div>
        <div className="space-y-3 border border-gray-200 rounded-lg p-4">
          {locations.map((location, index) => (
            <div key={index} className="grid grid-cols-12 gap-2 items-start">
              <div className="col-span-4">
                <input
                  type="text"
                  placeholder="Body area"
                  value={location.bodyArea}
                  onChange={(e) =>
                    handleLocationChange(index, "bodyArea", e.target.value)
                  }
                  className="input w-full text-sm"
                />
              </div>
              <div className="col-span-2">
                <input
                  type="number"
                  placeholder="Count"
                  min="0"
                  value={location.count}
                  onChange={(e) =>
                    handleLocationChange(
                      index,
                      "count",
                      parseInt(e.target.value) || 0,
                    )
                  }
                  className="input w-full text-sm"
                />
              </div>
              <div className="col-span-2 flex items-center">
                <label className="flex items-center text-sm">
                  <input
                    type="checkbox"
                    checked={location.hasAtypical || false}
                    onChange={(e) =>
                      handleLocationChange(
                        index,
                        "hasAtypical",
                        e.target.checked,
                      )
                    }
                    className="mr-1"
                  />
                  Atypical
                </label>
              </div>
              <div className="col-span-3">
                <input
                  type="text"
                  placeholder="Description"
                  value={location.description || ""}
                  onChange={(e) =>
                    handleLocationChange(index, "description", e.target.value)
                  }
                  className="input w-full text-sm"
                />
              </div>
              <div className="col-span-1 flex items-center">
                <button
                  type="button"
                  onClick={() => handleRemoveLocation(index)}
                  className="text-red-600 hover:text-red-800 text-sm"
                >
                  Remove
                </button>
              </div>
            </div>
          ))}
          {locations.length === 0 && (
            <p className="text-sm text-gray-500 text-center py-2">
              No locations added
            </p>
          )}
        </div>
      </div>

      <div>
        <label className="label">Recommendations</label>
        <textarea
          value={formData.recommendations}
          onChange={(e) =>
            setFormData({ ...formData, recommendations: e.target.value })
          }
          className="input w-full"
          rows={3}
          placeholder="Clinical recommendations and follow-up instructions"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Next Mapping Date</label>
          <input
            type="date"
            value={formData.nextMappingDate}
            onChange={(e) =>
              setFormData({ ...formData, nextMappingDate: e.target.value })
            }
            className="input w-full"
          />
        </div>
        <div>
          <label className="label">Performed By</label>
          <input
            type="text"
            required
            value={formData.performedBy}
            onChange={(e) =>
              setFormData({ ...formData, performedBy: e.target.value })
            }
            className="input w-full"
            placeholder="Provider name"
          />
        </div>
      </div>

      <div>
        <label className="label">Additional Notes</label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Any additional notes or observations"
        />
      </div>

      <div className="flex items-center justify-end gap-3 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary">
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="btn btn-primary"
        >
          {isSubmitting
            ? "Saving..."
            : mapping
              ? "Update Mapping"
              : "Create Mapping"}
        </button>
      </div>
    </form>
  );
};
