const base = require('@playwright/test');
const { restrictToLocal } = require('../utils/local-only');
import { MasterFixture } from './master-fixture';


// Extend base test by providing pages as required
// This extended  "test" can be used in multiple test files, and each of them will get the fixtures.
exports.test = base.test.extend({
  context: async ({ context }, use) => {
    await restrictToLocal(context);
    await use(context);
  },
  masterFixture: [
    async ({ browser }, use) => {
      const page = await browser.newPage({ serviceWorkers: 'block' });
      await restrictToLocal(page.context());
      try {
        await use(new MasterFixture(page));
      } finally {
        await page.context().close();
      }
    },
    { scope: 'worker' },
  ],

  // Add `browserName` to the fixture
  browserName: async ({ browserName }, use) => {
    await use(browserName);
  },
});
export { expect } from '@playwright/test';
