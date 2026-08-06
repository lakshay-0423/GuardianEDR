const Sidebar = () => (
  <aside className="sidebar">
    <div className="brand">
      <span className="brand__mark" aria-hidden="true">G</span>
      <span>Guardian EDR</span>
    </div>

    <nav className="sidebar__nav" aria-label="Primary navigation">
      <a href="/" className="sidebar__link active">
        <span aria-hidden="true">◈</span>
        Overview
      </a>
    </nav>

    <div className="sidebar__footer">
      <span className="status-dot" aria-hidden="true" />
      Platform protected
    </div>
  </aside>
);

export default Sidebar;
