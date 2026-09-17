const { z } = require('zod');

const eventLogQuerySchema = z.object({
  endpointId: z.string().uuid().optional(),
  eventType: z.enum(['PROCESS_STARTED', 'PROCESS_TERMINATED']).optional(),
  limit: z.coerce.number().int().min(1).max(100).default(25),
  page: z.coerce.number().int().min(1).default(1),
}).strict();

module.exports = { eventLogQuerySchema };
