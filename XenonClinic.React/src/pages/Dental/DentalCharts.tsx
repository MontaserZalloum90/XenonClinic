import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  MagnifyingGlassIcon,
  PlusIcon,
  DocumentChartBarIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import type { DentalChart, ToothCondition } from "../../types/dental";
import { DentalChartForm } from "../../components/DentalChartForm";
import { format } from "date-fns";
import { dentalChartApi } from "../../lib/api";

export const DentalCharts = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedChart, setSelectedChart] = useState<DentalChart | undefined>(
    undefined,
  );

  // Fetch dental charts
  const { data: chartsData, isLoading } = useQuery({
    queryKey: ["dental-charts"],
    queryFn: () => dentalChartApi.getAll(),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dentalChartApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["dental-charts"] });
    },
    onError: (error) => {
      console.error("Failed to delete dental chart:", error);
    },
  });

  const charts = chartsData?.data || [];

  // Filter charts
  const filteredCharts = charts.filter((chart) => {
    const matchesSearch =
      !searchTerm ||
      chart.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      chart.patientId.toString().includes(searchTerm);
    return matchesSearch;
  });

  const handleDelete = (chart: DentalChart) => {
    if (
      window.confirm(
        `Are you sure you want to delete the chart for ${chart.patientName}?`,
      )
    ) {
      deleteMutation.mutate(chart.id);
    }
  };

  const handleEdit = (chart: DentalChart) => {
    setSelectedChart(chart);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedChart(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedChart(undefined);
  };

  const getToothConditionSummary = (teeth: ToothCondition[]) => {
    const total = teeth.length;
    const healthy = teeth.filter((t) => t.condition === "healthy").length;
    const problematic = total - healthy;
    return { total, healthy, problematic };
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dental Charts</h1>
          <p className="text-gray-600 mt-1">
            Manage patient dental charts and tooth conditions
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2 inline" />
          New Chart
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Charts</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {charts.length}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Charts This Month</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {
              charts.filter(
                (c) =>
                  new Date(c.chartDate).getMonth() === new Date().getMonth() &&
                  new Date(c.chartDate).getFullYear() ===
                    new Date().getFullYear(),
              ).length
            }
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Updated This Week</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {
              charts.filter((c) => {
                const weekAgo = new Date();
                weekAgo.setDate(weekAgo.getDate() - 7);
                return new Date(c.chartDate) >= weekAgo;
              }).length
            }
          </p>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="card">
        <div className="mb-4">
          <div className="relative">
            <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
            <input
              type="text"
              placeholder="Search by patient name or ID..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input w-full pl-10"
            />
          </div>
        </div>

        {/* Charts List */}
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading charts...</p>
          </div>
        ) : filteredCharts.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Chart Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Teeth Charted
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status Summary
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Created By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredCharts.map((chart) => {
                  const summary = getToothConditionSummary(chart.teeth);
                  return (
                    <tr key={chart.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm">
                        <div className="font-medium text-gray-900">
                          {chart.patientName || `Patient #${chart.patientId}`}
                        </div>
                        <div className="text-gray-500">
                          ID: {chart.patientId}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {format(new Date(chart.chartDate), "MMM d, yyyy")}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {summary.total} teeth
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-2">
                          <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                            {summary.healthy} healthy
                          </span>
                          {summary.problematic > 0 && (
                            <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                              {summary.problematic} issues
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {chart.createdBy}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleEdit(chart)}
                            className="text-primary-600 hover:text-primary-800"
                          >
                            View
                          </button>
                          <button
                            onClick={() => handleDelete(chart)}
                            className="text-red-600 hover:text-red-800"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-12">
            <DocumentChartBarIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-500">
              {searchTerm
                ? "No charts found matching your search."
                : "No dental charts found."}
            </p>
            <button
              onClick={handleCreate}
              className="mt-4 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
            >
              <PlusIcon className="h-5 w-5 mr-2" />
              Create First Chart
            </button>
          </div>
        )}
      </div>

      {/* Visual Tooth Diagram Placeholder */}
      {selectedChart && (
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Tooth Diagram
          </h3>
          <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
            <DocumentChartBarIcon className="h-16 w-16 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-500">
              Visual tooth diagram will be displayed here
            </p>
            <p className="text-sm text-gray-400 mt-2">
              Interactive dental chart showing all 32 teeth with conditions
            </p>
          </div>
        </div>
      )}

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
                {selectedChart ? "View/Edit Dental Chart" : "New Dental Chart"}
              </Dialog.Title>
              <DentalChartForm
                chart={selectedChart}
                onSuccess={handleModalClose}
                onCancel={handleModalClose}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
