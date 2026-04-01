import client from './client';
import type { ApiResponse } from '../types';
import type { RoleDto, CreateRoleRequest, UpdateRoleRequest } from '../types/admin';

export const roleApi = {
  getAll: () => client.get<ApiResponse<RoleDto[]>>('/Role'),
  
  getById: (id: string) => client.get<ApiResponse<RoleDto>>(`/Role/${id}`),
  
  create: (data: CreateRoleRequest) => client.post<ApiResponse<RoleDto>>('/Role', data),
  
  update: (id: string, data: UpdateRoleRequest) => 
    client.put<ApiResponse<RoleDto>>(`/Role/${id}`, data),
  
  delete: (id: string) => client.delete(`/Role/${id}`),
  
  assignPermission: (roleId: string, permissionId: string) =>
    client.post(`/Role/${roleId}/permissions/${permissionId}`),
    
  removePermission: (roleId: string, permissionId: string) =>
    client.delete(`/Role/${roleId}/permissions/${permissionId}`),
};
