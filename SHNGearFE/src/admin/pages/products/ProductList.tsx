import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Edit2, Trash2, Plus, Search } from 'lucide-react';
import { adminProductApi } from '../../../api/adminProduct';
import { productApi } from '../../../api/product';
import { usePermission } from '../../../context/PermissionContext';
import Pagination from '../../../components/Pagination';
import type { ProductListItem, PagedResult } from '../../../types';
import toast, { Toaster } from 'react-hot-toast';

export default function ProductList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = parseInt(searchParams.get('page') || '1');
  const pageSize = 10;

  const [data, setData] = useState<PagedResult<ProductListItem> | null>(null);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const { hasPermission } = usePermission();

  const canCreate = hasPermission('product.create');
  const canEdit = hasPermission('product.update');
  const canDelete = hasPermission('product.delete');

  useEffect(() => {
    loadProducts();
  }, [page]);

  const loadProducts = async () => {
    try {
      setLoading(true);
      const response = await productApi.getPaged(page, pageSize);
      setData(response.data);
    } catch (error) {
      console.error('Failed to load products:', error);
      toast.error('Failed to load products');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Are you sure you want to delete "${name}"?`)) {
      return;
    }

    try {
      await adminProductApi.delete(id);
      toast.success('Product deleted successfully');
      loadProducts();
    } catch (error) {
      console.error('Delete error:', error);
      toast.error('Failed to delete product');
    }
  };

  const goToPage = (p: number) => {
    setSearchParams({ page: String(p) });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const filteredProducts = data?.items.filter(p =>
    p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    p.code.toLowerCase().includes(searchQuery.toLowerCase())
  ) || [];

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading products...</div>
      </div>
    );
  }

  return (
    <div>
      <Toaster position="top-right" />
      
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Products</h1>
        {canCreate && (
          <Link
            to="/admin/products/create"
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus size={20} />
            Add Product
          </Link>
        )}
      </div>

      {/* Search */}
      <div className="mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by name or code..."
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
      </div>

      {/* Products Grid */}
      {filteredProducts.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 mb-4">No products found</p>
          {canCreate && (
            <Link
              to="/admin/products/create"
              className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <Plus size={20} />
              Create First Product
            </Link>
          )}
        </div>
      ) : (
        <>
          <div className="bg-white rounded-lg shadow overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Code</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Brand</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Category</th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Price</th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
            {filteredProducts.map((product) => (
              <tr key={product.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-700">{product.code}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{product.name}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{product.brandName}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{product.categoryName}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-right">
                  {product.salePrice ? (
                    <>
                      <span className="text-red-600 font-semibold">${product.salePrice}</span>
                      <span className="ml-2 text-gray-500 line-through">${product.basePrice}</span>
                    </>
                  ) : (
                    <span className="text-gray-900 font-semibold">${product.basePrice}</span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right">
                  <div className="inline-flex gap-2">
                    {canEdit && (
                      <Link
                        to={`/admin/products/edit/${product.id}`}
                        className="inline-flex items-center justify-center gap-2 px-3 py-2 bg-blue-50 text-blue-600 rounded hover:bg-blue-100"
                      >
                        <Edit2 size={16} />
                        Edit
                      </Link>
                    )}
                    {canDelete && (
                      <button
                        onClick={() => handleDelete(product.id, product.name)}
                        className="inline-flex items-center justify-center gap-2 px-3 py-2 bg-red-50 text-red-600 rounded hover:bg-red-100"
                      >
                        <Trash2 size={16} />
                        Delete
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="mt-6">
              <Pagination
                currentPage={page}
                totalPages={data.totalPages}
                hasPreviousPage={data.hasPreviousPage}
                hasNextPage={data.hasNextPage}
                onPageChange={goToPage}
                loading={loading}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
}
