using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1 de area_conocimiento: HTTP. Sin lógica de negocio ni SQL. Traduce las
/// excepciones del negocio a códigos de estado, igual que AliadoController.
///
/// La ruta se escribe COMPLETA con el nombre de la tabla (Artículo 10).
///
/// El id es TEXTO ('1A01'), así que la ruta no lleva la restricción :int
/// que sí llevan las tablas con llave numérica.
/// </summary>
[ApiController]
[Route("api/area_conocimiento")]
public class AreaConocimientoController : ControllerBase
{
    private readonly IServicioAreaConocimiento _servicio;

    public AreaConocimientoController(IServicioAreaConocimiento servicio)
    {
        _servicio = servicio;
    }

    /// <summary>RF1 — Listar los registros activos.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int limite = 1000)
    {
        try
        {
            var lista = (await _servicio.ObtenerTodos(limite)).ToList();

            // Vacío NO es error: 204 sin cuerpo
            if (lista.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "area_conocimiento",
                limite = limite,
                total = lista.Count,
                datos = lista
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF2 — Obtener un registro por su id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(string id)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorId(id));
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Área de conocimiento no encontrada.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF3 — Crear.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AreaConocimientoCrear peticion)
    {
        try
        {
            // El controlador traduce: la capa 2 recibe la entidad, no el cuerpo HTTP
            var entidad = new AreaConocimiento
            {
                Id = peticion.Id,
                GranArea = peticion.GranArea,
                Area = peticion.Area,
                Disciplina = peticion.Disciplina
            };

            await _servicio.Crear(entidad);
            return Ok(new { estado = 200, mensaje = "Área de conocimiento creada exitosamente." });
        }
        catch (SqlException ex)
        {
            // Id duplicado: la llave la defiende la base, no la API (C11)
            return StatusCode(500, new { estado = 500, mensaje = "Error al insertar en la base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF4 — Reemplazo completo.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Reemplazar(string id, [FromBody] AreaConocimientoReemplazo peticion)
    {
        try
        {
            var entidad = new AreaConocimiento
            {
                Id = id,
                GranArea = peticion.GranArea,
                Area = peticion.Area,
                Disciplina = peticion.Disciplina
            };

            var filas = await _servicio.Reemplazar(id, entidad);
            return Ok(new { estado = 200, mensaje = "Área de conocimiento reemplazada.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Área de conocimiento no encontrada.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF5 — Actualización parcial.</summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> ActualizarParcial(string id, [FromBody] AreaConocimientoActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarParcial(id, peticion.GranArea, peticion.Area, peticion.Disciplina);
            return Ok(new { estado = 200, mensaje = "Área de conocimiento actualizada.", filasAfectadas = filas });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Área de conocimiento no encontrada.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF6 — Borrado lógico.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Área de conocimiento eliminada.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Área de conocimiento no encontrada.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
