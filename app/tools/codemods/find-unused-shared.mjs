// Lists src/shared files none of whose exported names are referenced outside the file itself
// (barrel index.ts files are not counted as references). Conservative: any identifier match
// anywhere else counts as a use.
import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript-api';

const root = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..', '..');
const src = path.join(root, 'src');
const walk = (d) => fs.readdirSync(d, { withFileTypes: true }).flatMap((e) => (e.isDirectory() ? walk(path.join(d, e.name)) : /\.(tsx?|jsx?)$/.test(e.name) ? [path.join(d, e.name)] : []));
const all = walk(src).filter((f) => !f.endsWith('.d.ts'));
const texts = new Map(all.map((f) => [f, fs.readFileSync(f, 'utf8')]));
const isBarrel = (f) => path.basename(f).startsWith('index.');

const unused = [];
for (const file of all.filter((f) => f.startsWith(path.join(src, 'shared')) && !isBarrel(f))) {
  const sf = ts.createSourceFile(file, texts.get(file), ts.ScriptTarget.Latest, true, file.endsWith('x') ? ts.ScriptKind.TSX : ts.ScriptKind.TS);
  const names = new Set();
  for (const st of sf.statements) {
    const exported = ts.canHaveModifiers(st) && ts.getModifiers(st)?.some((m) => m.kind === ts.SyntaxKind.ExportKeyword);
    if (!exported) continue;
    if (ts.isVariableStatement(st)) st.declarationList.declarations.forEach((d) => ts.isIdentifier(d.name) && names.add(d.name.text));
    else if (st.name && ts.isIdentifier(st.name)) names.add(st.name.text);
  }
  if (!names.size) continue;
  const used = [...names].some((n) => {
    const re = new RegExp(`\\b${n}\\b`);
    for (const [f, t] of texts) if (f !== file && !isBarrel(f) && re.test(t)) return true;
    return false;
  });
  if (!used) unused.push(`${path.relative(src, file)}  [${[...names].join(', ')}]`);
}
console.log(unused.join('\n'));
console.log(`${unused.length} unused files`);
