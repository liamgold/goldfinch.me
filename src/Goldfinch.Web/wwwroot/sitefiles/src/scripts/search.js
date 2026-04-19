/*!
 * search.js
 * Live search for the blog archive toolbar. Debounced fetch to /api/search.
 * Progressive enhancement: the input lives inside a <form method="get"
 * action="/blog"> so submitting without JS performs a normal server-side
 * search (?q=…).
 */
(() => {
  const input = document.querySelector('[data-live-search]');
  const resultsEl = document.querySelector('[data-live-search-results]');
  if (!input || !resultsEl) return;

  let timer = null;
  let abortCtl = null;

  input.addEventListener('input', () => {
    const q = input.value.trim();
    clearTimeout(timer);
    abortCtl?.abort();
    if (!q) { resultsEl.hidden = true; resultsEl.innerHTML = ''; return; }
    timer = setTimeout(() => run(q), 180);
  });

  // Esc clears + hides
  input.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') { input.value = ''; resultsEl.hidden = true; resultsEl.innerHTML = ''; }
    if (e.key === 'ArrowDown') {
      const first = resultsEl.querySelector('a');
      if (first) { e.preventDefault(); first.focus(); }
    }
  });

  // Up/down within the result list
  resultsEl.addEventListener('keydown', (e) => {
    if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp') return;
    const items = [...resultsEl.querySelectorAll('a')];
    const i = items.indexOf(document.activeElement);
    if (i === -1) return;
    e.preventDefault();
    const next = e.key === 'ArrowDown' ? items[i + 1] : items[i - 1] || input;
    next?.focus();
  });

  // Click outside closes
  document.addEventListener('click', (e) => {
    if (!resultsEl.contains(e.target) && e.target !== input) {
      resultsEl.hidden = true;
    }
  });

  async function run(q) {
    abortCtl = new AbortController();
    try {
      const res = await fetch(`/api/search?q=${encodeURIComponent(q)}&limit=6`, {
        signal: abortCtl.signal,
        headers: { Accept: 'application/json' },
      });
      const data = await res.json();
      resultsEl.innerHTML = (data.results || []).map(r => `
        <a class="live-search-item" href="${escape(r.url)}">
          <span class="live-search-item__title">${r.highlights?.title ? safeHighlight(r.highlights.title) : escape(r.title || r.label)}</span>
          ${r.date ? `<span class="live-search-item__meta mono">${formatDate(r.date)}</span>` : ''}
        </a>
      `).join('') || `<div class="live-search-empty mono">&gt; no results</div>`;
      resultsEl.hidden = false;
    } catch (err) {
      if (err.name !== 'AbortError') {
        resultsEl.innerHTML = `<div class="live-search-empty mono">&gt; search unavailable</div>`;
        resultsEl.hidden = false;
      }
    }
  }

  function escape(s) {
    return String(s ?? '').replace(/[&<>"']/g, (c) =>
      ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  }
  function safeHighlight(s) {
    return escape(s).replace(/&lt;mark&gt;(.*?)&lt;\/mark&gt;/g, '<mark>$1</mark>');
  }
  function formatDate(iso) {
    const d = new Date(iso);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
  }
})();
