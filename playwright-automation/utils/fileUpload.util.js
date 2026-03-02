const path = require('path');

function getFilePath(fileName) {
  return path.resolve(__dirname, `../test-data/files/${fileName}`);
}

module.exports = { getFilePath };