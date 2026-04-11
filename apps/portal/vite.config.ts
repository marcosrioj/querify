import { fileURLToPath, URL } from 'node:url';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

const allowedHosts = ['dev.portal.basefaq.com'];
const usePolling =
  process.env.CHOKIDAR_USEPOLLING === '1' ||
  process.env.VITE_USE_POLLING === '1' ||
  process.env.npm_lifecycle_event === 'dev:polling';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  base: '/',
  server: {
    allowedHosts,
    host: true,
    port: 5500,
    strictPort: true,
    watch: usePolling
      ? {
          interval: 300,
          usePolling: true,
        }
      : undefined,
  },
  preview: {
    allowedHosts,
    host: true,
    port: 5500,
    strictPort: true,
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  build: {
    chunkSizeWarningLimit: 3000,
  },
});
