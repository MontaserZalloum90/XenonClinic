import { format } from 'date-fns';
import {
  DocumentCheckIcon,
  DocumentTextIcon,
  ClockIcon,
  XCircleIcon,
  ExclamationTriangleIcon,
} from '@heroicons/react/24/outline';
import type { ConsentForm } from '../../types/audiology';
import { ConsentStatus, ConsentType } from '../../types/audiology';

interface ConsentFormCardProps {
  consent: ConsentForm;
  onSign?: () => void;
  onView?: () => void;
  onRevoke?: () => void;
}

const getConsentTypeLabel = (type: ConsentType): string => {
  const labels: Record<ConsentType, string> = {
    [ConsentType.GeneralTreatment]: 'General Treatment Consent',
    [ConsentType.HearingTest]: 'Hearing Test Consent',
    [ConsentType.HearingAidTrial]: 'Hearing Aid Trial Agreement',
    [ConsentType.HearingAidPurchase]: 'Hearing Aid Purchase Agreement',
    [ConsentType.ReleaseOfInformation]: 'Release of Information',
    [ConsentType.Photography]: 'Photography/Recording Consent',
    [ConsentType.Research]: 'Research Participation Consent',
    [ConsentType.Telehealth]: 'Telehealth Consent',
  };
  return labels[type] || type;
};

const getStatusConfig = (status: ConsentStatus) => {
  switch (status) {
    case ConsentStatus.Signed:
      return {
        label: 'Signed',
        className: 'bg-green-100 text-green-800',
        icon: DocumentCheckIcon,
        color: 'green',
      };
    case ConsentStatus.Pending:
      return {
        label: 'Pending',
        className: 'bg-yellow-100 text-yellow-800',
        icon: ClockIcon,
        color: 'yellow',
      };
    case ConsentStatus.Declined:
      return {
        label: 'Declined',
        className: 'bg-red-100 text-red-800',
        icon: XCircleIcon,
        color: 'red',
      };
    case ConsentStatus.Revoked:
      return {
        label: 'Revoked',
        className: 'bg-gray-100 text-gray-800',
        icon: XCircleIcon,
        color: 'gray',
      };
    case ConsentStatus.Expired:
      return {
        label: 'Expired',
        className: 'bg-orange-100 text-orange-800',
        icon: ExclamationTriangleIcon,
        color: 'orange',
      };
    default:
      return {
        label: 'Unknown',
        className: 'bg-gray-100 text-gray-800',
        icon: DocumentTextIcon,
        color: 'gray',
      };
  }
};

