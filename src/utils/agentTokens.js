const jwt = require('jsonwebtoken');

const env = require('../config/env');
const HttpError = require('./httpError');

const getAgentSecret = () => {
  if (!env.AGENT_JWT_SECRET) {
    throw new HttpError(500, 'Agent JWT secret is not configured');
  }

  return env.AGENT_JWT_SECRET;
};

const signAgentToken = ({ agentId, deviceUuid, endpointId }) => jwt.sign(
  {
    type: 'agent',
    agentId,
    deviceUuid,
  },
  getAgentSecret(),
  {
    audience: 'guardian-edr-agent',
    expiresIn: env.AGENT_JWT_EXPIRES_IN,
    issuer: 'guardian-edr',
    subject: endpointId,
  },
);

module.exports = { signAgentToken };
