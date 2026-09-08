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

    const defaultLabel = btn.dataset.labelDefault || 'Copy code';
    const copiedLabel = btn.dataset.labelCopied || 'Copied';

    btn.addEventListener('click', () => {
      navigator.clipboard.writeText(code.textContent || '').then(() => {
        btn.dataset.copied = 'true';
        btn.setAttribute('aria-label', copiedLabel);
        setTimeout(() => {
          delete btn.dataset.copied;
          btn.setAttribute('aria-label', defaultLabel);
        }, 2000);
      }).catch(() => {});
    });
  });
};

export default initialise;
