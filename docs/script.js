const menu = document.querySelector('.menu-toggle');
const nav = document.querySelector('.nav');
menu?.addEventListener('click', () => {
  const open = nav.classList.toggle('open');
  menu.setAttribute('aria-expanded', open);
});
const panels = document.querySelectorAll('.tab-panel');
const tabLinks = document.querySelectorAll('[data-tab]');

function showTab(tabName) {
  panels.forEach(panel => panel.classList.toggle('active', panel.id === tabName));
  tabLinks.forEach(link => link.classList.toggle('current', link.dataset.tab === tabName));
  nav.classList.remove('open');
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

tabLinks.forEach(link => link.addEventListener('click', event => {
  event.preventDefault();
  showTab(link.dataset.tab);
  history.replaceState(null, '', `#${link.dataset.tab}`);
}));

const initialTab = window.location.hash.slice(1);
if (initialTab && document.getElementById(initialTab)?.classList.contains('tab-panel')) showTab(initialTab);

const savedAdminData = localStorage.getItem('skogsvikensAdminData');
if (savedAdminData) {
  try {
    const data = JSON.parse(savedAdminData);
    const femaleCard = document.querySelector('.dog-iris');
    const maleCard = document.querySelector('.dog-loke');
    const litterCard = document.querySelector('.dog-saga');
    if (femaleCard && data.femaleImage) femaleCard.style.backgroundImage = `url(${data.femaleImage})`;
    if (maleCard && data.maleImage) maleCard.style.backgroundImage = `url(${data.maleImage})`;
    if (litterCard && data.litterImage) litterCard.style.backgroundImage = `url(${data.litterImage})`;
    if (data.femaleName) document.querySelector('.dog-iris + .dog-info h3').textContent = data.femaleName;
    if (data.maleName) document.querySelector('.dog-loke + .dog-info h3').textContent = data.maleName;
    if (data.litters?.length) {
      const orderedLitters = [...data.litters].sort((a, b) => (b.createdAt || 0) - (a.createdAt || 0));
      const latest = orderedLitters[0];
      const litterContent = document.querySelector('#kullar .litter-content');
      if (litterContent) {
        litterContent.querySelector('h2').innerHTML = `${latest.title}<br><em>${latest.year}</em>`;
        litterContent.querySelector('.status').innerHTML = `<span></span> Kull · ${latest.year}`;
        const description = litterContent.querySelector('.status').nextElementSibling.nextElementSibling;
        if (latest.description) description.textContent = latest.description;
        const detailValues = litterContent.querySelectorAll('.litter-details strong');
        if (detailValues[0]) detailValues[0].textContent = latest.parents;
        if (detailValues[1]) detailValues[1].textContent = latest.count;
        if (detailValues[2]) detailValues[2].textContent = latest.year;
      }
      if (litterContent && data.litterImage) document.querySelector('#kullar .litter-photo').style.backgroundImage = `url(${data.litterImage})`;
      const list = document.querySelector('#public-litter-list');
      if (list) list.innerHTML = `<div class="public-litters-title">Alla kullar</div>${orderedLitters.map(litter => `<article><strong>${litter.title}</strong><span>${litter.year} · ${litter.parents} · ${litter.count}</span>${litter.description ? `<p>${litter.description}</p>` : ''}</article>`).join('')}`;
    }
  } catch (error) { console.warn('Kunde inte läsa sparad admin-data.', error); }
}

function applyEditableContent() {
  const saved = JSON.parse(localStorage.getItem('skogsvikensAdminData') || '{}');
  const site = saved.site;
  if (site) {
    document.querySelectorAll('.brand strong').forEach(element => element.textContent = site.name);
    const setText = (selector, value) => { const element = document.querySelector(selector); if (element && value) element.textContent = value; };
    setText('.hero .eyebrow', site.eyebrow); setText('.hero h1', site.heroTitle); setText('.hero-intro', site.heroIntro); setText('.intro h2', site.aboutTitle); setText('.intro-text p', site.aboutText); setText('.contact h2', site.contactTitle); setText('.contact-copy>p:first-child', site.contactText); if (site.heroImage) document.querySelector('.hero-photo').style.backgroundImage = `linear-gradient(0deg,rgba(18,34,25,.1),transparent 55%),url('${site.heroImage}')`; if (site.dailyImage) document.querySelector('.daily-image').style.backgroundImage = `url('${site.dailyImage}')`;
    const email = document.querySelector('.contact-copy a[href^="mailto:"]'); if (email && site.email) { email.href = `mailto:${site.email}`; email.childNodes[0].textContent = site.email + ' '; }
  }
  if (saved.femaleName) { const article = document.querySelector('.dog-iris')?.parentElement; if (article) { article.querySelector('.dog-number').textContent = `01 — TIK`; article.querySelector('h3').textContent = saved.femaleName; article.querySelector('p').textContent = saved.femaleBio; article.querySelector('.tags').innerHTML = `<span>${saved.femaleYear || ''}</span><span>${(saved.femaleTags || '').replaceAll(',', '</span><span>')}</span>`; } }
  if (saved.maleName) { const article = document.querySelector('.dog-loke')?.parentElement; if (article) { article.querySelector('.dog-number').textContent = `02 — HANE`; article.querySelector('h3').textContent = saved.maleName; article.querySelector('p').textContent = saved.maleBio; article.querySelector('.tags').innerHTML = `<span>${saved.maleYear || ''}</span><span>${(saved.maleTags || '').replaceAll(',', '</span><span>')}</span>`; } }
}
applyEditableContent();

function renderLitterTabs() {
  const menu = document.querySelector('#litter-menu');
  const view = document.querySelector('#litter-view');
  if (!menu || !view) return;
  const stored = JSON.parse(localStorage.getItem('skogsvikensAdminData') || '{}');
  const litters = [...(stored.litters || [{title:'Första kullen',year:'2024',parents:'Iris & Loke',count:'6 valpar',description:'Vår första kull växte upp nära familjen.'}])].sort((a,b)=>(b.createdAt||0)-(a.createdAt||0));
  const esc = value => String(value ?? '').replace(/[&<>"']/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));
  const defaultFemale = stored.femaleImage || 'images/iris-tik.png';
  const defaultMale = stored.maleImage || 'images/loke-hane.png';
  const defaultLitter = stored.litterImage || 'images/iris-kull.png';
  const defaultLitterImages = ['images/iris-kull.png','images/kull-lek.png','images/kull-ute.png','images/kull-sov.png','images/kull-skog.png','images/kull-mys.png','images/kull-sjo.png'];
  let selected = 0;
  const paint = () => {
    const litter = litters[selected] || litters[0];
    menu.innerHTML = litters.map((item,index) => `<button class="litter-tab ${index===selected?'active':''}" role="tab" aria-selected="${index===selected}" data-index="${index}">Kull ${litters.length-index}<small>${esc(item.year)}</small></button>`).join('');
    const images = litter.litterImages?.length ? litter.litterImages : (litter.litterImage ? [litter.litterImage, ...defaultLitterImages.filter(image => image !== litter.litterImage)] : defaultLitterImages);
    const slides = images.map((image,index) => `<div class="puppies-slide ${index===0?'active':''}" style="background-image:url('${image}')" role="img" aria-label="Bild ${index+1} av kullen"></div>`).join('');
    const dots = images.map((_,index) => `<button class="carousel-dot ${index===0?'active':''}" data-slide="${index}" aria-label="Visa bild ${index+1}"></button>`).join('');
    view.innerHTML = `<div class="litter-intro"><span class="status"><span></span> ${esc(litter.year)} · ${esc(litter.title)}</span><h3>${esc(litter.title)}</h3><p>${esc(litter.description || 'En trygg kulluppväxt nära människor, natur och vardagsliv.')}</p></div><div class="parent-pair"><article class="parent-card"><div class="parent-photo" style="background-image:url('${litter.femaleImage || defaultFemale}')"></div><div><span>TIK</span><h4>${esc(litter.femaleName || stored.femaleName || 'Skogsvikens Iris')}</h4><p>${esc(stored.femaleBio || '')}</p></div></article><article class="parent-card"><div class="parent-photo" style="background-image:url('${litter.maleImage || defaultMale}')"></div><div><span>HANE</span><h4>${esc(litter.maleName || stored.maleName || 'Rävrikets Loke')}</h4><p>${esc(stored.maleBio || '')}</p></div></article></div><div class="puppies-row"><div class="puppies-carousel"><div class="carousel-track">${slides}</div>${images.length>1?`<button class="carousel-arrow prev" aria-label="Föregående bild">‹</button><button class="carousel-arrow next" aria-label="Nästa bild">›</button><div class="carousel-dots">${dots}</div><span class="carousel-count">${images.length} bilder</span>`:''}</div><div class="puppies-info"><span class="eyebrow">Valparna</span><h4>Kullen i siffror</h4><div class="puppy-stats"><div><strong>${esc(litter.femalePuppies || (litter.count ? '3' : '0'))}</strong><span>Tikvalpar</span></div><div><strong>${esc(litter.malePuppies || (litter.count ? '3' : '0'))}</strong><span>Hanvalpar</span></div><div><strong>${esc(litter.count || '6 valpar')}</strong><span>Totalt</span></div></div><a class="button button-dark" href="#kontakt" data-tab="kontakt">Fråga om kullen <span>↗</span></a></div></div>`;
    let currentSlide = 0;
    const showSlide = index => { currentSlide = (index + images.length) % images.length; view.querySelectorAll('.puppies-slide').forEach((slide,i) => slide.classList.toggle('active', i === currentSlide)); view.querySelectorAll('.carousel-dot').forEach((dot,i) => dot.classList.toggle('active', i === currentSlide)); };
    view.querySelector('.carousel-arrow.prev')?.addEventListener('click', () => showSlide(currentSlide - 1));
    view.querySelector('.carousel-arrow.next')?.addEventListener('click', () => showSlide(currentSlide + 1));
    view.querySelectorAll('.carousel-dot').forEach(dot => dot.addEventListener('click', () => showSlide(Number(dot.dataset.slide))));
    menu.querySelectorAll('.litter-tab').forEach(button => button.addEventListener('click', () => { selected = Number(button.dataset.index); paint(); }));
    view.querySelectorAll('[data-tab]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); showTab(link.dataset.tab); history.replaceState(null,'','#'+link.dataset.tab); }));
  };
  paint();
}
renderLitterTabs();

document.querySelectorAll('.daily-item').forEach(item => {
  item.addEventListener('click', () => {
    document.querySelectorAll('.daily-item').forEach(other => {
      if (other !== item) { other.classList.remove('active'); other.querySelector('b').textContent = '+'; }
    });
    item.classList.toggle('active');
    item.querySelector('b').textContent = item.classList.contains('active') ? '−' : '+';
  });
});
