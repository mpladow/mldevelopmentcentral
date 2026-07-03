import { useCallback, useState } from 'react';
import { apiBaseUrl } from '../../config/api';
import type { Account } from '../../types/accounts';
import type { DashboardSystem } from '../../types/systems';

type SystemPayload = {
	label: string;
	accounts: SystemAccountAssignmentPayload[];
};

type SystemAccountAssignmentPayload = {
	accountId: string;
	role: string;
};

export function useSystems(token: string) {
	const [systems, setSystems] = useState<DashboardSystem[]>([]);
	const [accounts, setAccounts] = useState<Account[]>([]);
	const [message, setMessage] = useState('');

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

	const saveSystem = useCallback(
		async (payload: SystemPayload, systemId?: number) => {
			setMessage('');

			const isCreateMode = systemId === undefined;
			const response = await fetch(
				isCreateMode ? `${apiBaseUrl}/api/systems` : `${apiBaseUrl}/api/systems/${systemId}`,
				{
					method: isCreateMode ? 'POST' : 'PUT',
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type': 'application/json',
					},
					body: JSON.stringify(payload),
				},
			);

			if (!response.ok) {
				setMessage(isCreateMode ? 'Unable to create the system.' : 'Unable to update the system.');
				return false;
			}

			setMessage(isCreateMode ? 'System created.' : 'System updated.');
			await loadSystems();
			return true;
		},
		[loadSystems, token],
	);

	const deleteSystem = useCallback(
		async (systemId: number) => {
			setMessage('');

			const response = await fetch(`${apiBaseUrl}/api/systems/${systemId}`, {
				method: 'DELETE',
				headers: { Authorization: `Bearer ${token}` },
			});

			if (!response.ok) {
				const body = await response.json().catch(() => null) as { message?: string } | null;
				setMessage(body?.message ?? 'Unable to delete the system.');
				return false;
			}

			setMessage('System deleted.');
			await loadSystems();
			return true;
		},
		[loadSystems, token],
	);

	return {
		systems,
		accounts,
		message,
		loadSystems,
		loadAccounts,
		saveSystem,
		deleteSystem,
	};
}
