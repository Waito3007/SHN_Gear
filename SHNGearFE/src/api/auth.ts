import api from './client';
import type {
  ApiResponse,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ResetForgotPasswordRequest,
  SendOtpRequest,
  VerifyForgotPasswordOtpResponse,
  VerifyOtpRequest,
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

  sendVerificationOtp: (data: SendOtpRequest) =>
    api.post<ApiResponse<{ message: string }>>('/Auth/send-verification-otp', data),

  verifyEmailOtp: (data: VerifyOtpRequest) =>
    api.post<ApiResponse<{ message: string }>>('/Auth/verify-email-otp', data),

  sendForgotPasswordOtp: (data: SendOtpRequest) =>
    api.post<ApiResponse<{ message: string }>>('/Auth/forgot-password/send-otp', data),

  verifyForgotPasswordOtp: (data: VerifyOtpRequest) =>
    api.post<ApiResponse<VerifyForgotPasswordOtpResponse>>('/Auth/forgot-password/verify-otp', data),

  resetForgotPassword: (data: ResetForgotPasswordRequest) =>
    api.post<ApiResponse<{ message: string }>>('/Auth/forgot-password/reset', data),
};
