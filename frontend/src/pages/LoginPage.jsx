import { useState } from 'react';

import { ApiError } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import LoadingState from '../components/LoadingState';

const LoginPage = ({ onAuthenticated }) => {
  const { isReady, login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!isReady) {
    return <LoadingState label="Restoring your session" fullScreen />;
  }

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      await login({ email, password });
      onAuthenticated();
    } catch (requestError) {
      setError(
        requestError instanceof ApiError
          ? requestError.message
          : 'Unable to sign in. Please try again.',
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="login-page">
      <section className="login-panel" aria-labelledby="login-title">
        <div className="brand brand--login">
          <span className="brand__mark" aria-hidden="true">G</span>
          <span>Guardian EDR</span>
        </div>
        <div className="login-panel__copy">
          <p className="eyebrow">Endpoint detection &amp; response</p>
          <h1 id="login-title">Welcome back</h1>
          <p>Sign in to monitor your endpoint security posture.</p>
        </div>
        <form className="login-form" onSubmit={handleSubmit}>
          <label htmlFor="email">
            Email address
            <input
              id="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </label>
          <label htmlFor="password">
            Password
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="button button--primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </section>
      <section className="login-page__aside" aria-hidden="true">
        <div>
          <span className="eyebrow">Guardian EDR</span>
          <h2>Know what is happening on every endpoint.</h2>
          <p>Focused visibility for the security signals that matter.</p>
        </div>
      </section>
    </main>
  );
};

export default LoginPage;
