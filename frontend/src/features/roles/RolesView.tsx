import { useMemo, useState } from 'react';
import { ShieldCheck } from 'lucide-react';
import { apiBaseUrl } from '../../config/api';

type RolesViewProps = {
  token: string;
};

export function RolesView({ token }: RolesViewProps) {
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
