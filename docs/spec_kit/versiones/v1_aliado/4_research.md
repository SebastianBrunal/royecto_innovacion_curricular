# Decisiones — Versión 1

> Cada decisión con sus alternativas y su razón. Esto es memoria del
> proyecto: sirve para **no volver a discutir** lo ya discutido, y para que
> quien llegue después —persona o IA— entienda por qué el sistema es así.
>
> Numeración `D-v1-N`, sin repetir entre versiones.

## D-v1-1 — Dapper, no Entity Framework

**Contexto.** Hay que llevar filas de SQL Server a objetos de C#.

**Alternativas.** (a) **Entity Framework Core**: escribe el SQL por
nosotros, migraciones incluidas. (b) **Dapper**: mapea fila→objeto pero el
SQL lo escribimos nosotros. (c) **ADO.NET puro**: hasta el mapeo a mano.

**Decisión: (b).** El Artículo 2 exige que el SQL esté a la vista. Con EF
el SQL que llega al motor lo genera un traductor: se pierde justo lo que
este curso quiere que se vea y se sustente en una revisión. ADO.NET puro
agregaría veinte líneas de `reader.GetString(i)` por consulta sin enseñar
nada nuevo.

**Consecuencias.** No hay migraciones: el esquema viene dado (Artículo 5).
Cada consulta hay que escribirla, y por eso mismo se puede leer.
**Estado:** vigente.

## D-v1-2 — Las tres capas desde el día 1, no un MVP en un archivo

**Contexto.** Una tabla y siete endpoints caben en un solo controlador de
80 líneas.

**Alternativas.** (a) Todo en el controlador y refactorizar en la v2.
(b) Capas con interfaces desde el principio.

**Decisión: (b).** "Refactorizar después" es una promesa que nadie cumple
con fecha de entrega encima, y la v2 llega con diez tablas: el momento de
separar sería el peor posible. Además, la prueba sin base de datos
—criterio 7— es **imposible** sin la interfaz del repositorio.

**Consecuencias.** Seis archivos donde cabría uno. A cambio, la v2 agrega
tablas sin tocar la arquitectura. **Estado:** vigente.

## D-v1-3 — Una petición por verbo

**Contexto.** `POST`, `PUT` y `PATCH` reciben cuerpos parecidos pero con
reglas distintas: el `PATCH` admite campos ausentes y el `PUT` no.

**Alternativas.** (a) Una sola clase con todos los campos opcionales y
validar a mano según el verbo. (b) Tres clases: `Crear`, `Reemplazo`,
`Actualizar`.

**Decisión: (b).** Con (a) la regla queda escondida en `if`s dentro del
servicio; con (b) la declara el tipo, y el 422 lo produce el framework
antes de que el negocio se entere. Es lo que hace demostrable la pareja
`PUT` 422 / `PATCH` 200 del criterio 4.

**Consecuencias.** Tres archivos pequeños y muy parecidos. Se acepta.
**Estado:** vigente.

## D-v1-4 — El borrado lógico se resuelve en el `UPDATE`, no consultando antes

**Contexto.** `DELETE` debe responder 404 si el registro no existe **o ya
está inactivo** (C4, C5).

**Alternativas.** (a) Consultar primero y luego actualizar. (b) Un solo
`UPDATE … WHERE id = @id AND activo = 1` y mirar las filas afectadas.

**Decisión: (b).** Una sola ida a la base, sin ventana entre la consulta y
la escritura. Cero filas significa exactamente "no existe o ya estaba
inactiva", que es la respuesta que pide la spec.

**Consecuencias.** El mensaje del 404 no distingue entre "nunca existió" y
"ya estaba borrada" — y **está bien**: para la API son el mismo caso.
**Estado:** vigente.

## D-v1-5 — El teléfono es texto, y el correo no se valida

**Contexto.** `aliado` guarda un teléfono y un correo. ¿Qué tipo y qué
validación llevan?

**Alternativas.** (a) `telefono` como número y `correo` con expresión
regular. (b) Ambos como texto, exigiendo solo que estén presentes.

**Decisión: (b).**

- **El teléfono es texto** porque lleva prefijos, espacios, guiones y
  extensiones, y **nunca se suma ni se ordena aritméticamente**. Guardarlo
  como número perdería el cero inicial de un fijo y rechazaría un
  `+57 604 555 1234` perfectamente válido.
