export interface ApiResponse<T = unknown> {
  success: boolean;
  code: number;
  message: string;
  data: T;
  errors: unknown;
  timestamp: string;
}

export interface LoginRequest {
  emailOrUsername: string;
  password: string;
}

export interface RegisterRequest {
  username?: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  address?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  account: AccountDto;
}

export interface AccountDto {
  id: string;
  username?: string;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  address?: string;
  roles: string[];
  permissions: string[];
}

export interface ProductListItem {
  id: string;
  code: string;
  name: string;
  slug: string;
  brandName: string;
  categoryName: string;
  basePrice: number;
  salePrice?: number;
  currency: string;
}

export interface ProductDetail {
  id: string;
  code: string;
  name: string;
  slug: string;
  description?: string;
  categoryId: string;
  categoryName: string;
  brandId: string;
  brandName: string;
  imageUrls: string[];
  tags: string[];
  attributes: ProductAttribute[];
  variants: ProductVariant[];
}

export interface ProductAttribute {
  attributeDefinitionId: string;
  name: string;
  value: string;
}

export interface ProductVariant {
  id: string;
  sku: string;
  name?: string;
  quantity: number;
  safetyStock: number;
  basePrice: number;
  salePrice?: number;
  currency: string;
  attributes: ProductAttribute[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ProductFilterRequest {
  searchTerm?: string;
  categoryId?: string;
  brandId?: string;
  page?: number;
  pageSize?: number;
}

export interface CartItemDto {
  productVariantId: string;
  productName: string;
  variantName: string;
  sku: string;
  imageUrl?: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  subTotal: number;
  availableStock: number;
}

export interface CartDto {
  accountId: string;
  items: CartItemDto[];
  totalAmount: number;
  totalItems: number;
  updatedAt: string;
}

export interface AddToCartRequest {
  productVariantId: string;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}
