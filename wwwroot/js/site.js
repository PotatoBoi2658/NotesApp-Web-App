// basic theme toggle using CSS custom properties
(function () {
  const root = document.documentElement;
  const toggle = document.getElementById('themeToggle');
  if (!toggle) return;

  const themes = {
    default: {
      '--brand': '#2b6cb0',
      '--brand-contrast': '#ffffff',
      '--accent': '#f6ad55',
      '--bg': '#f4f7fb',
      '--card-bg': '#ffffff'
    },
    dark: {
      '--brand': '#0b5a7a',
      '--brand-contrast': '#ffffff',
      '--accent': '#f6ad55',
      '--bg': '#0f1724',
      '--card-bg': '#0b1220'
    }
  };

  function applyTheme(name) {
    const t = themes[name] || themes.default;
    Object.entries(t).forEach(([k, v]) => root.style.setProperty(k, v));
    localStorage.setItem('notesapp-theme', name);
  }

  // init
  const saved = localStorage.getItem('notesapp-theme') || 'default';
  applyTheme(saved);

  toggle.addEventListener('click', () => {
    const current = localStorage.getItem('notesapp-theme') || 'default';
    const next = current === 'default' ? 'dark' : 'default';
    applyTheme(next);
  });
})();