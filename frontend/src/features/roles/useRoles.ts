import { useCallback, useState } from 'react';
import { apiBaseUrl } from '../../config/api';
import type { PermissionCatalogItem, Role } from '../../types/roles';

export function useRoles(token: string) {
  const [permissions, setPermissions] = useState<PermissionCatalogItem[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [message, setMessage] = useState('');

  const loadRoles = useCallback(async () => {
    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/roles`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load roles.');
      return;
    }

    setRoles((await response.json()) as Role[]);
  }, [token]);

  const loadPermissions = useCallback(async () => {
    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/roles/permissions`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load permissions.');
      return;
    }

    setPermissions((await response.json()) as PermissionCatalogItem[]);
  }, [token]);

  const createRole = useCallback(async (
    payload: { systemId: number; name: string; displayName: string; permissions: string[] },
  ) => {
    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/roles`, {
      body: JSON.stringify(payload),
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    if (!response.ok) {
      setMessage('Unable to create role.');
      return false;
    }

    await loadRoles();
    return true;
  }, [loadRoles, token]);

  const updateRole = useCallback(async (
    roleId: number,
    payload: { displayName: string; permissions: string[] },
  ) => {
    setMessage('');

    const response = await fetch(`${apiBaseUrl}/api/roles/${roleId}`, {
      body: JSON.stringify(payload),
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      method: 'PUT',
    });

    if (!response.ok) {
      setMessage('Unable to update role.');
      return false;
    }

    await loadRoles();
    return true;
  }, [loadRoles, token]);

  return {
    createRole,
    loadPermissions,
    loadRoles,
    message,
    permissions,
    roles,
    updateRole,
  };
}
