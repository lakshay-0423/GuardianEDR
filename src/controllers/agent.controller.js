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

module.exports = { register };
