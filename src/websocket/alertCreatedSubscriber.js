const { setAlertCreatedHandler } = require('../config/prismaEvents');

const registerAlertCreatedSubscriber = (onAlertCreated) => {
  setAlertCreatedHandler(onAlertCreated);
};

module.exports = registerAlertCreatedSubscriber;
