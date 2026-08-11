const { z } = require('zod');

const agentRegisterSchema = z.object({
  hostname: z.string().trim().min(1).max(255),
  operatingSystem: z.string().trim().min(1).max(100),
  architecture: z.string().trim().min(1).max(50),
  username: z.string().trim().min(1).max(255),
  agentVersion: z.string().trim().min(1).max(100),
  deviceUuid: z.string().uuid(),
}).strict();

const agentHeartbeatSchema = z.object({
  cpuUsage: z.number().min(0).max(100),
  memoryUsage: z.number().min(0).max(100),
  timestamp: z.coerce.date().refine(
    (value) => value <= new Date(Date.now() + (5 * 60 * 1000)),
    'Timestamp cannot be more than five minutes in the future',
  ),
}).strict();

module.exports = {
  agentHeartbeatSchema,
  agentRegisterSchema,
};
