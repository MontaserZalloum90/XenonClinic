import React, { useState, useCallback, useMemo } from 'react';
import { useT, useSchema, useListLayout, useTenant } from '../../contexts/TenantContext';
import type {
  UISchema,
  ListLayout,
  ListColumn,
  ListAction,
  FieldDefinition,
} from '../../types/tenant';

/**
 * EntityList - Renders a data table based on schema and layout from tenant context
 *
 * Usage:
 * <EntityList
 *   entityName="patient"
 *   data={patients}
 *   onRowAction={(action, row) => handleAction(action, row)}
 * />
 */

interface EntityListProps<T extends Record<string, unknown>> {
  /** Entity name to look up schema/layout */
  entityName: string;
  /** Data to display */
  data: T[];
  /** Total count for pagination */
  totalCount?: number;
  /** Loading state */
  isLoading?: boolean;
  /** Current page (1-indexed) */
  page?: number;
  /** Page size */
  pageSize?: number;
  /** Sort field */
  sortField?: string;
  /** Sort direction */
  sortDirection?: 'asc' | 'desc';
  /** Search query */
  searchQuery?: string;
  /** Selected row IDs */
  selectedIds?: (string | number)[];
  /** Called when row action is clicked */
  onRowAction?: (action: ListAction, row: T) => void;
  /** Called when bulk action is clicked */
  onBulkAction?: (action: ListAction, rows: T[]) => void;
  /** Called when header action is clicked */
  onHeaderAction?: (action: ListAction) => void;
  /** Called when page changes */
  onPageChange?: (page: number) => void;
  /** Called when page size changes */
  onPageSizeChange?: (pageSize: number) => void;
  /** Called when sort changes */
  onSortChange?: (field: string, direction: 'asc' | 'desc') => void;
  /** Called when search changes */
  onSearchChange?: (query: string) => void;
  /** Called when selection changes */
  onSelectionChange?: (ids: (string | number)[]) => void;
  /** Custom schema override */
  schema?: UISchema;
  /** Custom layout override */
  layout?: ListLayout;
  /** Row key field */
  rowKey?: string;
  /** Additional className */
  className?: string;
}

