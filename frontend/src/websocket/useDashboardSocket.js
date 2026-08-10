import { useEffect, useRef, useState } from 'react';

import { getStoredTokens } from '../api/client';

const INITIAL_RECONNECT_DELAY_MS = 1000;
const MAX_RECONNECT_DELAY_MS = 30000;

const createWebSocketUrl = (accessToken) => {
  const fallbackBaseUrl = `${window.location.protocol === 'https:' ? 'wss' : 'ws'}://${window.location.host}`;
  const configuredBaseUrl = import.meta.env.VITE_WS_BASE_URL ?? fallbackBaseUrl;
  const url = new URL('/ws', configuredBaseUrl);

  url.searchParams.set('token', accessToken);
  return url.toString();
};

const useDashboardSocket = (onEvent) => {
  const [connectionStatus, setConnectionStatus] = useState('connecting');
  const onEventRef = useRef(onEvent);

  useEffect(() => {
    onEventRef.current = onEvent;
  }, [onEvent]);

  useEffect(() => {
    if (!getStoredTokens()?.accessToken) {
      setConnectionStatus('disconnected');
      return undefined;
    }

    let reconnectTimer;
    let socket;
    let isActive = true;
    let reconnectAttempts = 0;

    const connect = () => {
      setConnectionStatus(reconnectAttempts === 0 ? 'connecting' : 'reconnecting');
      const accessToken = getStoredTokens()?.accessToken;

      if (!accessToken) {
        setConnectionStatus('disconnected');
        return;
      }

      try {
        socket = new WebSocket(createWebSocketUrl(accessToken));
      } catch {
        scheduleReconnect();
        return;
      }

      socket.onopen = () => {
        reconnectAttempts = 0;
        setConnectionStatus('connected');
      };

      socket.onmessage = (message) => {
        try {
          const event = JSON.parse(message.data);
          onEventRef.current?.(event);
        } catch {
          // Ignore malformed messages so the active dashboard connection remains usable.
        }
      };

      socket.onclose = () => {
        if (isActive) {
          scheduleReconnect();
        }
      };
    };

    const scheduleReconnect = () => {
      if (!isActive || reconnectTimer) {
        return;
      }

      const delay = Math.min(
        INITIAL_RECONNECT_DELAY_MS * 2 ** reconnectAttempts,
        MAX_RECONNECT_DELAY_MS,
      );

      reconnectAttempts += 1;
      setConnectionStatus('reconnecting');
      reconnectTimer = window.setTimeout(() => {
        reconnectTimer = undefined;
        connect();
      }, delay);
    };

    connect();

    return () => {
      isActive = false;
      window.clearTimeout(reconnectTimer);
      socket?.close();
    };
  }, []);

  return connectionStatus;
};

export default useDashboardSocket;
