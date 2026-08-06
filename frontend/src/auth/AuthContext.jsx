import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';

import {
  apiRequest,
  clearStoredTokens,
  getStoredTokens,
  storeTokens,
} from '../api/client';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isReady, setIsReady] = useState(false);

  const clearSession = useCallback(() => {
    clearStoredTokens();
    setUser(null);
  }, []);

  useEffect(() => {
    const restoreSession = async () => {
      if (!getStoredTokens()) {
        setIsReady(true);
        return;
      }

      try {
        const response = await apiRequest('/api/users/me');
        setUser(response.data.user);
      } catch {
        clearSession();
      } finally {
        setIsReady(true);
      }
    };

    restoreSession();
  }, [clearSession]);

  const login = useCallback(async (credentials) => {
    const response = await apiRequest('/api/auth/login', {
      method: 'POST',
      body: credentials,
    }, { authenticate: false });

    storeTokens(response.data.tokens);
    setUser(response.data.user);
  }, []);

  const logout = useCallback(async () => {
    const tokens = getStoredTokens();

    try {
      if (tokens?.refreshToken) {
        await apiRequest('/api/auth/logout', {
          method: 'POST',
          body: { refreshToken: tokens.refreshToken },
        }, { authenticate: false, retryOnUnauthorized: false });
      }
    } finally {
      clearSession();
    }
  }, [clearSession]);

  const value = useMemo(() => ({
    isAuthenticated: Boolean(user),
    isReady,
    login,
    logout,
    user,
  }), [isReady, login, logout, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
};

