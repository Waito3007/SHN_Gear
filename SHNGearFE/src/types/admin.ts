// Admin types
export interface RoleDto {
  id: string;
  name: string;
  description: string;
  permissions: PermissionDto[];
}

export interface PermissionDto {
  id: string;
  name: string;
  description: string;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
}

export interface CreatePermissionRequest {
  name: string;
  description?: string;
}

export interface UpdatePermissionRequest {
  name: string;
  description?: string;
}

export interface UpdateAccountRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  address?: string;
}

export interface CreateProductRequest {
  code: string;
  name: string;
  slug: string;
  description?: string;
  categoryId: string;
  brandId: string;
  imageUrls?: string[];
  tags?: string[];
  attributes?: Record<string, string>;
  variants: ProductVariantRequest[];
}

export interface UpdateProductRequest {
  id: string;
  code: string;
  name: string;
  slug: string;
  description?: string;
  categoryId: string;
  brandId: string;
  imageUrls?: string[];
  tags?: string[];
  attributes?: Record<string, string>;
  variants: ProductVariantRequest[];
}

export interface ProductVariantRequest {
  id?: string;
  sku: string;
  name: string;
  quantity: number;
  safetyStock: number;
  basePrice: number;
  salePrice?: number;
  currency: string;
  attributes?: Record<string, string>;
}

export interface ImageUploadResult {
  url: string;
  publicId: string;
  bytes: number;
  format: string;
}

export interface BrandDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateBrandRequest {
  name: string;
  description?: string;
}

export interface UpdateBrandRequest {
  name: string;
  description?: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  parentCategoryId?: string;
  isActive: boolean;
}

export interface CategoryTreeDto {
  id: string;
  name: string;
  slug: string;
  parentCategoryId?: string;
  children: CategoryTreeDto[];
  isActive: boolean;
}

export interface CreateCategoryRequest {
  name: string;
  slug: string;
  parentCategoryId?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  slug: string;
  parentCategoryId?: string;
}
