import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { App } from './App';

const loginResponse = {
  accessToken: 'test-token',
  expiresAt: '2026-06-29T00:00:00Z',
  user: {
    id: '3b9cda16-bc2f-4e18-a6f2-56ebdd50beef',
    email: 'ml.development.2022@gmail.com',
    displayName: 'ML Development Admin',
    roles: ['Admin'],
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

const viewerLoginResponse = {
  ...loginResponse,
  user: {
    ...loginResponse.user,
    roles: ['Viewer'],
  },
};

const accountsResponse = [
  {
    id: 'c2c4eab7-9266-408b-bb47-f7374a91f76d',
    email: 'admin@example.com',
    displayName: 'Admin Example',
    roles: ['Admin'],
  },
  {
    id: '015941d7-c05a-474a-9d8e-458fb86b89d5',
    email: 'viewer@example.com',
    displayName: 'Viewer Example',
    roles: ['Viewer'],
  },
];

const availableSystemsResponse = [
  {
    id: 1,
    systemKey: 'Global',
    label: 'Global',
  },
];

describe('App', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
    setViewportWidth(1280);
  });

  afterEach(() => {
    cleanup();
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
    expect(screen.getByLabelText(/select system/i)).toHaveValue('Global');
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
  });

  it('opens an edit account page with the selected account populated for admins', async () => {
    mockLogin(loginResponse);

    render(<App />);

    await signIn();
    await userEvent.click(await screen.findByRole('button', { name: /edit viewer example/i }));

    expect(screen.getByRole('heading', { level: 2, name: /edit account/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/account email/i)).toHaveValue('viewer@example.com');
    expect(screen.getByLabelText(/account display name/i)).toHaveValue('Viewer Example');
    expect(screen.getByLabelText(/account role/i)).toHaveValue('Viewer');
    expect(screen.queryByLabelText(/account temporary password/i)).not.toBeInTheDocument();
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

function mockLogin(response: typeof loginResponse) {
  vi.mocked(fetch).mockImplementation((input) => {
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

    return Promise.resolve(
      new Response(JSON.stringify(response), {
        status: 404,
        headers: { 'Content-Type': 'application/json' },
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
