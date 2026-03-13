const base = require('@playwright/test');
import { MasterFixture } from './master-fixture';


// Extend base test by providing pages as required
// This extended  "test" can be used in multiple test files, and each of them will get the fixtures.
exports.test = base.test.extend({
  masterFixture: [
    async ({ browser }, use) => {
      const page = await browser.newPage();
      await use(new MasterFixture(page));
    },
    { scope: 'worker' },
  ],

  // Add `browserName` to the fixture
  browserName: async ({ browserName }, use) => {
    await use(browserName);
  },
});
export { expect } from '@playwright/test';
