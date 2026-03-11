const path = require('path');

function getFilePath(fileName) {
  return path.resolve(__dirname, `../test-data/${process.env.ENV_NAME}/files/${fileName}`);
}

module.exports = { getFilePath };