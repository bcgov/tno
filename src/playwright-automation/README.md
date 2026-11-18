# Playwright JavaScript Automation Framework

##  Overview
This framework is built using Playwright with JavaScript following OOP and POM design.

## Folder Structure
- api  → Connecting to API client and common api methods
- fixtures -> Master, UI and API fixtures
- pages → Page Object Model classes
- test-data -> App test data files
- tests → Test cases
- tmp -> To store browser session state
- utils → Reusable utilities

## Setup Instructions

Run from the repository root (use Node.js 22, matching CI and the dependency engine requirements):

```bash
cd src/playwright-automation
npm ci
npx playwright install chromium
cp .env.example .env
```

Run all commands below from `src/playwright-automation`. After pulling the repository
restructure, `make move` at the repository root migrates an existing
`playwright-automation/.env` to `src/playwright-automation/.env` without overwriting
other settings. The manual GitHub Actions workflow uses this directory for dependency
installation, test execution, and Allure report artifacts.

## Environment Configuration

After completing the project setup, create a `.env` file in the root directory of the Playwright project.

### Step 1: Create `.env` File

Copy `.env.example` to `.env`, then fill in the values for your local Docker stack.
Start it from the repository root with `make up p=main`. The example defaults to
the local nginx gateways on ports 40080 (editor) and 40081 (subscriber).

### Step 2: Add the following environment variables

```
ENV=test
APP_NAME=qa
LOCAL_ONLY=true
EDITOR_URL=http://localhost:40080/
SUBSCRIBER_URL=http://localhost:40081/
API_BASE_URL=http://localhost:40080/api
APP_USERNAME=<editor_other_username>
APP_PASSWORD=<password>
SUB_USERNAME=<uName>
SUB_PASSWORD=<password>
SUB_USERNAME1=<other_subscriber_username>
SUB_PASSWORD1=<other_subscriber_password>
HEADLESS=false
TEST_TIMEOUT=300000
ACTION_TIMEOUT=30000
CI=false
MAIL_USER=<sender_Email_Id>
MAIL_PASSWORD=<sender_password_not_plain_text>
MAIL_TO=<receipient_Email_Id>
```
`ENV=test` selects the existing fixture dataset, not the deployment target.
`EDITOR_URL` and `SUBSCRIBER_URL` override the origins in login fixtures and page
navigation. `LOCAL_ONLY=true` requires loopback application URLs and blocks external
browser HTTP/WebSocket requests, including authentication redirects. Local accounts
use **Local**; shared environments use **Other**. Seeded local credentials are
`editor` / `password` and `subscriber` / `password`. Some tests need administrative
permissions, a distinct second subscriber, and seeded reports/content; a fresh local
database may not have those records. Leave `MAIL_TO` blank to skip preview-email tests.

The local realm import supplies Display Name, IDIR and IDIR GUID for seeded accounts.
Older existing realms may still be missing them. If global setup reports that Keycloak
requires account profile completion, follow the [local account repair instructions](../../dependencies/keycloak/README.md#local-browser-test-accounts)
before rerunning. A setup failure means the browser scenarios did not run; it is not
a passing suite.

Only target a shared environment when explicitly authorized: set its URLs and
credentials and disable `LOCAL_ONLY`. The GitHub `Playwright E2E (Test Env)` workflow
is manual (`workflow_dispatch`) and targets the shared TEST environment using its
GitHub Environment variables/secrets; it does not start a local Compose stack or run
automatically on PRs. Its test step currently uses `continue-on-error`, so inspect the
uploaded Allure report even if the workflow is green.

## Update password

Update your user credentials in the `.env` file at the Playwright project root.

```
APP_USERNAME=<editor_other_username>
APP_PASSWORD=<editor_other_password>
SUB_USERNAME=<subscriber_username>
SUB_PASSWORD=<subscriber_password>
```

## Run Test case
1. UI test for single file    :  npm run test -- tests/ui/smoke/login.spec.js
2. Configuration unit tests   :  npm run test:unit
3. Run all browser tests      :  npm run test

## View Report
4. Generate report            :  npm run report
