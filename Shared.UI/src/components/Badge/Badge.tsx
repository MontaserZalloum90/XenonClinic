import React from 'react';

export type BadgeVariant =
  | 'default'
  | 'primary'
  | 'secondary'
  | 'success'
  | 'warning'
  | 'danger'
  | 'info';

export type BadgeSize = 'sm' | 'md' | 'lg';

interface BadgeProps {
  children: React.ReactNode;
  variant?: BadgeVariant;
  size?: BadgeSize;
  className?: string;
  dot?: boolean;
}

const variantStyles: Record<BadgeVariant, string> = {
  default: 'bg-gray-100 text-gray-800',
  primary: 'bg-primary-100 text-primary-800',
  secondary: 'bg-gray-100 text-gray-600',
  success: 'bg-green-100 text-green-800',
  warning: 'bg-yellow-100 text-yellow-800',
  danger: 'bg-red-100 text-red-800',
  info: 'bg-blue-100 text-blue-800',
};

const sizeStyles: Record<BadgeSize, string> = {
  sm: 'px-2 py-0.5 text-xs',
  md: 'px-2.5 py-0.5 text-xs',
  lg: 'px-3 py-1 text-sm',
};

const dotColors: Record<BadgeVariant, string> = {
  default: 'bg-gray-400',
  primary: 'bg-primary-500',
  secondary: 'bg-gray-400',
  success: 'bg-green-500',
  warning: 'bg-yellow-500',
  danger: 'bg-red-500',
  info: 'bg-blue-500',
};

export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'default',
  size = 'md',
  className = '',
  dot = false,
}) => {
  return (
    <span
      className={`
        inline-flex items-center gap-1.5 rounded-full font-medium
        ${variantStyles[variant]}
        ${sizeStyles[size]}
        ${className}
      `}
    >
      {dot && (
        <span
          className={`w-1.5 h-1.5 rounded-full ${dotColors[variant]}`}
          aria-hidden="true"
        />
      )}
      {children}
    </span>
  );
};

// Pre-configured status badges for common use cases
export const StatusBadge: React.FC<{
  status: 'active' | 'inactive' | 'pending' | 'completed' | 'cancelled' | 'error';
  className?: string;
}> = ({ status, className }) => {
  const statusConfig: Record<string, { variant: BadgeVariant; label: string }> = {
    active: { variant: 'success', label: 'Active' },
    inactive: { variant: 'secondary', label: 'Inactive' },
    pending: { variant: 'warning', label: 'Pending' },
    completed: { variant: 'info', label: 'Completed' },
    cancelled: { variant: 'danger', label: 'Cancelled' },
    error: { variant: 'danger', label: 'Error' },
  };

  const config = statusConfig[status] || { variant: 'default' as BadgeVariant, label: status };

  return (
    <Badge variant={config.variant} dot className={className}>
      {config.label}
    </Badge>
  );
};
