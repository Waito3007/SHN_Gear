import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { accountApi } from '../../../api/account';
import { roleApi } from '../../../api/role';
import { Loader2, ArrowLeft } from 'lucide-react';
import type { AccountDto, RoleDto } from '../../../types';
import toast, { Toaster } from 'react-hot-toast';

export default function AccountRoles() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [account, setAccount] = useState<AccountDto | null>(null);
  const [allRoles, setAllRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, [id]);

  const loadData = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      const [accountRes, rolesRes] = await Promise.all([
        accountApi.getById(id),
        roleApi.getAll(),
      ]);
      setAccount(accountRes.data?.data || null);
      setAllRoles(rolesRes.data?.data || []);
    } catch (error) {
      toast.error('Failed to load data');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleAssignRole = async (roleId: string) => {
    if (!id) return;
    
    try {
      await accountApi.assignRole(id, roleId);
      toast.success('Role assigned successfully');
      loadData();
    } catch (error) {
      toast.error('Failed to assign role');
      console.error(error);
    }
  };

  const handleRemoveRole = async (roleId: string) => {
    if (!id) return;
    
    try {
      await accountApi.removeRole(id, roleId);
      toast.success('Role removed successfully');
      loadData();
    } catch (error) {
      toast.error('Failed to remove role');
      console.error(error);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="animate-spin" size={32} />
      </div>
    );
  }

  if (!account) {
    return <div>Account not found</div>;
  }

  const accountRoleIds = new Set(
    allRoles
      .filter(r => account.roles?.includes(r.name))
      .map(r => r.id)
  );

  return (
    <div>
      <Toaster position="top-right" />
      <button
        onClick={() => navigate('/admin/accounts')}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft size={20} />
        Back to Accounts
      </button>

      <h1 className="text-3xl font-bold mb-2">Manage Roles</h1>
      <p className="text-gray-600 mb-6">
        Account: <strong>{account.email}</strong>
      </p>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Current roles */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">Current Roles</h2>
          {account.roles && account.roles.length > 0 ? (
            <div className="space-y-2">
              {allRoles
                .filter(r => account.roles?.includes(r.name))
                .map(role => (
                  <div
                    key={role.id}
                    className="flex items-center justify-between p-3 bg-blue-50 rounded"
                  >
                    <div>
                      <div className="font-medium">{role.name}</div>
                      <div className="text-sm text-gray-600">
                        {role.description}
                      </div>
                    </div>
                    <button
                      onClick={() => handleRemoveRole(role.id)}
                      className="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
                    >
                      Remove
                    </button>
                  </div>
                ))}
            </div>
          ) : (
            <p className="text-gray-500">No roles assigned</p>
          )}
        </div>

        {/* Available roles */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">Available Roles</h2>
          <div className="space-y-2">
            {allRoles
              .filter(r => !accountRoleIds.has(r.id))
              .map(role => (
                <div
                  key={role.id}
                  className="flex items-center justify-between p-3 bg-gray-50 rounded"
                >
                  <div>
                    <div className="font-medium">{role.name}</div>
                    <div className="text-sm text-gray-600">
                      {role.description}
                    </div>
                  </div>
                  <button
                    onClick={() => handleAssignRole(role.id)}
                    className="px-3 py-1 text-sm text-blue-600 hover:bg-blue-50 rounded"
                  >
                    Assign
                  </button>
                </div>
              ))}
          </div>
          {allRoles.filter(r => !accountRoleIds.has(r.id)).length === 0 && (
            <p className="text-gray-500">All roles assigned</p>
          )}
        </div>
      </div>
    </div>
  );
}
