class TabManager {
    
  constructor(page) {
    this.parentPage = page;
    this.context = page.context();
  }

  /**
   * Switch to Newly Opened tab
   */
  async switchToNewTab(options = {}) {
    const { waitUtil = 'load', timeout = 10000 } = options;

    const existingPages = this.context.pages();

    const newPage = await this.context.waitForEvent('page', {
      timeout,
      predicate: (page) => !existingPages.includes(page),
    });

    await newPage.waitForLoadState(waitUntil, { timeout });
    await newPage.bringToFront();

    return newPage;
  }

  /**
   * Switch back to Parent tab
   */
  async switchToParent() {
    await this.parentPage.bringToFront()
    return this.parentPage();
  }

  /**
   * Close Current Tab an Switch Back
   */
  async closeCurrentTabAndSwitch(currentPage){
    await currentPage.close();
    await this.parentPage.bringToFront()
    return this.parentPage();
  }
}
