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

  const response = {
    success: false,
    message: statusCode >= 500 ? 'Internal server error' : error.message,
  };

  if (statusCode < 500 && error.details) {
    response.errors = error.details;
  }

  return res.status(statusCode).json(response);
};

module.exports = errorHandler;
