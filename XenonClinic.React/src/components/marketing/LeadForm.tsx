import { useForm } from "react-hook-form";
import { LeadSource, LeadPriority } from "../../types/marketing";
import type { CreateLeadRequest } from "../../types/marketing";

interface LeadFormProps {
  initialData?: Partial<CreateLeadRequest>;
  onSubmit: (data: CreateLeadRequest) => void;
  onCancel: () => void;
}

const leadSources = [
  { value: LeadSource.Website, label: "Website" },
  { value: LeadSource.Referral, label: "Referral" },
  { value: LeadSource.SocialMedia, label: "Social Media" },
  { value: LeadSource.Advertising, label: "Advertising" },
  { value: LeadSource.Event, label: "Event" },
  { value: LeadSource.WalkIn, label: "Walk-in" },
  { value: LeadSource.Phone, label: "Phone Inquiry" },
  { value: LeadSource.Email, label: "Email Inquiry" },
  { value: LeadSource.Partner, label: "Partner" },
  { value: LeadSource.Campaign, label: "Marketing Campaign" },
  { value: LeadSource.Other, label: "Other" },
];

const priorities = [
  { value: LeadPriority.Low, label: "Low" },
  { value: LeadPriority.Medium, label: "Medium" },
  { value: LeadPriority.High, label: "High" },
  { value: LeadPriority.Urgent, label: "Urgent" },
];

export const LeadForm = ({
  initialData,
  onSubmit,
  onCancel,
}: LeadFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateLeadRequest>({
    defaultValues: {
      firstName: "",
      lastName: "",
      source: LeadSource.Website,
      priority: LeadPriority.Medium,
      ...initialData,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Contact Information */}
      <div>
        <h3 className="text-lg font-medium text-gray-900 mb-4">
          Contact Information
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* First Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              First Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              {...register("firstName", { required: "First name is required" })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
            {errors.firstName && (
              <p className="mt-1 text-sm text-red-600">
                {errors.firstName.message}
              </p>
            )}
          </div>

          {/* Last Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Last Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              {...register("lastName", { required: "Last name is required" })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
            {errors.lastName && (
              <p className="mt-1 text-sm text-red-600">
                {errors.lastName.message}
              </p>
            )}
          </div>

          {/* Email */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              type="email"
              {...register("email", {
                pattern: {
                  value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                  message: "Invalid email address",
                },
              })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
            {errors.email && (
              <p className="mt-1 text-sm text-red-600">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Phone */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Phone Number
            </label>
            <input
              type="tel"
              {...register("phoneNumber")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
          </div>

          {/* Mobile */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Mobile Number
            </label>
            <input
              type="tel"
              {...register("mobileNumber")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
          </div>
        </div>
      </div>

      {/* Company Information (optional) */}
      <div>
        <h3 className="text-lg font-medium text-gray-900 mb-4">
          Company Information (Optional)
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Company Name
            </label>
            <input
              type="text"
              {...register("companyName")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Job Title
            </label>
            <input
              type="text"
              {...register("jobTitle")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
          </div>
        </div>
      </div>

      {/* Lead Details */}
      <div>
        <h3 className="text-lg font-medium text-gray-900 mb-4">Lead Details</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Source */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Lead Source <span className="text-red-500">*</span>
            </label>
            <select
              {...register("source", { required: "Lead source is required" })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            >
              {leadSources.map((source) => (
                <option key={source.value} value={source.value}>
                  {source.label}
                </option>
              ))}
            </select>
          </div>

          {/* Source Details */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Source Details
            </label>
            <input
              type="text"
              {...register("sourceDetails")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              placeholder="e.g., Google Ads, Facebook, Dr. Smith referral"
            />
          </div>

          {/* Priority */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Priority
            </label>
            <select
              {...register("priority")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            >
              {priorities.map((priority) => (
                <option key={priority.value} value={priority.value}>
                  {priority.label}
                </option>
              ))}
            </select>
          </div>

          {/* Interested In */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Interested In
            </label>
            <input
              type="text"
              {...register("interestedIn")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              placeholder="e.g., Hearing aids, Hearing test, Consultation"
            />
          </div>

          {/* Estimated Value */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Estimated Value (AED)
            </label>
            <input
              type="number"
              step="0.01"
              {...register("estimatedValue", { valueAsNumber: true })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              placeholder="0.00"
            />
          </div>

          {/* Tags */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Tags
            </label>
            <input
              type="text"
              {...register("tags")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              placeholder="Comma-separated tags"
            />
          </div>
        </div>
      </div>

      {/* Follow-up */}
      <div>
        <h3 className="text-lg font-medium text-gray-900 mb-4">Follow-up</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Next Follow-up Date
            </label>
            <input
              type="date"
              {...register("nextFollowUpDate")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Follow-up Notes
            </label>
            <input
              type="text"
              {...register("nextFollowUpNotes")}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              placeholder="What to discuss in follow-up"
            />
          </div>
        </div>
      </div>

      {/* Notes */}
      <div>
        <label className="block text-sm font-medium text-gray-700">Notes</label>
        <textarea
          {...register("notes")}
          rows={3}
          className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
          placeholder="Additional notes about this lead"
        />
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
          {isSubmitting ? "Creating..." : "Create Lead"}
        </button>
      </div>
    </form>
  );
};
