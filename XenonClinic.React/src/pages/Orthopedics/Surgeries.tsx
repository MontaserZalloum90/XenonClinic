import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { surgeriesApi } from "../../lib/api";
import type {
  OrthopedicSurgery,
  CreateOrthopedicSurgeryRequest,
} from "../../types/orthopedics";
import { useT } from "../../contexts/TenantContext";
import { Dialog } from "@headlessui/react";
import { format } from "date-fns";

export const Surgeries = () => {
  const t = useT();
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedSurgery, setSelectedSurgery] = useState<
    OrthopedicSurgery | undefined
  >(undefined);

  const { data: surgeries, isLoading } = useQuery<OrthopedicSurgery[]>({
    queryKey: ["orthopedic-surgeries"],
    queryFn: async () => {
      const response = await surgeriesApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateOrthopedicSurgeryRequest) =>
      surgeriesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orthopedic-surgeries"] });
      queryClient.invalidateQueries({ queryKey: ["orthopedics-stats"] });
      setIsModalOpen(false);
      setSelectedSurgery(undefined);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: Partial<CreateOrthopedicSurgeryRequest>;
    }) => surgeriesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orthopedic-surgeries"] });
      queryClient.invalidateQueries({ queryKey: ["orthopedics-stats"] });
      setIsModalOpen(false);
      setSelectedSurgery(undefined);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => surgeriesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orthopedic-surgeries"] });
      queryClient.invalidateQueries({ queryKey: ["orthopedics-stats"] });
    },
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const data: Record<string, string | number | undefined> = {
      patientId: parseInt(formData.get("patientId") as string),
      surgeryDate: formData.get("surgeryDate") as string,
      procedure: formData.get("procedure") as string,
      indication: formData.get("indication") as string,
      implants: (formData.get("implants") as string) || undefined,
      surgeon: formData.get("surgeon") as string,
      assistant: (formData.get("assistant") as string) || undefined,
      anesthesia: (formData.get("anesthesia") as string) || undefined,
      duration: formData.get("duration")
        ? parseInt(formData.get("duration") as string)
        : undefined,
      complications: (formData.get("complications") as string) || undefined,
      postOpInstructions:
        (formData.get("postOpInstructions") as string) || undefined,
      outcome: (formData.get("outcome") as string) || undefined,
      notes: (formData.get("notes") as string) || undefined,
    };

    if (selectedSurgery) {
      updateMutation.mutate({ id: selectedSurgery.id, data });
    } else {
      createMutation.mutate(data);
    }
  };

  const handleDelete = (surgery: OrthopedicSurgery) => {
    if (
      window.confirm(
        t(
          "message.confirmDelete",
          `Are you sure you want to delete this surgery record?`,
        ),
      )
    ) {
      deleteMutation.mutate(surgery.id);
    }
  };

  const handleEdit = (surgery: OrthopedicSurgery) => {
    setSelectedSurgery(surgery);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedSurgery(undefined);
    setIsModalOpen(true);
  };

  const handleViewDetails = (surgery: OrthopedicSurgery) => {
    setSelectedSurgery(surgery);
    setIsDetailModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setIsDetailModalOpen(false);
    setSelectedSurgery(undefined);
  };

  const filteredSurgeries = surgeries?.filter((surgery) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      surgery.patientName?.toLowerCase().includes(searchLower) ||
      surgery.procedure.toLowerCase().includes(searchLower) ||
      surgery.indication.toLowerCase().includes(searchLower) ||
      surgery.surgeon.toLowerCase().includes(searchLower) ||
      surgery.implants?.toLowerCase().includes(searchLower)
    );
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {t("page.surgeries.title", "Orthopedic Surgeries")}
          </h1>
          <p className="text-gray-600 mt-1">
            {t(
              "page.surgeries.description",
              "Manage surgical procedures and outcomes",
            )}
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <svg
            className="w-5 h-5 ltr:mr-2 rtl:ml-2"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 4v16m8-8H4"
            />
          </svg>
          {t("action.recordSurgery", "Record Surgery")}
        </button>
      </div>

      <div className="card animate-fade-in">
        <div className="mb-4">
          <input
            type="text"
            placeholder={t(
              "field.search",
              "Search by patient, procedure, surgeon, or indication...",
            )}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">
              {t("message.loading", "Loading...")}
            </p>
          </div>
        ) : filteredSurgeries && filteredSurgeries.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.date", "Date")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.patient", "Patient")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.procedure", "Procedure")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.indication", "Indication")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.surgeon", "Surgeon")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.duration", "Duration")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.complications", "Complications")}
                  </th>
                  <th className="px-4 py-3 ltr:text-left rtl:text-right text-xs font-medium text-gray-500 uppercase">
                    {t("field.actions", "Actions")}
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredSurgeries.map((surgery) => (
                  <tr key={surgery.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900 whitespace-nowrap">
                      {format(new Date(surgery.surgeryDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {surgery.patientName || `Patient #${surgery.patientId}`}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {surgery.procedure}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {surgery.indication}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {surgery.surgeon}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {surgery.duration ? `${surgery.duration} min` : "-"}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      {surgery.complications ? (
                        <span className="text-red-600 font-medium">
                          {t("label.yes", "Yes")}
                        </span>
                      ) : (
                        <span className="text-green-600">
                          {t("label.none", "None")}
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewDetails(surgery)}
                          className="text-primary-600 hover:text-primary-800"
                          title={t("action.view", "View")}
                        >
                          <svg
                            className="w-5 h-5"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                            />
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                            />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleEdit(surgery)}
                          className="text-blue-600 hover:text-blue-800"
                          title={t("action.edit", "Edit")}
                        >
                          <svg
                            className="w-5 h-5"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                            />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDelete(surgery)}
                          className="text-red-600 hover:text-red-800"
                          title={t("action.delete", "Delete")}
                        >
                          <svg
                            className="w-5 h-5"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                            />
                          </svg>
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
              ? t(
                  "message.noResults",
                  "No surgeries found matching your search.",
                )
              : t("message.noSurgeries", "No surgeries recorded yet.")}
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
                {selectedSurgery
                  ? t("action.editSurgery", "Edit Surgery")
                  : t("action.recordSurgery", "Record Surgery")}
              </Dialog.Title>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.patientId", "Patient ID")} *
                    </label>
                    <input
                      type="number"
                      name="patientId"
                      defaultValue={selectedSurgery?.patientId}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.surgeryDate", "Surgery Date")} *
                    </label>
                    <input
                      type="date"
                      name="surgeryDate"
                      defaultValue={
                        selectedSurgery?.surgeryDate
                          ? format(
                              new Date(selectedSurgery.surgeryDate),
                              "yyyy-MM-dd",
                            )
                          : format(new Date(), "yyyy-MM-dd")
                      }
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.procedure", "Procedure")} *
                    </label>
                    <input
                      type="text"
                      name="procedure"
                      defaultValue={selectedSurgery?.procedure}
                      required
                      className="input w-full"
                      placeholder="e.g., Total Knee Replacement, ACL Reconstruction"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.indication", "Indication")} *
                    </label>
                    <input
                      type="text"
                      name="indication"
                      defaultValue={selectedSurgery?.indication}
                      required
                      className="input w-full"
                      placeholder="e.g., Osteoarthritis, ACL tear"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.surgeon", "Surgeon")} *
                    </label>
                    <input
                      type="text"
                      name="surgeon"
                      defaultValue={selectedSurgery?.surgeon}
                      required
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.assistant", "Assistant Surgeon")}
                    </label>
                    <input
                      type="text"
                      name="assistant"
                      defaultValue={selectedSurgery?.assistant}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.anesthesia", "Anesthesia Type")}
                    </label>
                    <input
                      type="text"
                      name="anesthesia"
                      defaultValue={selectedSurgery?.anesthesia}
                      className="input w-full"
                      placeholder="e.g., General, Spinal, Regional"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("field.duration", "Duration (minutes)")}
                    </label>
                    <input
                      type="number"
                      name="duration"
                      defaultValue={selectedSurgery?.duration}
                      className="input w-full"
                      min="1"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t("field.implants", "Implants/Hardware Used")}
                  </label>
                  <textarea
                    name="implants"
                    defaultValue={selectedSurgery?.implants}
                    rows={2}
                    className="input w-full"
                    placeholder="e.g., Titanium screws, Knee prosthesis model XYZ"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t("field.complications", "Complications")}
                  </label>
                  <textarea
                    name="complications"
                    defaultValue={selectedSurgery?.complications}
                    rows={2}
                    className="input w-full"
                    placeholder="Describe any complications encountered during surgery"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t(
                      "field.postOpInstructions",
                      "Post-operative Instructions",
                    )}
                  </label>
                  <textarea
                    name="postOpInstructions"
                    defaultValue={selectedSurgery?.postOpInstructions}
                    rows={3}
                    className="input w-full"
                    placeholder="Recovery instructions, physical therapy, restrictions, etc."
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t("field.outcome", "Outcome")}
                  </label>
                  <textarea
                    name="outcome"
                    defaultValue={selectedSurgery?.outcome}
                    rows={2}
                    className="input w-full"
                    placeholder="Surgery outcome and patient condition"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t("field.notes", "Additional Notes")}
                  </label>
                  <textarea
                    name="notes"
                    defaultValue={selectedSurgery?.notes}
                    rows={2}
                    className="input w-full"
                  />
                </div>
                <div className="flex justify-end gap-2 pt-4">
                  <button
                    type="button"
                    onClick={handleModalClose}
                    className="btn btn-secondary"
                  >
                    {t("action.cancel", "Cancel")}
                  </button>
                  <button
                    type="submit"
                    disabled={
                      createMutation.isPending || updateMutation.isPending
                    }
                    className="btn btn-primary"
                  >
                    {createMutation.isPending || updateMutation.isPending
                      ? t("action.saving", "Saving...")
                      : selectedSurgery
                        ? t("action.update", "Update")
                        : t("action.create", "Create")}
                  </button>
                </div>
              </form>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* Detail Modal */}
      <Dialog
        open={isDetailModalOpen}
        onClose={handleModalClose}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {t("title.surgeryDetails", "Surgery Details")}
              </Dialog.Title>
              {selectedSurgery && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.patient", "Patient")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.patientName ||
                          `Patient #${selectedSurgery.patientId}`}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.surgeryDate", "Surgery Date")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {format(
                          new Date(selectedSurgery.surgeryDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.procedure", "Procedure")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.procedure}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.indication", "Indication")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.indication}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.surgeon", "Surgeon")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.surgeon}
                      </p>
                    </div>
                    {selectedSurgery.assistant && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">
                          {t("field.assistant", "Assistant")}
                        </label>
                        <p className="text-sm text-gray-900 mt-1">
                          {selectedSurgery.assistant}
                        </p>
                      </div>
                    )}
                    {selectedSurgery.anesthesia && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">
                          {t("field.anesthesia", "Anesthesia")}
                        </label>
                        <p className="text-sm text-gray-900 mt-1">
                          {selectedSurgery.anesthesia}
                        </p>
                      </div>
                    )}
                    {selectedSurgery.duration && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700">
                          {t("field.duration", "Duration")}
                        </label>
                        <p className="text-sm text-gray-900 mt-1">
                          {selectedSurgery.duration} minutes
                        </p>
                      </div>
                    )}
                  </div>
                  {selectedSurgery.implants && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.implants", "Implants/Hardware")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.implants}
                      </p>
                    </div>
                  )}
                  {selectedSurgery.complications && (
                    <div className="border-t pt-4">
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.complications", "Complications")}
                      </label>
                      <p className="text-sm text-red-600 mt-1">
                        {selectedSurgery.complications}
                      </p>
                    </div>
                  )}
                  {selectedSurgery.postOpInstructions && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t(
                          "field.postOpInstructions",
                          "Post-operative Instructions",
                        )}
                      </label>
                      <p className="text-sm text-gray-900 mt-1 whitespace-pre-wrap">
                        {selectedSurgery.postOpInstructions}
                      </p>
                    </div>
                  )}
                  {selectedSurgery.outcome && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.outcome", "Outcome")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.outcome}
                      </p>
                    </div>
                  )}
                  {selectedSurgery.notes && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        {t("field.notes", "Notes")}
                      </label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedSurgery.notes}
                      </p>
                    </div>
                  )}
                  <div className="flex justify-end gap-2 pt-4 border-t">
                    <button
                      onClick={handleModalClose}
                      className="btn btn-secondary"
                    >
                      {t("action.close", "Close")}
                    </button>
                    <button
                      onClick={() => {
                        handleModalClose();
                        handleEdit(selectedSurgery);
                      }}
                      className="btn btn-primary"
                    >
                      {t("action.edit", "Edit")}
                    </button>
                  </div>
                </div>
              )}
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
