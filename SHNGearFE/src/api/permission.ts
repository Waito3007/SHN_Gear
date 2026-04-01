import client from './client';
import type { ApiResponse } from '../types';
import type { PermissionDto, CreatePermissionRequest, UpdatePermissionRequest } from '../types/admin';

export const permissionApi = {
  getAll: () => client.get<ApiResponse<PermissionDto[]>>('/Permission'),
  
  getById: (id: string) => client.get<ApiResponse<PermissionDto>>(`/Permission/${id}`),
  
  create: (data: CreatePermissionRequest) => 
    client.post<ApiResponse<PermissionDto>>('/Permission', data),
  
  update: (id: string, data: UpdatePermissionRequest) => 
    client.put<ApiResponse<PermissionDto>>(`/Permission/${id}`, data),
  
  delete: (id: string) => client.delete(`/Permission/${id}`),
};
