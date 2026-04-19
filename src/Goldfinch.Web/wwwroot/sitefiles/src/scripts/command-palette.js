/*!
 * command-palette.js
 * ⌘K / Ctrl+K opens a dialog with a search input and keyboard-navigable
 * result list. Results come from GET /api/search. Each result is a real
 * URL — Enter just follows the link, so there's no SPA routing.
 *
 * Progressive enhancement: with JS off, ⌘K does nothing; the search
 * form on the blog archive (non-JS) is the fallback.
 *
 * Implementation note: the palette chrome (input + list container + footer)
 * is rendered once on open. Subsequent keystrokes only update the list's
 * innerHTML — the <input> DOM node is preserved so the caret position
 * stays put (rebuilding it on every keystroke makes typing appear to
 * reverse because focus drops the caret back to index 0).
 */
(() => {
  const root = document.getElementById('palette-root');
  if (!root) return;

  const state = { open: false, q: '', items: [], active: 0, loading: false };
  let abortCtl = null;
  let debounceTimer = null;
  let listEl = null;
  let inputEl = null;
  let countEl = null;

  const DEFAULTS = [
    { kind: 'page', url: '/', title: 'Go to Home' },
    { kind: 'page', url: '/blog', title: 'Go to Blog' },
    { kind: 'page', url: '/about', title: 'Go to About' },
    { kind: 'page', url: '/speaking', title: 'Go to Speaking' },
  ];

  // ------- rendering -------

  const renderShell = () => {
    root.innerHTML = `
      <div class="palette-backdrop" role="presentation">
        <div class="palette-dialog" role="dialog" aria-modal="true" aria-label="Command palette">
          <div class="palette-inputbar">
            <span class="palette-icon" aria-hidden="true">⌕</span>
            <input
              class="palette-input mono"
              type="search"
              autocomplete="off"
              spellcheck="false"
              placeholder="Type a post, tag, or page…"
              aria-label="Search posts and navigation"
              aria-controls="palette-listbox">
            <button class="palette-esc" type="button" aria-label="Close">ESC</button>
          </div>
          <div class="palette-list" role="listbox" id="palette-listbox"></div>
          <div class="palette-footer mono">
            <span><kbd class="palette-kbd">↑</kbd><kbd class="palette-kbd">↓</kbd> move</span>
            <span><kbd class="palette-kbd">↵</kbd> select</span>
            <span><kbd class="palette-kbd">esc</kbd> close</span>
            <span class="palette-count"></span>
          </div>
        </div>
      </div>
    `;
    inputEl = root.querySelector('.palette-input');
    listEl = root.querySelector('.palette-list');
    countEl = root.querySelector('.palette-count');
    inputEl.value = state.q;
    inputEl.focus();
  };

  const renderList = () => {
    if (!listEl) return;

    const hasItems = state.items.length > 0;
    let html = '';
    if (state.loading) {
      html = `<div class="palette-status mono">searching…</div>`;
    } else if (!hasItems && state.q) {
      html = `<div class="palette-status mono">&gt; no results</div>`;
    } else if (hasItems) {
      html = state.items.map((it, i) => renderItem(it, i)).join('');
    }
    listEl.innerHTML = html;

    if (countEl) {
      countEl.textContent = `${state.items.length} result${state.items.length === 1 ? '' : 's'}`;
    }
    if (inputEl) {
      inputEl.setAttribute('aria-activedescendant', hasItems ? `palette-item-${state.active}` : '');
    }
    listEl.querySelector(`[data-palette-idx="${state.active}"]`)?.scrollIntoView({ block: 'nearest' });
  };

  const safeHighlight = (s) => escape(s).replace(/&lt;mark&gt;(.*?)&lt;\/mark&gt;/g, '<mark>$1</mark>');

  const renderItem = (it, i) => {
    const active = i === state.active;
    const title = it.highlights?.title ? safeHighlight(it.highlights.title) : escape(it.title || it.label);
    const meta = it.kind === 'post'
      ? `${(it.tags || []).map(t => `#${t}`).join(' ')} · ${formatDate(it.date)}`
      : it.kind === 'tag'
        ? `${it.post_count} post${it.post_count === 1 ? '' : 's'}`
        : '';
    return `
      <a class="palette-item ${active ? 'is-active' : ''}"
         role="option" id="palette-item-${i}" data-palette-idx="${i}"
         aria-selected="${active}"
         href="${escape(it.url)}">
        <span class="palette-item__icon" aria-hidden="true">${it.kind === 'tag' ? '#' : '→'}</span>
        <span class="palette-item__body">
          <span class="palette-item__title">${title}</span>
          ${meta ? `<span class="palette-item__meta mono">${escape(meta)}</span>` : ''}
        </span>
      </a>
    `;
  };

  // ------- behaviour -------

  let openerEl = null;

  const openPalette = () => {
    openerEl = document.activeElement;
    state.open = true;
    state.q = '';
    state.items = DEFAULTS.slice();
    state.active = 0;
    state.loading = false;
    renderShell();
    renderList();
  };

  const closePalette = () => {
    abortCtl?.abort();
    clearTimeout(debounceTimer);
    state.open = false;
    state.q = '';
    state.items = [];
    state.active = 0;
    state.loading = false;
    root.innerHTML = '';
    inputEl = listEl = countEl = null;
    openerEl?.focus();
    openerEl = null;
  };

  const onQuery = (q) => {
    state.q = q;
    state.active = 0;

    clearTimeout(debounceTimer);
    abortCtl?.abort();

    if (!q.trim()) {
      state.loading = false;
      state.items = DEFAULTS.slice();
      renderList();
      return;
    }

    state.loading = true;
    renderList();

    debounceTimer = setTimeout(async () => {
      abortCtl = new AbortController();
      try {
        const data = await fetch(`/api/search?q=${encodeURIComponent(q)}&limit=8`, {
          signal: abortCtl.signal,
          headers: { Accept: 'application/json' },
        }).then(r => r.json());
        state.items = data.results || [];
        state.loading = false;
        state.active = 0;
        renderList();
      } catch (err) {
        if (err.name === 'AbortError') return;
        state.loading = false;
        renderList();
      }
    }, 180);
  };

  // ------- events -------

  document.addEventListener('keydown', (e) => {
    if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      state.open ? closePalette() : openPalette();
      return;
    }
    if (!state.open) return;

    if (e.key === 'Escape') { e.preventDefault(); closePalette(); return; }
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      if (state.items.length > 0) {
        state.active = (state.active + 1) % state.items.length;
        renderList();
      }
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      if (state.items.length > 0) {
        state.active = (state.active - 1 + state.items.length) % state.items.length;
        renderList();
      }
    } else if (e.key === 'Home') {
      e.preventDefault(); state.active = 0; renderList();
    } else if (e.key === 'End') {
      e.preventDefault(); state.active = Math.max(0, state.items.length - 1); renderList();
    } else if (e.key === 'Enter') {
      e.preventDefault();
      const it = state.items[state.active];
      if (it) window.location.href = it.url;
    }
  });

  root.addEventListener('input', (e) => {
    if (e.target.classList.contains('palette-input')) onQuery(e.target.value);
  });

  root.addEventListener('click', (e) => {
    if (e.target.closest('.palette-backdrop') && !e.target.closest('.palette-dialog')) closePalette();
    if (e.target.closest('.palette-esc')) { e.preventDefault(); closePalette(); }
  });

  root.addEventListener('mousemove', (e) => {
    const item = e.target.closest('[data-palette-idx]');
    if (item) {
      const i = parseInt(item.dataset.paletteIdx, 10);
      if (i !== state.active) { state.active = i; renderList(); }
    }
  });

  document.querySelectorAll('[data-palette-open]').forEach(btn => {
    btn.addEventListener('click', (e) => { e.preventDefault(); openPalette(); });
  });

  // ------- utils -------

  function escape(s) {
    return String(s ?? '').replace(/[&<>"']/g, (c) =>
      ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  }
  function formatDate(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
  }
})();
