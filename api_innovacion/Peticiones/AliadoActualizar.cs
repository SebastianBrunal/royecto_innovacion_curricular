using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH: los cinco campos son OPCIONALES, y solo se escriben los
/// que lleguen.
///
/// La diferencia con AliadoReemplazo es toda la lección del contrato: el MISMO
/// cuerpo responde 422 en PUT y 200 en PATCH, y no lo decide un if en el
/// servicio — lo decide el tipo.
///
/// Los topes de longitud SÍ se mantienen (clarificación C18): un campo opcional
/// que llega demasiado largo sigue siendo un cuerpo inválido → 422, y no un 500
/// del motor por truncamiento.
/// </summary>
public class AliadoActualizar
{
    [MaxLength(60, ErrorMessage = "El campo razonSocial no puede exceder los 60 caracteres.")]
    public string? RazonSocial { get; set; }

    [MaxLength(60, ErrorMessage = "El campo nombreContacto no puede exceder los 60 caracteres.")]
    public string? NombreContacto { get; set; }

    [MaxLength(70, ErrorMessage = "El campo correo no puede exceder los 70 caracteres.")]
    public string? Correo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo telefono no puede exceder los 45 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(45, ErrorMessage = "El campo ciudad no puede exceder los 45 caracteres.")]
    public string? Ciudad { get; set; }
}
