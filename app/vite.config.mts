/// <reference types="vitest/config" />
import { fileURLToPath } from 'node:url';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

import { mmiDesign, resolveDesign } from './tools/vite-plugin-mmi-design.mts';

const root = fileURLToPath(new URL('.', import.meta.url));

export default defineConfig(({ command, mode, isPreview }) => {
  const isTest = mode === 'test' || !!process.env.VITEST;
  const design =
    !isTest && (command === 'serve' || isPreview) ? resolveDesign(process.env.APP_MODE) : undefined;

  return {
    appType: 'mpa',
    publicDir: 'public/shared',
    plugins: [react(), ...(isTest ? [] : [mmiDesign({ root, design })])],
    resolve: {
      alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
    },
    css: {
      // The theme objects come from ICSS `:export` blocks, which need postcss-modules.
      transformer: 'postcss',
      // Silence deprecation warnings raised inside dependencies (Bootstrap still uses @import).
      preprocessorOptions: { scss: { quietDeps: true } },
    },
    optimizeDeps: {
      entries: design ? [`${design}.html`] : ['editor.html', 'subscriber.html'],
    },
    server: {
      host: true,
      port: 3000,
      strictPort: true,
      allowedHosts: ['editor', 'subscriber', 'host.docker.internal'],
      watch:
        process.env.CHOKIDAR_USEPOLLING === 'true' ? { usePolling: true, interval: 300 } : undefined,
    },
    preview: {
      host: true,
      port: 3000,
      strictPort: true,
    },
    build: {
      outDir: 'dist',
      assetsDir: 'static',
      sourcemap: false,
      rolldownOptions: {
        input: { editor: 'editor.html', subscriber: 'subscriber.html' },
        // Keep module evaluation in source order across chunks, as webpack did. Code relies on it
        // (e.g. Prism language files expect the Prism global that App.tsx loads first).
        output: { strictExecutionOrder: true },
      },
    },
    test: {
      globals: true,
      environment: 'jsdom',
      // Vitest 5 clears mocks between tests by default; the suites were written for Vitest 3.
      clearMocks: false,
      projects: [
        {
          extends: true,
          test: {
            name: 'editor',
            include: ['src/editor/test-vitest/**/*.test.{ts,tsx}'],
            setupFiles: ['./src/editor/test-vitest/setup.ts'],
          },
        },
        {
          extends: true,
          test: {
            name: 'subscriber',
            include: ['src/subscriber/test-vitest/**/*.test.{ts,tsx}'],
            setupFiles: ['./src/subscriber/test-vitest/setup.ts'],
          },
        },
      ],
    },
  };
});
