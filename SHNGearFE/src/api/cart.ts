import api from './client';
import type { ApiResponse, CartDto, AddToCartRequest, UpdateCartItemRequest } from '../types';

export const cartApi = {
  // Get current user's cart
  getCart: () =>
    api.get<ApiResponse<CartDto>>('/Cart'),

  // Add item to cart
  addItem: (request: AddToCartRequest) =>
    api.post<ApiResponse<CartDto>>('/Cart/items', request),

  // Update item quantity
  updateItem: (variantId: string, request: UpdateCartItemRequest) =>
    api.put<ApiResponse<CartDto>>(`/Cart/items/${variantId}`, request),

  // Remove item from cart
  removeItem: (variantId: string) =>
    api.delete<ApiResponse<CartDto>>(`/Cart/items/${variantId}`),

  // Clear entire cart
  clearCart: () =>
    api.delete<ApiResponse<void>>('/Cart'),
};
