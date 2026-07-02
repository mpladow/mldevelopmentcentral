import { LockKeyhole } from 'lucide-react';
import { FormEvent, useState } from 'react';
import { useAuth } from './AuthContext';
import { useLogin } from './useLogin';

export function LoginScreen() {
  const [email, setEmail] = useState('ml.development.2022@gmail.com');
  const [password, setPassword] = useState('');
  const { login: completeLogin } = useAuth();
  const { login, error, isLoading } = useLogin();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      const result = await login(email, password);
      completeLogin(result);
    } catch {
      // Error state is managed by the hook.
    }
  }

  return (
    <main className="login-page">
      <section className="login-panel" aria-label="Login">
        <div className="login-brand">
          <span className="brand-icon" aria-hidden="true">
            <LockKeyhole size={22} />
          </span>
          <div>
            <p className="eyebrow">Global</p>
            <h1>ML Dev Dashboard</h1>
          </div>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            Email address
            <input
              autoComplete="email"
              name="email"
              onChange={(event) => setEmail(event.target.value)}
              required
              type="email"
              value={email}
            />
          </label>

          <label>
            Password
            <input
              autoComplete="current-password"
              name="password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
          </label>

          {error && <p className="form-error">{error}</p>}

          <button className="primary-button" disabled={isLoading} type="submit">
            {isLoading ? 'Signing in' : 'Sign in'}
          </button>
        </form>
      </section>
    </main>
  );
}
