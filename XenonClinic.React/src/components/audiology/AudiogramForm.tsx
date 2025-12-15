import { useForm } from 'react-hook-form';
import type { CreateAudiogramRequest, AudiogramDataPoint, TympanogramResult } from '../../types/audiology';
import { AUDIOGRAM_FREQUENCIES } from '../../types/audiology';

interface AudiogramFormProps {
  patientId: number;
  encounterId?: number;
  initialData?: Partial<CreateAudiogramRequest>;
  onSubmit: (data: CreateAudiogramRequest) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

interface FormData {
  testDate: string;
  // Right ear air conduction
  rightAir250: string;
  rightAir500: string;
  rightAir1000: string;
  rightAir2000: string;
  rightAir4000: string;
  rightAir8000: string;
  // Left ear air conduction
  leftAir250: string;
  leftAir500: string;
  leftAir1000: string;
  leftAir2000: string;
  leftAir4000: string;
  leftAir8000: string;
  // Right ear bone conduction (optional)
  rightBone500: string;
  rightBone1000: string;
  rightBone2000: string;
  rightBone4000: string;
  // Left ear bone conduction (optional)
  leftBone500: string;
  leftBone1000: string;
  leftBone2000: string;
  leftBone4000: string;
  // Speech audiometry
  rightSRT: string;
  leftSRT: string;
  rightWRS: string;
  leftWRS: string;
  // Tympanometry
  rightTympType: string;
  leftTympType: string;
  // Notes
  interpretation: string;
  recommendations: string;
  notes: string;
}

export const AudiogramForm = ({
  patientId,
  encounterId,
  initialData,
  onSubmit,
  onCancel,
  isLoading,
}: AudiogramFormProps) => {
  const {
    register,
    handleSubmit,
  } = useForm<FormData>({
    defaultValues: {
      testDate: initialData?.testDate || new Date().toISOString().split('T')[0],
      interpretation: initialData?.interpretation || '',
      recommendations: initialData?.recommendations || '',
      notes: initialData?.notes || '',
    },
  });

  const parseThreshold = (value: string): number | undefined => {
    const num = parseInt(value, 10);
    return isNaN(num) ? undefined : num;
  };

  const buildDataPoints = (
    values: FormData,
    ear: 'right' | 'left',
    type: 'Air' | 'Bone'
  ): AudiogramDataPoint[] => {
    const frequencies = type === 'Air'
      ? AUDIOGRAM_FREQUENCIES
      : [500, 1000, 2000, 4000] as const;

    return frequencies
      .map((freq) => {
        const key = `${ear}${type}${freq}` as keyof FormData;
        const threshold = parseThreshold(values[key] as string);
        if (threshold === undefined) return null;
        return {
          frequency: freq,
          threshold,
          noResponse: threshold >= 120,
        } as AudiogramDataPoint;
      })
      .filter((p): p is AudiogramDataPoint => p !== null);
  };

  const handleFormSubmit = (data: FormData) => {
    const request: CreateAudiogramRequest = {
      patientId,
      encounterId,
      testDate: data.testDate,
      rightEarAir: buildDataPoints(data, 'right', 'Air'),
      leftEarAir: buildDataPoints(data, 'left', 'Air'),
      rightEarBone: buildDataPoints(data, 'right', 'Bone'),
      leftEarBone: buildDataPoints(data, 'left', 'Bone'),
      rightSRT: parseThreshold(data.rightSRT),
      leftSRT: parseThreshold(data.leftSRT),
      rightWRS: parseThreshold(data.rightWRS),
      leftWRS: parseThreshold(data.leftWRS),
      rightTympanogram: data.rightTympType
        ? ({ type: data.rightTympType } as TympanogramResult)
        : undefined,
      leftTympanogram: data.leftTympType
        ? ({ type: data.leftTympType } as TympanogramResult)
        : undefined,
      interpretation: data.interpretation || undefined,
      recommendations: data.recommendations || undefined,
      notes: data.notes || undefined,
    };

    onSubmit(request);
  };

  // eslint-disable-next-line react-hooks/static-components
  const ThresholdInput = ({
    label,
    name,
    error,
  }: {
    label: string;
    name: keyof FormData;
    error?: string;
  }) => (
    <div>
      <label className="block text-xs text-gray-500 mb-1">{label}</label>
      <input
        type="number"
        {...register(name)}
        min="-10"
        max="120"
        step="5"
        className={`w-full px-2 py-1 text-sm border rounded focus:ring-1 focus:ring-primary-500 ${
          error ? 'border-red-500' : 'border-gray-300'
        }`}
        placeholder="dB"
      />
    </div>
  );

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      {/* Test Date */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Test Date
        </label>
        <input
          type="date"
          {...register('testDate', { required: 'Test date is required' })}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
        />
      </div>

      {/* Air Conduction Section */}
      <div className="border rounded-lg p-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">
          Air Conduction Thresholds (dB HL)
        </h3>

        <div className="grid grid-cols-2 gap-6">
          {/* Right Ear */}
          <div>
            <h4 className="text-sm font-medium text-red-600 mb-3 flex items-center">
              <span className="w-3 h-3 rounded-full bg-red-500 mr-2"></span>
              Right Ear
            </h4>
            <div className="grid grid-cols-6 gap-2">
              <ThresholdInput label="250" name="rightAir250" />
              <ThresholdInput label="500" name="rightAir500" />
              <ThresholdInput label="1K" name="rightAir1000" />
              <ThresholdInput label="2K" name="rightAir2000" />
              <ThresholdInput label="4K" name="rightAir4000" />
              <ThresholdInput label="8K" name="rightAir8000" />
            </div>
          </div>

          {/* Left Ear */}
          <div>
            <h4 className="text-sm font-medium text-blue-600 mb-3 flex items-center">
              <span className="text-blue-500 font-bold mr-2">×</span>
              Left Ear
            </h4>
            <div className="grid grid-cols-6 gap-2">
              <ThresholdInput label="250" name="leftAir250" />
              <ThresholdInput label="500" name="leftAir500" />
              <ThresholdInput label="1K" name="leftAir1000" />
              <ThresholdInput label="2K" name="leftAir2000" />
              <ThresholdInput label="4K" name="leftAir4000" />
              <ThresholdInput label="8K" name="leftAir8000" />
            </div>
          </div>
        </div>
      </div>

      {/* Bone Conduction Section */}
      <div className="border rounded-lg p-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">
          Bone Conduction Thresholds (dB HL) - Optional
        </h3>

        <div className="grid grid-cols-2 gap-6">
          {/* Right Ear Bone */}
          <div>
            <h4 className="text-sm font-medium text-red-600 mb-3">Right Ear {'<'}</h4>
            <div className="grid grid-cols-4 gap-2">
              <ThresholdInput label="500" name="rightBone500" />
              <ThresholdInput label="1K" name="rightBone1000" />
              <ThresholdInput label="2K" name="rightBone2000" />
              <ThresholdInput label="4K" name="rightBone4000" />
            </div>
          </div>

          {/* Left Ear Bone */}
          <div>
            <h4 className="text-sm font-medium text-blue-600 mb-3">Left Ear {'>'}</h4>
            <div className="grid grid-cols-4 gap-2">
              <ThresholdInput label="500" name="leftBone500" />
              <ThresholdInput label="1K" name="leftBone1000" />
              <ThresholdInput label="2K" name="leftBone2000" />
              <ThresholdInput label="4K" name="leftBone4000" />
            </div>
          </div>
        </div>
      </div>

      {/* Speech Audiometry */}
      <div className="border rounded-lg p-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Speech Audiometry</h3>

        <div className="grid grid-cols-2 gap-6">
          <div>
            <h4 className="text-sm font-medium text-red-600 mb-3">Right Ear</h4>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 mb-1">SRT (dB)</label>
                <input
                  type="number"
                  {...register('rightSRT')}
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                  placeholder="SRT"
                />
              </div>
              <div>
                <label className="block text-xs text-gray-500 mb-1">WRS (%)</label>
                <input
                  type="number"
                  {...register('rightWRS')}
                  min="0"
                  max="100"
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                  placeholder="WRS"
                />
              </div>
            </div>
          </div>

          <div>
            <h4 className="text-sm font-medium text-blue-600 mb-3">Left Ear</h4>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 mb-1">SRT (dB)</label>
                <input
                  type="number"
                  {...register('leftSRT')}
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                  placeholder="SRT"
                />
              </div>
              <div>
                <label className="block text-xs text-gray-500 mb-1">WRS (%)</label>
                <input
                  type="number"
                  {...register('leftWRS')}
                  min="0"
                  max="100"
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                  placeholder="WRS"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tympanometry */}
      <div className="border rounded-lg p-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Tympanometry</h3>

        <div className="grid grid-cols-2 gap-6">
          <div>
            <label className="block text-sm text-gray-600 mb-1">Right Ear Type</label>
            <select
              {...register('rightTympType')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">Select type...</option>
              <option value="A">Type A (Normal)</option>
              <option value="As">Type As (Stiff)</option>
              <option value="Ad">Type Ad (Flaccid)</option>
              <option value="B">Type B (Flat)</option>
              <option value="C">Type C (Negative pressure)</option>
            </select>
          </div>

          <div>
            <label className="block text-sm text-gray-600 mb-1">Left Ear Type</label>
            <select
              {...register('leftTympType')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">Select type...</option>
              <option value="A">Type A (Normal)</option>
              <option value="As">Type As (Stiff)</option>
              <option value="Ad">Type Ad (Flaccid)</option>
              <option value="B">Type B (Flat)</option>
              <option value="C">Type C (Negative pressure)</option>
            </select>
          </div>
        </div>
      </div>

      {/* Notes Section */}
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Interpretation
          </label>
          <textarea
            {...register('interpretation')}
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
            placeholder="Clinical interpretation of results..."
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Recommendations
          </label>
          <textarea
            {...register('recommendations')}
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
            placeholder="Recommended next steps..."
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Additional Notes
          </label>
          <textarea
            {...register('notes')}
            rows={2}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
            placeholder="Any additional notes..."
          />
        </div>
      </div>

      {/* Form Actions */}
      <div className="flex justify-end space-x-3 pt-4 border-t">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700 disabled:opacity-50"
        >
          {isLoading ? 'Saving...' : 'Save Audiogram'}
        </button>
      </div>
    </form>
  );
};
