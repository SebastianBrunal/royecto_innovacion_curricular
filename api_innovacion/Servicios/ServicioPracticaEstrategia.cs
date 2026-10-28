using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// La capa 2 de practica_estrategia: las reglas de negocio. Depende solo de la interfaz
/// del repositorio, así que se puede probar sin base de datos.
/// </summary>
public class ServicioPracticaEstrategia : IServicioPracticaEstrategia
{
    private readonly IRepositorioPracticaEstrategia _repositorio;

    public ServicioPracticaEstrategia(IRepositorioPracticaEstrategia repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<PracticaEstrategia>> ObtenerTodos(int limite)
    {
        // La forma del dato es correcta (sí es un entero): es regla de negocio → 400 (C10)
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<PracticaEstrategia> ObtenerPorId(int id)
    {
        var entidad = await _repositorio.ObtenerPorId(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe una práctica o estrategia con el id {id}.");
        }

        return entidad;
    }

    public async Task Crear(PracticaEstrategia entidad)
    {
        await _repositorio.Crear(entidad);
    }

    public async Task<int> Reemplazar(int id, PracticaEstrategia entidad)
    {
        // El id identifica la fila y viene de la ruta, no del cuerpo
        entidad.Id = id;

        var filasAfectadas = await _repositorio.Reemplazar(entidad);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe una práctica o estrategia con el id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int id, string? tipo, string? nombre, string? descripcion)
    {
        // Sin esto el repositorio devolvería 0 filas, que significa "no existe",
        // y responderíamos 404 en vez del 400 que pide el contrato.
        if (tipo == null
            && nombre == null
            && descripcion == null)
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var filasAfectadas = await _repositorio.ActualizarParcial(id, tipo, nombre, descripcion);

        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe una práctica o estrategia con el id {id}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int id)
    {
        var filasAfectadas = await _repositorio.EliminarLogico(id);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe una práctica o estrategia con el id {id}.");
        }

        return filasAfectadas;
    }
}
