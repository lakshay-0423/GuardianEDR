const crypto = require('crypto');

const prisma = require('../config/prisma');
const env = require('../config/env');
const { signAgentToken } = require('../utils/agentTokens');

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

  return {
    agentId: endpoint.agentId,
    agentToken,
    heartbeatInterval: env.AGENT_HEARTBEAT_INTERVAL_SECONDS,
  };
};

module.exports = { registerEndpoint };
