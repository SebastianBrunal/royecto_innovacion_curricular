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
public class AreaConocimientoActualizar
{
    [MaxLength(60, ErrorMessage = "El campo granArea no puede exceder los 60 caracteres.")]
    public string? GranArea { get; set; }

    [MaxLength(60, ErrorMessage = "El campo area no puede exceder los 60 caracteres.")]
    public string? Area { get; set; }

    [MaxLength(150, ErrorMessage = "El campo disciplina no puede exceder los 150 caracteres.")]
    public string? Disciplina { get; set; }
}
