import Sidebar from './Sidebar';
import TopNavigation from './TopNavigation';

const DashboardLayout = ({ children, connectionStatus, onLogout, title, user }) => (
  <div className="dashboard-shell">
    <Sidebar />
    <main className="dashboard-content">
      <TopNavigation
        connectionStatus={connectionStatus}
        title={title}
        user={user}
        onLogout={onLogout}
      />
      {children}
    </main>
  </div>
);

export default DashboardLayout;
