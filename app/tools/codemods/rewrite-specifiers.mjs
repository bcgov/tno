// Rewrites module specifiers (import/export/import()/import type/vi.mock and friends) in place.
// Only the string literal is replaced, so formatting and import order are untouched.
// Idempotent: run it again after merging `dev` to catch newly added files.
//
//   node tools/codemods/rewrite-specifiers.mjs design-roots   bare 'store', 'features/x'... -> '@/<design|shared>/...'
//   node tools/codemods/rewrite-specifiers.mjs tno-core       'tno-core' -> '@/shared'
//   node tools/codemods/rewrite-specifiers.mjs react-router   'react-router-dom' -> 'react-router'
//   node tools/codemods/rewrite-specifiers.mjs dnd            'react-beautiful-dnd' -> '@/shared/components/dnd'
import fs from 'node:fs';
import path from 'node:path';
// TypeScript 7 has no JavaScript API; the codemods use the TypeScript 6 API under an alias.
import ts from 'typescript-api';

const root = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..', '..');
const src = path.join(root, 'src');
const mode = process.argv[2];

const callees = new Set([
  'import',
  'require',
  'vi.mock',
  'vi.doMock',
  'vi.unmock',
  'vi.importActual',
  'vi.importMock',
  'jest.mock',
  'jest.requireActual',
]);

function files(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) return files(full);
    return /\.(ts|tsx|js|jsx|mjs)$/.test(entry.name) ? [full] : [];
  });
}

function designOf(file) {
  const rel = path.relative(src, file).split(path.sep);
  return ['editor', 'subscriber', 'shared'].includes(rel[0]) ? rel[0] : undefined;
}

const rewriters = {
  'design-roots': (spec, file) => {
    const design = designOf(file);
    if (!design || spec.startsWith('.') || spec.startsWith('@')) return;
    const [head, ...rest] = spec.split('/');
    let folder = head;
    // Vitest aliased `test` to `src/test-vitest`; the legacy Jest tests used `src/test`.
    if (head === 'test') folder = file.includes(`${path.sep}test-vitest${path.sep}`) ? 'test-vitest' : 'test';
    if (!fs.existsSync(path.join(src, design, folder))) return;
    const roots = design === 'shared' ? ['hooks', 'components', 'features', 'utils', 'css', 'assets'] : ['store', 'components', 'features', 'utils', 'test', 'css'];
    if (!roots.includes(head)) return;
    return ['@', design, folder, ...rest].join('/');
  },
  'tno-core': (spec) => {
    if (spec === 'tno-core') return '@/shared';
    if (spec.startsWith('tno-core/dist/')) return `@/shared/${spec.slice('tno-core/dist/'.length)}`;
    if (spec.startsWith('tno-core/src/')) return `@/shared/${spec.slice('tno-core/src/'.length)}`;
  },
  'react-router': (spec) => (spec === 'react-router-dom' ? 'react-router' : undefined),
  'es-types': (spec) =>
    spec === '@elastic/elasticsearch/lib/api/types' ? '@/shared/utils/elastic/esTypes' : undefined,
  dnd: (spec) => (spec === 'react-beautiful-dnd' ? '@/shared/components/dnd' : undefined),
};

const rewrite = rewriters[mode];
if (!rewrite) {
  console.error(`Unknown mode '${mode}'. Use one of: ${Object.keys(rewriters).join(', ')}`);
  process.exit(1);
}

let changedFiles = 0;
let changedSpecifiers = 0;
for (const file of files(src)) {
  const text = fs.readFileSync(file, 'utf8');
  const kind = file.endsWith('x') ? ts.ScriptKind.TSX : ts.ScriptKind.TS;
  const source = ts.createSourceFile(file, text, ts.ScriptTarget.Latest, true, kind);
  const edits = [];
  const consider = (literal) => {
    if (!literal || !ts.isStringLiteralLike(literal)) return;
    const next = rewrite(literal.text, file);
    if (next && next !== literal.text) edits.push([literal.getStart(source) + 1, literal.getEnd() - 1, next]);
  };
  const visit = (node) => {
    if (ts.isImportDeclaration(node) || ts.isExportDeclaration(node)) consider(node.moduleSpecifier);
    else if (ts.isImportTypeNode(node) && ts.isLiteralTypeNode(node.argument)) consider(node.argument.literal);
    else if (ts.isCallExpression(node)) {
      const callee = node.expression.kind === ts.SyntaxKind.ImportKeyword ? 'import' : node.expression.getText(source);
      if (callees.has(callee)) consider(node.arguments[0]);
    }
    ts.forEachChild(node, visit);
  };
  visit(source);
  if (!edits.length) continue;
  let output = text;
  for (const [start, end, value] of edits.sort((a, b) => b[0] - a[0])) output = output.slice(0, start) + value + output.slice(end);
  fs.writeFileSync(file, output);
  changedFiles += 1;
  changedSpecifiers += edits.length;
}
console.log(`${mode}: ${changedSpecifiers} specifiers in ${changedFiles} files`);
