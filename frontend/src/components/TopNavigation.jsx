const getInitial = (email) => email?.charAt(0).toUpperCase() ?? 'U';

const connectionLabels = {
  connected: 'Realtime connected',
  connecting: 'Connecting realtime',
  disconnected: 'Realtime unavailable',
  reconnecting: 'Reconnecting realtime',
};

const TopNavigation = ({ user, onLogout, connectionStatus }) => (
  <header className="top-navigation">
    <div>
      <p className="eyebrow">Security operations</p>
      <h1>Dashboard overview</h1>
    </div>
    <div className="account-menu">
      <span className={`connection-status connection-status--${connectionStatus}`}>
        <span aria-hidden="true" className="connection-status__dot" />
        {connectionLabels[connectionStatus] ?? connectionLabels.disconnected}
      </span>
      <span className="account-menu__avatar">{getInitial(user.email)}</span>
      <span className="account-menu__email">{user.email}</span>
      <button className="button button--quiet" type="button" onClick={onLogout}>
        Sign out
      </button>
    </div>
  </header>
);

export default TopNavigation;
