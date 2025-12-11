import React from 'react';
import { LoadingSkeleton } from '../LoadingSkeleton/LoadingSkeleton';
import { EmptyState } from '../EmptyState/EmptyState';

export interface Column<T> {
  key: string;
  header: string;
  render?: (item: T, index: number) => React.ReactNode;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
}

export type SortDirection = 'asc' | 'desc' | null;

export interface SortState {
  column: string | null;
  direction: SortDirection;
}

interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  keyExtractor: (item: T) => string | number;
  isLoading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyAction?: {
    label: string;
    onClick: () => void;
  };
  sortState?: SortState;
  onSort?: (column: string) => void;
  onRowClick?: (item: T) => void;
  selectedRows?: Set<string | number>;
  onSelectRow?: (key: string | number) => void;
  onSelectAll?: () => void;
  className?: string;
}

export function DataTable<T>({
  data,
  columns,
  keyExtractor,
  isLoading = false,
  emptyTitle = 'No data available',
  emptyDescription,
  emptyAction,
  sortState,
  onSort,
  onRowClick,
  selectedRows,
  onSelectRow,
  onSelectAll,
  className = '',
}: DataTableProps<T>) {
  const showCheckboxes = onSelectRow !== undefined;
  const allSelected = selectedRows && data.length > 0 && data.every((item) => selectedRows.has(keyExtractor(item)));
  const someSelected = selectedRows && data.some((item) => selectedRows.has(keyExtractor(item)));

  if (isLoading) {
    return (
      <div className={`border border-gray-200 rounded-lg overflow-hidden ${className}`}>
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              {showCheckboxes && <th className="w-12 px-4 py-3" />}
              {columns.map((column) => (
                <th
                  key={column.key}
                  className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  <LoadingSkeleton variant="text" width="80px" />
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {Array.from({ length: 5 }).map((_, rowIndex) => (
              <tr key={rowIndex}>
                {showCheckboxes && (
                  <td className="px-4 py-4">
                    <LoadingSkeleton variant="rectangular" width="16px" height="16px" />
                  </td>
                )}
                {columns.map((column) => (
                  <td key={column.key} className="px-4 py-4">
                    <LoadingSkeleton variant="text" />
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className={`border border-gray-200 rounded-lg ${className}`}>
        <EmptyState
          title={emptyTitle}
          description={emptyDescription}
          action={emptyAction}
        />
      </div>
    );
  }

  return (
    <div className={`border border-gray-200 rounded-lg overflow-hidden ${className}`}>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              {showCheckboxes && (
                <th className="w-12 px-4 py-3">
                  <input
                    type="checkbox"
                    checked={!!allSelected}
                    ref={(el) => {
                      if (el) el.indeterminate = !!(someSelected && !allSelected);
                    }}
                    onChange={onSelectAll}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                </th>
              )}
              {columns.map((column) => (
                <th
                  key={column.key}
                  className={`px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider ${
                    column.align === 'center' ? 'text-center' : column.align === 'right' ? 'text-right' : 'text-left'
                  } ${column.sortable && onSort ? 'cursor-pointer hover:bg-gray-100 select-none' : ''}`}
                  style={{ width: column.width }}
                  onClick={() => column.sortable && onSort?.(column.key)}
                >
                  <div className="flex items-center gap-1">
                    {column.header}
                    {column.sortable && sortState?.column === column.key && (
                      <span className="text-primary-600">
                        {sortState.direction === 'asc' ? '↑' : '↓'}
                      </span>
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {data.map((item, index) => {
              const key = keyExtractor(item);
              const isSelected = selectedRows?.has(key);

              return (
                <tr
                  key={key}
                  className={`
                    ${onRowClick ? 'cursor-pointer hover:bg-gray-50' : ''}
                    ${isSelected ? 'bg-primary-50' : ''}
                  `}
                  onClick={() => onRowClick?.(item)}
                >
                  {showCheckboxes && (
                    <td className="px-4 py-4" onClick={(e) => e.stopPropagation()}>
                      <input
                        type="checkbox"
                        checked={isSelected}
                        onChange={() => onSelectRow?.(key)}
                        className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                      />
                    </td>
                  )}
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={`px-4 py-4 text-sm text-gray-900 ${
                        column.align === 'center' ? 'text-center' : column.align === 'right' ? 'text-right' : 'text-left'
                      }`}
                    >
                      {column.render
                        ? column.render(item, index)
                        : (item as Record<string, unknown>)[column.key]?.toString()}
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// Hook for managing table state
export function useDataTable<T>(
  initialData: T[],
  options: {
    initialSort?: SortState;
    sortFn?: (data: T[], sort: SortState) => T[];
  } = {}
) {
  const [sortState, setSortState] = React.useState<SortState>(
    options.initialSort || { column: null, direction: null }
  );
  const [selectedRows, setSelectedRows] = React.useState<Set<string | number>>(new Set());

  const handleSort = (column: string) => {
    setSortState((prev) => {
      if (prev.column !== column) {
        return { column, direction: 'asc' };
      }
      if (prev.direction === 'asc') {
        return { column, direction: 'desc' };
      }
      return { column: null, direction: null };
    });
  };

  const handleSelectRow = (key: string | number) => {
    setSelectedRows((prev) => {
      const next = new Set(prev);
      if (next.has(key)) {
        next.delete(key);
      } else {
        next.add(key);
      }
      return next;
    });
  };

  const handleSelectAll = (keys: (string | number)[]) => {
    setSelectedRows((prev) => {
      const allSelected = keys.every((key) => prev.has(key));
      if (allSelected) {
        return new Set();
      }
      return new Set(keys);
    });
  };

  const sortedData = React.useMemo(() => {
    if (!sortState.column || !sortState.direction) {
      return initialData;
    }
    if (options.sortFn) {
      return options.sortFn(initialData, sortState);
    }
    return [...initialData].sort((a, b) => {
      const aVal = (a as Record<string, unknown>)[sortState.column!];
      const bVal = (b as Record<string, unknown>)[sortState.column!];
      const comparison = String(aVal).localeCompare(String(bVal));
      return sortState.direction === 'asc' ? comparison : -comparison;
    });
  }, [initialData, sortState, options.sortFn]);

  return {
    sortState,
    handleSort,
    selectedRows,
    handleSelectRow,
    handleSelectAll,
    sortedData,
    clearSelection: () => setSelectedRows(new Set()),
  };
}
