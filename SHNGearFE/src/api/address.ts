import api from './client';
import type { ApiResponse, AddressDto, CreateAddressRequest, UpdateAddressRequest } from '../types';

export const addressApi = {
  getMyAddresses: () => api.get<ApiResponse<AddressDto[]>>('/Address'),

  getById: (id: string) => api.get<ApiResponse<AddressDto>>(`/Address/${id}`),

  create: (data: CreateAddressRequest) =>
    api.post<ApiResponse<AddressDto>>('/Address', data),

  update: (id: string, data: UpdateAddressRequest) =>
    api.put<ApiResponse<AddressDto>>(`/Address/${id}`, data),

  delete: (id: string) => api.delete<ApiResponse<void>>(`/Address/${id}`),

  setDefault: (id: string) => api.patch<ApiResponse<AddressDto>>(`/Address/${id}/default`),
};
