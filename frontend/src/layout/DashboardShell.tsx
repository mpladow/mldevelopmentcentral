import { Box } from '@mui/material';
import { useEffect, useState } from 'react';
import { AccountsView } from '../features/accounts/AccountsView';
import { useAuthenticatedSession } from '../features/auth/AuthContext';
import { RolesView } from '../features/roles/RolesView';
import { SystemsView } from '../features/systems/SystemsView';
import { permissions } from '../config/permissions';
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
  const visibleViews = [
    session.user.permissions.includes(permissions.globalMenuAccounts) ? 'accounts' : null,
    session.user.permissions.includes(permissions.globalMenuSystems) ? 'systems' : null,
    session.user.permissions.includes(permissions.globalMenuRoles) ? 'roles' : null,
  ].filter((view): view is ActiveView => Boolean(view));

  useEffect(() => {
    const updateSidebarForViewport = () => {
      setIsSidebarCollapsed(window.innerWidth <= sidebarCollapseBreakpoint);
    };

    updateSidebarForViewport();
    window.addEventListener('resize', updateSidebarForViewport);

    return () => window.removeEventListener('resize', updateSidebarForViewport);
  }, []);

  useEffect(() => {
    if (!visibleViews.includes(activeView) && visibleViews.length > 0) {
      setActiveView(visibleViews[0]);
    }
  }, [activeView, visibleViews]);

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

          {visibleViews.length === 0 ? null : activeView === 'accounts' ? (
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
