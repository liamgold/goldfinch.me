/*!
 * ask.js
 * Site-wide "Ask" — grounded, conversational Q&A over the blog, in a modal opened from the header
 * ([data-ask-open]). Keeps the transcript client-side and POSTs { question, history } to /api/ask,
 * appending each turn to a thread. Progressive enhancement: the modal ships hidden and is only
 * wired here, so with JS disabled nothing is shown and the triggers do nothing.
 */
(() => {
  const modal = document.querySelector('[data-ask]');
  if (!modal) return;

  const dialog = modal.querySelector('.ask-modal__dialog');
  const thread = modal.querySelector('[data-ask-thread]');
  const input = modal.querySelector('[data-ask-input]');
  const submit = modal.querySelector('[data-ask-submit]');
  if (!dialog || !thread || !input || !submit) return;

  const history = [];
  let loading = false;
  let opener = null;

  // ── Open / close ────────────────────────────────────────────────────────
  document.querySelectorAll('[data-ask-open]').forEach((btn) => {
    btn.addEventListener('click', () => open(btn));
  });
  modal.querySelectorAll('[data-ask-close]').forEach((btn) => {
    btn.addEventListener('click', close);
  });

  function open(triggerEl) {
    opener = triggerEl || null;
    modal.hidden = false;
    document.body.style.overflow = 'hidden';
    input.focus();
  }

  function close() {
    modal.hidden = true;
    document.body.style.overflow = '';
    if (opener) opener.focus();
  }

  document.addEventListener('keydown', (e) => {
    if (modal.hidden) return;
    if (e.key === 'Escape') { close(); return; }
    if (e.key === 'Tab') trapFocus(e);
  });

  // Keep focus within the dialog while open.
  function trapFocus(e) {
    const focusable = dialog.querySelectorAll(
      'a[href], button:not([disabled]), input:not([disabled]), [tabindex]:not([tabindex="-1"])');
    if (!focusable.length) return;
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (e.shiftKey && document.activeElement === first) {
      e.preventDefault(); last.focus();
    } else if (!e.shiftKey && document.activeElement === last) {
      e.preventDefault(); first.focus();
    }
  }

  // ── Ask ─────────────────────────────────────────────────────────────────
  submit.addEventListener('click', ask);
  input.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') { e.preventDefault(); ask(); }
  });

  async function ask() {
    const question = input.value.trim();
    if (!question || loading) return;

    loading = true;
    submit.disabled = true;
    input.value = '';

    const answerEl = appendTurn(question);
    answerEl.innerHTML = `<div class="ask__status mono">&gt; thinking…</div>`;

    try {
      const res = await fetch('/api/ask', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ question, history }),
      });

      if (res.status === 429) {
        answerEl.innerHTML = `<div class="ask__status mono">&gt; too many questions — give it a minute.</div>`;
        return;
      }
      if (!res.ok) {
        answerEl.innerHTML = `<div class="ask__status mono">&gt; ask is unavailable right now.</div>`;
        return;
      }

      const data = await res.json();
      const sources = (data.sources || []).filter((s) => s && s.url);
      const sourcesHtml = sources.length
        ? `<div class="ask__sources">
             <span class="ask__sources-label mono">sources</span>
             ${sources.map((s) => `<a class="ask__source" href="${escape(s.url)}" target="_blank" rel="noopener noreferrer">${escape(s.title)}</a>`).join('')}
           </div>`
        : '';
      answerEl.innerHTML = `<div class="ask__answer">${paragraphs(data.answer)}</div>${sourcesHtml}`;

      history.push({ role: 'user', content: question });
      history.push({ role: 'assistant', content: String(data.answer ?? '') });
    } catch (err) {
      answerEl.innerHTML = `<div class="ask__status mono">&gt; something went wrong. try again.</div>`;
    } finally {
      loading = false;
      submit.disabled = false;
      input.focus();
    }
  }

  // Appends a turn — a "you" message (echoed question) and a "goldfinch" message (the reply) —
  // and returns the reply body container so the caller can fill it once the response arrives.
  // Scrolls the thread to the newest turn.
  function appendTurn(question) {
    const turn = document.createElement('div');
    turn.className = 'ask__turn';

    const userMsg = document.createElement('div');
    userMsg.className = 'ask__msg ask__msg--user';
    const userRole = document.createElement('div');
    userRole.className = 'ask__role mono';
    userRole.textContent = 'you';
    const q = document.createElement('div');
    q.className = 'ask__q mono';
    q.textContent = `> ${question}`;
    userMsg.appendChild(userRole);
    userMsg.appendChild(q);

    const botMsg = document.createElement('div');
    botMsg.className = 'ask__msg ask__msg--assistant';
    const botRole = document.createElement('div');
    botRole.className = 'ask__role ask__role--bot mono';
    botRole.textContent = '✦ goldfinch';
    const body = document.createElement('div');
    body.className = 'ask__body';
    botMsg.appendChild(botRole);
    botMsg.appendChild(body);

    turn.appendChild(userMsg);
    turn.appendChild(botMsg);
    thread.appendChild(turn);
    thread.scrollTop = thread.scrollHeight;

    return body;
  }

  // Model output — escape first, then turn blank-line-separated blocks into paragraphs and single
  // newlines into <br>.
  function paragraphs(text) {
    const safe = escape(String(text ?? '').trim());
    if (!safe) return '';
    return safe
      .split(/\n{2,}/)
      .map((block) => `<p>${block.replace(/\n/g, '<br>')}</p>`)
      .join('');
  }

  function escape(s) {
    return String(s ?? '').replace(/[&<>"']/g, (c) =>
      ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  }
})();
