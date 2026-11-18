const assert = require('node:assert/strict');
const test = require('node:test');
const { isLoopback, validateLocalTargets } = require('../utils/local-only');
const DataLoader = require('../utils/dataLoader');

function localEnvironment(t) {
  const original = { ...process.env };
  t.after(() => {
    for (const key of Object.keys(process.env)) if (!(key in original)) delete process.env[key];
    Object.assign(process.env, original);
  });
  Object.assign(process.env, {
    LOCAL_ONLY: 'true',
    EDITOR_URL: 'http://localhost:40080',
    SUBSCRIBER_URL: 'http://localhost:40081',
    API_BASE_URL: 'http://localhost:40080/api',
    LOGIN_URL: 'http://localhost:40080',
  });
}

test('local targets exclude remote hosts and misleading localhost prefixes', () => {
  for (const url of ['http://localhost:40080', 'http://127.0.0.1:40001', 'ws://[::1]:3000']) {
    assert.equal(isLoopback(url), true);
  }
  for (const url of ['https://test.editor.mmi.gov.bc.ca', 'http://localhost.example.com', 'http://localhost@remote.example', 'file:///tmp/local']) {
    assert.equal(isLoopback(url), false);
  }
});

test('local mode refuses missing targets and remote login overrides', (t) => {
  localEnvironment(t);
  assert.doesNotThrow(validateLocalTargets);
  process.env.LOGIN_URL = 'https://test.editor.mmi.gov.bc.ca';
  assert.throws(validateLocalTargets, /LOGIN_URL/);
  delete process.env.LOGIN_URL;
  assert.throws(validateLocalTargets, /LOGIN_URL/);
});

test('application URL overrides replace shared fixture origins while preserving routes', (t) => {
  localEnvironment(t);
  const data = DataLoader.loadJSON('test-data/test/loginData.json');
  assert.equal(data.qa.editor.url, 'http://localhost:40080/contents');
  assert.equal(data.qa.editor.reportUrl, 'http://localhost:40080/admin/reports/199');
  assert.equal(data.qa.subscriber.url, 'http://localhost:40081/');
  assert.equal(data.qa.microsoftMMI.url, 'http://localhost:40080/');
});
