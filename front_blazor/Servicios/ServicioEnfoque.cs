using System.Net.Http.Json;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>
/// Enfoque, tal como el front lo maneja. Es una clase del front, no de la
/// API: lo único que las une es el JSON.
/// </summary>
public class Enfoque
{
    /// <summary>Identificador</summary>
    public int Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción</summary>
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para enfoque: la ÚNICA pieza que sabe que los datos
/// viven en la API (/api/enfoque) y la única que habla HTTP con ella.
///
/// Es específica del recurso, como ServicioAliado (Artículo 10.1). La lectura del
/// sobre de error sí es común a toda la API y vive en SobreDeError.
/// </summary>
public class ServicioEnfoque
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ServicioEnfoque(HttpClient http)
    {
        _http = http;
    }

    // RF1 — Listar
    public async Task<Resultado<List<Enfoque>>> Listar(int limite = 1000)
    {
        try
        {
            var r = await _http.GetAsync($"/api/enfoque?limite={limite}");

            // 204 es «no hay ninguno», y NO es un error
            if (r.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return Resultado<List<Enfoque>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<Enfoque>>.Mal(await SobreDeError.Mensajes(r));
            }

            // El sobre del contrato: { tabla, limite, total, datos[] }
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<Enfoque>>(_opciones) ?? new();

            return Resultado<List<Enfoque>>.Bien(datos);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<List<Enfoque>>.Mal(SobreDeError.NoDisponible());
        }
    }

    // RF2 — Obtener uno (lo usa el botón «Ver»)
    public async Task<Resultado<Enfoque>> Obtener(int id)
    {
        try
        {
            var r = await _http.GetAsync($"/api/enfoque/{id}");
            if (!r.IsSuccessStatusCode)
            {
                return Resultado<Enfoque>.Mal(await SobreDeError.Mensajes(r));
            }

            var ficha = await r.Content.ReadFromJsonAsync<Enfoque>(_opciones);
            return Resultado<Enfoque>.Bien(ficha!);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<Enfoque>.Mal(SobreDeError.NoDisponible());
        }
    }

    // RF3 — Crear
    public async Task<Resultado<bool>> Crear(Enfoque entidad)
    {
        return await Enviar(HttpMethod.Post, "/api/enfoque", entidad);
    }

    // RF4 — Reemplazar: «guardar la ficha completa». La llave viaja en la ruta.
    public async Task<Resultado<bool>> Reemplazar(int id, Enfoque entidad)
    {
        var cuerpo = new
        {
            nombre = entidad.Nombre,
            descripcion = entidad.Descripcion
        };
        return await Enviar(HttpMethod.Put, $"/api/enfoque/{id}", cuerpo);
    }

    // RF5 — Actualizar: «guardar solo lo que cambié». Un campo en blanco no viaja.
    public async Task<Resultado<bool>> Actualizar(
        int id,
        string? nombre,
        string? descripcion)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(nombre)) cuerpo["nombre"] = nombre;
        if (!string.IsNullOrWhiteSpace(descripcion)) cuerpo["descripcion"] = descripcion;

        return await Enviar(HttpMethod.Patch, $"/api/enfoque/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public async Task<Resultado<bool>> Eliminar(int id)
    {
        return await Enviar(HttpMethod.Delete, $"/api/enfoque/{id}", null);
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
