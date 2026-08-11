const express = require('express');

const endpointController = require('../controllers/endpoint.controller');
const authenticate = require('../middleware/authenticate');
const { validateParams } = require('../middleware/validate');
const { endpointIdSchema } = require('../validations/endpoint.validation');

const router = express.Router();

router.use(authenticate);
router.get('/', endpointController.listEndpoints);
router.get('/:id', validateParams(endpointIdSchema), endpointController.getEndpointById);

module.exports = router;
