// Copies monaco's prebuilt files into public/ so the app serves the JSON editor itself (no CDN).
// Runs before `yarn start` and `yarn build`; public/monaco is ignored by git.
const fs = require('fs');
const path = require('path');

const source = path.join(__dirname, '..', 'node_modules', 'monaco-editor', 'min', 'vs');
const target = path.join(__dirname, '..', 'public', 'monaco', 'vs');

if (!fs.existsSync(source)) {
  console.error(`monaco-editor is not installed: ${source} is missing. Run "yarn install".`);
  process.exit(1);
}
fs.rmSync(target, { recursive: true, force: true });
fs.cpSync(source, target, { recursive: true });
console.log(`Copied monaco to ${path.relative(process.cwd(), target)}`);
