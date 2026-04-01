import { createContext, useContext, type ReactNode } from 'react';
import { useAuth } from './AuthContext';
import { hasAdminAccess } from '../utils/permissions';

interface PermissionContextType {
  permissions: string[];
  hasPermission: (permission: string) => boolean;
  hasAnyPermission: (permissions: string[]) => boolean;
  hasAllPermissions: (permissions: string[]) => boolean;
  isAdmin: boolean;
}

const PermissionContext = createContext<PermissionContextType | null>(null);

export function PermissionProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();

  const permissions = user?.permissions || [];

  const hasPermission = (permission: string): boolean => {
    return permissions.includes(permission);
  };

  const hasAnyPermission = (perms: string[]): boolean => {
    return perms.some(p => permissions.includes(p));
  };

  const hasAllPermissions = (perms: string[]): boolean => {
    return perms.every(p => permissions.includes(p));
  };

  // Use a shared predicate so route guards and menus stay consistent.
  const isAdmin = hasAdminAccess(permissions);

  return (
    <PermissionContext.Provider
      value={{
        permissions,
        hasPermission,
        hasAnyPermission,
        hasAllPermissions,
        isAdmin,
      }}
    >
      {children}
    </PermissionContext.Provider>
  );
}

export function usePermission() {
  const ctx = useContext(PermissionContext);
  if (!ctx) throw new Error('usePermission must be used within PermissionProvider');
  return ctx;
}
