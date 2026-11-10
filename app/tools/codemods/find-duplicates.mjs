// Inventory of duplicated source between the designs (and shared).
// Normalises away comments, whitespace, `type` import modifiers, quote style and the
// '@/editor/' vs '@/subscriber/' alias prefix, then groups files with equal content.
// Barrels (index.ts) and test files are skipped.
import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript-api';

const root = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..', '..');
const src = path.join(root, 'src');
const walk = (d) => fs.readdirSync(d, { withFileTypes: true }).flatMap((e) => (e.isDirectory() ? walk(path.join(d, e.name)) : /\.(tsx?|jsx?)$/.test(e.name) && !e.name.endsWith('.d.ts') ? [path.join(d, e.name)] : []));
const files = walk(src).filter((f) => !path.basename(f).startsWith('index.') && !/test-vitest|\.test\./.test(f));

function normalise(file) {
  const text = fs.readFileSync(file, 'utf8');
  const sf = ts.createSourceFile(file, text, ts.ScriptTarget.Latest, true, file.endsWith('x') ? ts.ScriptKind.TSX : ts.ScriptKind.TS);
  const printer = ts.createPrinter({ removeComments: true });
  return printer
    .printFile(sf)
    .replace(/@\/(editor|subscriber)\//g, '@/X/')
    .replace(/\btype\s+(?=[A-Za-z_{])/g, (m, o, s) => (/import|export/.test(s.slice(Math.max(0, o - 200), o)) ? '' : m))
    .replace(/\s+/g, ' ');
}
const groups = new Map();
for (const f of files) {
  const h = crypto.createHash('sha1').update(normalise(f)).digest('hex');
  groups.set(h, [...(groups.get(h) ?? []), path.relative(src, f)]);
}
const dup = [...groups.values()].filter((g) => g.length > 1 && new Set(g.map((f) => f.split(path.sep)[0])).size > 1);
for (const g of dup) console.log(g.join('  ==  '));
console.log(`${dup.length} groups of identical files across designs/shared`);
