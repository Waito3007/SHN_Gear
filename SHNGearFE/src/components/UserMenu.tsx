import { Link } from 'react-router-dom';
import { User, LogOut, Settings, LayoutDashboard } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import type { AccountDto } from '../types';
import { hasAdminAccess } from '../utils/permissions';

interface UserMenuProps {
  user: AccountDto;
  onLogout: () => void;
}

export default function UserMenu({ user, onLogout }: UserMenuProps) {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const canAccessAdmin = hasAdminAccess(user.permissions || []);

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => document.removeEventListener('mousedown', handleClickOutside);
    }
  }, [isOpen]);

  return (
    <div className="relative" ref={menuRef}>
      {/* User Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        onMouseEnter={() => setIsOpen(true)}
        className="flex items-center gap-2 px-3 py-2 rounded-full hover:bg-gray-100 transition-colors group"
      >
        <div className="w-8 h-8 bg-gradient-to-br from-gray-700 to-black rounded-full flex items-center justify-center text-white text-sm font-semibold">
          {user.username?.[0]?.toUpperCase() || user.email[0].toUpperCase()}
        </div>
        <span className="text-sm text-gray-700 group-hover:text-black font-medium hidden lg:block">
          {user.username || user.email.split('@')[0]}
        </span>
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div 
          onMouseLeave={() => setIsOpen(false)}
          className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-xl border border-gray-100 py-2 z-50 animate-in fade-in slide-in-from-top-2 duration-200"
        >
          {/* User Info */}
          <div className="px-4 py-3 border-b border-gray-100">
            <p className="text-sm font-semibold text-gray-900">
              {user.username || 'User'}
            </p>
            <p className="text-xs text-gray-500 truncate">
              {user.email}
            </p>
            {user.roles && user.roles.length > 0 && (
              <div className="mt-2 flex flex-wrap gap-1">
                {user.roles.map(role => (
                  <span 
                    key={role}
                    className="inline-block px-2 py-0.5 bg-black text-white text-xs rounded-full"
                  >
                    {role}
                  </span>
                ))}
              </div>
            )}
          </div>

          {/* Menu Items */}
          <div className="py-1">
            {canAccessAdmin && (
              <Link
                to="/admin/dashboard"
                onClick={() => setIsOpen(false)}
                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 hover:text-black transition-colors"
              >
                <LayoutDashboard className="w-4 h-4" />
                <span className="font-medium">Admin Dashboard</span>
              </Link>
            )}
            
            <Link
              to="/profile"
              onClick={() => setIsOpen(false)}
              className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 hover:text-black transition-colors"
            >
              <User className="w-4 h-4" />
              <span>My Profile</span>
            </Link>
            
            <Link
              to="/orders"
              onClick={() => setIsOpen(false)}
              className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 hover:text-black transition-colors"
            >
              <Settings className="w-4 h-4" />
              <span>My Orders</span>
            </Link>
          </div>

          {/* Logout */}
          <div className="border-t border-gray-100 pt-1">
            <button
              onClick={() => {
                setIsOpen(false);
                onLogout();
              }}
              className="flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 transition-colors w-full"
            >
              <LogOut className="w-4 h-4" />
              <span className="font-medium">Sign Out</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
