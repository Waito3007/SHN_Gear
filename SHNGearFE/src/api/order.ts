import api from './client';
import type {
  ApiResponse,
  CreatePayPalOrderResponse,
  CreateOrderRequest,
  OrderResponse,
  OrderStatus,
  PayPalClientConfigResponse,
  PagedResult,
} from '../types';

export const orderApi = {
  checkout: (request: CreateOrderRequest) =>
    api.post<ApiResponse<OrderResponse>>('/Order/checkout', request),

  getPayPalClientConfig: () =>
    api.get<ApiResponse<PayPalClientConfigResponse>>('/Order/paypal/client-config'),

  createPayPalOrder: () =>
    api.post<ApiResponse<CreatePayPalOrderResponse>>('/Order/paypal/create-order', {}),

  getMyOrders: (page = 1, pageSize = 10) =>
    api.get<ApiResponse<PagedResult<OrderResponse>>>(`/Order/my-orders?page=${page}&pageSize=${pageSize}`),

  getMyOrderById: (id: string) =>
    api.get<ApiResponse<OrderResponse>>(`/Order/my-orders/${id}`),

  cancelMyOrder: (id: string, reason?: string) =>
    api.patch<ApiResponse<OrderResponse>>(`/Order/my-orders/${id}/cancel`, { reason }),

  getAdminOrders: (status?: OrderStatus, page = 1, pageSize = 20) => {
    const statusQuery = typeof status === 'number' ? `&status=${status}` : '';
    return api.get<ApiResponse<PagedResult<OrderResponse>>>(`/Order/admin?page=${page}&pageSize=${pageSize}${statusQuery}`);
  },

  updateOrderStatus: (id: string, status: OrderStatus) =>
    api.patch<ApiResponse<OrderResponse>>(`/Order/admin/${id}/status`, { status }),

  approveRefund: (id: string, amountUsd?: number, reason?: string) =>
    api.post<ApiResponse<OrderResponse>>(`/Order/admin/${id}/approve-refund`, {
      amountUsd,
      reason,
    }),
};
