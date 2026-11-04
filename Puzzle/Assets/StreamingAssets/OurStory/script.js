const data = window.OUR_STORY;
document.getElementById('dedication').textContent = `写给${data.to}，来自${data.from}`;
document.getElementById('salutation').textContent = `亲爱的${data.to}：`;
document.getElementById('signature').textContent = `一直在这里的 ${data.from}`;
data.letter.forEach(text => { const p = document.createElement('p'); p.textContent = text; document.getElementById('letter-body').append(p); });
const modal = document.getElementById('memory-dialog');
data.memories.forEach((memory, index) => {
  const card = document.createElement('button'); card.className = 'memory';
  const art = document.createElement('div'); art.className = `memory-art ${memory.theme}`;
  if (memory.photo) { const img = document.createElement('img'); img.src = memory.photo; img.alt = memory.title; art.append(img); }
  else { const caption = document.createElement('span'); caption.textContent = '待放入我们的照片'; art.append(caption); }
  const number = document.createElement('span'); number.className = 'memory-number'; number.textContent = `0${index + 1}`; art.append(number);
  const date = document.createElement('p'); date.className = 'eyebrow'; date.textContent = memory.date;
  const title = document.createElement('h3'); title.textContent = memory.title;
  const description = document.createElement('p'); description.className = 'memory-description'; description.textContent = memory.summary;
  const link = document.createElement('span'); link.className = 'memory-link'; link.textContent = '读这段回忆 ↗';
  card.append(art, date, title, description, link);
  card.addEventListener('click', () => { document.getElementById('dialog-date').textContent = memory.date; document.getElementById('dialog-title').textContent = memory.title; document.getElementById('dialog-body').textContent = memory.text; modal.showModal(); });
  document.getElementById('memories').append(card);
});
document.getElementById('close-dialog').addEventListener('click', () => modal.close());
modal.addEventListener('click', event => { if (event.target === modal) { const r = modal.getBoundingClientRect(); if (event.clientX < r.left || event.clientX > r.right || event.clientY < r.top || event.clientY > r.bottom) modal.close(); } });
data.wishes.forEach(text => { const item = document.createElement('span'); item.textContent = `♡  ${text}`; document.getElementById('wishes').append(item); });
