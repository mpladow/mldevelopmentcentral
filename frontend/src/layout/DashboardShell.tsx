import { Box } from '@mui/material';
import { useCallback, useEffect, useState } from 'react';
import { AccountsView } from '../features/accounts/AccountsView';
import { useAuthenticatedSession } from '../features/auth/AuthContext';
import { RolesView } from '../features/roles/RolesView';
import { SystemsView } from '../features/systems/SystemsView';
import { FactionsView } from '../features/warmaster/factions/FactionsView';
import { UnitsView } from '../features/warmaster/units/UnitsView';
import { permissions } from '../config/permissions';
import type { ActiveView } from '../types/navigation';
import type { AvailableSystem } from '../types/systems';
import { loadStoredActiveSystemKey, saveStoredActiveSystemKey } from './activeSystemStorage';
import { DashboardHeader } from './DashboardHeader';
import { Sidebar } from './Sidebar';
import { TopBar } from './TopBar';

const sidebarCollapseBreakpoint = 920;

export function DashboardShell() {
  const session = useAuthenticatedSession();
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const [availableSystems, setAvailableSystems] = useState<AvailableSystem[]>([]);
  const [activeSystemKey, setActiveSystemKey] = useState(loadStoredActiveSystemKey);
  const [activeView, setActiveView] = useState<ActiveView>('accounts');
  const [systemListVersion, setSystemListVersion] = useState(0);
  const activeSystem = availableSystems.find((system) =>
    system.systemKey.toLowerCase() === activeSystemKey.toLowerCase()
  ) ?? null;
  const activeSystemKeyLower = activeSystem?.systemKey.toLowerCase() ?? '';
  const visibleViews = [
    activeSystemKeyLower === 'global' && session.user.permissions.includes(permissions.globalMenuAccounts)
      ? 'accounts'
      : null,
    activeSystemKeyLower === 'global' && session.user.permissions.includes(permissions.globalMenuSystems)
      ? 'systems'
      : null,
    activeSystemKeyLower === 'global' && session.user.permissions.includes(permissions.globalMenuRoles)
      ? 'roles'
      : null,
    activeSystemKeyLower === 'warmaster' && session.user.permissions.includes(permissions.warmasterMenuFactions)
      ? 'factions'
      : null,
    activeSystemKeyLower === 'warmaster' && session.user.permissions.includes(permissions.warmasterMenuUnits)
      ? 'units'
      : null,
  ].filter((view): view is ActiveView => Boolean(view));

  const handleActiveSystemKeyChange = useCallback((systemKey: string) => {
    setActiveSystemKey(systemKey);
    saveStoredActiveSystemKey(systemKey);
  }, []);

  const handleAvailableSystemsChange = useCallback((systems: AvailableSystem[]) => {
    setAvailableSystems(systems);
  }, []);

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
        activeSystem={activeSystem}
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
          activeSystemKey={activeSystemKey}
          isSidebarCollapsed={isSidebarCollapsed}
          onActiveSystemKeyChange={handleActiveSystemKeyChange}
          onAvailableSystemsChange={handleAvailableSystemsChange}
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
          {visibleViews.length > 0 && <DashboardHeader activeSystem={activeSystem} activeView={activeView} />}

          {visibleViews.length === 0 ? null : activeView === 'accounts' ? (
            <AccountsView token={session.accessToken} user={session.user} />
          ) : activeView === 'systems' ? (
            <SystemsView
              onSystemsChanged={() => setSystemListVersion((value) => value + 1)}
              token={session.accessToken}
              user={session.user}
            />
          ) : activeView === 'roles' ? (
            <RolesView token={session.accessToken} />
          ) : activeView === 'factions' ? (
            <FactionsView token={session.accessToken} />
          ) : (
            <UnitsView token={session.accessToken} />
          )}
        </Box>
      </Box>
    </Box>
  );
}
