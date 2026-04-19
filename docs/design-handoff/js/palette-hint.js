/*!
 * palette-hint.js
 * Small home-page prompt that says "Press ⌘K for anywhere."
 * Dismissible; remembers via localStorage.
 */
(() => {
  const hint = document.querySelector('[data-palette-hint]');
  if (!hint) return;
  const KEY = 'gf2_palette_hint_dismissed';
  if (localStorage.getItem(KEY) === '1') { hint.hidden = true; return; }
  hint.hidden = false;
  hint.querySelector('[data-dismiss]')?.addEventListener('click', () => {
    hint.hidden = true;
    localStorage.setItem(KEY, '1');
  });
})();
