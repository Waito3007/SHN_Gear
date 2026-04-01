import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { roleApi } from '../../../api/role';
import { permissionApi } from '../../../api/permission';
import type { RoleDto, PermissionDto } from '../../../types/admin';
import toast, { Toaster } from 'react-hot-toast';

export default function RolePermissionsManager() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [role, setRole] = useState<RoleDto | null>(null);
  const [allPermissions, setAllPermissions] = useState<PermissionDto[]>([]);
  const [currentPermissions, setCurrentPermissions] = useState<PermissionDto[]>([]);
  const [loading, setLoading] = useState(true);

  const loadData = useCallback(async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      const [roleData, permissionsData] = await Promise.all([
        roleApi.getById(id),
        permissionApi.getAll(),
      ]);
      
      const role = roleData.data?.data;
      const allPermissions = permissionsData.data?.data || [];
      setRole(role || null);
      setAllPermissions(allPermissions);
      setCurrentPermissions(role?.permissions || []);
    } catch (error) {
      console.error('Failed to load data:', error);
      toast.error('Failed to load role information');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleAssign = async (permissionId: string) => {
    if (!id) return;

    try {
      await roleApi.assignPermission(id, permissionId);
      toast.success('Permission assigned successfully');
      loadData();
    } catch (error) {
      console.error('Assign error:', error);
      toast.error('Failed to assign permission');
    }
  };

  const handleRemove = async (permissionId: string) => {
    if (!id) return;

    try {
      await roleApi.removePermission(id, permissionId);
      toast.success('Permission removed successfully');
      loadData();
    } catch (error) {
      console.error('Remove error:', error);
      toast.error('Failed to remove permission');
    }
  };

  const currentPermissionIds = currentPermissions.map(p => p.id);
  const availablePermissions = allPermissions.filter(
    p => !currentPermissionIds.includes(p.id)
  );

  // Group permissions by module
  const groupByModule = (permissions: PermissionDto[]) => {
    return permissions.reduce((acc, perm) => {
      const module = perm.name.split('.')[0];
      if (!acc[module]) acc[module] = [];
      acc[module].push(perm);
      return acc;
    }, {} as Record<string, PermissionDto[]>);
  };

  const currentGrouped = groupByModule(currentPermissions);
  const availableGrouped = groupByModule(availablePermissions);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading...</div>
      </div>
    );
  }

  if (!role) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Role not found</p>
      </div>
    );
  }

  return (
    <div>
      <Toaster position="top-right" />
      
      <button
        onClick={() => navigate('/admin/roles')}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft size={20} />
        Back to Roles
      </button>

      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Manage Permissions</h1>
        <p className="text-gray-600">
          Role: <span className="font-semibold">{role.name}</span>
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Current Permissions */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">
            Current Permissions ({currentPermissions.length})
          </h2>
          {currentPermissions.length === 0 ? (
            <p className="text-gray-500 text-center py-8">No permissions assigned</p>
          ) : (
            <div className="space-y-4">
              {Object.entries(currentGrouped).map(([module, perms]) => (
                <div key={module}>
                  <h3 className="text-sm font-semibold text-gray-700 uppercase mb-2">
                    {module}
                  </h3>
                  <div className="space-y-2">
                    {perms.map((perm) => (
                      <div
                        key={perm.id}
                        className="flex items-center justify-between p-3 bg-blue-50 rounded-lg"
                      >
                        <div>
                          <div className="font-mono text-sm text-blue-600">
                            {perm.name}
                          </div>
                          {perm.description && (
                            <div className="text-xs text-gray-600 mt-1">
                              {perm.description}
                            </div>
                          )}
                        </div>
                        <button
                          onClick={() => handleRemove(perm.id)}
                          className="p-2 text-red-600 hover:bg-red-50 rounded"
                          title="Remove permission"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Available Permissions */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">
            Available Permissions ({availablePermissions.length})
          </h2>
          {availablePermissions.length === 0 ? (
            <p className="text-gray-500 text-center py-8">All permissions assigned</p>
          ) : (
            <div className="space-y-4">
              {Object.entries(availableGrouped).map(([module, perms]) => (
                <div key={module}>
                  <h3 className="text-sm font-semibold text-gray-700 uppercase mb-2">
                    {module}
                  </h3>
                  <div className="space-y-2">
                    {perms.map((perm) => (
                      <div
                        key={perm.id}
                        className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                      >
                        <div>
                          <div className="font-mono text-sm text-gray-700">
                            {perm.name}
                          </div>
                          {perm.description && (
                            <div className="text-xs text-gray-600 mt-1">
                              {perm.description}
                            </div>
                          )}
                        </div>
                        <button
                          onClick={() => handleAssign(perm.id)}
                          className="p-2 text-green-600 hover:bg-green-50 rounded"
                          title="Assign permission"
                        >
                          <Plus size={16} />
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
