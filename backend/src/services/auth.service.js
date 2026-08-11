const bcrypt = require('bcrypt');
const { Prisma } = require('@prisma/client');

const env = require('../config/env');
const prisma = require('../config/prisma');
const HttpError = require('../utils/httpError');
const {
  getRefreshTokenExpiry,
  hashToken,
  signAccessToken,
  signRefreshToken,
  verifyRefreshToken,
} = require('../utils/authTokens');

const publicUserFields = {
  id: true,
  email: true,
  createdAt: true,
  updatedAt: true,
};

const authenticateRefreshToken = (refreshToken) => {
  try {
    const payload = verifyRefreshToken(refreshToken);

    if (payload.type !== 'refresh' || typeof payload.sub !== 'string') {
      throw new Error('Invalid refresh token payload');
    }

    return payload;
  } catch {
    throw new HttpError(401, 'Invalid or expired refresh token');
  }
};

const createTokenPair = async (userId, database = prisma) => {
  const accessToken = signAccessToken(userId);
  const refreshToken = signRefreshToken(userId);

  await database.refreshToken.create({
    data: {
      userId,
      tokenHash: hashToken(refreshToken),
      expiresAt: getRefreshTokenExpiry(),
    },
  });

  return { accessToken, refreshToken };
};

const register = async ({ email, password }) => {
  const passwordHash = await bcrypt.hash(password, env.BCRYPT_SALT_ROUNDS);

  try {
    return await prisma.$transaction(async (transaction) => {
      const user = await transaction.user.create({
        data: { email, passwordHash },
        select: publicUserFields,
      });

      const tokens = await createTokenPair(user.id, transaction);
      return { user, tokens };
    });
  } catch (error) {
    if (error instanceof Prisma.PrismaClientKnownRequestError && error.code === 'P2002') {
      throw new HttpError(409, 'A user with this email already exists');
    }

    throw error;
  }
};

const login = async ({ email, password }) => {
  const user = await prisma.user.findUnique({
    where: { email },
    select: {
      ...publicUserFields,
      passwordHash: true,
    },
  });

  const isPasswordValid = user
    ? await bcrypt.compare(password, user.passwordHash)
    : false;

  if (!isPasswordValid) {
    throw new HttpError(401, 'Invalid email or password');
  }

  const { passwordHash, ...publicUser } = user;
  const tokens = await createTokenPair(user.id);

  return { user: publicUser, tokens };
};

const refresh = async (refreshToken) => {
  const payload = authenticateRefreshToken(refreshToken);
  const currentTime = new Date();
  const tokenHash = hashToken(refreshToken);

  const storedToken = await prisma.refreshToken.findUnique({
    where: { tokenHash },
    select: {
      id: true,
      userId: true,
      expiresAt: true,
      revokedAt: true,
    },
  });

  if (
    !storedToken
    || storedToken.userId !== payload.sub
    || storedToken.revokedAt
    || storedToken.expiresAt <= currentTime
  ) {
    throw new HttpError(401, 'Invalid or expired refresh token');
  }

  return prisma.$transaction(async (transaction) => {
    const revocation = await transaction.refreshToken.updateMany({
      where: {
        id: storedToken.id,
        revokedAt: null,
        expiresAt: { gt: currentTime },
      },
      data: { revokedAt: currentTime },
    });

    if (revocation.count !== 1) {
      throw new HttpError(401, 'Invalid or expired refresh token');
    }

    return createTokenPair(storedToken.userId, transaction);
  });
};

const logout = async (refreshToken) => {
  await prisma.refreshToken.updateMany({
    where: {
      tokenHash: hashToken(refreshToken),
      revokedAt: null,
    },
    data: { revokedAt: new Date() },
  });
};

const getCurrentUser = async (userId) => {
  const user = await prisma.user.findUnique({
    where: { id: userId },
    select: publicUserFields,
  });

  if (!user) {
    throw new HttpError(401, 'Authentication is no longer valid');
  }

  return user;
};

module.exports = {
  getCurrentUser,
  login,
  logout,
  refresh,
  register,
};
