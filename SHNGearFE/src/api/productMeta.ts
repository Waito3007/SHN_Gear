import client from './client';
import type { ApiResponse, BrandOption, CategoryOption } from '../types';

export const productMetaApi = {
  getCategories: () => client.get<ApiResponse<CategoryOption[]>>('/ProductMeta/categories'),
  getBrands: () => client.get<ApiResponse<BrandOption[]>>('/ProductMeta/brands'),
};
