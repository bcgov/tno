const { test, expect } = require('../../fixtures/api-fixtures');
const AppPagePage = require('../../pages/appPage');

test('@smoke Add product and validate via API', async ({ page, apiClient }) => {

  const appPage = new AppPage(page);

  await appPage.navigate('/');
  await appPage.login('standard_user', 'secret');

  await page.click('#add-to-cart');

  const response = await apiClient.get('/cart');
  const cartData = await response.json();

  expect(cartData.items.length).toBeGreaterThan(0);
});