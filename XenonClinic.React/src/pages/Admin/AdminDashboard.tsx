import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Cog6ToothIcon,
  CircleStackIcon,
  ArrowPathIcon,
  TrashIcon,
  ArrowDownTrayIcon,
  ArrowUpTrayIcon,
  CheckCircleIcon,
  ExclamationCircleIcon,
  ClipboardDocumentListIcon,
  UserGroupIcon,
  CalendarIcon,
  BuildingStorefrontIcon,
  DocumentTextIcon,
  LanguageIcon,
  ChevronRightIcon,
} from '@heroicons/react/24/outline';
import { demoService, type DemoStatus, type ClinicType } from '../../lib/demo';

interface Toast {
  type: 'success' | 'error';
  message: string;
}

export const AdminDashboard = () => {
  const [demoStatus, setDemoStatus] = useState<DemoStatus | null>(null);
  const [selectedClinicType, setSelectedClinicType] = useState<ClinicType>('audiology');
  const [isLoading, setIsLoading] = useState(false);
  const [toast, setToast] = useState<Toast | null>(null);
  const [showImportModal, setShowImportModal] = useState(false);
  const [importData, setImportData] = useState('');

  const clinicTypes = demoService.getAvailableClinicTypes();

  useEffect(() => {
    refreshStatus();
  }, []);

  useEffect(() => {
    if (toast) {
      const timer = setTimeout(() => setToast(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [toast]);

  const refreshStatus = () => {
    setDemoStatus(demoService.getStatus());
  };

  const showToast = (type: 'success' | 'error', message: string) => {
    setToast({ type, message });
  };

  const handleLoadDemo = async () => {
    setIsLoading(true);
    try {
      const result = await demoService.loadDemoData(selectedClinicType);
      if (result.success) {
        showToast('success', result.message);
        refreshStatus();
      } else {
        showToast('error', result.error || result.message);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleResetDemo = async () => {
    if (!confirm('Are you sure you want to reset the demo data? This will clear all current data and reload fresh demo data.')) {
      return;
    }
    setIsLoading(true);
    try {
      const result = await demoService.resetDemoData(selectedClinicType);
      if (result.success) {
        showToast('success', result.message);
        refreshStatus();
      } else {
        showToast('error', result.error || result.message);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleClearDemo = async () => {
    if (!confirm('Are you sure you want to clear ALL demo data? This cannot be undone.')) {
      return;
    }
    setIsLoading(true);
    try {
      const result = await demoService.clearDemoData();
      if (result.success) {
        showToast('success', result.message);
        refreshStatus();
      } else {
        showToast('error', result.error || result.message);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleExport = () => {
    const data = demoService.exportData();
    const blob = new Blob([data], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `xenonclinic-demo-${new Date().toISOString().split('T')[0]}.json`;
    a.click();
    URL.revokeObjectURL(url);
    showToast('success', 'Demo data exported successfully');
  };

  const handleImport = async () => {
    if (!importData.trim()) {
      showToast('error', 'Please paste valid JSON data');
      return;
    }
    setIsLoading(true);
    try {
      const result = await demoService.importData(importData);
      if (result.success) {
        showToast('success', result.message);
        refreshStatus();
        setShowImportModal(false);
        setImportData('');
      } else {
        showToast('error', result.error || result.message);
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Toast Notification */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 flex items-center gap-2 px-4 py-3 rounded-lg shadow-lg ${
            toast.type === 'success' ? 'bg-green-50 text-green-800 border border-green-200' : 'bg-red-50 text-red-800 border border-red-200'
          }`}
        >
          {toast.type === 'success' ? (
            <CheckCircleIcon className="h-5 w-5 text-green-600" />
          ) : (
            <ExclamationCircleIcon className="h-5 w-5 text-red-600" />
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
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <Cog6ToothIcon className="h-7 w-7 text-gray-600" />
            Admin Dashboard
          </h1>
          <p className="text-gray-600">System settings and demo data management</p>
        </div>
      </div>

      {/* Admin Quick Navigation */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <Link
          to="/admin/translations"
          className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow group"
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center">
                <LanguageIcon className="h-6 w-6 text-primary-600" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900 group-hover:text-primary-600">
                  Translation Management
                </h3>
                <p className="text-sm text-gray-500">Customize labels and terminology</p>
              </div>
            </div>
            <ChevronRightIcon className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
          </div>
        </Link>
      </div>

      {/* Demo Data Management Section */}
      <div className="bg-white rounded-lg shadow-md overflow-hidden">
        <div className="bg-gradient-to-r from-primary-600 to-primary-700 px-6 py-4">
          <h2 className="text-lg font-semibold text-white flex items-center gap-2">
            <CircleStackIcon className="h-5 w-5" />
            Demo Data Management
          </h2>
          <p className="text-primary-100 text-sm mt-1">
            Load sample datasets for sales demonstrations and testing
          </p>
        </div>

        <div className="p-6 space-y-6">
          {/* Current Status */}
          <div className="bg-gray-50 rounded-lg p-4">
            <h3 className="text-sm font-medium text-gray-700 mb-3">Current Status</h3>
            {demoStatus?.isLoaded ? (
              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    <CheckCircleIcon className="h-3.5 w-3.5 mr-1" />
                    Demo Active
                  </span>
                  <span className="text-sm text-gray-600">{demoStatus.clinicName}</span>
                </div>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                  <StatCard icon={UserGroupIcon} label="Patients" value={demoStatus.stats.patients} color="blue" />
                  <StatCard icon={CalendarIcon} label="Appointments" value={demoStatus.stats.appointments} color="green" />
                  <StatCard icon={ClipboardDocumentListIcon} label="Encounters" value={demoStatus.stats.encounters} color="purple" />
                  <StatCard icon={BuildingStorefrontIcon} label="Inventory" value={demoStatus.stats.inventory} color="yellow" />
                </div>
                {demoStatus.loadedAt && (
                  <p className="text-xs text-gray-500">
                    Loaded: {new Date(demoStatus.loadedAt).toLocaleString()}
                  </p>
                )}
              </div>
            ) : (
              <div className="flex items-center gap-2 text-gray-500">
                <ExclamationCircleIcon className="h-5 w-5" />
                <span>No demo data loaded</span>
              </div>
            )}
          </div>

          {/* Clinic Type Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select Clinic Type
            </label>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
              {clinicTypes.map((clinic) => (
                <button
                  key={clinic.type}
                  onClick={() => setSelectedClinicType(clinic.type)}
                  className={`p-4 rounded-lg border-2 text-left transition-all ${
                    selectedClinicType === clinic.type
                      ? 'border-primary-500 bg-primary-50'
                      : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                  }`}
                >
                  <div className="font-medium text-gray-900">{clinic.name.replace('XenonClinic ', '')}</div>
                  <div className="text-xs text-gray-500 mt-1 line-clamp-2">{clinic.description}</div>
                  {demoStatus?.clinicType === clinic.type && demoStatus.isLoaded && (
                    <span className="inline-flex items-center mt-2 px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-700">
                      Currently Loaded
                    </span>
                  )}
                </button>
              ))}
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-wrap gap-3 pt-4 border-t">
            <button
              onClick={handleLoadDemo}
              disabled={isLoading}
              className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <CircleStackIcon className="h-5 w-5 mr-2" />
              {isLoading ? 'Loading...' : 'Load Demo Dataset'}
            </button>

            <button
              onClick={handleResetDemo}
              disabled={isLoading || !demoStatus?.isLoaded}
              className="inline-flex items-center px-4 py-2 bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <ArrowPathIcon className="h-5 w-5 mr-2" />
              Reset Demo Data
            </button>

            <button
              onClick={handleClearDemo}
              disabled={isLoading || !demoStatus?.isLoaded}
              className="inline-flex items-center px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <TrashIcon className="h-5 w-5 mr-2" />
              Clear All Data
            </button>

            <div className="border-l border-gray-300 mx-2" />

            <button
              onClick={handleExport}
              disabled={isLoading || !demoStatus?.isLoaded}
              className="inline-flex items-center px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <ArrowDownTrayIcon className="h-5 w-5 mr-2" />
              Export
            </button>

            <button
              onClick={() => setShowImportModal(true)}
              disabled={isLoading}
              className="inline-flex items-center px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <ArrowUpTrayIcon className="h-5 w-5 mr-2" />
              Import
            </button>
          </div>
        </div>
      </div>

      {/* Feature Highlights */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Demo Features</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <FeatureCard
            title="Clinic-Specific Data"
            description="Pre-configured datasets for audiology, dental, general medicine, and more."
            icon={BuildingStorefrontIcon}
          />
          <FeatureCard
            title="Idempotent Operations"
            description="Safe to run multiple times. Reloading the same dataset has no side effects."
            icon={ArrowPathIcon}
          />
          <FeatureCard
            title="Export & Import"
            description="Backup your customized demo data and restore it anytime."
            icon={DocumentTextIcon}
          />
        </div>
      </div>

      {/* Import Modal */}
      {showImportModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4">
            <div className="px-6 py-4 border-b">
              <h3 className="text-lg font-semibold">Import Demo Data</h3>
            </div>
            <div className="p-6">
              <p className="text-sm text-gray-600 mb-4">
                Paste the exported JSON data below. This will replace all current demo data.
              </p>
              <textarea
                value={importData}
                onChange={(e) => setImportData(e.target.value)}
                className="w-full h-64 p-3 border rounded-lg font-mono text-sm"
                placeholder="Paste JSON data here..."
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
                disabled={isLoading || !importData.trim()}
                className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
              >
                {isLoading ? 'Importing...' : 'Import Data'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Helper Components
const StatCard = ({
  icon: Icon,
  label,
  value,
  color,
}: {
  icon: React.ElementType;
  label: string;
  value: number;
  color: 'blue' | 'green' | 'purple' | 'yellow';
}) => {
  const colors = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    yellow: 'bg-yellow-50 text-yellow-600',
  };

  return (
    <div className={`rounded-lg p-3 ${colors[color]}`}>
      <div className="flex items-center gap-2">
        <Icon className="h-4 w-4" />
        <span className="text-xs font-medium">{label}</span>
      </div>
      <div className="text-2xl font-bold mt-1">{value}</div>
    </div>
  );
};

const FeatureCard = ({
  title,
  description,
  icon: Icon,
}: {
  title: string;
  description: string;
  icon: React.ElementType;
}) => (
  <div className="p-4 bg-gray-50 rounded-lg">
    <div className="flex items-center gap-2 text-primary-600 mb-2">
      <Icon className="h-5 w-5" />
      <span className="font-medium">{title}</span>
    </div>
    <p className="text-sm text-gray-600">{description}</p>
  </div>
);
