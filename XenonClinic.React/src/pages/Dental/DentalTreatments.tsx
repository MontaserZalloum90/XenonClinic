import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  ClipboardDocumentListIcon,
} from '@heroicons/react/24/outline';
import { Dialog } from '@headlessui/react';
import type {
  DentalTreatment,
  CreateDentalTreatmentRequest,
  DentalTreatmentStatus,
  TreatmentType,
} from '../../types/dental';
import { format } from 'date-fns';

// Mock API functions - Replace with actual API calls
const dentalTreatmentApi = {
  getAll: async () => ({
    data: [] as DentalTreatment[],
  }),
  create: async (data: CreateDentalTreatmentRequest) => ({
    data: { id: Date.now(), ...data },
  }),
  update: async (id: number, data: Partial<DentalTreatment>) => ({
    data: { id, ...data },
  }),
  delete: async () => ({
    data: { success: true },
  }),
};

const getTreatmentTypeLabel = (type: TreatmentType): string => {
  const labels: Record<TreatmentType, string> = {
    cleaning: 'Cleaning',
    filling: 'Filling',
    extraction: 'Extraction',
    root_canal: 'Root Canal',
    crown: 'Crown',
    bridge: 'Bridge',
    implant: 'Implant',
    veneer: 'Veneer',
    whitening: 'Whitening',
    orthodontics: 'Orthodontics',
    denture: 'Denture',
    scaling: 'Scaling',
    polishing: 'Polishing',
    xray: 'X-Ray',
    consultation: 'Consultation',
    emergency: 'Emergency',
    other: 'Other',
  };
  return labels[type] || type;
};

const getStatusBadge = (status: DentalTreatmentStatus) => {
  const config: Record<
    DentalTreatmentStatus,
    { className: string; label: string }
  > = {
    planned: { className: 'bg-blue-100 text-blue-800', label: 'Planned' },
    in_progress: { className: 'bg-yellow-100 text-yellow-800', label: 'In Progress' },
    completed: { className: 'bg-green-100 text-green-800', label: 'Completed' },
    cancelled: { className: 'bg-red-100 text-red-800', label: 'Cancelled' },
  };
  const c = config[status];
  return (
    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
      {c.label}
    </span>
  );
};

interface TreatmentFormData {
  patientId: number;
  toothNumber?: number;
  treatmentType: TreatmentType;
  description?: string;
  status: DentalTreatmentStatus;
  performedBy?: string;
  performedDate?: string;
  scheduledDate?: string;
  cost?: number;
  isPaid?: boolean;
  notes?: string;
}

