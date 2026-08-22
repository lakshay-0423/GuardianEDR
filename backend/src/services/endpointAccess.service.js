const getDashboardEndpointFilter = (userId) => ({
  OR: [
    { ownerId: userId },
    { ownerId: null },
  ],
});

module.exports = { getDashboardEndpointFilter };
