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

module.exports = {
  getEndpointById,
  listEndpoints,
};