const TreatmentForm = ({
  treatment,
  onSuccess,
  onCancel,
}: {
  treatment?: DentalTreatment;
  onSuccess: () => void;
  onCancel: () => void;
}) => {
  const queryClient = useQueryClient();
  const isEditing = !!treatment;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TreatmentFormData>({
    defaultValues: treatment
      ? {
          patientId: treatment.patientId,
          toothNumber: treatment.toothNumber,
          treatmentType: treatment.treatmentType,
          description: treatment.description || '',
          status: treatment.status,
          performedBy: treatment.performedBy || '',
          performedDate: treatment.performedDate
            ? format(new Date(treatment.performedDate), 'yyyy-MM-dd')
            : '',
          scheduledDate: treatment.scheduledDate
            ? format(new Date(treatment.scheduledDate), 'yyyy-MM-dd')
            : '',
          cost: treatment.cost,
          isPaid: treatment.isPaid || false,
          notes: treatment.notes || '',
        }
      : {
          status: 'planned',
          treatmentType: 'consultation',
          isPaid: false,
        },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateDentalTreatmentRequest) => dentalTreatmentApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dental-treatments'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: TreatmentFormData) =>
      dentalTreatmentApi.update(treatment!.id, { ...data, id: treatment!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dental-treatments'] });
      onSuccess();
    },
  });

  const onSubmit = (data: TreatmentFormData) => {
    if (isEditing) {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(data as CreateDentalTreatmentRequest);
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;
  const error = createMutation.error || updateMutation.error;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {error && (
        <div className="p-3 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-600">
            Error: {error instanceof Error ? error.message : 'An error occurred'}
          </p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Patient ID */}
        <div>
          <label htmlFor="patientId" className="block text-sm font-medium text-gray-700 mb-1">
            Patient ID *
          </label>
          <input
            type="number"
            id="patientId"
            {...register('patientId', { required: 'Patient ID is required', valueAsNumber: true })}
            className="input"
            placeholder="1"
          />
          {errors.patientId && (
            <p className="mt-1 text-sm text-red-600">{errors.patientId.message}</p>
          )}
        </div>

        {/* Tooth Number */}
        <div>
          <label htmlFor="toothNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Tooth Number (1-32)
          </label>
          <input
            type="number"
            id="toothNumber"
            {...register('toothNumber', { valueAsNumber: true, min: 1, max: 32 })}
            className="input"
            placeholder="Optional"
          />
        </div>

        {/* Treatment Type */}
        <div>
          <label htmlFor="treatmentType" className="block text-sm font-medium text-gray-700 mb-1">
            Treatment Type *
          </label>
          <select id="treatmentType" {...register('treatmentType', { required: true })} className="input">
            <option value="consultation">Consultation</option>
            <option value="cleaning">Cleaning</option>
            <option value="filling">Filling</option>
            <option value="extraction">Extraction</option>
            <option value="root_canal">Root Canal</option>
            <option value="crown">Crown</option>
            <option value="bridge">Bridge</option>
            <option value="implant">Implant</option>
            <option value="veneer">Veneer</option>
            <option value="whitening">Whitening</option>
            <option value="orthodontics">Orthodontics</option>
            <option value="denture">Denture</option>
            <option value="scaling">Scaling</option>
            <option value="polishing">Polishing</option>
            <option value="xray">X-Ray</option>
            <option value="emergency">Emergency</option>
            <option value="other">Other</option>
          </select>
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select id="status" {...register('status', { required: true })} className="input">
            <option value="planned">Planned</option>
            <option value="in_progress">In Progress</option>
            <option value="completed">Completed</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </div>

        {/* Performed By */}
        <div>
          <label htmlFor="performedBy" className="block text-sm font-medium text-gray-700 mb-1">
            Performed By
          </label>
          <input
            type="text"
            id="performedBy"
            {...register('performedBy')}
            className="input"
            placeholder="Dr. Smith"
          />
        </div>

        {/* Cost */}
        <div>
          <label htmlFor="cost" className="block text-sm font-medium text-gray-700 mb-1">
            Cost (AED)
          </label>
          <input
            type="number"
            step="0.01"
            id="cost"
            {...register('cost', { valueAsNumber: true })}
            className="input"
            placeholder="0.00"
          />
        </div>

        {/* Scheduled Date */}
        <div>
          <label htmlFor="scheduledDate" className="block text-sm font-medium text-gray-700 mb-1">
            Scheduled Date
          </label>
          <input type="date" id="scheduledDate" {...register('scheduledDate')} className="input" />
        </div>

        {/* Performed Date */}
        <div>
          <label htmlFor="performedDate" className="block text-sm font-medium text-gray-700 mb-1">
            Performed Date
          </label>
          <input type="date" id="performedDate" {...register('performedDate')} className="input" />
        </div>

        {/* Is Paid */}
        <div className="flex items-center pt-6">
          <input
            type="checkbox"
            id="isPaid"
            {...register('isPaid')}
            className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
          />
          <label htmlFor="isPaid" className="ml-2 block text-sm text-gray-700">
            Payment Received
          </label>
        </div>
      </div>

      {/* Description */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          id="description"
          {...register('description')}
          rows={2}
          className="input"
          placeholder="Treatment details..."
        />
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={3}
          className="input"
          placeholder="Additional notes..."
        />
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-3 pt-4 border-t">
        <button type="button" onClick={onCancel} disabled={isPending} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" disabled={isPending} className="btn btn-primary">
          {isPending ? (
            <div className="flex items-center">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              {isEditing ? 'Updating...' : 'Creating...'}
            </div>
          ) : isEditing ? (
            'Update Treatment'
          ) : (
            'Create Treatment'
          )}
        </button>
      </div>
    </form>
  );
};

export const DentalTreatments = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTreatment, setSelectedTreatment] = useState<DentalTreatment | undefined>(
    undefined
  );

  // Fetch treatments
  const { data: treatmentsData, isLoading } = useQuery({
    queryKey: ['dental-treatments'],
    queryFn: () => dentalTreatmentApi.getAll(),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dentalTreatmentApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dental-treatments'] });
    },
  });

  const treatments = treatmentsData?.data || [];

  // Filter treatments
  const filteredTreatments = treatments.filter((treatment) => {
    const matchesSearch =
      !searchTerm ||
      treatment.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      treatment.patientId.toString().includes(searchTerm);
    const matchesStatus = !statusFilter || treatment.status === statusFilter;
    const matchesType = !typeFilter || treatment.treatmentType === typeFilter;
    return matchesSearch && matchesStatus && matchesType;
  });

  const handleDelete = (treatment: DentalTreatment) => {
    if (
      window.confirm(
        `Are you sure you want to delete the treatment for ${treatment.patientName}?`
      )
    ) {
      deleteMutation.mutate(treatment.id);
    }
  };

  const handleEdit = (treatment: DentalTreatment) => {
    setSelectedTreatment(treatment);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedTreatment(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedTreatment(undefined);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dental Treatments</h1>
          <p className="text-gray-600 mt-1">Manage patient treatments and procedures</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2 inline" />
          New Treatment
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Treatments</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{treatments.length}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {treatments.filter((t) => t.status === 'planned').length}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">In Progress</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">
            {treatments.filter((t) => t.status === 'in_progress').length}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {treatments.filter((t) => t.status === 'completed').length}
          </p>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="card">
        <div className="flex flex-wrap gap-4 mb-4">
          <div className="flex-1 min-w-[200px]">
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
          <div className="flex items-center gap-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="input"
            >
              <option value="">All Status</option>
              <option value="planned">Planned</option>
              <option value="in_progress">In Progress</option>
              <option value="completed">Completed</option>
              <option value="cancelled">Cancelled</option>
            </select>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className="input"
            >
              <option value="">All Types</option>
              <option value="consultation">Consultation</option>
              <option value="cleaning">Cleaning</option>
              <option value="filling">Filling</option>
              <option value="extraction">Extraction</option>
              <option value="root_canal">Root Canal</option>
              <option value="crown">Crown</option>
              <option value="implant">Implant</option>
            </select>
          </div>
        </div>

        {/* Treatments Table */}
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading treatments...</p>
          </div>
        ) : filteredTreatments.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Tooth
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Treatment
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Cost
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
                {filteredTreatments.map((treatment) => (
                  <tr key={treatment.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm">
                      <div className="font-medium text-gray-900">
                        {treatment.patientName || `Patient #${treatment.patientId}`}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {treatment.toothNumber || '-'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {getTreatmentTypeLabel(treatment.treatmentType)}
                    </td>
                    <td className="px-4 py-3 text-sm">{getStatusBadge(treatment.status)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {treatment.performedDate
                        ? format(new Date(treatment.performedDate), 'MMM d, yyyy')
                        : treatment.scheduledDate
                        ? format(new Date(treatment.scheduledDate), 'MMM d, yyyy')
                        : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {treatment.cost ? `AED ${treatment.cost.toFixed(2)}` : '-'}
                      {treatment.isPaid && (
                        <span className="ml-2 text-xs text-green-600">Paid</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {treatment.performedBy || '-'}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(treatment)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(treatment)}
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
          <div className="text-center py-12">
            <ClipboardDocumentListIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-500">
              {searchTerm || statusFilter || typeFilter
                ? 'No treatments found matching your filters.'
                : 'No treatments found.'}
            </p>
            <button
              onClick={handleCreate}
              className="mt-4 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
            >
              <PlusIcon className="h-5 w-5 mr-2" />
              Create First Treatment
            </button>
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedTreatment ? 'Edit Treatment' : 'New Treatment'}
              </Dialog.Title>
              <TreatmentForm
                treatment={selectedTreatment}
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
