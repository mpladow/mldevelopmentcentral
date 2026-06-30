import type { ActiveView } from '../types/navigation';

type DashboardHeaderProps = {
  activeView: ActiveView;
};

export function DashboardHeader({ activeView }: DashboardHeaderProps) {
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
