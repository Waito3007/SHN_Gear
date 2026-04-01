import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import { brandApi } from '../../../api/brand';
import type { BrandDto } from '../../../types';
import toast, { Toaster } from 'react-hot-toast';

const brandSchema = z.object({
  name: z.string().min(1, 'Brand name is required'),
  description: z.string().optional(),
});

type BrandFormData = z.infer<typeof brandSchema>;

interface BrandFormProps {
  brand?: BrandDto;
  mode: 'create' | 'edit';
}

export default function BrandForm({ brand, mode }: BrandFormProps) {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<BrandFormData>({
    resolver: zodResolver(brandSchema),
    defaultValues: {
      name: brand?.name || '',
      description: brand?.description || '',
    },
  });

  const onSubmit = async (data: BrandFormData) => {
    try {
      setSubmitting(true);

      if (mode === 'create') {
        await brandApi.create(data);
        toast.success('Brand created successfully');
      } else if (brand) {
        await brandApi.update(brand.id, data);
        toast.success('Brand updated successfully');
      }

      navigate('/admin/brands');
    } catch (error: unknown) {
      const errorMessage = (error as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(errorMessage || 'Failed to save brand');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-2xl">
      <Toaster position="top-right" />

      <button
        onClick={() => navigate('/admin/brands')}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft size={20} />
        Back to Brands
      </button>

      <h1 className="text-3xl font-bold mb-6">
        {mode === 'create' ? 'Create New Brand' : 'Edit Brand'}
      </h1>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="bg-white p-6 rounded-lg shadow space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Brand Name *
            </label>
            <input
              {...register('name')}
              type="text"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Logitech"
            />
            {errors.name && (
              <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              {...register('description')}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Brand description..."
            />
          </div>
        </div>

        <div className="flex gap-4">
          <button
            type="submit"
            disabled={submitting}
            className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {submitting ? (
              <Loader2 className="animate-spin" size={20} />
            ) : (
              <Save size={20} />
            )}
            {mode === 'create' ? 'Create Brand' : 'Update Brand'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/admin/brands')}
            className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
