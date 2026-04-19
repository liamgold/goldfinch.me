/*!
 * command-palette.js
 * ⌘K / Ctrl+K opens a dialog with a search input and keyboard-navigable
 * result list. Results come from GET /api/search. Each result is a real
 * URL — Enter just follows the link, so there's no SPA routing.
 *
 * Progressive enhancement: with JS off, ⌘K does nothing; the search
 * form on the blog archive (non-JS) is the fallback.
 */
(() => {
  const root = document.getElementById('palette-root');
  if (!root) return;

  let state = { open: false, q: '', items: [], active: 0, loading: false };
  let abortCtl = null;
  let debounceTimer = null;

  // ------- rendering -------

  const render = () => {
    if (!state.open) { root.innerHTML = ''; return; }
    const hasItems = state.items.length > 0;
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
              aria-controls="palette-listbox"
              ${hasItems ? `aria-activedescendant="palette-item-${state.active}"` : ''}
              value="${escape(state.q)}">
            <button class="palette-esc" type="button" aria-label="Close">ESC</button>
          </div>
          <div class="palette-list" role="listbox" id="palette-listbox">
            ${state.loading ? `<div class="palette-status mono">searching…</div>` : ''}
            ${!state.loading && !hasItems && state.q ? `<div class="palette-status mono">&gt; no results</div>` : ''}
            ${!state.loading && !hasItems && !state.q ? renderDefaults() : ''}
            ${hasItems ? state.items.map((it, i) => renderItem(it, i)).join('') : ''}
          </div>
          <div class="palette-footer mono">
            <span><kbd class="palette-kbd">↑</kbd><kbd class="palette-kbd">↓</kbd> move</span>
            <span><kbd class="palette-kbd">↵</kbd> select</span>
            <span><kbd class="palette-kbd">esc</kbd> close</span>
            <span class="palette-count">${state.items.length} result${state.items.length === 1 ? '' : 's'}</span>
          </div>
        </div>
      </div>
    `;
    root.querySelector('.palette-input')?.focus();
    root.querySelector(`[data-palette-idx="${state.active}"]`)?.scrollIntoView({ block: 'nearest' });
  };

  const renderItem = (it, i) => {
    const active = i === state.active;
    const title = it.highlights?.title || escape(it.title || it.label);
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

  const renderDefaults = () => {
    const pages = [
      { url: '/', label: 'Home' },
      { url: '/blog', label: 'Blog' },
      { url: '/about', label: 'About' },
      { url: '/speaking', label: 'Speaking' },
    ];
    state.items = pages.map(p => ({ kind: 'page', url: p.url, title: `Go to ${p.label}` }));
    return state.items.map((it, i) => renderItem(it, i)).join('');
  };

  // ------- behaviour -------

  const openPalette = () => {
    state = { open: true, q: '', items: [], active: 0, loading: false };
    render();
  };

  const closePalette = () => {
    abortCtl?.abort();
    clearTimeout(debounceTimer);
    state.open = false;
    render();
  };

  const onQuery = (q) => {
    state.q = q;
    state.active = 0;
    clearTimeout(debounceTimer);
    if (!q.trim()) { state.loading = false; state.items = []; render(); return; }
    state.loading = true;
    render();
    debounceTimer = setTimeout(async () => {
      abortCtl?.abort();
      abortCtl = new AbortController();
      try {
        const data = await fetch(`/api/search?q=${encodeURIComponent(q)}&limit=8`, {
          signal: abortCtl.signal,
          headers: { Accept: 'application/json' },
        }).then(r => r.json());
        state.items = data.results || [];
        state.loading = false;
        state.active = 0;
        render();
      } catch (err) {
        if (err.name === 'AbortError') return;
        state.loading = false;
        render();
      }
    }, 180);
  };

  // ------- events -------

  document.addEventListener('keydown', (e) => {
    // Open: Cmd+K / Ctrl+K
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
        render();
      }
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      if (state.items.length > 0) {
        state.active = (state.active - 1 + state.items.length) % state.items.length;
        render();
      }
    } else if (e.key === 'Home') {
      e.preventDefault(); state.active = 0; render();
    } else if (e.key === 'End') {
      e.preventDefault(); state.active = Math.max(0, state.items.length - 1); render();
    } else if (e.key === 'Enter') {
      e.preventDefault();
      const it = state.items[state.active];
      if (it) window.location.href = it.url;
    }
  });

  // Click handlers are delegated from #palette-root
  root.addEventListener('input', (e) => {
    if (e.target.classList.contains('palette-input')) onQuery(e.target.value);
  });
  root.addEventListener('click', (e) => {
    if (e.target.closest('.palette-backdrop') && !e.target.closest('.palette-dialog')) closePalette();
    if (e.target.closest('.palette-esc')) { e.preventDefault(); closePalette(); }
    const item = e.target.closest('[data-palette-idx]');
    if (item) { /* let the <a> handle the navigation itself */ }
  });
  root.addEventListener('mousemove', (e) => {
    const item = e.target.closest('[data-palette-idx]');
    if (item) {
      const i = parseInt(item.dataset.paletteIdx, 10);
      if (i !== state.active) { state.active = i; render(); }
    }
  });

  // Also wire header trigger buttons if present
  document.querySelectorAll('[data-palette-open]').forEach(btn => {
    btn.addEventListener('click', (e) => { e.preventDefault(); openPalette(); });
  });

  // ------- utils -------
  function escape(s) {
    return String(s ?? '').replace(/[&<>"']/g, (c) =>
      ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  }
  function formatDate(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
  }
})();
