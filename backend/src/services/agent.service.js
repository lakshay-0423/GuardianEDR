const crypto = require('crypto');

const prisma = require('../config/prisma');
const env = require('../config/env');
const { signAgentToken } = require('../utils/agentTokens');
const HttpError = require('../utils/httpError');
const { publishEndpointOnline, publishHeartbeatUpdate } = require('../websocket');

const registerEndpoint = async ({
  agentVersion,
  architecture,
  deviceUuid,
  hostname,
  operatingSystem,
  username,
}) => {
  const endpointData = {
    agentVersion,
    architecture,
    hostname,
    osName: operatingSystem,
    status: 'ONLINE',
    lastSeenAt: new Date(),
    username,
  };

  const existingEndpoint = await prisma.endpoint.findUnique({
    where: { deviceUuid },
  });

  const endpoint = existingEndpoint
    ? await prisma.endpoint.update({
      where: { id: existingEndpoint.id },
      data: endpointData,
    })
    : await prisma.endpoint.create({
      data: {
        ...endpointData,
        agentId: crypto.randomUUID(),
        deviceUuid,
      },
    });

  const agentToken = signAgentToken({
    agentId: endpoint.agentId,
    deviceUuid: endpoint.deviceUuid,
    endpointId: endpoint.id,
  });

  if (!existingEndpoint || existingEndpoint.status !== 'ONLINE') {
    publishEndpointOnline(endpoint);
  }

  return {
    agentId: endpoint.agentId,
    agentToken,
    heartbeatInterval: env.AGENT_HEARTBEAT_INTERVAL_SECONDS,
  };
};

const recordHeartbeat = async ({ agentId, deviceUuid, endpointId }, {
  cpuUsage,
  memoryUsage,
  timestamp,
}) => {
  const endpoint = await prisma.endpoint.findFirst({
    where: {
      id: endpointId,
      agentId,
      deviceUuid,
    },
    select: {
      id: true,
      status: true,
    },
  });

  if (!endpoint) {
    throw new HttpError(401, 'Agent is not registered');
  }

  const updatedEndpoint = await prisma.endpoint.update({
    where: { id: endpoint.id },
    data: {
      cpuUsage,
      lastSeenAt: timestamp,
      memoryUsage,
      status: 'ONLINE',
    },
    select: {
      agentId: true,
      cpuUsage: true,
      id: true,
      lastSeenAt: true,
      memoryUsage: true,
      status: true,
    },
  });

  if (endpoint.status !== 'ONLINE') {
    publishEndpointOnline(updatedEndpoint);
  }

  publishHeartbeatUpdate(updatedEndpoint);
  return updatedEndpoint;
};

const ingestEvent = async ({ agentId, deviceUuid, endpointId }, {
  eventType,
  payload,
  timestamp,
}) => {
  const endpoint = await prisma.endpoint.findFirst({
    where: {
      id: endpointId,
      agentId,
      deviceUuid,
    },
    select: { id: true },
  });

  if (!endpoint) {
    throw new HttpError(401, 'Agent is not registered');
  }

  return prisma.event.create({
    data: {
      endpointId: endpoint.id,
      eventType,
      occurredAt: timestamp,
      payload,
    },
    select: {
      endpointId: true,
      eventType: true,
      id: true,
      occurredAt: true,
      payload: true,
    },
  });
};

module.exports = {
  ingestEvent,
  recordHeartbeat,
  registerEndpoint,
};
