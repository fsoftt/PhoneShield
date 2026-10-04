'use strict';

// Back office: review spam appeals and purge expired data. Signs in with Firebase's REST API; only the uids listed in
// BackOffice:AdminUids get past the API. Everything shown comes from the API and is rendered as text, never as HTML.

const texts = {
  es: {
    title: 'Back office', signOut: 'Cerrar sesión', signInTitle: 'Iniciar sesión', email: 'Correo', password: 'Contraseña',
    signIn: 'Entrar', overview: 'Resumen', accounts: 'Cuentas', spamReports: 'Reportes', contactContributions: 'Contactos aportados',
    pendingAppeals: 'Apelaciones pendientes', overdueAppeals: 'Vencidas', hiddenNumbers: 'Números con nombres ocultos',
    clearedNumbers: 'Números despejados', retentionTitle: 'Datos vencidos',
    retentionText: 'Se borran solos cada día. Borra ahora los usos de cuota de más de un año, las apelaciones resueltas de más de un año y los reportes de más de dos años.',
    purge: 'Borrar datos vencidos ahora', purgeConfirm: '¿Borrar ahora los datos vencidos? No se puede deshacer.',
    purged: 'Borrados: {0} usos de cuota, {1} apelaciones y {2} reportes.', appeals: 'Apelaciones', pending: 'Pendientes',
    approved: 'Aprobadas', rejected: 'Rechazadas', applied: 'Nombres ocultados',
    privacyHint: 'El número nunca se muestra: solo guardamos su hash. Decide con el motivo y lo que dice la comunidad. Responde por correo antes de resolver: al resolver se borran el motivo y el correo.',
    empty: 'No hay apelaciones aquí.', hideNames: 'Ocultar nombres', reviewSpam: 'Revisar spam', created: 'Recibida',
    due: 'Responder antes de', resolved: 'Resuelta', overdue: 'Vencida', noReason: '(sin motivo)', noEmail: 'Sin correo de contacto',
    reply: 'Responder por correo', community: 'Comunidad', spamCount: 'reportes de spam', notSpamCount: 'de “no es spam”',
    savedBy: 'lo tienen guardado', Spam: 'Spam', Identified: 'Identificado', Unknown: 'Sin datos',
    approve: 'Aprobar: no es spam', reject: 'Rechazar', approveConfirm: '¿Aprobar? Los reportes de spam anteriores dejarán de contar y se borrarán el motivo y el correo.',
    rejectConfirm: '¿Rechazar? Los reportes se mantienen y se borrarán el motivo y el correo.', done: 'Listo.',
    forbidden: 'Esta cuenta no tiene acceso al back office.', signInFailed: 'Correo o contraseña incorrectos.',
    notConfigured: 'Falta configurar BackOffice:FirebaseApiKey en el servidor.', error: 'Algo falló. Inténtalo de nuevo.',
  },
  en: {
    title: 'Back office', signOut: 'Sign out', signInTitle: 'Sign in', email: 'Email', password: 'Password', signIn: 'Sign in',
    overview: 'Overview', accounts: 'Accounts', spamReports: 'Reports', contactContributions: 'Contributed contacts',
    pendingAppeals: 'Pending appeals', overdueAppeals: 'Overdue', hiddenNumbers: 'Numbers with hidden names',
    clearedNumbers: 'Cleared numbers', retentionTitle: 'Expired data',
    retentionText: 'Deleted automatically every day. Delete now quota uses older than a year, appeals resolved over a year ago and reports older than two years.',
    purge: 'Delete expired data now', purgeConfirm: 'Delete expired data now? This cannot be undone.',
    purged: 'Deleted: {0} quota uses, {1} appeals and {2} reports.', appeals: 'Appeals', pending: 'Pending',
    approved: 'Approved', rejected: 'Rejected', applied: 'Names hidden',
    privacyHint: 'The number is never shown: only its hash is stored. Decide from the reason and what the community says. Reply by email before resolving: resolving erases the reason and the email.',
    empty: 'No appeals here.', hideNames: 'Hide names', reviewSpam: 'Spam review', created: 'Received',
    due: 'Answer by', resolved: 'Resolved', overdue: 'Overdue', noReason: '(no reason)', noEmail: 'No contact email',
    reply: 'Reply by email', community: 'Community', spamCount: 'spam reports', notSpamCount: '“not spam”',
    savedBy: 'have it saved', Spam: 'Spam', Identified: 'Identified', Unknown: 'No data',
    approve: 'Approve: not spam', reject: 'Reject', approveConfirm: 'Approve? Earlier spam reports will stop counting and the reason and email will be erased.',
    rejectConfirm: 'Reject? The reports stand and the reason and email will be erased.', done: 'Done.',
    forbidden: 'This account has no access to the back office.', signInFailed: 'Wrong email or password.',
    notConfigured: 'BackOffice:FirebaseApiKey is not configured on the server.', error: 'Something went wrong. Try again.',
  },
};

