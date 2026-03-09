import api from './client';
import type {
  PagedResult,
  ProductDetail,
  ProductFilterRequest,
  ProductListItem,
} from '../types';

export const productApi = {
  getPaged: (page = 1, pageSize = 20) =>
    api.get<PagedResult<ProductListItem>>('/Product', {
      params: { page, pageSize },
    }),

  getBySlug: (slug: string) =>
    api.get<ProductDetail>(`/Product/slug/${slug}`),

  getById: (id: string) =>
    api.get<ProductDetail>(`/Product/${id}`),

  search: (params: ProductFilterRequest) =>
    api.get<PagedResult<ProductListItem>>('/Product/search', {
      params,
    }),

  getByCategory: (categoryId: string, page = 1, pageSize = 20) =>
    api.get<PagedResult<ProductListItem>>(
      `/Product/category/${categoryId}`,
      { params: { page, pageSize } }
    ),

  getByBrand: (brandId: string, page = 1, pageSize = 20) =>
    api.get<PagedResult<ProductListItem>>(
      `/Product/brand/${brandId}`,
      { params: { page, pageSize } }
    ),
};
