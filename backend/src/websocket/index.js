const env = require('../config/env');
const createDashboardConnectionManager = require('./dashboardConnectionManager');
const createEndpointStatusMonitor = require('./endpointStatusMonitor');
const registerAlertCreatedSubscriber = require('./alertCreatedSubscriber');

let connectionManager;

const publish = (event, data) => {
  connectionManager?.broadcast(event, data);
};

const publishEndpointOnline = (endpoint) => publish('endpoint.online', endpoint);
const publishEndpointOffline = (endpoint) => publish('endpoint.offline', endpoint);
const publishHeartbeatUpdate = (endpoint) => publish('heartbeat.update', endpoint);
const publishAlertCreated = (alert) => publish('alert.created', alert);

const initializeWebSocketServer = (server) => {
  connectionManager = createDashboardConnectionManager(server);
  const endpointStatusMonitor = createEndpointStatusMonitor({
    checkIntervalSeconds: env.ENDPOINT_STATUS_CHECK_INTERVAL_SECONDS,
    offlineThresholdSeconds: env.ENDPOINT_OFFLINE_THRESHOLD_SECONDS,
    onEndpointOffline: publishEndpointOffline,
  });

  registerAlertCreatedSubscriber(publishAlertCreated);
  endpointStatusMonitor.start();

  return {
    close: () => {
      endpointStatusMonitor.stop();
      connectionManager.close();
    },
  };
};

module.exports = {
  initializeWebSocketServer,
  publishEndpointOnline,
  publishHeartbeatUpdate,
};
