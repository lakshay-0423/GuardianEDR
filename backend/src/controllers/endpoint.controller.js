const endpointService = require('../services/endpoint.service');
const asyncHandler = require('../utils/asyncHandler');

const listEndpoints = asyncHandler(async (req, res) => {
  const endpoints = await endpointService.listEndpoints(req.auth.userId);

  res.status(200).json({
    success: true,
    message: 'Endpoints retrieved successfully',
    data: { endpoints },
  });
});

const getEndpointById = asyncHandler(async (req, res) => {
  const endpoint = await endpointService.getEndpointById(req.auth.userId, req.params.id);

  res.status(200).json({
    success: true,
    message: 'Endpoint retrieved successfully',
    data: { endpoint },
  });
});

const listEndpointEvents = asyncHandler(async (req, res) => {
  const events = await endpointService.listEndpointEvents(
    req.auth.userId,
    req.params.id,
    req.validatedQuery.limit,
  );

  res.status(200).json({
    success: true,
    message: 'Endpoint events retrieved successfully',
    data: { events },
  });
});

module.exports = {
  getEndpointById,
  listEndpointEvents,
  listEndpoints,
};
