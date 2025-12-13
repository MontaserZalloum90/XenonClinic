import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import { api } from '../../lib/api';
import type { PortalDocument, PortalDocumentType } from '../../types/portal';
import { useToast } from '../../components/ui/Toast';

const DOCUMENT_TYPE_LABELS = [
  'Medical Record',
  'Lab Result',
  'Prescription',
  'Imaging Report',
  'Insurance',
  'Other'
];

export const PortalDocuments = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [filterType, setFilterType] = useState<number | null>(null);
  const [uploadData, setUploadData] = useState({
    name: '',
    type: 0 as PortalDocumentType,
    description: '',
    file: null as File | null,
  });

  // Fetch documents
  const { data: documents, isLoading } = useQuery<PortalDocument[]>({
    queryKey: ['portal-documents'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/documents');
      return response.data;
    },
  });

  // Upload mutation
  const uploadMutation = useMutation({
    mutationFn: async (data: FormData) => {
      const response = await api.post('/api/Portal/documents/upload', data, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portal-documents'] });
      queryClient.invalidateQueries({ queryKey: ['portal-statistics'] });
      setIsUploadModalOpen(false);
      resetUploadForm();
      showToast('success', 'Document uploaded successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Upload failed: ${error.message}`);
    },
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/Portal/documents/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portal-documents'] });
      queryClient.invalidateQueries({ queryKey: ['portal-statistics'] });
      showToast('success', 'Document deleted successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Delete failed: ${error.message}`);
    },
  });

  const resetUploadForm = () => {
    setUploadData({
      name: '',
      type: 0,
      description: '',
      file: null,
    });
  };

  const handleUploadSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!uploadData.file) {
      showToast('error', 'Please select a file to upload');
      return;
    }

    const formData = new FormData();
    formData.append('name', uploadData.name);
    formData.append('type', uploadData.type.toString());
    formData.append('description', uploadData.description);
    formData.append('file', uploadData.file);

    uploadMutation.mutate(formData);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setUploadData({
        ...uploadData,
        file,
        name: uploadData.name || file.name,
      });
    }
  };

  const handleDelete = (document: PortalDocument) => {
    if (confirm(`Are you sure you want to delete "${document.name}"? This action cannot be undone.`)) {
      deleteMutation.mutate(document.id);
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const getDocumentIcon = (type: number) => {
    const icons = [
      'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', // Medical Record
      'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z', // Lab Result
      'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2', // Prescription
      'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z', // Imaging Report
      'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', // Insurance
      'M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z', // Other
    ];
    return icons[type] || icons[5];
  };

  const filteredDocuments = documents?.filter(
    doc => filterType === null || doc.type === filterType
  );

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">My Documents</h1>
            <div className="flex items-center gap-3">
              <a href="/portal/dashboard" className="text-blue-600 hover:text-blue-700 text-sm font-medium">
                Back to Dashboard
              </a>
              <button
                onClick={() => setIsUploadModalOpen(true)}
                className="btn btn-primary"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                Upload Document
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Filter */}
        <div className="card mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">Filter by Type</label>
          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => setFilterType(null)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                filterType === null
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              All Documents ({documents?.length || 0})
            </button>
            {DOCUMENT_TYPE_LABELS.map((label, index) => (
              <button
                key={index}
                onClick={() => setFilterType(index)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                  filterType === index
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {label} ({documents?.filter(d => d.type === index).length || 0})
              </button>
            ))}
          </div>
        </div>

        {/* Documents Grid */}
        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        ) : filteredDocuments && filteredDocuments.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredDocuments.map((document) => (
              <div key={document.id} className="card hover:shadow-lg transition-shadow">
                <div className="flex items-start justify-between mb-4">
                  <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={getDocumentIcon(document.type)} />
                    </svg>
                  </div>
                  <div className="flex gap-2">
                    <a
                      href={document.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                      title="Download"
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                      </svg>
                    </a>
                    <button
                      onClick={() => handleDelete(document)}
                      disabled={deleteMutation.isPending}
                      className="p-2 text-red-600 hover:bg-red-50 rounded-lg disabled:opacity-50 transition-colors"
                      title="Delete"
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </div>

                <h3 className="font-semibold text-gray-900 mb-2 truncate" title={document.name}>
                  {document.name}
                </h3>

                <div className="space-y-2 text-sm">
                  <div className="flex items-center justify-between">
                    <span className="text-gray-600">Type:</span>
                    <span className="px-2 py-1 bg-blue-50 text-blue-700 rounded text-xs font-medium">
                      {DOCUMENT_TYPE_LABELS[document.type]}
                    </span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="text-gray-600">Size:</span>
                    <span className="text-gray-900 font-medium">{formatFileSize(document.size)}</span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="text-gray-600">Uploaded:</span>
                    <span className="text-gray-900 font-medium">
                      {format(new Date(document.uploadedAt), 'MMM d, yyyy')}
                    </span>
                  </div>

                  {document.description && (
                    <div className="pt-2 border-t border-gray-200">
                      <p className="text-gray-600 text-xs line-clamp-2">{document.description}</p>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="card text-center py-12">
            <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              {filterType !== null ? 'No documents in this category' : 'No documents yet'}
            </h3>
            <p className="text-gray-600 mb-4">
              {filterType !== null
                ? 'Try selecting a different category or upload a new document'
                : 'Upload your medical documents to keep them organized and accessible'}
            </p>
            <button onClick={() => setIsUploadModalOpen(true)} className="btn btn-primary">
              Upload Your First Document
            </button>
          </div>
        )}
      </div>

      {/* Upload Modal */}
      <Dialog
        open={isUploadModalOpen}
        onClose={() => {
          setIsUploadModalOpen(false);
          resetUploadForm();
        }}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />

        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-lg w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-xl font-semibold text-gray-900 mb-4">
                Upload Document
              </Dialog.Title>

              <form onSubmit={handleUploadSubmit} className="space-y-4">
                {/* File Upload */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Select File *
                  </label>
                  <input
                    type="file"
                    onChange={handleFileChange}
                    className="block w-full text-sm text-gray-900 border border-gray-300 rounded-lg cursor-pointer bg-gray-50 focus:outline-none"
                    required
                    accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    Supported formats: PDF, DOC, DOCX, JPG, PNG (Max 10MB)
                  </p>
                </div>

                {/* Document Name */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Document Name *
                  </label>
                  <input
                    type="text"
                    value={uploadData.name}
                    onChange={(e) => setUploadData({ ...uploadData, name: e.target.value })}
                    className="input w-full"
                    placeholder="Enter document name"
                    required
                  />
                </div>

                {/* Document Type */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Document Type *
                  </label>
                  <select
                    value={uploadData.type}
                    onChange={(e) => setUploadData({ ...uploadData, type: parseInt(e.target.value) as PortalDocumentType })}
                    className="input w-full"
                    required
                  >
                    {DOCUMENT_TYPE_LABELS.map((label, index) => (
                      <option key={index} value={index}>
                        {label}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Description */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Description (Optional)
                  </label>
                  <textarea
                    value={uploadData.description}
                    onChange={(e) => setUploadData({ ...uploadData, description: e.target.value })}
                    className="input w-full"
                    rows={3}
                    placeholder="Add any notes or description about this document..."
                  />
                </div>

                {/* Actions */}
                <div className="flex justify-end gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => {
                      setIsUploadModalOpen(false);
                      resetUploadForm();
                    }}
                    className="btn btn-secondary"
                    disabled={uploadMutation.isPending}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={uploadMutation.isPending}
                  >
                    {uploadMutation.isPending ? (
                      <div className="flex items-center">
                        <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
                        Uploading...
                      </div>
                    ) : (
                      'Upload'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
