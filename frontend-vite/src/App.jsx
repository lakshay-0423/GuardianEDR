import { useCallback, useEffect, useState } from 'react';

import useAuth from './auth/useAuth';
import LoadingState from './components/LoadingState';
import DashboardPage from './pages/DashboardPage';
import LoginPage from './pages/LoginPage';

const App = () => {
  const { isAuthenticated, isReady } = useAuth();
  const [path, setPath] = useState(() => window.location.pathname);

  const navigate = useCallback((target, replace = false) => {
    window.history[replace ? 'replaceState' : 'pushState']({}, '', target);
    setPath(target);
  }, []);

  useEffect(() => {
    const handlePopState = () => setPath(window.location.pathname);

    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, []);

  useEffect(() => {
    if (!isReady) {
      return;
    }

    if (!isAuthenticated && path !== '/login') {
      navigate('/login', true);
    }

    if (isAuthenticated && path !== '/') {
      navigate('/', true);
    }
  }, [isAuthenticated, isReady, navigate, path]);

  if (!isReady) {
    return <LoadingState label="Restoring your session" fullScreen />;
  }

  if (!isAuthenticated) {
    return <LoginPage onAuthenticated={() => navigate('/', true)} />;
  }

  return <DashboardPage />;
};

export default App;
