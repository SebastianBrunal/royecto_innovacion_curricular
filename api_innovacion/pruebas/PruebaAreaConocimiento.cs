using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Las mismas comprobaciones de la prueba de aliado, para area_conocimiento: el servicio
/// corre con el repositorio de mentiras, SIN base de datos.
/// </summary>
public static class PruebaAreaConocimiento
{
    public static async Task Correr()
    {
        Console.WriteLine();
        Console.WriteLine("--- area_conocimiento ---");

        IRepositorioAreaConocimiento repoFalso = new RepositorioFalsoAreaConocimiento();
        IServicioAreaConocimiento servicio = new ServicioAreaConocimiento(repoFalso);

        var nuevo = new AreaConocimiento
        {
            Id = "1A01",
            GranArea = "Ciencias Naturales",
            Area = "Matemáticas",
            Disciplina = "Matemáticas puras"
        };

        Programa.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
                "Arranca vacío.", "Debía arrancar vacío.");

        await servicio.Crear(nuevo);
        var lista = (await servicio.ObtenerTodos(1000)).ToList();
        Programa.Revisar(lista.Count == 1,
                $"Creado y listado: {lista[0].GranArea}", "Debía haber exactamente uno.");

        try
        {
            await servicio.ObtenerPorId("9Z99");
            Programa.Revisar(false, "", "Debió lanzar NoEncontradoExcepcion.");
        }
        catch (NoEncontradoExcepcion)
        {
            Programa.Revisar(true, "Un id inexistente lanza NoEncontradoExcepcion.", "");
        }

        try
        {
            await servicio.ObtenerTodos(0);
            Programa.Revisar(false, "", "Debió lanzar ArgumentException.");
        }
        catch (ArgumentException)
        {
            Programa.Revisar(true, "Límite menor o igual a cero rechazado.", "");
        }

        try
        {
            await servicio.ActualizarParcial("1A01", null, null, null);
            Programa.Revisar(false, "", "Debió lanzar ArgumentException por cuerpo vacío.");
        }
        catch (ArgumentException)
        {
            Programa.Revisar(true, "Actualización parcial vacía rechazada (400, no 404).", "");
        }

        await servicio.ActualizarParcial("1A01", "Cambiado", null, null);
        Programa.Revisar((await servicio.ObtenerPorId("1A01")).GranArea == "Cambiado",
                "La actualización parcial cambia solo el campo enviado.",
                "La actualización parcial no se aplicó.");

        await servicio.Eliminar("1A01");
        try
        {
            await servicio.Eliminar("1A01");
            Programa.Revisar(false, "", "La segunda eliminación debió lanzar NoEncontradoExcepcion.");
        }
        catch (NoEncontradoExcepcion)
        {
            Programa.Revisar(true, "Segunda eliminación rechazada: ya no existe.", "");
        }

        Programa.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
                "Tras el borrado vuelve a estar vacío.", "Debía quedar vacío otra vez.");
    }
}
