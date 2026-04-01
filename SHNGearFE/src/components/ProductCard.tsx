import { Link } from 'react-router-dom';
import type { ProductListItem } from '../types';
import { resolveImageUrl } from '../utils/image';

function formatPrice(price: number, currency = 'VND') {
  const locale = currency === 'VND' ? 'vi-VN' : 'en-US';
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(price);
}

export default function ProductCard({ product }: { product: ProductListItem }) {
  const hasDiscount = product.salePrice != null && product.salePrice < product.basePrice;
  const firstImage = product.imageUrl || product.imageUrls?.[0];
  const imageSrc = resolveImageUrl(firstImage);

  return (
    <Link
      to={`/product/${product.slug}`}
      className="group block gradient-card rounded-2xl border border-gray-100 overflow-hidden hover:shadow-xl hover:-translate-y-1 transition-all duration-300"
    >
      {/* Product image */}
      <div className="aspect-square bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center relative overflow-hidden">
        {imageSrc ? (
          <img
            src={imageSrc}
            alt={product.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        ) : (
          <span className="text-4xl font-bold text-gray-300 group-hover:scale-110 transition-transform duration-300">
            {product.name.charAt(0)}
          </span>
        )}
        {hasDiscount && (
          <span className="absolute top-3 right-3 bg-black text-white text-xs font-bold px-2 py-1 rounded-full">
            SALE
          </span>
        )}
      </div>

      {/* Content */}
      <div className="p-4">
        <p className="text-xs text-gray-400 uppercase tracking-wider mb-1">
          {product.brandName}
        </p>
        <h3 className="text-sm font-semibold text-gray-900 line-clamp-2 group-hover:text-black transition-colors">
          {product.name}
        </h3>
        <p className="text-xs text-gray-500 mt-1">{product.categoryName}</p>
        <div className="flex items-center gap-2 mt-3">
          {hasDiscount ? (
            <>
              <span className="text-sm font-bold text-black">
                {formatPrice(product.salePrice!, product.currency)}
              </span>
              <span className="text-xs text-gray-400 line-through">
                {formatPrice(product.basePrice, product.currency)}
              </span>
            </>
          ) : (
            <span className="text-sm font-bold text-black">
              {formatPrice(product.basePrice, product.currency)}
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
