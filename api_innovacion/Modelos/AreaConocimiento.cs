namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla area_conocimiento: el catálogo jerárquico de áreas de conocimiento: gran área, área y disciplina. Viene cargado con las 218 filas del Excel de referencia.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// Igual que Aliado, no incluye Activo: el borrado lógico es un detalle interno
/// del motor y no forma parte de lo que la API expone.
/// </summary>
public class AreaConocimiento
{
    /// <summary>Es la llave primaria y es TEXTO: los códigos del catálogo son
    /// alfanuméricos ('1A01'). Ver la clarificación C1 de 2_spec.md.</summary>
    public string Id { get; set; } = string.Empty;

    public string GranArea { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string Disciplina { get; set; } = string.Empty;
}
