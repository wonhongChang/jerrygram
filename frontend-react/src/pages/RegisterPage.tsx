import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { FiUserPlus } from 'react-icons/fi';
import { useAuth } from '../contexts/AuthContext';
import { getApiErrorMessage } from '../utils/apiData';

const RegisterPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await register(email, username, password);
      navigate('/');
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Registration failed. Please try again.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen bg-[var(--jg-bg)] px-4 py-8 text-gray-950 sm:px-6">
      <div className="mx-auto grid min-h-[calc(100vh-4rem)] w-full max-w-5xl items-center gap-8 lg:grid-cols-[1fr_420px]">
        <section className="hidden lg:block">
          <div className="max-w-md">
            <div className="mb-5 flex h-12 w-12 items-center justify-center rounded-md bg-gray-950 text-lg font-black text-white">
              J
            </div>
            <h1 className="text-4xl font-black">Jerrygram</h1>
            <p className="mt-3 text-base text-gray-600">Create a profile, post photos, and follow the people you care about.</p>
          </div>
        </section>

        <section className="w-full min-w-0 max-w-[360px] sm:mx-auto sm:max-w-md lg:max-w-none">
          <div className="mb-6 text-center lg:hidden">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-md bg-gray-950 text-lg font-black text-white">
              J
            </div>
            <h1 className="text-3xl font-black">Jerrygram</h1>
          </div>

          <div className="w-full min-w-0 rounded-lg border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
            <div className="mb-6">
              <h2 className="text-xl font-bold text-gray-950">Create account</h2>
              <p className="mt-1 text-sm text-gray-500">Start with a username and password.</p>
            </div>

            <form className="space-y-4" onSubmit={handleSubmit}>
              {error && (
                <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
                  {error}
                </div>
              )}

              <div>
                <label htmlFor="email" className="mb-2 block text-sm font-semibold text-gray-800">
                  Email
                </label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  required
                  className="block w-full rounded-md border border-gray-300 px-3 py-3 text-sm text-gray-900 outline-none transition placeholder:text-gray-400 focus:border-primary focus:ring-2 focus:ring-blue-100"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>

              <div>
                <label htmlFor="username" className="mb-2 block text-sm font-semibold text-gray-800">
                  Username
                </label>
                <input
                  id="username"
                  name="username"
                  type="text"
                  autoComplete="username"
                  required
                  className="block w-full rounded-md border border-gray-300 px-3 py-3 text-sm text-gray-900 outline-none transition placeholder:text-gray-400 focus:border-primary focus:ring-2 focus:ring-blue-100"
                  placeholder="username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                />
              </div>

              <div>
                <label htmlFor="password" className="mb-2 block text-sm font-semibold text-gray-800">
                  Password
                </label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  autoComplete="new-password"
                  required
                  className="block w-full rounded-md border border-gray-300 px-3 py-3 text-sm text-gray-900 outline-none transition placeholder:text-gray-400 focus:border-primary focus:ring-2 focus:ring-blue-100"
                  placeholder="Password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="inline-flex w-full items-center justify-center gap-2 rounded-md bg-primary px-4 py-3 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <FiUserPlus size={17} />
                {loading ? 'Signing up...' : 'Sign up'}
              </button>
            </form>
          </div>

          <div className="mt-4 rounded-lg border border-gray-200 bg-white p-4 text-center text-sm shadow-sm">
            <span className="text-gray-600">Have an account?</span>{' '}
            <Link to="/login" className="font-semibold text-primary hover:text-blue-700">
              Log in
            </Link>
          </div>
        </section>
      </div>
    </main>
  );
};

export default RegisterPage;
