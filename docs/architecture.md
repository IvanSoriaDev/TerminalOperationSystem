# Terminal Operation System - Architecture Document

## 1. High-level Overview

The platform is implemented as four independent .NET services:

1. **Auth Service**
2. **Container Operations Service**
3. **Yard Move Planning Service**
4. **API Gateway (Ocelot)**

Each domain service owns its API surface, persistence model, seed data, and authorization rules. The gateway handles routing and exposes the aggregated Swagger UI. Business logic stays inside the domain services.

## 2. Domain Boundaries

### Auth Service

- Owns technical client authentication
- Persists client credentials in `auth.db`
- Issues JWTs with `sub`, `role`, and `scope` claims
- Does not require a token for `POST /api/auth/token`

Seeded clients:

- `terminal-web-client`: operator client with read/write scopes
- `yard-readonly-client`: readonly client with read scopes

### Container Operations Service

- Owns container data and operational events
- Persists data in `container-operations.db`
- Supports:
  - container listing
  - container detail
  - status change
  - event registration
- Enforces:
  - `containers.read`
  - `containers.write`

Valid operational statuses:

```text
inbound
outbound
hold
customs-release
loaded
unloaded
```

### Yard Move Planning Service

- Owns yard move job data and planning state
- Persists data in `yard-move-planning.db`
- Supports:
  - job listing
  - pending task listing
  - assignment
  - priority update
  - completion
  - rescheduling
  - planning status
- Enforces:
  - `yard.read`
  - `yard.write`

## 3. Security Model

Protected services use JWT Bearer authentication and scope-based authorization policies.

Expected behavior:

```text
Missing token          -> 401 Unauthorized
Invalid token          -> 401 Unauthorized
Insufficient scope     -> 403 Forbidden
```

The `role` claim is included for future role-based authorization, while current endpoint authorization is scope-driven.

## 4. Data and Persistence

Each microservice owns its SQLite database:

```text
auth.db
container-operations.db
yard-move-planning.db
```

There is no shared database and no cross-service table dependency. This keeps ownership explicit and allows each service to evolve independently.

## 5. API Gateway

The gateway provides a unified entrypoint:

```text
/auth/*        -> Auth Service
/containers/*  -> Container Operations Service
/yard/*        -> Yard Move Planning Service
```

Local Docker Compose routing uses service hostnames:

```text
auth-service
container-operations-service
yard-move-planning-service
```

For public deployment, the gateway can override Ocelot settings through environment variables. This allows the same image to route to public or private service URLs depending on the deployment environment.

## 6. API Documentation

Each service exposes Swagger/OpenAPI in `Development`.

The gateway exposes aggregated Swagger:

```text
http://localhost:5001/swagger/index.html
```

`Container Operations` and `Yard Move Planning` include a Bearer security scheme so evaluators can paste the JWT into Swagger UI and call protected endpoints.

## 7. Error Handling

Controllers return HTTP status codes according to result type:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Validation errors and not-found responses use `ProblemDetails` where appropriate.

## 8. Testing Strategy

The test suite covers the main behaviors of the system:

- Auth integration tests for valid and invalid token requests
- Domain unit tests for container status rules
- Domain unit tests for yard priority/completion rules
- Protected endpoint tests for missing token, valid scoped token, and insufficient scope

CI runs restore, build, and tests through GitHub Actions.

## 9. Deployment

The repository contains:

- Dockerfile per service
- `docker-compose.yml` for local orchestration
- `render.yaml` as a Render blueprint starter
- environment-variable override support for gateway routing

For public deployment, configure the gateway with the deployed hosts for Auth, Container Operations, and Yard Move Planning, then update the Postman `gateway_url` variable with the public gateway URL.
