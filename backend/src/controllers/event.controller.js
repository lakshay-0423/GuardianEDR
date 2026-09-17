const eventService = require('../services/event.service');
const asyncHandler = require('../utils/asyncHandler');

const listEventLogs = asyncHandler(async (req, res) => {
  const result = await eventService.listEventLogs(req.auth.userId, req.validatedQuery);

  res.status(200).json({
    success: true,
    message: 'Event logs retrieved successfully',
    data: result,
  });
});

module.exports = { listEventLogs };
