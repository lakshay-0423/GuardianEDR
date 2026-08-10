import { useCallback, useEffect, useState } from 'react';

import { apiRequest } from '../api/client';
import useAuth from '../auth/useAuth';
import LoadingState from '../components/LoadingState';
import Sidebar from '../components/Sidebar';
import StatCard from '../components/StatCard';
import TopNavigation from '../components/TopNavigation';
import useDashboardSocket from '../websocket/useDashboardSocket';

const dashboardEvents = new Set([
  'endpoint.online',
  'endpoint.offline',
  'heartbeat.update',
  'alert.created',
]);

const formatLastSeen = (dateTime) => {
  if (!dateTime) {
    return 'No recent activity';
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(dateTime));
};

const DashboardPage = () => {
  const { logout, user } = useAuth();
  const [summary, setSummary] = useState(null);
  const [endpoints, setEndpoints] = useState([]);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSigningOut, setIsSigningOut] = useState(false);

  const loadDashboard = useCallback(async ({ showLoading = true } = {}) => {
    setError('');

    if (showLoading) {
      setIsLoading(true);
    }

    try {
      const [summaryResponse, endpointsResponse] = await Promise.all([
        apiRequest('/api/dashboard/summary'),
        apiRequest('/api/endpoints'),
      ]);

      setSummary(summaryResponse.data.summary);
      setEndpoints(endpointsResponse.data.endpoints);
    } catch (requestError) {
      setError(requestError.message ?? 'Unable to load dashboard data.');
    } finally {
      if (showLoading) {
        setIsLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  const handleDashboardEvent = useCallback((event) => {
    if (dashboardEvents.has(event.event)) {
      loadDashboard({ showLoading: false });
    }
  }, [loadDashboard]);

  const connectionStatus = useDashboardSocket(handleDashboardEvent);

  const handleLogout = async () => {
    setIsSigningOut(true);
    await logout();
  };

  return (
    <div className="dashboard-shell">
      <Sidebar />
      <main className="dashboard-content">
        <TopNavigation
          connectionStatus={connectionStatus}
          user={user}
          onLogout={handleLogout}
        />

        {isLoading && !summary && <LoadingState label="Loading dashboard data" />}

        {!isLoading && error && !summary && (
          <section className="error-panel" role="alert">
            <div>
              <p className="eyebrow">Data unavailable</p>
              <h2>We could not load your dashboard.</h2>
              <p>{error}</p>
            </div>
            <button className="button button--primary" type="button" onClick={loadDashboard}>
              Try again
            </button>
          </section>
        )}

        {summary && (
          <>
            {error && <p className="dashboard-update-error" role="alert">{error}</p>}
            <section className="summary-grid" aria-label="Endpoint security summary">
              <StatCard label="Total endpoints" value={summary.totalEndpoints} tone="neutral" />
              <StatCard label="Online endpoints" value={summary.onlineEndpoints} tone="success" />
              <StatCard label="Offline endpoints" value={summary.offlineEndpoints} tone="muted" />
              <StatCard label="Total alerts" value={summary.totalAlerts} tone="alert" />
            </section>

            <section className="endpoint-panel" aria-labelledby="endpoint-title">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">Inventory</p>
                  <h2 id="endpoint-title">Endpoints</h2>
                </div>
                <span>{endpoints.length} total</span>
              </div>

              {endpoints.length === 0 ? (
                <div className="empty-state">
                  <h3>No endpoints yet</h3>
                  <p>Enrolled devices will appear here when they begin reporting.</p>
                </div>
              ) : (
                <div className="table-wrapper">
                  <table>
                    <thead>
                      <tr>
                        <th>Endpoint</th>
                        <th>Platform</th>
                        <th>Status</th>
                        <th>Last seen</th>
                      </tr>
                    </thead>
                    <tbody>
                      {endpoints.map((endpoint) => (
                        <tr key={endpoint.id}>
                          <td>
                            <strong>{endpoint.hostname}</strong>
                            <span>{endpoint.ipAddress ?? endpoint.agentId}</span>
                          </td>
                          <td>{endpoint.osName ?? 'Unknown'}</td>
                          <td>
                            <span className={`endpoint-status endpoint-status--${endpoint.status.toLowerCase()}`}>
                              {endpoint.status.toLowerCase()}
                            </span>
                          </td>
                          <td>{formatLastSeen(endpoint.lastSeenAt)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </section>
          </>
        )}
      </main>
      {isSigningOut && <LoadingState label="Signing out" fullScreen />}
    </div>
  );
};

export default DashboardPage;
