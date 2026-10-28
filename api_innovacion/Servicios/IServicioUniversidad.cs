using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>
/// El contrato de la capa de negocio de universidad. Solo conoce Modelos/: las
/// clases de Peticiones/ son de la frontera HTTP y no cruzan a esta capa.
///
///   ArgumentException      → 400
///   NoEncontradoExcepcion  → 404
/// </summary>
public interface IServicioUniversidad
{
    /// <summary>Hasta 'limite' registros activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<Universidad>> ObtenerTodos(int limite);

    /// <summary>El registro con ese id. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<Universidad> ObtenerPorId(int id);

    /// <summary>Crea el registro. El cuerpo ya fue validado por UniversidadCrear.</summary>
    Task Crear(Universidad entidad);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> Reemplazar(int id, Universidad entidad);

    /// <summary>Escribe solo los campos enviados. ArgumentException si no llegó ninguno ·
    /// NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> ActualizarParcial(int id, string? nombre, string? tipo, string? ciudad);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe o ya estaba inactivo.</summary>
    Task<int> Eliminar(int id);
}
