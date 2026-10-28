namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla car_innovacion: las características de innovación curricular que se implementan en los programas.
/// Es lo que viaja entre las capas de repositorio, servicio y controlador.
///
/// Igual que Aliado, no incluye Activo: el borrado lógico es un detalle interno
/// del motor y no forma parte de lo que la API expone.
/// </summary>
public class CaracteristicaInnovacion
{
    /// <summary>Es la llave primaria. El script la declara INT sin IDENTITY: el
    /// número lo pone quien crea la ficha, no la base.</summary>
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
}
