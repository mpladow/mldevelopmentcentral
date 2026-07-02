import { Box } from '@mui/material';
import { useEffect, useState } from 'react';
import { AccountsView } from '../features/accounts/AccountsView';
import { useAuthenticatedSession } from '../features/auth/AuthContext';
import { RolesView } from '../features/roles/RolesView';
import { SystemsView } from '../features/systems/SystemsView';
import type { ActiveView } from '../types/navigation';
import { DashboardHeader } from './DashboardHeader';
import { Sidebar } from './Sidebar';
import { TopBar } from './TopBar';

const sidebarCollapseBreakpoint = 920;

export function DashboardShell() {
  const session = useAuthenticatedSession();
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const [activeView, setActiveView] = useState<ActiveView>('accounts');
  const [systemListVersion, setSystemListVersion] = useState(0);

  useEffect(() => {
    const updateSidebarForViewport = () => {
      setIsSidebarCollapsed(window.innerWidth <= sidebarCollapseBreakpoint);
    };

    updateSidebarForViewport();
    window.addEventListener('resize', updateSidebarForViewport);

    return () => window.removeEventListener('resize', updateSidebarForViewport);
  }, []);

  return (
    <Box
      component="main"
      sx={{
        display: 'flex',
        minHeight: '100vh',
        bgcolor: 'background.default',
      }}
    >
      <Sidebar
        activeView={activeView}
        isCollapsed={isSidebarCollapsed}
        onNavigate={setActiveView}
        user={session.user}
      />

      <Box
        component="section"
        sx={{
          display: 'flex',
          flex: 1,
          flexDirection: 'column',
          minWidth: 0,
        }}
      >
        <TopBar
          isSidebarCollapsed={isSidebarCollapsed}
          onToggleSidebar={() => setIsSidebarCollapsed((value) => !value)}
          systemListVersion={systemListVersion}
        />
        <Box
          sx={{
            display: 'grid',
            gap: 3,
            p: { xs: 2, md: 3 },
          }}
        >
          <DashboardHeader activeView={activeView} />

          {activeView === 'accounts' ? (
            <AccountsView token={session.accessToken} user={session.user} />
          ) : activeView === 'systems' ? (
            <SystemsView
              onSystemsChanged={() => setSystemListVersion((value) => value + 1)}
              token={session.accessToken}
            />
          ) : (
            <RolesView token={session.accessToken} />
          )}
        </Box>
      </Box>
    </Box>
  );
}