const lang = navigator.language?.toLowerCase().startsWith('es') ? 'es' : 'en';
const t = (key, ...args) => (texts[lang][key] ?? key).replace(/\{(\d)\}/g, (_, i) => String(args[Number(i)]));
const $ = (selector) => document.querySelector(selector);
const sessionKey = 'tranqui.backoffice.session';
let apiKey = '';
let currentStatus = 'Pending';

function setMessage(text) {
  $('#message').textContent = text ?? '';
}

function readSession() {
  try {
    return JSON.parse(sessionStorage.getItem(sessionKey) ?? 'null');
  } catch {
    return null;
  }
}

function saveSession(session) {
  if (session) {
    sessionStorage.setItem(sessionKey, JSON.stringify(session));
  } else {
    sessionStorage.removeItem(sessionKey);
  }
}

async function firebase(url, body, form = false) {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': form ? 'application/x-www-form-urlencoded' : 'application/json' },
    body: form ? new URLSearchParams(body) : JSON.stringify(body),
  });
  if (!response.ok) {
    throw new Error('firebase');
  }

  return response.json();
}

async function signIn(email, password) {
  const body = await firebase(
    `https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=${encodeURIComponent(apiKey)}`,
    { email, password, returnSecureToken: true });
  saveSession({ idToken: body.idToken, refreshToken: body.refreshToken, expiresAt: Date.now() + Number(body.expiresIn) * 1000 });
}

async function freshToken() {
  const session = readSession();
  if (!session) {
    return null;
  }

  if (Date.now() < session.expiresAt - 60_000) {
    return session.idToken;
  }

  const body = await firebase(
    `https://securetoken.googleapis.com/v1/token?key=${encodeURIComponent(apiKey)}`,
    { grant_type: 'refresh_token', refresh_token: session.refreshToken }, true);
  saveSession({ idToken: body.id_token, refreshToken: body.refresh_token, expiresAt: Date.now() + Number(body.expires_in) * 1000 });

  return body.id_token;
}

async function api(path, options = {}) {
  const token = await freshToken();
  const response = await fetch(path, {
    ...options,
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
  });
  if (response.status === 401) {
    saveSession(null);
    show('sign-in');
    throw new Error('unauthorized');
  }

  if (response.status === 403) {
    setMessage(t('forbidden'));
    throw new Error('forbidden');
  }

  if (!response.ok) {
    throw new Error(String(response.status));
  }

  return response.json();
}

function show(view) {
  $('#sign-in').hidden = view !== 'sign-in';
  $('#dashboard').hidden = view !== 'dashboard';
  $('#sign-out').hidden = view !== 'dashboard';
}

function element(tag, text, className) {
  const node = document.createElement(tag);
  if (text !== undefined) {
    node.textContent = text;
  }

  if (className) {
    node.className = className;
  }

  return node;
}

function formatDate(value) {
  return value ? new Date(value).toLocaleString(lang) : '';
}

async function loadOverview() {
  const overview = await api('/v1/admin/overview');
  const list = $('#overview');
  list.replaceChildren();
  for (const [key, value] of Object.entries(overview)) {
    const item = element('div');
    item.append(element('dt', t(key)), element('dd', String(value)));
    list.append(item);
  }
}

