const logger = require('../config/logger');
const { markInactiveEndpointsOffline } = require('../services/endpointStatus.service');

const createEndpointStatusMonitor = ({
  checkIntervalSeconds,
  offlineThresholdSeconds,
  onEndpointOffline,
}) => {
  let isRunning = false;
  let interval;

  const checkEndpointStatuses = async () => {
    if (isRunning) {
      return;
    }

    isRunning = true;

    try {
      const offlineEndpoints = await markInactiveEndpointsOffline(offlineThresholdSeconds);
      offlineEndpoints.forEach(onEndpointOffline);
    } catch (error) {
      logger.error('Endpoint status monitor failed', { message: error.message });
    } finally {
      isRunning = false;
    }
  };

  return {
    start: () => {
      void checkEndpointStatuses();
      interval = setInterval(() => {
        void checkEndpointStatuses();
      }, checkIntervalSeconds * 1000);
    },
    stop: () => {
      clearInterval(interval);
    },
  };
};

module.exports = createEndpointStatusMonitor;
