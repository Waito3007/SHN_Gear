import { Navigate } from 'react-router-dom';
import { usePermission } from '../context/PermissionContext';
import { useAuth } from '../context/AuthContext';
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
  permission?: string;
  requireAdmin?: boolean;
}

export default function ProtectedRoute({ 
  children, 
  permission, 
  requireAdmin = false 
}: ProtectedRouteProps) {
  const { isAuthenticated, isInitialized } = useAuth();
  const { hasPermission, isAdmin } = usePermission();

  // Wait for auth initialization to complete
  if (!isInitialized) {
    return null;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requireAdmin && !isAdmin) {
    return <Navigate to="/admin/unauthorized" replace />;
  }

  if (permission && !hasPermission(permission)) {
    return <Navigate to="/admin/unauthorized" replace />;
  }

  return <>{children}</>;
}
