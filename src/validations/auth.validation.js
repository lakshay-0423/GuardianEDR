const { z } = require('zod');

const email = z.string().trim().email().max(320).transform((value) => value.toLowerCase());
const password = z.string()
  .min(12, 'Password must be at least 12 characters long')
  .max(128, 'Password must be at most 128 characters long')
  .regex(/[a-z]/, 'Password must contain a lowercase letter')
  .regex(/[A-Z]/, 'Password must contain an uppercase letter')
  .regex(/\d/, 'Password must contain a number');

const registerSchema = z.object({
  email,
  password,
}).strict();

const loginSchema = z.object({
  email,
  password: z.string().min(1).max(128),
}).strict();

const refreshTokenSchema = z.object({
  refreshToken: z.string().min(1).max(4096),
}).strict();

module.exports = {
  loginSchema,
  refreshTokenSchema,
  registerSchema,
};
