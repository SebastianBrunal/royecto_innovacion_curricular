"""Prueba de humo de la API — las 7 tablas sin clave foránea de la v1.

Recorre, tabla por tabla, el mismo ciclo que el criterio 4 de 2_spec.md pide
para aliado:

    listar → crear → obtener → reemplazar (PUT) → actualizar (PATCH) →
    eliminar (lógico) → eliminar otra vez (404)

y los desenlaces de error del contrato: 400 (límite inválido, PATCH vacío),
404 (no existe), 422 (cuerpo incompleto, PUT sin un campo) y 500 (llave
duplicada). Al final comprueba **en la base** que las filas siguen ahí con
activo = 0: el borrado es lógico.

Cada corrida usa identificadores al azar y deja sus fichas retiradas, así
que se puede correr las veces que se quiera sin resetear la base.

Uso:  python pruebas_humo/humo_api.py      (con el sistema arriba)
"""
import json
import random
import string
import subprocess
import urllib.error
import urllib.request

API = "http://localhost:8072"
fallos = []

SUFIJO = "".join(random.choices(string.digits, k=4))

# Por tabla: la ruta, una ficha válida y el campo que se le quita al PUT.
# Los textos respetan el tope de cada columna (VARCHAR(45) en casi todas).
RECURSOS = [
    ("aliado", "nit", int("97" + SUFIJO), {
        "razonSocial": "Fundación Prueba " + SUFIJO, "nombreContacto": "Ana Restrepo",
        "correo": "ana@prueba.co", "telefono": "604 555 1234", "ciudad": "Medellín"},
     "correo"),
    ("area_conocimiento", "id", "Z" + SUFIJO, {
        "granArea": "Ciencias de prueba", "area": "Área de prueba",
        "disciplina": "Disciplina " + SUFIJO},
     "disciplina"),
    ("universidad", "id", int("97" + SUFIJO), {
        "nombre": "Universidad de prueba " + SUFIJO, "tipo": "Seccional", "ciudad": "Pasto"},
     "ciudad"),
    ("aspecto_normativo", "id", int("97" + SUFIJO), {
        "tipo": "Decreto", "descripcion": "Aspecto de prueba " + SUFIJO,
        "fuente": "Decreto 1330 de 2019"},
     "fuente"),
    ("practica_estrategia", "id", int("97" + SUFIJO), {
        "tipo": "Estrategia", "nombre": "Práctica de prueba " + SUFIJO,
        "descripcion": "Aprender haciendo"},
     "descripcion"),
    ("enfoque", "id", int("97" + SUFIJO), {
        "nombre": "Enfoque de prueba " + SUFIJO, "descripcion": "Constructivista"},
     "descripcion"),
    ("car_innovacion", "id", int("97" + SUFIJO), {
        "nombre": "Característica " + SUFIJO,
        "descripcion": "Una descripción larga que en esta tabla no tiene tope de longitud, "
                       "porque la columna es VARCHAR(MAX).",
        "tipo": "Curricular"},
     "tipo"),
]


