// Type-aware inventory: every method called on a Moment value, every .format()/parse string,
// discarded results of mutating methods, and .toString()/.toISOString() on Moments.
import path from 'node:path';
import ts from 'typescript-api';
const root = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..', '..');
const cfg = ts.getParsedCommandLineOfConfigFile(path.join(root, 'tsconfig.json'), {}, { ...ts.sys, onUnRecoverableConfigFileDiagnostic: () => {} });
const program = ts.createProgram(cfg.fileNames, { ...cfg.options, noEmit: true });
const checker = program.getTypeChecker();
const isMoment = (node) => { const t = checker.getTypeAtLocation(node); const s = checker.typeToString(t); return /\bMoment\b/.test(s) && !/\[\]/.test(s); };
const methods = new Map(), formats = new Map(), discarded = [], toStr = [], iso = [];
const mutators = new Set(['add', 'subtract', 'startOf', 'endOf', 'set', 'tz', 'utc', 'local', 'year', 'month', 'date', 'day', 'weekday', 'hour', 'hours', 'minute', 'minutes', 'second', 'seconds', 'millisecond', 'milliseconds', 'utcOffset']);
const at = (sf, n) => `${path.relative(root, sf.fileName)}:${sf.getLineAndCharacterOfPosition(n.getStart(sf)).line + 1}`;
for (const sf of program.getSourceFiles()) {
  if (!sf.fileName.startsWith(path.join(root, 'src'))) continue;
  const visit = (n) => {
    if (ts.isCallExpression(n) && ts.isPropertyAccessExpression(n.expression) && isMoment(n.expression.expression)) {
      const name = n.expression.name.text; methods.set(name, (methods.get(name) ?? 0) + 1);
      if (name === 'format' && n.arguments[0]) { const k = n.arguments[0].getText(sf); formats.set(k, (formats.get(k) ?? 0) + 1); }
      if (name === 'toString') toStr.push(at(sf, n));
      if (name === 'toISOString') iso.push(at(sf, n));
      if (mutators.has(name) && n.arguments.length > (['startOf', 'endOf', 'utc', 'local'].includes(name) ? -1 : 0)) {
        const receiverIsFresh = ts.isCallExpression(n.expression.expression);
        if (ts.isExpressionStatement(n.parent)) discarded.push(`${at(sf, n)}  ${n.getText(sf).slice(0, 90)}`);
        else if (!receiverIsFresh) discarded.push(`${at(sf, n)}  (receiver reused) ${n.getText(sf).slice(0, 90)}`);
      }
    }
    if (ts.isCallExpression(n) && ts.isIdentifier(n.expression) && n.expression.text === 'moment' && n.arguments[1]) {
      const k = 'PARSE ' + n.arguments.slice(1).map((a) => a.getText(sf)).join(', '); formats.set(k, (formats.get(k) ?? 0) + 1);
    }
    ts.forEachChild(n, visit);
  };
  visit(sf);
}
const dump = (title, m) => console.log(`== ${title}\n` + [...m].sort((a, b) => b[1] - a[1]).map(([k, v]) => `${String(v).padStart(4)} ${k}`).join('\n'));
dump('methods on Moment values', methods); dump('format / parse arguments', formats);
console.log(`== mutating calls whose receiver is a variable or whose result is discarded (${discarded.length})\n${discarded.join('\n')}`);
console.log(`== .toString() on Moment (${toStr.length})`); console.log(`== .toISOString() on Moment (${iso.length})`);
