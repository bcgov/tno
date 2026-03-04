const base = require('@playwright/test');
const APIClient = require('../api/APIClient');

exports.test = base.test.extend({

  apiClient: async ({}, use) => {
    const client = new APIClient(process.env.API_BASE_URL);
    await client.createContext();
    await use(client);
    await client.dispose();
  }

});

exports.expect = base.expect;