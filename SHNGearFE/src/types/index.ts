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

export interface SendOtpRequest {
  email: string;
}

export interface VerifyOtpRequest {
  email: string;
  otp: string;
}

export interface VerifyForgotPasswordOtpResponse {
  verificationToken: string;
  expiresAt: string;
}

export interface ResetForgotPasswordRequest {
  email: string;
  verificationToken: string;
  newPassword: string;
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
  imageUrl?: string;
  imageUrls?: string[];
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

export interface CategoryOption {
  id: string;
  name: string;
  slug: string;
}

export interface BrandOption {
  id: string;
  name: string;
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

export type OrderStatus = 0 | 1 | 2 | 3 | 4 | 5;
export type PaymentProvider = 0 | 1 | 2 | 3;
export type PaymentStatus = 0 | 1 | 2 | 3;

export interface CreateOrderRequest {
  deliveryAddressId: string;
  paymentProvider: PaymentProvider;
  paymentToken?: string;
  note?: string;
}

export interface PayPalClientConfigResponse {
  clientId: string;
  currencyCode: string;
}

export interface CreatePayPalOrderResponse {
  orderId: string;
  currencyCode: string;
  amountUsd: number;
  appliedVndPerUsdRate: number;
  usedFallbackRate: boolean;
}

export interface OrderItemResponse {
  id: string;
  productVariantId: string;
  productName: string;
  variantName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  subTotal: number;
}

export interface OrderResponse {
  id: string;
  code: string;
  accountId: string;
  deliveryAddressId: string;
  status: OrderStatus;
  paymentProvider: PaymentProvider;
  paymentStatus: PaymentStatus;
  subTotal: number;
  shippingFee: number;
  totalAmount: number;
  note?: string;
  paymentTransactionId?: string;
  paidAt?: string;
  cancelledAt?: string;
  cancelledReason?: string;
  createdAt: string;
  items: OrderItemResponse[];
}

export interface AddressDto {
  id: string;
  recipientName: string;
  phoneNumber: string;
  province: string;
  district: string;
  ward: string;
  street: string;
  note?: string;
  isDefault: boolean;
}

export interface CreateAddressRequest {
  recipientName: string;
  phoneNumber: string;
  province: string;
  district: string;
  ward: string;
  street: string;
  note?: string;
  isDefault: boolean;
}

export interface UpdateAddressRequest {
  recipientName: string;
  phoneNumber: string;
  province: string;
  district: string;
  ward: string;
  street: string;
  note?: string;
  isDefault: boolean;
}

// Re-export admin types
export type {
  RoleDto,
  PermissionDto,
  UpdateAccountRequest,
  CreateProductRequest,
  UpdateProductRequest,
  ProductVariantRequest,
  ImageUploadResult,
  BrandDto,
  CreateBrandRequest,
  UpdateBrandRequest,
  CategoryDto,
  CategoryTreeDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from './admin';
