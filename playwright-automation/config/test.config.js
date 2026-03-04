const ENV = require('./env.config');

const TEST_CONFIG = {
  baseURL: ENV.baseURL,
  timeout: ENV.timeout,
  retries: process.env.CI ? 2 : 1,
  workers: process.env.CI ? 4 : undefined,
  username: ENV.username,
  password: ENV.password,
  sub_username: ENV.sub_username,
  sub_password: ENV.sub_password
};

module.exports = TEST_CONFIG;