const express = require('express');

const agentController = require('../controllers/agent.controller');
const validateBody = require('../middleware/validate');
const { agentRegisterSchema } = require('../validations/agent.validation');

const router = express.Router();

router.post('/register', validateBody(agentRegisterSchema), agentController.register);

module.exports = router;
