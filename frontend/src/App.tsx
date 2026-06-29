const navigationItems = ['Factions', 'Templates', 'Users', 'Audit'];

export function App() {
  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true">M</span>
          <div>
            <p className="brand-name">ML Dev Dashboard</p>
            <p className="brand-subtitle">CRM admin</p>
          </div>
        </div>

        <nav aria-label="Primary navigation">
          {navigationItems.map((item) => (
            <button className="nav-item" key={item} type="button">
              {item}
            </button>
          ))}
        </nav>
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div>
            <h1>Project foundation</h1>
            <p>Authentication, data models, and CRM screens will be wired in as the domain model is defined.</p>
          </div>
          <button className="primary-action" type="button">Sign in</button>
        </header>

        <section className="status-grid" aria-label="Project status">
          <article>
            <span>Backend</span>
            <strong>API shell ready</strong>
          </article>
          <article>
            <span>Database</span>
            <strong>Awaiting initial model</strong>
          </article>
          <article>
            <span>Auth</span>
            <strong>JWT plan selected</strong>
          </article>
        </section>
      </section>
    </main>
  );
}
