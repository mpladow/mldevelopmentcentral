import { FormEvent, useCallback, useEffect, useMemo, useState } from 'react';
import {
  ArrowLeft,
  Bell,
  CircleUserRound,
  Edit3,
  LayoutDashboard,
  LockKeyhole,
  LogOut,
  Menu,
  Save,
  Search,
  Server,
  Settings,
  ShieldCheck,
  UserPlus,
  Users,
} from 'lucide-react';
import './styles.css';

type SessionUser = {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
};

type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  user: SessionUser;
};

type Account = {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
};

type DashboardSystem = {
  id: number;
  systemKey: string;
  label: string;
  sortOrder: number;
  isActive: boolean;
  accounts: SystemAccount[];
};

type SystemAccount = {
  id: string;
  email: string;
  displayName: string;
};

type AvailableSystem = {
  id: number;
  systemKey: string;
  label: string;
};

type ActiveView = 'accounts' | 'systems' | 'roles';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5058';
const sidebarCollapseBreakpoint = 920;

export function App() {
  const [session, setSession] = useState<LoginResponse | null>(null);

  if (!session) {
    return <LoginScreen onLogin={setSession} />;
  }

  return <DashboardShell session={session} onLogout={() => setSession(null)} />;
}

type LoginScreenProps = {
  onLogin: (session: LoginResponse) => void;
};

function LoginScreen({ onLogin }: LoginScreenProps) {
  const [email, setEmail] = useState('ml.development.2022@gmail.com');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        setError('The email or password is incorrect.');
        return;
      }

      const result = (await response.json()) as LoginResponse;
      onLogin(result);
    } catch {
      setError('Unable to reach the API.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-panel" aria-label="Login">
        <div className="login-brand">
          <span className="brand-icon" aria-hidden="true">
            <LockKeyhole size={22} />
          </span>
          <div>
            <p className="eyebrow">Global</p>
            <h1>ML Dev Dashboard</h1>
          </div>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            Email address
            <input
              autoComplete="email"
              name="email"
              onChange={(event) => setEmail(event.target.value)}
              required
              type="email"
              value={email}
            />
          </label>

          <label>
            Password
            <input
              autoComplete="current-password"
              name="password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
          </label>

          {error && <p className="form-error">{error}</p>}

          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Signing in' : 'Sign in'}
          </button>
        </form>
      </section>
    </main>
  );
}

type DashboardShellProps = {
  session: LoginResponse;
  onLogout: () => void;
};

