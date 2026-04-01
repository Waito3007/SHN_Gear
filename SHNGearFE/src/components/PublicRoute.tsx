import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { ReactNode } from 'react';

interface PublicRouteProps {
  children: ReactNode;
}

/**
 * PublicRoute restricts access to public auth pages (login, register, etc.)
 * Only allows unauthenticated users (guests) to access these pages.
 * Authenticated users are redirected to home page.
 */
export default function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, isInitialized } = useAuth();

  // Wait for auth initialization to complete
  if (!isInitialized) {
    return null;
  }

  // If already logged in, redirect to home
  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
