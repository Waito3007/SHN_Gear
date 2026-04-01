import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { MailCheck, ShieldCheck } from 'lucide-react';
import { authApi } from '../api/auth';

function normalizeOtpInput(value: string): string {
  return value.replace(/\D/g, '').slice(0, 6);
}

export default function VerifyEmailOtpPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialEmail = searchParams.get('email') || '';

  const [email, setEmail] = useState(initialEmail);
  const [otp, setOtp] = useState('');
  const [loadingSend, setLoadingSend] = useState(false);
  const [loadingVerify, setLoadingVerify] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    setEmail(initialEmail);
  }, [initialEmail]);

  const canVerify = useMemo(() => email.trim().length > 0 && otp.length === 6, [email, otp]);

  const handleResendOtp = async () => {
    setError('');
    setMessage('');
    if (!email.trim()) {
      setError('Please provide your email first.');
      return;
    }

    setLoadingSend(true);
    try {
      const res = await authApi.sendVerificationOtp({ email: email.trim() });
      if (res.data.success) {
        setMessage('A new OTP has been sent to your email.');
      } else {
        setError(res.data.message || 'Unable to send OTP.');
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Unable to send OTP. Please try again.';
      setError(msg);
    } finally {
      setLoadingSend(false);
    }
  };

  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (!canVerify) {
      setError('Please enter email and 6-digit OTP.');
      return;
    }

    setLoadingVerify(true);
    try {
      const res = await authApi.verifyEmailOtp({
        email: email.trim(),
        otp,
      });

      if (res.data.success) {
        navigate(`/login?verified=1&email=${encodeURIComponent(email.trim())}`);
        return;
      }

      setError(res.data.message || 'OTP verification failed.');
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'OTP verification failed. Please try again.';
      setError(msg);
    } finally {
      setLoadingVerify(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-4 py-12 bg-gray-50">
      <div className="w-full max-w-lg bg-white rounded-2xl border border-gray-200 shadow-sm p-6 sm:p-8">
        <div className="text-center mb-6">
          <MailCheck className="w-10 h-10 mx-auto text-black" />
          <h1 className="text-2xl font-bold text-gray-900 mt-3">Verify your email</h1>
          <p className="text-sm text-gray-500 mt-2">
            Enter the 6-digit OTP from your inbox to activate your account.
          </p>
        </div>

        {(error || message) && (
          <div className={`mb-4 p-3 rounded-xl text-sm border ${error ? 'bg-red-50 border-red-200 text-red-700' : 'bg-emerald-50 border-emerald-200 text-emerald-700'}`}>
            {error || message}
          </div>
        )}

        <form onSubmit={handleVerify} className="space-y-4">
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
            <label className="block text-sm font-medium text-gray-700 mb-1.5">OTP Code</label>
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

          <button
            type="submit"
            disabled={loadingVerify}
            className="w-full gradient-btn text-white py-3 rounded-xl font-semibold text-sm transition-all hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loadingVerify ? 'Verifying OTP...' : 'Verify Email'}
          </button>
        </form>

        <div className="mt-4 flex flex-col sm:flex-row gap-3">
          <button
            type="button"
            disabled={loadingSend}
            onClick={handleResendOtp}
            className="flex-1 px-4 py-2.5 rounded-xl border border-gray-300 text-sm font-medium hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loadingSend ? 'Sending...' : 'Resend OTP'}
          </button>

          <Link
            to="/login"
            className="flex-1 inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border border-gray-300 text-sm font-medium hover:bg-gray-50"
          >
            <ShieldCheck className="w-4 h-4" />
            Back to login
          </Link>
        </div>
      </div>
    </div>
  );
}
