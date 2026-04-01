import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import { categoryApi } from '../../../api/category';
import type { CategoryDto } from '../../../types';
import toast, { Toaster } from 'react-hot-toast';

const categorySchema = z.object({
  name: z.string().min(1, 'Category name is required'),
  slug: z.string().min(1, 'Slug is required'),
  parentCategoryId: z.string().optional(),
});

type CategoryFormData = z.infer<typeof categorySchema>;

interface CategoryFormProps {
  category?: CategoryDto;
  mode: 'create' | 'edit';
}

const toSlug = (value: string) =>
  value
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

export default function CategoryForm({ category, mode }: CategoryFormProps) {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [options, setOptions] = useState<CategoryDto[]>([]);
  const [loadingOptions, setLoadingOptions] = useState(true);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: category?.name || '',
      slug: category?.slug || '',
      parentCategoryId: category?.parentCategoryId || '',
    },
  });

  useEffect(() => {
    const loadOptions = async () => {
      try {
        setLoadingOptions(true);
        const response = await categoryApi.getAll();
        setOptions(response.data?.data || []);
      } catch {
        toast.error('Failed to load categories for parent selection');
      } finally {
        setLoadingOptions(false);
      }
    };

    loadOptions();
  }, []);

  const parentOptions = useMemo(() => {
    if (!category) return options;
    return options.filter((x) => x.id !== category.id);
  }, [options, category]);

  const currentName = watch('name');
  const onNameChange = (value: string) => {
    if (mode === 'create') {
      setValue('slug', toSlug(value));
    }
  };

  const onSubmit = async (data: CategoryFormData) => {
    try {
      setSubmitting(true);

      const payload = {
        ...data,
        slug: toSlug(data.slug),
        parentCategoryId: data.parentCategoryId || undefined,
      };

      if (mode === 'create') {
        await categoryApi.create(payload);
        toast.success('Category created successfully');
      } else if (category) {
        await categoryApi.update(category.id, payload);
        toast.success('Category updated successfully');
      }

      navigate('/admin/categories');
    } catch (error: unknown) {
      const errorMessage = (error as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(errorMessage || 'Failed to save category');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-3xl">
      <Toaster position="top-right" />

      <button
        onClick={() => navigate('/admin/categories')}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft size={20} />
        Back to Categories
      </button>

      <h1 className="text-3xl font-bold mb-6">
        {mode === 'create' ? 'Create New Category' : 'Edit Category'}
      </h1>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="bg-white p-6 rounded-lg shadow space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Category Name *
              </label>
              <input
                {...register('name')}
                type="text"
                onChange={(e) => {
                  register('name').onChange(e);
                  onNameChange(e.target.value);
                }}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Gaming Gear"
              />
              {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Slug *
              </label>
              <input
                {...register('slug')}
                type="text"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="gaming-gear"
              />
              {errors.slug && <p className="mt-1 text-sm text-red-600">{errors.slug.message}</p>}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Parent Category</label>
            <select
              {...register('parentCategoryId')}
              disabled={loadingOptions}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">No parent (root category)</option>
              {parentOptions.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.name}
                </option>
              ))}
            </select>
          </div>

          <p className="text-xs text-gray-500">
            Slug preview: <span className="font-mono">/{toSlug(currentName || '') || 'category-slug'}</span>
          </p>
        </div>

        <div className="flex gap-4">
          <button
            type="submit"
            disabled={submitting}
            className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {submitting ? <Loader2 className="animate-spin" size={20} /> : <Save size={20} />}
            {mode === 'create' ? 'Create Category' : 'Update Category'}
          </button>

          <button
            type="button"
            onClick={() => navigate('/admin/categories')}
            className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
