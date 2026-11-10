import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-vitest/setup.ts'],
    include: ['src/test-vitest/**/*.test.{ts,tsx}'],
    exclude: ['node_modules', 'dist', 'build'],
  },
  resolve: {
    alias: {
      src: path.resolve(__dirname, './src'),
      store: path.resolve(__dirname, './src/store'),
      hooks: path.resolve(__dirname, './src/hooks'),
      features: path.resolve(__dirname, './src/features'),
      components: path.resolve(__dirname, './src/components'),
      test: path.resolve(__dirname, './src/test-vitest'),
    },
  },
});
