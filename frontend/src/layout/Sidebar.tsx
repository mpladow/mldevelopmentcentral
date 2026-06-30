import { LayoutDashboard, Server, ShieldCheck, Users } from 'lucide-react';
import type { ActiveView } from '../types/navigation';
import type { SessionUser } from '../types/auth';
import { getInitials } from '../utils/getInitials';

type SidebarProps = {
  activeView: ActiveView;
  isCollapsed: boolean;
  onNavigate: (view: ActiveView) => void;
  user: SessionUser;
};

export function Sidebar({ activeView, isCollapsed, onNavigate, user }: SidebarProps) {
  return (
    <aside className="sidebar" aria-label="Main navigation">
      <div className="sidebar-brand">
        <span className="brand-icon" aria-hidden="true">
          <LayoutDashboard size={22} />
        </span>
        {!isCollapsed && <strong>ML Dashboard</strong>}
      </div>

      <div className="sidebar-profile">
        <div className="avatar">{getInitials(user.displayName || user.email)}</div>
        {!isCollapsed && (
          <div className="sidebar-profile-copy">
            <strong className="sidebar-profile-name">{user.displayName}</strong>
            <span className="sidebar-profile-email">{user.email}</span>
          </div>
        )}
      </div>

      <div className="nav-section">
        {!isCollapsed && <p>Global</p>}
        <button
          className={activeView === 'accounts' ? 'nav-button active' : 'nav-button'}
          onClick={() => onNavigate('accounts')}
          type="button"
        >
          <Users size={18} />
          {!isCollapsed && <span>Accounts</span>}
        </button>
        <button
          className={activeView === 'systems' ? 'nav-button active' : 'nav-button'}
          onClick={() => onNavigate('systems')}
          type="button"
        >
          <Server size={18} />
          {!isCollapsed && <span>Systems</span>}
        </button>
        <button
          className={activeView === 'roles' ? 'nav-button active' : 'nav-button'}
          onClick={() => onNavigate('roles')}
          type="button"
        >
          <ShieldCheck size={18} />
          {!isCollapsed && <span>Roles</span>}
        </button>
      </div>
    </aside>
  );
}
