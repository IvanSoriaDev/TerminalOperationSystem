# Terminal Operation System - Microservices Backend (.NET)

Backend platform for a Terminal Operation System implemented with ASP.NET Core Web API, JWT security, SQLite persistence, Docker Compose, Swagger/OpenAPI, Postman assets, and an Ocelot API Gateway.

## Services

- **Auth Service** (`src/AuthService`)
  - Validates mock/persisted technical clients
  - Generates JWT access tokens
  - Includes `sub`, `role`, and `scope` claims

- **Container Operations Service** (`src/ContainerOperationsService`)
  - Lists containers
  - Retrieves container detail by id
  - Changes operational status
  - Registers operational events
  - Requires `containers.read` or `containers.write`

- **Yard Move Planning Service** (`src/YardMovePlanningService`)
  - Lists yard move jobs
  - Returns pending tasks
  - Assigns jobs
  - Changes priority
  - Marks jobs as completed
  - Reschedules operations
  - Retrieves planning status
  - Requires `yard.read` or `yard.write`

- **API Gateway (Ocelot)** (`src/ApiGateway`)
  - Unified entrypoint for Postman and Swagger testing
  - Routes `/auth/*`, `/containers/*`, and `/yard/*`
  - Aggregates downstream Swagger documents

Architecture details are available in `docs/architecture.md`.

## Local Run

### Prerequisites

- Docker Desktop
- Optional: .NET 8 SDK if running services/tests without Docker

### Run with Docker Compose

```bash
docker compose up -d --build
```

Local URLs:

```text
API Gateway:           http://localhost:5001
Auth Service:          http://localhost:5004
Container Operations:  http://localhost:5002
Yard Move Planning:    http://localhost:5003
```

Swagger URLs:

```text
Gateway Swagger:       http://localhost:5001/swagger/index.html
Auth Swagger:          http://localhost:5004/swagger/index.html
Container Swagger:     http://localhost:5002/swagger/index.html
Yard Swagger:          http://localhost:5003/swagger/index.html
```

Stop services:

```bash
docker compose down
```

## Demo Credentials

Operator client, full access:

```json
{
  "clientId": "terminal-web-client",
  "clientSecret": "change-me-in-production"
}
```

Scopes:

```text
containers.read containers.write yard.read yard.write
```

Readonly client:

```json
{
  "clientId": "yard-readonly-client",
  "clientSecret": "readonly-secret"
}
```

Scopes:

```text
yard.read containers.read
```

## Main API Flow

Request a token through the gateway:

```bash
curl -X POST http://localhost:5001/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId":"terminal-web-client","clientSecret":"change-me-in-production"}'
```

Use the returned `accessToken`:

```bash
curl http://localhost:5001/containers \
  -H "Authorization: Bearer <access_token>"
```

Expected security behavior:

```text
Missing token              -> 401 Unauthorized
Invalid token              -> 401 Unauthorized
Missing required scope     -> 403 Forbidden
Invalid request data       -> 400 Bad Request
Missing resource           -> 404 Not Found
```

## Postman

Files:

```text
postman/Terminal-Operation-System.postman_collection.json
postman/Terminal-Operation-System.postman_environment.json
```

### Public Render Execution

Public deployment URLs:

```text
API Gateway:                 https://tos-api-gateway.onrender.com
Gateway Swagger:             https://tos-api-gateway.onrender.com/swagger/index.html
Auth Service Health:         https://tos-auth-service.onrender.com/health
Container Service Health:    https://tos-container-operations-service.onrender.com/health
Yard Service Health:         https://tos-yard-move-planning-service.onrender.com/health
```

Important note for evaluators and Postman users:

```text
The public services run on Render Free.
Render Free web services can go to sleep after inactivity.
Before opening Swagger or calling a protected endpoint through the API Gateway,
first call the health endpoints to wake up the downstream services.
```

Recommended public execution order:

1. Import the collection and environment.
2. Confirm `gateway_url` is `https://tos-api-gateway.onrender.com`.
3. Run the `Render Free Warm Up` folder.
4. Open `https://tos-api-gateway.onrender.com/swagger/index.html` if you want to use Swagger.
5. Run `Auth / 1 - Get JWT Token - Operator`.
6. Run `Auth / 2 - Get JWT Token - Readonly`.
7. Run the `Container Operations` folder.
8. Run the `Yard Move Planning` folder.