export function EntityList<T extends Record<string, unknown>>({
  entityName,
  data,
  totalCount,
  isLoading = false,
  page = 1,
  pageSize = 25,
  sortField,
  sortDirection = 'asc',
  searchQuery = '',
  selectedIds = [],
  onRowAction,
  onBulkAction,
  onHeaderAction,
  onPageChange,
  onPageSizeChange,
  onSortChange,
  onSearchChange,
  onSelectionChange,
  schema: schemaProp,
  layout: layoutProp,
  rowKey = 'id',
  className = '',
}: EntityListProps<T>): React.ReactElement {
  const t = useT();
  const { hasFeature, context } = useTenant();
  const contextSchema = useSchema(entityName);
  const contextLayout = useListLayout(entityName);

  const schema = schemaProp ?? contextSchema;
  const layout = layoutProp ?? contextLayout;

  const [localSearch, setLocalSearch] = useState(searchQuery);

  // Build field map
  const fieldMap = useMemo(() => {
    if (!schema) return new Map<string, FieldDefinition>();
    return new Map(schema.fields.map(f => [f.name, f]));
  }, [schema]);

  // Get columns from layout or generate from schema
  const columns = useMemo((): ListColumn[] => {
    if (layout?.columns) return layout.columns;
    if (!schema) return [];

    return schema.fields
      .filter(f => f.sortable !== false)
      .slice(0, 6)
      .map(f => ({
        field: f.name,
        sortable: f.sortable,
      }));
  }, [layout, schema]);

  // Filter actions by feature and role
  const filterActions = useCallback((actions: ListAction[]): ListAction[] => {
    const userRoles = context?.userRoles ?? [];

    return actions.filter(action => {
      // Check feature
      if (action.featureCode && !hasFeature(action.featureCode)) {
        return false;
      }
      // Check roles
      if (action.requiredRoles && action.requiredRoles.length > 0) {
        if (!action.requiredRoles.some(role => userRoles.includes(role))) {
          return false;
        }
      }
      return true;
    });
  }, [hasFeature, context]);

  const rowActions = useMemo(() =>
    filterActions(layout?.actions.row ?? []), [layout, filterActions]);
  const bulkActions = useMemo(() =>
    filterActions(layout?.actions.bulk ?? []), [layout, filterActions]);
  const headerActions = useMemo(() =>
    filterActions(layout?.actions.header ?? []), [layout, filterActions]);

  // Handlers
  const handleSort = (field: string) => {
    if (sortField === field) {
      onSortChange?.(field, sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      onSortChange?.(field, 'asc');
    }
  };

  const handleSearch = () => {
    onSearchChange?.(localSearch);
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      onSelectionChange?.(data.map(row => row[rowKey] as string | number));
    } else {
      onSelectionChange?.([]);
    }
  };

  const handleSelectRow = (id: string | number, checked: boolean) => {
    if (checked) {
      onSelectionChange?.([...selectedIds, id]);
    } else {
      onSelectionChange?.(selectedIds.filter(i => i !== id));
    }
  };

  const total = totalCount ?? data.length;
  const totalPages = Math.ceil(total / pageSize);
  const hasSelection = selectedIds.length > 0;
  const allSelected = data.length > 0 && selectedIds.length === data.length;

  if (!schema) {
    return (
      <div className="p-4 text-center text-gray-500">
        Schema not found for entity: {entityName}
      </div>
    );
  }

  return (
    <div className={`bg-white dark:bg-gray-800 rounded-lg shadow ${className}`}>
      {/* Header */}
      <div className="px-4 py-4 border-b flex flex-wrap items-center gap-4">
        {/* Search */}
        {layout?.showSearch && (
          <div className="flex-1 min-w-[200px] max-w-md">
            <div className="relative">
              <input
                type="text"
                value={localSearch}
                onChange={(e) => setLocalSearch(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                placeholder={t('common.search')}
                className="w-full pl-10 pr-4 py-2 border rounded-md text-sm"
              />
              <svg
                className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
        )}

        {/* Bulk Actions */}
        {hasSelection && bulkActions.length > 0 && (
          <div className="flex items-center gap-2">
            <span className="text-sm text-gray-500">
              {selectedIds.length} selected
            </span>
            {bulkActions.map(action => (
              <button
                key={action.id}
                onClick={() => {
                  const selectedRows = data.filter(row =>
                    selectedIds.includes(row[rowKey] as string | number)
                  );
                  onBulkAction?.(action, selectedRows);
                }}
                className={`px-3 py-1.5 text-sm rounded-md ${getActionButtonClass(action.type)}`}
              >
                {t(action.label)}
              </button>
            ))}
          </div>
        )}

        <div className="flex-1" />

        {/* Header Actions */}
        {headerActions.map(action => (
          <button
            key={action.id}
            onClick={() => onHeaderAction?.(action)}
            className={`px-4 py-2 text-sm font-medium rounded-md ${getActionButtonClass(action.type)}`}
          >
            {t(action.label)}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              {/* Selection checkbox */}
              {bulkActions.length > 0 && (
                <th className="w-12 px-4 py-3">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={(e) => handleSelectAll(e.target.checked)}
                    className="rounded border-gray-300"
                  />
                </th>
              )}

              {/* Data columns */}
              {columns.map(col => {
                const field = fieldMap.get(col.field);
                const label = field?.label ? t(field.label) : col.field;
                const isSortable = col.sortable ?? field?.sortable ?? true;
                const isSorted = sortField === col.field;

                return (
                  <th
                    key={col.field}
                    className={`px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider ${
                      isSortable ? 'cursor-pointer hover:bg-gray-100' : ''
                    }`}
                    style={{ width: col.width }}
                    onClick={() => isSortable && handleSort(col.field)}
                  >
                    <div className="flex items-center gap-1">
                      {label}
                      {isSortable && isSorted && (
                        <svg
                          className={`h-4 w-4 ${sortDirection === 'desc' ? 'rotate-180' : ''}`}
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                        </svg>
                      )}
                    </div>
                  </th>
                );
              })}

              {/* Actions column */}
              {rowActions.length > 0 && (
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">
                  {t('common.actions', 'Actions')}
                </th>
              )}
            </tr>
          </thead>

          <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200">
            {isLoading ? (
              // Loading skeleton
              [...Array(5)].map((_, i) => (
                <tr key={i}>
                  {bulkActions.length > 0 && <td className="px-4 py-4"><div className="h-4 w-4 bg-gray-200 rounded animate-pulse" /></td>}
                  {columns.map(col => (
                    <td key={col.field} className="px-4 py-4">
                      <div className="h-4 bg-gray-200 rounded animate-pulse" />
                    </td>
                  ))}
                  {rowActions.length > 0 && <td className="px-4 py-4"><div className="h-4 w-20 bg-gray-200 rounded animate-pulse ml-auto" /></td>}
                </tr>
              ))
            ) : data.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length + (bulkActions.length > 0 ? 1 : 0) + (rowActions.length > 0 ? 1 : 0)}
                  className="px-4 py-12 text-center text-gray-500"
                >
                  {t('common.noResults')}
                </td>
              </tr>
            ) : (
              data.map(row => {
                const rowId = row[rowKey] as string | number;
                const isSelected = selectedIds.includes(rowId);

                return (
                  <tr
                    key={rowId}
                    className={`hover:bg-gray-50 dark:hover:bg-gray-700 ${isSelected ? 'bg-primary-50' : ''}`}
                  >
                    {bulkActions.length > 0 && (
                      <td className="px-4 py-4">
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={(e) => handleSelectRow(rowId, e.target.checked)}
                          className="rounded border-gray-300"
                        />
                      </td>
                    )}

                    {columns.map(col => {
                      const field = fieldMap.get(col.field);
                      const value = row[col.field];

                      return (
                        <td
                          key={col.field}
                          className={`px-4 py-4 text-sm text-gray-900 dark:text-gray-100 ${
                            col.align === 'center' ? 'text-center' : col.align === 'right' ? 'text-right' : ''
                          }`}
                        >
                          <CellValue value={value} format={col.format} field={field} />
                        </td>
                      );
                    })}

                    {rowActions.length > 0 && (
                      <td className="px-4 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {rowActions.map(action => (
                            <button
                              key={action.id}
                              onClick={() => onRowAction?.(action, row)}
                              className={`p-1.5 rounded hover:bg-gray-100 ${getActionIconClass(action.type)}`}
                              title={t(action.label)}
                            >
                              <ActionIcon icon={action.icon} />
                            </button>
                          ))}
                        </div>
                      </td>
                    )}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="px-4 py-3 border-t flex items-center justify-between">
          <div className="text-sm text-gray-500">
            Showing {(page - 1) * pageSize + 1} to {Math.min(page * pageSize, total)} of {total}
          </div>

          <div className="flex items-center gap-2">
            {/* Page size selector */}
            <select
              value={pageSize}
              onChange={(e) => onPageSizeChange?.(Number(e.target.value))}
              className="text-sm border rounded-md py-1 px-2"
            >
              {(layout?.pageSizeOptions ?? [10, 25, 50, 100]).map(size => (
                <option key={size} value={size}>{size} / page</option>
              ))}
            </select>

            {/* Page buttons */}
            <div className="flex items-center gap-1">
              <button
                onClick={() => onPageChange?.(1)}
                disabled={page === 1}
                className="p-1 rounded hover:bg-gray-100 disabled:opacity-50"
              >
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
                </svg>
              </button>
              <button
                onClick={() => onPageChange?.(page - 1)}
                disabled={page === 1}
                className="p-1 rounded hover:bg-gray-100 disabled:opacity-50"
              >
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>

              <span className="px-2 text-sm">
                Page {page} of {totalPages}
              </span>

              <button
                onClick={() => onPageChange?.(page + 1)}
                disabled={page === totalPages}
                className="p-1 rounded hover:bg-gray-100 disabled:opacity-50"
              >
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>
              <button
                onClick={() => onPageChange?.(totalPages)}
                disabled={page === totalPages}
                className="p-1 rounded hover:bg-gray-100 disabled:opacity-50"
              >
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 5l7 7-7 7M5 5l7 7-7 7" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ============================================
// Cell Value Component
// ============================================

interface CellValueProps {
  value: unknown;
  format?: string;
  field?: FieldDefinition;
}

const CellValue: React.FC<CellValueProps> = ({ value, format, field }) => {
  if (value === null || value === undefined) {
    return <span className="text-gray-400">-</span>;
  }

  const fmt = format ?? (field?.type === 'currency' ? 'currency' : field?.type === 'date' ? 'date' : undefined);

  switch (fmt) {
    case 'date':
      return <span>{new Date(value as string).toLocaleDateString()}</span>;

    case 'datetime':
      return <span>{new Date(value as string).toLocaleString()}</span>;

    case 'currency':
      return (
        <span>
          {field?.currency ?? 'AED'} {Number(value).toLocaleString(undefined, { minimumFractionDigits: 2 })}
        </span>
      );

    case 'percentage':
      return <span>{Number(value).toFixed(field?.decimals ?? 1)}%</span>;

    case 'badge':
      return (
        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getBadgeClass(String(value))}`}>
          {String(value)}
        </span>
      );

    case 'avatar':
      return (
        <div className="flex items-center">
          <div className="h-8 w-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 font-medium">
            {String(value).charAt(0).toUpperCase()}
          </div>
        </div>
      );

    default:
      return <span>{String(value)}</span>;
  }
};

// ============================================
// Action Icon Component
// ============================================

const ActionIcon: React.FC<{ icon: string }> = ({ icon }) => {
  const iconClass = "h-4 w-4";

  switch (icon) {
    case 'Eye':
      return <svg className={iconClass} fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>;
    case 'Pencil':
      return <svg className={iconClass} fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>;
    case 'Trash':
      return <svg className={iconClass} fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>;
    case 'Calendar':
      return <svg className={iconClass} fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>;
    default:
      return <svg className={iconClass} fill="none" viewBox="0 0 24 24" stroke="currentColor"><circle cx="12" cy="12" r="3" /></svg>;
  }
};

// ============================================
// Helper Functions
// ============================================

function getActionButtonClass(type: string): string {
  switch (type) {
    case 'primary':
      return 'bg-primary-600 text-white hover:bg-primary-700';
    case 'danger':
      return 'bg-red-600 text-white hover:bg-red-700';
    default:
      return 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50';
  }
}

function getActionIconClass(type: string): string {
  switch (type) {
    case 'danger':
      return 'text-red-600 hover:text-red-700';
    case 'primary':
      return 'text-primary-600 hover:text-primary-700';
    default:
      return 'text-gray-600 hover:text-gray-700';
  }
}

function getBadgeClass(value: string): string {
  const lowered = value.toLowerCase();
  if (['active', 'completed', 'success', 'approved'].includes(lowered)) {
    return 'bg-green-100 text-green-800';
  }
  if (['inactive', 'cancelled', 'failed', 'rejected'].includes(lowered)) {
    return 'bg-red-100 text-red-800';
  }
  if (['pending', 'draft', 'inProgress'].includes(lowered)) {
    return 'bg-yellow-100 text-yellow-800';
  }
  if (['male'].includes(lowered)) {
    return 'bg-blue-100 text-blue-800';
  }
  if (['female'].includes(lowered)) {
    return 'bg-pink-100 text-pink-800';
  }
  return 'bg-gray-100 text-gray-800';
}

export default EntityList;
