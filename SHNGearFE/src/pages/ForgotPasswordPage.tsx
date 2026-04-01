import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { KeyRound, Mail, ShieldAlert } from 'lucide-react';
import { authApi } from '../api/auth';

function normalizeOtpInput(value: string): string {
  return value.replace(/\D/g, '').slice(0, 6);
}

type Step = 1 | 2 | 3;

export default function ForgotPasswordPage() {
  const navigate = useNavigate();

  const [step, setStep] = useState<Step>(1);
  const [email, setEmail] = useState('');
  const [otp, setOtp] = useState('');
  const [verificationToken, setVerificationToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const canVerifyOtp = useMemo(() => email.trim().length > 0 && otp.length === 6, [email, otp]);

  const handleSendOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (!email.trim()) {
      setError('Please enter your email.');
      return;
    }

    setLoading(true);
    try {
      const res = await authApi.sendForgotPasswordOtp({ email: email.trim() });
      if (res.data.success) {
        setStep(2);
        setMessage('If the account exists, OTP has been sent to your email.');
      } else {
        setError(res.data.message || 'Unable to send OTP.');
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Unable to send OTP. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (!canVerifyOtp) {
      setError('Please enter your email and a 6-digit OTP.');
      return;
    }

    setLoading(true);
    try {
      const res = await authApi.verifyForgotPasswordOtp({ email: email.trim(), otp });
      if (res.data.success) {
        setVerificationToken(res.data.data.verificationToken);
        setStep(3);
        setMessage('OTP verified. Please set a new password.');
      } else {
        setError(res.data.message || 'OTP verification failed.');
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'OTP verification failed. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (newPassword.length < 6) {
      setError('New password must be at least 6 characters.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Password confirmation does not match.');
      return;
    }

    if (!verificationToken) {
      setError('Verification token is missing. Please verify OTP again.');
      setStep(2);
      return;
    }

    setLoading(true);
    try {
      const res = await authApi.resetForgotPassword({
        email: email.trim(),
        verificationToken,
        newPassword,
      });

      if (res.data.success) {
        navigate(`/login?reset=1&email=${encodeURIComponent(email.trim())}`);
        return;
      }

      setError(res.data.message || 'Password reset failed.');
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Password reset failed. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-4 py-12 bg-gray-50">
      <div className="w-full max-w-lg bg-white rounded-2xl border border-gray-200 shadow-sm p-6 sm:p-8">
        <div className="text-center mb-6">
          <KeyRound className="w-10 h-10 mx-auto text-black" />
          <h1 className="text-2xl font-bold text-gray-900 mt-3">Forgot Password</h1>
          <p className="text-sm text-gray-500 mt-2">
            Recover your account in 3 quick steps.
          </p>
        </div>

        <div className="mb-5 flex items-center justify-between text-xs text-gray-500">
          <span className={step >= 1 ? 'font-semibold text-black' : ''}>1. Email</span>
          <span className={step >= 2 ? 'font-semibold text-black' : ''}>2. OTP</span>
          <span className={step >= 3 ? 'font-semibold text-black' : ''}>3. New password</span>
        </div>

        {(error || message) && (
          <div className={`mb-4 p-3 rounded-xl text-sm border ${error ? 'bg-red-50 border-red-200 text-red-700' : 'bg-emerald-50 border-emerald-200 text-emerald-700'}`}>
            {error || message}
          </div>
        )}

        {step === 1 && (
          <form onSubmit={handleSendOtp} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Account email</label>
              <div className="relative">
                <Mail className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full pl-10 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
                  placeholder="you@example.com"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full gradient-btn text-white py-3 rounded-xl font-semibold text-sm transition-all hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Sending OTP...' : 'Send OTP'}
            </button>
          </form>
        )}

        {step === 2 && (
          <form onSubmit={handleVerifyOtp} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Email</label>
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
                placeholder="you@example.com"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">OTP code</label>
              <input
                type="text"
                inputMode="numeric"
                required
                value={otp}
                onChange={(e) => setOtp(normalizeOtpInput(e.target.value))}
                className="w-full px-4 py-3 tracking-[0.35em] text-center font-semibold bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
                placeholder="000000"
              />
            </div>

            <div className="flex gap-3">
              <button
                type="button"
                onClick={() => setStep(1)}
                className="flex-1 px-4 py-3 rounded-xl border border-gray-300 text-sm font-medium hover:bg-gray-50"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={loading}
                className="flex-1 gradient-btn text-white py-3 rounded-xl font-semibold text-sm transition-all hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Verifying...' : 'Verify OTP'}
              </button>
            </div>

            <button
              type="button"
              disabled={loading}
              onClick={handleSendOtp}
              className="w-full px-4 py-2.5 rounded-xl border border-gray-300 text-sm font-medium hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Resend OTP
            </button>
          </form>
        )}

        {step === 3 && (
          <form onSubmit={handleResetPassword} className="space-y-4">
            <div className="p-3 rounded-xl bg-amber-50 border border-amber-200 text-amber-800 text-sm flex gap-2">
              <ShieldAlert className="w-4 h-4 mt-0.5 shrink-0" />
              Your OTP is verified. Set a strong new password.
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">New password</label>
              <input
                type="password"
                required
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
                placeholder="At least 6 characters"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Confirm new password</label>
              <input
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:border-black focus:bg-white transition-colors"
                placeholder="Retype new password"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full gradient-btn text-white py-3 rounded-xl font-semibold text-sm transition-all hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Updating password...' : 'Reset password'}
            </button>
          </form>
        )}

        <p className="text-center text-sm text-gray-500 mt-6">
          Remembered your password?{' '}
          <Link to="/login" className="font-medium text-black hover:underline">
            Back to sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
