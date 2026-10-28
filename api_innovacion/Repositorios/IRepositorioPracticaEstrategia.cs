using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos de practica_estrategia. El servicio conoce ESTA
/// interfaz y nada más: no sabe que detrás hay SQL Server.
/// </summary>
public interface IRepositorioPracticaEstrategia
{
    Task<IEnumerable<PracticaEstrategia>> ObtenerTodos(int limite);
    Task<PracticaEstrategia?> ObtenerPorId(int id);
    Task Crear(PracticaEstrategia entidad);
    Task<int> Reemplazar(PracticaEstrategia entidad);
    Task<int> ActualizarParcial(int id, string? tipo, string? nombre, string? descripcion);
    Task<int> EliminarLogico(int id);
}
