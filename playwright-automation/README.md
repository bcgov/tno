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

1. Install Node.js
2. Clone repository - git clone <your-repo-url>
3. Install Dependencies 
   1. Run:
         npm install
4. Install browsers:
   npx playwright install

## Environment Configuration

After completing the project setup, create a `.env` file in the root directory of the Playwright project.

### Step 1: Create `.env` File

Copy `.env.example` to `.env`, then fill in the real values.

### Step 2: Add the following environment variables

```
ENV=test
APP_NAME=qa
EDITOR_URL=https://test.editor.mmi.gov.bc.ca/
SUBSCRIBER_URL=https://test.mmi.gov.bc.ca/
API_BASE_URL=
APP_USERNAME=<uName>
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
## Update password

Update your user credentials in the `.env` file at the Playwright project root. Please add the variables for both Editor and Subscriber users.

```
APP_USERNAME=<editor_username>
APP_PASSWORD=<editor_password>
SUB_USERNAME=<subscriber_username>
SUB_PASSWORD=<subscriber_password>
```

## Run Test case
1. UI test for single file    :  npm run test -- tests/ui/login.spec.js
2. API test for single file   :  npm run test -- tests/api/apiExample.spec.js
3. Run All Tests              :  npm run test

## View Report
4. Generate report            :  npm run report
