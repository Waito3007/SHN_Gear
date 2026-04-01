import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { AccountDto } from '../types';
import { authApi } from '../api/auth';
import { accountApi } from '../api/account';

interface AuthContextType {
  user: AccountDto | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
  login: (emailOrUsername: string, password: string) => Promise<boolean>;
  register: (email: string, password: string, username?: string) => Promise<boolean>;
  logout: () => Promise<void>;
  refreshCurrentUser: () => Promise<void>;
  loading: boolean;
  error: string | null;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AccountDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    // Restore auth state from localStorage on app load
    const stored = localStorage.getItem('user');
    const token = localStorage.getItem('accessToken');
    if (stored && token) {
      try { 
        setUser(JSON.parse(stored)); 
      } catch { 
        // If parse fails, clear invalid data
        localStorage.removeItem('user');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
      }
    }
    setIsInitialized(true);
  }, []);

  const login = async (emailOrUsername: string, password: string) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authApi.login({ emailOrUsername, password });
      if (res.data.success) {
        const { accessToken, refreshToken, account } = res.data.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        localStorage.setItem('user', JSON.stringify(account));
        setUser(account);
        return true;
      } else {
        setError(res.data.message || 'Login failed');
        return false;
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Login failed. Please try again.';
      setError(msg);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const register = async (email: string, password: string, username?: string) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authApi.register({ email, password, username });
      if (res.data.success) {
        // Registration now requires explicit OTP confirmation before normal login.
        return true;
      } else {
        setError(res.data.message || 'Registration failed');
        return false;
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Registration failed. Please try again.';
      setError(msg);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } catch { /* ignore */ }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
  };

  const refreshCurrentUser = async () => {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      return;
    }

    try {
      const res = await accountApi.getMyProfile();
      if (res.data.success) {
        localStorage.setItem('user', JSON.stringify(res.data.data));
        setUser(res.data.data);
      }
    } catch {
      // Keep existing user data on transient refresh errors.
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isInitialized,
        login,
        register,
        logout,
        refreshCurrentUser,
        loading,
        error,
        clearError: () => setError(null),
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
