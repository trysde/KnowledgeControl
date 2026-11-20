const SERVICE_URLS = {
  user: 'http://localhost:5001',
  creation: 'http://localhost:5002',
  taking: 'http://localhost:5003',
  search: 'http://localhost:5004'
};

async function apiRequest(service, path, options = {}) {
  const baseUrl = SERVICE_URLS[service] || '';
  const token = localStorage.getItem('kc_token');

  const headers = {
    ...(options.body !== undefined ? { 'Content-Type': 'application/json' } : {}),
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...(options.headers || {})
  };

  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined
  });

  if (response.status === 204) return null;

  const data = await response.json().catch(() => null);

  if (response.status === 401) {
    localStorage.removeItem('kc_token');
    localStorage.removeItem('kc_nickname');
    if (!window.location.pathname.endsWith('login.html')) {
      window.location.href = 'login.html';
      throw new Error('Требуется авторизация');
    }
    const msg = data?.error?.message || 'Неверный email или пароль.';
    throw new Error(msg);
  }

  if (!response.ok) {
    if (data && data.error) {
      const detailsText = Array.isArray(data.error.details) && data.error.details.length
        ? ': ' + data.error.details.map(d => (typeof d === 'string' ? d : d.message)).join('; ')
        : '';
      throw new Error(`${data.error.message || 'Ошибка запроса'}${detailsText}`);
    }
    throw new Error(`HTTP ${response.status}`);
  }

  return data;
}

function showAlert(elementId, message, type = 'error') {
  const el = document.getElementById(elementId);
  if (!el) return;
  el.textContent = message;
  el.className = `alert alert-${type} show`;
}

function hideAlert(elementId) {
  const el = document.getElementById(elementId);
  if (!el) return;
  el.className = 'alert';
}

function escapeHtml(value) {
  if (value === null || value === undefined) return '';
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}