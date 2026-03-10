import { test, expect } from '../../../fixtures/ui-fixture';
import AppPage from '../../../pages/appPage';
import { username, password } from '../../../config/env.config';
import { loadJSON } from '../../../utils/dataLoader';

const users = loadJSON('test-data/loginData.json');

test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test.describe('Scenario 2: Adding a new minister', () => {
  test(`Login as ${username}`, async ({ page }) => {
    const appPage = new AppPage(page);

    await appPage.mmiMSLogin(username, password);
    await expect(page).toHaveURL('/contents');

    await appPage.navigateToContentConfiguration();
    await appPage.navigateToMinisters();
    await appPage.clickAddNewMinister();

    const randomNum = Math.floor(Math.random() * 10000);
    const ministerName = `Test Minister Name - ${randomNum}`;
    const ministerDescription = `Test Minister Description - ${randomNum}`;
    const ministerPosition = `Test Minister Position - ${randomNum}`;
    const ministerSortOrder = 1;

    await appPage.enterMinisterDetails(
      ministerName,
      ministerDescription,
      ministerPosition,
      ministerSortOrder
    );

    await page.waitForTimeout(10000);
    await appPage.clickBackToMinisters();
    await page.waitForTimeout(2000);

    await appPage.searchMinisterByName(ministerName);

    const searchedMinisterName = await appPage.getSearchedMinisterName(ministerName);

    console.log(`Searched Minister Name: ${searchedMinisterName}`);
    console.log(`Expected Minister Name: ${ministerName}`);

    await expect(searchedMinisterName).toContain(ministerName);
  });
});