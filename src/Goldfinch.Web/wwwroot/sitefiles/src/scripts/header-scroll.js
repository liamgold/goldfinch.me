/*!
 * header-scroll.js
 * Toggle data-scrolled on the main <header> when the page has scrolled
 * past a few pixels. The visual change (blurred background, border) is
 * driven entirely by CSS matching [data-scrolled="true"].
 */
(() => {
  const header = document.querySelector('header.header');
  if (!header) return;
  const update = () => {
    header.dataset.scrolled = window.scrollY > 6 ? 'true' : 'false';
  };
  update();
  window.addEventListener('scroll', update, { passive: true });
})();
