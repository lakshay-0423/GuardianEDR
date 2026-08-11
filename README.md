# Guardian EDR

Guardian EDR is an endpoint detection and response platform with a Node.js API, a React dashboard, and a Windows endpoint agent.

## Project layout

- `backend/` — Express API, Prisma schema, PostgreSQL migrations, and WebSocket server.
- `frontend/` — Vite and React dashboard.
- `agent/` — Windows .NET endpoint agent foundation.

## Local development

1. Configure the API environment by copying `backend/.env.example` to `backend/.env` and supplying the required PostgreSQL and JWT values.
2. Start the backend:

   ```bash
   cd backend
   npm install
   npm run prisma:migrate
   npm run dev
   ```

3. In another terminal, start the dashboard:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

The dashboard development server runs on port 5173 and proxies API and WebSocket requests to the backend on port 3000.

4. Build and run the Windows agent:

   ```bash
   cd agent
   dotnet restore
   dotnet build
   dotnet run
   ```

Agent settings are in `agent/appsettings.json`. Environment variables prefixed with
`GUARDIAN_AGENT_` override settings, for example `GUARDIAN_AGENT_Agent__ServiceName`.
