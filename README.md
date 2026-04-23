# Terminal Operation System - Microservices Backend (.NET)

This repository contains a backend platform for a Terminal Operation System using a microservices architecture in .NET.

## Services

- **Auth Service** (`src/AuthService`)
  - Validates technical client credentials
  - Generates JWT token
  - Includes `sub`, `role`, and `scope` claims

- **Container Operations Service** (`src/ContainerOperationsService`)
  - Lists containers
  - Retrieves container detail by id
  - Changes operational status
  - Registers operational events

- **Yard Move Planning Service** (`src/YardMovePlanningService`)
  - Lists operational jobs
  - Returns pending tasks
  - Assigns jobs
  - Changes priority
  - Marks jobs as completed
  - Reschedules operations
  - Retrieves planning status

- **API Gateway (Ocelot)** (`src/ApiGateway`)
  - Unified API entrypoint
  - Routes external requests to each microservice

## Architecture and Technical Decisions

- Independent services with strict domain boundaries
- Controller-based APIs for readability and maintainability
- JWT authentication + scope-based authorization
- SQLite per service to keep data ownership explicit
- Health check endpoint (`/health`) in each service
- Dockerized services + local orchestration with Docker Compose

Architecture details are available in `docs/architecture.md`.

## Security

### JWT Claims
- `sub`: technical client identifier
- `role`: technical role (e.g., operator, viewer)
- `scope`: permissions consumed by authorization policies

### Scope Policies
- `containers.read`
- `containers.write`
- `yard.read`
- `yard.write`

## Local Run

### Prerequisites
- .NET 8 SDK
- Docker (optional, for Compose run)

### Option A - Run with Docker Compose
```bash
docker compose up --build
```

Services:
- Gateway: `http://localhost:5000`
- Auth: `http://localhost:5001`
- Container Operations: `http://localhost:5002`
- Yard Move Planning: `http://localhost:5003`

### Option B - Run services manually
Run each project independently with `dotnet run`.

## Main API Flow (Postman)

1. Call `POST {{gateway_url}}/auth/token`
2. Save returned `accessToken`
3. Call protected endpoints with `Authorization: Bearer {{access_token}}`

### Expected Security Behavior
- Missing token -> `401 Unauthorized`
- Invalid token -> `401 Unauthorized`
- Missing required scope -> `403 Forbidden`

## Postman Assets
- Collection: `postman/Terminal-Operation-System.postman_collection.json`
- Environment: `postman/Terminal-Operation-System.postman_environment.json`

## Testing

Projects:
- `tests/AuthService.IntegrationTests.csproj`
- `tests/ContainerOperationsService.UnitTests.csproj`
- `tests/YardMovePlanningService.UnitTests.csproj`

Run:
```bash
dotnet test tests/AuthService.IntegrationTests.csproj
dotnet test tests/ContainerOperationsService.UnitTests.csproj
dotnet test tests/YardMovePlanningService.UnitTests.csproj
```

## CI

GitHub Actions workflow: `.github/workflows/build-and-test.yml`

Pipeline stages:
1. Restore
2. Build
3. Test

## Render Deployment

The repository includes `render.yaml` as deployment blueprint.

After deployment, replace with real URLs:
- Auth Service URL: `https://<auth-service-url>`
- Container Operations Service URL: `https://<container-service-url>`
- Yard Move Planning Service URL: `https://<yard-service-url>`
- API Gateway URL: `https://<gateway-url>`
