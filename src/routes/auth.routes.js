const express = require('express');

const authController = require('../controllers/auth.controller');
const authenticate = require('../middleware/authenticate');
const validateBody = require('../middleware/validate');
const {
  loginSchema,
  refreshTokenSchema,
  registerSchema,
} = require('../validations/auth.validation');

const router = express.Router();

router.post('/register', validateBody(registerSchema), authController.register);
router.post('/login', validateBody(loginSchema), authController.login);
router.post('/refresh', validateBody(refreshTokenSchema), authController.refresh);
router.post('/logout', validateBody(refreshTokenSchema), authController.logout);
router.get('/me', authenticate, authController.getCurrentUser);

module.exports = router;
