import { AppointmentStatus } from '../../types/appointment';

interface StatusBadgeProps {
  status: AppointmentStatus;
}

export const StatusBadge = ({ status }: StatusBadgeProps) => {
  const getStatusConfig = (status: AppointmentStatus) => {
    switch (status) {
      case AppointmentStatus.Booked:
        return { label: 'Booked', className: 'bg-blue-100 text-blue-800' };
      case AppointmentStatus.Confirmed:
        return { label: 'Confirmed', className: 'bg-green-100 text-green-800' };
      case AppointmentStatus.CheckedIn:
        return { label: 'Checked In', className: 'bg-purple-100 text-purple-800' };
      case AppointmentStatus.Completed:
        return { label: 'Completed', className: 'bg-gray-100 text-gray-800' };
      case AppointmentStatus.Cancelled:
        return { label: 'Cancelled', className: 'bg-red-100 text-red-800' };
      case AppointmentStatus.NoShow:
        return { label: 'No Show', className: 'bg-yellow-100 text-yellow-800' };
      default:
        return { label: 'Unknown', className: 'bg-gray-100 text-gray-800' };
    }
  };

  const config = getStatusConfig(status);

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.className}`}
    >
      {config.label}
    </span>
  );
};
