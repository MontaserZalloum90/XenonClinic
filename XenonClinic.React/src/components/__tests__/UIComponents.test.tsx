import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { useState } from 'react';

// Mock LoadingSkeleton component
const MockLoadingSkeleton = ({ type = 'text', count = 1, width, height }: {
  type?: 'text' | 'avatar' | 'card' | 'table' | 'form';
  count?: number;
  width?: string;
  height?: string;
}) => (
  <div data-testid="loading-skeleton" data-type={type}>
    {Array.from({ length: count }).map((_, i) => (
      <div key={i} className="skeleton-item animate-pulse bg-gray-200" style={{ width, height }} />
    ))}
  </div>
);

// Mock EmptyState component
const MockEmptyState = ({
  title,
  description,
  icon,
  actionLabel,
  onAction
}: {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  actionLabel?: string;
  onAction?: () => void;
}) => (
  <div data-testid="empty-state" className="text-center py-8">
    {icon && <div data-testid="empty-state-icon">{icon}</div>}
    <h3 data-testid="empty-state-title" className="text-lg font-medium">{title}</h3>
    {description && <p data-testid="empty-state-description" className="text-gray-500">{description}</p>}
    {actionLabel && (
      <button
        data-testid="empty-state-action"
        onClick={onAction}
        className="mt-4 btn btn-primary"
      >
        {actionLabel}
      </button>
    )}
  </div>
);

// Mock Badge component
const MockBadge = ({
  children,
  variant = 'default',
  size = 'md'
}: {
  children: React.ReactNode;
  variant?: 'default' | 'success' | 'warning' | 'error' | 'info';
  size?: 'sm' | 'md' | 'lg';
}) => {
  const variantClasses = {
    default: 'bg-gray-100 text-gray-800',
    success: 'bg-green-100 text-green-800',
    warning: 'bg-yellow-100 text-yellow-800',
    error: 'bg-red-100 text-red-800',
    info: 'bg-blue-100 text-blue-800'
  };

  const sizeClasses = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-0.5 text-sm',
    lg: 'px-3 py-1 text-base'
  };

  return (
    <span
      data-testid="badge"
      data-variant={variant}
      data-size={size}
      className={`inline-flex items-center rounded-full font-medium ${variantClasses[variant]} ${sizeClasses[size]}`}
    >
      {children}
    </span>
  );
};

// Mock Toast component
const MockToast = ({
  message,
  type = 'info',
  isVisible = true,
  onClose,
  duration = 5000
}: {
  message: string;
  type?: 'success' | 'error' | 'warning' | 'info';
  isVisible?: boolean;
  onClose?: () => void;
  duration?: number;
}) => {
  if (!isVisible) return null;

  const typeClasses = {
    success: 'bg-green-500',
    error: 'bg-red-500',
    warning: 'bg-yellow-500',
    info: 'bg-blue-500'
  };

  return (
    <div
      data-testid="toast"
      data-type={type}
      className={`fixed bottom-4 right-4 p-4 rounded-lg text-white ${typeClasses[type]}`}
    >
      <span data-testid="toast-message">{message}</span>
      <button data-testid="toast-close" onClick={onClose}>×</button>
    </div>
  );
};

// Mock ConfirmDialog component
const MockConfirmDialog = ({
  isOpen = false,
  title,
  message,
  onConfirm,
  onCancel,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  variant = 'default'
}: {
  isOpen?: boolean;
  title: string;
  message: string;
  onConfirm?: () => void;
  onCancel?: () => void;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'default' | 'danger';
}) => {
  if (!isOpen) return null;

  return (
    <div data-testid="confirm-dialog" role="dialog">
      <div data-testid="confirm-dialog-overlay" onClick={onCancel} />
      <div data-testid="confirm-dialog-content">
        <h3 data-testid="confirm-dialog-title">{title}</h3>
        <p data-testid="confirm-dialog-message">{message}</p>
        <div data-testid="confirm-dialog-actions">
          <button
            data-testid="confirm-dialog-cancel"
            onClick={onCancel}
          >
            {cancelLabel}
          </button>
          <button
            data-testid="confirm-dialog-confirm"
            data-variant={variant}
            onClick={onConfirm}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
};

// Mock Pagination component
const MockPagination = ({
  currentPage = 1,
  totalPages = 1,
  onPageChange,
  siblingCount = 1
}: {
  currentPage?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
  siblingCount?: number;
}) => {
  const pages = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <nav data-testid="pagination" aria-label="Pagination">
      <button
        data-testid="pagination-prev"
        onClick={() => onPageChange?.(currentPage - 1)}
        disabled={currentPage <= 1}
      >
        Previous
      </button>
      {pages.map(page => (
        <button
          key={page}
          data-testid={`pagination-page-${page}`}
          onClick={() => onPageChange?.(page)}
          aria-current={page === currentPage ? 'page' : undefined}
          className={page === currentPage ? 'active' : ''}
        >
          {page}
        </button>
      ))}
      <button
        data-testid="pagination-next"
        onClick={() => onPageChange?.(currentPage + 1)}
        disabled={currentPage >= totalPages}
      >
        Next
      </button>
    </nav>
  );
};

