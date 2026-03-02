const { test, expect } = require('../../fixtures/api-fixtures');
const ReqresRoutes = require('../../api/routes/reqres.routes');

test.describe('Reqres API Tests', () => {

  test('List users returns data', async ({ apiClient }) => {
    const reqres = new ReqresRoutes(apiClient);

    const response = await reqres.listUsers(2);
    expect(response.status()).toBe(200);

    const body = await response.json();
    expect(body.page).toBe(2);
    expect(body.data.length).toBeGreaterThan(0);
  });

  test('Create a new user', async ({ apiClient }) => {
    const reqres = new ReqresRoutes(apiClient);

    const payload = {
      name: 'Jeval Jani',
      job: 'Team Lead'
    };

    const response = await reqres.createUser(payload);
    expect(response.status()).toBe(201);

    const body = await response.json();
    expect(body.name).toBe(payload.name);
    expect(body.job).toBe(payload.job);
  });

  test('Get user by ID', async ({ apiClient }) => {
    const reqres = new ReqresRoutes(apiClient);

    const response = await reqres.getUser(2);
    expect(response.status()).toBe(200);

    const body = await response.json();
    expect(body.data.id).toBe(2);
  });

  test('Update user', async ({ apiClient }) => {
    const reqres = new ReqresRoutes(apiClient);

    const payload = {
      name: 'Jane Doe',
      job: 'Engineer'
    };
    
    const response = await reqres.updateUser(2, payload);
    expect(response.status()).toBe(200);

    const body = await response.json();
    expect(body.name).toBe(payload.name);
    expect(body.job).toBe(payload.job);
  });

  test('Delete user', async ({ apiClient }) => {
    const reqres = new ReqresRoutes(apiClient);

    const response = await reqres.deleteUser(2);
    expect(response.status()).toBe(204);
  });

});