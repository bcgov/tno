require('dotenv').config();

const requiredEnv = ['BASE_URL', 'API_BASE_URL', 'ENV'];

requiredEnv.forEach((key) => {
  if (!process.env[key]) {
    throw new Error(`Missing required environment variable: ${key}`);
  }
});

const ENV_CONFIG = {
  env: process.env.ENV,
  baseURL: process.env.BASE_URL,
  apiBaseURL: process.env.API_BASE_URL,
  headless: process.env.HEADLESS === 'true',
  timeout: Number(process.env.TIMEOUT) || 60000,
  username: process.env.APP_USERNAME,
  password: process.env.APP_PASSWORD,
  sub_username: process.env.SUB_USERNAME,
  sub_password: process.env.SUB_PASSWORD
};

module.exports = ENV_CONFIG;