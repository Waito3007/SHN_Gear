import api from './client';
import type {
  PagedResult,
  ProductDetail,
  ProductFilterRequest,
  ProductListItem,
} from '../types';

async function hydrateProductImages(items: ProductListItem[]): Promise<ProductListItem[]> {
  const tasks = items.map(async (item) => {
    if (item.imageUrl || item.imageUrls?.length) {
      return item;
    }

    try {
      const detail = await api.get<ProductDetail>(`/Product/slug/${item.slug}`);
      const firstImage = detail.data.imageUrls?.[0];
      return firstImage ? { ...item, imageUrl: firstImage } : item;
    } catch {
      return item;
    }
  });

  return Promise.all(tasks);
}

export const productApi = {
  getPaged: (page = 1, pageSize = 20) =>
    api.get<PagedResult<ProductListItem>>('/Product', {
      params: { page, pageSize },
    }),

  getBySlug: (slug: string) =>
    api.get<ProductDetail>(`/Product/slug/${slug}`),

  getById: (id: string) =>
    api.get<ProductDetail>(`/Product/${id}`),

  getPagedWithImages: async (page = 1, pageSize = 20) => {
    const res = await api.get<PagedResult<ProductListItem>>('/Product', {
      params: { page, pageSize },
    });

    const hydratedItems = await hydrateProductImages(res.data.items);
    return {
      ...res,
      data: {
        ...res.data,
        items: hydratedItems,
      },
    };
  },

  search: (params: ProductFilterRequest) =>
    api.get<PagedResult<ProductListItem>>('/Product/search', {
      params,
    }),

  searchWithImages: async (params: ProductFilterRequest) => {
    const res = await api.get<PagedResult<ProductListItem>>('/Product/search', {
      params,
    });

    const hydratedItems = await hydrateProductImages(res.data.items);
    return {
      ...res,
      data: {
        ...res.data,
        items: hydratedItems,
      },
    };
  },

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
