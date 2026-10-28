using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Repositorio de mentiras para aspecto_normativo: la MISMA interfaz, con una lista en
/// memoria. Igual que el de aliado, el servicio no se entera de la diferencia.
/// </summary>
public class RepositorioFalsoAspectoNormativo : IRepositorioAspectoNormativo
{
    private readonly List<AspectoNormativo> _filas = new();

    public Task<IEnumerable<AspectoNormativo>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Take(limite).AsEnumerable());

    public Task<AspectoNormativo?> ObtenerPorId(int id) =>
        Task.FromResult(_filas.FirstOrDefault(x => x.Id == id));

    public Task Crear(AspectoNormativo entidad)
    {
        _filas.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(AspectoNormativo entidad)
    {
        var i = _filas.FindIndex(x => x.Id == entidad.Id);
        if (i == -1) return Task.FromResult(0);
        _filas[i] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int id, string? tipo, string? descripcion, string? fuente)
    {
        var x = _filas.FirstOrDefault(f => f.Id == id);
        if (x == null) return Task.FromResult(0);

        if (tipo != null) x.Tipo = tipo;
        if (descripcion != null) x.Descripcion = descripcion;
        if (fuente != null) x.Fuente = fuente;

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
