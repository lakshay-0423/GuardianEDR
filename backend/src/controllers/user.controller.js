const userService = require('../services/user.service');
const asyncHandler = require('../utils/asyncHandler');

const getCurrentUser = asyncHandler(async (req, res) => {
  const user = await userService.getCurrentUser(req.auth.userId);

  res.status(200).json({
    success: true,
    message: 'Current user retrieved successfully',
    data: { user },
  });
});

module.exports = { getCurrentUser };
