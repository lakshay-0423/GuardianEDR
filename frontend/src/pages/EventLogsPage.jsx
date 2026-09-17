import { useCallback, useEffect, useMemo, useState } from 'react';

import { apiRequest } from '../api/client';
import useAuth from '../auth/useAuth';
import DashboardLayout from '../components/DashboardLayout';
import LoadingState from '../components/LoadingState';
import useDashboardSocket from '../websocket/useDashboardSocket';

const eventTypeLabels = {
  PROCESS_STARTED: 'Process started',
  PROCESS_TERMINATED: 'Process terminated',
};

const formatTimestamp = (timestamp) => new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'medium',
}).format(new Date(timestamp));

const formatParentProcess = (process) => {
  if (!process.parentName && !process.parentId) {
    return 'Unavailable';
  }

  return process.parentId ? `${process.parentName ?? 'Unknown'} (${process.parentId})` : process.parentName;
};

const EventLogsPage = () => {
  const { logout, user } = useAuth();
  const [endpoints, setEndpoints] = useState([]);
  const [endpointId, setEndpointId] = useState('');
  const [eventType, setEventType] = useState('');
  const [events, setEvents] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSigningOut, setIsSigningOut] = useState(false);
  const [page, setPage] = useState(1);

  const eventLogPath = useMemo(() => {
    const query = new URLSearchParams({ limit: '25', page: String(page) });

    if (endpointId) {
      query.set('endpointId', endpointId);
    }

    if (eventType) {
      query.set('eventType', eventType);
    }

    return `/api/events?${query.toString()}`;
  }, [endpointId, eventType, page]);

  const loadEvents = useCallback(async () => {
    setError('');
    setIsLoading(true);

    try {
      const response = await apiRequest(eventLogPath);
      setEvents(response.data.events);
      setPagination(response.data.pagination);
    } catch (requestError) {
      setError(requestError.message ?? 'Unable to load event logs.');
    } finally {
      setIsLoading(false);
    }
  }, [eventLogPath]);

  useEffect(() => {
    loadEvents();
  }, [loadEvents]);

  useEffect(() => {
    const loadEndpoints = async () => {
      try {
        const response = await apiRequest('/api/endpoints');
        setEndpoints(response.data.endpoints);
      } catch (requestError) {
        setError(requestError.message ?? 'Unable to load endpoint filters.');
      }
    };

    loadEndpoints();
  }, []);

  const connectionStatus = useDashboardSocket();

  const handleEndpointFilter = (event) => {
    setEndpointId(event.target.value);
    setPage(1);
  };

  const handleEventTypeFilter = (event) => {
    setEventType(event.target.value);
    setPage(1);
  };

  const handleLogout = async () => {
    setIsSigningOut(true);
    await logout();
  };

  const totalPages = pagination?.totalPages ?? 0;

  return (
    <>
      <DashboardLayout
        connectionStatus={connectionStatus}
        title="Event logs"
        user={user}
        onLogout={handleLogout}
      >
        <section className="event-log-panel" aria-labelledby="event-log-title">
          <div className="section-heading">
            <div>
              <p className="eyebrow">Telemetry</p>
              <h2 id="event-log-title">Endpoint event logs</h2>
            </div>
            <span>{pagination?.total ?? 0} events</span>
          </div>

          <div className="event-log-filters">
            <label>
              Endpoint
              <select value={endpointId} onChange={handleEndpointFilter}>
                <option value="">All endpoints</option>
                {endpoints.map((endpoint) => (
                  <option key={endpoint.id} value={endpoint.id}>{endpoint.hostname}</option>
                ))}
              </select>
            </label>
            <label>
              Event type
              <select value={eventType} onChange={handleEventTypeFilter}>
                <option value="">All event types</option>
                {Object.entries(eventTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>{label}</option>
                ))}
              </select>
            </label>
          </div>

          {isLoading && <LoadingState label="Loading event logs" />}

          {!isLoading && error && (
            <div className="event-log-error" role="alert">
              <p>{error}</p>
              <button className="button button--primary" type="button" onClick={loadEvents}>Try again</button>
            </div>
          )}

          {!isLoading && !error && events.length === 0 && (
            <div className="empty-state">
              <h3>No matching events</h3>
              <p>Process telemetry will appear here as enrolled endpoints report activity.</p>
            </div>
          )}

          {!isLoading && !error && events.length > 0 && (
            <>
              <div className="table-wrapper event-log-table-wrapper">
                <table className="event-log-table">
                  <thead>
                    <tr>
                      <th>Timestamp</th>
                      <th>Endpoint</th>
                      <th>Event type</th>
                      <th>Process</th>
                      <th>Parent process</th>
                      <th>Executable path</th>
                      <th>Username</th>
                    </tr>
                  </thead>
                  <tbody>
                    {events.map((event) => (
                      <tr key={event.id}>
                        <td>{formatTimestamp(event.timestamp)}</td>
                        <td>{event.endpoint.hostname}</td>
                        <td><span className="event-type">{eventTypeLabels[event.eventType] ?? event.eventType}</span></td>
                        <td>
                          <strong>{event.process.name ?? 'Unavailable'}</strong>
                          <span>PID {event.process.id ?? 'Unavailable'}</span>
                        </td>
                        <td>{formatParentProcess(event.process)}</td>
                        <td className="event-log-table__path">{event.process.executablePath ?? 'Unavailable'}</td>
                        <td>{event.process.username ?? 'Unavailable'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <nav className="pagination" aria-label="Event log pagination">
                <span>Page {pagination.page} of {Math.max(totalPages, 1)}</span>
                <div>
                  <button className="button button--quiet" type="button" disabled={page === 1} onClick={() => setPage((current) => current - 1)}>Previous</button>
                  <button className="button button--quiet" type="button" disabled={page >= totalPages} onClick={() => setPage((current) => current + 1)}>Next</button>
                </div>
              </nav>
            </>
          )}
        </section>
      </DashboardLayout>
      {isSigningOut && <LoadingState label="Signing out" fullScreen />}
    </>
  );
};

export default EventLogsPage;
