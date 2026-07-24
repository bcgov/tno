# Skill: AI Dependencies

## Primary AI Integration Points

- Shared AI client library: `libs/net/ai/`
  - Package refs include `Azure.AI.Projects` and `Azure.Identity`.
- Template/report AI usage: `libs/net/template/` via `TNO.AI` project reference.
- API AI endpoints/controllers include admin/subscriber LLM endpoints.
- Auto-clipper service includes AI-related settings and Azure identity usage.

## Configuration Keys (Common)

- `Azure__AI__ProjectEndpoint`
- `Azure__AI__ApiKey`
- `Azure__AI__DefaultModelDeploymentName`
- `Azure__AI__DefaultAgentName`
- `Azure__AI__DefaultSystemPrompt`
- `Azure__AI__DefaultUserPrompt`

## Related Speech/Media AI

- `Microsoft.CognitiveServices.Speech` is used in `services/net/auto-clipper`.
- Extract quotes service supports external LLM provider settings in env (primary/fallback models and URLs).

## Operational Guidance

- Keep API keys and model endpoints in local `.env` only; do not commit secrets.
- Use shared abstractions (`TNO.AI`, template services) instead of duplicating provider SDK calls.
- Validate behavior by building affected .NET projects and running targeted workflows.
