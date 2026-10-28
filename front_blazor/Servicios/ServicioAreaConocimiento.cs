using System.Net.Http.Json;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>
/// Área de conocimiento, tal como el front lo maneja. Es una clase del front, no de la
/// API: lo único que las une es el JSON.
/// </summary>
public class AreaConocimiento
{
    /// <summary>Identificador</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gran área</summary>
    public string GranArea { get; set; } = string.Empty;

    /// <summary>Área</summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>Disciplina</summary>
    public string Disciplina { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para area_conocimiento: la ÚNICA pieza que sabe que los datos
/// viven en la API (/api/area_conocimiento) y la única que habla HTTP con ella.
///
/// Es específica del recurso, como ServicioAliado (Artículo 10.1). La lectura del
/// sobre de error sí es común a toda la API y vive en SobreDeError.
/// </summary>
public class ServicioAreaConocimiento
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ServicioAreaConocimiento(HttpClient http)
    {
        _http = http;
    }

    // RF1 — Listar
    public async Task<Resultado<List<AreaConocimiento>>> Listar(int limite = 1000)
    {
        try
        {
            var r = await _http.GetAsync($"/api/area_conocimiento?limite={limite}");

            // 204 es «no hay ninguno», y NO es un error
            if (r.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return Resultado<List<AreaConocimiento>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<AreaConocimiento>>.Mal(await SobreDeError.Mensajes(r));
            }

            // El sobre del contrato: { tabla, limite, total, datos[] }
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<AreaConocimiento>>(_opciones) ?? new();

            return Resultado<List<AreaConocimiento>>.Bien(datos);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<List<AreaConocimiento>>.Mal(SobreDeError.NoDisponible());
        }
    }

    // RF2 — Obtener uno (lo usa el botón «Ver»)
    public async Task<Resultado<AreaConocimiento>> Obtener(string id)
    {
        try
        {
            var r = await _http.GetAsync($"/api/area_conocimiento/{Uri.EscapeDataString(id)}");
            if (!r.IsSuccessStatusCode)
            {
                return Resultado<AreaConocimiento>.Mal(await SobreDeError.Mensajes(r));
            }

            var ficha = await r.Content.ReadFromJsonAsync<AreaConocimiento>(_opciones);
            return Resultado<AreaConocimiento>.Bien(ficha!);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<AreaConocimiento>.Mal(SobreDeError.NoDisponible());
        }
    }

    // RF3 — Crear
    public async Task<Resultado<bool>> Crear(AreaConocimiento entidad)
    {
        return await Enviar(HttpMethod.Post, "/api/area_conocimiento", entidad);
    }

    // RF4 — Reemplazar: «guardar la ficha completa». La llave viaja en la ruta.
    public async Task<Resultado<bool>> Reemplazar(string id, AreaConocimiento entidad)
    {
        var cuerpo = new
        {
            granArea = entidad.GranArea,
            area = entidad.Area,
            disciplina = entidad.Disciplina
        };
        return await Enviar(HttpMethod.Put, $"/api/area_conocimiento/{Uri.EscapeDataString(id)}", cuerpo);
    }

    // RF5 — Actualizar: «guardar solo lo que cambié». Un campo en blanco no viaja.
    public async Task<Resultado<bool>> Actualizar(
        string id,
        string? granArea,
        string? area,
        string? disciplina)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(granArea)) cuerpo["granArea"] = granArea;
        if (!string.IsNullOrWhiteSpace(area)) cuerpo["area"] = area;
        if (!string.IsNullOrWhiteSpace(disciplina)) cuerpo["disciplina"] = disciplina;

        return await Enviar(HttpMethod.Patch, $"/api/area_conocimiento/{Uri.EscapeDataString(id)}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public async Task<Resultado<bool>> Eliminar(string id)
    {
        return await Enviar(HttpMethod.Delete, $"/api/area_conocimiento/{Uri.EscapeDataString(id)}", null);
    }

    private async Task<Resultado<bool>> Enviar(HttpMethod metodo, string ruta, object? cuerpo)
    {
        try
        {
            var peticion = new HttpRequestMessage(metodo, ruta);
            if (cuerpo != null)
            {
                peticion.Content = JsonContent.Create(cuerpo);
            }

            var r = await _http.SendAsync(peticion);
            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await SobreDeError.Mensajes(r));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<bool>.Mal(SobreDeError.NoDisponible());
        }
    }
}
