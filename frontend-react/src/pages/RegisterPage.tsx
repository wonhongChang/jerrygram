import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { FiUserPlus } from 'react-icons/fi';
import { useAuth } from '../contexts/AuthContext';
import { getApiErrorMessage, getApiValidationErrors } from '../utils/apiData';

type RegisterField = 'email' | 'username' | 'password';
type FieldErrors = Partial<Record<RegisterField, string>>;

const usernamePattern = /^[a-zA-Z0-9_]+$/;
const emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
const passwordPatterns = {
  uppercase: /[A-Z]/,
  lowercase: /[a-z]/,
  number: /\d/,
  special: /[@$!%*?&]/,
};

const normalizeServerField = (field: string): RegisterField | undefined => {
  const normalized = field.toLowerCase();
  if (normalized.includes('email')) return 'email';
  if (normalized.includes('username')) return 'username';
  if (normalized.includes('password')) return 'password';
  return undefined;
};

const validateRegisterForm = (values: {
  email: string;
  username: string;
  password: string;
}): FieldErrors => {
  const errors: FieldErrors = {};

  if (!values.email) {
    errors.email = 'Email is required.';
  } else if (values.email.length > 100) {
    errors.email = 'Email cannot exceed 100 characters.';
  } else if (!emailPattern.test(values.email)) {
    errors.email = 'Enter a valid email address.';
  }

  if (!values.username) {
    errors.username = 'Username is required.';
  } else if (values.username.length < 3 || values.username.length > 30) {
    errors.username = 'Username must be between 3 and 30 characters.';
  } else if (!usernamePattern.test(values.username)) {
    errors.username = 'Username can only contain letters, numbers, and underscores.';
  }

  if (!values.password) {
    errors.password = 'Password is required.';
  } else if (values.password.length < 8) {
    errors.password = 'Password must be at least 8 characters.';
  } else if (
    !passwordPatterns.uppercase.test(values.password) ||
    !passwordPatterns.lowercase.test(values.password) ||
    !passwordPatterns.number.test(values.password) ||
    !passwordPatterns.special.test(values.password)
  ) {
    errors.password = 'Password must include uppercase, lowercase, number, and one of @$!%*?&.';
  }

  return errors;
};

const getRegisterServerErrors = (error: any): { fieldErrors: FieldErrors; formError?: string } => {
  const apiFieldErrors = getApiValidationErrors(error);
  const fieldErrors = Object.entries(apiFieldErrors).reduce<FieldErrors>((errors, [field, message]) => {
    const registerField = normalizeServerField(field);
    if (registerField) errors[registerField] = message;
    return errors;
  }, {});

  const message = getApiErrorMessage(error, 'Registration failed. Please check the fields and try again.');

  if (/email.*already|already.*email/i.test(message)) {
    fieldErrors.email = message;
  }

  if (/username.*already|username.*taken|taken.*username/i.test(message)) {
    fieldErrors.username = message;
  }

  return Object.keys(fieldErrors).length > 0 ? { fieldErrors } : { fieldErrors, formError: message };
};

const RegisterPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const getInputClassName = (field: RegisterField) =>
    [
      'block w-full rounded-md border px-3 py-3 text-sm text-gray-900 outline-none transition placeholder:text-gray-400 focus:ring-2',
      fieldErrors[field]
        ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-100'
        : 'border-gray-300 focus:border-primary focus:ring-blue-100',
    ].join(' ');

  const updateField = (field: RegisterField, value: string) => {
    if (field === 'email') setEmail(value);
    if (field === 'username') setUsername(value);
    if (field === 'password') setPassword(value);

    if (fieldErrors[field]) {
      setFieldErrors((current) => {
        const next = { ...current };
        delete next[field];
        return next;
      });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setFieldErrors({});

    const values = {
      email: email.trim(),
      username: username.trim(),
      password,
    };
    setEmail(values.email);
    setUsername(values.username);

    const validationErrors = validateRegisterForm(values);
    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors);
      return;
    }

    try {
      setLoading(true);
      await register(values.email, values.username, values.password);
      navigate('/');
    } catch (err: any) {
      const { fieldErrors: serverFieldErrors, formError } = getRegisterServerErrors(err);
      setFieldErrors(serverFieldErrors);
      setError(formError || '');
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

            <form className="space-y-4" onSubmit={handleSubmit} noValidate>
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
                  className={getInputClassName('email')}
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => updateField('email', e.target.value)}
                  aria-invalid={Boolean(fieldErrors.email)}
                  aria-describedby={fieldErrors.email ? 'email-error' : undefined}
                />
                {fieldErrors.email && (
                  <p id="email-error" className="mt-2 text-xs font-medium text-red-600">
                    {fieldErrors.email}
                  </p>
                )}
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
                  className={getInputClassName('username')}
                  placeholder="username"
                  value={username}
                  onChange={(e) => updateField('username', e.target.value)}
                  aria-invalid={Boolean(fieldErrors.username)}
                  aria-describedby="username-help"
                />
                <p
                  id="username-help"
                  className={`mt-2 text-xs ${fieldErrors.username ? 'font-medium text-red-600' : 'text-gray-500'}`}
                >
                  {fieldErrors.username || '3-30 characters: letters, numbers, and underscores.'}
                </p>
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
                  className={getInputClassName('password')}
                  placeholder="Password"
                  value={password}
                  onChange={(e) => updateField('password', e.target.value)}
                  aria-invalid={Boolean(fieldErrors.password)}
                  aria-describedby="password-help"
                />
                <p
                  id="password-help"
                  className={`mt-2 text-xs ${fieldErrors.password ? 'font-medium text-red-600' : 'text-gray-500'}`}
                >
                  {fieldErrors.password || '8+ characters with uppercase, lowercase, number, and one of @$!%*?&.'}
                </p>
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
