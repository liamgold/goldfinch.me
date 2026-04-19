/*!
 * toc-scrollspy.js
 * Post detail — dynamically builds the TOC from h2[id]/h3[id] headings in the
 * article, highlights the active entry as the reader scrolls, and updates the
 * reading-progress bar.
 *
 * Progressive enhancement: with JS off, the TOC renders as a plain <nav>
 * with no links (hidden by CSS). Scrollspy and progress bar require JS.
 */
(() => {
  const toc = document.querySelector('nav.toc-rail');
  const article = document.querySelector('article.post-article');
  const fill = document.querySelector('.reading-progress__fill');
  if (!toc || !article) return;

  // Build TOC links dynamically from headings in the article.
  const headings = [...article.querySelectorAll('h2[id], h3[id]')];
  if (headings.length > 0) {
    const ul = document.createElement('ul');
    headings.forEach((h, i) => {
      const li = document.createElement('li');
      const a = document.createElement('a');
      a.href = `#${h.id}`;
      a.innerHTML = `<span class="idx">${String(i + 1).padStart(2, '0')}</span>${h.textContent}`;
      li.appendChild(a);
      ul.appendChild(li);
    });
    toc.appendChild(ul);
  }

  // Scrollspy
  const links = new Map(
    [...toc.querySelectorAll('a[href^="#"]')].map(a => [a.getAttribute('href').slice(1), a])
  );

  if (headings.length > 0 && links.size > 0) {
    const setActive = (id) => {
      links.forEach(a => a.classList.remove('is-active'));
      if (id) links.get(id)?.classList.add('is-active');
    };

    // Suppress scroll-driven updates while smooth-scroll from a click is in flight.
    let suppressScroll = false;
    let suppressTimer = null;

    links.forEach((a, id) => {
      a.addEventListener('click', () => {
        setActive(id);
        suppressScroll = true;
        clearTimeout(suppressTimer);
        suppressTimer = setTimeout(() => { suppressScroll = false; }, 800);
      });
    });

    // Scroll-based active detection — finds the last heading that has scrolled
    // past the top threshold. Handles all directions and deactivates when
    // scrolled back above all headings.
    const OFFSET = 80 + 16; // sticky header height + buffer, matches scroll-margin-top
    const getActiveId = () => {
      let active = null;
      for (const h of headings) {
        if (h.getBoundingClientRect().top <= OFFSET) {
          active = h.id;
        }
      }
      return active;
    };

    let rafPending = false;
    window.addEventListener('scroll', () => {
      if (suppressScroll || rafPending) return;
      rafPending = true;
      requestAnimationFrame(() => {
        setActive(getActiveId());
        rafPending = false;
      });
    }, { passive: true });
  }

  // Reading progress
  if (fill) {
    const update = () => {
      const rect = article.getBoundingClientRect();
      const vh = window.innerHeight;
      const total = rect.height - vh;
      const scrolled = Math.min(Math.max(-rect.top, 0), Math.max(total, 1));
      const pct = Math.min(1, total > 0 ? scrolled / total : 1);
      fill.style.transform = `scaleX(${pct})`;
    };
    update();
    window.addEventListener('scroll', update, { passive: true });
    window.addEventListener('resize', update);
  }
})();
