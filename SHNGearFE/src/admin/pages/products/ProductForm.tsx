import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import ImageUploader from '../../../components/ImageUploader';
import { adminProductApi } from '../../../api/adminProduct';
import { productMetaApi } from '../../../api/productMeta';
import type { ProductDetail, CreateProductRequest, UpdateProductRequest, CategoryOption, BrandOption } from '../../../types';
import toast, { Toaster } from 'react-hot-toast';

const productSchema = z.object({
  code: z.string().min(1, 'Code is required'),
  name: z.string().min(1, 'Name is required'),
  slug: z.string().min(1, 'Slug is required'),
  description: z.string().optional(),
  categoryId: z.string().min(1, 'Please select a category'),
  brandId: z.string().min(1, 'Please select a brand'),
  tags: z.string().optional(),
});

type ProductFormData = z.infer<typeof productSchema>;

interface ProductFormProps {
  product?: ProductDetail;
  mode: 'create' | 'edit';
}

const toAttributeMap = (attributes?: { attributeDefinitionId: string; value: string }[]) => {
  if (!attributes?.length) return undefined;

  return attributes.reduce<Record<string, string>>((acc, item) => {
    if (item.attributeDefinitionId && item.value?.trim()) {
      acc[item.attributeDefinitionId] = item.value;
    }
    return acc;
  }, {});
};

