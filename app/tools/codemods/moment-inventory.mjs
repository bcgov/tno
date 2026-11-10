// Inventory of moment usage: call shapes of moment(...) / moment.x(...) and chained methods.
import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript-api';
const src = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..', '..', 'src');
const walk = (d) => fs.readdirSync(d, { withFileTypes: true }).flatMap((e) => (e.isDirectory() ? walk(path.join(d, e.name)) : /\.(tsx?|jsx?)$/.test(e.name) ? [path.join(d, e.name)] : []));
const count = new Map(); const add = (k) => count.set(k, (count.get(k) ?? 0) + 1);
for (const f of walk(src)) {
  const text = fs.readFileSync(f, 'utf8'); if (!/from 'moment/.test(text)) continue;
  const sf = ts.createSourceFile(f, text, ts.ScriptTarget.Latest, true, f.endsWith('x') ? ts.ScriptKind.TSX : ts.ScriptKind.TS);
  const visit = (n) => {
    if (ts.isCallExpression(n)) {
      const e = n.expression;
      if (ts.isIdentifier(e) && e.text === 'moment') add(`moment(${n.arguments.length} args${n.arguments.length > 1 ? ': ' + n.arguments.slice(1).map((a) => a.getText(sf).slice(0, 30)).join(', ') : ''})`);
      if (ts.isPropertyAccessExpression(e) && ts.isIdentifier(e.expression) && e.expression.text === 'moment') add(`moment.${e.name.text}(${n.arguments.length})`);
      if (ts.isPropertyAccessExpression(e) && ts.isPropertyAccessExpression(e.expression) && e.expression.expression.getText(sf) === 'moment') add(`moment.${e.expression.name.text}.${e.name.text}(${n.arguments.length})`);
    }
    ts.forEachChild(n, visit);
  };
  visit(sf);
}
console.log([...count].sort((a, b) => b[1] - a[1]).map(([k, v]) => `${String(v).padStart(4)} ${k}`).join('\n'));
