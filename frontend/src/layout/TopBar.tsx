import { useEffect, useState } from 'react';
import { Bell, CircleUserRound, LogOut, Menu, Search, Settings } from 'lucide-react';
import { apiBaseUrl } from '../config/api';
import { useAuthenticatedSession, useAuth } from '../features/auth/AuthContext';
import type { AvailableSystem } from '../types/systems';
import { getInitials } from '../utils/getInitials';

type TopBarProps = {
  isSidebarCollapsed: boolean;
  onToggleSidebar: () => void;
  systemListVersion: number;
};

export function TopBar({
  isSidebarCollapsed,
  onToggleSidebar,
  systemListVersion,
}: TopBarProps) {
  const { accessToken, user } = useAuthenticatedSession();
  const { logout } = useAuth();
  const [availableSystems, setAvailableSystems] = useState<AvailableSystem[]>([]);
  const [selectedSystemKey, setSelectedSystemKey] = useState('');

  useEffect(() => {
    async function loadAvailableSystems() {
      const response = await fetch(`${apiBaseUrl}/api/systems/available`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });

      if (!response.ok) {
        setAvailableSystems([]);
        setSelectedSystemKey('');
        return;
      }

      const systems = (await response.json()) as AvailableSystem[];
      setAvailableSystems(systems);
      setSelectedSystemKey((currentValue) => currentValue || systems[0]?.systemKey || '');
    }

    void loadAvailableSystems();
  }, [accessToken, systemListVersion]);

  return (
    <header className="topbar">
      <button
        aria-expanded={!isSidebarCollapsed}
        aria-label={isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        className="icon-button"
        onClick={onToggleSidebar}
        type="button"
      >
        <Menu size={20} />
      </button>

      <label className="search-box">
        <Search size={17} />
        <input aria-label="Search" placeholder="Search" type="search" />
      </label>

      <div className="topbar-actions">
        <select
          aria-label="Select system"
          onChange={(event) => setSelectedSystemKey(event.target.value)}
          value={selectedSystemKey}
        >
          {availableSystems.length === 0 ? (
            <option value="">No systems</option>
          ) : (
            availableSystems.map((system) => (
              <option key={system.id} value={system.systemKey}>
                {system.label}
              </option>
            ))
          )}
        </select>
        <button className="icon-button" type="button" aria-label="Notifications">
          <Bell size={18} />
        </button>
        <button className="icon-button" type="button" aria-label="Settings">
          <Settings size={18} />
        </button>
        <div className="profile-chip">
          <CircleUserRound size={20} />
          <span>{getInitials(user.displayName || user.email)}</span>
        </div>
        <button className="icon-button" onClick={logout} type="button" aria-label="Sign out">
          <LogOut size={18} />
        </button>
      </div>
    </header>
  );
}
