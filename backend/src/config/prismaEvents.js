let alertCreatedHandler;

const emitAlertCreated = (alert) => {
  alertCreatedHandler?.(alert);
};

const setAlertCreatedHandler = (handler) => {
  alertCreatedHandler = handler;
};

module.exports = {
  emitAlertCreated,
  setAlertCreatedHandler,
};
