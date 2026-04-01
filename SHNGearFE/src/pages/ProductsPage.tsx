import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { productApi } from '../api/product';
import ProductCard from '../components/ProductCard';
import Pagination from '../components/Pagination';
import type { ProductListItem, PagedResult } from '../types';

export default function ProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = parseInt(searchParams.get('page') || '1');
  const [data, setData] = useState<PagedResult<ProductListItem> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    productApi
      .getPagedWithImages(page, 12)
      .then((res) => {
        setData(res.data);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [page]);

  const goToPage = (p: number) => {
    setSearchParams({ page: String(p) });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      {/* Breadcrumb */}
      <nav className="text-sm text-gray-500 mb-6">
        <Link to="/" className="hover:text-black">Home</Link>
        <span className="mx-2">/</span>
        <span className="text-black font-medium">Products</span>
      </nav>

      <h1 className="text-3xl font-bold text-gray-900 mb-8">All Products</h1>

      {loading ? (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {Array.from({ length: 12 }).map((_, i) => (
            <div key={i} className="animate-pulse">
              <div className="aspect-square bg-gray-100 rounded-2xl" />
              <div className="mt-3 h-3 bg-gray-100 rounded w-1/3" />
              <div className="mt-2 h-4 bg-gray-100 rounded w-2/3" />
              <div className="mt-2 h-4 bg-gray-100 rounded w-1/4" />
            </div>
          ))}
        </div>
      ) : data && data.items.length > 0 ? (
        <>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {data.items.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>

          {/* Pagination */}
          <Pagination
            currentPage={page}
            totalPages={data.totalPages}
            hasPreviousPage={data.hasPreviousPage}
            hasNextPage={data.hasNextPage}
            onPageChange={goToPage}
            loading={loading}
          />
        </>
      ) : (
        <div className="text-center py-20">
          <p className="text-gray-400 text-lg">No products found.</p>
        </div>
      )}
    </div>
  );
}
