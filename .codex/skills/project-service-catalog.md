# Skill: Project And Service Catalog

## Core Projects

- `app/editor`: React admin/editor app. Run with `cd app/editor && yarn start`; test with `yarn test`; build with `yarn build`. Depends on API + Keycloak.
- `app/subscriber`: React subscriber app. Run with `cd app/subscriber && yarn start`; test with `yarn test`; build with `yarn build`. Depends on API + Keycloak.
- `libs/npm/core` (`tno-core`): shared React components/types/hooks. Build with `cd libs/npm/core && yarn build`.
- `api/net`: primary .NET REST + SignalR API. Run with `cd api/net && dotnet run`; build with `dotnet build api/net/`. Depends on Postgres, Keycloak, Kafka, optional Elastic.
- `api/node`: charts API (Node/TS). Run with `cd api/node && yarn start:watch`; build with `yarn build`.
- `libs/net/*`: shared .NET packages (DAL, entities, services, keycloak, ai, template, etc.). Build all with `dotnet build libs/net/TNO.sln`.
- `db/postgres`, `db/elasticsearch`, `db/kafka`: storage/messaging bootstrap and migrations.
- `auth/keycloak`: local Keycloak realm import/config.

## .NET Service Catalog (`services/net`)

- `syndication`: ingest syndication feeds. Run `make up n=syndication`; build `dotnet build services/net/syndication/`; depends on `api`, `broker`.
- `fileupload`: upload pipeline service. Run `make up n=fileupload`; build `dotnet build services/net/fileupload/`; depends on `api`, `broker`.
- `image`: image ingest. Run `make up n=image`; build `dotnet build services/net/image/`; depends on `api`, `broker`, volume `tno-av-data`.
- `filemonitor`: file ingest monitor. Run `make up n=filemonitor`; build `dotnet build services/net/filemonitor/`; depends on `api`, `broker`, volume `tno-av-data`.
- `content`: content creation/processing. Run `make up n=content`; build `dotnet build services/net/content/`; depends on `api`, `broker`, volume `tno-av-data`.
- `contentmigration`: migration/import service. Run `make up n=contentmigration`; build `dotnet build services/net/contentmigration/`; depends on `api`, `broker`, volume `tno-av-data`.
- `indexing`: search indexing worker. Run `make up n=indexing`; build `dotnet build services/net/indexing/`; depends on `api`, `broker`, `elastic`.
- `transcription`: speech transcription worker. Run `make up n=transcription`; build `dotnet build services/net/transcription/`; depends on `api`, `broker`, volume `tno-api-data`.
- `auto-clipper`: clipping + AI/speech flow. Run `make up n=auto-clipper`; build `dotnet build services/net/auto-clipper/`; depends on `api`, `broker`, volume `tno-api-data`.
- `nlp`: NLP processing worker. Run `make up n=nlp`; build `dotnet build services/net/nlp/`; depends on `api`, `broker`.
- `notification`: notifications/emails. Run `make up n=notification`; build `dotnet build services/net/notification/`; depends on `api`, `broker`, volume `tno-api-data`.
- `reporting`: reporting generation worker. Run `make up n=reporting`; build `dotnet build services/net/reporting/`; depends on `api`, `broker`, volume `tno-av-data`.
- `folder-collection`: folder aggregation/sync tasks. Run `make up n=folder-collection`; build `dotnet build services/net/folder-collection/`; depends on `api`, `broker`.
- `extract-quotes`: quote extraction worker. Run `make up n=extract-quotes`; build `dotnet build services/net/extract-quotes/`; depends on `api`, `broker`.
- `ffmpeg`: media transformation worker. Run `make up n=ffmpeg`; build `dotnet build services/net/ffmpeg/`; depends on `api`, `broker`, volume `tno-api-data`.
- `scheduler`: scheduled jobs orchestrator. Run `make up n=scheduler`; build `dotnet build services/net/scheduler/`; depends on `api`, `broker`.
- `event-handler`: event-driven processing. Run `make up n=event-handler`; build `dotnet build services/net/event-handler/`; depends on `api`, `broker`.
- `automation`: morning automation service scaffold. Build `dotnet build services/net/automation/`; run manually with `cd services/net/automation && dotnet run`.
- `ches-retry`: CHES retry support service. Build `dotnet build services/net/ches-retry/`; run manually as needed.

## Service Testing Guidance

- First-level verification: `dotnet build` on changed project(s).
- API/service integration checks: run required containers and call health endpoints.
- Frontend checks: `yarn build` and targeted `yarn test` for modified app(s).
- Repo integration tests: see `test/README.md` (Postman/e2e flows).
