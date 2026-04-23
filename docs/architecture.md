# Terminal Operation System - Architecture Document

## 1. High-level Overview
The platform is implemented as four independent .NET services:

1. **Auth Service**
2. **Container Operations Service**
3. **Yard Move Planning Service**
4. **API Gateway (Ocelot)**

Each service has a dedicated bounded context, dedicated SQLite storage, and explicit responsibility boundaries.

## 2. Domain Boundaries

### Auth Service
- Client authentication (technical clients)
- JWT issuance with `sub`, `role`, and `scope` claims

### Container Operations Service
- Container listing and detail retrieval
- Operational status transitions
- Operational events registration (`inbound`, `outbound`, `hold`, `customs release`, `loaded`, `unloaded`)

### Yard Move Planning Service
- Yard move jobs listing and planning status
- Job assignment
- Priority changes
- Completion and rescheduling

## 3. Security Model
- JWT bearer authentication on protected services
- Authorization policies based on scope claims
- Examples:
  - `containers.read`, `containers.write`
  - `yard.read`, `yard.write`

## 4. Data and Persistence
Each microservice owns its own SQLite file:
- `auth.db`
- `container-operations.db`
- `yard-move-planning.db`

No shared table or cross-service DB dependency exists.

## 5. API Gateway
Gateway provides a unified entrypoint and routes requests to domain services.

- `/auth/*` -> Auth Service
- `/containers/*` -> Container Operations Service
- `/yard/*` -> Yard Move Planning Service

## 6. Error Handling
All services return HTTP status codes according to result type:
- `200/201/204` for success
- `400` for validation issues
- `401` for missing/invalid token
- `403` for insufficient scope
- `404` for missing resources

## 7. Testing Strategy
- Unit tests for domain rules
- Integration tests for token flow
- Build and test workflow in GitHub Actions

## 8. Deployment
The repository contains:
- Dockerfiles per service
- `docker-compose.yml` for local orchestrated execution
- `render.yaml` blueprint for Render deployment
