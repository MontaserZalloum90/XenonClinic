import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import type { SkinPhoto, CreateSkinPhotoRequest } from '../../types/dermatology';

// Mock API - Replace with actual dermatology API
const skinPhotosApi = {
  getAll: async () => {
    // TODO: Implement actual API call
    return { data: [] as SkinPhoto[] };
  },
  create: async (_data: FormData) => {
    // TODO: Implement actual API call with file upload
    return {
      data: {
        id: Date.now(),
        photoUrl: '/placeholder-image.jpg',
        createdAt: new Date().toISOString()
      }
    };
  },
  delete: async (_id: number) => {
    // TODO: Implement actual API call
    return { data: { success: true } };
  },
};

export const SkinPhotos = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPhoto, setSelectedPhoto] = useState<SkinPhoto | undefined>(undefined);

  const { data: photos, isLoading } = useQuery<SkinPhoto[]>({
    queryKey: ['skin-photos'],
    queryFn: async () => {
      const response = await skinPhotosApi.getAll();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: FormData) => skinPhotosApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-photos'] });
      setIsModalOpen(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => skinPhotosApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skin-photos'] });
      setSelectedPhoto(undefined);
    },
  });

  const handleDelete = (photo: SkinPhoto) => {
    if (window.confirm('Are you sure you want to delete this photo?')) {
      deleteMutation.mutate(photo.id);
    }
  };

  const handleView = (photo: SkinPhoto) => {
    setSelectedPhoto(photo);
  };

  const handleCreate = () => {
    setSelectedPhoto(undefined);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
  };

  const handleViewClose = () => {
    setSelectedPhoto(undefined);
  };

  const filteredPhotos = photos?.filter(
    (photo) =>
      photo.patient?.fullNameEn.toLowerCase().includes(searchTerm.toLowerCase()) ||
      photo.bodyLocation.toLowerCase().includes(searchTerm.toLowerCase()) ||
      photo.description?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Skin Photos</h1>
          <p className="text-gray-600 mt-1">Document and manage patient skin photos</p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          Upload Photo
        </button>
      </div>

      <div className="card">
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search by patient name, body location, or description..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input w-full"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading photos...</p>
          </div>
        ) : filteredPhotos && filteredPhotos.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {filteredPhotos.map((photo) => (
              <div
                key={photo.id}
                className="border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
                onClick={() => handleView(photo)}
              >
                <div className="aspect-square bg-gray-100 flex items-center justify-center">
                  {photo.thumbnailUrl || photo.photoUrl ? (
                    <img
                      src={photo.thumbnailUrl || photo.photoUrl}
                      alt={photo.bodyLocation}
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <div className="text-gray-400">
                      <svg
                        className="w-16 h-16"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                        />
                      </svg>
                    </div>
                  )}
                </div>
                <div className="p-3">
                  <p className="font-medium text-gray-900 text-sm">
                    {photo.patient?.fullNameEn || `Patient #${photo.patientId}`}
                  </p>
                  <p className="text-xs text-gray-600 mt-1">{photo.bodyLocation}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    {format(new Date(photo.photoDate), 'MMM d, yyyy')}
                  </p>
                  {photo.description && (
                    <p className="text-xs text-gray-600 mt-2 line-clamp-2">{photo.description}</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm ? 'No photos found matching your search.' : 'No photos found.'}
          </div>
        )}
      </div>

      {/* Upload Modal */}
      <Dialog open={isModalOpen} onClose={handleModalClose} className="relative z-50">
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Upload Skin Photo
              </Dialog.Title>
              <SkinPhotoForm
                onSubmit={(formData) => createMutation.mutate(formData)}
                onCancel={handleModalClose}
                isSubmitting={createMutation.isPending}
              />
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* View Photo Modal */}
      <Dialog
        open={!!selectedPhoto && !isModalOpen}
        onClose={handleViewClose}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            {selectedPhoto && (
              <div className="p-6">
                <div className="flex items-start justify-between mb-4">
                  <Dialog.Title className="text-lg font-medium text-gray-900">
                    Skin Photo Details
                  </Dialog.Title>
                  <button
                    onClick={handleViewClose}
                    className="text-gray-400 hover:text-gray-500"
                  >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </div>

                <div className="space-y-4">
                  <div className="bg-gray-100 rounded-lg overflow-hidden">
                    <img
                      src={selectedPhoto.photoUrl}
                      alt={selectedPhoto.bodyLocation}
                      className="w-full max-h-96 object-contain"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-700">Patient</label>
                      <p className="text-sm text-gray-900 mt-1">
                        {selectedPhoto.patient?.fullNameEn || `Patient #${selectedPhoto.patientId}`}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-700">Date</label>
                      <p className="text-sm text-gray-900 mt-1">
                        {format(new Date(selectedPhoto.photoDate), 'MMMM d, yyyy')}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-700">Body Location</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPhoto.bodyLocation}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-700">Taken By</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPhoto.takenBy}</p>
                    </div>
                  </div>

                  {selectedPhoto.description && (
                    <div>
                      <label className="text-sm font-medium text-gray-700">Description</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPhoto.description}</p>
                    </div>
                  )}

                  {selectedPhoto.notes && (
                    <div>
                      <label className="text-sm font-medium text-gray-700">Notes</label>
                      <p className="text-sm text-gray-900 mt-1">{selectedPhoto.notes}</p>
                    </div>
                  )}

                  <div className="flex items-center justify-end gap-3 pt-4 border-t">
                    <button onClick={handleViewClose} className="btn btn-secondary">
                      Close
                    </button>
                    <button
                      onClick={() => handleDelete(selectedPhoto)}
                      className="btn bg-red-600 text-white hover:bg-red-700"
                    >
                      Delete Photo
                    </button>
                  </div>
                </div>
              </div>
            )}
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};

