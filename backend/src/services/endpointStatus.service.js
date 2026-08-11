const prisma = require('../config/prisma');

const markInactiveEndpointsOffline = async (offlineThresholdSeconds) => {
  const cutoff = new Date(Date.now() - (offlineThresholdSeconds * 1000));
  const staleEndpoints = await prisma.endpoint.findMany({
    where: {
      lastSeenAt: { lt: cutoff },
      status: 'ONLINE',
    },
    select: {
      agentId: true,
      cpuUsage: true,
      hostname: true,
      id: true,
      lastSeenAt: true,
      memoryUsage: true,
    },
  });

  const transitionedEndpoints = [];

  for (const endpoint of staleEndpoints) {
    const update = await prisma.endpoint.updateMany({
      where: {
        id: endpoint.id,
        lastSeenAt: { lt: cutoff },
        status: 'ONLINE',
      },
      data: { status: 'OFFLINE' },
    });

    if (update.count === 1) {
      transitionedEndpoints.push({ ...endpoint, status: 'OFFLINE' });
    }
  }

  return transitionedEndpoints;
};

module.exports = { markInactiveEndpointsOffline };
