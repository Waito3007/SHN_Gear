import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { categoryApi } from '../../../api/category';
import { Plus, Trash2, Edit2, Search, FolderTree } from 'lucide-react';
import type { CategoryDto } from '../../../types';
import { usePermission } from '../../../context/PermissionContext';
import toast, { Toaster } from 'react-hot-toast';

export default function CategoryList() {
  const { hasAnyPermission } = usePermission();
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');

  const canManage = hasAnyPermission(['category.manage', 'category.create', 'category.update', 'category.delete']);

  const loadCategories = async () => {
    try {
      setLoading(true);
      const response = await categoryApi.getAll();
      setCategories(response.data?.data || []);
    } catch (error) {
      toast.error('Failed to load categories');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  const byId = useMemo(() => {
    const index = new Map<string, string>();
    categories.forEach((item) => index.set(item.id, item.name));
    return index;
  }, [categories]);

  const filteredCategories = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return categories;
    return categories.filter((item) =>
      item.name.toLowerCase().includes(q) ||
      item.slug.toLowerCase().includes(q)
    );
  }, [categories, query]);

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete category \"${name}\"?`)) return;

    try {
      await categoryApi.delete(id);
      toast.success('Category deleted successfully');
      loadCategories();
    } catch (error: unknown) {
      const errorMessage = (error as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(errorMessage || 'Failed to delete category');
    }
  };

  if (loading) {
    return <div className="text-gray-500">Loading categories...</div>;
  }

  return (
    <div>
      <Toaster position="top-right" />

      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-3">
          <div className="w-11 h-11 rounded-xl bg-emerald-50 text-emerald-700 flex items-center justify-center">
            <FolderTree size={20} />
          </div>
          <div>
            <h1 className="text-3xl font-bold">Categories</h1>
            <p className="text-gray-500 text-sm">Organize category taxonomy and hierarchy</p>
          </div>
        </div>

        {canManage && (
          <Link
            to="/admin/categories/create"
            className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700"
          >
            <Plus size={18} />
            Add Category
          </Link>
        )}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4 mb-4">
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} />
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search by category name or slug"
            className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
          />
        </div>
      </div>

      {filteredCategories.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center text-gray-500">
          No categories found.
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Name</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Slug</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Parent</th>
                <th className="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {filteredCategories.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{item.name}</td>
                  <td className="px-6 py-4 text-sm text-gray-700 font-mono">{item.slug}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">
                    {item.parentCategoryId ? byId.get(item.parentCategoryId) || 'Unknown parent' : 'Root'}
                  </td>
                  <td className="px-6 py-4 text-right">
                    {canManage ? (
                      <div className="inline-flex gap-2">
                        <Link
                          to={`/admin/categories/edit/${item.id}`}
                          className="inline-flex items-center gap-1 px-3 py-2 rounded bg-emerald-50 text-emerald-700 hover:bg-emerald-100"
                        >
                          <Edit2 size={16} />
                          Edit
                        </Link>
                        <button
                          onClick={() => handleDelete(item.id, item.name)}
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