export const ConsentFormCard = ({
  consent,
  onSign,
  onView,
  onRevoke,
}: ConsentFormCardProps) => {
  const statusConfig = getStatusConfig(consent.status);
  const StatusIcon = statusConfig.icon;

  return (
    <div className="border rounded-lg p-4 hover:shadow-md transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-3">
        <div className="flex items-center space-x-3">
          <div className={`p-2 rounded-lg bg-${statusConfig.color}-50`}>
            <StatusIcon className={`h-6 w-6 text-${statusConfig.color}-600`} />
          </div>
          <div>
            <h3 className="font-medium text-gray-900">
              {getConsentTypeLabel(consent.consentType)}
            </h3>
            {consent.formVersion && (
              <p className="text-xs text-gray-500">Version: {consent.formVersion}</p>
            )}
          </div>
        </div>
        <span className={`px-2 py-1 rounded text-xs font-medium ${statusConfig.className}`}>
          {statusConfig.label}
        </span>
      </div>

      {/* Details */}
      <div className="space-y-2 text-sm">
        {consent.status === ConsentStatus.Signed && consent.signedAt && (
          <div className="flex justify-between">
            <span className="text-gray-500">Signed</span>
            <span className="text-gray-900">
              {format(new Date(consent.signedAt), 'MMM d, yyyy h:mm a')}
            </span>
          </div>
        )}

        {consent.signedBy && (
          <div className="flex justify-between">
            <span className="text-gray-500">Signed by</span>
            <span className="text-gray-900">{consent.signedBy}</span>
          </div>
        )}

        {consent.signatureMethod && (
          <div className="flex justify-between">
            <span className="text-gray-500">Method</span>
            <span className="text-gray-900 capitalize">{consent.signatureMethod}</span>
          </div>
        )}

        {consent.witnessName && (
          <div className="flex justify-between">
            <span className="text-gray-500">Witness</span>
            <span className="text-gray-900">{consent.witnessName}</span>
          </div>
        )}

        {consent.expiresAt && (
          <div className="flex justify-between">
            <span className="text-gray-500">Expires</span>
            <span className={`${
              new Date(consent.expiresAt) < new Date() ? 'text-red-600' : 'text-gray-900'
            }`}>
              {format(new Date(consent.expiresAt), 'MMM d, yyyy')}
            </span>
          </div>
        )}

        {consent.status === ConsentStatus.Revoked && consent.revokedAt && (
          <div className="mt-2 p-2 bg-red-50 rounded text-red-700 text-xs">
            <p>Revoked on {format(new Date(consent.revokedAt), 'MMM d, yyyy')}</p>
            {consent.revokedReason && <p>Reason: {consent.revokedReason}</p>}
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="flex space-x-2 mt-4">
        {onView && (
          <button
            onClick={onView}
            className="flex-1 px-3 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
          >
            View
          </button>
        )}

        {consent.status === ConsentStatus.Pending && onSign && (
          <button
            onClick={onSign}
            className="flex-1 px-3 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700"
          >
            Sign Now
          </button>
        )}

        {consent.status === ConsentStatus.Signed && onRevoke && (
          <button
            onClick={onRevoke}
            className="flex-1 px-3 py-2 text-sm font-medium text-red-700 bg-red-50 rounded-md hover:bg-red-100"
          >
            Revoke
          </button>
        )}
      </div>
    </div>
  );
};

// Compact list item for consent forms
export const ConsentFormListItem = ({
  consent,
  onClick,
}: {
  consent: ConsentForm;
  onClick?: () => void;
}) => {
  const statusConfig = getStatusConfig(consent.status);
  const StatusIcon = statusConfig.icon;

  return (
    <div
      className={`flex items-center justify-between p-3 border-b hover:bg-gray-50 ${
        onClick ? 'cursor-pointer' : ''
      }`}
      onClick={onClick}
    >
      <div className="flex items-center space-x-3">
        <StatusIcon className={`h-5 w-5 text-${statusConfig.color}-500`} />
        <div>
          <p className="text-sm font-medium text-gray-900">
            {getConsentTypeLabel(consent.consentType)}
          </p>
          <p className="text-xs text-gray-500">
            {consent.status === ConsentStatus.Signed && consent.signedAt
              ? `Signed ${format(new Date(consent.signedAt), 'MMM d, yyyy')}`
              : `Created ${format(new Date(consent.createdAt), 'MMM d, yyyy')}`}
          </p>
        </div>
      </div>
      <span className={`px-2 py-0.5 rounded text-xs font-medium ${statusConfig.className}`}>
        {statusConfig.label}
      </span>
    </div>
  );
};

// Signature pad placeholder component
export const SignaturePad = ({
  onSign,
  onClear,
}: {
  onSign: (signatureData: string) => void;
  onClear: () => void;
}) => {
  return (
    <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
      <p className="text-gray-500 mb-4">Sign here</p>
      <div className="h-32 bg-gray-50 rounded mb-4 flex items-center justify-center">
        <p className="text-gray-400 text-sm">
          [Signature pad would be rendered here using a library like react-signature-canvas]
        </p>
      </div>
      <div className="flex justify-center space-x-3">
        <button
          onClick={onClear}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
        >
          Clear
        </button>
        <button
          onClick={() => onSign('signature_data_placeholder')}
          className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700"
        >
          Accept & Sign
        </button>
      </div>
    </div>
  );
};