// Form Component
interface SkinPhotoFormProps {
  onSubmit: (data: FormData) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

const SkinPhotoForm = ({ onSubmit, onCancel, isSubmitting }: SkinPhotoFormProps) => {
  const [formData, setFormData] = useState({
    patientId: 0,
    photoDate: new Date().toISOString().split('T')[0],
    bodyLocation: '',
    description: '',
    notes: '',
    takenBy: '',
  });
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedFile) {
      alert('Please select a photo to upload');
      return;
    }

    const data = new FormData();
    data.append('photo', selectedFile);
    data.append('patientId', formData.patientId.toString());
    data.append('photoDate', formData.photoDate);
    data.append('bodyLocation', formData.bodyLocation);
    data.append('description', formData.description);
    data.append('notes', formData.notes);
    data.append('takenBy', formData.takenBy);

    onSubmit(data);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Patient ID</label>
          <input
            type="number"
            required
            value={formData.patientId || ''}
            onChange={(e) => setFormData({ ...formData, patientId: parseInt(e.target.value) })}
            className="input w-full"
          />
        </div>
        <div>
          <label className="label">Photo Date</label>
          <input
            type="date"
            required
            value={formData.photoDate}
            onChange={(e) => setFormData({ ...formData, photoDate: e.target.value })}
            className="input w-full"
          />
        </div>
      </div>

      <div>
        <label className="label">Photo Upload</label>
        <input
          type="file"
          accept="image/*"
          required
          onChange={handleFileChange}
          className="input w-full"
        />
        {preview && (
          <div className="mt-2 border border-gray-200 rounded-lg overflow-hidden">
            <img src={preview} alt="Preview" className="w-full max-h-64 object-contain" />
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">Body Location</label>
          <input
            type="text"
            required
            value={formData.bodyLocation}
            onChange={(e) => setFormData({ ...formData, bodyLocation: e.target.value })}
            className="input w-full"
            placeholder="e.g., Left arm, Back - upper"
          />
        </div>
        <div>
          <label className="label">Taken By</label>
          <input
            type="text"
            required
            value={formData.takenBy}
            onChange={(e) => setFormData({ ...formData, takenBy: e.target.value })}
            className="input w-full"
            placeholder="Provider name"
          />
        </div>
      </div>

      <div>
        <label className="label">Description</label>
        <textarea
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Brief description of what the photo shows"
        />
      </div>

      <div>
        <label className="label">Clinical Notes</label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="input w-full"
          rows={2}
          placeholder="Additional clinical notes or observations"
        />
      </div>

      <div className="flex items-center justify-end gap-3 pt-4">
        <button type="button" onClick={onCancel} className="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" disabled={isSubmitting} className="btn btn-primary">
          {isSubmitting ? 'Uploading...' : 'Upload Photo'}
        </button>
      </div>
    </form>
  );
};