- **El correo no se valida más allá de su presencia** porque validar
  formatos de correo es un pozo sin fondo: toda expresión regular
  razonable rechaza direcciones legítimas. Y ninguna versión lo ha pedido.

**Consecuencias.** Se puede crear un aliado con el correo `no-es-un-correo`.
Si una versión futura necesita que el correo sea alcanzable, la forma de
comprobarlo no es una expresión regular: es enviarle un mensaje.
**Estado:** vigente.

## D-v1-6 — Un contenedor aparte para inicializar la base

**Contexto.** SQL Server **no ejecuta los scripts que se le monten**:
alguien tiene que conectarse al motor y correrlos.

**Alternativas.** (a) Instrucciones manuales en el README ("conéctese y
corra esto"). (b) Un contenedor `sqlserver-init` que lo haga solo.

**Decisión: (b).** El Artículo 4 exige un solo comando; (a) lo rompe en la
primera línea. El inicializador espera a que el motor **responda
consultas**, crea la base si no existe, corre el script y se muere.

**Consecuencias.** Un servicio más en el compose, que termina en segundos y
es idempotente. **Estado:** vigente.

## D-v1-7 — El catálogo se corrige antes de sembrarlo

**Contexto.** El Excel trae `Cienias Naturales` (sin la `c`) en 48 de las 218 filas de `area_conocimiento` — un catálogo que la v1 no usa, pero que la base sí carga.

**Alternativas.** (a) Cargarlo tal cual: el dato es dado. (b) Corregir la
digitación al generar las semillas y documentarlo.

**Decisión: (b).** Un error de digitación no es un dato: es ruido de la
fuente. Cargarlo lo dejaría a la vista en cada listado, en cada informe y
en el dashboard de la v4. La corrección queda anotada en la cabecera de
`db/innovacion_curricular.sql`, de modo que cualquiera puede ver qué se cambió.

**Consecuencias.** El script deja de ser una copia literal del Excel, y por
eso mismo la cabecera del script tiene que decirlo. **Estado:** vigente.
## D-v1-8 — Los campos JSON en camelCase

**Contexto.** Las columnas de la base son `gran_area`, `area`,
`disciplina`. ¿El JSON las repite tal cual, o usa la convención de C#?

**Alternativas.** (a) **snake_case** en el JSON, igual que la base: un
`SELECT` y una respuesta se leen igual. (b) **camelCase**, que es lo que
ASP.NET Core hace por defecto.

**Decisión: (b).** Tres razones, en orden de peso:

1. **Es el comportamiento por defecto**: cero configuración. Lo que no se
   configura no se puede configurar mal, y una política de serialización
   mal puesta rompe TODOS los endpoints a la vez, con un síntoma
   desconcertante — un `POST` correcto respondiendo "falta el campo".
2. **El JSON no es una ventana a la tabla.** La API es una frontera: si
   mañana la columna se renombra, el contrato no tiene por qué cambiar. El
   parecido con la base es una coincidencia cómoda, no un requisito.
3. **El front de la v4 lo consume directo.** `granArea` es lo que espera
   quien escribe JavaScript.

**Consecuencias.** El JSON deja de parecerse a la tabla, y hay que
traducir mentalmente al leer el repositorio. A cambio, `Program.cs` no
lleva ni una línea de configuración de serialización.

**De dónde salió esta decisión.** El `6_contracts.md` tenía las dos
convenciones mezcladas —`gran_area` en los cuerpos y `filasAfectadas` en
las respuestas de escritura—, y eso solo se descubrió al escribir la
primera clase de petición: era imposible cumplir las dos a la vez.
**Estado:** vigente.

## D-v1-9 — La v1 se construye sobre una tabla vacía

**Contexto.** De las siete tablas sin clave foránea, `aliado` es la de más
campos (seis) — pero el Excel de referencia **no trae un solo aliado**.
`area_conocimiento`, en cambio, trae 218 filas.

**Alternativas.** (a) Construir sobre `area_conocimiento`, que tiene datos
y da un smoke test con cifras. (b) Construir sobre `aliado`, la de más
campos, aunque arranque vacía.

**Decisión: (b)**, por dos razones:

1. **`area_conocimiento` ya está construida** en el ejemplo del módulo de
   Investigación. Repetirla haría que los dos ejemplos fueran el mismo
   código con otro nombre de repositorio.
2. **Arrancar vacía resultó una ventaja**, no una carencia: el smoke test
   puede recorrer el ciclo completo desde el estado inicial —204, crear,
   listar, borrar, 204 otra vez— y **ejercita el 204 del listado vacío**,
   que una tabla con 218 filas nunca deja probar.

**Consecuencias.** No hay una cifra de catálogo que verificar, así que el
criterio 2 comprueba el **204** en vez de un total. Y el ejemplo demuestra
algo que el otro no puede: que el sistema funciona **antes** de tener
datos. **Estado:** vigente.

## D-v1-10 — La clave de la base sale del `.env` (enmienda del Artículo 7)

**Contexto.** El ejemplo del profesor llevaba la contraseña de `sa` escrita
en el `docker-compose.yml` y en el `appsettings.json`, como excepción
declarada de una plantilla didáctica. La metodología del proyecto de aula
dice lo contrario: **un secreto quemado anula el criterio de seguridad de la
versión**, y este repositorio ya no es la plantilla: es el del equipo.

**Decisión.** La clave vive en un `.env` fuera de git. El repositorio trae
`.env.example` con un valor de mentira. El compose la lee con
`${MSSQL_SA_PASSWORD:?…}`, que **detiene el arranque con un mensaje claro**
si falta el `.env`, en vez de crear un `sa` sin clave. El `appsettings.json`
queda sin cadena de conexión: fuera de Docker se pasa por la variable de
entorno `ConnectionStrings__SqlServer`.

**Alternativa descartada.** Dejarla como estaba porque «el sistema corre en
contenedores desechables». Es cierto hoy, pero el sistema se publica en la
v4, y la historia de git no olvida: una clave que se sube una vez hay que
rotarla.

**Consecuencia.** El arranque pasa de un comando a dos la primera vez
(copiar el `.env.example`). Es el precio de que la clave no esté en GitHub.

## D-v1-11 — El front y la API en repositorios separados

**Contexto.** El ejemplo del profesor tenía la API y el front en el mismo
repositorio, como dos carpetas y un solo `docker-compose.yml`. El documento
del módulo pide otra cosa: **«dos proyectos (repositorios) completamente
separados»** (§3) y, desde la entrega 2, **«repositorio en GitHub para cada
proyecto (API y Frontend)»**.

**Decisión.** Dos repositorios:

| Repositorio | Qué tiene |
|---|---|
| `royecto_innovacion_curricular` (este) | La API, la base (`db/`), su compose (SQL Server + inicializador + API), Postman, `humo_api.py` y **el spec kit** —la metodología lo ubica en el repositorio de la API— |
| [`innovacion_curricular_front`](https://github.com/SebastianBrunal/innovacion_curricular_front) | El front Blazor, su compose (solo el front), la identidad visual (`marca/`, `MANUAL_DE_MARCA.md`), `humo_front.py` y `recorrido_clics.js` |

El front se separó con `git filter-repo`, **conservando la historia** de
sus archivos.

**Es SOLID aplicado a nivel de sistema:**

- **S — responsabilidad única.** Cada repositorio tiene una sola razón para
  cambiar: la API cambia por datos y reglas; el front, por cómo se ve y se
  usa. Un commit de estilos ya no pasa por el repositorio que tiene el SQL.
- **D — inversión de dependencias.** El front depende de una abstracción —el
  contrato HTTP de `6_contracts.md`— y no de la implementación. Lo único que
  sabe de la API es su URL (`UrlApi`).
- **O — abierto/cerrado.** La API puede cambiar de motor, de servidor o de
  lenguaje sin tocar el front, mientras cumpla el contrato.

**Consecuencias.**

1. El front ya no encuentra a la API por el **nombre del servicio**
   (`api-innovacion`), porque está en otro compose: la busca en el puerto
   publicado del PC, `http://host.docker.internal:8072`, configurable con
   `URL_API`.
2. «Un solo comando» pasa a ser **uno por repositorio** (Artículo 4): primero
   la API, después el front.
3. `humo_front.py` congela la API por su `container_name`
   (`innovacion-api`), no por el servicio del compose.
4. Cambiar el contrato ahora exige **dos commits en dos repositorios**. Es el
   precio, y también la protección: nadie lo cambia sin darse cuenta.

**Alternativa descartada.** Un solo repositorio con dos carpetas, como el
ejemplo. Es más cómodo, pero no cumple lo que pide el módulo y permite que
un cambio en un lado se cuele sin revisar el otro.
