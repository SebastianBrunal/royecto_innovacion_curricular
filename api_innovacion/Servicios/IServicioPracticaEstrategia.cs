using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>
/// El contrato de la capa de negocio de practica_estrategia. Solo conoce Modelos/: las
/// clases de Peticiones/ son de la frontera HTTP y no cruzan a esta capa.
///
///   ArgumentException      → 400
///   NoEncontradoExcepcion  → 404
/// </summary>
public interface IServicioPracticaEstrategia
{
    /// <summary>Hasta 'limite' registros activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<PracticaEstrategia>> ObtenerTodos(int limite);

    /// <summary>El registro con ese id. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<PracticaEstrategia> ObtenerPorId(int id);

    /// <summary>Crea el registro. El cuerpo ya fue validado por PracticaEstrategiaCrear.</summary>
    Task Crear(PracticaEstrategia entidad);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> Reemplazar(int id, PracticaEstrategia entidad);

    /// <summary>Escribe solo los campos enviados. ArgumentException si no llegó ninguno ·
    /// NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> ActualizarParcial(int id, string? tipo, string? nombre, string? descripcion);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe o ya estaba inactivo.</summary>
    Task<int> Eliminar(int id);
}
