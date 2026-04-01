import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { usePermission } from '../../context/PermissionContext';
import { 
  LayoutDashboard, Users, Shield, Key, Package, Tags, FolderTree,
  LogOut, Menu, X, ClipboardList
} from 'lucide-react';
import { useState } from 'react';
import clsx from 'clsx';

interface MenuItem {
  path: string;
  label: string;
  icon: React.ReactNode;
  permission?: string;
}

export default function AdminLayout() {
  const { user, logout } = useAuth();
  const { hasPermission } = usePermission();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const menuItems: MenuItem[] = [
    { 
      path: '/admin/dashboard', 
      label: 'Dashboard', 
      icon: <LayoutDashboard size={20} /> 
    },
    { 
      path: '/admin/accounts', 
      label: 'Accounts', 
      icon: <Users size={20} />,
      permission: 'account.view'
    },
    { 
      path: '/admin/roles', 
      label: 'Roles', 
      icon: <Shield size={20} />,
      permission: 'role.view'
    },
    { 
      path: '/admin/permissions', 
      label: 'Permissions', 
      icon: <Key size={20} />,
      permission: 'permission.view'
    },
    { 
      path: '/admin/products', 
      label: 'Products', 
      icon: <Package size={20} />,
      permission: 'product.view'
    },
    {
      path: '/admin/brands',
      label: 'Brands',
      icon: <Tags size={20} />,
      permission: 'brand.view'
    },
    {
      path: '/admin/categories',
      label: 'Categories',
      icon: <FolderTree size={20} />,
      permission: 'category.view'
    },
    {
      path: '/admin/orders',
      label: 'Orders',
      icon: <ClipboardList size={20} />
    },
  ];

  const visibleMenuItems = menuItems.filter(
    item => !item.permission || hasPermission(item.permission)
  );

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar */}
      <aside className={clsx(
        'bg-gray-900 text-white transition-all duration-300',
        sidebarOpen ? 'w-64' : 'w-20'
      )}>
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="flex items-center justify-between p-4 border-b border-gray-800">
            {sidebarOpen && <h1 className="text-xl font-bold">SHNGear Admin</h1>}
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="p-2 rounded hover:bg-gray-800"
            >
              {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
            </button>
          </div>

          {/* Menu */}
          <nav className="flex-1 p-4 space-y-2">
            {visibleMenuItems.map(item => (
              <Link
                key={item.path}
                to={item.path}
                className={clsx(
                  'flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-gray-800 transition-colors',
                  !sidebarOpen && 'justify-center'
                )}
              >
                {item.icon}
                {sidebarOpen && <span>{item.label}</span>}
              </Link>
            ))}
          </nav>

          {/* User info */}
          <div className="p-4 border-t border-gray-800">
            <div className={clsx(
              'flex items-center gap-3',
              !sidebarOpen && 'justify-center'
            )}>
              <div className="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center">
                {user?.email?.[0]?.toUpperCase() || 'A'}
              </div>
              {sidebarOpen && (
                <div className="flex-1">
                  <p className="text-sm font-medium">{user?.email}</p>
                  <button
                    onClick={handleLogout}
                    className="text-xs text-gray-400 hover:text-white flex items-center gap-1"
                  >
                    <LogOut size={12} />
                    Logout
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Top navbar */}
        <header className="bg-white shadow-sm px-6 py-4">
          <div className="flex items-center justify-between">
            <h2 className="text-2xl font-semibold text-gray-800">
              Admin Dashboard
            </h2>
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-600">
                {user?.email}
              </span>
            </div>
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
