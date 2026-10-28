# Los prompts de la entrega 1 — cómo pasamos de la spec al código

> La metodología (§3 de `0_METODOLOGIA.md`) pide entregar **los prompts que
> de verdad se usaron**, con **lo que hubo que corregirle a la IA**. Este
> documento es eso: el registro de cómo dirigí a la IA para completar las seis
> tablas que le faltaban al ejemplo del profesor.
>
> **Camino usado:** B — IDE agéntico (Claude Code). El agente leyó el
> repositorio, escribió los archivos y corrió las pruebas; yo definí qué
> construir, sobre qué base, le corregí el rumbo y reviso lo entregado.
>
> La guía general de cómo trabajar con IA en esta versión es la del profesor:
> [GUIA_IA1.md](GUIA_IA1.md).

---

## 1. Lo que puse yo antes de que la IA escribiera una línea

| Qué | Por qué importa |
|---|---|
| **El enunciado**: el PDF `modulo_innovacion_curricular.pdf` con las 25 tablas y la sección 5.1 (Primera entrega) | Es la fuente del QUÉ. Sin él la IA no sabía cuáles eran las 7 tablas ni qué pedía la entrega |
| **El alcance**: copié en el prompt los entregables exactos de la API y del front | Así la IA no tenía que adivinar qué era «terminado» |
| **La referencia de arquitectura**: `ccastro2050/proyecto_paradigmas` | Para que siguiera las capas del curso y no inventara una estructura |
| **Mi proyecto anterior** (`ProyectoFacturas.zip`) | Como contexto de lo que ya había hecho en el curso |
| **La base de datos de mi caso**: `ccastro2050/proyecto_innovacion_curricular1` | La corrección más importante. Ver §3 |

## 2. Los prompts, en orden

### Prompt 1 — el encargo

Adjunté el PDF del módulo y `ProyectoFacturas.zip`, y escribí:

```text
CRUD de las tablas que NO requieren claves foráneas. Se debe entregar tanto
la API REST como el Frontend funcionando para estas tablas.
Tablas a implementar:
• area_conocimiento: Catálogo jerárquico de áreas de conocimiento organizadas
  por gran área, área y disciplina.
• universidad: Sedes y seccionales de la universidad en diferentes ciudades del país.
• aspecto_normativo: Catálogo de aspectos normativos que regulan los programas.
• practica_estrategia: Catálogo de prácticas y estrategias pedagógicas.
• enfoque: Catálogo de enfoques pedagógicos.
• car_innovacion: Catálogo de características de innovación curricular.
• aliado: Entidades externas aliadas (empresas, instituciones, organizaciones).
Entregables del Proyecto API (Backend):
• Conexión a la base de datos.
• Endpoints REST para cada tabla sin FK (7 tablas):
• GET /api/{tabla} — Listar todos los registros.
• GET /api/{tabla}/{id} — Consultar un registro por ID.
• POST /api/{tabla} — Crear un nuevo registro.
• PUT /api/{tabla}/{id} — Actualizar un registro existente.
• DELETE /api/{tabla}/{id} — Borrado lógico (activo = 0).
• Respuestas en formato JSON con códigos HTTP apropiados (200, 201, 400, 404, 500).
Entregables del Proyecto Frontend:
• Interfaz CRUD para cada tabla sin FK (7 tablas):
• Listado de registros con opciones de ver, editar y eliminar.
• Formulario para crear nuevos registros.
• Formulario para editar registros existentes.
• Botón de eliminación con confirmación (borrado lógico).
• Las páginas deben consumir la API REST (no conectarse directamente a la BD).
Toma de referencia https://github.com/ccastro2050/proyecto_paradigmas
```

*(Se omiten del registro las instrucciones de logística del mismo mensaje:
subir el repositorio a mi GitHub e invitar al profesor como colaborador.)*

### Prompt 2 — primera corrección: el alcance

La IA leyó el PDF completo, que describe las **cuatro** entregas (JWT,
dashboard, publicación…). Para que no anticipara nada, se lo acoté:

```text
me debes hacer la primera entrega plis
```

### Prompt 3 — segunda corrección: la base del proyecto

La IA había decidido construir sobre mi `ProyectoFacturas` (Flask +
PostgreSQL) y ya había empezado a montar ese entorno. La detuve:

```text
no lo hagas con ese proyecto facturas ya te envio la bd de mi caso
```

### Prompt 4 — el repositorio del caso

```text
https://github.com/ccastro2050/proyecto_innovacion_curricular1 mira con esta
```

Este repositorio cambió todo: el caso **no es Python sino C# / ASP.NET Core
+ Blazor + SQL Server**, ya trae la base `innovacion_local` con sus 25 tablas
y sus catálogos, el spec kit de la v1, y `aliado` construida como ejemplo.
La instrucción implícita fue: completar las otras seis **con ese mismo
patrón**, sin cambiar la especificación.

### Prompt 5 — separar el front de la API

Con la entrega ya subida, le pedí a la IA dividir el proyecto en dos
repositorios:

```text
hay que dividir el front de el api por metodo solid en los repositorios hazlo
```

