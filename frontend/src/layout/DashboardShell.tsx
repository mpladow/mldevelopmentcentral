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
    <main className={isSidebarCollapsed ? 'dashboard-shell sidebar-collapsed' : 'dashboard-shell'}>
      <Sidebar
        activeView={activeView}
        isCollapsed={isSidebarCollapsed}
        onNavigate={setActiveView}
        user={session.user}
      />

      <section className="main-area">
        <TopBar
          isSidebarCollapsed={isSidebarCollapsed}
          onToggleSidebar={() => setIsSidebarCollapsed((value) => !value)}
          systemListVersion={systemListVersion}
        />
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
      </section>
    </main>
  );
}
