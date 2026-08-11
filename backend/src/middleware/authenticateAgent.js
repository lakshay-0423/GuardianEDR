const HttpError = require('../utils/httpError');
const asyncHandler = require('../utils/asyncHandler');
const { verifyAgentToken } = require('../utils/agentTokens');

const authenticateAgent = asyncHandler(async (req, res, next) => {
  const authorization = req.get('authorization');

  if (!authorization?.startsWith('Bearer ')) {
    throw new HttpError(401, 'Agent token is required');
  }

  const token = authorization.slice('Bearer '.length).trim();

  if (!token) {
    throw new HttpError(401, 'Agent token is required');
  }

  try {
    const payload = verifyAgentToken(token);

    if (
      payload.type !== 'agent'
      || typeof payload.sub !== 'string'
      || typeof payload.agentId !== 'string'
      || typeof payload.deviceUuid !== 'string'
    ) {
      throw new Error('Invalid agent token payload');
    }

    req.agent = {
      agentId: payload.agentId,
      deviceUuid: payload.deviceUuid,
      endpointId: payload.sub,
    };
    return next();
  } catch {
    throw new HttpError(401, 'Invalid or expired agent token');
  }
});

module.exports = authenticateAgent;
