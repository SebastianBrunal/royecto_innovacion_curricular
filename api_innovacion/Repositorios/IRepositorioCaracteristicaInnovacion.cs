using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// El contrato de la capa de datos de car_innovacion. El servicio conoce ESTA
/// interfaz y nada más: no sabe que detrás hay SQL Server.
/// </summary>
public interface IRepositorioCaracteristicaInnovacion
{
    Task<IEnumerable<CaracteristicaInnovacion>> ObtenerTodos(int limite);
    Task<CaracteristicaInnovacion?> ObtenerPorId(int id);
    Task Crear(CaracteristicaInnovacion entidad);
    Task<int> Reemplazar(CaracteristicaInnovacion entidad);
    Task<int> ActualizarParcial(int id, string? nombre, string? descripcion, string? tipo);
    Task<int> EliminarLogico(int id);
}
