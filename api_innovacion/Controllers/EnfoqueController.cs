using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// La capa 1 de enfoque: HTTP. Sin lógica de negocio ni SQL. Traduce las
/// excepciones del negocio a códigos de estado, igual que AliadoController.
///
/// La ruta se escribe COMPLETA con el nombre de la tabla (Artículo 10).
/// </summary>
[ApiController]
[Route("api/enfoque")]
public class EnfoqueController : ControllerBase
{
    private readonly IServicioEnfoque _servicio;

    public EnfoqueController(IServicioEnfoque servicio)
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
                tabla = "enfoque",
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
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorId(id));
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Enfoque no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF3 — Crear.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] EnfoqueCrear peticion)
    {
        try
        {
            // El controlador traduce: la capa 2 recibe la entidad, no el cuerpo HTTP
            var entidad = new Enfoque
            {
                Id = peticion.Id!.Value,
                Nombre = peticion.Nombre,
                Descripcion = peticion.Descripcion
            };

            await _servicio.Crear(entidad);
            return Ok(new { estado = 200, mensaje = "Enfoque creado exitosamente." });
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
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] EnfoqueReemplazo peticion)
    {
        try
        {
            var entidad = new Enfoque
            {
                Id = id,
                Nombre = peticion.Nombre,
                Descripcion = peticion.Descripcion
            };

            var filas = await _servicio.Reemplazar(id, entidad);
            return Ok(new { estado = 200, mensaje = "Enfoque reemplazado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Enfoque no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF5 — Actualización parcial.</summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] EnfoqueActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarParcial(id, peticion.Nombre, peticion.Descripcion);
            return Ok(new { estado = 200, mensaje = "Enfoque actualizado.", filasAfectadas = filas });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Enfoque no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF6 — Borrado lógico.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Enfoque eliminado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Enfoque no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
