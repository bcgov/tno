# Today's News Online (TNO)

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/bcgov/tno/issues)
[![Lifecycle:Experimental](https://img.shields.io/badge/Lifecycle-Experimental-339999)](https://github.com/bcgov/tno/wiki)
[![Maintained](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://github.com/bcgov/tno/wiki)
[![codecov](https://codecov.io/gh/bcgov/tno/branch/dev/graph/badge.svg)](https://codecov.io/gh/bcgov/tno)

[![Forks](https://img.shields.io/github/forks/bcgov/tno.svg)](https://github.com/bcgov/tno/network/members)
[![Stars](https://img.shields.io/github/stars/bcgov/tno.svg)](https://github.com/bcgov/tno/stargazers)
[![Wachters](https://img.shields.io/github/watchers/bcgov/tno.svg)](https://github.com/bcgov/tno/watchers)

**dev:** [![React Application](https://github.com/bcgov/tno/actions/workflows/react-app.yml/badge.svg?branch=dev)](https://github.com/bcgov/tno/actions/workflows/react-app.yml)

**master:** [![React Application](https://github.com/bcgov/tno/actions/workflows/react-app.yml/badge.svg?branch=master)](https://github.com/bcgov/tno/actions/workflows/react-app.yml)

GCPE’s primary role is to inform the public about government programs, services, policies, and priorities through traditional communication practices and, increasingly, through direct engagement and online services.

One of GCPE’s featured services to clients is Today’s News Online Media Monitoring. Through this application, GCPE tracks breaking news stories in British Columbia, Canada, and around the world that impact our province.

The TNO application provides a variety of key services to senior executives throughout government including near-live issue alerts, morning and evening summary reports, as well as in-depth media analysis of key government initiatives and issues.

The application also provides government staff with direct access to all events held in the Legislature Press Theatre including Dr. Bonnie Henry's weekly press conferences on the COVID-19 outbreak.

Today's News Online (TNO) is a news aggregation system that takes in news sources of varying types and provides a single location for clients to access the data.

## Introduction

This repository is a mono-repo containing all relevant documentation, code, and infrastructure that is required to setup and run the TNO solution.

### Frontend Dependencies

[![APP (React)](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://github.com/bcgov/tno/wiki)
[![Redux](https://img.shields.io/badge/Redux-593D88?style=for-the-badge&logo=redux&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Javascript](https://img.shields.io/badge/JavaScript-323330?style=for-the-badge&logo=javascript&logoColor=F7DF1E)](https://github.com/bcgov/tno/wiki)
[![Typescript](https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Node](https://img.shields.io/badge/Node.js-43853D?style=for-the-badge&logo=node.js&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Yarn](https://img.shields.io/badge/yarn-43853D?style=for-the-badge&logo=node.js&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Styled Components](https://img.shields.io/badge/styled%20components-CC6699?style=for-the-badge&logo=sass&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![SAAS](https://img.shields.io/badge/Sass-CC6699?style=for-the-badge&logo=sass&logoColor=white)](https://github.com/bcgov/tno/wiki)

### Backend Dependencies

[![.NET](https://img.shields.io/badge/.NET-0F1BF0?style=for-the-badge&logo=csharp&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Java](https://img.shields.io/badge/Java-ED8B00?style=for-the-badge&logo=java&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![SPRING](https://img.shields.io/badge/Spring-6DB33F?style=for-the-badge&logo=spring&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Elasticsearch](https://img.shields.io/badge/Elasticsearch-4EA94B?style=for-the-badge&logo=elastic&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Kafka](https://img.shields.io/badge/Apache%20Kafka-20232A?style=for-the-badge&logo=apachekafka&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Keycloak](https://img.shields.io/badge/Keycloak-593D88?style=for-the-badge&logo=keycloak&logoColor=white)](https://github.com/bcgov/tno/wiki)

### Infrastructure

[![Docker](https://img.shields.io/badge/Docker-593D88?style=for-the-badge&logo=docker&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-E34F26?style=for-the-badge&logo=kubernetes&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Openshift](https://img.shields.io/badge/Openshift-CC6699?style=for-the-badge&logo=redhatopenshift&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Linux](https://img.shields.io/badge/Linux-6DB33F?style=for-the-badge&logo=linux&logoColor=white)](https://github.com/bcgov/tno/wiki)
[![Azure](https://img.shields.io/badge/Azure-007ACC?style=for-the-badge&logo=MicrosoftAzure&logoColor=white)](https://github.com/bcgov/tno/wiki)

## Links

- [Begin Development](docs/DEVELOPMENT.md) : Get started and help contribute to the project
- [Wiki](https://github.com/bcgov/tno/wiki) : Documentation for this repository
- [Openshift](openshift/README.md) : Infrastructure and DevOps
- [.NET tests](src/tests/README.md) : Automated tests and coverage
- [Playwright](src/playwright-automation/README.md) : Browser and API end-to-end tests
- [Postman](docs/POSTMAN.md) : Import the API collection and request an access token

## Repository layout and local environment migration

Application code and developer tools live in `src/` (`api`, `charts-api`, `app`, `libs`, `services`,
`tests`, `playwright-automation`, `tools`). Supporting infrastructure lives directly in `dependencies/` (the former
contents of `db/`, `auth/`, and `network/`). Deployment manifests remain in `openshift/`.

After pulling the folder restructure, run from the repository root:

```bash
bash do move
bash do setup
```

`bash do move` requires Python 3 and moves ignored `.env`, `.env.*`, and `*.env` files
from the old directories to their matching new locations, preserving contents and
permissions. API environments move from either `api/net/` or `src/api/net/` to
`src/api/`; charts API environments move from either `api/node/` or `src/api/node/`
to `src/charts-api/`. Library and service environments move from `libs/net/` or
`src/libs/net/` to `src/libs/`, and from `services/net/` or `src/services/net/` to
`src/services/`. The obsolete `src/libs/npm/` package tree has been removed and its
environments are not migrated. Kafka UI environments move directly to `dependencies/kafka-ui/`, including
from the intermediate `dependencies/kafka/kafka-ui/` location. Obsolete Kowl environments
are left at their old locations and are not used or migrated. It leaves the root `.env` in place and updates its `DB_CONTEXT` and
`ORACLE_DB_CONTEXT` overrides when they refer to the old `db/` paths. It skips package
and build directories. If a destination contains different settings, it stops before
moving any files: resolve that conflict manually and rerun. Identical copies are left
in place. Repeated runs are safe. `bash do setup` also runs this migration before creating
missing environment files.

Run frontend commands from `src/app` (`nvm use`, `npm ci`, `npm run dev`). The root
`TNO.sln`, Bash commands, and Compose configuration use the new paths. Recreate local
containers with `bash do docker up` to apply the updated bind mounts and environment paths.

Local development uses `bash do docker ...` or `bash do podman ...`; cluster operations
use `bash do oc ...` from this directory. Run `bash do <group> help` for all commands.
See [development setup](docs/DEVELOPMENT.md) for Linux, macOS, and Windows prerequisites.

Database tools run locally with `bash do db-*` or in an SDK container with
`bash do docker db-*` / `bash do podman db-*`. See [the full command reference](docs/DO.md).
