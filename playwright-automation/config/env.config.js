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
  timeout: Number(process.env.TIMEOUT) || 60000
};

module.exports = ENV_CONFIG;