import client from './client';
import type { ImageUploadResult } from '../types/admin';

export const mediaApi = {
  uploadImages: async (files: File[], folder?: string): Promise<ImageUploadResult[]> => {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    if (folder) formData.append('folder', folder);
    
      const response = await client.post<ImageUploadResult[]>('/Media/images', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data;
  },
};
