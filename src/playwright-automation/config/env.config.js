const { boolEnv, loadEnv, numberEnv, requireEnv } = require('../utils/env');

loadEnv();

const requiredEnv = ['EDITOR_URL', 'SUBSCRIBER_URL', 'ENV', 'APP_USERNAME', 'APP_PASSWORD'];
requireEnv(requiredEnv);

const ENV_CONFIG = {
  env: process.env.ENV,
  baseURL: process.env.EDITOR_URL,
  apiBaseURL: process.env.API_BASE_URL,
  headless: boolEnv('HEADLESS'),
  timeout: numberEnv('ACTION_TIMEOUT', 30000),
  username: process.env.APP_USERNAME,
  password: process.env.APP_PASSWORD,
  subscriberUsername: process.env.SUB_USERNAME,
  subscriberPassword: process.env.SUB_PASSWORD
};

module.exports = ENV_CONFIG;
