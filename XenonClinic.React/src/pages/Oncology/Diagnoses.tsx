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
  CancerDiagnosis,
  CreateCancerDiagnosisRequest,
  CancerType,
  CancerStage,
  CancerGrade,
} from '../../types/oncology';
import { format } from 'date-fns';

// Mock API functions - Replace with actual API calls
const diagnosisApi = {
  getAll: async () => ({
    data: [] as CancerDiagnosis[],
  }),
  create: async (data: CreateCancerDiagnosisRequest) => ({
    data: { id: Date.now(), ...data },
  }),
  update: async (id: number, data: Partial<CancerDiagnosis>) => ({
    data: { id, ...data },
  }),
  delete: async (id: number) => ({
    data: { success: true },
  }),
};

const getCancerTypeLabel = (type: CancerType): string => {
  const labels: Record<CancerType, string> = {
    breast: 'Breast',
    lung: 'Lung',
    colorectal: 'Colorectal',
    prostate: 'Prostate',
    pancreatic: 'Pancreatic',
    liver: 'Liver',
    stomach: 'Stomach',
    kidney: 'Kidney',
    bladder: 'Bladder',
    thyroid: 'Thyroid',
    melanoma: 'Melanoma',
    leukemia: 'Leukemia',
    lymphoma: 'Lymphoma',
    ovarian: 'Ovarian',
    cervical: 'Cervical',
    uterine: 'Uterine',
    brain: 'Brain',
    esophageal: 'Esophageal',
    other: 'Other',
  };
  return labels[type] || type;
};

const getStageLabel = (stage: CancerStage): string => {
  const labels: Record<CancerStage, string> = {
    stage_0: 'Stage 0',
    stage_1: 'Stage I',
    stage_2: 'Stage II',
    stage_3: 'Stage III',
    stage_4: 'Stage IV',
    unknown: 'Unknown',
  };
  return labels[stage] || stage;
};

const getStageBadge = (stage: CancerStage) => {
  const config: Record<CancerStage, { className: string }> = {
    stage_0: { className: 'bg-green-100 text-green-800' },
    stage_1: { className: 'bg-blue-100 text-blue-800' },
    stage_2: { className: 'bg-yellow-100 text-yellow-800' },
    stage_3: { className: 'bg-orange-100 text-orange-800' },
    stage_4: { className: 'bg-red-100 text-red-800' },
    unknown: { className: 'bg-gray-100 text-gray-800' },
  };
  const c = config[stage];
  return (
    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
      {getStageLabel(stage)}
    </span>
  );
};

interface DiagnosisFormData {
  patientId: number;
  diagnosisDate: string;
  cancerType: CancerType;
  stage: CancerStage;
  grade: CancerGrade;
  primarySite: string;
  metastasis: boolean;
  metastaticSites?: string;
  histology: string;
  diagnosedBy: string;
  tnmT?: string;
  tnmN?: string;
  tnmM?: string;
  biomarkers?: string;
  notes?: string;
}

