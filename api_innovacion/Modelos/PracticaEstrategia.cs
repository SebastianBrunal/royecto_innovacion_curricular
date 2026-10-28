namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla practica_estrategia: las prácticas y estrategias pedagógicas: aprendizaje basado en proyectos, prácticas empresariales, laboratorios, talleres.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// Igual que Aliado, no incluye Activo: el borrado lógico es un detalle interno
/// del motor y no forma parte de lo que la API expone.
/// </summary>
public class PracticaEstrategia
{
    /// <summary>Es la llave primaria. El script la declara INT sin IDENTITY: el
    /// número lo pone quien crea la ficha, no la base.</summary>
    public int Id { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}
