using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace ApiInnovacion.Pruebas;

/// <summary>
/// Las mismas comprobaciones de la prueba de aliado, para universidad: el servicio
/// corre con el repositorio de mentiras, SIN base de datos.
/// </summary>
public static class PruebaUniversidad
{
    public static async Task Correr()
    {
        Console.WriteLine();
        Console.WriteLine("--- universidad ---");

        IRepositorioUniversidad repoFalso = new RepositorioFalsoUniversidad();
        IServicioUniversidad servicio = new ServicioUniversidad(repoFalso);

        var nuevo = new Universidad
        {
            Id = 1,
            Nombre = "Universidad de San Buenaventura - Bogotá",
            Tipo = "Seccional",
            Ciudad = "Bogotá"
        };

        Programa.Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
                "Arranca vacío.", "Debía arrancar vacío.");

        await servicio.Crear(nuevo);
        var lista = (await servicio.ObtenerTodos(1000)).ToList();
        Programa.Revisar(lista.Count == 1,
                $"Creado y listado: {lista[0].Nombre}", "Debía haber exactamente uno.");

        try
        {
            await servicio.ObtenerPorId(999999);
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
            await servicio.ActualizarParcial(1, null, null, null);
            Programa.Revisar(false, "", "Debió lanzar ArgumentException por cuerpo vacío.");
        }
        catch (ArgumentException)
        {
            Programa.Revisar(true, "Actualización parcial vacía rechazada (400, no 404).", "");
        }

        await servicio.ActualizarParcial(1, "Cambiado", null, null);
        Programa.Revisar((await servicio.ObtenerPorId(1)).Nombre == "Cambiado",
                "La actualización parcial cambia solo el campo enviado.",
                "La actualización parcial no se aplicó.");

        await servicio.Eliminar(1);
        try
        {
            await servicio.Eliminar(1);
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
