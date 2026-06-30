import { useState } from 'react';
import { LoginScreen } from './features/auth/LoginScreen';
import { DashboardShell } from './layout/DashboardShell';
import type { LoginResponse } from './types/auth';

export function App() {
  const [session, setSession] = useState<LoginResponse | null>(null);

  if (!session) {
    return <LoginScreen onLogin={setSession} />;
  }

  return <DashboardShell session={session} onLogout={() => setSession(null)} />;
}
