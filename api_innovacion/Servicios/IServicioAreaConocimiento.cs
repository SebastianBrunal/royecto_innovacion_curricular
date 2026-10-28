using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>
/// El contrato de la capa de negocio de area_conocimiento. Solo conoce Modelos/: las
/// clases de Peticiones/ son de la frontera HTTP y no cruzan a esta capa.
///
///   ArgumentException      → 400
///   NoEncontradoExcepcion  → 404
/// </summary>
public interface IServicioAreaConocimiento
{
    /// <summary>Hasta 'limite' registros activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite);

    /// <summary>El registro con ese id. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<AreaConocimiento> ObtenerPorId(string id);

    /// <summary>Crea el registro. El cuerpo ya fue validado por AreaConocimientoCrear.</summary>
    Task Crear(AreaConocimiento entidad);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> Reemplazar(string id, AreaConocimiento entidad);

    /// <summary>Escribe solo los campos enviados. ArgumentException si no llegó ninguno ·
    /// NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe o ya estaba inactivo.</summary>
    Task<int> Eliminar(string id);
}
