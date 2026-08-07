const { z } = require('zod');

const agentRegisterSchema = z.object({
  hostname: z.string().trim().min(1).max(255),
  operatingSystem: z.string().trim().min(1).max(100),
  architecture: z.string().trim().min(1).max(50),
  username: z.string().trim().min(1).max(255),
  agentVersion: z.string().trim().min(1).max(100),
  deviceUuid: z.string().uuid(),
}).strict();

module.exports = { agentRegisterSchema };
