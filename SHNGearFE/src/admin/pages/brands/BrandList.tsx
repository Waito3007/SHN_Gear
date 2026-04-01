import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { brandApi } from '../../../api/brand';
import { Plus, Trash2, Edit2, Search, Tags } from 'lucide-react';
import type { BrandDto } from '../../../types';
import { usePermission } from '../../../context/PermissionContext';
import toast, { Toaster } from 'react-hot-toast';

export default function BrandList() {
  const { hasAnyPermission } = usePermission();
  const [brands, setBrands] = useState<BrandDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');

  const canManage = hasAnyPermission(['brand.manage', 'brand.create', 'brand.update', 'brand.delete']);

  const loadBrands = async () => {
    try {
      setLoading(true);
      const response = await brandApi.getAll();
      setBrands(response.data?.data || []);
    } catch (error) {
      toast.error('Failed to load brands');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadBrands();
  }, []);

  const filteredBrands = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return brands;
    return brands.filter((brand) =>
      brand.name.toLowerCase().includes(q) ||
      (brand.description || '').toLowerCase().includes(q)
    );
  }, [brands, query]);

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete brand \"${name}\"?`)) return;

    try {
      await brandApi.delete(id);
      toast.success('Brand deleted successfully');
      loadBrands();
    } catch (error: unknown) {
      const errorMessage = (error as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(errorMessage || 'Failed to delete brand');
    }
  };

  if (loading) {
    return <div className="text-gray-500">Loading brands...</div>;
  }

  return (
    <div>
      <Toaster position="top-right" />

      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-3">
          <div className="w-11 h-11 rounded-xl bg-blue-50 text-blue-700 flex items-center justify-center">
            <Tags size={20} />
          </div>
          <div>
            <h1 className="text-3xl font-bold">Brands</h1>
            <p className="text-gray-500 text-sm">Manage product brands for admin operations</p>
          </div>
        </div>

        {canManage && (
          <Link
            to="/admin/brands/create"
            className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700"
          >
            <Plus size={18} />
            Add Brand
          </Link>
        )}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4 mb-4">
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} />
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search by brand name or description"
            className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
      </div>

      {filteredBrands.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center text-gray-500">
          No brands found.
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Name</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Description</th>
                <th className="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {filteredBrands.map((brand) => (
                <tr key={brand.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{brand.name}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{brand.description || '-'}</td>
                  <td className="px-6 py-4 text-right">
                    {canManage ? (
                      <div className="inline-flex gap-2">
                        <Link
                          to={`/admin/brands/edit/${brand.id}`}
                          className="inline-flex items-center gap-1 px-3 py-2 rounded bg-blue-50 text-blue-700 hover:bg-blue-100"
                        >
                          <Edit2 size={16} />
                          Edit
                        </Link>
                        <button
                          onClick={() => handleDelete(brand.id, brand.name)}
                          className="inline-flex items-center gap-1 px-3 py-2 rounded bg-red-50 text-red-700 hover:bg-red-100"
                        >
                          <Trash2 size={16} />
                          Delete
                        </button>
                      </div>
                    ) : (
                      <span className="text-xs text-gray-400">No permission</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
