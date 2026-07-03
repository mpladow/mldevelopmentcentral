import { cleanup, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { App } from './App';
import { permissions } from './config/permissions';
import type { LoginResponse } from './types/auth';

const loginResponse: LoginResponse = {
  accessToken: 'test-token',
  expiresAt: '2099-06-29T00:00:00Z',
  user: {
    id: '3b9cda16-bc2f-4e18-a6f2-56ebdd50beef',
    email: 'ml.development.2022@gmail.com',
    displayName: 'ML Development Admin',
    permissions: [
      permissions.globalMenuAccounts,
      permissions.globalMenuSystems,
      permissions.globalMenuRoles,
      permissions.globalAccountsManage,
      permissions.globalSystemsManage,
      permissions.globalRolesManage,
    ],
    roles: ['GlobalAdmin'],
  },
};

const longProfileLoginResponse = {
  ...loginResponse,
  user: {
    ...loginResponse.user,
    email: 'ml.development.administrator.with.a.very.long.email.address@example-development-domain.com',
    displayName: 'ML Development Administrator With An Extremely Long Display Name',
  },
};

const warmasterLoginResponse = {
  ...loginResponse,
  user: {
    ...loginResponse.user,
    permissions: [
      ...loginResponse.user.permissions,
      permissions.warmasterMenuFactions,
      permissions.warmasterMenuUnits,
      permissions.warmasterFactionsView,
      permissions.warmasterUnitsView,
    ],
    roles: ['GlobalAdmin', 'WarmasterAdmin'],
  },
};

const viewerLoginResponse = {
  ...loginResponse,
  user: {
    ...loginResponse.user,
    permissions: [permissions.globalMenuAccounts],
    roles: ['GlobalViewer'],
  },
};

const accountsResponse = [
  {
    id: 'c2c4eab7-9266-408b-bb47-f7374a91f76d',
    email: 'admin@example.com',
    displayName: 'Admin Example',
    roles: ['GlobalAdmin'],
  },
  {
    id: '015941d7-c05a-474a-9d8e-458fb86b89d5',
    email: 'viewer@example.com',
    displayName: 'Viewer Example',
    roles: ['GlobalViewer', 'WarmasterAdmin'],
  },
];

const systemsResponse = [
  {
    id: 1,
    systemKey: 'global',
    label: 'Global',
    sortOrder: 0,
    isActive: true,
    theme: {
      primaryColor: '#1d4ed8',
      secondaryColor: '#0f766e',
      backgroundColor: '#f6f8fb',
      surfaceColor: '#ffffff',
      textColor: '#111827',
      borderRadius: 8,
    },
    accounts: [
      {
        id: accountsResponse[0].id,
        email: accountsResponse[0].email,
        displayName: accountsResponse[0].displayName,
        role: 'GlobalAdmin',
      },
    ],
  },
  {
    id: 2,
    systemKey: 'warmaster',
    label: 'Warmaster',
    sortOrder: 1,
    isActive: true,
    theme: {
      primaryColor: '#1d4ed8',
      secondaryColor: '#0f766e',
      backgroundColor: '#f6f8fb',
      surfaceColor: '#ffffff',
      textColor: '#111827',
      borderRadius: 8,
    },
    accounts: [
      {
        id: accountsResponse[1].id,
        email: accountsResponse[1].email,
        displayName: accountsResponse[1].displayName,
        role: 'WarmasterAdmin',
      },
    ],
  },
];

const rolesResponse = [
  {
    id: 1,
    systemId: 1,
    systemKey: 'global',
    systemLabel: 'Global',
    name: 'GlobalAdmin',
    displayName: 'Global Admin',
    isProtected: true,
    permissions: [
      permissions.globalMenuAccounts,
      permissions.globalMenuSystems,
      permissions.globalMenuRoles,
      permissions.globalAccountsManage,
      permissions.globalSystemsManage,
      permissions.globalRolesManage,
    ],
  },
  {
    id: 2,
    systemId: 1,
    systemKey: 'global',
    systemLabel: 'Global',
    name: 'GlobalViewer',
    displayName: 'Global Viewer',
    isProtected: false,
    permissions: [permissions.globalMenuAccounts],
  },
  {
    id: 3,
    systemId: 2,
    systemKey: 'warmaster',
    systemLabel: 'Warmaster',
    name: 'WarmasterAdmin',
    displayName: 'Warmaster Admin',
    isProtected: true,
    permissions: [permissions.warmasterMenuFactions, permissions.warmasterMenuUnits],
  },
];

const permissionCatalogResponse = [
  {
    id: 1,
    systemId: 1,
    systemKey: 'global',
    systemLabel: 'Global',
    permissionKey: permissions.globalMenuAccounts,
    displayName: 'Accounts menu',
    category: 'Menus',
  },
  {
    id: 2,
    systemId: 1,
    systemKey: 'global',
    systemLabel: 'Global',
    permissionKey: permissions.globalAccountsManage,
    displayName: 'Manage accounts',
    category: 'Administration',
  },
];

const availableSystemsResponse = [
  {
    id: 1,
    systemKey: 'global',
    label: 'Global',
  },
];

const warmasterAvailableSystemsResponse = [
  ...availableSystemsResponse,
  {
    id: 2,
    systemKey: 'warmaster',
    label: 'Warmaster',
  },
];

describe('App', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
    window.localStorage.clear();
    setViewportWidth(1280);
  });

  afterEach(() => {
    cleanup();
    window.localStorage.clear();
    vi.unstubAllGlobals();
  });

  it('renders the login screen before authentication', () => {
    render(<App />);

    expect(screen.getByRole('heading', { name: /ml dev dashboard/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('shows the Global dashboard after successful login', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await userEvent.type(screen.getByLabelText(/password/i), 'not-a-real-password');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1, name: /accounts/i })).toBeInTheDocument();
    });

    expect(screen.getByRole('button', { name: /accounts/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /roles/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/select system/i)).toHaveValue('global');
  });

  it('changes the visible menu options when switching systems', async () => {
    mockLogin(warmasterLoginResponse, warmasterAvailableSystemsResponse);

    render(<App />);

    await signIn();

    expect(await screen.findByRole('button', { name: /accounts/i })).toBeInTheDocument();

    await userEvent.selectOptions(screen.getByLabelText(/select system/i), 'warmaster');

    expect(await screen.findByRole('button', { name: /factions/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /units/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /accounts/i })).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /units/i }));

    expect(screen.getByRole('heading', { level: 1, name: /units/i })).toBeInTheDocument();
    expect(screen.getByRole('table', { name: /units table/i })).toBeInTheDocument();
  });

  it('restores an unexpired session after a page refresh', async () => {
    window.localStorage.setItem('mldev-dashboard.auth-session', JSON.stringify(loginResponse));
    mockAuthenticatedRequests();

    render(<App />);

    expect(await screen.findByRole('heading', { level: 1, name: /accounts/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /sign in/i })).not.toBeInTheDocument();
  });

  it('clears an expired stored session and shows login', () => {
    window.localStorage.setItem(
      'mldev-dashboard.auth-session',
      JSON.stringify({ ...loginResponse, expiresAt: '2000-01-01T00:00:00Z' }),
    );

    render(<App />);

    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    expect(window.localStorage.getItem('mldev-dashboard.auth-session')).toBeNull();
  });

  it('persists the session after login and clears it on logout', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await screen.findByRole('heading', { level: 1, name: /accounts/i });

    expect(window.localStorage.getItem('mldev-dashboard.auth-session')).toBe(JSON.stringify(loginResponse));

    await userEvent.click(screen.getByRole('button', { name: /sign out/i }));

    expect(window.localStorage.getItem('mldev-dashboard.auth-session')).toBeNull();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('can navigate to Roles and collapse the sidebar from the header menu', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await userEvent.type(screen.getByLabelText(/password/i), 'not-a-real-password');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));
    await userEvent.click(await screen.findByRole('button', { name: /roles/i }));

    expect(screen.getByRole('heading', { level: 1, name: /roles/i })).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /collapse sidebar/i }));

    expect(screen.queryByText('ML Dashboard')).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: /expand sidebar/i })).toBeInTheDocument();
  });

  it('creates a system permission from the Roles screen', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await userEvent.click(await screen.findByRole('button', { name: /roles/i }));
    await userEvent.click(await screen.findByRole('button', { name: /create permission/i }));
    await userEvent.type(screen.getByLabelText(/permission key/i), 'global.menu.reports');
    await userEvent.type(screen.getByLabelText(/display name/i), 'Reports menu');
    await userEvent.clear(screen.getByLabelText(/category/i));
    await userEvent.type(screen.getByLabelText(/category/i), 'Menus');
    await userEvent.click(screen.getByRole('button', { name: /create permission/i }));

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledWith(
        expect.stringMatching(/\/api\/roles\/permissions$/),
        expect.objectContaining({
          body: JSON.stringify({
            systemId: 1,
            permissionKey: 'global.menu.reports',
            displayName: 'Reports menu',
            category: 'Menus',
          }),
          method: 'POST',
        }),
      );
    });
  }, 10000);

  it('shows current system role holders before deleting a system', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await userEvent.click(await screen.findByRole('button', { name: /systems/i }));
    await userEvent.click(await screen.findByRole('button', { name: /delete warmaster/i }));

    expect(screen.getByRole('dialog', { name: /delete warmaster/i })).toBeInTheDocument();
    expect(screen.getByText(/accounts with roles in this system/i)).toBeInTheDocument();
    expect(screen.getByText('Viewer Example')).toBeInTheDocument();
    expect(screen.getByText(/viewer@example.com - WarmasterAdmin/i)).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /delete system/i }));

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledWith(
        expect.stringMatching(/\/api\/systems\/2$/),
        expect.objectContaining({ method: 'DELETE' }),
      );
    });
  }, 10000);

  it('automatically collapses the sidebar on narrow windows', async () => {
    setViewportWidth(760);
    mockLogin(loginResponse);

    render(<App />);

    await userEvent.type(screen.getByLabelText(/password/i), 'not-a-real-password');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /expand sidebar/i })).toBeInTheDocument();
    });

    expect(screen.queryByText('ML Dashboard')).not.toBeInTheDocument();
  });

  it('truncates long sidebar profile names and email addresses', async () => {
    mockLogin(longProfileLoginResponse);

    render(<App />);

    await userEvent.type(screen.getByLabelText(/password/i), 'not-a-real-password');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));

    const displayName = await screen.findByText(longProfileLoginResponse.user.displayName);
    const email = screen.getByText(longProfileLoginResponse.user.email);

    expect(displayName).toHaveClass('sidebar-profile-name');
    expect(email).toHaveClass('sidebar-profile-email');
    expect(displayName.parentElement).toHaveClass('sidebar-profile-copy');
  });

  it('opens the create account editor from the accounts grid for admins', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await userEvent.click(await screen.findByRole('button', { name: /create account/i }));

    expect(screen.getByRole('heading', { level: 2, name: /create account/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/account email/i)).toHaveValue('');
    expect(screen.getByLabelText(/account temporary password/i)).toBeInTheDocument();
    expect(screen.getByText('No roles selected')).toBeInTheDocument();
  });

  it('opens an edit account page with the selected account populated for admins', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await userEvent.click(await screen.findByRole('button', { name: /edit viewer example/i }));

    expect(screen.getByRole('heading', { level: 2, name: /edit account/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/account email/i)).toHaveValue('viewer@example.com');
    expect(screen.getByLabelText(/account display name/i)).toHaveValue('Viewer Example');
    expect(screen.getByText('Global / Global Viewer')).toBeInTheDocument();
    expect(screen.getByText('Warmaster / Warmaster Admin')).toBeInTheDocument();
    expect(screen.queryByLabelText(/account temporary password/i)).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('combobox', { name: /^roles$/i }));

    const roleList = await screen.findByRole('listbox');

    expect(within(roleList).getByText('Global')).toBeInTheDocument();
    expect(within(roleList).getByText('Warmaster')).toBeInTheDocument();
  });

  it('submits multiple selected roles when saving an account', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await userEvent.click(await screen.findByRole('button', { name: /edit viewer example/i }));
    await userEvent.click(screen.getByRole('button', { name: /save account/i }));

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledWith(
        expect.stringMatching(/\/api\/accounts\/015941d7-c05a-474a-9d8e-458fb86b89d5$/),
        expect.objectContaining({
          body: JSON.stringify({
            email: 'viewer@example.com',
            displayName: 'Viewer Example',
            roles: ['GlobalViewer', 'WarmasterAdmin'],
          }),
          method: 'PUT',
        }),
      );
    });
  });

  it('does not show account create or edit actions for viewers', async () => {
    mockLogin(viewerLoginResponse);

    render(<App />);

    await signIn();

    expect(await screen.findByText('Admin Example')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /create account/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /edit admin example/i })).not.toBeInTheDocument();
  });
});