const DiagnosisForm = ({
  diagnosis,
  onSuccess,
  onCancel,
}: {
  diagnosis?: CancerDiagnosis;
  onSuccess: () => void;
  onCancel: () => void;
}) => {
  const queryClient = useQueryClient();
  const isEditing = !!diagnosis;

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<DiagnosisFormData>({
    defaultValues: diagnosis
      ? {
          patientId: diagnosis.patientId,
          diagnosisDate: format(new Date(diagnosis.diagnosisDate), 'yyyy-MM-dd'),
          cancerType: diagnosis.cancerType,
          stage: diagnosis.stage,
          grade: diagnosis.grade,
          primarySite: diagnosis.primarySite,
          metastasis: diagnosis.metastasis,
          metastaticSites: diagnosis.metastaticSites?.join(', ') || '',
          histology: diagnosis.histology,
          diagnosedBy: diagnosis.diagnosedBy,
          tnmT: diagnosis.tnmStaging?.t || '',
          tnmN: diagnosis.tnmStaging?.n || '',
          tnmM: diagnosis.tnmStaging?.m || '',
          biomarkers: diagnosis.biomarkers || '',
          notes: diagnosis.notes || '',
        }
      : {
          metastasis: false,
          cancerType: 'breast',
          stage: 'stage_1',
          grade: 'g2',
        },
  });

  const metastasis = watch('metastasis');

  const createMutation = useMutation({
    mutationFn: (data: CreateCancerDiagnosisRequest) => diagnosisApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cancer-diagnoses'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: DiagnosisFormData) =>
      diagnosisApi.update(diagnosis!.id, { ...data, id: diagnosis!.id } as Partial<CancerDiagnosis>),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cancer-diagnoses'] });
      onSuccess();
    },
  });

  const onSubmit = (data: DiagnosisFormData) => {
    const payload: CreateCancerDiagnosisRequest = {
      patientId: data.patientId,
      diagnosisDate: data.diagnosisDate,
      cancerType: data.cancerType,
      stage: data.stage,
      grade: data.grade,
      primarySite: data.primarySite,
      metastasis: data.metastasis,
      metastaticSites: data.metastaticSites
        ? data.metastaticSites.split(',').map((s) => s.trim())
        : undefined,
      histology: data.histology,
      diagnosedBy: data.diagnosedBy,
      tnmStaging:
        data.tnmT || data.tnmN || data.tnmM
          ? {
              t: data.tnmT || '',
              n: data.tnmN || '',
              m: data.tnmM || '',
            }
          : undefined,
      biomarkers: data.biomarkers,
      notes: data.notes,
    };

    if (isEditing) {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(payload);
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

        {/* Diagnosis Date */}
        <div>
          <label htmlFor="diagnosisDate" className="block text-sm font-medium text-gray-700 mb-1">
            Diagnosis Date *
          </label>
          <input
            type="date"
            id="diagnosisDate"
            {...register('diagnosisDate', { required: 'Diagnosis date is required' })}
            className="input"
          />
          {errors.diagnosisDate && (
            <p className="mt-1 text-sm text-red-600">{errors.diagnosisDate.message}</p>
          )}
        </div>

        {/* Cancer Type */}
        <div>
          <label htmlFor="cancerType" className="block text-sm font-medium text-gray-700 mb-1">
            Cancer Type *
          </label>
          <select id="cancerType" {...register('cancerType', { required: true })} className="input">
            <option value="breast">Breast</option>
            <option value="lung">Lung</option>
            <option value="colorectal">Colorectal</option>
            <option value="prostate">Prostate</option>
            <option value="pancreatic">Pancreatic</option>
            <option value="liver">Liver</option>
            <option value="stomach">Stomach</option>
            <option value="kidney">Kidney</option>
            <option value="bladder">Bladder</option>
            <option value="thyroid">Thyroid</option>
            <option value="melanoma">Melanoma</option>
            <option value="leukemia">Leukemia</option>
            <option value="lymphoma">Lymphoma</option>
            <option value="ovarian">Ovarian</option>
            <option value="cervical">Cervical</option>
            <option value="uterine">Uterine</option>
            <option value="brain">Brain</option>
            <option value="esophageal">Esophageal</option>
            <option value="other">Other</option>
          </select>
        </div>

        {/* Stage */}
        <div>
          <label htmlFor="stage" className="block text-sm font-medium text-gray-700 mb-1">
            Stage *
          </label>
          <select id="stage" {...register('stage', { required: true })} className="input">
            <option value="stage_0">Stage 0</option>
            <option value="stage_1">Stage I</option>
            <option value="stage_2">Stage II</option>
            <option value="stage_3">Stage III</option>
            <option value="stage_4">Stage IV</option>
            <option value="unknown">Unknown</option>
          </select>
        </div>

        {/* Grade */}
        <div>
          <label htmlFor="grade" className="block text-sm font-medium text-gray-700 mb-1">
            Grade *
          </label>
          <select id="grade" {...register('grade', { required: true })} className="input">
            <option value="g1">G1 - Well Differentiated</option>
            <option value="g2">G2 - Moderately Differentiated</option>
            <option value="g3">G3 - Poorly Differentiated</option>
            <option value="g4">G4 - Undifferentiated</option>
            <option value="gx">GX - Cannot Be Assessed</option>
          </select>
        </div>

        {/* Primary Site */}
        <div>
          <label htmlFor="primarySite" className="block text-sm font-medium text-gray-700 mb-1">
            Primary Site *
          </label>
          <input
            type="text"
            id="primarySite"
            {...register('primarySite', { required: 'Primary site is required' })}
            className="input"
            placeholder="e.g., Left breast, upper outer quadrant"
          />
          {errors.primarySite && (
            <p className="mt-1 text-sm text-red-600">{errors.primarySite.message}</p>
          )}
        </div>

        {/* Histology */}
        <div>
          <label htmlFor="histology" className="block text-sm font-medium text-gray-700 mb-1">
            Histology *
          </label>
          <input
            type="text"
            id="histology"
            {...register('histology', { required: 'Histology is required' })}
            className="input"
            placeholder="e.g., Invasive ductal carcinoma"
          />
          {errors.histology && (
            <p className="mt-1 text-sm text-red-600">{errors.histology.message}</p>
          )}
        </div>

        {/* Diagnosed By */}
        <div>
          <label htmlFor="diagnosedBy" className="block text-sm font-medium text-gray-700 mb-1">
            Diagnosed By *
          </label>
          <input
            type="text"
            id="diagnosedBy"
            {...register('diagnosedBy', { required: 'Diagnosed by is required' })}
            className="input"
            placeholder="Dr. Smith"
          />
          {errors.diagnosedBy && (
            <p className="mt-1 text-sm text-red-600">{errors.diagnosedBy.message}</p>
          )}
        </div>

        {/* Metastasis */}
        <div className="flex items-center pt-6">
          <input
            type="checkbox"
            id="metastasis"
            {...register('metastasis')}
            className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
          />
          <label htmlFor="metastasis" className="ml-2 block text-sm text-gray-700">
            Metastasis Present
          </label>
        </div>
      </div>

      {/* Metastatic Sites - Conditional */}
      {metastasis && (
        <div>
          <label htmlFor="metastaticSites" className="block text-sm font-medium text-gray-700 mb-1">
            Metastatic Sites
          </label>
          <input
            type="text"
            id="metastaticSites"
            {...register('metastaticSites')}
            className="input"
            placeholder="Comma-separated: Liver, Lungs, Bone"
          />
        </div>
      )}

      {/* TNM Staging */}
      <div className="border-t pt-4">
        <h3 className="text-sm font-medium text-gray-900 mb-3">TNM Staging (Optional)</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label htmlFor="tnmT" className="block text-sm font-medium text-gray-700 mb-1">
              T (Tumor)
            </label>
            <input
              type="text"
              id="tnmT"
              {...register('tnmT')}
              className="input"
              placeholder="e.g., T2"
            />
          </div>
          <div>
            <label htmlFor="tnmN" className="block text-sm font-medium text-gray-700 mb-1">
              N (Nodes)
            </label>
            <input
              type="text"
              id="tnmN"
              {...register('tnmN')}
              className="input"
              placeholder="e.g., N1"
            />
          </div>
          <div>
            <label htmlFor="tnmM" className="block text-sm font-medium text-gray-700 mb-1">
              M (Metastasis)
            </label>
            <input
              type="text"
              id="tnmM"
              {...register('tnmM')}
              className="input"
              placeholder="e.g., M0"
            />
          </div>
        </div>
      </div>

      {/* Biomarkers */}
      <div>
        <label htmlFor="biomarkers" className="block text-sm font-medium text-gray-700 mb-1">
          Biomarkers
        </label>
        <textarea
          id="biomarkers"
          {...register('biomarkers')}
          rows={2}
          className="input"
          placeholder="e.g., ER+, PR+, HER2-"
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
          placeholder="Additional clinical notes..."
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
            'Update Diagnosis'
          ) : (
            'Create Diagnosis'
          )}
        </button>
      </div>
    </form>
  );
};

