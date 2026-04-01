import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import { permissionApi } from '../../../api/permission';
import type { PermissionDto } from '../../../types/admin';
import toast, { Toaster } from 'react-hot-toast';

const permissionSchema = z.object({
  name: z.string().min(1, 'Permission name is required').regex(
    /^[a-z]+\.[a-z_]+$/,
    'Permission name must be in format: module.action (e.g., products.create)'
  ),
  description: z.string().optional(),
});

type PermissionFormData = z.infer<typeof permissionSchema>;

interface PermissionFormProps {
  permission?: PermissionDto;
  mode: 'create' | 'edit';
}

export default function PermissionForm({ permission, mode }: PermissionFormProps) {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PermissionFormData>({
    resolver: zodResolver(permissionSchema),
    defaultValues: {
      name: permission?.name || '',
      description: permission?.description || '',
    },
  });

  const onSubmit = async (data: PermissionFormData) => {
    try {
      setSubmitting(true);
      
      if (mode === 'create') {
        await permissionApi.create(data);
        toast.success('Permission created successfully');
      } else if (permission) {
        await permissionApi.update(permission.id, data);
        toast.success('Permission updated successfully');
      }

      navigate('/admin/permissions');
    } catch (error: unknown) {
      console.error('Submit error:', error);
      const errorMessage = (error as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(errorMessage || 'Failed to save permission');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-2xl">
      <Toaster position="top-right" />
      
      <button
        onClick={() => navigate('/admin/permissions')}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft size={20} />
        Back to Permissions
      </button>

      <h1 className="text-3xl font-bold mb-6">
        {mode === 'create' ? 'Create New Permission' : 'Edit Permission'}
      </h1>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="bg-white p-6 rounded-lg shadow space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Permission Name *
            </label>
            <input
              {...register('name')}
              type="text"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono"
              placeholder="products.create"
              disabled={mode === 'edit'} // Cannot change permission name once created
            />
            {errors.name && (
              <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
            )}
            <p className="mt-1 text-xs text-gray-500">
              Format: module.action (e.g., products.create, accounts.view)
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              {...register('description')}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Permission description..."
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
            {mode === 'create' ? 'Create Permission' : 'Update Permission'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/admin/permissions')}
            className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
