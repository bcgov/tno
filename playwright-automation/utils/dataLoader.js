const fs = require('fs');
const path = require('path');

class DataLoader {

  static loadJSON(relativePath) {
    
    const filePath = path.resolve(process.cwd(), relativePath);
    
    if (!fs.existsSync(filePath)) {
     throw new Error(`Test data file not found: ${filePath}`);
    }
  
    const rawData = fs.readFileSync(filePath, 'utf-8');
    return JSON.parse(rawData);
  }
 static loadByEnvironment(relativePath, env) {
    const filePath = path.resolve(process.cwd(), `${relativePath}.${env}.json`);
    return this.loadJSON(filePath);
  }
}

module.exports = DataLoader;