export const Diagnoses = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [stageFilter, setStageFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedDiagnosis, setSelectedDiagnosis] = useState<CancerDiagnosis | undefined>(
    undefined
  );

  // Fetch diagnoses
  const { data: diagnosesData, isLoading } = useQuery({
    queryKey: ['cancer-diagnoses'],
    queryFn: () => diagnosisApi.getAll(),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => diagnosisApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cancer-diagnoses'] });
    },
  });

  const diagnoses = diagnosesData?.data || [];

  // Filter diagnoses
  const filteredDiagnoses = diagnoses.filter((diagnosis) => {
    const matchesSearch =
      !searchTerm ||
      diagnosis.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      diagnosis.patientId.toString().includes(searchTerm);
    const matchesStage = !stageFilter || diagnosis.stage === stageFilter;
    const matchesType = !typeFilter || diagnosis.cancerType === typeFilter;
    return matchesSearch && matchesStage && matchesType;
  });

  const handleDelete = (diagnosis: CancerDiagnosis) => {
    if (
      window.confirm(
        `Are you sure you want to delete the diagnosis for ${diagnosis.patientName}?`
      )
    ) {
      deleteMutation.mutate(diagnosis.id);
    }
  };

  const handleEdit = (diagnosis: CancerDiagnosis) => {
    setSelectedDiagnosis(diagnosis);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setSelectedDiagnosis(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedDiagnosis(undefined);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Cancer Diagnoses</h1>
          <p className="text-gray-600 mt-1">Manage patient cancer diagnoses and staging</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2 inline" />
          New Diagnosis
        </button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Diagnoses</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{diagnoses.length}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Stage I-II</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {diagnoses.filter((d) => d.stage === 'stage_1' || d.stage === 'stage_2').length}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Stage III</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">
            {diagnoses.filter((d) => d.stage === 'stage_3').length}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Stage IV</p>
          <p className="text-2xl font-bold text-red-600 mt-1">
            {diagnoses.filter((d) => d.stage === 'stage_4').length}
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
              value={stageFilter}
              onChange={(e) => setStageFilter(e.target.value)}
              className="input"
            >
              <option value="">All Stages</option>
              <option value="stage_0">Stage 0</option>
              <option value="stage_1">Stage I</option>
              <option value="stage_2">Stage II</option>
              <option value="stage_3">Stage III</option>
              <option value="stage_4">Stage IV</option>
            </select>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className="input"
            >
              <option value="">All Types</option>
              <option value="breast">Breast</option>
              <option value="lung">Lung</option>
              <option value="colorectal">Colorectal</option>
              <option value="prostate">Prostate</option>
              <option value="pancreatic">Pancreatic</option>
            </select>
          </div>
        </div>

        {/* Diagnoses Table */}
        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading diagnoses...</p>
          </div>
        ) : filteredDiagnoses.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Patient
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Cancer Type
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Stage
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Primary Site
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Diagnosis Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Diagnosed By
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredDiagnoses.map((diagnosis) => (
                  <tr key={diagnosis.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm">
                      <div className="font-medium text-gray-900">
                        {diagnosis.patientName || `Patient #${diagnosis.patientId}`}
                      </div>
                      {diagnosis.metastasis && (
                        <span className="text-xs text-red-600">Metastatic</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {getCancerTypeLabel(diagnosis.cancerType)}
                    </td>
                    <td className="px-4 py-3 text-sm">{getStageBadge(diagnosis.stage)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{diagnosis.primarySite}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(diagnosis.diagnosisDate), 'MMM d, yyyy')}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{diagnosis.diagnosedBy}</td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(diagnosis)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(diagnosis)}
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
              {searchTerm || stageFilter || typeFilter
                ? 'No diagnoses found matching your filters.'
                : 'No diagnoses found.'}
            </p>
            <button
              onClick={handleCreate}
              className="mt-4 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
            >
              <PlusIcon className="h-5 w-5 mr-2" />
              Create First Diagnosis
            </button>
          </div>
        )}
      </div>

      {/* Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                {selectedDiagnosis ? 'Edit Diagnosis' : 'New Cancer Diagnosis'}
              </Dialog.Title>
              <DiagnosisForm
                diagnosis={selectedDiagnosis}
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
