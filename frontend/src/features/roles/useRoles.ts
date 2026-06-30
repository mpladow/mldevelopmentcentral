import { useState } from 'react';
import { apiBaseUrl } from '../../config/api';

export function useRoles(token: string) {
  const [roles, setRoles] = useState<string[]>([]);
  const [message, setMessage] = useState('');

  const loadRoles = async () => {
    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/roles`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load roles.');
      return;
    }

    setRoles((await response.json()) as string[]);
  };

  return { roles, message, loadRoles };
}
