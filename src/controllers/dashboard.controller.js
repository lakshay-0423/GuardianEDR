const dashboardService = require('../services/dashboard.service');
const asyncHandler = require('../utils/asyncHandler');

const getSummary = asyncHandler(async (req, res) => {
  const summary = await dashboardService.getSummary(req.auth.userId);

  res.status(200).json({
    success: true,
    message: 'Dashboard summary retrieved successfully',
    data: { summary },
  });
});

module.exports = { getSummary };
