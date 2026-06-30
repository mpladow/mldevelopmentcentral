import { ArrowLeft, Edit3, Save, UserPlus } from 'lucide-react';
import { FormEvent, useEffect, useState } from 'react';
import type { Account } from '../../types/accounts';
import type { SessionUser } from '../../types/auth';
import { useAccounts } from './useAccounts';

type AccountEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; account: Account };

type AccountFormState = {
  email: string;
  displayName: string;
  password: string;
  role: string;
};

type AccountsViewProps = {
  token: string;
  user: SessionUser;
};

const emptyAccountForm: AccountFormState = {
  email: '',
  displayName: '',
  password: '',
  role: 'User',
};

export function AccountsView({ token, user }: AccountsViewProps) {
  const [mode, setMode] = useState<AccountEditorMode>({ type: 'list' });
  const [form, setForm] = useState(emptyAccountForm);
  const canManageAccounts = user.roles.includes('Admin');
  const { accounts, message, clearMessage, loadAccounts, createAccount, updateAccount } = useAccounts(token);

  useEffect(() => {
    void loadAccounts();
  }, [loadAccounts]);

  function openCreateAccount() {
    clearMessage();
    setForm(emptyAccountForm);
    setMode({ type: 'create' });
  }

  function openEditAccount(account: Account) {
    clearMessage();
    setForm({
      email: account.email,
      displayName: account.displayName,
      password: '',
      role: account.roles[0] ?? 'User',
    });
    setMode({ type: 'edit', account });
  }

  function closeEditor() {
    clearMessage();
    setMode({ type: 'list' });
  }

  async function handleCreateAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearMessage();

    const created = await createAccount({
      email: form.email,
      displayName: form.displayName,
      password: form.password,
      roles: [form.role],
    });

    if (!created) {
      return;
    }

    setForm(emptyAccountForm);
    setMode({ type: 'list' });
  }

  async function handleUpdateAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (mode.type !== 'edit') {
      return;
    }

    clearMessage();

    const updated = await updateAccount(mode.account.id, {
      email: form.email,
      displayName: form.displayName,
      roles: [form.role],
    });

    if (!updated) {
      return;
    }

    setMode({ type: 'list' });
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

        <form className="account-form account-editor-form" onSubmit={isCreateMode ? handleCreateAccount : handleUpdateAccount}>
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
            <div className={canManageAccounts ? 'data-grid-row with-actions' : 'data-grid-row'} key={account.id}>
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
