using System.Net.Http.Json;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>
/// Cómo se leen los errores de ESTA API.
///
/// Estaba dentro de ServicioAliado. Al llegar las otras seis tablas había dos
/// caminos: copiarlo siete veces o sacarlo aquí. Se sacó, porque el formato del
/// sobre de error **no es de un recurso, es de la API entera** (Artículo 10 de
/// la constitución): si mañana cambia, se cambia en un solo sitio.
///
/// Lo que sigue siendo de cada recurso —sus rutas, sus campos, sus mensajes—
/// se queda en su propio servicio (Artículo 10.1).
/// </summary>
public static class SobreDeError
{
    /// <summary>Lo que ve el usuario cuando la API no contesta.</summary>
    public static List<string> NoDisponible() => new()
    {
        "El servicio no está disponible. ¿Está arriba la API?"
    };

    /// <summary>
    /// Traduce a texto los errores que produce la API.
    ///
    /// El sobre es plano y tiene dos formas:
    ///   { estado, mensaje, detalle }   → 400, 404, 500
    ///   { estado, mensaje, errores[] } → 422, cuando el cuerpo no cumple
    /// </summary>
    public static async Task<List<string>> Mensajes(HttpResponseMessage r)
    {
        try
        {
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();

            if (sobre.TryGetProperty("errores", out var errores)
                && errores.ValueKind == JsonValueKind.Array
                && errores.GetArrayLength() > 0)
            {
                return errores.EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .Where(x => x.Length > 0)
                    .ToList();
            }

            var partes = new List<string>();
            if (sobre.TryGetProperty("mensaje", out var m)) partes.Add(m.GetString() ?? "");
            if (sobre.TryGetProperty("detalle", out var dt)) partes.Add(dt.GetString() ?? "");
            partes.RemoveAll(string.IsNullOrWhiteSpace);

            return partes.Count > 0
                ? partes
                : new List<string> { "No se pudo completar la operación." };
        }
        catch
        {
            // Un 500 puede devolver HTML en vez de JSON.
            return new List<string> { "No se pudo completar la operación." };
        }
    }
}
