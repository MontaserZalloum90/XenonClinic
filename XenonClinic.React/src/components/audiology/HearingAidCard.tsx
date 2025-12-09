import { format, differenceInDays, isPast } from 'date-fns';
import {
  DevicePhoneMobileIcon,
  WrenchScrewdriverIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
} from '@heroicons/react/24/outline';
import type { HearingAid } from '../../types/audiology';
import { HearingAidStatus, EarSide } from '../../types/audiology';

interface HearingAidCardProps {
  hearingAid: HearingAid;
  onEdit?: () => void;
  onViewFittings?: () => void;
  onAddAdjustment?: () => void;
}

const getStatusConfig = (status: HearingAidStatus) => {
  switch (status) {
    case HearingAidStatus.Active:
      return { label: 'Active', className: 'bg-green-100 text-green-800', icon: CheckCircleIcon };
    case HearingAidStatus.InRepair:
      return { label: 'In Repair', className: 'bg-yellow-100 text-yellow-800', icon: WrenchScrewdriverIcon };
    case HearingAidStatus.Replaced:
      return { label: 'Replaced', className: 'bg-gray-100 text-gray-800', icon: DevicePhoneMobileIcon };
    case HearingAidStatus.Lost:
      return { label: 'Lost', className: 'bg-red-100 text-red-800', icon: ExclamationTriangleIcon };
    case HearingAidStatus.Returned:
      return { label: 'Returned', className: 'bg-blue-100 text-blue-800', icon: ClockIcon };
    default:
      return { label: 'Unknown', className: 'bg-gray-100 text-gray-800', icon: DevicePhoneMobileIcon };
  }
};

