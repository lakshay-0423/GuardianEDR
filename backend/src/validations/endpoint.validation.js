const { z } = require('zod');

const endpointIdSchema = z.object({
  id: z.string().uuid(),
});

const endpointEventsQuerySchema = z.object({
  limit: z.coerce.number().int().min(1).max(100).default(50),
}).strict();

module.exports = { endpointEventsQuerySchema, endpointIdSchema };
