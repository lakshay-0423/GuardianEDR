const crypto = require('crypto');
const jwt = require('jsonwebtoken');
const env = require('../config/env');

const tokenOptions = {
  issuer: 'guardian-edr',
  audience: 'guardian-edr-dashboard',
};

const durationInMilliseconds = (duration) => {
  const amount = Number.parseInt(duration, 10);
  const unit = duration.at(-1);
  const multipliers = {
    s: 1000,
    m: 60 * 1000,
    h: 60 * 60 * 1000,
    d: 24 * 60 * 60 * 1000,
  };

  return amount * multipliers[unit];
};

const signAccessToken = (userId) => jwt.sign(
  { type: 'access' },
  env.JWT_ACCESS_SECRET,
  {
    ...tokenOptions,
    subject: userId,
    expiresIn: env.JWT_ACCESS_EXPIRES_IN,
  },
);

const signRefreshToken = (userId) => jwt.sign(
  { type: 'refresh', jti: crypto.randomUUID() },
  env.JWT_REFRESH_SECRET,
  {
    ...tokenOptions,
    subject: userId,
    expiresIn: env.JWT_REFRESH_EXPIRES_IN,
  },
);

const verifyAccessToken = (token) => jwt.verify(token, env.JWT_ACCESS_SECRET, tokenOptions);

const verifyRefreshToken = (token) => jwt.verify(token, env.JWT_REFRESH_SECRET, tokenOptions);

const hashToken = (token) => crypto.createHash('sha256').update(token).digest('hex');

const getRefreshTokenExpiry = () => (
  new Date(Date.now() + durationInMilliseconds(env.JWT_REFRESH_EXPIRES_IN))
);

module.exports = {
  getRefreshTokenExpiry,
  hashToken,
  signAccessToken,
  signRefreshToken,
  verifyAccessToken,
  verifyRefreshToken,
};
