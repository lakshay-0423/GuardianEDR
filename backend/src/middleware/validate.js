const HttpError = require('../utils/httpError');

const validate = (schema, source) => (req, res, next) => {
  const result = schema.safeParse(req[source]);

  if (!result.success) {
    return next(new HttpError(400, 'Request validation failed', result.error.flatten()));
  }

  req[source] = result.data;
  return next();
};

const validateBody = (schema) => validate(schema, 'body');
const validateParams = (schema) => validate(schema, 'params');

module.exports = validateBody;
module.exports.validateParams = validateParams;
