-- AlterTable
ALTER TABLE "Endpoint" ADD COLUMN     "agentVersion" VARCHAR(100) NOT NULL,
ADD COLUMN     "architecture" VARCHAR(50) NOT NULL,
ADD COLUMN     "deviceUuid" UUID NOT NULL,
ADD COLUMN     "username" VARCHAR(255) NOT NULL,
ALTER COLUMN "ownerId" DROP NOT NULL,
ALTER COLUMN "osName" SET NOT NULL;

-- CreateIndex
CREATE UNIQUE INDEX "Endpoint_deviceUuid_key" ON "Endpoint"("deviceUuid");
