import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { productApi } from '../api/product';
import type { ProductDetail } from '../types';
import { ChevronRight } from 'lucide-react';

function formatPrice(price: number, currency = 'VND') {
  const locale = currency === 'VND' ? 'vi-VN' : 'en-US';
  return new Intl.NumberFormat(locale, { style: 'currency', currency, maximumFractionDigits: 0 }).format(price);
}

export default function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedVariant, setSelectedVariant] = useState(0);

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    productApi
      .getBySlug(slug)
      .then((res) => {
        setProduct(res.data);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [slug]);

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <div className="animate-pulse grid md:grid-cols-2 gap-12">
          <div className="aspect-square bg-gray-100 rounded-2xl" />
          <div className="space-y-4">
            <div className="h-4 bg-gray-100 rounded w-1/4" />
            <div className="h-8 bg-gray-100 rounded w-3/4" />
            <div className="h-4 bg-gray-100 rounded w-1/2" />
            <div className="h-6 bg-gray-100 rounded w-1/3 mt-4" />
          </div>
        </div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 text-center">
        <h2 className="text-2xl font-bold text-gray-900">Product not found</h2>
        <Link to="/products" className="mt-4 inline-block text-sm text-black underline">
          Back to products
        </Link>
      </div>
    );
  }

  const variant = product.variants[selectedVariant];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-1 text-sm text-gray-500 mb-8">
        <Link to="/" className="hover:text-black">Home</Link>
        <ChevronRight className="w-3 h-3" />
        <Link to="/products" className="hover:text-black">Products</Link>
        <ChevronRight className="w-3 h-3" />
        <span className="text-black font-medium truncate">{product.name}</span>
      </nav>

      <div className="grid md:grid-cols-2 gap-12">
        {/* Image */}
        <div className="aspect-square bg-gradient-to-br from-gray-50 to-gray-100 rounded-2xl flex items-center justify-center border border-gray-100">
          {product.imageUrls.length > 0 ? (
            <img
              src={product.imageUrls[0]}
              alt={product.name}
              className="w-full h-full object-cover rounded-2xl"
            />
          ) : (
            <span className="text-8xl font-bold text-gray-200">
              {product.name.charAt(0)}
            </span>
          )}
        </div>

        {/* Info */}
        <div>
          <p className="text-sm text-gray-400 uppercase tracking-wider">
            {product.brandName}
          </p>
          <h1 className="text-3xl font-bold text-gray-900 mt-2">{product.name}</h1>
          <p className="text-sm text-gray-500 mt-1">
            {product.categoryName} &middot; Code: {product.code}
          </p>

          {product.description && (
            <p className="text-gray-600 mt-6 leading-relaxed">{product.description}</p>
          )}

          {/* Tags */}
          {product.tags.length > 0 && (
            <div className="flex flex-wrap gap-2 mt-4">
              {product.tags.map((tag) => (
                <span
                  key={tag}
                  className="px-3 py-1 bg-gray-100 rounded-full text-xs font-medium text-gray-600"
                >
                  {tag}
                </span>
              ))}
            </div>
          )}

          {/* Attributes */}
          {product.attributes.length > 0 && (
            <div className="mt-6 space-y-2">
              <h3 className="text-sm font-semibold text-gray-900">Specifications</h3>
              <div className="grid grid-cols-2 gap-2">
                {product.attributes.map((attr) => (
                  <div key={attr.attributeDefinitionId} className="text-sm">
                    <span className="text-gray-500">{attr.name}:</span>{' '}
                    <span className="font-medium">{attr.value}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Variants */}
          {product.variants.length > 0 && (
            <div className="mt-8">
              <h3 className="text-sm font-semibold text-gray-900 mb-3">
                {product.variants.length > 1 ? 'Choose Variant' : 'Variant'}
              </h3>
              <div className="flex flex-wrap gap-2">
                {product.variants.map((v, i) => (
                  <button
                    key={v.id}
                    onClick={() => setSelectedVariant(i)}
                    className={`px-4 py-2 rounded-xl text-sm font-medium border transition-all ${
                      i === selectedVariant
                        ? 'bg-black text-white border-black'
                        : 'border-gray-200 hover:border-black'
                    }`}
                  >
                    {v.name || v.sku}
                  </button>
                ))}
              </div>

              {variant && (
                <div className="mt-6 p-6 bg-gray-50 rounded-2xl">
                  <div className="flex items-baseline gap-3">
                    {variant.salePrice != null && variant.salePrice < variant.basePrice ? (
                      <>
                        <span className="text-2xl font-bold text-black">
                          {formatPrice(variant.salePrice, variant.currency)}
                        </span>
                        <span className="text-lg text-gray-400 line-through">
                          {formatPrice(variant.basePrice, variant.currency)}
                        </span>
                      </>
                    ) : (
                      <span className="text-2xl font-bold text-black">
                        {formatPrice(variant.basePrice, variant.currency)}
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-500 mt-2">
                    SKU: {variant.sku}
                    {variant.quantity > 0 ? (
                      <span className="ml-3 text-green-600">In stock ({variant.quantity})</span>
                    ) : (
                      <span className="ml-3 text-red-500">Out of stock</span>
                    )}
                  </p>

                  {variant.attributes.length > 0 && (
                    <div className="flex flex-wrap gap-3 mt-3">
                      {variant.attributes.map((a) => (
                        <span key={a.attributeDefinitionId} className="text-xs text-gray-600">
                          {a.name}: <strong>{a.value}</strong>
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