export default function ProductForm({ product, mode }: ProductFormProps) {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [loadingMeta, setLoadingMeta] = useState(true);
  const [categories, setCategories] = useState<CategoryOption[]>([]);
  const [brands, setBrands] = useState<BrandOption[]>([]);
  const [imageUrls, setImageUrls] = useState<string[]>(product?.imageUrls || []);
  
  // Simple variant for MVP - just one variant with basic fields
  const [variantSku, setVariantSku] = useState(product?.variants[0]?.sku || '');
  const [variantName, setVariantName] = useState(product?.variants[0]?.name || 'Default');
  const [quantity, setQuantity] = useState(product?.variants[0]?.quantity || 0);
  const [basePrice, setBasePrice] = useState(product?.variants[0]?.basePrice || 0);
  const [salePrice, setSalePrice] = useState(product?.variants[0]?.salePrice || '');

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    reset,
  } = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      code: product?.code || '',
      name: product?.name || '',
      slug: product?.slug || '',
      description: product?.description || '',
      categoryId: product?.categoryId || '',
      brandId: product?.brandId || '',
      tags: product?.tags?.join(', ') || '',
    },
  });

  useEffect(() => {
    if (!product) return;

    reset({
      code: product.code || '',
      name: product.name || '',
      slug: product.slug || '',
      description: product.description || '',
      categoryId: product.categoryId || '',
      brandId: product.brandId || '',
      tags: product.tags?.join(', ') || '',
    });

    setImageUrls(product.imageUrls || []);
    setVariantSku(product.variants[0]?.sku || '');
    setVariantName(product.variants[0]?.name || 'Default');
    setQuantity(product.variants[0]?.quantity || 0);
    setBasePrice(product.variants[0]?.basePrice || 0);
    setSalePrice(product.variants[0]?.salePrice || '');
  }, [product, reset]);

  useEffect(() => {
    const loadMeta = async () => {
      try {
        setLoadingMeta(true);
        const [categoriesRes, brandsRes] = await Promise.all([
          productMetaApi.getCategories(),
          productMetaApi.getBrands(),
        ]);
        setCategories(categoriesRes.data.data || []);
        setBrands(brandsRes.data.data || []);
      } catch (error) {
        console.error('Failed to load product metadata:', error);
        toast.error('Failed to load categories/brands');
      } finally {
        setLoadingMeta(false);
      }
    };

    loadMeta();
  }, []);

  // Auto-generate slug from name
  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const name = e.target.value;
    if (!product) { // Only auto-generate for new products
      const slug = name
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-|-$/g, '');
      setValue('slug', slug);
    }
  };

  const onSubmit = async (data: ProductFormData) => {
    if (imageUrls.length === 0) {
      toast.error('Please upload at least one product image');
      return;
    }

    if (!variantSku) {
      toast.error('Variant SKU is required');
      return;
    }

    if (basePrice <= 0) {
      toast.error('Base price must be greater than 0');
      return;
    }

    const tags = data.tags ? data.tags.split(',').map(t => t.trim()).filter(Boolean) : [];
    
    const firstVariant = product?.variants[0];
    const variant = {
      id: firstVariant?.id,
      sku: variantSku,
      name: variantName,
      quantity: quantity,
      safetyStock: firstVariant?.safetyStock ?? 0,
      basePrice: basePrice,
      salePrice: salePrice ? parseFloat(salePrice.toString()) : undefined,
      currency: firstVariant?.currency || 'VND',
    };

    try {
      setSubmitting(true);
      
      if (mode === 'create') {
        const request: CreateProductRequest = {
          ...data,
          imageUrls,
          tags,
          variants: [variant],
        };
        await adminProductApi.create(request);
        toast.success('Product created successfully');
      } else if (product) {
        const existingVariants = product.variants.slice(1).map((v) => ({
          id: v.id,
          sku: v.sku,
          name: v.name || 'Default',
          quantity: v.quantity,
          safetyStock: v.safetyStock,
          basePrice: v.basePrice,
          salePrice: v.salePrice,
          currency: v.currency || 'VND',
          attributes: toAttributeMap(v.attributes),
        }));

        const request: UpdateProductRequest = {
          id: product.id,
          ...data,
          imageUrls,
          tags,
          attributes: toAttributeMap(product.attributes),
          variants: [
            {
              ...variant,
              attributes: toAttributeMap(product.variants[0]?.attributes),
            },
            ...existingVariants,
          ],
        };
        await adminProductApi.update(product.id, request);
        toast.success('Product updated successfully');
      }

      navigate('/admin/products');
    } catch (error: unknown) {
      console.error('Submit error:', error);
      const errorMessage = (error as { response?: { data?: { message?: string; errorMessage?: string } } })?.response?.data?.errorMessage
        || (error as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(errorMessage || 'Failed to save product');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-4xl">
      <Toaster position="top-right" />
      
      <button
        onClick={() => navigate('/admin/products')}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft size={20} />
        Back to Products
      </button>

      <h1 className="text-3xl font-bold mb-6">
        {mode === 'create' ? 'Create New Product' : 'Edit Product'}
      </h1>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* Product Images */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">Product Images</h2>
          <ImageUploader
            existingImages={imageUrls}
            onUploadComplete={setImageUrls}
            maxFiles={8}
          />
        </div>

        {/* Basic Information */}
        <div className="bg-white p-6 rounded-lg shadow space-y-4">
          <h2 className="text-xl font-semibold mb-4">Basic Information</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Product Code *
              </label>
              <input
                {...register('code')}
                type="text"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="SKU-001"
              />
              {errors.code && (
                <p className="mt-1 text-sm text-red-600">{errors.code.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Product Name *
              </label>
              <input
                {...register('name')}
                type="text"
                onChange={handleNameChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Product name"
              />
              {errors.name && (
                <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              URL Slug *
            </label>
            <input
              {...register('slug')}
              type="text"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="product-name"
            />
            {errors.slug && (
              <p className="mt-1 text-sm text-red-600">{errors.slug.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              {...register('description')}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Product description..."
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Category *
              </label>
              <select
                {...register('categoryId')}
                disabled={loadingMeta}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">{loadingMeta ? 'Loading categories...' : 'Select a category'}</option>
                {product?.categoryId && !categories.some((x) => x.id === product.categoryId) && (
                  <option value={product.categoryId}>
                    {product.categoryName || 'Current category'}
                  </option>
                )}
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
              {errors.categoryId && (
                <p className="mt-1 text-sm text-red-600">{errors.categoryId.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Brand *
              </label>
              <select
                {...register('brandId')}
                disabled={loadingMeta}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">{loadingMeta ? 'Loading brands...' : 'Select a brand'}</option>
                {product?.brandId && !brands.some((x) => x.id === product.brandId) && (
                  <option value={product.brandId}>
                    {product.brandName || 'Current brand'}
                  </option>
                )}
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>
                    {brand.name}
                  </option>
                ))}
              </select>
              {errors.brandId && (
                <p className="mt-1 text-sm text-red-600">{errors.brandId.message}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tags
            </label>
            <input
              {...register('tags')}
              type="text"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="tag1, tag2, tag3"
            />
            <p className="mt-1 text-xs text-gray-500">Comma-separated tags</p>
          </div>
        </div>

        {/* Variant Information */}
        <div className="bg-white p-6 rounded-lg shadow space-y-4">
          <h2 className="text-xl font-semibold mb-4">Product Variant</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                SKU *
              </label>
              <input
                value={variantSku}
                onChange={(e) => setVariantSku(e.target.value)}
                type="text"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="VARIANT-SKU-001"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Variant Name
              </label>
              <input
                value={variantName}
                onChange={(e) => setVariantName(e.target.value)}
                type="text"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Default"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Quantity *
              </label>
              <input
                value={quantity}
                onChange={(e) => setQuantity(parseInt(e.target.value) || 0)}
                type="number"
                min="0"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Base Price (VND) *
              </label>
              <input
                value={basePrice}
                onChange={(e) => setBasePrice(parseFloat(e.target.value) || 0)}
                type="number"
                min="0"
                step="0.01"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Sale Price (VND)
              </label>
              <input
                value={salePrice}
                onChange={(e) => setSalePrice(e.target.value)}
                type="number"
                min="0"
                step="0.01"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        {/* Submit buttons */}
        <div className="flex gap-4">
          <button
            type="submit"
            disabled={submitting || loadingMeta}
            className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {submitting ? (
              <Loader2 className="animate-spin" size={20} />
            ) : (
              <Save size={20} />
            )}
            {mode === 'create' ? 'Create Product' : 'Update Product'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/admin/products')}
            className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
