const { test, expect } = require('../../fixtures/ui-fixture');
const { getFilePath } = require('../../utils/fileUpload.util');


  test('Upload PDF file', async ({ page }) => {
    const filePath = getFilePath('sample.pdf');
    await page.locator('#upload').setInputFiles(filePath);
  });

