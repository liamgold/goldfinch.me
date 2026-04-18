/*!
 * toc-scrollspy.js
 * Post detail — highlights the active TOC entry as the reader scrolls,
 * and updates the reading-progress bar. Loads only on post-detail pages.
 *
 * Progressive enhancement: with JS off, the TOC renders as a plain
 * <nav> with anchor links; no highlight, no progress bar movement.
 */
(() => {
  if (!('IntersectionObserver' in window)) return;

  const toc = document.querySelector('nav.toc-rail');
  const article = document.querySelector('article.post-article');
  const fill = document.querySelector('.reading-progress__fill');
  if (!toc || !article) return;

  // Scrollspy
  const headings = [...article.querySelectorAll('h2[id], h3[id]')];
  const links = new Map(
    [...toc.querySelectorAll('a[href^="#"]')].map(a => [a.getAttribute('href').slice(1), a])
  );
  if (headings.length === 0 || links.size === 0) {
    // Still wire progress bar
  } else {
    const setActive = (id) => {
      links.forEach(a => a.classList.remove('is-active'));
      links.get(id)?.classList.add('is-active');
    };

    const io = new IntersectionObserver((entries) => {
      // Pick the heading closest to the top inside the observation margin
      const visible = entries
        .filter(e => e.isIntersecting)
        .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);
      if (visible[0]) setActive(visible[0].target.id);
    }, {
      rootMargin: '-88px 0px -70% 0px',
      threshold: 0,
    });
    headings.forEach(h => io.observe(h));
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
