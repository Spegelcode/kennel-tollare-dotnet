const menu = document.querySelector('.menu-toggle');
const nav = document.querySelector('.nav');
menu?.addEventListener('click', () => { const open = nav.classList.toggle('open'); menu.setAttribute('aria-expanded', open); });

const panels = document.querySelectorAll('.tab-panel');
const links = document.querySelectorAll('[data-tab]');
function showTab(name) {
  panels.forEach(panel => {
    const active = panel.id === name;
    panel.classList.toggle('active', active);
    panel.setAttribute('aria-hidden', String(!active));
  });
  links.forEach(link => link.classList.toggle('current', link.dataset.tab === name));
  nav?.classList.remove('open');
  window.scrollTo({ top: 0, behavior: 'smooth' });
}
links.forEach(link => link.addEventListener('click', event => { event.preventDefault(); showTab(link.dataset.tab); history.replaceState(null, '', `#${link.dataset.tab}`); }));
const initial = window.location.hash.slice(1);
showTab(initial && document.getElementById(initial) ? initial : 'start');

document.querySelectorAll('.server-litter-panel').forEach(panel => {
  const slides = panel.querySelectorAll('.puppies-slide');
  const dots = panel.querySelectorAll('.carousel-dot');
  let current = 0;
  const showSlide = index => {
    current = (index + slides.length) % slides.length;
    slides.forEach((slide, i) => slide.classList.toggle('active', i === current));
    dots.forEach((dot, i) => dot.classList.toggle('active', i === current));
  };
  panel.querySelector('.carousel-arrow.prev')?.addEventListener('click', () => showSlide(current - 1));
  panel.querySelector('.carousel-arrow.next')?.addEventListener('click', () => showSlide(current + 1));
  dots.forEach(dot => dot.addEventListener('click', () => showSlide(Number(dot.dataset.slide))));
});

const litterTabs = document.querySelectorAll('.litter-tab');
const litterPanels = document.querySelectorAll('.server-litter-panel');
litterTabs.forEach(tab => tab.addEventListener('click', () => {
  const index = tab.dataset.index;
  litterTabs.forEach(item => { const selected = item === tab; item.classList.toggle('active', selected); item.setAttribute('aria-selected', selected); });
  litterPanels.forEach(panel => panel.classList.toggle('active', panel.dataset.litterPanel === index));
}));

document.querySelectorAll('.daily-item').forEach(item => item.addEventListener('click', () => {
  document.querySelectorAll('.daily-item').forEach(other => { if (other !== item) { other.classList.remove('active'); other.querySelector('b').textContent = '+'; } });
  item.classList.toggle('active'); item.querySelector('b').textContent = item.classList.contains('active') ? '−' : '+';
}));
