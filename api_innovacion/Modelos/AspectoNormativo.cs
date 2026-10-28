namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla aspecto_normativo: las leyes, decretos, resoluciones y demás normas que regulan los programas.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// Igual que Aliado, no incluye Activo: el borrado lógico es un detalle interno
/// del motor y no forma parte de lo que la API expone.
/// </summary>
public class AspectoNormativo
{
    /// <summary>Es la llave primaria. El script la declara INT sin IDENTITY: el
    /// número lo pone quien crea la ficha, no la base.</summary>
    public int Id { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Fuente { get; set; } = string.Empty;
}
