import { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ShoppingBag, Eye, EyeOff } from 'lucide-react';

export default function LoginPage() {
  const { login, loading, error, clearError } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const emailFromQuery = searchParams.get('email') || '';
  const [emailOrUsername, setEmailOrUsername] = useState(emailFromQuery);
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const sessionExpired = searchParams.get('reason') === 'session-expired';
  const verified = searchParams.get('verified') === '1';
  const passwordReset = searchParams.get('reset') === '1';

  useEffect(() => {
    clearError();
    if (emailFromQuery) {
      setEmailOrUsername(emailFromQuery);
    }
  }, [clearError, emailFromQuery]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const success = await login(emailOrUsername, password);
    if (success) navigate('/');
  };

  return (
    <div className="min-h-[calc(100vh-4rem)] flex">
      {/* Left - Form */}
      <div className="flex-1 flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-md">
          <div className="text-center mb-8">
            <ShoppingBag className="w-10 h-10 mx-auto text-black" />
            <h1 className="text-2xl font-bold mt-4 text-gray-900">Welcome back</h1>
            <p className="text-gray-500 mt-1 text-sm">
              Sign in to your SHNGear account
            </p>
          </div>

          {sessionExpired && (
            <div className="mb-4 p-3 bg-amber-50 border border-amber-200 rounded-xl text-sm text-amber-800">
              Your session expired. Please sign in again to continue.
            </div>
          )}

          {verified && (
            <div className="mb-4 p-3 bg-emerald-50 border border-emerald-200 rounded-xl text-sm text-emerald-800">
              Email verified successfully. You can now sign in.
            </div>
          )}

          {passwordReset && (
            <div className="mb-4 p-3 bg-emerald-50 border border-emerald-200 rounded-xl text-sm text-emerald-800">
              Password reset successful. Please sign in with your new password.
            </div>
          )}

          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-xl text-sm text-red-700">
              {error}
              <button onClick={clearError} className="float-right font-bold">&times;</button>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Email or Username
              </label>
              <input
                type="text"
                required
                value={emailOrUsername}
                onChange={(e) => setEmailOrUsername(e.target.value)}
                className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
                placeholder="you@example.com"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative">
                <input
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors pr-10"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
              <div className="mt-2 text-right">
                <Link to="/forgot-password" className="text-xs text-gray-600 hover:text-black hover:underline">
                  Forgot password?
                </Link>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full gradient-btn text-white py-3 rounded-xl font-semibold text-sm transition-all hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? (
                <span className="inline-flex items-center gap-2">
                  <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Signing in...
                </span>
              ) : (
                'Sign In'
              )}
            </button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Don&apos;t have an account?{' '}
            <Link to="/register" className="font-medium text-black hover:underline">
              Sign up
            </Link>
          </p>

          <p className="text-center text-xs text-gray-500 mt-2">
            Not verified yet?{' '}
            <Link
              to={`/verify-email-otp${emailOrUsername.trim() ? `?email=${encodeURIComponent(emailOrUsername.trim())}` : ''}`}
              className="font-medium text-black hover:underline"
            >
              Enter OTP
            </Link>
          </p>
        </div>
      </div>

      {/* Right - Visual */}
      <div className="hidden lg:flex flex-1 gradient-hero items-center justify-center">
        <div className="text-center text-white max-w-md px-8">
          <ShoppingBag className="w-16 h-16 mx-auto opacity-20" />
          <h2 className="text-3xl font-bold mt-6">SHNGear</h2>
          <p className="mt-3 text-gray-400 leading-relaxed">
            Premium gear for those who demand the best. Sign in to access your account and exclusive deals.
          </p>
        </div>
      </div>
    </div>
  );
}
