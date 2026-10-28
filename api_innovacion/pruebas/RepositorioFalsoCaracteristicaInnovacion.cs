using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Repositorio de mentiras para car_innovacion: la MISMA interfaz, con una lista en
/// memoria. Igual que el de aliado, el servicio no se entera de la diferencia.
/// </summary>
public class RepositorioFalsoCaracteristicaInnovacion : IRepositorioCaracteristicaInnovacion
{
    private readonly List<CaracteristicaInnovacion> _filas = new();

    public Task<IEnumerable<CaracteristicaInnovacion>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Take(limite).AsEnumerable());

    public Task<CaracteristicaInnovacion?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.FirstOrDefault(x => x.Id == id));

    public Task Crear(CaracteristicaInnovacion entidad)
    {
        _filas.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(CaracteristicaInnovacion entidad)
    {
        var i = _filas.FindIndex(x => x.Id == entidad.Id);
        if (i == -1) return Task.FromResult(0);
        _filas[i] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, string? nombre, string? descripcion, string? tipo)
    {
        var x = _filas.FirstOrDefault(f => f.Id == id);
        if (x == null) return Task.FromResult(0);

        if (nombre != null) x.Nombre = nombre;
        if (descripcion != null) x.Descripcion = descripcion;
        if (tipo != null) x.Tipo = tipo;

        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int id)
    {
        // Sacarlo de la lista reproduce el efecto observable del borrado lógico
        var x = _filas.FirstOrDefault(f => f.Id == id);
        if (x == null) return Task.FromResult(0);
        _filas.Remove(x);
        return Task.FromResult(1);
    }
}
