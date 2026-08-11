const prisma = require('../config/prisma');
const HttpError = require('../utils/httpError');

const getCurrentUser = async (userId) => {
  const user = await prisma.user.findUnique({
    where: { id: userId },
    select: {
      id: true,
      email: true,
      createdAt: true,
      updatedAt: true,
    },
  });

  if (!user) {
    throw new HttpError(401, 'Authentication is no longer valid');
  }

  return user;
};

module.exports = { getCurrentUser };
