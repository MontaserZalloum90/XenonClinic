import { useState, useEffect, useMemo, useCallback } from 'react';
import { Link } from 'react-router-dom';
import {
  LanguageIcon,
  MagnifyingGlassIcon,
  PencilSquareIcon,
  CheckIcon,
  XMarkIcon,
  ArrowPathIcon,
  FunnelIcon,
  PlusIcon,
  TrashIcon,
  DocumentDuplicateIcon,
  ArrowDownTrayIcon,
  ArrowUpTrayIcon,
  CheckCircleIcon,
  ExclamationCircleIcon,
  InformationCircleIcon,
  ArrowLeftIcon,
} from '@heroicons/react/24/outline';
import { useTenant } from '../../contexts/TenantContext';

interface Toast {
  type: 'success' | 'error' | 'info';
  message: string;
}

interface TranslationEntry {
  key: string;
  value: string;
  isCustom: boolean;
  category: string;
}

interface EditingState {
  key: string;
  value: string;
}

// Default translation keys organized by category
const DEFAULT_TRANSLATION_KEYS: Record<string, string[]> = {
  'Entities': [
    'entity.patient.singular',
    'entity.patient.plural',
    'entity.appointment.singular',
    'entity.appointment.plural',
    'entity.encounter.singular',
    'entity.encounter.plural',
    'entity.invoice.singular',
    'entity.invoice.plural',
    'entity.prescription.singular',
    'entity.prescription.plural',
    'entity.labTest.singular',
    'entity.labTest.plural',
    'entity.inventory.singular',
    'entity.inventory.plural',
    'entity.employee.singular',
    'entity.employee.plural',
  ],
  'Navigation': [
    'nav.dashboard',
    'nav.appointments',
    'nav.patients',
    'nav.laboratory',
    'nav.pharmacy',
    'nav.radiology',
    'nav.audiology',
    'nav.hr',
    'nav.financial',
    'nav.inventory',
    'nav.admin',
    'nav.settings',
    'nav.reports',
  ],
  'Pages': [
    'page.dashboard.title',
    'page.dashboard.welcome',
    'page.patients.title',
    'page.patients.description',
    'page.appointments.title',
    'page.appointments.description',
    'page.laboratory.title',
    'page.laboratory.description',
    'page.pharmacy.title',
    'page.inventory.title',
    'page.hr.title',
    'page.financial.title',
    'page.admin.title',
  ],
  'Actions': [
    'action.save',
    'action.cancel',
    'action.delete',
    'action.edit',
    'action.create',
    'action.search',
    'action.filter',
    'action.export',
    'action.import',
    'action.refresh',
    'action.print',
    'action.download',
    'action.upload',
    'action.submit',
    'action.confirm',
    'action.back',
    'action.next',
    'action.previous',
  ],
  'Status': [
    'status.active',
    'status.inactive',
    'status.pending',
    'status.completed',
    'status.cancelled',
    'status.scheduled',
    'status.inProgress',
    'status.draft',
    'status.approved',
    'status.rejected',
    'status.paid',
    'status.unpaid',
    'status.overdue',
  ],
  'Messages': [
    'message.success.saved',
    'message.success.created',
    'message.success.updated',
    'message.success.deleted',
    'message.error.generic',
    'message.error.notFound',
    'message.error.unauthorized',
    'message.error.validation',
    'message.confirm.delete',
    'message.confirm.unsavedChanges',
    'message.loading',
    'message.noData',
    'message.noResults',
  ],
  'Fields': [
    'field.firstName',
    'field.lastName',
    'field.fullName',
    'field.email',
    'field.phone',
    'field.address',
    'field.dateOfBirth',
    'field.gender',
    'field.nationalId',
    'field.passportNumber',
    'field.nationality',
    'field.date',
    'field.time',
    'field.startDate',
    'field.endDate',
    'field.status',
    'field.notes',
    'field.description',
    'field.amount',
    'field.quantity',
    'field.price',
    'field.total',
    'field.discount',
    'field.tax',
  ],
  'Clinic-Specific': [
    'clinic.doctor',
    'clinic.nurse',
    'clinic.receptionist',
    'clinic.diagnosis',
    'clinic.treatment',
    'clinic.prescription',
    'clinic.vitals',
    'clinic.bloodPressure',
    'clinic.temperature',
    'clinic.weight',
    'clinic.height',
    'clinic.allergies',
    'clinic.medicalHistory',
    'clinic.chiefComplaint',
    'clinic.followUp',
  ],
};

