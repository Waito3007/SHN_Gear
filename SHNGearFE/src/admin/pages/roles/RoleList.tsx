import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { roleApi } from '../../../api/role';
import { Plus, Trash2, Edit, Shield } from 'lucide-react';
import type { RoleDto } from '../../../types/admin';
import { usePermission } from '../../../context/PermissionContext';
import toast, { Toaster } from 'react-hot-toast';

export default function RoleList() {
  const { hasPermission } = usePermission();
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);

  const loadRoles = async () => {
    try {
      setLoading(true);
      const response = await roleApi.getAll();
      setRoles(response.data?.data || []);
    } catch (error) {
      toast.error('Failed to load roles');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRoles();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this role?')) return;
    
    try {
      await roleApi.delete(id);
      toast.success('Role deleted successfully');
      loadRoles();
    } catch (error) {
      toast.error('Failed to delete role');
      console.error(error);
    }
  };

  return (
    <div>
      <Toaster position="top-right" />
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Roles</h1>
        {hasPermission('role.create') && (
          <Link
            to="/admin/roles/create"
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus size={20} />
            Create Role
          </Link>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {roles.map((role) => (
          <div key={role.id} className="bg-white p-6 rounded-lg shadow">
            <div className="flex items-start justify-between mb-4">
              <div className="flex items-center gap-2">
                <Shield className="text-blue-600" size={24} />
                <h3 className="text-xl font-semibold">{role.name}</h3>
              </div>
              <div className="flex gap-2">
                {hasPermission('role.update') && (
                  <Link
                    to={`/admin/roles/edit/${role.id}`}
                    className="text-blue-600 hover:text-blue-800"
                  >
                    <Edit size={18} />
                  </Link>
                )}
                {hasPermission('role.delete') && (
                  <button
                    onClick={() => handleDelete(role.id)}
                    className="text-red-600 hover:text-red-800"
                  >
                    <Trash2 size={18} />
                  </button>
                )}
              </div>
            </div>
            <p className="text-gray-600 mb-4">{role.description}</p>
            <div>
              <p className="text-sm text-gray-500 mb-2">
                {role.permissions?.length || 0} permissions
              </p>
              {hasPermission('permission.assign') && (
                <Link
                  to={`/admin/roles/${role.id}/permissions`}
                  className="text-sm text-blue-600 hover:text-blue-800"
                >
                  Manage Permissions →
                </Link>
              )}
            </div>
          </div>
        ))}
      </div>

      {roles.length === 0 && !loading && (
        <div className="text-center py-12 text-gray-500">No roles found</div>
      )}
    </div>
  );
}
