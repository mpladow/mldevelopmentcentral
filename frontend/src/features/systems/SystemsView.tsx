import { FormEvent, useCallback, useEffect, useState } from 'react';
import { ArrowLeft, Edit3, Save, Server } from 'lucide-react';
import { apiBaseUrl } from '../../config/api';
import type { Account } from '../../types/accounts';
import type { DashboardSystem } from '../../types/systems';

type SystemEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; system: DashboardSystem };

type SystemFormState = {
  label: string;
  accountIds: string[];
};

type SystemsViewProps = {
  onSystemsChanged: () => void;
  token: string;
};

const emptySystemForm: SystemFormState = {
  label: '',
  accountIds: [],
};

export function SystemsView({ onSystemsChanged, token }: SystemsViewProps) {
  const [systems, setSystems] = useState<DashboardSystem[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [message, setMessage] = useState('');
  const [mode, setMode] = useState<SystemEditorMode>({ type: 'list' });
  const [form, setForm] = useState<SystemFormState>(emptySystemForm);

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
    setForm(emptySystemForm);
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
