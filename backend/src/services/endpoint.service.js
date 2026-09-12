const prisma = require('../config/prisma');
const HttpError = require('../utils/httpError');
const { getDashboardEndpointFilter } = require('./endpointAccess.service');

const endpointFields = {
  id: true,
  agentId: true,
  hostname: true,
  ipAddress: true,
  osName: true,
  status: true,
  lastSeenAt: true,
  createdAt: true,
  updatedAt: true,
};

const listEndpoints = (userId) => prisma.endpoint.findMany({
  where: getDashboardEndpointFilter(userId),
  select: endpointFields,
  orderBy: { updatedAt: 'desc' },
});

const getEndpointById = async (userId, endpointId) => {
  const endpoint = await prisma.endpoint.findFirst({
    where: {
      id: endpointId,
      ...getDashboardEndpointFilter(userId),
    },
    select: endpointFields,
  });

  if (!endpoint) {
    throw new HttpError(404, 'Endpoint not found');
  }

  return endpoint;
};

const listEndpointEvents = async (userId, endpointId, limit) => {
  const endpoint = await prisma.endpoint.findFirst({
    where: {
      id: endpointId,
      ...getDashboardEndpointFilter(userId),
    },
    select: { id: true },
  });

  if (!endpoint) {
    throw new HttpError(404, 'Endpoint not found');
  }

  return prisma.event.findMany({
    where: { endpointId: endpoint.id },
    select: {
      createdAt: true,
      endpointId: true,
      eventType: true,
      id: true,
      occurredAt: true,
      payload: true,
    },
    orderBy: { occurredAt: 'desc' },
    take: limit,
  });
};

module.exports = {
  getEndpointById,
  listEndpointEvents,
  listEndpoints,
};
