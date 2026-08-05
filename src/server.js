require('dotenv').config();

const app = require('./app');
const env = require('./config/env');
const logger = require('./config/logger');
const initializeWebSocketServer = require('./websocket');

const server = app.listen(env.PORT, () => {
  logger.info('Guardian EDR backend started', {
    environment: env.NODE_ENV,
    port: env.PORT,
  });
});

initializeWebSocketServer(server);

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
