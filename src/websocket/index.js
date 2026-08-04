const { WebSocketServer } = require('ws');
const logger = require('../config/logger');

const initializeWebSocketServer = (server) => {
  const webSocketServer = new WebSocketServer({ server, path: '/ws' });

  webSocketServer.on('connection', (socket, request) => {
    logger.info('WebSocket client connected', { ip: request.socket.remoteAddress });

    socket.on('close', () => {
      logger.info('WebSocket client disconnected');
    });

    socket.on('error', (error) => {
      logger.warn('WebSocket client error', { message: error.message });
    });
  });

  return webSocketServer;
};

module.exports = initializeWebSocketServer;

