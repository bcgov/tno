const base = require('@playwright/test');
const APIClient = require('../api/APIClient');
const { loadEnv, requireEnv } = require('../utils/env');

loadEnv();

exports.test = base.test.extend({

  apiClient: async ({}, use) => {
    requireEnv(['API_BASE_URL']);
    const client = new APIClient(process.env.API_BASE_URL);
    await client.createContext();
    await use(client);
    await client.dispose();
  }

});

exports.expect = base.expect;
