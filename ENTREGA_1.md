# Entrega 1 — CRUD de las 7 tablas sin clave foránea

> Módulo **Innovación Curricular** · API REST (C# / ASP.NET Core + Dapper) +
> Frontend (Blazor Server) + SQL Server, en Docker.
>
> Este repositorio parte del ejemplo del profesor
> ([`ccastro2050/proyecto_innovacion_curricular1`](https://github.com/ccastro2050/proyecto_innovacion_curricular1)),
> que traía **una** de las siete tablas (`aliado`) construida de punta a punta.
> La entrega 1 completa las **otras seis** con el mismo patrón, y ajusta lo que
> el proyecto de aula exige distinto del ejemplo (los secretos).

---

## 1. Cómo se corre

Solo hace falta **Docker Desktop**.

```powershell
git clone <url de este repositorio>
cd proyecto_innovacion_curricular
copy .env.example .env          # y ponga su propia clave en MSSQL_SA_PASSWORD
docker compose up -d --build
```

| Qué | Dónde |
|---|---|
| **La pantalla** | http://localhost:8073 |
| API — diagnóstico | http://localhost:8072/ |
| Documentación interactiva (Swagger) | http://localhost:8072/swagger |
| SQL Server (opcional, SSMS / SQLTools) | `localhost,11471` · usuario `sa` · clave: la de su `.env` |

> **¿Por qué hay que copiar el `.env`?** Porque la clave de la base **no está
> en el repositorio** (Artículo 7 de la constitución, decisión D-v1-10). Si se
> olvida, `docker compose` se detiene con el mensaje *«Falta el .env - copie
> .env.example a .env»*.

## 2. Lo que pide el documento de la entrega, y dónde está

### API (backend)

| Requisito | Dónde |
|---|---|
| Conexión a la base de datos | `api_innovacion/Repositorios/*SqlServer.cs` (Dapper + `Microsoft.Data.SqlClient`); la cadena llega por variable de entorno |
| `GET /api/{tabla}` — listar | Un controlador por tabla en `api_innovacion/Controllers/` |
| `GET /api/{tabla}/{id}` — consultar uno | Ídem |
| `POST /api/{tabla}` — crear | Ídem, con la petición `*Crear.cs` que valida el cuerpo |
| `PUT /api/{tabla}/{id}` — actualizar | Ídem, con `*Reemplazo.cs` (y `PATCH` con `*Actualizar.cs`, que pide el contrato del curso) |
| `DELETE /api/{tabla}/{id}` — borrado lógico | `UPDATE … SET activo = 0` en cada repositorio; la fila **no** se borra |
| JSON con códigos 200, 400, 404, 500 | Más 204 (lista vacía) y 422 (cuerpo inválido), según `6_contracts.md`. El `POST` responde **200**, no 201: ver C19 |

### Frontend

| Requisito | Dónde |
|---|---|
| Listado con **ver**, editar y eliminar | Una página por tabla en `front_blazor/Components/Pages/` — botones **Ver**, **Editar**, **Retirar** en cada fila |
| Formulario para crear | Botón **Agregar** de cada página |
| Formulario para editar | **Editar** → «Guardar la ficha completa» (`PUT`) o «Guardar solo lo que cambié» (`PATCH`) |
| Eliminar con confirmación (lógico) | **Retirar** pide confirmación y llama al `DELETE` |
| Consume la API, no la base | `front_blazor/Servicios/Servicio*.cs` hablan HTTP con la API; el proyecto del front no tiene ningún driver de base de datos |

## 3. Las siete tablas

| Tabla | API | Pantalla | Llave | Datos iniciales |
|---|---|---|---|---|
| `aliado` | `/api/aliado` | `/aliados` | `nit` (entero) | 0 — *del ejemplo del profesor* |
| `area_conocimiento` | `/api/area_conocimiento` | `/areas-conocimiento` | `id` (**texto**, `1A01`) | 218 (con buscador) |
| `universidad` | `/api/universidad` | `/universidades` | `id` (entero) | 6 |
| `aspecto_normativo` | `/api/aspecto_normativo` | `/aspectos-normativos` | `id` (entero) | 0 |
| `practica_estrategia` | `/api/practica_estrategia` | `/practicas-estrategias` | `id` (entero) | 0 |
| `enfoque` | `/api/enfoque` | `/enfoques` | `id` (entero) | 0 |
| `car_innovacion` | `/api/car_innovacion` | `/caracteristicas-innovacion` | `id` (entero) | 0 |

Los campos de cada cuerpo y sus topes están en
[`6_contracts.md` §8](docs/spec_kit/versiones/v1_aliado/6_contracts.md).

Por cada tabla nueva hay, igual que con `aliado`:

```
api_innovacion/
  Modelos/<Entidad>.cs                         la entidad
  Peticiones/<Entidad>Crear|Reemplazo|Actualizar.cs   un cuerpo por verbo → 422
  Repositorios/IRepositorio<Entidad>.cs + Repositorio<Entidad>SqlServer.cs
  Servicios/IServicio<Entidad>.cs + Servicio<Entidad>.cs
  Controllers/<Entidad>Controller.cs
  pruebas/RepositorioFalso<Entidad>.cs + Prueba<Entidad>.cs   sin base de datos
front_blazor/
  Servicios/Servicio<Entidad>.cs               lo único que habla con la API
  Components/Pages/<Plural>.razor              la pantalla
```

## 4. Decisiones tomadas en esta entrega

Quedaron como **Clarificaciones C13 a C20** en
[`2_spec.md`](docs/spec_kit/versiones/v1_aliado/2_spec.md) §6 y como
**D-v1-10** en [`4_research.md`](docs/spec_kit/versiones/v1_aliado/4_research.md).
En resumen:

| # | Decisión |
|---|---|
| C13 | Las seis tablas se construyen con el mismo molde, **una clase por recurso**, nada genérico (Artículo 10.1) |
| C14 | El `id` de las seis tablas lo manda quien crea (la base no lo genera); repetido → 500 |
| C15 | `area_conocimiento` tiene llave de texto: su ruta es `{id}`, no `{id:int}` |
| C16 | `car_innovacion.descripcion` es `VARCHAR(MAX)`: sin tope en la validación, y área de texto en pantalla |
| C17 | Se agrega **Ver** a las siete pantallas (el documento de la entrega lo pide) |
| C18 | El `PATCH` también valida longitudes → 422 en vez de un 500 del motor (se corrigió en `aliado` también) |
| C19 | El `POST` responde 200 y no 201: lo fija el contrato del curso, y las siete tablas responden igual |
| C20 | Sin paginación en la v1; la pantalla de áreas trae un buscador sobre las 218 filas |
| D-v1-10 | La clave de la base sale del `.env` (enmienda del Artículo 7) |

## 5. Evidencia — cómo se verificó

| Prueba | Qué comprueba | Resultado |
|---|---|---|
| `dotnet run --project api_innovacion/pruebas` | Los servicios de las 7 tablas con un repositorio **falso** en memoria, **sin** SQL Server | 55 comprobaciones en verde |
| `python pruebas_humo/humo_api.py` | Las 7 tablas: listar, crear, obtener, `PUT`, `PUT` sin un campo → 422, el mismo cuerpo por `PATCH` → 200, `PATCH` vacío → 400, 404, llave duplicada → 500, `DELETE` → 200, segundo `DELETE` → 404 **y la fila sigue en la base con `activo = 0`** | Todo en verde |
| `python pruebas_humo/humo_front.py` | Las 8 pantallas responden por su dirección, muestran los datos que dio la API, no hablan en jerga, y **con la API congelada la pantalla sigue en pie sin datos** | Todo en verde |
| `node pruebas_humo/recorrido_clics.js` *(opcional, requiere Node y Playwright)* | Clics reales en las 7 pantallas: agregar, ver, «la ficha completa» a medio llenar se rechaza sin borrar lo escrito, «solo lo que cambié» guarda, retirar con confirmación | Todo en verde |
| Colección de Postman | 14 peticiones de `aliado` + 12 por cada tabla nueva | Los códigos coinciden con las descripciones |

## 6. Lo que falta para cerrar la v1 (lo hace una persona)

- [ ] Recorrer a mano el [`7_quickstart.md`](docs/spec_kit/versiones/v1_aliado/7_quickstart.md) en el PC del equipo.
- [ ] Revisar y **firmar** el [`9_checklist.md`](docs/spec_kit/versiones/v1_aliado/9_checklist.md) — lo marca una persona, no la IA.
- [ ] Revisar las decisiones C13–C20 y D-v1-10: si el equipo no está de acuerdo con alguna, se cambia en la spec **y** en el código.
- [ ] Completar los prompts del equipo en [`PROMPTS_ENTREGA1.md`](docs/spec_kit/versiones/v1_aliado/PROMPTS_ENTREGA1.md).
- [ ] `git tag v1` y `git push --tags`.