function DashboardShell({ session, onLogout }: DashboardShellProps) {
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
          onLogout={onLogout}
          onToggleSidebar={() => setIsSidebarCollapsed((value) => !value)}
          systemListVersion={systemListVersion}
          token={session.accessToken}
          user={session.user}
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

type SidebarProps = {
  activeView: ActiveView;
  isCollapsed: boolean;
  onNavigate: (view: ActiveView) => void;
  user: SessionUser;
};

function Sidebar({ activeView, isCollapsed, onNavigate, user }: SidebarProps) {
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

type TopBarProps = {
  isSidebarCollapsed: boolean;
  onLogout: () => void;
  onToggleSidebar: () => void;
  systemListVersion: number;
  token: string;
  user: SessionUser;
};

function TopBar({ isSidebarCollapsed, onLogout, onToggleSidebar, systemListVersion, token, user }: TopBarProps) {
  const [availableSystems, setAvailableSystems] = useState<AvailableSystem[]>([]);
  const [selectedSystemKey, setSelectedSystemKey] = useState('');

  useEffect(() => {
    async function loadAvailableSystems() {
      const response = await fetch(`${apiBaseUrl}/api/systems/available`, {
        headers: { Authorization: `Bearer ${token}` },
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
  }, [systemListVersion, token]);

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
        <button className="icon-button" onClick={onLogout} type="button" aria-label="Sign out">
          <LogOut size={18} />
        </button>
      </div>
    </header>
  );
}

function DashboardHeader({ activeView }: { activeView: ActiveView }) {
  const activeTitle = activeView === 'accounts'
    ? 'Accounts'
    : activeView === 'systems'
      ? 'Systems'
      : 'Roles';

  return (
    <section className="dashboard-header">
      <div>
        <p className="eyebrow">Global</p>
        <h1>{activeTitle}</h1>
      </div>
      <div className="metric-row" aria-label="Global summary">
        <article className="metric-card pink">
          <span>System</span>
          <strong>Global</strong>
        </article>
        <article className="metric-card orange">
          <span>Mode</span>
          <strong>Admin</strong>
        </article>
        <article className="metric-card navy">
          <span>Auth</span>
          <strong>Identity</strong>
        </article>
      </div>
    </section>
  );
}

type AccountEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; account: Account };

function AccountsView({ token, user }: { token: string; user: SessionUser }) {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [message, setMessage] = useState('');
  const [mode, setMode] = useState<AccountEditorMode>({ type: 'list' });
  const [form, setForm] = useState({
    email: '',
    displayName: '',
    password: '',
    role: 'User',
  });
  const canManageAccounts = user.roles.includes('Admin');

  const loadAccounts = useCallback(async () => {
    setMessage('');
    const response = await fetch(`${apiBaseUrl}/api/accounts`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load accounts.');
      return;
    }

    setAccounts((await response.json()) as Account[]);
  }, [token]);

  useEffect(() => {
    void loadAccounts();
  }, [loadAccounts]);

  function openCreateAccount() {
    setMessage('');
    setForm({ email: '', displayName: '', password: '', role: 'User' });
    setMode({ type: 'create' });
  }

  function openEditAccount(account: Account) {
    setMessage('');
    setForm({
      email: account.email,
      displayName: account.displayName,
      password: '',
      role: account.roles[0] ?? 'User',
    });
    setMode({ type: 'edit', account });
  }

  function closeEditor() {
    setMessage('');
    setMode({ type: 'list' });
  }

  async function createAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/accounts`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: form.email,
        displayName: form.displayName,
        password: form.password,
        roles: [form.role],
      }),
    });

    if (!response.ok) {
      setMessage('Unable to create the account.');
      return;
    }

    setForm({ email: '', displayName: '', password: '', role: 'User' });
    setMessage('Account created.');
    setMode({ type: 'list' });
    await loadAccounts();
  }

  async function updateAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (mode.type !== 'edit') {
      return;
    }

    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/accounts/${mode.account.id}`, {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: form.email,
        displayName: form.displayName,
        roles: [form.role],
      }),
    });

    if (!response.ok) {
      setMessage('Unable to update the account.');
      return;
    }

    setMessage('Account updated.');
    setMode({ type: 'list' });
    await loadAccounts();
  }

  if (mode.type !== 'list') {
    const isCreateMode = mode.type === 'create';

    return (
      <section className="panel account-editor-panel">
        <div className="panel-heading">
          <div>
            <h2>{isCreateMode ? 'Create account' : 'Edit account'}</h2>
            <p>{isCreateMode ? 'Admin-created accounts only.' : 'Update account details and role.'}</p>
          </div>
          <button className="secondary-button icon-text-button" onClick={closeEditor} type="button">
            <ArrowLeft size={16} />
            Back
          </button>
        </div>

        <form className="account-form account-editor-form" onSubmit={isCreateMode ? createAccount : updateAccount}>
          <label>
            Email address
            <input
              aria-label="Account email"
              onChange={(event) => setForm((value) => ({ ...value, email: event.target.value }))}
              required
              type="email"
              value={form.email}
            />
          </label>
          <label>
            Display name
            <input
              aria-label="Account display name"
              onChange={(event) => setForm((value) => ({ ...value, displayName: event.target.value }))}
              required
              type="text"
              value={form.displayName}
            />
          </label>
          {isCreateMode && (
            <label>
              Temporary password
              <input
                aria-label="Account temporary password"
                onChange={(event) => setForm((value) => ({ ...value, password: event.target.value }))}
                required
                type="password"
                value={form.password}
              />
            </label>
          )}
          <label>
            Role
            <select
              aria-label="Account role"
              onChange={(event) => setForm((value) => ({ ...value, role: event.target.value }))}
              value={form.role}
            >
              <option>Admin</option>
              <option>User</option>
              <option>Viewer</option>
            </select>
          </label>
          <button className="primary-button icon-text-button" type="submit">
            {isCreateMode ? <UserPlus size={16} /> : <Save size={16} />}
            {isCreateMode ? 'Create account' : 'Save account'}
          </button>
        </form>

        {message && <p className="form-note">{message}</p>}
      </section>
    );
  }

  return (
    <section className="panel accounts-panel">
      <div className="panel-heading">
        <div>
          <h2>Accounts</h2>
          <p>Global users with assigned roles.</p>
        </div>
        <div className="panel-actions">
          <button className="secondary-button" onClick={loadAccounts} type="button">Refresh</button>
          {canManageAccounts && (
            <button className="primary-button icon-text-button" onClick={openCreateAccount} type="button">
              <UserPlus size={16} />
              Create account
            </button>
          )}
        </div>
      </div>

      {message && <p className="form-note">{message}</p>}

      <div className="data-grid" aria-label="Accounts grid">
        <div className={canManageAccounts ? 'data-grid-header with-actions' : 'data-grid-header'}>
          <span>Name</span>
          <span>Email</span>
          <span>Roles</span>
          {canManageAccounts && <span>Actions</span>}
        </div>
        {accounts.length === 0 ? (
          <p className="empty-state">No accounts loaded.</p>
        ) : (
          accounts.map((account) => (
            <div
              className={canManageAccounts ? 'data-grid-row with-actions' : 'data-grid-row'}
              key={account.id}
            >
              <strong>{account.displayName}</strong>
              <span>{account.email}</span>
              <span>{account.roles.join(', ')}</span>
              {canManageAccounts && (
                <button
                  aria-label={`Edit ${account.displayName}`}
                  className="secondary-button icon-text-button"
                  onClick={() => openEditAccount(account)}
                  type="button"
                >
                  <Edit3 size={16} />
                  Edit
                </button>
              )}
            </div>
          ))
        )}
      </div>
    </section>
  );
}

type SystemEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; system: DashboardSystem };

function SystemsView({
  onSystemsChanged,
  token,
}: {
  onSystemsChanged: () => void;
  token: string;
}) {
  const [systems, setSystems] = useState<DashboardSystem[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [message, setMessage] = useState('');
  const [mode, setMode] = useState<SystemEditorMode>({ type: 'list' });
  const [form, setForm] = useState({
    label: '',
    accountIds: [] as string[],
  });

  const loadSystems = useCallback(async () => {
    setMessage('');
    const response = await fetch(`${apiBaseUrl}/api/systems`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load systems.');
      return;
    }

    setSystems((await response.json()) as DashboardSystem[]);
  }, [token]);

  const loadAccounts = useCallback(async () => {
    const response = await fetch(`${apiBaseUrl}/api/accounts`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load account options.');
      return;
    }

    setAccounts((await response.json()) as Account[]);
  }, [token]);

  useEffect(() => {
    void loadSystems();
    void loadAccounts();
  }, [loadAccounts, loadSystems]);

  function openCreateSystem() {
    setMessage('');
    setForm({ label: '', accountIds: [] });
    setMode({ type: 'create' });
  }

  function openEditSystem(system: DashboardSystem) {
    setMessage('');
    setForm({
      label: system.label,
      accountIds: system.accounts.map((account) => account.id),
    });
    setMode({ type: 'edit', system });
  }

  function closeEditor() {
    setMessage('');
    setMode({ type: 'list' });
  }

  function toggleAccount(accountId: string) {
    setForm((value) => ({
      ...value,
      accountIds: value.accountIds.includes(accountId)
        ? value.accountIds.filter((selectedAccountId) => selectedAccountId !== accountId)
        : [...value.accountIds, accountId],
    }));
  }

  async function saveSystem(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage('');

    const isCreateMode = mode.type === 'create';
    const response = await fetch(
      isCreateMode ? `${apiBaseUrl}/api/systems` : `${apiBaseUrl}/api/systems/${mode.type === 'edit' ? mode.system.id : ''}`,
      {
        method: isCreateMode ? 'POST' : 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          label: form.label,
          accountIds: form.accountIds,
        }),
      },
    );

    if (!response.ok) {
      setMessage(isCreateMode ? 'Unable to create the system.' : 'Unable to update the system.');
      return;
    }

    setMessage(isCreateMode ? 'System created.' : 'System updated.');
    setMode({ type: 'list' });
    await loadSystems();
    onSystemsChanged();
  }

  if (mode.type !== 'list') {
    const isCreateMode = mode.type === 'create';

    return (
      <section className="panel systems-panel system-editor-panel">
        <div className="panel-heading">
          <div>
            <h2>{isCreateMode ? 'Create system' : 'Edit system'}</h2>
            <p>{isCreateMode ? 'Name the system and assign account access.' : 'Update the system name and account access.'}</p>
          </div>
          <button className="secondary-button icon-text-button" onClick={closeEditor} type="button">
            <ArrowLeft size={16} />
            Back
          </button>
        </div>

        <form className="account-form system-editor-form" onSubmit={saveSystem}>
          <label>
            System name
            <input
              aria-label="System name"
              onChange={(event) => setForm((value) => ({ ...value, label: event.target.value }))}
              required
              type="text"
              value={form.label}
            />
          </label>

          <fieldset className="checkbox-fieldset">
            <legend>Account access</legend>
            <div className="checkbox-list">
              {accounts.length === 0 ? (
                <p className="empty-state">No accounts available.</p>
              ) : (
                accounts.map((account) => (
                  <label className="checkbox-row" key={account.id}>
                    <input
                      checked={form.accountIds.includes(account.id)}
                      onChange={() => toggleAccount(account.id)}
                      type="checkbox"
                    />
                    <span>
                      <strong>{account.displayName}</strong>
                      <small>{account.email}</small>
                    </span>
                  </label>
                ))
              )}
            </div>
          </fieldset>

          <button className="primary-button icon-text-button" type="submit">
            {isCreateMode ? <Server size={16} /> : <Save size={16} />}
            {isCreateMode ? 'Create system' : 'Save system'}
          </button>
        </form>

        {message && <p className="form-note">{message}</p>}
      </section>
    );
  }

  return (
    <section className="panel systems-panel">
      <div className="panel-heading">
        <div>
          <h2>Systems</h2>
          <p>Dashboard systems and account access.</p>
        </div>
        <div className="panel-actions">
          <button className="secondary-button" onClick={loadSystems} type="button">Refresh</button>
          <button className="primary-button icon-text-button" onClick={openCreateSystem} type="button">
            <Server size={16} />
            Create system
          </button>
        </div>
      </div>

      {message && <p className="form-note">{message}</p>}

      <div className="data-grid systems-grid" aria-label="Systems grid">
        <div className="data-grid-header system-grid-row">
          <span>System</span>
          <span>Key</span>
          <span>Accounts</span>
          <span>Actions</span>
        </div>
        {systems.length === 0 ? (
          <p className="empty-state">No systems loaded.</p>
        ) : (
          systems.map((system) => (
            <div className="data-grid-row system-grid-row" key={system.id}>
              <strong>{system.label}</strong>
              <span>{system.systemKey}</span>
              <span>{system.accounts.length === 0 ? 'No accounts assigned' : system.accounts.map((account) => account.displayName).join(', ')}</span>
              <button
                aria-label={`Edit ${system.label}`}
                className="secondary-button icon-text-button"
                onClick={() => openEditSystem(system)}
                type="button"
              >
                <Edit3 size={16} />
                Edit
              </button>
            </div>
          ))
        )}
      </div>
    </section>
  );
}

