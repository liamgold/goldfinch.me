/*!
 * mobile-drawer.js
 * Hamburger → slide-in drawer. Progressive enhancement: with JS off,
 * the drawer stays hidden and the hamburger button is hidden too
 * (the desktop nav is visible at all widths via the .nav-desktop
 * no-JS fallback in styles.css).
 */
(() => {
  const btn = document.querySelector('[data-drawer-toggle]');
  const drawer = document.getElementById('mobile-drawer');
  if (!btn || !drawer) return;

  const FOCUSABLE = 'a[href], button, [tabindex]:not([tabindex="-1"]), input, select';
  let lastFocus = null;

  const open = () => {
    lastFocus = document.activeElement;
    drawer.hidden = false;
    // next frame so the CSS transition runs
    requestAnimationFrame(() => drawer.classList.add('is-open'));
    btn.setAttribute('aria-expanded', 'true');
    document.body.style.overflow = 'hidden';
    // Focus first link
    drawer.querySelector(FOCUSABLE)?.focus();
    document.addEventListener('keydown', onKey);
  };

  const close = () => {
    drawer.classList.remove('is-open');
    btn.setAttribute('aria-expanded', 'false');
    document.body.style.overflow = '';
    // Match the CSS transition duration (220ms)
    setTimeout(() => { drawer.hidden = true; }, 220);
    document.removeEventListener('keydown', onKey);
    lastFocus?.focus();
  };

  const onKey = (e) => {
    if (e.key === 'Escape') {
      e.preventDefault();
      close();
      return;
    }
    // Focus trap
    if (e.key === 'Tab') {
      const items = [...drawer.querySelectorAll(FOCUSABLE)].filter(el => !el.disabled);
      if (items.length === 0) return;
      const first = items[0];
      const last = items[items.length - 1];
      if (e.shiftKey && document.activeElement === first) {
        e.preventDefault(); last.focus();
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault(); first.focus();
      }
    }
  };

  btn.addEventListener('click', () => {
    drawer.hidden ? open() : close();
  });

  // Backdrop click (the <aside> itself is clickable; its inner panel stops propagation)
  drawer.addEventListener('click', (e) => {
    if (e.target === drawer) close();
  });

  // Close when navigating to a same-page link
  drawer.addEventListener('click', (e) => {
    const link = e.target.closest('a[href]');
    if (link && !link.hasAttribute('data-keep-drawer')) close();
  });
})();
