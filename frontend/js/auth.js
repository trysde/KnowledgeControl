function getToken() {
  return localStorage.getItem('kc_token');
}

function parseJwt(token) {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return null;
  }
}

function getCurrentUser() {
  const token = getToken();
  if (!token) return null;
  const claims = parseJwt(token);
  if (!claims) return null;
  if (claims.exp && claims.exp * 1000 < Date.now()) {
    localStorage.removeItem('kc_token');
    return null;
  }
  return {
    id: claims.sub || claims.nameid,
    email: claims.email || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 'Пользователь',
    role: claims.role || claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'User'
  };
}

function requireAuth() {
  const user = getCurrentUser();
  if (!user) {
    window.location.href = 'login.html';
    return null;
  }
  return user;
}

function logout() {
  localStorage.removeItem('kc_token');
  window.location.href = 'login.html';
}

function renderNavbar(activePage = '') {
  const user = getCurrentUser();
  const nav = document.getElementById('navbar');
  if (!nav) return;

  const isMod = user && (user.role === 'Moderator' || user.role === 'Admin');

  nav.innerHTML = `
    <div class="topbar-inner">
      <div style="display:flex;align-items:center;gap:24px;">
        <a href="index.html" class="brand">KnowledgeControl <span class="brand-tag">LMS</span></a>
        <nav class="nav-links">
          <a href="index.html" class="${activePage === 'index' ? 'active' : ''}">Каталог</a>
          <a href="search.html" class="${activePage === 'search' ? 'active' : ''}">Поиск</a>
          ${user ? `
            <a href="tests.html" class="${activePage === 'tests' ? 'active' : ''}">Мои тесты</a>
            <a href="questions.html" class="${activePage === 'questions' ? 'active' : ''}">Вопросы</a>
            ${isMod ? `<a href="moderation.html" class="${activePage === 'moderation' ? 'active' : ''}">Модерация</a>` : ''}
          ` : ''}
        </nav>
      </div>
      <div class="nav-links">
        ${user ? `
          <a href="profile.html" class="${activePage === 'profile' ? 'active' : ''}">${user.email}</a>
          <button class="btn btn-sm" onclick="logout()">Выйти</button>
        ` : `
          <a href="login.html">Вход</a>
          <a href="register.html" class="btn btn-sm btn-primary" style="color:#fff;">Регистрация</a>
        `}
      </div>
    </div>
  `;
}