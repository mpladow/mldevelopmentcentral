import { useCallback, useState } from 'react';
import { apiBaseUrl } from '../../config/api';
import type { Account } from '../../types/accounts';

type CreateAccountPayload = {
  email: string;
  displayName: string;
  password: string;
  roles: string[];
};

type UpdateAccountPayload = {
  email: string;
  displayName: string;
  roles: string[];
};

export function useAccounts(token: string) {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [message, setMessage] = useState('');

  const clearMessage = useCallback(() => setMessage(''), []);

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

  const createAccount = useCallback(
    async (payload: CreateAccountPayload) => {
      setMessage('');

      const response = await fetch(`${apiBaseUrl}/api/accounts`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        setMessage('Unable to create the account.');
        return false;
      }

      setMessage('Account created.');
      await loadAccounts();
      return true;
    },
    [loadAccounts, token],
  );

  const updateAccount = useCallback(
    async (accountId: string, payload: UpdateAccountPayload) => {
      setMessage('');

      const response = await fetch(`${apiBaseUrl}/api/accounts/${accountId}`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        setMessage('Unable to update the account.');
        return false;
      }

      setMessage('Account updated.');
      await loadAccounts();
      return true;
    },
    [loadAccounts, token],
  );

  return {
    accounts,
    message,
    clearMessage,
    loadAccounts,
    createAccount,
    updateAccount,
  };
}
