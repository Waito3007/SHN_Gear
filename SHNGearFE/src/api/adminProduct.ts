import client from './client';
import type { ProductDetail, CreateProductRequest, UpdateProductRequest } from '../types';

export const adminProductApi = {
  create: (data: CreateProductRequest) => 
    client.post<ProductDetail>('/Product', data),
  
  update: (id: string, data: UpdateProductRequest) => 
    client.put<ProductDetail>(`/Product/${id}`, data),
  
  delete: (id: string) => client.delete(`/Product/${id}`),
};