// Generate fallback values for keys
const generateFallback = (key: string): string => {
  const parts = key.split('.');
  const lastPart = parts[parts.length - 1];
  // Convert camelCase to Title Case
  return lastPart
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (str) => str.toUpperCase())
    .trim();
};

export const TranslationManagement = () => {
  const { context, refresh, updateTerminology, isCacheValid } = useTenant();
  const [translations, setTranslations] = useState<TranslationEntry[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [showOnlyCustom, setShowOnlyCustom] = useState(false);
  const [editing, setEditing] = useState<EditingState | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [toast, setToast] = useState<Toast | null>(null);
  const [showAddModal, setShowAddModal] = useState(false);
  const [newKey, setNewKey] = useState('');
  const [newValue, setNewValue] = useState('');
  const [pendingChanges, setPendingChanges] = useState<Record<string, string>>({});
  const [showImportModal, setShowImportModal] = useState(false);
  const [importData, setImportData] = useState('');

  // Get all categories including 'Custom' for any keys not in defaults
  const categories = useMemo(() => {
    return ['all', ...Object.keys(DEFAULT_TRANSLATION_KEYS), 'Custom'];
  }, []);

  const buildTranslationList = useCallback(() => {
    const entries: TranslationEntry[] = [];
    const currentTerminology = context?.terminology || {};
    const usedKeys = new Set<string>();

    // Add default keys with their values
    Object.entries(DEFAULT_TRANSLATION_KEYS).forEach(([category, keys]) => {
      keys.forEach((key) => {
        usedKeys.add(key);
        entries.push({
          key,
          value: currentTerminology[key] || generateFallback(key),
          isCustom: key in currentTerminology,
          category,
        });
      });
    });

    // Add any custom keys not in defaults
    Object.entries(currentTerminology).forEach(([key, value]) => {
      if (!usedKeys.has(key)) {
        entries.push({
          key,
          value,
          isCustom: true,
          category: 'Custom',
        });
      }
    });

    setTranslations(entries);
  }, [context?.terminology]);

  // Initialize translations from context
  useEffect(() => {
    if (context?.terminology) {
      buildTranslationList();
    }
  }, [context?.terminology, buildTranslationList]);

  // Filter translations based on search and category
  const filteredTranslations = useMemo(() => {
    return translations.filter((entry) => {
      const matchesSearch =
        searchQuery === '' ||
        entry.key.toLowerCase().includes(searchQuery.toLowerCase()) ||
        entry.value.toLowerCase().includes(searchQuery.toLowerCase());

      const matchesCategory =
        selectedCategory === 'all' || entry.category === selectedCategory;

      const matchesCustomFilter = !showOnlyCustom || entry.isCustom;

      return matchesSearch && matchesCategory && matchesCustomFilter;
    });
  }, [translations, searchQuery, selectedCategory, showOnlyCustom]);

  // Toast auto-dismiss
  useEffect(() => {
    if (toast) {
      const timer = setTimeout(() => setToast(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [toast]);

  const showToast = (type: Toast['type'], message: string) => {
    setToast({ type, message });
  };

  const handleStartEdit = (entry: TranslationEntry) => {
    setEditing({ key: entry.key, value: pendingChanges[entry.key] ?? entry.value });
  };

  const handleCancelEdit = () => {
    setEditing(null);
  };

  const handleSaveEdit = () => {
    if (!editing) return;

    setPendingChanges((prev) => ({
      ...prev,
      [editing.key]: editing.value,
    }));

    // Update local state
    setTranslations((prev) =>
      prev.map((entry) =>
        entry.key === editing.key
          ? { ...entry, value: editing.value, isCustom: true }
          : entry
      )
    );

    setEditing(null);
    showToast('info', 'Change saved locally. Click "Save All Changes" to persist.');
  };

  const handleResetKey = (key: string) => {
    // Remove from pending changes
    setPendingChanges((prev) => {
      const newChanges = { ...prev };
      delete newChanges[key];
      return newChanges;
    });

    // Reset to fallback value
    setTranslations((prev) =>
      prev.map((entry) =>
        entry.key === key
          ? { ...entry, value: generateFallback(key), isCustom: false }
          : entry
      )
    );

    showToast('info', 'Translation reset to default. Click "Save All Changes" to persist.');
  };

  const handleSaveAllChanges = async () => {
    if (Object.keys(pendingChanges).length === 0) {
      showToast('info', 'No changes to save');
      return;
    }

    setIsSaving(true);
    try {
      // Merge current terminology with pending changes
      const updatedTerminology = {
        ...(context?.terminology || {}),
        ...pendingChanges,
      };

      // Remove keys that were reset to default (empty values or matching fallback)
      Object.entries(updatedTerminology).forEach(([key, value]) => {
        if (!value || value === generateFallback(key)) {
          delete updatedTerminology[key];
        }
      });

      // Use updateTerminology from context - this handles API call + cache invalidation
      await updateTerminology(updatedTerminology);

      setPendingChanges({});
      showToast('success', 'Translations saved and cache refreshed');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to save translations';
      showToast('error', message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleAddNewKey = () => {
    if (!newKey.trim() || !newValue.trim()) {
      showToast('error', 'Please enter both key and value');
      return;
    }

    // Check if key already exists
    if (translations.some((t) => t.key === newKey)) {
      showToast('error', 'This key already exists');
      return;
    }

    // Add to translations
    setTranslations((prev) => [
      ...prev,
      {
        key: newKey,
        value: newValue,
        isCustom: true,
        category: 'Custom',
      },
    ]);

    // Add to pending changes
    setPendingChanges((prev) => ({
      ...prev,
      [newKey]: newValue,
    }));

    setNewKey('');
    setNewValue('');
    setShowAddModal(false);
    showToast('info', 'Translation added. Click "Save All Changes" to persist.');
  };

  const handleDeleteCustomKey = (key: string) => {
    // Remove from translations
    setTranslations((prev) => prev.filter((t) => t.key !== key));

    // Mark as empty in pending changes (will be removed on save)
    setPendingChanges((prev) => ({
      ...prev,
      [key]: '',
    }));

    showToast('info', 'Translation marked for deletion. Click "Save All Changes" to persist.');
  };

  const handleExport = () => {
    const exportData: Record<string, string> = {};
    translations.forEach((t) => {
      if (t.isCustom || pendingChanges[t.key]) {
        exportData[t.key] = pendingChanges[t.key] ?? t.value;
      }
    });

    const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `translations-${new Date().toISOString().split('T')[0]}.json`;
    a.click();
    URL.revokeObjectURL(url);
    showToast('success', 'Translations exported successfully');
  };

  const handleImport = () => {
    if (!importData.trim()) {
      showToast('error', 'Please paste valid JSON data');
      return;
    }

    try {
      const imported = JSON.parse(importData) as Record<string, string>;

      // Validate structure
      if (typeof imported !== 'object' || imported === null) {
        throw new Error('Invalid format');
      }

      // Add imported translations to pending changes
      Object.entries(imported).forEach(([key, value]) => {
        if (typeof key === 'string' && typeof value === 'string') {
          // Check if key exists in current translations
          const existing = translations.find((t) => t.key === key);
          if (existing) {
            setPendingChanges((prev) => ({ ...prev, [key]: value }));
            setTranslations((prev) =>
              prev.map((t) => (t.key === key ? { ...t, value, isCustom: true } : t))
            );
          } else {
            // Add new key
            setTranslations((prev) => [
              ...prev,
              { key, value, isCustom: true, category: 'Custom' },
            ]);
            setPendingChanges((prev) => ({ ...prev, [key]: value }));
          }
        }
      });

      setImportData('');
      setShowImportModal(false);
      showToast('success', `Imported ${Object.keys(imported).length} translations. Click "Save All Changes" to persist.`);
    } catch {
      showToast('error', 'Invalid JSON format');
    }
  };

  const handleRefresh = async () => {
    setIsLoading(true);
    try {
      await refresh();
      setPendingChanges({});
      showToast('success', 'Translations refreshed from server');
    } catch {
      showToast('error', 'Failed to refresh translations');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCopyKey = (key: string) => {
    navigator.clipboard.writeText(key);
    showToast('info', 'Key copied to clipboard');
  };

  const pendingCount = Object.keys(pendingChanges).length;

  return (
    <div className="space-y-6">
      {/* Toast Notification */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 flex items-center gap-2 px-4 py-3 rounded-lg shadow-lg ${
            toast.type === 'success'
              ? 'bg-green-50 text-green-800 border border-green-200'
              : toast.type === 'error'
              ? 'bg-red-50 text-red-800 border border-red-200'
              : 'bg-blue-50 text-blue-800 border border-blue-200'
          }`}
        >
          {toast.type === 'success' ? (
            <CheckCircleIcon className="h-5 w-5 text-green-600" />
          ) : toast.type === 'error' ? (
            <ExclamationCircleIcon className="h-5 w-5 text-red-600" />
          ) : (
            <InformationCircleIcon className="h-5 w-5 text-blue-600" />
          )}
          <span>{toast.message}</span>
          <button onClick={() => setToast(null)} className="ml-2 text-gray-400 hover:text-gray-600">
            &times;
          </button>
        </div>
      )}

      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <Link
            to="/admin"
            className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700 mb-2"
          >
            <ArrowLeftIcon className="h-4 w-4 mr-1" />
            Back to Admin
          </Link>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <LanguageIcon className="h-7 w-7 text-gray-600" />
            Translation Management
          </h1>
          <p className="text-gray-600">Customize terminology and labels for your clinic</p>
        </div>
        <div className="flex items-center gap-3">
          <button
            onClick={handleRefresh}
            disabled={isLoading}
            className="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
          >
            <ArrowPathIcon className={`h-4 w-4 mr-1.5 ${isLoading ? 'animate-spin' : ''}`} />
            Refresh
          </button>
          {pendingCount > 0 && (
            <button
              onClick={handleSaveAllChanges}
              disabled={isSaving}
              className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-lg hover:bg-primary-700 disabled:opacity-50"
            >
              <CheckIcon className="h-4 w-4 mr-1.5" />
              {isSaving ? 'Saving...' : `Save All Changes (${pendingCount})`}
            </button>
          )}
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="text-sm font-medium text-gray-500">Total Keys</div>
          <div className="text-2xl font-bold text-gray-900">{translations.length}</div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="text-sm font-medium text-gray-500">Customized</div>
          <div className="text-2xl font-bold text-primary-600">
            {translations.filter((t) => t.isCustom).length}
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="text-sm font-medium text-gray-500">Pending Changes</div>
          <div className="text-2xl font-bold text-yellow-600">{pendingCount}</div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="text-sm font-medium text-gray-500">Categories</div>
          <div className="text-2xl font-bold text-gray-900">{categories.length - 1}</div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="text-sm font-medium text-gray-500">Cache Status</div>
          <div className="flex items-center gap-2 mt-1">
            <span
              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                isCacheValid
                  ? 'bg-green-100 text-green-800'
                  : 'bg-yellow-100 text-yellow-800'
              }`}
            >
              {isCacheValid ? 'Valid' : 'Stale'}
            </span>
          </div>
        </div>
      </div>

      {/* Filters and Actions */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-col md:flex-row gap-4">
          {/* Search */}
          <div className="relative flex-1">
            <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search by key or value..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            />
          </div>

          {/* Category Filter */}
          <div className="flex items-center gap-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              {categories.map((cat) => (
                <option key={cat} value={cat}>
                  {cat === 'all' ? 'All Categories' : cat}
                </option>
              ))}
            </select>
          </div>

          {/* Custom Only Toggle */}
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={showOnlyCustom}
              onChange={(e) => setShowOnlyCustom(e.target.checked)}
              className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
            />
            <span className="text-sm text-gray-700">Customized only</span>
          </label>

          {/* Action Buttons */}
          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowAddModal(true)}
              className="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              <PlusIcon className="h-4 w-4 mr-1.5" />
              Add Key
            </button>
            <button
              onClick={handleExport}
              className="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              <ArrowDownTrayIcon className="h-4 w-4 mr-1.5" />
              Export
            </button>
            <button
              onClick={() => setShowImportModal(true)}
              className="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              <ArrowUpTrayIcon className="h-4 w-4 mr-1.5" />
              Import
            </button>
          </div>
        </div>
      </div>

      {/* Translations Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Category
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Key
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Value
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredTranslations.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-12 text-center text-gray-500">
                    No translations found matching your criteria
                  </td>
                </tr>
              ) : (
                filteredTranslations.map((entry) => (
                  <tr
                    key={entry.key}
                    className={`hover:bg-gray-50 ${
                      pendingChanges[entry.key] !== undefined ? 'bg-yellow-50' : ''
                    }`}
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                        {entry.category}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <code className="text-sm font-mono text-gray-700 bg-gray-100 px-2 py-0.5 rounded">
                          {entry.key}
                        </code>
                        <button
                          onClick={() => handleCopyKey(entry.key)}
                          className="text-gray-400 hover:text-gray-600"
                          title="Copy key"
                        >
                          <DocumentDuplicateIcon className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      {editing?.key === entry.key ? (
                        <input
                          type="text"
                          value={editing.value}
                          onChange={(e) => setEditing({ ...editing, value: e.target.value })}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter') handleSaveEdit();
                            if (e.key === 'Escape') handleCancelEdit();
                          }}
                          className="w-full px-3 py-1.5 border border-primary-300 rounded focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                          autoFocus
                        />
                      ) : (
                        <span className="text-gray-900">
                          {pendingChanges[entry.key] ?? entry.value}
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {pendingChanges[entry.key] !== undefined ? (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                          Pending
                        </span>
                      ) : entry.isCustom ? (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                          Customized
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600">
                          Default
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      {editing?.key === entry.key ? (
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={handleSaveEdit}
                            className="text-green-600 hover:text-green-800"
                            title="Save"
                          >
                            <CheckIcon className="h-5 w-5" />
                          </button>
                          <button
                            onClick={handleCancelEdit}
                            className="text-gray-400 hover:text-gray-600"
                            title="Cancel"
                          >
                            <XMarkIcon className="h-5 w-5" />
                          </button>
                        </div>
                      ) : (
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => handleStartEdit(entry)}
                            className="text-primary-600 hover:text-primary-800"
                            title="Edit"
                          >
                            <PencilSquareIcon className="h-5 w-5" />
                          </button>
                          {entry.isCustom && entry.category !== 'Custom' && (
                            <button
                              onClick={() => handleResetKey(entry.key)}
                              className="text-gray-400 hover:text-gray-600"
                              title="Reset to default"
                            >
                              <ArrowPathIcon className="h-5 w-5" />
                            </button>
                          )}
                          {entry.category === 'Custom' && (
                            <button
                              onClick={() => handleDeleteCustomKey(entry.key)}
                              className="text-red-400 hover:text-red-600"
                              title="Delete"
                            >
                              <TrashIcon className="h-5 w-5" />
                            </button>
                          )}
                        </div>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Table Footer */}
        <div className="px-6 py-3 bg-gray-50 border-t text-sm text-gray-500">
          Showing {filteredTranslations.length} of {translations.length} translations
        </div>
      </div>

      {/* Usage Instructions */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h3 className="text-sm font-medium text-blue-800 mb-2">How to use translations in code</h3>
        <div className="text-sm text-blue-700 space-y-2">
          <p>
            <strong>Component:</strong>{' '}
            <code className="bg-blue-100 px-1 rounded">{`<T k="key.name" fallback="Default" />`}</code>
          </p>
          <p>
            <strong>Hook:</strong>{' '}
            <code className="bg-blue-100 px-1 rounded">{`const t = useT(); t('key.name', 'Default')`}</code>
          </p>
          <p>
            <strong>With interpolation:</strong>{' '}
            <code className="bg-blue-100 px-1 rounded">{`<TInterpolate k="greeting" values={{ name: 'John' }} />`}</code>
          </p>
        </div>
      </div>

      {/* Add New Key Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
            <div className="px-6 py-4 border-b">
              <h3 className="text-lg font-semibold">Add New Translation Key</h3>
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Key</label>
                <input
                  type="text"
                  value={newKey}
                  onChange={(e) => setNewKey(e.target.value)}
                  placeholder="e.g., custom.myLabel"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
                <p className="mt-1 text-xs text-gray-500">Use dot notation for organization</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Value</label>
                <input
                  type="text"
                  value={newValue}
                  onChange={(e) => setNewValue(e.target.value)}
                  placeholder="Enter the display text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
              </div>
            </div>
            <div className="px-6 py-4 border-t bg-gray-50 flex justify-end gap-3">
              <button
                onClick={() => {
                  setShowAddModal(false);
                  setNewKey('');
                  setNewValue('');
                }}
                className="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg"
              >
                Cancel
              </button>
              <button
                onClick={handleAddNewKey}
                disabled={!newKey.trim() || !newValue.trim()}
                className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
              >
                Add Translation
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Import Modal */}
      {showImportModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4">
            <div className="px-6 py-4 border-b">
              <h3 className="text-lg font-semibold">Import Translations</h3>
            </div>
            <div className="p-6">
              <p className="text-sm text-gray-600 mb-4">
                Paste JSON data with key-value pairs. This will merge with existing translations.
              </p>
              <textarea
                value={importData}
                onChange={(e) => setImportData(e.target.value)}
                className="w-full h-64 p-3 border rounded-lg font-mono text-sm"
                placeholder={`{\n  "custom.key1": "Value 1",\n  "custom.key2": "Value 2"\n}`}
              />
            </div>
            <div className="px-6 py-4 border-t bg-gray-50 flex justify-end gap-3">
              <button
                onClick={() => {
                  setShowImportModal(false);
                  setImportData('');
                }}
                className="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg"
              >
                Cancel
              </button>
              <button
                onClick={handleImport}
                disabled={!importData.trim()}
                className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
              >
                Import
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
