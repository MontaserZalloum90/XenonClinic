import { useForm, useFieldArray } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import type {
  DentalChart,
  CreateDentalChartRequest,
  ToothCondition,
  ToothConditionType,
} from '../types/dental';
import { format } from 'date-fns';
import { PlusIcon, TrashIcon } from '@heroicons/react/24/outline';

// Mock API functions - Replace with actual API calls
const dentalChartApi = {
  create: async (data: CreateDentalChartRequest) => ({
    data: { id: Date.now(), ...data },
  }),
  update: async (id: number, data: Partial<DentalChart>) => ({
    data: { id, ...data },
  }),
};

interface DentalChartFormProps {
  chart?: DentalChart;
  onSuccess: () => void;
  onCancel: () => void;
}

interface ChartFormData {
  patientId: number;
  chartDate: string;
  teeth: ToothCondition[];
  notes?: string;
}

const getConditionLabel = (condition: ToothConditionType): string => {
  const labels: Record<ToothConditionType, string> = {
    healthy: 'Healthy',
    cavity: 'Cavity',
    filled: 'Filled',
    crown: 'Crown',
    bridge: 'Bridge',
    implant: 'Implant',
    missing: 'Missing',
    root_canal: 'Root Canal',
    fractured: 'Fractured',
    worn: 'Worn',
    decayed: 'Decayed',
  };
  return labels[condition] || condition;
};

export const DentalChartForm = ({ chart, onSuccess, onCancel }: DentalChartFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!chart;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<ChartFormData>({
    defaultValues: chart
      ? {
          patientId: chart.patientId,
          chartDate: format(new Date(chart.chartDate), 'yyyy-MM-dd'),
          teeth: chart.teeth,
          notes: chart.notes || '',
        }
      : {
          chartDate: format(new Date(), 'yyyy-MM-dd'),
          teeth: [],
        },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'teeth',
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateDentalChartRequest) => dentalChartApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dental-charts'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: ChartFormData) =>
      dentalChartApi.update(chart!.id, { ...data, id: chart!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dental-charts'] });
      onSuccess();
    },
  });

  const onSubmit = (data: ChartFormData) => {
    if (isEditing) {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(data as CreateDentalChartRequest);
    }
  };

  const addTooth = () => {
    append({
      toothNumber: 1,
      condition: 'healthy',
      notes: '',
    });
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

        {/* Chart Date */}
        <div>
          <label htmlFor="chartDate" className="block text-sm font-medium text-gray-700 mb-1">
            Chart Date *
          </label>
          <input
            type="date"
            id="chartDate"
            {...register('chartDate', { required: 'Chart date is required' })}
            className="input"
          />
          {errors.chartDate && (
            <p className="mt-1 text-sm text-red-600">{errors.chartDate.message}</p>
          )}
        </div>
      </div>

      {/* Teeth Section */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <label className="block text-sm font-medium text-gray-700">Tooth Conditions</label>
          <button
            type="button"
            onClick={addTooth}
            className="inline-flex items-center px-3 py-1 bg-primary-600 text-white text-sm rounded-md hover:bg-primary-700"
          >
            <PlusIcon className="h-4 w-4 mr-1" />
            Add Tooth
          </button>
        </div>

        {fields.length > 0 ? (
          <div className="space-y-3 max-h-96 overflow-y-auto border border-gray-200 rounded-lg p-4">
            {fields.map((field, index) => (
              <div key={field.id} className="grid grid-cols-12 gap-2 items-start p-3 bg-gray-50 rounded-lg">
                {/* Tooth Number */}
                <div className="col-span-2">
                  <label className="block text-xs text-gray-600 mb-1">Tooth #</label>
                  <input
                    type="number"
                    {...register(`teeth.${index}.toothNumber`, {
                      valueAsNumber: true,
                      min: 1,
                      max: 32,
                      required: true,
                    })}
                    className="input text-sm"
                    placeholder="1-32"
                  />
                </div>

                {/* Condition */}
                <div className="col-span-3">
                  <label className="block text-xs text-gray-600 mb-1">Condition</label>
                  <select
                    {...register(`teeth.${index}.condition`, { required: true })}
                    className="input text-sm"
                  >
                    <option value="healthy">Healthy</option>
                    <option value="cavity">Cavity</option>
                    <option value="filled">Filled</option>
                    <option value="crown">Crown</option>
                    <option value="bridge">Bridge</option>
                    <option value="implant">Implant</option>
                    <option value="missing">Missing</option>
                    <option value="root_canal">Root Canal</option>
                    <option value="fractured">Fractured</option>
                    <option value="worn">Worn</option>
                    <option value="decayed">Decayed</option>
                  </select>
                </div>

                {/* Notes */}
                <div className="col-span-6">
                  <label className="block text-xs text-gray-600 mb-1">Notes</label>
                  <input
                    type="text"
                    {...register(`teeth.${index}.notes`)}
                    className="input text-sm"
                    placeholder="Additional notes..."
                  />
                </div>

                {/* Delete Button */}
                <div className="col-span-1 pt-6">
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    className="text-red-600 hover:text-red-800"
                  >
                    <TrashIcon className="h-5 w-5" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
            <p className="text-gray-500 mb-2">No teeth charted yet</p>
            <button
              type="button"
              onClick={addTooth}
              className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
            >
              <PlusIcon className="h-5 w-5 mr-2" />
              Add First Tooth
            </button>
          </div>
        )}
      </div>

      {/* Visual Tooth Diagram Placeholder */}
      <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center bg-gray-50">
        <svg className="h-16 w-16 text-gray-400 mx-auto mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <p className="text-sm text-gray-500">Visual Dental Chart</p>
        <p className="text-xs text-gray-400 mt-1">
          Interactive tooth diagram will be displayed here (32 teeth layout)
        </p>
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-1">
          General Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={3}
          className="input"
          placeholder="General observations and notes about the dental chart..."
        />
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-3 pt-4 border-t">
        <button
          type="button"
          onClick={onCancel}
          disabled={isPending}
          className="btn btn-secondary"
        >
          Cancel
        </button>
        <button type="submit" disabled={isPending} className="btn btn-primary">
          {isPending ? (
            <div className="flex items-center">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              {isEditing ? 'Updating...' : 'Creating...'}
            </div>
          ) : isEditing ? (
            'Update Chart'
          ) : (
            'Create Chart'
          )}
        </button>
      </div>
    </form>
  );
};
