using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2 de area_conocimiento: las reglas de negocio. Depende solo de la interfaz
/// del repositorio, así que se puede probar sin base de datos.
/// </summary>
public class ServicioAreaConocimiento : IServicioAreaConocimiento
{
    private readonly IRepositorioAreaConocimiento _repositorio;

    public ServicioAreaConocimiento(IRepositorioAreaConocimiento repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite)
    {
        // La forma del dato es correcta (sí es un entero): es regla de negocio → 400 (C10)
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<AreaConocimiento> ObtenerPorId(string id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe un área de conocimiento con el id {id}.");
        }

        return entidad;
    }

    public async Task Crear(AreaConocimiento entidad)
    {
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(string id, AreaConocimiento entidad)
    {
        // El id identifica la fila y viene de la ruta, no del cuerpo
        entidad.Id = id;

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un área de conocimiento con el id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina)
    {
        // Sin esto el repositorio devolvería 0 filas, que significa "no existe",
        // y responderíamos 404 en vez del 400 que pide el contrato.
        if (granArea == null
            && area == null
            && disciplina == null)
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var filasAfectadas = await _repositorio.ActualizarParcial(id, granArea, area, disciplina);

        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un área de conocimiento con el id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(string id)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un área de conocimiento con el id {id}.");
        }

        return filasAfectadas;
    }
}