export const HearingAidCard = ({
  hearingAid,
  onEdit,
  onViewFittings,
  onAddAdjustment,
}: HearingAidCardProps) => {
  const statusConfig = getStatusConfig(hearingAid.status);
  const StatusIcon = statusConfig.icon;

  const warrantyEndDate = new Date(hearingAid.warrantyEndDate);
  const daysUntilWarrantyExpires = differenceInDays(warrantyEndDate, new Date());
  const warrantyExpired = isPast(warrantyEndDate);
  const warrantyExpiringSoon = !warrantyExpired && daysUntilWarrantyExpires <= 30;

  return (
    <div className={`border rounded-lg p-4 ${
      hearingAid.ear === EarSide.Right ? 'border-l-4 border-l-red-500' : 'border-l-4 border-l-blue-500'
    }`}>
      {/* Header */}
      <div className="flex justify-between items-start mb-3">
        <div>
          <div className="flex items-center space-x-2">
            <span className={`px-2 py-0.5 rounded text-xs font-medium ${
              hearingAid.ear === EarSide.Right ? 'bg-red-100 text-red-800' : 'bg-blue-100 text-blue-800'
            }`}>
              {hearingAid.ear === EarSide.Right ? 'Right Ear' : 'Left Ear'}
            </span>
            <span className={`px-2 py-0.5 rounded text-xs font-medium ${statusConfig.className}`}>
              <StatusIcon className="h-3 w-3 inline mr-1" />
              {statusConfig.label}
            </span>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mt-1">
            {hearingAid.manufacturer} {hearingAid.model}
          </h3>
          <p className="text-sm text-gray-500">{hearingAid.style}</p>
        </div>

        {onEdit && (
          <button
            onClick={onEdit}
            className="text-primary-600 hover:text-primary-700 text-sm font-medium"
          >
            Edit
          </button>
        )}
      </div>

      {/* Details */}
      <div className="grid grid-cols-2 gap-4 text-sm mb-4">
        <div>
          <p className="text-gray-500">Serial Number</p>
          <p className="font-mono text-gray-900">{hearingAid.serialNumber}</p>
        </div>
        <div>
          <p className="text-gray-500">Technology Level</p>
          <p className="text-gray-900">{hearingAid.technologyLevel || 'N/A'}</p>
        </div>
        <div>
          <p className="text-gray-500">Purchase Date</p>
          <p className="text-gray-900">{format(new Date(hearingAid.purchaseDate), 'MMM d, yyyy')}</p>
        </div>
        <div>
          <p className="text-gray-500">Last Programming</p>
          <p className="text-gray-900">
            {hearingAid.lastProgrammingDate
              ? format(new Date(hearingAid.lastProgrammingDate), 'MMM d, yyyy')
              : 'N/A'}
          </p>
        </div>
      </div>

      {/* Warranty Status */}
      <div className={`rounded-lg p-3 mb-4 ${
        warrantyExpired
          ? 'bg-red-50 border border-red-200'
          : warrantyExpiringSoon
          ? 'bg-yellow-50 border border-yellow-200'
          : 'bg-green-50 border border-green-200'
      }`}>
        <div className="flex items-center justify-between">
          <div>
            <p className={`text-sm font-medium ${
              warrantyExpired ? 'text-red-800' : warrantyExpiringSoon ? 'text-yellow-800' : 'text-green-800'
            }`}>
              {warrantyExpired ? 'Warranty Expired' : 'Warranty Active'}
              {hearingAid.warrantyExtended && ' (Extended)'}
            </p>
            <p className={`text-xs ${
              warrantyExpired ? 'text-red-600' : warrantyExpiringSoon ? 'text-yellow-600' : 'text-green-600'
            }`}>
              {warrantyExpired
                ? `Expired ${format(warrantyEndDate, 'MMM d, yyyy')}`
                : `Expires ${format(warrantyEndDate, 'MMM d, yyyy')} (${daysUntilWarrantyExpires} days)`}
            </p>
          </div>
          {(warrantyExpired || warrantyExpiringSoon) && (
            <ExclamationTriangleIcon className={`h-5 w-5 ${
              warrantyExpired ? 'text-red-500' : 'text-yellow-500'
            }`} />
          )}
        </div>
      </div>

      {/* Fittings and Adjustments Summary */}
      <div className="flex items-center justify-between text-sm text-gray-500 mb-4">
        <span>
          {hearingAid.fittings?.length || 0} Fitting{hearingAid.fittings?.length !== 1 ? 's' : ''}
        </span>
        <span>
          {hearingAid.adjustments?.length || 0} Adjustment{hearingAid.adjustments?.length !== 1 ? 's' : ''}
        </span>
      </div>

      {/* Actions */}
      <div className="flex space-x-2">
        {onViewFittings && (
          <button
            onClick={onViewFittings}
            className="flex-1 px-3 py-2 text-sm font-medium text-primary-700 bg-primary-50 rounded-md hover:bg-primary-100"
          >
            View History
          </button>
        )}
        {onAddAdjustment && hearingAid.status === HearingAidStatus.Active && (
          <button
            onClick={onAddAdjustment}
            className="flex-1 px-3 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700"
          >
            Add Adjustment
          </button>
        )}
      </div>

      {/* Notes */}
      {hearingAid.notes && (
        <div className="mt-4 p-2 bg-gray-50 rounded text-sm text-gray-600">
          <p className="font-medium text-gray-700">Notes:</p>
          <p>{hearingAid.notes}</p>
        </div>
      )}
    </div>
  );
};

// Mini card for compact display
export const HearingAidMiniCard = ({ hearingAid }: { hearingAid: HearingAid }) => {
  const statusConfig = getStatusConfig(hearingAid.status);

  return (
    <div className={`flex items-center justify-between p-2 border rounded ${
      hearingAid.ear === EarSide.Right ? 'border-l-2 border-l-red-500' : 'border-l-2 border-l-blue-500'
    }`}>
      <div className="flex items-center space-x-2">
        <DevicePhoneMobileIcon className="h-5 w-5 text-gray-400" />
        <div>
          <p className="text-sm font-medium text-gray-900">
            {hearingAid.manufacturer} {hearingAid.model}
          </p>
          <p className="text-xs text-gray-500">
            {hearingAid.ear === EarSide.Right ? 'R' : 'L'} - {hearingAid.serialNumber}
          </p>
        </div>
      </div>
      <span className={`px-2 py-0.5 rounded text-xs font-medium ${statusConfig.className}`}>
        {statusConfig.label}
      </span>
    </div>
  );
};
