const getInitial = (email) => email?.charAt(0).toUpperCase() ?? 'U';

const TopNavigation = ({ user, onLogout }) => (
  <header className="top-navigation">
    <div>
      <p className="eyebrow">Security operations</p>
      <h1>Dashboard overview</h1>
    </div>
    <div className="account-menu">
      <span className="account-menu__avatar">{getInitial(user.email)}</span>
      <span className="account-menu__email">{user.email}</span>
      <button className="button button--quiet" type="button" onClick={onLogout}>
        Sign out
      </button>
    </div>
  </header>
);

export default TopNavigation;

