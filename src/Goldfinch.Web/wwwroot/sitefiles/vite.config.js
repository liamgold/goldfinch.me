import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  build: {
    manifest: false,
    rolldownOptions: {
      input: {
        codeblock: 'src/codeblock/codeblock.ts',
        main: 'src/main.ts',
        global: 'src/tailwind.css',
      },
    },
  },
  plugins: [tailwindcss()],
});
