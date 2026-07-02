import { createContext, ReactNode, useContext, useMemo, useState } from 'react';
import type { LoginResponse, SessionUser } from '../../types/auth';
import { clearStoredSession, loadStoredSession, storeSession } from './authSessionStorage';

type AuthContextValue = {
  accessToken: string | null;
  hasAnyRole: (roles: string[]) => boolean;
  hasRole: (role: string) => boolean;
  isAuthenticated: boolean;
  login: (session: LoginResponse) => void;
  logout: () => void;
  session: LoginResponse | null;
  user: SessionUser | null;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

type AuthProviderProps = {
  children: ReactNode;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const [session, setSession] = useState<LoginResponse | null>(() => loadStoredSession());

  const value = useMemo<AuthContextValue>(() => {
    const user = session?.user ?? null;

    return {
      accessToken: session?.accessToken ?? null,
      hasAnyRole: (roles) => Boolean(user && roles.some((role) => user.roles.includes(role))),
      hasRole: (role) => Boolean(user?.roles.includes(role)),
      isAuthenticated: Boolean(session),
      login: (nextSession) => {
        storeSession(nextSession);
        setSession(nextSession);
      },
      logout: () => {
        clearStoredSession();
        setSession(null);
      },
      session,
      user,
    };
  }, [session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
}

export function useAuthenticatedSession() {
  const { session } = useAuth();

  if (!session) {
    throw new Error('An authenticated session is required.');
  }

  return session;
}
