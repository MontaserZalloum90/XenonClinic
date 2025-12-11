import { useForm } from 'react-hook-form';
import { CampaignType } from '../../types/marketing';
import type { CreateCampaignRequest } from '../../types/marketing';

interface CampaignFormProps {
  initialData?: Partial<CreateCampaignRequest>;
  onSubmit: (data: CreateCampaignRequest) => void;
  onCancel: () => void;
}

const campaignTypes = [
  { value: CampaignType.Email, label: 'Email Marketing' },
  { value: CampaignType.Social, label: 'Social Media' },
  { value: CampaignType.SMS, label: 'SMS/Text Message' },
  { value: CampaignType.Event, label: 'Event/Webinar' },
  { value: CampaignType.Referral, label: 'Referral Program' },
  { value: CampaignType.Recall, label: 'Patient Recall' },
  { value: CampaignType.Seasonal, label: 'Seasonal Promotion' },
  { value: CampaignType.Digital, label: 'Digital Advertising' },
  { value: CampaignType.Print, label: 'Print/Traditional' },
  { value: CampaignType.Other, label: 'Other' },
];

export const CampaignForm = ({ initialData, onSubmit, onCancel }: CampaignFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateCampaignRequest>({
    defaultValues: {
      name: '',
      description: '',
      type: CampaignType.Email,
      startDate: new Date().toISOString().split('T')[0],
      ...initialData,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Campaign Name */}
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700">
            Campaign Name <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            {...register('name', { required: 'Campaign name is required' })}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="Enter campaign name"
          />
          {errors.name && (
            <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
          )}
        </div>

        {/* Campaign Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700">
            Campaign Type <span className="text-red-500">*</span>
          </label>
          <select
            {...register('type', { required: 'Campaign type is required' })}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
          >
            {campaignTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>

        {/* Target Audience */}
        <div>
          <label className="block text-sm font-medium text-gray-700">Target Audience</label>
          <input
            type="text"
            {...register('targetAudience')}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="e.g., New patients, Hearing aid users"
          />
        </div>

        {/* Start Date */}
        <div>
          <label className="block text-sm font-medium text-gray-700">
            Start Date <span className="text-red-500">*</span>
          </label>
          <input
            type="date"
            {...register('startDate', { required: 'Start date is required' })}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
          />
          {errors.startDate && (
            <p className="mt-1 text-sm text-red-600">{errors.startDate.message}</p>
          )}
        </div>

        {/* End Date */}
        <div>
          <label className="block text-sm font-medium text-gray-700">End Date</label>
          <input
            type="date"
            {...register('endDate')}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
          />
        </div>

        {/* Budget */}
        <div>
          <label className="block text-sm font-medium text-gray-700">Budget (AED)</label>
          <input
            type="number"
            step="0.01"
            {...register('budget', { valueAsNumber: true })}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="0.00"
          />
        </div>

        {/* Target Leads */}
        <div>
          <label className="block text-sm font-medium text-gray-700">Target Leads</label>
          <input
            type="number"
            {...register('targetLeads', { valueAsNumber: true })}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="0"
          />
        </div>

        {/* Target Conversions */}
        <div>
          <label className="block text-sm font-medium text-gray-700">Target Conversions</label>
          <input
            type="number"
            {...register('targetConversions', { valueAsNumber: true })}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="0"
          />
        </div>

        {/* Tags */}
        <div>
          <label className="block text-sm font-medium text-gray-700">Tags</label>
          <input
            type="text"
            {...register('tags')}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="Comma-separated tags"
          />
        </div>

        {/* Subject */}
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700">Subject Line</label>
          <input
            type="text"
            {...register('subject')}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="Email/SMS subject line"
          />
        </div>

        {/* Call to Action */}
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700">Call to Action</label>
          <input
            type="text"
            {...register('callToAction')}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="e.g., Book your free consultation today!"
          />
        </div>

        {/* Description */}
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700">Description</label>
          <textarea
            {...register('description')}
            rows={3}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="Campaign description and goals"
          />
        </div>

        {/* Content */}
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700">Campaign Content</label>
          <textarea
            {...register('content')}
            rows={5}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            placeholder="Main campaign message/content"
          />
        </div>
      </div>

      {/* Form Actions */}
      <div className="flex justify-end space-x-3 pt-4 border-t">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50"
        >
          {isSubmitting ? 'Creating...' : 'Create Campaign'}
        </button>
      </div>
    </form>
  );
};
