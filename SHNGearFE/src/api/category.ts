import client from './client';
import type { ApiResponse } from '../types';
import type {
  CategoryDto,
  CategoryTreeDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../types/admin';

export const categoryApi = {
  getAll: () => client.get<ApiResponse<CategoryDto[]>>('/Category'),
  getById: (id: string) => client.get<ApiResponse<CategoryDto>>(`/Category/${id}`),
  getActive: () => client.get<ApiResponse<CategoryDto[]>>('/Category/active'),
  getTree: () => client.get<ApiResponse<CategoryTreeDto[]>>('/Category/tree'),
  create: (data: CreateCategoryRequest) =>
    client.post<ApiResponse<CategoryDto>>('/Category', data),
  update: (id: string, data: UpdateCategoryRequest) =>
    client.put<ApiResponse<CategoryDto>>(`/Category/${id}`, data),
  delete: (id: string) => client.delete<ApiResponse<{ message: string }>>(`/Category/${id}`),
};