// Mock DataTable component
const MockDataTable = ({
  columns = [],
  data = [],
  onRowClick,
  isLoading = false,
  emptyMessage = 'No data available',
  sortable = false,
  selectable = false,
  onSort,
  onSelect
}: {
  columns?: Array<{ key: string; label: string; sortable?: boolean }>;
  data?: Record<string, unknown>[];
  onRowClick?: (row: Record<string, unknown>) => void;
  isLoading?: boolean;
  emptyMessage?: string;
  sortable?: boolean;
  selectable?: boolean;
  onSort?: (key: string, direction: 'asc' | 'desc') => void;
  onSelect?: (selected: Record<string, unknown>[]) => void;
}) => {
  if (isLoading) {
    return <div data-testid="data-table-loading">Loading...</div>;
  }

  if (data.length === 0) {
    return <div data-testid="data-table-empty">{emptyMessage}</div>;
  }

  return (
    <table data-testid="data-table">
      <thead>
        <tr>
          {selectable && <th><input type="checkbox" data-testid="select-all" /></th>}
          {columns.map(col => (
            <th key={col.key} data-testid={`header-${col.key}`}>
              {col.label}
              {sortable && col.sortable && (
                <button data-testid={`sort-${col.key}`} onClick={() => onSort?.(col.key, 'asc')}>↕</button>
              )}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {data.map((row, i) => (
          <tr key={i} data-testid={`row-${i}`} onClick={() => onRowClick?.(row)}>
            {selectable && <td><input type="checkbox" data-testid={`select-${i}`} /></td>}
            {columns.map(col => (
              <td key={col.key} data-testid={`cell-${i}-${col.key}`}>{row[col.key]}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
};

describe('LoadingSkeleton Component Tests', () => {
  // Rendering Tests
  describe('Rendering', () => {
    it('renders the loading skeleton', () => {
      render(<MockLoadingSkeleton />);
      expect(screen.getByTestId('loading-skeleton')).toBeInTheDocument();
    });

    it('renders with default text type', () => {
      render(<MockLoadingSkeleton />);
      expect(screen.getByTestId('loading-skeleton')).toHaveAttribute('data-type', 'text');
    });

    it('renders with avatar type', () => {
      render(<MockLoadingSkeleton type="avatar" />);
      expect(screen.getByTestId('loading-skeleton')).toHaveAttribute('data-type', 'avatar');
    });

    it('renders with card type', () => {
      render(<MockLoadingSkeleton type="card" />);
      expect(screen.getByTestId('loading-skeleton')).toHaveAttribute('data-type', 'card');
    });

    it('renders with table type', () => {
      render(<MockLoadingSkeleton type="table" />);
      expect(screen.getByTestId('loading-skeleton')).toHaveAttribute('data-type', 'table');
    });

    it('renders with form type', () => {
      render(<MockLoadingSkeleton type="form" />);
      expect(screen.getByTestId('loading-skeleton')).toHaveAttribute('data-type', 'form');
    });

    it('renders single skeleton item by default', () => {
      render(<MockLoadingSkeleton />);
      const items = screen.getByTestId('loading-skeleton').querySelectorAll('.skeleton-item');
      expect(items.length).toBe(1);
    });

    it('renders multiple skeleton items when count is specified', () => {
      render(<MockLoadingSkeleton count={5} />);
      const items = screen.getByTestId('loading-skeleton').querySelectorAll('.skeleton-item');
      expect(items.length).toBe(5);
    });

    it('renders 10 skeleton items', () => {
      render(<MockLoadingSkeleton count={10} />);
      const items = screen.getByTestId('loading-skeleton').querySelectorAll('.skeleton-item');
      expect(items.length).toBe(10);
    });

    it('applies custom width', () => {
      render(<MockLoadingSkeleton width="200px" />);
      const item = screen.getByTestId('loading-skeleton').querySelector('.skeleton-item');
      expect(item).toHaveStyle({ width: '200px' });
    });

    it('applies custom height', () => {
      render(<MockLoadingSkeleton height="50px" />);
      const item = screen.getByTestId('loading-skeleton').querySelector('.skeleton-item');
      expect(item).toHaveStyle({ height: '50px' });
    });

    it('applies animation class', () => {
      render(<MockLoadingSkeleton />);
      const item = screen.getByTestId('loading-skeleton').querySelector('.skeleton-item');
      expect(item).toHaveClass('animate-pulse');
    });
  });
  // End
});

describe('EmptyState Component Tests', () => {
  // Rendering Tests
  describe('Rendering', () => {
    it('renders the empty state', () => {
      render(<MockEmptyState title="No Data" />);
      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
    });

    it('renders title', () => {
      render(<MockEmptyState title="No patients found" />);
      expect(screen.getByTestId('empty-state-title')).toHaveTextContent('No patients found');
    });

    it('renders description when provided', () => {
      render(<MockEmptyState title="No Data" description="Add your first patient to get started" />);
      expect(screen.getByTestId('empty-state-description')).toHaveTextContent('Add your first patient to get started');
    });

    it('does not render description when not provided', () => {
      render(<MockEmptyState title="No Data" />);
      expect(screen.queryByTestId('empty-state-description')).not.toBeInTheDocument();
    });

    it('renders icon when provided', () => {
      render(<MockEmptyState title="No Data" icon={<span>📋</span>} />);
      expect(screen.getByTestId('empty-state-icon')).toBeInTheDocument();
    });

    it('does not render icon when not provided', () => {
      render(<MockEmptyState title="No Data" />);
      expect(screen.queryByTestId('empty-state-icon')).not.toBeInTheDocument();
    });

    it('renders action button when actionLabel is provided', () => {
      render(<MockEmptyState title="No Data" actionLabel="Add Patient" />);
      expect(screen.getByTestId('empty-state-action')).toHaveTextContent('Add Patient');
    });

    it('does not render action button when actionLabel is not provided', () => {
      render(<MockEmptyState title="No Data" />);
      expect(screen.queryByTestId('empty-state-action')).not.toBeInTheDocument();
    });
  });
  // End

  // Interaction Tests
  describe('Interactions', () => {
    it('calls onAction when action button is clicked', async () => {
      const onAction = vi.fn();
      const user = userEvent.setup();
      render(<MockEmptyState title="No Data" actionLabel="Add Patient" onAction={onAction} />);
      await user.click(screen.getByTestId('empty-state-action'));
      expect(onAction).toHaveBeenCalledTimes(1);
    });

    it('action button is clickable', async () => {
      const onAction = vi.fn();
      const user = userEvent.setup();
      render(<MockEmptyState title="No Data" actionLabel="Click Me" onAction={onAction} />);
      const button = screen.getByTestId('empty-state-action');
      expect(button).not.toBeDisabled();
    });
  });
  // End

  // Content Tests
  describe('Different Content Types', () => {
    it('displays empty patients message', () => {
      render(<MockEmptyState title="No patients found" description="Start by adding a new patient" />);
      expect(screen.getByText('No patients found')).toBeInTheDocument();
    });

    it('displays empty appointments message', () => {
      render(<MockEmptyState title="No appointments" description="Schedule your first appointment" />);
      expect(screen.getByText('No appointments')).toBeInTheDocument();
    });

    it('displays empty lab results message', () => {
      render(<MockEmptyState title="No lab results" description="Order lab tests for patients" />);
      expect(screen.getByText('No lab results')).toBeInTheDocument();
    });

    it('displays empty invoices message', () => {
      render(<MockEmptyState title="No invoices" description="Generate invoices for services" />);
      expect(screen.getByText('No invoices')).toBeInTheDocument();
    });

    it('displays empty inventory message', () => {
      render(<MockEmptyState title="No items in inventory" description="Add items to your inventory" />);
      expect(screen.getByText('No items in inventory')).toBeInTheDocument();
    });
  });
  // End
});

describe('Badge Component Tests', () => {
  // Rendering Tests
  describe('Rendering', () => {
    it('renders the badge', () => {
      render(<MockBadge>Test</MockBadge>);
      expect(screen.getByTestId('badge')).toBeInTheDocument();
    });

    it('renders children content', () => {
      render(<MockBadge>Active</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveTextContent('Active');
    });

    it('renders with default variant', () => {
      render(<MockBadge>Default</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-variant', 'default');
    });

    it('renders with success variant', () => {
      render(<MockBadge variant="success">Success</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-variant', 'success');
    });

    it('renders with warning variant', () => {
      render(<MockBadge variant="warning">Warning</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-variant', 'warning');
    });

    it('renders with error variant', () => {
      render(<MockBadge variant="error">Error</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-variant', 'error');
    });

    it('renders with info variant', () => {
      render(<MockBadge variant="info">Info</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-variant', 'info');
    });

    it('renders with default md size', () => {
      render(<MockBadge>Medium</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-size', 'md');
    });

    it('renders with sm size', () => {
      render(<MockBadge size="sm">Small</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-size', 'sm');
    });

    it('renders with lg size', () => {
      render(<MockBadge size="lg">Large</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveAttribute('data-size', 'lg');
    });
  });
  // End

  // Style Tests
  describe('Styling', () => {
    it('applies default variant classes', () => {
      render(<MockBadge>Default</MockBadge>);
      const badge = screen.getByTestId('badge');
      expect(badge).toHaveClass('bg-gray-100', 'text-gray-800');
    });

    it('applies success variant classes', () => {
      render(<MockBadge variant="success">Success</MockBadge>);
      const badge = screen.getByTestId('badge');
      expect(badge).toHaveClass('bg-green-100', 'text-green-800');
    });

    it('applies warning variant classes', () => {
      render(<MockBadge variant="warning">Warning</MockBadge>);
      const badge = screen.getByTestId('badge');
      expect(badge).toHaveClass('bg-yellow-100', 'text-yellow-800');
    });

    it('applies error variant classes', () => {
      render(<MockBadge variant="error">Error</MockBadge>);
      const badge = screen.getByTestId('badge');
      expect(badge).toHaveClass('bg-red-100', 'text-red-800');
    });

    it('applies info variant classes', () => {
      render(<MockBadge variant="info">Info</MockBadge>);
      const badge = screen.getByTestId('badge');
      expect(badge).toHaveClass('bg-blue-100', 'text-blue-800');
    });

    it('applies rounded-full class', () => {
      render(<MockBadge>Test</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveClass('rounded-full');
    });

    it('applies font-medium class', () => {
      render(<MockBadge>Test</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveClass('font-medium');
    });

    it('applies inline-flex class', () => {
      render(<MockBadge>Test</MockBadge>);
      expect(screen.getByTestId('badge')).toHaveClass('inline-flex');
    });
  });
  // End

  // Status Display Tests
  describe('Status Display', () => {
    it('displays Active status', () => {
      render(<MockBadge variant="success">Active</MockBadge>);
      expect(screen.getByText('Active')).toBeInTheDocument();
    });

    it('displays Inactive status', () => {
      render(<MockBadge variant="default">Inactive</MockBadge>);
      expect(screen.getByText('Inactive')).toBeInTheDocument();
    });

    it('displays Pending status', () => {
      render(<MockBadge variant="warning">Pending</MockBadge>);
      expect(screen.getByText('Pending')).toBeInTheDocument();
    });

    it('displays Cancelled status', () => {
      render(<MockBadge variant="error">Cancelled</MockBadge>);
      expect(screen.getByText('Cancelled')).toBeInTheDocument();
    });

    it('displays Completed status', () => {
      render(<MockBadge variant="success">Completed</MockBadge>);
      expect(screen.getByText('Completed')).toBeInTheDocument();
    });

    it('displays In Progress status', () => {
      render(<MockBadge variant="info">In Progress</MockBadge>);
      expect(screen.getByText('In Progress')).toBeInTheDocument();
    });
  });
  // End
});

describe('Toast Component Tests', () => {
  // Visibility Tests
  describe('Visibility', () => {
    it('renders when isVisible is true', () => {
      render(<MockToast message="Test message" isVisible={true} />);
      expect(screen.getByTestId('toast')).toBeInTheDocument();
    });

    it('does not render when isVisible is false', () => {
      render(<MockToast message="Test message" isVisible={false} />);
      expect(screen.queryByTestId('toast')).not.toBeInTheDocument();
    });
  });
  // End

  // Content Tests
  describe('Content', () => {
    it('displays message', () => {
      render(<MockToast message="Operation successful" isVisible={true} />);
      expect(screen.getByTestId('toast-message')).toHaveTextContent('Operation successful');
    });

    it('displays different messages', () => {
      render(<MockToast message="Patient saved successfully" isVisible={true} />);
      expect(screen.getByTestId('toast-message')).toHaveTextContent('Patient saved successfully');
    });

    it('renders close button', () => {
      render(<MockToast message="Test" isVisible={true} />);
      expect(screen.getByTestId('toast-close')).toBeInTheDocument();
    });
  });
  // End

  // Type Tests
  describe('Types', () => {
    it('renders with default info type', () => {
      render(<MockToast message="Info" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveAttribute('data-type', 'info');
    });

    it('renders with success type', () => {
      render(<MockToast message="Success" type="success" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveAttribute('data-type', 'success');
    });

    it('renders with error type', () => {
      render(<MockToast message="Error" type="error" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveAttribute('data-type', 'error');
    });

    it('renders with warning type', () => {
      render(<MockToast message="Warning" type="warning" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveAttribute('data-type', 'warning');
    });
  });
  // End

  // Style Tests
  describe('Styling', () => {
    it('applies success background class', () => {
      render(<MockToast message="Success" type="success" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveClass('bg-green-500');
    });

    it('applies error background class', () => {
      render(<MockToast message="Error" type="error" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveClass('bg-red-500');
    });

    it('applies warning background class', () => {
      render(<MockToast message="Warning" type="warning" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveClass('bg-yellow-500');
    });

    it('applies info background class', () => {
      render(<MockToast message="Info" type="info" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveClass('bg-blue-500');
    });

    it('applies text-white class', () => {
      render(<MockToast message="Test" isVisible={true} />);
      expect(screen.getByTestId('toast')).toHaveClass('text-white');
    });
  });
  // End

  // Interaction Tests
  describe('Interactions', () => {
    it('calls onClose when close button is clicked', async () => {
      const onClose = vi.fn();
      const user = userEvent.setup();
      render(<MockToast message="Test" isVisible={true} onClose={onClose} />);
      await user.click(screen.getByTestId('toast-close'));
      expect(onClose).toHaveBeenCalledTimes(1);
    });
  });
  // End
});

describe('ConfirmDialog Component Tests', () => {
  // Visibility Tests
  describe('Visibility', () => {
    it('renders when isOpen is true', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" />);
      expect(screen.getByTestId('confirm-dialog')).toBeInTheDocument();
    });

    it('does not render when isOpen is false', () => {
      render(<MockConfirmDialog isOpen={false} title="Confirm" message="Are you sure?" />);
      expect(screen.queryByTestId('confirm-dialog')).not.toBeInTheDocument();
    });
  });
  // End

  // Content Tests
  describe('Content', () => {
    it('displays title', () => {
      render(<MockConfirmDialog isOpen={true} title="Delete Patient" message="Are you sure?" />);
      expect(screen.getByTestId('confirm-dialog-title')).toHaveTextContent('Delete Patient');
    });

    it('displays message', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="This action cannot be undone" />);
      expect(screen.getByTestId('confirm-dialog-message')).toHaveTextContent('This action cannot be undone');
    });

    it('displays default confirm label', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" />);
      expect(screen.getByTestId('confirm-dialog-confirm')).toHaveTextContent('Confirm');
    });

    it('displays default cancel label', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" />);
      expect(screen.getByTestId('confirm-dialog-cancel')).toHaveTextContent('Cancel');
    });

    it('displays custom confirm label', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" confirmLabel="Delete" />);
      expect(screen.getByTestId('confirm-dialog-confirm')).toHaveTextContent('Delete');
    });

    it('displays custom cancel label', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" cancelLabel="Go Back" />);
      expect(screen.getByTestId('confirm-dialog-cancel')).toHaveTextContent('Go Back');
    });
  });
  // End

  // Interaction Tests
  describe('Interactions', () => {
    it('calls onConfirm when confirm button is clicked', async () => {
      const onConfirm = vi.fn();
      const user = userEvent.setup();
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" onConfirm={onConfirm} />);
      await user.click(screen.getByTestId('confirm-dialog-confirm'));
      expect(onConfirm).toHaveBeenCalledTimes(1);
    });

    it('calls onCancel when cancel button is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" onCancel={onCancel} />);
      await user.click(screen.getByTestId('confirm-dialog-cancel'));
      expect(onCancel).toHaveBeenCalledTimes(1);
    });

    it('calls onCancel when overlay is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" onCancel={onCancel} />);
      await user.click(screen.getByTestId('confirm-dialog-overlay'));
      expect(onCancel).toHaveBeenCalledTimes(1);
    });
  });
  // End

  // Variant Tests
  describe('Variants', () => {
    it('renders with default variant', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" />);
      expect(screen.getByTestId('confirm-dialog-confirm')).toHaveAttribute('data-variant', 'default');
    });

    it('renders with danger variant', () => {
      render(<MockConfirmDialog isOpen={true} title="Delete" message="Are you sure?" variant="danger" />);
      expect(screen.getByTestId('confirm-dialog-confirm')).toHaveAttribute('data-variant', 'danger');
    });
  });
  // End

  // Dialog Role Tests
  describe('Accessibility', () => {
    it('has dialog role', () => {
      render(<MockConfirmDialog isOpen={true} title="Confirm" message="Are you sure?" />);
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });
  });
  // End
});

describe('Pagination Component Tests', () => {
  // Rendering Tests
  describe('Rendering', () => {
    it('renders pagination', () => {
      render(<MockPagination />);
      expect(screen.getByTestId('pagination')).toBeInTheDocument();
    });

    it('renders previous button', () => {
      render(<MockPagination />);
      expect(screen.getByTestId('pagination-prev')).toBeInTheDocument();
    });

    it('renders next button', () => {
      render(<MockPagination />);
      expect(screen.getByTestId('pagination-next')).toBeInTheDocument();
    });

    it('renders page buttons', () => {
      render(<MockPagination totalPages={5} />);
      expect(screen.getByTestId('pagination-page-1')).toBeInTheDocument();
      expect(screen.getByTestId('pagination-page-5')).toBeInTheDocument();
    });

    it('renders correct number of page buttons', () => {
      render(<MockPagination totalPages={10} />);
      for (let i = 1; i <= 10; i++) {
        expect(screen.getByTestId(`pagination-page-${i}`)).toBeInTheDocument();
      }
    });
  });
  // End

  // State Tests
  describe('State', () => {
    it('disables previous button on first page', () => {
      render(<MockPagination currentPage={1} totalPages={5} />);
      expect(screen.getByTestId('pagination-prev')).toBeDisabled();
    });

    it('enables previous button on non-first page', () => {
      render(<MockPagination currentPage={3} totalPages={5} />);
      expect(screen.getByTestId('pagination-prev')).not.toBeDisabled();
    });

    it('disables next button on last page', () => {
      render(<MockPagination currentPage={5} totalPages={5} />);
      expect(screen.getByTestId('pagination-next')).toBeDisabled();
    });

    it('enables next button on non-last page', () => {
      render(<MockPagination currentPage={3} totalPages={5} />);
      expect(screen.getByTestId('pagination-next')).not.toBeDisabled();
    });

    it('marks current page with aria-current', () => {
      render(<MockPagination currentPage={3} totalPages={5} />);
      expect(screen.getByTestId('pagination-page-3')).toHaveAttribute('aria-current', 'page');
    });

    it('other pages do not have aria-current', () => {
      render(<MockPagination currentPage={3} totalPages={5} />);
      expect(screen.getByTestId('pagination-page-1')).not.toHaveAttribute('aria-current');
      expect(screen.getByTestId('pagination-page-2')).not.toHaveAttribute('aria-current');
    });
  });
  // End

  // Interaction Tests
  describe('Interactions', () => {
    it('calls onPageChange when page button is clicked', async () => {
      const onPageChange = vi.fn();
      const user = userEvent.setup();
      render(<MockPagination totalPages={5} onPageChange={onPageChange} />);
      await user.click(screen.getByTestId('pagination-page-3'));
      expect(onPageChange).toHaveBeenCalledWith(3);
    });

    it('calls onPageChange with next page when next is clicked', async () => {
      const onPageChange = vi.fn();
      const user = userEvent.setup();
      render(<MockPagination currentPage={2} totalPages={5} onPageChange={onPageChange} />);
      await user.click(screen.getByTestId('pagination-next'));
      expect(onPageChange).toHaveBeenCalledWith(3);
    });

    it('calls onPageChange with previous page when prev is clicked', async () => {
      const onPageChange = vi.fn();
      const user = userEvent.setup();
      render(<MockPagination currentPage={3} totalPages={5} onPageChange={onPageChange} />);
      await user.click(screen.getByTestId('pagination-prev'));
      expect(onPageChange).toHaveBeenCalledWith(2);
    });

    it('clicks multiple page buttons', async () => {
      const onPageChange = vi.fn();
      const user = userEvent.setup();
      render(<MockPagination totalPages={5} onPageChange={onPageChange} />);
      await user.click(screen.getByTestId('pagination-page-2'));
      await user.click(screen.getByTestId('pagination-page-4'));
      expect(onPageChange).toHaveBeenCalledTimes(2);
    });
  });
  // End

  // Accessibility Tests
  describe('Accessibility', () => {
    it('has navigation role', () => {
      render(<MockPagination />);
      expect(screen.getByRole('navigation')).toBeInTheDocument();
    });

    it('has aria-label', () => {
      render(<MockPagination />);
      expect(screen.getByRole('navigation')).toHaveAttribute('aria-label', 'Pagination');
    });
  });
  // End
});

describe('DataTable Component Tests', () => {
  const columns = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'email', label: 'Email', sortable: true },
    { key: 'status', label: 'Status', sortable: false }
  ];

  const data = [
    { name: 'John Doe', email: 'john@example.com', status: 'Active' },
    { name: 'Jane Smith', email: 'jane@example.com', status: 'Inactive' },
    { name: 'Bob Wilson', email: 'bob@example.com', status: 'Active' }
  ];

  // Rendering Tests
  describe('Rendering', () => {
    it('renders the data table', () => {
      render(<MockDataTable columns={columns} data={data} />);
      expect(screen.getByTestId('data-table')).toBeInTheDocument();
    });

    it('renders column headers', () => {
      render(<MockDataTable columns={columns} data={data} />);
      expect(screen.getByTestId('header-name')).toHaveTextContent('Name');
      expect(screen.getByTestId('header-email')).toHaveTextContent('Email');
      expect(screen.getByTestId('header-status')).toHaveTextContent('Status');
    });

    it('renders data rows', () => {
      render(<MockDataTable columns={columns} data={data} />);
      expect(screen.getByTestId('row-0')).toBeInTheDocument();
      expect(screen.getByTestId('row-1')).toBeInTheDocument();
      expect(screen.getByTestId('row-2')).toBeInTheDocument();
    });

    it('renders cell data', () => {
      render(<MockDataTable columns={columns} data={data} />);
      expect(screen.getByTestId('cell-0-name')).toHaveTextContent('John Doe');
      expect(screen.getByTestId('cell-0-email')).toHaveTextContent('john@example.com');
    });

    it('renders loading state', () => {
      render(<MockDataTable columns={columns} data={[]} isLoading={true} />);
      expect(screen.getByTestId('data-table-loading')).toBeInTheDocument();
    });

    it('renders empty state when no data', () => {
      render(<MockDataTable columns={columns} data={[]} />);
      expect(screen.getByTestId('data-table-empty')).toBeInTheDocument();
    });

    it('renders custom empty message', () => {
      render(<MockDataTable columns={columns} data={[]} emptyMessage="No patients found" />);
      expect(screen.getByTestId('data-table-empty')).toHaveTextContent('No patients found');
    });
  });
  // End

  // Sorting Tests
  describe('Sorting', () => {
    it('renders sort buttons when sortable is true', () => {
      render(<MockDataTable columns={columns} data={data} sortable={true} />);
      expect(screen.getByTestId('sort-name')).toBeInTheDocument();
      expect(screen.getByTestId('sort-email')).toBeInTheDocument();
    });

    it('does not render sort button for non-sortable column', () => {
      render(<MockDataTable columns={columns} data={data} sortable={true} />);
      expect(screen.queryByTestId('sort-status')).not.toBeInTheDocument();
    });

    it('calls onSort when sort button is clicked', async () => {
      const onSort = vi.fn();
      const user = userEvent.setup();
      render(<MockDataTable columns={columns} data={data} sortable={true} onSort={onSort} />);
      await user.click(screen.getByTestId('sort-name'));
      expect(onSort).toHaveBeenCalledWith('name', 'asc');
    });
  });
  // End

  // Selection Tests
  describe('Selection', () => {
    it('renders select checkboxes when selectable is true', () => {
      render(<MockDataTable columns={columns} data={data} selectable={true} />);
      expect(screen.getByTestId('select-all')).toBeInTheDocument();
      expect(screen.getByTestId('select-0')).toBeInTheDocument();
      expect(screen.getByTestId('select-1')).toBeInTheDocument();
    });

    it('does not render checkboxes when selectable is false', () => {
      render(<MockDataTable columns={columns} data={data} selectable={false} />);
      expect(screen.queryByTestId('select-all')).not.toBeInTheDocument();
    });
  });
  // End

  // Row Click Tests
  describe('Row Click', () => {
    it('calls onRowClick when row is clicked', async () => {
      const onRowClick = vi.fn();
      const user = userEvent.setup();
      render(<MockDataTable columns={columns} data={data} onRowClick={onRowClick} />);
      await user.click(screen.getByTestId('row-0'));
      expect(onRowClick).toHaveBeenCalledWith(data[0]);
    });

    it('calls onRowClick with correct row data', async () => {
      const onRowClick = vi.fn();
      const user = userEvent.setup();
      render(<MockDataTable columns={columns} data={data} onRowClick={onRowClick} />);
      await user.click(screen.getByTestId('row-1'));
      expect(onRowClick).toHaveBeenCalledWith(data[1]);
    });
  });
  // End

  // Large Data Tests
  describe('Large Data Sets', () => {
    it('renders 50 rows', () => {
      const largeData = Array.from({ length: 50 }, (_, i) => ({
        name: `User ${i}`,
        email: `user${i}@example.com`,
        status: 'Active'
      }));
      render(<MockDataTable columns={columns} data={largeData} />);
      expect(screen.getByTestId('row-0')).toBeInTheDocument();
      expect(screen.getByTestId('row-49')).toBeInTheDocument();
    });

    it('renders 100 rows', () => {
      const largeData = Array.from({ length: 100 }, (_, i) => ({
        name: `User ${i}`,
        email: `user${i}@example.com`,
        status: 'Active'
      }));
      render(<MockDataTable columns={columns} data={largeData} />);
      expect(screen.getByTestId('row-0')).toBeInTheDocument();
      expect(screen.getByTestId('row-99')).toBeInTheDocument();
    });
  });
  // End
});

// Additional UI interaction tests
describe('UI Component Integration Tests', () => {
  // Toast Notification Flow
  describe('Toast Notification Flow', () => {
    it('shows and hides toast notification', async () => {
      const ToastDemo = () => {
        const [visible, setVisible] = useState(false);
        return (
          <>
            <button onClick={() => setVisible(true)}>Show Toast</button>
            <MockToast message="Success!" isVisible={visible} onClose={() => setVisible(false)} />
          </>
        );
      };

      const user = userEvent.setup();
      render(<ToastDemo />);

      expect(screen.queryByTestId('toast')).not.toBeInTheDocument();
      await user.click(screen.getByText('Show Toast'));
      expect(screen.getByTestId('toast')).toBeInTheDocument();
      await user.click(screen.getByTestId('toast-close'));
      expect(screen.queryByTestId('toast')).not.toBeInTheDocument();
    });
  });
  // End

  // Confirm Dialog Flow
  describe('Confirm Dialog Flow', () => {
    it('confirms action', async () => {
      const onConfirm = vi.fn();
      const DialogDemo = () => {
        const [open, setOpen] = useState(false);
        return (
          <>
            <button onClick={() => setOpen(true)}>Delete</button>
            <MockConfirmDialog
              isOpen={open}
              title="Delete Item"
              message="Are you sure?"
              onConfirm={() => { onConfirm(); setOpen(false); }}
              onCancel={() => setOpen(false)}
            />
          </>
        );
      };

      const user = userEvent.setup();
      render(<DialogDemo />);

      await user.click(screen.getByText('Delete'));
      expect(screen.getByTestId('confirm-dialog')).toBeInTheDocument();
      await user.click(screen.getByTestId('confirm-dialog-confirm'));
      expect(onConfirm).toHaveBeenCalled();
    });

    it('cancels action', async () => {
      const onConfirm = vi.fn();
      const DialogDemo = () => {
        const [open, setOpen] = useState(false);
        return (
          <>
            <button onClick={() => setOpen(true)}>Delete</button>
            <MockConfirmDialog
              isOpen={open}
              title="Delete Item"
              message="Are you sure?"
              onConfirm={() => { onConfirm(); setOpen(false); }}
              onCancel={() => setOpen(false)}
            />
          </>
        );
      };

      const user = userEvent.setup();
      render(<DialogDemo />);

      await user.click(screen.getByText('Delete'));
      await user.click(screen.getByTestId('confirm-dialog-cancel'));
      expect(onConfirm).not.toHaveBeenCalled();
    });
  });
  // End

  // Pagination with Data Table
  describe('Pagination with Data Table', () => {
    it('updates table when page changes', async () => {
      const TableWithPagination = () => {
        const [page, setPage] = useState(1);
        const allData = Array.from({ length: 30 }, (_, i) => ({
          name: `User ${i + 1}`,
          email: `user${i + 1}@example.com`,
          status: 'Active'
        }));
        const pageSize = 10;
        const totalPages = Math.ceil(allData.length / pageSize);
        const pageData = allData.slice((page - 1) * pageSize, page * pageSize);

        return (
          <>
            <MockDataTable
              columns={[
                { key: 'name', label: 'Name' },
                { key: 'email', label: 'Email' }
              ]}
              data={pageData}
            />
            <MockPagination
              currentPage={page}
              totalPages={totalPages}
              onPageChange={setPage}
            />
          </>
        );
      };

      const user = userEvent.setup();
      render(<TableWithPagination />);

      expect(screen.getByText('User 1')).toBeInTheDocument();
      await user.click(screen.getByTestId('pagination-page-2'));
      expect(screen.getByText('User 11')).toBeInTheDocument();
    });
  });
  // End
});
