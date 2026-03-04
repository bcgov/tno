# Playwright JavaScript Automation Framework

##  Overview
This framework is built using Playwright with JavaScript following OOP and POM design.

## 📂 Folder Structure
- api  → Connecting to API client and common api methods
- pages → Page Object Model classes
- tests → Test cases
- fixtures → Custom fixtures
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

After completing the project setup, you ust create a `.env` file in the root directory of the project

### Step 1: Create .env File

Create a file named : .env at the root level of the project.

### Step 2: Add the following environment variables

```
ENV=qa
BASE_URL=https://test.editor.mmi.gov.bc.ca/
API_BASE_URL=<base url>
HEADLESS=true
TIMEOUT=60000
CI=false
```
## Update password

Update your user credentials in `.env` file at root level. Please add belwo variables for user credentials (both Editor and Subscriber)

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