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

After completing the project setup, you ust create a `.env.test` file in the root directory of the project

### Step 1: Create `.env.test` File

Create a file named : `.env.test` at the root level of the project.

### Step 2: Add the following environment variables

```
ENV=test
QA_URL=https://test.editor.mmi.gov.bc.ca/
API_QA_URL=https://test.editor.mmi.gov.bc.ca/
APP_USERNAME=<uName>
APP_PASSWORD=<password>
SUB_USERNAME=<uNmae>
SUB_PASSWORD=<password>
HEADLESS=true
TIMEOUT=60000
CI=false
MAIL_USER=<sender_Email_Id>
MAIL_PASSWORD=<sender_password_not_plain_text>
MAIL_TO=<receipient_Email_Id>
```
## Update password

Update your user credentials in `.env.test` file at root level. Please add belwo variables for user credentials (both Editor and Subscriber)

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