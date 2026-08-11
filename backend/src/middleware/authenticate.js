const HttpError = require('../utils/httpError');
const asyncHandler = require('../utils/asyncHandler');
const { verifyAccessToken } = require('../utils/authTokens');

const authenticate = asyncHandler(async (req, res, next) => {
  const authorization = req.get('authorization');

  if (!authorization?.startsWith('Bearer ')) {
    throw new HttpError(401, 'Authentication token is required');
  }

  const token = authorization.slice('Bearer '.length).trim();

  if (!token) {
    throw new HttpError(401, 'Authentication token is required');
  }

  try {
    const payload = verifyAccessToken(token);

    if (payload.type !== 'access' || typeof payload.sub !== 'string') {
      throw new Error('Invalid access token payload');
    }

    req.auth = { userId: payload.sub };
    return next();
  } catch {
    throw new HttpError(401, 'Invalid or expired access token');
  }
});

module.exports = authenticate;
