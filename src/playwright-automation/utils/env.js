const path = require('path');
const dotenv = require('dotenv');

const envPath = path.resolve(__dirname, '..', '.env');

function loadEnv() {
  dotenv.config({ path: envPath });

  process.env.ENV_NAME = process.env.ENV_NAME || process.env.ENV || 'test';
  process.env.APP_NAME = process.env.APP_NAME || 'qa';
  process.env.LOGIN_URL = process.env.LOGIN_URL || process.env.EDITOR_URL;
}

function requireEnv(keys) {
  const missingKeys = keys.filter((key) => !process.env[key]);

  if (missingKeys.length > 0) {
    throw new Error(`Missing required environment variable(s): ${missingKeys.join(', ')}`);
  }
}

function boolEnv(key, fallback = false) {
  const value = process.env[key];

  if (value === undefined) {
    return fallback;
  }

  return value.toLowerCase() === 'true';
}

function numberEnv(key, fallback) {
  const value = Number(process.env[key]);

  return Number.isFinite(value) && value > 0 ? value : fallback;
}

module.exports = {
  boolEnv,
  loadEnv,
  numberEnv,
  requireEnv,
};
