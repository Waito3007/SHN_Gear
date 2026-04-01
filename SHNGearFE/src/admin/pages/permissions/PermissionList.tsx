import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { permissionApi } from '../../../api/permission';
import { Plus, Trash2 } from 'lucide-react';
import type { PermissionDto } from '../../../types/admin';
import { usePermission } from '../../../context/PermissionContext';
import toast, { Toaster } from 'react-hot-toast';

export default function PermissionList() {
  const { hasPermission } = usePermission();
  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [loading, setLoading] = useState(true);

  const loadPermissions = async () => {
    try {
      setLoading(true);
      const response = await permissionApi.getAll();
      setPermissions(response.data?.data || []);
    } catch (error) {
      toast.error('Failed to load permissions');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPermissions();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this permission?')) return;
    
    try {
      await permissionApi.delete(id);
      toast.success('Permission deleted successfully');
      loadPermissions();
    } catch (error) {
      toast.error('Failed to delete permission');
      console.error(error);
    }
  };

  return (
    <div>
      <Toaster position="top-right" />
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Permissions</h1>
        {hasPermission('permission.assign') && (
          <Link
            to="/admin/permissions/create"
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus size={20} />
            Create Permission
          </Link>
        )}
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Description
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {permissions.map((permission) => (
              <tr key={permission.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <code className="px-2 py-1 bg-gray-100 rounded text-sm font-mono">
                    {permission.name}
                  </code>
                </td>
                <td className="px-6 py-4">
                  <div className="text-sm text-gray-900">
                    {permission.description}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  {hasPermission('permission.assign') && (
                    <button
                      onClick={() => handleDelete(permission.id)}
                      className="text-red-600 hover:text-red-900"
                      title="Delete permission"
                    >
                      <Trash2 size={18} />
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {permissions.length === 0 && !loading && (
        <div className="text-center py-12 text-gray-500">
          No permissions found
        </div>
      )}
    </div>
  );
}
