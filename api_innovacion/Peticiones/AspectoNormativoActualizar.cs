using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// El cuerpo del PATCH: todos los campos son OPCIONALES y solo se escriben los
/// que lleguen. Lo que sí se mantiene son los topes de longitud: un campo que
/// llega demasiado largo sigue siendo un cuerpo inválido.
///
/// El MISMO cuerpo que el PUT rechaza por incompleto, aquí es válido. No lo
/// decide un if en el servicio: lo decide el tipo.
/// </summary>
public class AspectoNormativoActualizar
{
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo descripcion no puede exceder los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo fuente no puede exceder los 45 caracteres.")]
    public string? Fuente { get; set; }
}
