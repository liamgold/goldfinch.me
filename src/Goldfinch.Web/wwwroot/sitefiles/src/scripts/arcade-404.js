/*!
 * arcade-404.js
 * Arcade 404: counts the "Continue?" prompt down from 9, then shows "Thanks for playing".
 * Progressive enhancement: the count ships hidden, so with JS disabled the page reads
 * "Continue?" with the links underneath. Never redirects on its own.
 */
(() => {
  const root = document.querySelector('[data-arcade]');
  if (!root) return;

  const line = root.querySelector('[data-arcade-continue]');
  const count = root.querySelector('[data-arcade-count]');
  if (!line || !count) return;

  let n = 9;
  count.textContent = String(n);
  count.hidden = false;

  const timer = setInterval(() => {
    n -= 1;
    if (n > 0) {
      count.textContent = String(n);
      return;
    }
    clearInterval(timer);
    line.textContent = 'Thanks for playing';
    line.classList.add('arcade__continue--done');
  }, 1000);
})();
