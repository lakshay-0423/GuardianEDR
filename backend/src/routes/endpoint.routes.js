const express = require('express');

const endpointController = require('../controllers/endpoint.controller');
const authenticate = require('../middleware/authenticate');
const { validateParams, validateQuery } = require('../middleware/validate');
const { endpointEventsQuerySchema, endpointIdSchema } = require('../validations/endpoint.validation');

const router = express.Router();

router.use(authenticate);
router.get('/', endpointController.listEndpoints);
router.get('/:id/events', validateParams(endpointIdSchema), validateQuery(endpointEventsQuerySchema), endpointController.listEndpointEvents);
router.get('/:id', validateParams(endpointIdSchema), endpointController.getEndpointById);

module.exports = router;
