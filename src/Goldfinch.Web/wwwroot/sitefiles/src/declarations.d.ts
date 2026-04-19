// TypeScript shim so importing a CSS asset from node_modules (e.g. highlight.js
// themes) doesn't trip the compiler. Vite handles the actual bundling.
declare module '*.css';
