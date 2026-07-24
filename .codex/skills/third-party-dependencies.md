# Skill: Third-Party Dependencies

## Platform And Infrastructure

- PostgreSQL (primary relational DB)
- Elasticsearch (search/indexing)
- Apache Kafka (event streaming)
- Keycloak (OIDC auth and role management)
- Docker + Docker Compose (local orchestration)
- OpenShift/Kubernetes (deployment)

## Cloud/External Integrations

- Azure AI Foundry / Azure OpenAI (`Azure.AI.Projects`, Azure credentials)
- Azure Cognitive Services Speech
- CHES email service (Gov BC)
- CSS identity/integration endpoints (where configured)
- Optional S3-compatible storage integrations

## Frontend/Node Ecosystem Highlights

- React 18, Redux Toolkit, Formik, Styled Components
- SignalR client (`@microsoft/signalr`)
- Keycloak JS adapters (`keycloak-js`, `@react-keycloak/web`)
- Chart stack (`chart.js`, plugin libraries)

## .NET Ecosystem Highlights

- ASP.NET Core, JWT bearer auth, SignalR
- Entity Framework Core (via DAL layer)
- Prometheus metrics integration
- Newtonsoft.Json + System.Text.Json usage in different layers

## Dependency Management Rules

- Use Yarn (not npm) in app and npm-library workspaces.
- Keep versions aligned with existing workspace `package.json` files.
- Prefer adding shared dependencies at library layer when reused across services.
- Rebuild impacted projects after dependency updates.
