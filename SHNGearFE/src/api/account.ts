import client from './client';
import type { ApiResponse, AccountDto, UpdateAccountRequest } from '../types';

export const accountApi = {
  getMyProfile: () => client.get<ApiResponse<AccountDto>>('/Account/me'),

  updateMyProfile: (data: UpdateAccountRequest) =>
    client.put<ApiResponse<AccountDto>>('/Account/me', data),

  getAll: () => client.get<ApiResponse<AccountDto[]>>('/Account'),
  
  getById: (id: string) => client.get<ApiResponse<AccountDto>>(`/Account/${id}`),
  
  deleteAccount: (id: string) => client.delete(`/Account/${id}`),
  
  assignRole: (accountId: string, roleId: string) => 
    client.post(`/Account/${accountId}/roles/${roleId}`),
  
  removeRole: (accountId: string, roleId: string) =>
    client.delete(`/Account/${accountId}/roles/${roleId}`),
    
  updateAccount: (id: string, data: UpdateAccountRequest) =>
    client.put<ApiResponse<AccountDto>>(`/Account/${id}`, data),
};