def pedir(metodo, ruta, cuerpo=None):
    datos = json.dumps(cuerpo).encode() if cuerpo is not None else None
    p = urllib.request.Request(API + ruta, data=datos, method=metodo,
                               headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(p, timeout=15) as r:
            texto = r.read().decode()
            return r.status, json.loads(texto) if texto else None
    except urllib.error.HTTPError as e:
        texto = e.read().decode()
        try:
            return e.code, json.loads(texto) if texto else None
        except ValueError:
            return e.code, texto
    except Exception as e:
        return 0, str(e)


def revisar(nombre, condicion, detalle=""):
    print(f"{'[OK]    ' if condicion else '[FALLO] '}{nombre} {str(detalle)[:120]}")
    if not condicion:
        fallos.append(nombre)


def consultar_activo(tabla, llave, valor):
    """Lee la columna activo DIRECTAMENTE en la base (criterio 5)."""
    filtro = f"'{valor}'" if isinstance(valor, str) else str(valor)
    sql = f"SET NOCOUNT ON; SELECT activo FROM {tabla} WHERE {llave} = {filtro}"
    r = subprocess.run(
        ["docker", "exec", "innovacion-sqlserver", "bash", "-c",
         '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" '
         f'-C -d innovacion_local -h -1 -W -Q "{sql}"'],
        capture_output=True, text=True)
    return r.stdout.strip()


codigo, cuerpo = pedir("GET", "/")
revisar("GET / diagnóstico v1", codigo == 200 and cuerpo.get("version") == "v1", cuerpo)

for tabla, llave, valor, ficha, campo_put in RECURSOS:
    print(f"\n=== {tabla} ===")
    base = f"/api/{tabla}"
    uno = f"{base}/{valor}"

    c, _ = pedir("GET", base)
    revisar("listar responde 200 o 204", c in (200, 204), f"HTTP {c}")

    c, _ = pedir("GET", f"{base}?limite=0")
    revisar("limite=0 → 400", c == 400, f"HTTP {c}")

    c, r = pedir("POST", base, {llave: valor, **ficha})
    revisar("POST completo → 200", c == 200, r)

    c, r = pedir("POST", base, {llave: valor, **ficha})
    revisar("POST con la llave repetida → 500", c == 500, f"HTTP {c}")

    incompleta = {llave: valor}
    c, r = pedir("POST", base, incompleta)
    revisar("POST incompleto → 422 con errores[]",
            c == 422 and isinstance(r, dict) and len(r.get("errores", [])) > 0, r)

    c, r = pedir("GET", uno)
    revisar("GET por llave → 200", c == 200 and r.get(llave) == valor, r)

    c, r = pedir("GET", base)
    revisar("y aparece en el listado", c == 200 and any(f[llave] == valor for f in r["datos"]),
            f"total={r.get('total') if isinstance(r, dict) else r}")

    c, r = pedir("PUT", uno, ficha)
    revisar("PUT completo → 200", c == 200 and r.get("filasAfectadas") == 1, r)

    sin_un_campo = {k: v for k, v in ficha.items() if k != campo_put}
    c, r = pedir("PUT", uno, sin_un_campo)
    revisar(f"PUT sin '{campo_put}' → 422", c == 422, r)

    c, r = pedir("PATCH", uno, sin_un_campo)
    revisar("el MISMO cuerpo por PATCH → 200", c == 200, r)

    primer_campo = next(iter(ficha))
    c, r = pedir("PATCH", uno, {primer_campo: "Cambiado " + SUFIJO})
    c2, r2 = pedir("GET", uno)
    revisar("PATCH de un solo campo cambia solo ese campo",
            c == 200 and r2.get(primer_campo) == "Cambiado " + SUFIJO
            and r2.get(campo_put) == ficha[campo_put], r2)

    c, r = pedir("PATCH", uno, {})
    revisar("PATCH vacío → 400", c == 400, r)

    no_existe = "9Z99" if isinstance(valor, str) else 999999
    c, r = pedir("GET", f"{base}/{no_existe}")
    revisar("GET de una llave que no existe → 404", c == 404, r)

    c, r = pedir("DELETE", uno)
    revisar("DELETE → 200", c == 200, r)

    c, r = pedir("DELETE", uno)
    revisar("DELETE otra vez → 404", c == 404, f"HTTP {c}")

    c, r = pedir("GET", uno)
    revisar("y ya no se encuentra → 404", c == 404, f"HTTP {c}")

    revisar("pero la fila SIGUE en la base con activo = 0",
            consultar_activo(tabla, llave, valor) == "0")

print()
if fallos:
    print(f"=== RESULTADO: {len(fallos)} FALLO(S) ===")
    for f in fallos:
        print("   -", f)
    raise SystemExit(1)
print("=== RESULTADO: TODO EN VERDE — 7 tablas ===")