function RolesView({ token }: { token: string }) {
  const [roles, setRoles] = useState<string[]>([]);
  const [message, setMessage] = useState('');

  async function loadRoles() {
    setMessage('');
    const response = await fetch(`${apiBaseUrl}/api/roles`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load roles.');
      return;
    }

    setRoles((await response.json()) as string[]);
  }

  const roleDescriptions = useMemo(
    () => ({
      Admin: 'Can create accounts and manage Global reference access.',
      User: 'Can work in assigned systems after access is configured.',
      Viewer: 'Read-only access for dashboard and reference views.',
    }),
    [],
  );

  return (
    <section className="panel roles-panel">
      <div className="panel-heading">
        <div>
          <h2>Roles</h2>
          <p>Initial Global permission groups.</p>
        </div>
        <button className="secondary-button" onClick={loadRoles} type="button">Refresh</button>
      </div>

      {message && <p className="form-note">{message}</p>}

      <div className="role-grid">
        {(roles.length > 0 ? roles : ['Admin', 'User', 'Viewer']).map((role) => (
          <article className="role-card" key={role}>
            <ShieldCheck size={22} />
            <strong>{role}</strong>
            <p>{roleDescriptions[role as keyof typeof roleDescriptions]}</p>
          </article>
        ))}
      </div>
    </section>
  );
}

function getInitials(value: string) {
  return value
    .split(/[\s@.]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('');
}
