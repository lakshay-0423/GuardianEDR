const express = require('express');

const eventController = require('../controllers/event.controller');
const authenticate = require('../middleware/authenticate');
const { validateQuery } = require('../middleware/validate');
const { eventLogQuerySchema } = require('../validations/event.validation');

const router = express.Router();

router.use(authenticate);
router.get('/', validateQuery(eventLogQuerySchema), eventController.listEventLogs);

module.exports = router;
