import { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, X, Loader2, Image as ImageIcon } from 'lucide-react';
import { mediaApi } from '../api/media';
import toast from 'react-hot-toast';
import { resolveImageUrl } from '../utils/image';

interface ImageUploaderProps {
  onUploadComplete: (urls: string[]) => void;
  existingImages?: string[];
  maxFiles?: number;
  folder?: string;
}

export default function ImageUploader({
  onUploadComplete,
  existingImages = [],
  maxFiles = 10,
  folder = 'shn-gear/products',
}: ImageUploaderProps) {
  const [uploading, setUploading] = useState(false);
  const [uploadedUrls, setUploadedUrls] = useState<string[]>(existingImages);
  const [previewUrls, setPreviewUrls] = useState<string[]>(existingImages);

  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    if (uploadedUrls.length + acceptedFiles.length > maxFiles) {
      toast.error(`Maximum ${maxFiles} images allowed`);
      return;
    }

    // Show previews immediately
    const newPreviews = acceptedFiles.map(file => URL.createObjectURL(file));
    setPreviewUrls(prev => [...prev, ...newPreviews]);

    try {
      setUploading(true);
      const results = await mediaApi.uploadImages(acceptedFiles, folder);
      const newUrls = results.map(r => r.url);
      
      const allUrls = [...uploadedUrls, ...newUrls];
      setUploadedUrls(allUrls);
      onUploadComplete(allUrls);
      
      toast.success(`${results.length} image(s) uploaded successfully`);
    } catch (error: any) {
      console.error('Upload error:', error);
      const errorMessage =
        error?.response?.data?.errorMessage ||
        error?.response?.data?.message ||
        'Failed to upload images';
      toast.error(errorMessage);
      // Remove failed previews
      setPreviewUrls(prev => prev.slice(0, uploadedUrls.length));
    } finally {
      setUploading(false);
    }
  }, [uploadedUrls, folder, maxFiles, onUploadComplete]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'image/jpeg': ['.jpg', '.jpeg'],
      'image/png': ['.png'],
      'image/webp': ['.webp'],
      'image/gif': ['.gif'],
      'image/avif': ['.avif'],
    },
    maxSize: 10 * 1024 * 1024, // 10MB
    multiple: true,
    disabled: uploading,
  });

  const removeImage = (index: number) => {
    const newUrls = uploadedUrls.filter((_, i) => i !== index);
    const newPreviews = previewUrls.filter((_, i) => i !== index);
    setUploadedUrls(newUrls);
    setPreviewUrls(newPreviews);
    onUploadComplete(newUrls);
  };

  return (
    <div className="space-y-4">
      {/* Upload area */}
      <div
        {...getRootProps()}
        className={`
          border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors
          ${isDragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400'}
          ${uploading ? 'opacity-50 cursor-not-allowed' : ''}
        `}
      >
        <input {...getInputProps()} />
        <div className="flex flex-col items-center gap-3">
          {uploading ? (
            <Loader2 className="animate-spin text-blue-600" size={48} />
          ) : (
            <Upload className="text-gray-400" size={48} />
          )}
          <div>
            <p className="text-lg font-medium text-gray-700">
              {isDragActive ? 'Drop images here' : 'Drag & drop images here'}
            </p>
            <p className="text-sm text-gray-500 mt-1">
              or click to browse (max {maxFiles} images, 10MB each)
            </p>
            <p className="text-xs text-gray-400 mt-2">
              Supported: JPEG, PNG, WebP, GIF, AVIF
            </p>
          </div>
        </div>
      </div>

      {/* Image previews */}
      {previewUrls.length > 0 && (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {previewUrls.map((url, index) => (
            <div key={index} className="relative group">
              <div className="aspect-square rounded-lg overflow-hidden bg-gray-100 border border-gray-200">
                {url ? (
                  <img
                    src={resolveImageUrl(url)}
                    alt={`Upload ${index + 1}`}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center">
                    <ImageIcon className="text-gray-300" size={32} />
                  </div>
                )}
              </div>
              {!uploading && (
                <button
                  onClick={() => removeImage(index)}
                  className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                  title="Remove image"
                >
                  <X size={16} />
                </button>
              )}
              {index === 0 && (
                <div className="absolute bottom-2 left-2 px-2 py-1 bg-blue-600 text-white text-xs rounded">
                  Primary
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {uploadedUrls.length > 0 && (
        <p className="text-sm text-gray-600">
          {uploadedUrls.length} / {maxFiles} images uploaded
        </p>
      )}
    </div>
  );
}
