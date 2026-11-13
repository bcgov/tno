import fs from 'node:fs';
import type { IncomingMessage, ServerResponse } from 'node:http';
import path from 'node:path';

import type { Plugin, ResolvedConfig } from 'vite';

/** The two designs this project builds. One deployment presents exactly one of them. */
export const designs = ['editor', 'subscriber'] as const;
export type Design = (typeof designs)[number];

/**
 * Validate the design a server should present (the APP_MODE environment variable).
 * There is deliberately no default: a misconfigured deployment must fail loudly.
 */
export function resolveDesign(value: string | undefined): Design {
  if (value === 'editor' || value === 'subscriber') return value;
  throw new Error(`APP_MODE must be 'editor' or 'subscriber' (got '${value ?? '<unset>'}').`);
}

export interface IMmiDesignOptions {
  /** Absolute path of the project root. */
  root: string;
  /** The design served by `vite` / `vite preview`. Not used by `vite build`, which builds both. */
  design?: Design;
}

const contentTypes: Record<string, string> = {
  '.css': 'text/css',
  '.html': 'text/html; charset=utf-8',
  '.ico': 'image/x-icon',
  '.js': 'text/javascript',
  '.json': 'application/json',
  '.map': 'application/json',
  '.mp4': 'video/mp4',
  '.png': 'image/png',
  '.svg': 'image/svg+xml',
  '.ttf': 'font/ttf',
  '.txt': 'text/plain',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
};

const monacoSource = (root: string) => path.join(root, 'node_modules', 'monaco-editor', 'min', 'vs');

/** Resolve a URL path inside `base`, returning the file only if it exists and stays inside `base`. */
function resolveFile(base: string, pathname: string): string | undefined {
  let decoded: string;
  try {
    decoded = decodeURIComponent(pathname);
  } catch {
    return undefined;
  }
  const file = path.join(base, decoded);
  if (!file.startsWith(base + path.sep)) return undefined;
  return fs.existsSync(file) && fs.statSync(file).isFile() ? file : undefined;
}

function sendFile(res: ServerResponse, file: string) {
  res.setHeader('Content-Type', contentTypes[path.extname(file)] ?? 'application/octet-stream');
  fs.createReadStream(file).pipe(res);
}

function pathnameOf(req: IncomingMessage) {
  return new URL(req.url ?? '/', 'http://localhost').pathname;
}

/** A browser page load (as opposed to a module, asset or API request). */
function isNavigation(req: IncomingMessage, pathname: string) {
  if (req.method !== 'GET' && req.method !== 'HEAD') return false;
  if (!(req.headers.accept ?? '').includes('text/html')) return false;
  if (pathname.startsWith('/@') || pathname === '/api' || pathname.startsWith('/api/')) return false;
  return path.extname(pathname) === '' || pathname.endsWith('.html');
}

/**
 * Serves one of two designs from a single project.
 *
 * - `vite` / `vite preview`: page navigations get the selected design's HTML document,
 *   `public/<design>` is served over `public/shared`, and Monaco is served from node_modules.
 * - `vite build`: both designs are built; each design's document and public files are moved to
 *   `dist/_<design>/`, which the production nginx serves as an overlay over the shared root.
 *   The build fails if either design's chunk graph contains the other design's source.
 */
export function mmiDesign(options: IMmiDesignOptions): Plugin {
  const { root } = options;
  let config: ResolvedConfig;

  return {
    name: 'mmi-design',

    configResolved(resolved) {
      config = resolved;
    },

    configureServer(server) {
      const design = options.design;
      if (!design) throw new Error('mmi-design: the dev server requires APP_MODE.');
      const overlay = path.join(root, 'public', design);
      const monaco = monacoSource(root);
      server.middlewares.use((req, res, next) => {
        const pathname = pathnameOf(req);
        if (pathname.startsWith('/monaco/vs/')) {
          const file = resolveFile(monaco, pathname.slice('/monaco/vs'.length));
          if (file) return sendFile(res, file);
        }
        if (pathname !== '/') {
          const file = resolveFile(overlay, pathname);
          if (file) return sendFile(res, file);
        }
        if (isNavigation(req, pathname)) req.url = `/${design}.html`;
        next();
      });
    },

    configurePreviewServer(server) {
      const design = options.design;
      if (!design) throw new Error('mmi-design: the preview server requires APP_MODE.');
      const outDir = path.resolve(root, config.build.outDir);
      const overlay = path.join(outDir, `_${design}`);
      server.middlewares.use((req, res, next) => {
        const pathname = pathnameOf(req);
        if (pathname.startsWith('/_editor') || pathname.startsWith('/_subscriber')) {
          res.statusCode = 404;
          return res.end();
        }
        if (pathname !== '/') {
          const file = resolveFile(overlay, pathname);
          if (file) return sendFile(res, file);
          if (resolveFile(outDir, pathname)) return next();
        }
        if (isNavigation(req, pathname)) return sendFile(res, path.join(overlay, 'index.html'));
        next();
      });
    },

    generateBundle(_options, bundle) {
      const chunks = Object.values(bundle).filter((output) => output.type === 'chunk');
      const byFile = new Map(chunks.map((chunk) => [chunk.fileName, chunk]));
      for (const design of designs) {
        const other = designs.find((value) => value !== design)!;
        const entry = chunks.find((chunk) => chunk.isEntry && chunk.name === design);
        if (!entry) this.error(`mmi-design: no entry chunk for '${design}'.`);
        const seen = new Set<string>();
        const pending = [entry.fileName];
        while (pending.length) {
          const fileName = pending.pop()!;
          if (seen.has(fileName)) continue;
          seen.add(fileName);
          const chunk = byFile.get(fileName);
          if (chunk) pending.push(...chunk.imports, ...chunk.dynamicImports);
        }
        const marker = `${path.sep}src${path.sep}${other}${path.sep}`;
        const leaked = [...seen]
          .flatMap((fileName) => byFile.get(fileName)?.moduleIds ?? [])
          .filter((id) => id.includes(marker));
        if (leaked.length)
          this.error(
            `mmi-design: the ${design} design loads ${other} source:\n  ${leaked
              .slice(0, 20)
              .join('\n  ')}`,
          );
      }
    },

    closeBundle() {
      if (config.command !== 'build') return;
      const outDir = path.resolve(root, config.build.outDir);
      for (const design of designs) {
        const target = path.join(outDir, `_${design}`);
        fs.mkdirSync(target, { recursive: true });
        fs.renameSync(path.join(outDir, `${design}.html`), path.join(target, 'index.html'));
        fs.cpSync(path.join(root, 'public', design), target, { recursive: true });
      }
      fs.cpSync(monacoSource(root), path.join(outDir, 'monaco', 'vs'), { recursive: true });
    },
  };
}
