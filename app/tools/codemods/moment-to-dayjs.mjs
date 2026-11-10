// Migrates moment / moment-timezone to the shared Day.js module (src/shared/utils/date).
// - imports -> import { dayjs, type Dayjs } from '@/shared/utils/date'
// - value references to the moment import -> dayjs; Moment / moment.Moment types -> Dayjs
// - moment.tz(zone) (current time in a zone) -> dayjs().tz(zone)
// - in format() calls on moment values, 'yyyy' / 'yy' (moment: the 4-digit year) -> 'YYYY'
import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript-api';

const root = path.resolve(path.dirname(new URL(import.meta.url).pathname), '..', '..');
const cfg = ts.getParsedCommandLineOfConfigFile(path.join(root, 'tsconfig.json'), {}, { ...ts.sys, onUnRecoverableConfigFileDiagnostic: () => {} });
const program = ts.createProgram(cfg.fileNames, { ...cfg.options, noEmit: true });
const checker = program.getTypeChecker();
const isMoment = (node) => /\bMoment\b/.test(checker.typeToString(checker.getTypeAtLocation(node)));
const skip = new Set([path.join(root, 'src/shared/utils/date/dayjs.moment.test.ts')]);

let files = 0;
for (const sf of program.getSourceFiles()) {
  if (!sf.fileName.startsWith(path.join(root, 'src')) || skip.has(sf.fileName)) continue;
  const imports = sf.statements.filter((s) => ts.isImportDeclaration(s) && /^moment(-timezone)?$/.test(s.moduleSpecifier.text));
  if (!imports.length) continue;
  const edits = [];
  let defaultName;
  let typeNames = new Set();
  for (const imp of imports) {
    const clause = imp.importClause;
    if (clause?.name) defaultName = clause.name.text;
    if (clause?.namedBindings && ts.isNamedImports(clause.namedBindings))
      for (const el of clause.namedBindings.elements) typeNames.add(el.name.text);
  }
  const names = [defaultName ? 'dayjs' : undefined, typeNames.size ? 'type Dayjs' : undefined].filter(Boolean);
  edits.push([imports[0].getStart(sf), imports[0].getEnd(), `import { ${names.join(', ')} } from '@/shared/utils/date';`]);
  for (const imp of imports.slice(1)) edits.push([imp.getFullStart(), imp.getEnd(), '']);
  const inImport = (n) => imports.some((imp) => n.pos >= imp.pos && n.end <= imp.end);
  const visit = (n) => {
    if (inImport(n)) return;
    if (ts.isQualifiedName(n) && ts.isIdentifier(n.left) && n.left.text === defaultName && n.right.text === 'Moment') {
      edits.push([n.getStart(sf), n.getEnd(), 'Dayjs']);
      return;
    }
    if (ts.isCallExpression(n) && ts.isPropertyAccessExpression(n.expression) && ts.isIdentifier(n.expression.expression) &&
        n.expression.expression.text === defaultName && n.expression.name.text === 'tz' && n.arguments.length === 1) {
      edits.push([n.expression.getStart(sf), n.expression.getEnd(), 'dayjs().tz']);
      n.arguments.forEach(visit);
      return;
    }
    if (ts.isCallExpression(n) && ts.isPropertyAccessExpression(n.expression) && n.expression.name.text === 'format' &&
        n.arguments[0] && ts.isStringLiteral(n.arguments[0]) && /y/.test(n.arguments[0].text) && isMoment(n.expression.expression)) {
      const lit = n.arguments[0];
      edits.push([lit.getStart(sf) + 1, lit.getEnd() - 1, lit.text.replace(/y{2,4}/g, 'YYYY')]);
    }
    if (ts.isIdentifier(n)) {
      const p = n.parent;
      const isPropertyName = (ts.isPropertyAccessExpression(p) && p.name === n) || (ts.isPropertyAssignment(p) && p.name === n) || (ts.isQualifiedName(p) && p.right === n);
      if (!isPropertyName && n.text === defaultName) edits.push([n.getStart(sf), n.getEnd(), 'dayjs']);
      else if (!isPropertyName && typeNames.has(n.text) && n.text === 'Moment') edits.push([n.getStart(sf), n.getEnd(), 'Dayjs']);
    }
    ts.forEachChild(n, visit);
  };
  ts.forEachChild(sf, visit);
  let text = sf.getFullText();
  for (const [s, e, v] of edits.sort((a, b) => b[0] - a[0])) text = text.slice(0, s) + v + text.slice(e);
  fs.writeFileSync(sf.fileName, text);
  files++;
}
console.log(`moment-to-dayjs: ${files} files`);
