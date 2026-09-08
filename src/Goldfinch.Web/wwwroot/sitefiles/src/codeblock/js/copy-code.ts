/*!
 * copy-code.ts
 * Reveals a copy button on each code block when navigator.clipboard is
 * available, then writes the block's raw source (not the highlighted
 * markup) to the clipboard on click.
 */
const initialise = () => {
  if (!navigator.clipboard) return;

  document.querySelectorAll<HTMLElement>('.js-codeblock').forEach((pre) => {
    const code = pre.querySelector('code');
    const btn = pre.querySelector<HTMLButtonElement>('.codeblock-copy-btn');
    if (!code || !btn) return;

    btn.hidden = false;

    btn.addEventListener('click', () => {
      navigator.clipboard.writeText(code.textContent || '').then(() => {
        btn.dataset.copied = 'true';
        setTimeout(() => delete btn.dataset.copied, 2000);
      });
    });
  });
};

export default initialise;
