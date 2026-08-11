const authService = require('../services/auth.service');
const asyncHandler = require('../utils/asyncHandler');

const register = asyncHandler(async (req, res) => {
  const result = await authService.register(req.body);

  res.status(201).json({
    success: true,
    message: 'User registered successfully',
    data: result,
  });
});

const login = asyncHandler(async (req, res) => {
  const result = await authService.login(req.body);

  res.status(200).json({
    success: true,
    message: 'Login successful',
    data: result,
  });
});

const refresh = asyncHandler(async (req, res) => {
  const tokens = await authService.refresh(req.body.refreshToken);

  res.status(200).json({
    success: true,
    message: 'Tokens refreshed successfully',
    data: { tokens },
  });
});

const logout = asyncHandler(async (req, res) => {
  await authService.logout(req.body.refreshToken);

  res.status(200).json({
    success: true,
    message: 'Logout successful',
    data: null,
  });
});

const getCurrentUser = asyncHandler(async (req, res) => {
  const user = await authService.getCurrentUser(req.auth.userId);

  res.status(200).json({
    success: true,
    message: 'Current user retrieved successfully',
    data: { user },
  });
});

module.exports = {
  getCurrentUser,
  login,
  logout,
  refresh,
  register,
};
