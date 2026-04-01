import { useEffect, useState } from 'react';
import { accountApi } from '../../api/account';
import { roleApi } from '../../api/role';
import { permissionApi } from '../../api/permission';
import { productApi } from '../../api/product';
import { usePermission } from '../../context/PermissionContext';
import { Package, Users, Shield, Key } from 'lucide-react';

interface Stats {
  totalProducts: number;
  totalAccounts: number;
  totalRoles: number;
  totalPermissions: number;
}

export default function AdminDashboard() {
  const { hasPermission } = usePermission();
  const [stats, setStats] = useState<Stats>({
    totalProducts: 0,
    totalAccounts: 0,
    totalRoles: 0,
    totalPermissions: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      setLoading(true);
      try {
        const [productsRes, accountsRes, rolesRes, permissionsRes] = await Promise.all([
          hasPermission('product.view') 
            ? productApi.getPaged(1, 1).catch(() => ({ data: { totalCount: 0 } }))
            : Promise.resolve({ data: { totalCount: 0 } }),
          hasPermission('account.view')
            ? accountApi.getAll().catch(() => ({ data: { data: [] } }))
            : Promise.resolve({ data: { data: [] } }),
          hasPermission('role.view')
            ? roleApi.getAll().catch(() => ({ data: { data: [] } }))
            : Promise.resolve({ data: { data: [] } }),
          hasPermission('permission.view')
            ? permissionApi.getAll().catch(() => ({ data: { data: [] } }))
            : Promise.resolve({ data: { data: [] } }),
        ]);

        setStats({
          totalProducts: productsRes.data?.totalCount || 0,
          totalAccounts: Array.isArray(accountsRes.data?.data) ? accountsRes.data.data.length : 0,
          totalRoles: Array.isArray(rolesRes.data?.data) ? rolesRes.data.data.length : 0,
          totalPermissions: Array.isArray(permissionsRes.data?.data) ? permissionsRes.data.data.length : 0,
        });
      } catch (error) {
        console.error('Failed to fetch dashboard stats:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [hasPermission]);

  const statCards = [
    {
      title: 'Total Products',
      value: stats.totalProducts,
      icon: <Package className="w-8 h-8 text-blue-500" />,
      bgColor: 'bg-blue-50',
      permission: 'product.view',
    },
    {
      title: 'Total Accounts',
      value: stats.totalAccounts,
      icon: <Users className="w-8 h-8 text-green-500" />,
      bgColor: 'bg-green-50',
      permission: 'account.view',
    },
    {
      title: 'Total Roles',
      value: stats.totalRoles,
      icon: <Shield className="w-8 h-8 text-purple-500" />,
      bgColor: 'bg-purple-50',
      permission: 'role.view',
    },
    {
      title: 'Total Permissions',
      value: stats.totalPermissions,
      icon: <Key className="w-8 h-8 text-orange-500" />,
      bgColor: 'bg-orange-50',
      permission: 'permission.view',
    },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6 text-gray-800">Dashboard</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statCards.map((card) => {
          const canView = !card.permission || hasPermission(card.permission);
          
          return (
            <div key={card.title} className="bg-white p-6 rounded-lg shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
              <div className="flex items-center justify-between mb-3">
                <h3 className="text-gray-600 text-sm font-medium">{card.title}</h3>
                <div className={`p-2 rounded-lg ${card.bgColor}`}>
                  {card.icon}
                </div>
              </div>
              <p className="text-3xl font-bold text-gray-900">
                {loading ? (
                  <span className="text-gray-400">...</span>
                ) : canView ? (
                  card.value.toLocaleString()
                ) : (
                  <span className="text-gray-300">--</span>
                )}
              </p>
            </div>
          );
        })}
      </div>

      <div className="mt-8 bg-white p-6 rounded-lg shadow-sm border border-gray-100">
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Welcome to Admin Dashboard</h2>
        <p className="text-gray-600">
          Use the sidebar to navigate through different admin sections.
          You can manage accounts, roles, permissions, and products here.
        </p>
      </div>
    </div>
  );
}