Recommended execution order:

1. Import the collection and environment.
2. Run the `Render Free Warm Up` folder if you are using the public Render deployment.
3. Run `Auth / 1 - Get JWT Token - Operator`.
4. Run `Auth / 2 - Get JWT Token - Readonly`.
5. Run the `Container Operations` folder.
6. Run the `Yard Move Planning` folder.

The collection automatically stores:

```text
access_token
readonly_token
container_id
yard_move_id
future_scheduled_at_utc
gateway_url
auth_service_url
container_service_url
yard_service_url
gateway_swagger_url
```

## Swagger Authorization

`Container Operations` and `Yard Move Planning` Swagger documents include JWT Bearer security. In Swagger UI:

If you are using the public Render deployment, wake up the services first:

1. `https://tos-auth-service.onrender.com/health`
2. `https://tos-container-operations-service.onrender.com/health`
3. `https://tos-yard-move-planning-service.onrender.com/health`
4. Open `https://tos-api-gateway.onrender.com/swagger/index.html`

Then:

1. Call `Auth Service - v1` -> `POST /auth/token`.
2. Copy the `accessToken`.
3. Switch to `Container Operations Service - v1` or `Yard Move Planning Service - v1`.
4. Click `Authorize`.
5. Paste only the token value. Swagger adds the `Bearer` scheme.

## Testing

Run all tests:

```bash
dotnet test TerminalOperationSystem.sln
```

Or run them individually:

```bash
dotnet test tests/AuthService.IntegrationTests.csproj
dotnet test tests/ContainerOperationsService.UnitTests.csproj
dotnet test tests/YardMovePlanningService.UnitTests.csproj
```

Test coverage includes:

- Auth integration flow for valid and invalid credentials
- Domain unit tests for container statuses
- Domain unit tests for yard priority/completion rules
- Protected endpoint tests for `401`, `403`, and valid scoped access

## CI

GitHub Actions workflow:

```text
.github/workflows/build-and-test.yml
```

Pipeline stages:

```text
restore -> build -> test
```

## Deployment

The repository includes `render.yaml` as a Render blueprint starter. The services can also be deployed to Railway, Azure App Service, or another container-compatible platform.

After public deployment, share these URLs:

```text
Auth Service URL:          https://<auth-service-url>
Container Service URL:     https://<container-service-url>
Yard Service URL:          https://<yard-service-url>
API Gateway URL:           https://<gateway-url>
```

For public gateway deployment, configure Ocelot with environment variables so it routes to the deployed services instead of Docker Compose hostnames. The gateway supports environment overrides because `Program.cs` loads environment variables after `ocelot.json`.

Useful gateway override examples:

```text
Routes__0__DownstreamScheme=https
Routes__0__DownstreamHostAndPorts__0__Host=<auth-service-host>
Routes__0__DownstreamHostAndPorts__0__Port=443
SwaggerEndPoints__0__Config__0__Url=https://<auth-service-host>/swagger/v1/swagger.json

Routes__1__DownstreamScheme=https
Routes__1__DownstreamHostAndPorts__0__Host=<container-service-host>
Routes__1__DownstreamHostAndPorts__0__Port=443
SwaggerEndPoints__1__Config__0__Url=https://<container-service-host>/swagger/v1/swagger.json

Routes__2__DownstreamScheme=https
Routes__2__DownstreamHostAndPorts__0__Host=<yard-service-host>
Routes__2__DownstreamHostAndPorts__0__Port=443
SwaggerEndPoints__2__Config__0__Url=https://<yard-service-host>/swagger/v1/swagger.json

GlobalConfiguration__BaseUrl=https://<gateway-host>
```

For Postman against the public deployment, update `gateway_url` in the Postman environment to the deployed API Gateway URL.

Current public deployment:

```text
gateway_url = https://tos-api-gateway.onrender.com
```

## Technical Decisions

- Separate ASP.NET Core projects per domain boundary
- SQLite database per service to avoid shared persistence
- Controller-based APIs for clear contracts and status codes
- JWT Bearer authentication with scope-based authorization policies
- Ocelot Gateway for a unified consumer entrypoint
- Swagger/OpenAPI for local and gateway-level API exploration
- Docker Compose for reproducible local orchestration