function appealCard(appeal) {
  const card = element('article', undefined, 'card');
  const heading = element('h3', `${t(appeal.kind === 'HideNames' ? 'hideNames' : 'reviewSpam')} · ${formatDate(appeal.createdAt)}`);
  card.append(heading);

  if (appeal.isOverdue) {
    card.append(element('span', t('overdue'), 'badge overdue'));
  }

  const dates = appeal.status === 'Pending'
    ? `${t('due')}: ${formatDate(appeal.dueAt)}`
    : `${t('resolved')}: ${formatDate(appeal.resolvedAt ?? appeal.createdAt)}`;
  card.append(element('p', dates, 'muted'));

  if (appeal.status === 'Pending') {
    card.append(element('p', appeal.reason ?? t('noReason'), 'reason'));
    if (appeal.contactEmail) {
      const link = element('a', `${t('reply')}: ${appeal.contactEmail}`);
      link.href = `mailto:${encodeURIComponent(appeal.contactEmail)}`;
      card.append(link);
    } else {
      card.append(element('p', t('noEmail'), 'muted'));
    }
  }

  const community = element('p');
  community.append(
    element('strong', `${t('community')}: `),
    element('span', t(appeal.numberStatus), `badge ${appeal.numberStatus}`),
    document.createTextNode(` ${appeal.spamReportCount} ${t('spamCount')}, ${appeal.notSpamReportCount} ${t('notSpamCount')}, `
      + `${appeal.savedByCount} ${t('savedBy')}`));
  card.append(community);

  if (appeal.status === 'Pending') {
    const actions = element('div', undefined, 'actions');
    const approve = element('button', t('approve'));
    const reject = element('button', t('reject'), 'secondary');
    approve.addEventListener('click', () => resolve(appeal.id, 'Approve', t('approveConfirm')));
    reject.addEventListener('click', () => resolve(appeal.id, 'Reject', t('rejectConfirm')));
    actions.append(approve, reject);
    card.append(actions);
  }

  return card;
}

async function loadAppeals(status = currentStatus) {
  currentStatus = status;
  for (const tab of document.querySelectorAll('[role="tab"]')) {
    tab.setAttribute('aria-selected', String(tab.dataset.status === status));
  }

  const appeals = await api(`/v1/admin/appeals?status=${status}`);
  const container = $('#appeals');
  container.replaceChildren(...(appeals.length ? appeals.map(appealCard) : [element('p', t('empty'), 'muted')]));
}

async function resolve(id, decision, question) {
  if (!confirm(question)) {
    return;
  }

  await run(async () => {
    await api(`/v1/admin/appeals/${encodeURIComponent(id)}/resolution`, { method: 'POST', body: JSON.stringify({ decision }) });
    setMessage(t('done'));
    await Promise.all([loadOverview(), loadAppeals()]);
  });
}

async function run(action) {
  const buttons = document.querySelectorAll('button');
  buttons.forEach((button) => { button.disabled = true; });
  try {
    await action();
  } catch (error) {
    if (!['unauthorized', 'forbidden'].includes(error.message)) {
      setMessage(t('error'));
    }
  } finally {
    buttons.forEach((button) => { button.disabled = false; });
  }
}

async function openDashboard() {
  show('dashboard');
  await run(() => Promise.all([loadOverview(), loadAppeals()]));
}

async function start() {
  document.documentElement.lang = lang;
  for (const node of document.querySelectorAll('[data-i18n]')) {
    node.textContent = t(node.dataset.i18n);
  }

  try {
    apiKey = (await (await fetch('/v1/admin/config')).json()).firebaseApiKey;
  } catch {
    apiKey = '';
  }

  if (!apiKey) {
    setMessage(t('notConfigured'));
    return;
  }

  $('#sign-in').addEventListener('submit', async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await signIn($('#email').value, $('#password').value);
      $('#password').value = '';
      await openDashboard();
    } catch {
      setMessage(t('signInFailed'));
    }
  });

  $('#sign-out').addEventListener('click', () => {
    saveSession(null);
    setMessage('');
    show('sign-in');
  });

  $('#purge').addEventListener('click', async () => {
    if (!confirm(t('purgeConfirm'))) {
      return;
    }

    await run(async () => {
      const result = await api('/v1/admin/purges', { method: 'POST' });
      setMessage(t('purged', result.appealQuotaUsages, result.resolvedAppeals, result.spamReports));
      await loadOverview();
    });
  });

  for (const tab of document.querySelectorAll('[role="tab"]')) {
    tab.addEventListener('click', () => run(() => loadAppeals(tab.dataset.status)));
  }

  if (readSession()) {
    await openDashboard();
  } else {
    show('sign-in');
  }
}

start();
