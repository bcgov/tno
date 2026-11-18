const fs = require('fs');
const path = require('path');

class DataLoader {

  static loadJSON(relativePath) {

    const filePath = path.resolve(process.cwd(), relativePath);

    if (!fs.existsSync(filePath)) {
     throw new Error(`Test data file not found: ${filePath}`);
    }

    const rawData = fs.readFileSync(filePath, 'utf-8');
    const data = JSON.parse(rawData);
    // Environment URLs must win over addresses stored in shared test fixtures.
    if (path.basename(filePath) === 'loginData.json') {
      for (const app of Object.values(data)) {
        for (const [name, settings] of Object.entries(app)) {
          const base = name === 'subscriber' ? process.env.SUBSCRIBER_URL : process.env.EDITOR_URL;
          if (!base) continue;
          for (const [key, value] of Object.entries(settings)) {
            if (typeof value === 'string' && /^https?:\/\//.test(value)) {
              const original = new URL(value);
              settings[key] = new URL(original.pathname + original.search + original.hash, base).href;
            }
          }
        }
      }
    }
    return data;
  }
 static loadByEnvironment(relativePath, env) {
    const filePath = path.resolve(process.cwd(), `${relativePath}.${env}.json`);
    return this.loadJSON(filePath);
  }
}

module.exports = DataLoader;
