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

## Run Test case
1. UI test for single file    :  npm run test -- tests/ui/login.spec.js
2. API test for single file   :  npm run test -- tests/ui/reqres.spec.js
3. Run All Tests              :  npm run test

## View Report
4. Generate report            :  npm run report