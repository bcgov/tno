const ENV = require('./env.config');
const { boolEnv } = require('../utils/env');

const TEST_CONFIG = {
  baseURL: ENV.baseURL,
  timeout: ENV.timeout,
  retries: boolEnv('CI') ? 2 : 1,
  workers: boolEnv('CI') ? 4 : undefined,
  username: ENV.username,
  password: ENV.password,
  subscriberUsername: ENV.subscriberUsername,
  subscriberPassword: ENV.subscriberPassword
};

module.exports = TEST_CONFIG;
