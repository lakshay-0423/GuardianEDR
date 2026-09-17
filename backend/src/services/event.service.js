const prisma = require('../config/prisma');
const { getDashboardEndpointFilter } = require('./endpointAccess.service');

const toProcessDetails = (payload) => ({
  commandLine: typeof payload?.commandLine === 'string' ? payload.commandLine : null,
  executablePath: typeof payload?.executablePath === 'string' ? payload.executablePath : null,
  id: typeof payload?.processId === 'number' ? payload.processId : null,
  name: typeof payload?.processName === 'string' ? payload.processName : null,
  parentId: typeof payload?.parentProcessId === 'number' ? payload.parentProcessId : null,
  parentName: typeof payload?.parentProcessName === 'string' ? payload.parentProcessName : null,
  username: typeof payload?.username === 'string' ? payload.username : null,
});

const toEventLog = (event) => ({
  endpoint: {
    hostname: event.endpoint.hostname,
    id: event.endpoint.id,
  },
  eventType: event.eventType,
  id: event.id,
  process: toProcessDetails(event.payload),
  timestamp: event.occurredAt,
});

const listEventLogs = async (userId, {
  endpointId,
  eventType,
  limit,
  page,
}) => {
  const endpointFilter = {
    ...getDashboardEndpointFilter(userId),
    ...(endpointId ? { id: endpointId } : {}),
  };
  const where = {
    ...(eventType ? { eventType } : {}),
    endpoint: { is: endpointFilter },
  };
  const skip = (page - 1) * limit;

  const [total, events] = await prisma.$transaction([
    prisma.event.count({ where }),
    prisma.event.findMany({
      where,
      select: {
        endpoint: {
          select: {
            hostname: true,
            id: true,
          },
        },
        eventType: true,
        id: true,
        occurredAt: true,
        payload: true,
      },
      orderBy: [
        { occurredAt: 'desc' },
        { id: 'desc' },
      ],
      skip,
      take: limit,
    }),
  ]);

  return {
    events: events.map(toEventLog),
    pagination: {
      limit,
      page,
      total,
      totalPages: Math.ceil(total / limit),
    },
  };
};

module.exports = { listEventLogs };
