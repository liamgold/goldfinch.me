/*!
 * copy-link.js
 * Reveals the copy-link button on post detail pages when navigator.clipboard
 * is available, then writes the canonical URL to the clipboard on click.
 */
(() => {
  const btn = document.querySelector('.copy-link-btn');
  if (!btn || !navigator.clipboard) return;

  btn.hidden = false;

  btn.addEventListener('click', () => {
    const url = btn.dataset.copyUrl || window.location.href;
    navigator.clipboard.writeText(url).then(() => {
      btn.dataset.copied = 'true';
      setTimeout(() => delete btn.dataset.copied, 2000);
    });
  });
})();