El ejemplo del profesor tenía API y front en un solo repositorio, pero el
documento del módulo pide «dos proyectos (repositorios) completamente
separados». La IA separó el front a
[innovacion_curricular_front](https://github.com/SebastianBrunal/innovacion_curricular_front)
**conservando su historia** (`git filter-repo`), le dio su propio
`docker-compose.yml` y lo hizo llegar a la API **solo por su URL**. Quedó
registrado como D-v1-11 y C21.

## 3. Lo que hubo que corregirle a la IA

| # | Qué hizo la IA | Por qué estaba mal | Cómo se corrigió | Destino (GUIA_IA1, «los tres destinos») |
|---|---|---|---|---|
| 1 | Planeó el sistema completo leyendo el PDF entero | La entrega 1 son solo las 7 tablas sin FK; lo demás es de la v2 a la v4 (Artículo 1: no se anticipa) | Prompt 2 | **Yo**: le señalé el alcance |
| 2 | Escogió como base Flask + PostgreSQL, a partir de `ProyectoFacturas` | El caso del curso ya fija el stack (Artículo 2: C# + Dapper + SQL Server) y la base viene **dada** (Artículo 5) | Prompts 3 y 4 | **El prompt**: la próxima vez el repositorio del caso va desde el primer mensaje |

Y dos que encontró la propia IA al probar, y que reviso yo antes de aceptarlas:

| # | Hallazgo | Decisión |
|---|---|---|
| 3 | El `PATCH` de `aliado` no validaba longitudes: un texto largo llegaba a la base y salía **500** | C18: también es 422. Se corrigió en las 7 tablas |
| 4 | El ejemplo del profesor tiene la clave de la base escrita en el compose; la metodología dice que en el proyecto de aula eso anula el criterio de seguridad | D-v1-10: la clave pasa a `.env` |

## 4. Decisiones que propuso la IA — las reviso y las acepto (o no) yo

Están escritas en [2_spec.md](2_spec.md) §6 (C13 a C21) y en
[4_research.md](4_research.md) (D-v1-10 y D-v1-11). La IA **propone**; la decisión es
del equipo. Marque cada una antes de firmar el `9_checklist.md`:

| # | Propuesta | ¿Aceptada? | Comentario del equipo |
|---|---|---|---|
| C13 | Una clase por recurso, mismo molde que `aliado` | ☐ | |
| C14 | El `id` lo manda quien crea; repetido → 500 | ☐ | |
| C15 | Ruta `{id}` (texto) en `area_conocimiento` | ☐ | |
| C16 | Sin tope para `car_innovacion.descripcion` | ☐ | |
| C17 | Botón **Ver** en las 7 pantallas | ☐ | |
| C18 | `PATCH` valida longitudes → 422 | ☐ | |
| C19 | `POST` responde 200 (contrato del curso), no 201 (PDF) | ☐ | |
| C20 | Buscador en áreas de conocimiento, sin paginar | ☐ | |
| D-v1-10 | Clave de la base en `.env` | ☐ | |
| D-v1-11 / C21 | Front y API en repositorios separados; el front solo conoce la URL de la API | ☐ | |

> C19 es la que más conviene preguntarle al profesor: el PDF menciona 201 y
> el contrato del curso dice 200. Se dejó 200 porque el Artículo 9 manda
> cumplir el contrato al pie de la letra.

## 5. El prompt consolidado — para reproducir la entrega desde cero

Lo que aprendí de las correcciones, en un solo prompt. **No es el que usé**
(ese está en §2, con sus correcciones): es el que usaría si empezara de
nuevo, y es el que sirve para que otro integrante reconstruya la entrega.

```text
Construye la ENTREGA 1 de este proyecto: el CRUD (API + pantalla) de las 7
tablas sin clave foránea. aliado YA está hecha: es el molde. Faltan
area_conocimiento, universidad, aspecto_normativo, practica_estrategia,
enfoque y car_innovacion.

Primero lee docs/spec_kit/1_constitution.md y los documentos de
docs/spec_kit/versiones/v1_aliado/ (2_spec a 8_tasks), y el código de
aliado en api_innovacion/ y, en el repositorio del front
(innovacion_curricular_front), front_blazor/. Resume en 10 líneas qué vas a
construir y espera mi OK.

Reglas:
1. Stack fijo: C# / ASP.NET Core (.NET 10) + Dapper + SQL Server, front en
   Blazor Server. No propongas otro lenguaje ni otra base.
2. La base YA viene dada en db/: no escribas CREATE TABLE. Saca los nombres,
   tipos y tamaños de las columnas de db/innovacion_curricular.sql.
3. Copia el patrón de aliado tabla por tabla: modelo, tres peticiones por
   verbo, interfaz + repositorio, interfaz + servicio, controlador, servicio
   del front y pantalla con dirección propia. Nada genérico (Artículo 10.1).
4. Borrado lógico (activo = 0) y los listados filtran inactivos.
5. Contrato de 6_contracts.md al pie de la letra (204, 400, 404, 422, 500).
6. La pantalla tiene Ver, Editar y Retirar con confirmación.
7. Si algo de la spec admite dos lecturas, PREGÚNTAME y escríbelo como
   Clarificación en 2_spec.md. No asumas.
8. La clave de la base va en .env, no en archivos versionados.
9. El front y la API van en repositorios separados: el front solo conoce
   la URL de la API, nunca la base.
10. Al terminar cada tabla: compila y corre la prueba de capas. Al final:
   pruebas_humo/humo_api.py (aquí) y humo_front.py (en el front), con la
   salida real.
```

## 6. Lo que verifiqué yo — a completar por el equipo

> Esta sección la llena cada integrante con lo que **hizo de verdad**: es lo
> que se le muestra al profesor en la sustentación. No se llena con frases
> genéricas.

- [ ] Corrí `docker compose up -d --build` en los dos repositorios (primero la API, después el front) en mi PC y abrí las 7 pantallas.
- [ ] Hice el recorrido a mano del [7_quickstart.md](7_quickstart.md).
- [ ] Revisé el código de al menos una tabla completa (controlador → servicio → repositorio) y sé explicar por qué cada capa está separada.
- [ ] Revisé y marqué las decisiones de la §4.
- [ ] Firmé el [9_checklist.md](9_checklist.md).
- Otras cosas que hice / cambié a mano:
  - …
