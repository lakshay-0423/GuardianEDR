require('dotenv').config();

const http = require('http');
const app = require('./app');
const env = require('./config/env');
const logger = require('./config/logger');
const initializeWebSocketServer = require('./websocket');

const server = http.createServer(app);
initializeWebSocketServer(server);

server.listen(env.PORT, () => {
  logger.info('Guardian EDR backend started', {
    environment: env.NODE_ENV,
    port: env.PORT,
  });
});

const shutdown = (signal) => {
  logger.info('Shutdown signal received', { signal });
  server.close((error) => {
    if (error) {
      logger.error('Error while shutting down server', { message: error.message });
      process.exit(1);
    }

    process.exit(0);
  });
};

process.once('SIGINT', () => shutdown('SIGINT'));
process.once('SIGTERM', () => shutdown('SIGTERM'));
