const prisma = require('../config/prisma');
const { getDashboardEndpointFilter } = require('./endpointAccess.service');

const getSummary = async (userId) => {
  const endpointFilter = getDashboardEndpointFilter(userId);

  const [totalEndpoints, onlineEndpoints, offlineEndpoints, totalAlerts] = await Promise.all([
    prisma.endpoint.count({ where: endpointFilter }),
    prisma.endpoint.count({
      where: { ...endpointFilter, status: 'ONLINE' },
    }),
    prisma.endpoint.count({
      where: { ...endpointFilter, status: 'OFFLINE' },
    }),
    prisma.alert.count({
      where: { endpoint: { is: endpointFilter } },
    }),
  ]);

  return {
    totalEndpoints,
    onlineEndpoints,
    offlineEndpoints,
    totalAlerts,
  };
};

module.exports = { getSummary };
