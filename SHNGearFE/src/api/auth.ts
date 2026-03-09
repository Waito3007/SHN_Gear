import api from './client';
import type {
  ApiResponse,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from '../types';

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<ApiResponse<LoginResponse>>('/Auth/login', data),

  register: (data: RegisterRequest) =>
    api.post<ApiResponse<LoginResponse>>('/Auth/register', data),

  logout: () => api.post<ApiResponse>('/Auth/logout'),

  refreshToken: (accessToken: string, refreshToken: string) =>
    api.post<ApiResponse<LoginResponse>>('/Auth/refresh-token', {
      accessToken,
      refreshToken,
    }),
};
