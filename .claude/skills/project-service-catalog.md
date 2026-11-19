# Skill: Project And Service Catalog

## Core Projects

- `app`: React + Vite web app with two designs, editor (`src/app/src/editor`, admin/editor users) and subscriber (`src/app/src/subscriber`), plus shared code in `src/app/src/shared`. Node 26 + npm: run with `cd src/app && APP_MODE=editor npm run dev` (or `bash do docker up -n editor` / `-n subscriber`); test with `npm test`; build with `npm run build`. One `app` image serves both; `APP_MODE` selects the design. Depends on API + Keycloak.
- `src/api`: primary .NET REST + SignalR API. Run with `cd src/api && dotnet run`; build with `dotnet build src/api/`. Depends on Postgres, Keycloak, Kafka, optional Elastic.
- `src/charts-api`: charts API (Node/TS). Run with `cd src/charts-api && yarn start:watch`; build with `yarn build`.
- `src/libs/*`: shared .NET packages (DAL, entities, services, keycloak, ai, template, etc.). Build all with `dotnet build src/libs/TNO.sln`.
- `dependencies/postgres`, `dependencies/elasticsearch`, `dependencies/kafka`: storage/messaging bootstrap and migrations.
- `dependencies/keycloak`: local Keycloak realm import/config.

## .NET Service Catalog (`src/services`)

- `syndication`: ingest syndication feeds. Run `bash do docker up -n syndication`; build `dotnet build src/services/syndication/`; depends on `api`, `broker`.
- `fileupload`: upload pipeline service. Run `bash do docker up -n fileupload`; build `dotnet build src/services/fileupload/`; depends on `api`, `broker`.
- `image`: image ingest. Run `bash do docker up -n image`; build `dotnet build src/services/image/`; depends on `api`, `broker`, volume `tno-av-data`.
- `filemonitor`: file ingest monitor. Run `bash do docker up -n filemonitor`; build `dotnet build src/services/filemonitor/`; depends on `api`, `broker`, volume `tno-av-data`.
- `content`: content creation/processing. Run `bash do docker up -n content`; build `dotnet build src/services/content/`; depends on `api`, `broker`, volume `tno-av-data`.
- `contentmigration`: migration/import service. Run `bash do docker up -n contentmigration`; build `dotnet build src/services/contentmigration/`; depends on `api`, `broker`, volume `tno-av-data`.
- `indexing`: search indexing worker. Run `bash do docker up -n indexing`; build `dotnet build src/services/indexing/`; depends on `api`, `broker`, `elastic`.
- `transcription`: speech transcription worker. Run `bash do docker up -n transcription`; build `dotnet build src/services/transcription/`; depends on `api`, `broker`, volume `tno-api-data`.
- `auto-clipper`: clipping + AI/speech flow. Run `bash do docker up -n auto-clipper`; build `dotnet build src/services/auto-clipper/`; depends on `api`, `broker`, volume `tno-api-data`.
- `notification`: notifications/emails. Run `bash do docker up -n notification`; build `dotnet build src/services/notification/`; depends on `api`, `broker`, volume `tno-api-data`.
- `reporting`: reporting generation worker. Run `bash do docker up -n reporting`; build `dotnet build src/services/reporting/`; depends on `api`, `broker`, volume `tno-av-data`.
- `folder-collection`: folder aggregation/sync tasks. Run `bash do docker up -n folder-collection`; build `dotnet build src/services/folder-collection/`; depends on `api`, `broker`.
- `content-analysis`: LLM content analysis worker (replaces the retired `extract-quotes` and `nlp` services). Consumes analysis requests (keyed by content ID) from the `analysis`, `analysis-retry`, and `analysis-backfill` Kafka topics, each with its own consumer, and sends exhausted requests to `analysis-dlq`; uses the LLM configured through the API. Run `bash do docker up -n content-analysis`; build `dotnet build src/services/content-analysis/TNO.Services.ContentAnalysis.csproj`; depends on `api`, `broker`.
- `ffmpeg`: media transformation worker. Run `bash do docker up -n ffmpeg`; build `dotnet build src/services/ffmpeg/`; depends on `api`, `broker`, volume `tno-api-data`.
- `scheduler`: scheduled jobs orchestrator. Run `bash do docker up -n scheduler`; build `dotnet build src/services/scheduler/`; depends on `api`, `broker`.
- `event-handler`: event-driven processing: event schedules (`event-schedule`) and work orders (`work-order`, e.g. Content-Analysis backfills and bulk topic rescores, a page at a time), each with its own consumer. Run `bash do docker up -n event-handler`; build `dotnet build src/services/event-handler/`; depends on `api`, `broker`.
- `automation`: AI automation engine — executes schema-v2 automation profiles (steps, analyses, actions, scoring, saves) and writes each run's decision log and outcome summary back through the API. Run `bash do docker up -n automation`; build `dotnet build src/services/automation/TNO.Services.Automation.csproj`; depends on `api`, `broker`, `elastic`, and an LLM configured on the profile.
- `ches-retry`: CHES retry support service. Build `dotnet build src/services/ches-retry/`; run manually as needed. **Not a compose service** — it has no entry in `src/services/docker-compose.yml`, so `bash do docker up -n ches-retry` will not work.

## Service Testing Guidance

- First-level verification: `dotnet build` on changed project(s).
- API/service integration checks: run required containers and call health endpoints.
- Frontend checks: in `src/app/`, `npm run typecheck`, `npm run lint`, `npm run build` and targeted `npm test`.
- Repo integration tests: see `docs/POSTMAN.md` (Postman/e2e flows).

## Seeing a Change in a Running Stack

A `dotnet build` proves the code compiles; it does **not** update the running container. Rebuild
what the change actually reaches — `bash do docker refresh -n <service>` for a service, `-n api` for `src/api`
**or any `src/libs/**` change the UIs read through the API**. Editor/subscriber `src/` changes need
no rebuild (bind-mounted, Vite hot-reloads). Full rules in the `docker-compose-services` skill.
