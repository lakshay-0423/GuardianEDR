const HttpError = require('../utils/httpError');

const validateBody = (schema) => (req, res, next) => {
  const result = schema.safeParse(req.body);

  if (!result.success) {
    return next(new HttpError(400, 'Request validation failed', result.error.flatten()));
  }

  req.body = result.data;
  return next();
};

module.exports = validateBody;
