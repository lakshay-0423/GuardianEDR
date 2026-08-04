const logger = require('../config/logger');

const errorHandler = (error, req, res, next) => {
  logger.error('Unhandled request error', {
    message: error.message,
    stack: error.stack,
    method: req.method,
    path: req.originalUrl,
  });

  if (res.headersSent) {
    return next(error);
  }

  const statusCode = error.statusCode || 500;

  return res.status(statusCode).json({
    success: false,
    message: statusCode >= 500 ? 'Internal server error' : error.message,
  });
};

module.exports = errorHandler;

