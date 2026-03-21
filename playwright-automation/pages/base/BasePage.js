const { expect } = require("allure-playwright");

class BasePage {
  constructor(page) {
    this.page = page;
  }

  async navigate(path = '') {
    await this.page.goto(path);
  }

  async click(locator) {
    await locator.waitFor({ state: 'visible' });
    await locator.click();
  }

  async clear(locator) {
    return locator.clear();
  }

  async type(locator, value) {
    await locator.waitFor({ state: 'visible' });
    await locator.fill(value);
  }

  async getText(locator) {
    return await locator.textContent();
  }

  async isElementVisible(locator, timeout = 5000) {
    try {
      await locator.waitFor({state: 'visible', timeout});
      return true;
    } catch (error) {
      return false;
    }
  }

  async uploadFile(locator, filePath) {
    await locator.setInputFiles(filePath);
  }

  async getInputValue(locator, timeout = 5000) {
    await locator.waitFor({state: 'visible', timeout});
    const value = await locator.inputValue();
    return value.trim();
  }

  /**
   * Method to check if any element in a locator collection contains given text
   * @param {Locator} locator - Playwright locator returning multiple elements
   * @param {string} expectedText - Text to search for
   * @returns {boolean} - true if present else false
   */
  async isTextPresentInCollection(locator, expectedText) {
    try {
      const allTexts = await locator.allTextContents();

      return allTexts.some(text => 
        text.trim().includes(expectedText)
      );
    } catch (error) {
      return false;
    }
  }

  async hardWait(milliseconds) {
    await this.page.waitForTimeout(milliseconds);
  }

  async clickElementByText(locator, expectedText) {
    const count = await locator.count();
    if(count === 0){
      return false;
    }

    for(let i=0; i < count; i++){
      const element = await locator.nth(i);
      const text = (await element.innerText()).trim();

      if( text === expectedText) {
        await this.click(element);
        return true;
      }
    }
    return false;
  }

  async isElementClickable(locator) {
    try {
      await locator.click({trail : true});
      return true;
    } catch (error) {
      return false;
    }
  }

  async refreshPage() {
    await this.page.reload({ waitUntil : 'load' });
  }

  async isElementEnabled(locator) {
    await locator.waitFor({state:'attached'});
    return await locator.isEnabled();
  }

  async getElementText(locator) {
    await locator.waitFor( { state: 'attached' } );
    return await locator.innerText();
  }
}

module.exports = BasePage;