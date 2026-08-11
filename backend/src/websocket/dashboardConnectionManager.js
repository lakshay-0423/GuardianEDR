const { WebSocket, WebSocketServer } = require('ws');

const logger = require('../config/logger');
const { verifyAccessToken } = require('../utils/authTokens');

const parseDashboardConnection = (request) => {
  const requestUrl = new URL(request.url, `http://${request.headers.host ?? 'localhost'}`);
  const token = requestUrl.searchParams.get('token');

  if (!token) {
    throw new Error('Dashboard token is required');
  }

  const payload = verifyAccessToken(token);

  if (payload.type !== 'access' || typeof payload.sub !== 'string') {
    throw new Error('Invalid dashboard token');
  }

  return { userId: payload.sub };
};

const createDashboardConnectionManager = (server) => {
  const webSocketServer = new WebSocketServer({ noServer: true });
  const connections = new Map();

  server.on('upgrade', (request, socket, head) => {
    const requestUrl = new URL(request.url, `http://${request.headers.host ?? 'localhost'}`);

    if (requestUrl.pathname !== '/ws') {
      socket.destroy();
      return;
    }

    let connection;

    try {
      connection = parseDashboardConnection(request);
    } catch {
      socket.write('HTTP/1.1 401 Unauthorized\r\nConnection: close\r\n\r\n');
      socket.destroy();
      return;
    }

    webSocketServer.handleUpgrade(request, socket, head, (webSocket) => {
      webSocketServer.emit('connection', webSocket, request, connection);
    });
  });

  webSocketServer.on('connection', (socket, request, connection) => {
    socket.isAlive = true;
    connections.set(socket, connection);

    logger.info('Dashboard WebSocket client connected', {
      ip: request.socket.remoteAddress,
      userId: connection.userId,
    });

    socket.on('pong', () => {
      socket.isAlive = true;
    });

    socket.on('close', () => {
      connections.delete(socket);
      logger.info('Dashboard WebSocket client disconnected', { userId: connection.userId });
    });

    socket.on('error', (error) => {
      logger.warn('Dashboard WebSocket client error', {
        message: error.message,
        userId: connection.userId,
      });
    });
  });

  const pingInterval = setInterval(() => {
    connections.forEach((connection, socket) => {
      if (!socket.isAlive) {
        connections.delete(socket);
        socket.terminate();
        return;
      }

      socket.isAlive = false;
      socket.ping();
    });
  }, 30000);

  const broadcast = (event, data) => {
    const message = JSON.stringify({
      data,
      event,
      timestamp: new Date().toISOString(),
    });

    connections.forEach((connection, socket) => {
      if (socket.readyState !== WebSocket.OPEN) {
        return;
      }

      try {
        socket.send(message);
      } catch (error) {
        logger.warn('Dashboard WebSocket broadcast failed', {
          event,
          message: error.message,
          userId: connection.userId,
        });
      }
    });
  };

  const close = () => {
    clearInterval(pingInterval);
    connections.forEach((connection, socket) => {
      socket.close(1001, 'Server shutting down');
    });
    webSocketServer.close();
  };

  return { broadcast, close };
};

module.exports = createDashboardConnectionManager;