function mockLogin(
  response: typeof loginResponse,
  availableSystems: typeof availableSystemsResponse = availableSystemsResponse,
) {
  vi.mocked(fetch).mockImplementation((input, init) => {
    const url = input.toString();

    if (url.endsWith('/api/auth/login')) {
      return Promise.resolve(
        new Response(JSON.stringify(response), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/systems/available')) {
      return Promise.resolve(
        new Response(JSON.stringify(availableSystems), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/warmaster/factions') || url.endsWith('/api/warmaster/units')) {
      return Promise.resolve(
        new Response(JSON.stringify([]), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith(`/api/accounts/${accountsResponse[1].id}`) && init?.method === 'PUT') {
      return Promise.resolve(
        new Response(JSON.stringify({
          ...accountsResponse[1],
          ...JSON.parse(init.body?.toString() ?? '{}'),
        }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/accounts')) {
      return Promise.resolve(
        new Response(JSON.stringify(accountsResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/systems')) {
      return Promise.resolve(
        new Response(JSON.stringify(systemsResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/systems/2')) {
      return Promise.resolve(new Response(null, { status: 204 }));
    }

    if (url.endsWith('/api/roles/permissions') && init?.method === 'POST') {
      return Promise.resolve(
        new Response(JSON.stringify({
          id: 99,
          systemId: 1,
          systemKey: 'global',
          systemLabel: 'Global',
          permissionKey: 'global.menu.reports',
          displayName: 'Reports menu',
          category: 'Menus',
        }), {
          status: 201,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/roles/permissions')) {
      return Promise.resolve(
        new Response(JSON.stringify(permissionCatalogResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/roles')) {
      return Promise.resolve(
        new Response(JSON.stringify(rolesResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    return Promise.resolve(
      new Response(JSON.stringify(response), {
        status: 404,
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  });
}

function mockAuthenticatedRequests() {
  vi.mocked(fetch).mockImplementation((input, init) => {
    const url = input.toString();

    if (url.endsWith('/api/systems/available')) {
      return Promise.resolve(
        new Response(JSON.stringify(availableSystemsResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/accounts')) {
      return Promise.resolve(
        new Response(JSON.stringify(accountsResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/systems')) {
      return Promise.resolve(
        new Response(JSON.stringify(systemsResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/roles/permissions') && init?.method === 'POST') {
      return Promise.resolve(
        new Response(JSON.stringify({
          id: 99,
          systemId: 1,
          systemKey: 'global',
          systemLabel: 'Global',
          permissionKey: 'global.menu.reports',
          displayName: 'Reports menu',
          category: 'Menus',
        }), {
          status: 201,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/roles/permissions')) {
      return Promise.resolve(
        new Response(JSON.stringify(permissionCatalogResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    if (url.endsWith('/api/roles')) {
      return Promise.resolve(
        new Response(JSON.stringify(rolesResponse), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }

    return Promise.resolve(
      new Response(null, {
        status: 404,
      }),
    );
  });
}

async function signIn() {
  await userEvent.type(screen.getByLabelText(/password/i), 'not-a-real-password');
  await userEvent.click(screen.getByRole('button', { name: /sign in/i }));
}

function setViewportWidth(width: number) {
  Object.defineProperty(window, 'innerWidth', {
    configurable: true,
    writable: true,
    value: width,
  });

  window.dispatchEvent(new Event('resize'));
}
