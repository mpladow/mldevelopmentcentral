import { CssBaseline, ThemeProvider } from '@mui/material';
import { AuthProvider, useAuth } from './features/auth/AuthContext';
import { LoginScreen } from './features/auth/LoginScreen';
import { DashboardShell } from './layout/DashboardShell';
import { dashboardTheme } from './theme/dashboardTheme';

export function App() {
  return (
    <ThemeProvider theme={dashboardTheme}>
      <CssBaseline />
      <AuthProvider>
        <AuthenticatedApp />
      </AuthProvider>
    </ThemeProvider>
  );
}

function AuthenticatedApp() {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <DashboardShell /> : <LoginScreen />;
}
