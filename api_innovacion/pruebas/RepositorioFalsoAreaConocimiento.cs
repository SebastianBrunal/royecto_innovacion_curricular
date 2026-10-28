using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Repositorio de mentiras para area_conocimiento: la MISMA interfaz, con una lista en
/// memoria. Igual que el de aliado, el servicio no se entera de la diferencia.
/// </summary>
public class RepositorioFalsoAreaConocimiento : IRepositorioAreaConocimiento
{
    private readonly List<AreaConocimiento> _filas = new();

    public Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Take(limite).AsEnumerable());

    public Task<AreaConocimiento?> ObtenerPorId(string id) =>
        Task.FromResult(_filas.FirstOrDefault(x => x.Id == id));

    public Task Crear(AreaConocimiento entidad)
    {
        _filas.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> Reemplazar(AreaConocimiento entidad)
    {
        var i = _filas.FindIndex(x => x.Id == entidad.Id);
        if (i == -1) return Task.FromResult(0);
        _filas[i] = entidad;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina)
    {
        var x = _filas.FirstOrDefault(f => f.Id == id);
        if (x == null) return Task.FromResult(0);

        if (granArea != null) x.GranArea = granArea;
        if (area != null) x.Area = area;
        if (disciplina != null) x.Disciplina = disciplina;

        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(string id)
    {
        // Sacarlo de la lista reproduce el efecto observable del borrado lógico
        var x = _filas.FirstOrDefault(f => f.Id == id);
        if (x == null) return Task.FromResult(0);
        _filas.Remove(x);
        return Task.FromResult(1);
    }
}
