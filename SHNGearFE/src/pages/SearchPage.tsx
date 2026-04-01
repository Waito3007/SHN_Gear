import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { productApi } from '../api/product';
import ProductCard from '../components/ProductCard';
import Pagination from '../components/Pagination';
import type { ProductListItem, PagedResult } from '../types';
import { Search as SearchIcon } from 'lucide-react';

export default function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const q = searchParams.get('q') || '';
  const page = parseInt(searchParams.get('page') || '1');
  const [searchTerm, setSearchTerm] = useState(q);
  const [data, setData] = useState<PagedResult<ProductListItem> | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!q) return;
    setLoading(true);
    productApi
      .searchWithImages({ searchTerm: q, page, pageSize: 12 })
      .then((res) => {
        setData(res.data);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [q, page]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchTerm.trim()) {
      setSearchParams({ q: searchTerm.trim(), page: '1' });
    }
  };

  const goToPage = (p: number) => {
    setSearchParams({ q, page: String(p) });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      <nav className="text-sm text-gray-500 mb-6">
        <Link to="/" className="hover:text-black">Home</Link>
        <span className="mx-2">/</span>
        <span className="text-black font-medium">Search</span>
      </nav>

      <h1 className="text-3xl font-bold text-gray-900 mb-6">Search Products</h1>

      <form onSubmit={handleSearch} className="max-w-xl mb-10">
        <div className="relative">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by name, brand, category..."
            className="w-full pl-12 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
          />
          <SearchIcon className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <button
            type="submit"
            className="absolute right-2 top-1/2 -translate-y-1/2 gradient-btn text-white px-4 py-1.5 rounded-lg text-sm font-medium"
          >
            Search
          </button>
        </div>
      </form>

      {q && (
        <p className="text-sm text-gray-500 mb-6">
          {loading
            ? 'Searching...'
            : data
            ? `${data.totalCount} result${data.totalCount !== 1 ? 's' : ''} for "${q}"`
            : `No results for "${q}"`}
        </p>
      )}

      {loading ? (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="animate-pulse">
              <div className="aspect-square bg-gray-100 rounded-2xl" />
              <div className="mt-3 h-3 bg-gray-100 rounded w-1/3" />
              <div className="mt-2 h-4 bg-gray-100 rounded w-2/3" />
            </div>
          ))}
        </div>
      ) : data && data.items.length > 0 ? (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {data.items.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      ) : q ? (
        <div className="text-center py-20">
          <p className="text-gray-400 text-lg">No products match your search.</p>
        </div>
      ) : null}

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <Pagination
          currentPage={page}
          totalPages={data.totalPages}
          hasPreviousPage={data.hasPreviousPage}
          hasNextPage={data.hasNextPage}
          onPageChange={goToPage}
          loading={loading}
        />
      )}
    </div>
  );
}
