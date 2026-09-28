import { FormEvent, useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { LockKeyhole, Mail, LogIn } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

export default function LoginPage() {
  const { login, token, loading } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  if (token) {
    return <Navigate to="/dashboard" replace />;
  }

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError('');

    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err: any) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data?.error ||
          err?.message ||
          'Invalid email or password.'
      );
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-indigo-950 px-4">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center text-white">
          <div className="text-3xl font-bold">SILVERLAND</div>
          <div className="mt-1 text-sm tracking-[0.3em] text-indigo-300">
            CRM
          </div>
        </div>

        <form
          onSubmit={submit}
          className="rounded-2xl bg-white p-8 shadow-2xl"
        >
          <h2 className="text-2xl font-bold text-slate-900">
            Welcome back
          </h2>
          <p className="mt-1 mb-6 text-sm text-slate-500">
            Sign in to your CRM account
          </p>

          {error && (
            <div className="mb-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              {error}
            </div>
          )}

          <label className="mb-1 block text-sm font-medium text-slate-700">
            Email
          </label>

          <div className="relative mb-4">
            <Mail
              size={18}
              className="absolute left-3 top-3 text-slate-400"
            />
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-lg border border-slate-300 py-2.5 pl-10 pr-3 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              required
            />
          </div>

          <label className="mb-1 block text-sm font-medium text-slate-700">
            Password
          </label>

          <div className="relative mb-6">
            <LockKeyhole
              size={18}
              className="absolute left-3 top-3 text-slate-400"
            />
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-lg border border-slate-300 py-2.5 pl-10 pr-3 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              required
            />
          </div>

          <button
            disabled={loading}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-indigo-600 py-3 font-semibold text-white transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <LogIn size={18} />
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        <p className="mt-6 text-center text-xs text-slate-400">
          Silverland CRM
        </p>
      </div>
    </div>
  );
}
