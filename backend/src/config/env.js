const { z } = require('zod');

const environmentSchema = z.object({
  NODE_ENV: z.enum(['development', 'test', 'production']).default('development'),
  PORT: z.coerce.number().int().min(1).max(65535).default(3000),
  CORS_ORIGIN: z.string().url().default('http://localhost:3000'),
  DATABASE_URL: z.string().url().optional(),
  JWT_SECRET: z.string().min(32).optional(),
  JWT_ACCESS_SECRET: z.string().min(32).optional(),
  JWT_REFRESH_SECRET: z.string().min(32).optional(),
  JWT_ACCESS_EXPIRES_IN: z.string().regex(/^\d+[smhd]$/).default('15m'),
  JWT_REFRESH_EXPIRES_IN: z.string().regex(/^\d+[smhd]$/).default('7d'),
  BCRYPT_SALT_ROUNDS: z.coerce.number().int().min(10).max(14).default(12),
  AGENT_JWT_SECRET: z.string().min(32).optional(),
  AGENT_JWT_EXPIRES_IN: z.string().regex(/^\d+[smhd]$/).default('30d'),
  AGENT_HEARTBEAT_INTERVAL_SECONDS: z.coerce.number().int().min(30).max(86400).default(300),
  ENDPOINT_OFFLINE_THRESHOLD_SECONDS: z.coerce.number().int().min(1).max(604800).default(900),
  ENDPOINT_STATUS_CHECK_INTERVAL_SECONDS: z.coerce.number().int().min(1).max(3600).default(60),
}).superRefine((environment, context) => {
  if (!environment.JWT_ACCESS_SECRET && !environment.JWT_SECRET) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['JWT_ACCESS_SECRET'],
      message: 'JWT_ACCESS_SECRET or JWT_SECRET is required',
    });
  }

  if (!environment.JWT_REFRESH_SECRET && !environment.JWT_SECRET) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['JWT_REFRESH_SECRET'],
      message: 'JWT_REFRESH_SECRET or JWT_SECRET is required',
    });
  }
});

const parsedEnvironment = environmentSchema.safeParse(process.env);

if (!parsedEnvironment.success) {
  throw new Error(`Invalid environment configuration: ${parsedEnvironment.error.message}`);
}

module.exports = Object.freeze({
  ...parsedEnvironment.data,
  JWT_ACCESS_SECRET: parsedEnvironment.data.JWT_ACCESS_SECRET ?? parsedEnvironment.data.JWT_SECRET,
  JWT_REFRESH_SECRET: parsedEnvironment.data.JWT_REFRESH_SECRET ?? parsedEnvironment.data.JWT_SECRET,
});
