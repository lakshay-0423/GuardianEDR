const { z } = require('zod');

const endpointIdSchema = z.object({
  id: z.string().uuid(),
});

module.exports = { endpointIdSchema };
