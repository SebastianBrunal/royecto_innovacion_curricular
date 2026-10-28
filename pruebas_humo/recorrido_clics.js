// ==============================================================
// Recorrido con CLICS REALES por las 7 pantallas — opcional.
//
// humo_front.py no puede hacer clics: Blazor Server los manda por una
// conexión persistente, no como peticiones sueltas. Un navegador de verdad
// sí puede. Esto automatiza el recorrido «a mano» del 7_quickstart.md:
// agregar, ver, la ficha completa a medio llenar (se rechaza y NO borra lo
// escrito), «solo lo que cambié» (guarda), y retirar con confirmación.
//
// Requiere Node y Playwright:  npm i -D playwright && npx playwright install chromium
// Uso (con el sistema arriba):  node pruebas_humo/recorrido_clics.js [carpeta_capturas]
// ==============================================================
const { chromium } = require('playwright');
const OUT = process.argv[2];   // si se da, ahí quedan las capturas
const FRONT = 'http://localhost:8073';

const PANTALLAS = [
  { ruta: '/aliados', id: '881122', campos: ['Fundación Clic', 'Pedro Pérez', 'pedro@clic.co', '604 111', 'Cali'], tipoId: 'number' },
  { ruta: '/areas-conocimiento', id: 'X9001', campos: ['Gran área clic', 'Área clic', 'Disciplina clic'] },
  { ruta: '/universidades', id: '881122', campos: ['Universidad clic', 'Extensión', 'Pasto'] },
  { ruta: '/aspectos-normativos', id: '881122', campos: ['Ley', 'Ley de educación', 'Ley 30 de 1992'] },
  { ruta: '/practicas-estrategias', id: '881122', campos: ['Práctica', 'Laboratorio', 'Trabajo práctico'] },
  { ruta: '/enfoques', id: '881122', campos: ['Por competencias', 'Formación por competencias'] },
  { ruta: '/caracteristicas-innovacion', id: '881122', campos: ['Flexibilidad', 'Ruta de formación elegible', 'Curricular'] },
];

let fallos = 0;
const ok = (c, m) => { console.log((c ? '[OK]    ' : '[FALLO] ') + m); if (!c) fallos++; };

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
  page.on('dialog', d => d.accept());   // el confirm() de «Retirar»

  for (const p of PANTALLAS) {
    console.log(`\n=== ${p.ruta} ===`);
    await page.goto(FRONT + p.ruta);
    await page.waitForSelector('h1');
    await page.waitForTimeout(1500);    // que el circuito de Blazor conecte

    // AGREGAR
    await page.getByRole('button', { name: 'Agregar' }).first().click();
    const form = page.locator('.tarjeta-formulario');
    const inputs = form.locator('input, textarea');
    await inputs.nth(0).fill(p.id);
    for (let i = 0; i < p.campos.length; i++) await inputs.nth(i + 1).fill(p.campos[i]);
    await form.getByRole('button', { name: 'Agregar' }).click();
    await page.waitForSelector('.aviso.exito', { timeout: 10000 }).catch(() => {});
    ok(await page.locator('.aviso.exito').count() > 0, 'Agregar desde el formulario');
    ok((await page.locator('table').innerText()).includes(p.campos[0]), '  y la fila nueva aparece en la tabla');

    // VER
    const fila = page.locator('tr', { hasText: p.campos[0] }).first();
    await fila.getByRole('button', { name: 'Ver' }).click();
    await page.waitForSelector('.ficha', { timeout: 10000 }).catch(() => {});
    ok((await page.locator('.ficha').innerText().catch(() => '')).includes(p.campos[0]), 'Ver muestra la ficha');
    if (OUT && p.ruta === '/universidades') await page.screenshot({ path: `${OUT}/ver_universidad.png`, fullPage: true });

    // EDITAR — «solo lo que cambié» con todo en blanco menos un campo
    await page.locator('.ficha').getByRole('button', { name: 'Editar' }).click();
    const inputsE = page.locator('.tarjeta-formulario').locator('input, textarea');
    for (let i = 1; i <= p.campos.length; i++) await inputsE.nth(i).fill('');
    await inputsE.nth(1).fill(p.campos[0] + ' (editado)');

    // la ficha completa con campos en blanco → se rechaza y NO se borra lo escrito
    await page.getByRole('button', { name: 'Guardar la ficha completa' }).click();
    await page.waitForSelector('.aviso.error', { timeout: 10000 }).catch(() => {});
    ok(await page.locator('.aviso.error').count() > 0, '«La ficha completa» a medio llenar se rechaza');
    ok(await inputsE.nth(1).inputValue() === p.campos[0] + ' (editado)', '  y el formulario conserva lo escrito');
    if (OUT && p.ruta === '/enfoques') await page.screenshot({ path: `${OUT}/error_ficha_completa.png`, fullPage: true });

    await page.getByRole('button', { name: 'Guardar solo lo que cambié' }).click();
    await page.waitForSelector('.aviso.exito', { timeout: 10000 }).catch(() => {});
    const tabla = await page.locator('table').innerText();
    ok(tabla.includes(p.campos[0] + ' (editado)') && tabla.includes(p.campos[1]),
       '«Solo lo que cambié» guarda ese campo y deja los demás como estaban');

    // RETIRAR (con confirmación)
    const filaE = page.locator('tr', { hasText: p.campos[0] + ' (editado)' }).first();
    await filaE.getByRole('button', { name: 'Retirar' }).click();
    await page.waitForTimeout(1500);
    ok(!(await page.locator('body').innerText()).includes(p.campos[0] + ' (editado)') ||
       (await page.locator('.aviso.exito').innerText()).startsWith('Se retiró'),
       'Retirar (tras confirmar) la saca del listado');
  }

  // El buscador de áreas, y las capturas de presentación si se pidieron
  if (OUT) {
    await page.goto(FRONT + '/');
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${OUT}/inicio.png`, fullPage: true });
  }
  await page.goto(FRONT + '/areas-conocimiento');
  await page.waitForTimeout(1500);
  await page.locator('.buscador input').fill('matem');
  await page.waitForTimeout(800);
  ok((await page.locator('.nota', { hasText: 'ficha(s)' }).innerText()).match(/^\d+ de 218/), 'El buscador filtra las 218 áreas');
  if (OUT) {
    await page.screenshot({ path: `${OUT}/areas_buscador.png`, fullPage: false });
    const movil = await browser.newPage({ viewport: { width: 390, height: 844 } });
    await movil.goto(FRONT + '/universidades');
    await movil.waitForTimeout(1500);
    await movil.screenshot({ path: `${OUT}/universidades_movil.png`, fullPage: true });
  }

  await browser.close();
  console.log(fallos ? `\n=== ${fallos} FALLO(S) ===` : '\n=== RECORRIDO CON CLICS: TODO EN VERDE ===');
  process.exit(fallos ? 1 : 0);
})();
