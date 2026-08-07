const express = require('express');

const agentController = require('../controllers/agent.controller');
const authenticateAgent = require('../middleware/authenticateAgent');
const validateBody = require('../middleware/validate');
const { agentHeartbeatSchema, agentRegisterSchema } = require('../validations/agent.validation');

const router = express.Router();

router.post('/register', validateBody(agentRegisterSchema), agentController.register);
router.post('/heartbeat', authenticateAgent, validateBody(agentHeartbeatSchema), agentController.heartbeat);

module.exports = router;
