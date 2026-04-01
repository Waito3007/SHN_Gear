import client from './client';
import type { ApiResponse } from '../types';
import type { BrandDto, CreateBrandRequest, UpdateBrandRequest } from '../types/admin';

export const brandApi = {
  getAll: () => client.get<ApiResponse<BrandDto[]>>('/Brand'),
  getById: (id: string) => client.get<ApiResponse<BrandDto>>(`/Brand/${id}`),
  getActive: () => client.get<ApiResponse<BrandDto[]>>('/Brand/active'),
  create: (data: CreateBrandRequest) => client.post<ApiResponse<BrandDto>>('/Brand', data),
  update: (id: string, data: UpdateBrandRequest) =>
    client.put<ApiResponse<BrandDto>>(`/Brand/${id}`, data),
  delete: (id: string) => client.delete<ApiResponse<{ message: string }>>(`/Brand/${id}`),
};
