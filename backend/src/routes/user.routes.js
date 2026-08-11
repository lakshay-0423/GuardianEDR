const express = require('express');

const userController = require('../controllers/user.controller');
const authenticate = require('../middleware/authenticate');

const router = express.Router();

router.use(authenticate);
router.get('/me', userController.getCurrentUser);

module.exports = router;
