const { PrismaClient } = require('@prisma/client');

const { emitAlertCreated } = require('./prismaEvents');

const globalForPrisma = global;

const createPrismaClient = () => new PrismaClient({
  log: process.env.NODE_ENV === 'development' ? ['warn', 'error'] : ['error'],
}).$extends({
  query: {
    alert: {
      create: async ({ args, query }) => {
        const alert = await query(args);
        emitAlertCreated(alert);
        return alert;
      },
    },
  },
});

const prisma = globalForPrisma.prisma ?? createPrismaClient();

if (process.env.NODE_ENV !== 'production') {
  globalForPrisma.prisma = prisma;
}

module.exports = prisma;
