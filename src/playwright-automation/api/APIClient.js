const { request } = require('@playwright/test');

class APIClient {
  constructor(baseURL) {
    this.baseURL = baseURL;
    this.context = null;
  }

  async createContext() {
    this.context = await request.newContext({
      baseURL: this.baseURL,
      extraHTTPHeaders: {
        'Content-Type': 'application/json',
      },
    });
  }

  async get(endpoint) {
    return await this.context.get(endpoint);
  }

  async post(endpoint, body) {
    return await this.context.post(endpoint, { data: body });
  }

  async put(endpoint, body) {
    return await this.context.put(endpoint, { data: body });
  }

  async delete(endpoint) {
    return await this.context.delete(endpoint);
  }

  async dispose() {
    await this.context.dispose();
  }
}

module.exports = APIClient;