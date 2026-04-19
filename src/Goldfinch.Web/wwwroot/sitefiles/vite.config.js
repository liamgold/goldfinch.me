import { defineConfig } from 'vite';

export default defineConfig({
  build: {
    manifest: false,
    rolldownOptions: {
      input: {
        codeblock: 'src/codeblock/codeblock.ts',
        main: 'src/main.ts',
        global: 'src/global.css',
      },
    },
  },
});
