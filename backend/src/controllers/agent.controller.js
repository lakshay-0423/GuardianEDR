const agentService = require('../services/agent.service');
const asyncHandler = require('../utils/asyncHandler');

const register = asyncHandler(async (req, res) => {
  const registration = await agentService.registerEndpoint(req.body);

  res.status(201).json({
    success: true,
    message: 'Endpoint registered successfully',
    data: registration,
  });
});

const heartbeat = asyncHandler(async (req, res) => {
  const endpoint = await agentService.recordHeartbeat(req.agent, req.body);

  res.status(200).json({
    success: true,
    message: 'Heartbeat received successfully',
    data: { endpoint },
  });
});

const ingestEvent = asyncHandler(async (req, res) => {
  const event = await agentService.ingestEvent(req.agent, req.body);

  res.status(201).json({
    success: true,
    message: 'Agent event received successfully',
    data: { event },
  });
});

module.exports = {
  heartbeat,
  ingestEvent,
  register,
};
