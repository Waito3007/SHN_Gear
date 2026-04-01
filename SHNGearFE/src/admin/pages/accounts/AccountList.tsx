import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { accountApi } from '../../../api/account';
import { Trash2, UserCog, Loader2 } from 'lucide-react';
import type { AccountDto } from '../../../types';
import { usePermission } from '../../../context/PermissionContext';
import toast, { Toaster } from 'react-hot-toast';

export default function AccountList() {
  const { hasPermission } = usePermission();
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [loading, setLoading] = useState(true);

  const loadAccounts = async () => {
    try {
      setLoading(true);
      const response = await accountApi.getAll();
      setAccounts(response.data?.data || []);
    } catch (error) {
      toast.error('Failed to load accounts');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAccounts();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this account?')) return;
    
    try {
      await accountApi.deleteAccount(id);
      toast.success('Account deleted successfully');
      loadAccounts();
    } catch (error) {
      toast.error('Failed to delete account');
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

  return (
    <div>
      <Toaster position="top-right" />
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Accounts</h1>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Email
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Roles
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Phone
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {accounts.map((account) => (
              <tr key={account.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">
                    {account.email}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-900">
                    {account.firstName || account.lastName
                      ? `${account.firstName || ''} ${account.lastName || ''}`
                      : '-'}
                  </div>
                </td>
                <td className="px-6 py-4">
                  <div className="flex gap-1 flex-wrap">
                    {account.roles?.length > 0 ? (
                      account.roles.map((role, idx) => (
                        <span
                          key={idx}
                          className="px-2 py-1 text-xs rounded-full bg-blue-100 text-blue-800"
                        >
                          {role}
                        </span>
                      ))
                    ) : (
                      <span className="text-sm text-gray-500">No roles</span>
                    )}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {account.phoneNumber || '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div className="flex justify-end gap-2">
                    {hasPermission('account.update') && (
                      <Link
                        to={`/admin/accounts/${account.id}/roles`}
                        className="text-blue-600 hover:text-blue-900"
                        title="Manage roles"
                      >
                        <UserCog size={18} />
                      </Link>
                    )}
                    {hasPermission('account.delete') && (
                      <button
                        onClick={() => handleDelete(account.id)}
                        className="text-red-600 hover:text-red-900"
                        title="Delete account"
                      >
                        <Trash2 size={18} />
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {accounts.length === 0 && (
        <div className="text-center py-12 text-gray-500">
          No accounts found
        </div>
      )}
    </div>
  );
}
