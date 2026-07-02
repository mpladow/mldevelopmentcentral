import type { LoginResponse } from '../../types/auth';

const storageKey = 'mldev-dashboard.auth-session';

export function loadStoredSession(): LoginResponse | null {
  try {
    const storedSession = window.localStorage.getItem(storageKey);

    if (!storedSession) {
      return null;
    }

    const session = JSON.parse(storedSession) as Partial<LoginResponse>;

    if (!isValidSession(session)) {
      clearStoredSession();
      return null;
    }

    return session;
  } catch {
    clearStoredSession();
    return null;
  }
}

export function storeSession(session: LoginResponse) {
  window.localStorage.setItem(storageKey, JSON.stringify(session));
}

export function clearStoredSession() {
  window.localStorage.removeItem(storageKey);
}

function isValidSession(session: Partial<LoginResponse>): session is LoginResponse {
  if (
    typeof session.accessToken !== 'string' ||
    typeof session.expiresAt !== 'string' ||
    !session.user ||
    typeof session.user.id !== 'string' ||
    typeof session.user.email !== 'string' ||
    typeof session.user.displayName !== 'string' ||
    !Array.isArray(session.user.roles)
  ) {
    return false;
  }

  const expiresAt = Date.parse(session.expiresAt);

  return Number.isFinite(expiresAt) && expiresAt > Date.now();
}
