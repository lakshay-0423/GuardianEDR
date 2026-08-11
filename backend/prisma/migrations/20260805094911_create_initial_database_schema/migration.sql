-- CreateEnum
CREATE TYPE "EndpointStatus" AS ENUM ('ONLINE', 'OFFLINE', 'ISOLATED');

-- CreateEnum
CREATE TYPE "AlertSeverity" AS ENUM ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL');

-- CreateEnum
CREATE TYPE "AlertStatus" AS ENUM ('OPEN', 'ACKNOWLEDGED', 'RESOLVED', 'DISMISSED');

-- CreateTable
CREATE TABLE "User" (
    "id" UUID NOT NULL,
    "email" VARCHAR(320) NOT NULL,
    "passwordHash" VARCHAR(255) NOT NULL,
    "createdAt" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMPTZ(3) NOT NULL,

    CONSTRAINT "User_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Endpoint" (
    "id" UUID NOT NULL,
    "ownerId" UUID NOT NULL,
    "agentId" VARCHAR(255) NOT NULL,
    "hostname" VARCHAR(255) NOT NULL,
    "ipAddress" VARCHAR(45),
    "osName" VARCHAR(100),
    "status" "EndpointStatus" NOT NULL DEFAULT 'OFFLINE',
    "lastSeenAt" TIMESTAMPTZ(3),
    "createdAt" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMPTZ(3) NOT NULL,

    CONSTRAINT "Endpoint_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Alert" (
    "id" UUID NOT NULL,
    "endpointId" UUID NOT NULL,
    "sourceEventId" UUID,
    "type" VARCHAR(100) NOT NULL,
    "title" VARCHAR(255) NOT NULL,
    "description" TEXT,
    "severity" "AlertSeverity" NOT NULL,
    "status" "AlertStatus" NOT NULL DEFAULT 'OPEN',
    "detectedAt" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "resolvedAt" TIMESTAMPTZ(3),
    "createdAt" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMPTZ(3) NOT NULL,

    CONSTRAINT "Alert_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Event" (
    "id" UUID NOT NULL,
    "endpointId" UUID NOT NULL,
    "eventType" VARCHAR(100) NOT NULL,
    "payload" JSONB NOT NULL,
    "occurredAt" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "createdAt" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "Event_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "User_email_key" ON "User"("email");

-- CreateIndex
CREATE INDEX "User_createdAt_idx" ON "User"("createdAt");

-- CreateIndex
CREATE UNIQUE INDEX "Endpoint_agentId_key" ON "Endpoint"("agentId");

-- CreateIndex
CREATE INDEX "Endpoint_status_lastSeenAt_idx" ON "Endpoint"("status", "lastSeenAt");

-- CreateIndex
CREATE INDEX "Endpoint_ownerId_idx" ON "Endpoint"("ownerId");

-- CreateIndex
CREATE UNIQUE INDEX "Endpoint_ownerId_hostname_key" ON "Endpoint"("ownerId", "hostname");

-- CreateIndex
CREATE INDEX "Alert_endpointId_status_detectedAt_idx" ON "Alert"("endpointId", "status", "detectedAt");

-- CreateIndex
CREATE INDEX "Alert_status_severity_detectedAt_idx" ON "Alert"("status", "severity", "detectedAt");

-- CreateIndex
CREATE INDEX "Alert_sourceEventId_idx" ON "Alert"("sourceEventId");

-- CreateIndex
CREATE INDEX "Event_endpointId_occurredAt_idx" ON "Event"("endpointId", "occurredAt");

-- CreateIndex
CREATE INDEX "Event_eventType_occurredAt_idx" ON "Event"("eventType", "occurredAt");

-- AddForeignKey
ALTER TABLE "Endpoint" ADD CONSTRAINT "Endpoint_ownerId_fkey" FOREIGN KEY ("ownerId") REFERENCES "User"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Alert" ADD CONSTRAINT "Alert_endpointId_fkey" FOREIGN KEY ("endpointId") REFERENCES "Endpoint"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Alert" ADD CONSTRAINT "Alert_sourceEventId_fkey" FOREIGN KEY ("sourceEventId") REFERENCES "Event"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Event" ADD CONSTRAINT "Event_endpointId_fkey" FOREIGN KEY ("endpointId") REFERENCES "Endpoint"("id") ON DELETE CASCADE ON UPDATE CASCADE;
