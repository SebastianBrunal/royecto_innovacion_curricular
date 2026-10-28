using System.Net.Http.Json;
using System.Text.Json;

namespace FrontInnovacion.Servicios;

/// <summary>
/// Característica de innovación, tal como el front lo maneja. Es una clase del front, no de la
/// API: lo único que las une es el JSON.
/// </summary>
public class CaracteristicaInnovacion
{
    /// <summary>Identificador</summary>
    public int Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Tipo</summary>
    public string Tipo { get; set; } = string.Empty;
}

/// <summary>
/// La capa de datos del front para car_innovacion: la ÚNICA pieza que sabe que los datos
/// viven en la API (/api/car_innovacion) y la única que habla HTTP con ella.
///
/// Es específica del recurso, como ServicioAliado (Artículo 10.1). La lectura del
/// sobre de error sí es común a toda la API y vive en SobreDeError.
/// </summary>
public class ServicioCaracteristicaInnovacion
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ServicioCaracteristicaInnovacion(HttpClient http)
    {
        _http = http;
    }

    // RF1 — Listar
    public async Task<Resultado<List<CaracteristicaInnovacion>>> Listar(int limite = 1000)
    {
        try
        {
            var r = await _http.GetAsync($"/api/car_innovacion?limite={limite}");

            // 204 es «no hay ninguno», y NO es un error
            if (r.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return Resultado<List<CaracteristicaInnovacion>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<CaracteristicaInnovacion>>.Mal(await SobreDeError.Mensajes(r));
            }

            // El sobre del contrato: { tabla, limite, total, datos[] }
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<CaracteristicaInnovacion>>(_opciones) ?? new();

            return Resultado<List<CaracteristicaInnovacion>>.Bien(datos);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<List<CaracteristicaInnovacion>>.Mal(SobreDeError.NoDisponible());
        }
    }

    // RF2 — Obtener uno (lo usa el botón «Ver»)
    public async Task<Resultado<CaracteristicaInnovacion>> Obtener(int id)
    {
        try
        {
            var r = await _http.GetAsync($"/api/car_innovacion/{id}");
            if (!r.IsSuccessStatusCode)
            {
                return Resultado<CaracteristicaInnovacion>.Mal(await SobreDeError.Mensajes(r));
            }

            var ficha = await r.Content.ReadFromJsonAsync<CaracteristicaInnovacion>(_opciones);
            return Resultado<CaracteristicaInnovacion>.Bien(ficha!);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<CaracteristicaInnovacion>.Mal(SobreDeError.NoDisponible());
        }
    }

    // RF3 — Crear
    public async Task<Resultado<bool>> Crear(CaracteristicaInnovacion entidad)
    {
        return await Enviar(HttpMethod.Post, "/api/car_innovacion", entidad);
    }

    // RF4 — Reemplazar: «guardar la ficha completa». La llave viaja en la ruta.
    public async Task<Resultado<bool>> Reemplazar(int id, CaracteristicaInnovacion entidad)
    {
        var cuerpo = new
        {
            nombre = entidad.Nombre,
            descripcion = entidad.Descripcion,
            tipo = entidad.Tipo
        };
        return await Enviar(HttpMethod.Put, $"/api/car_innovacion/{id}", cuerpo);
    }

    // RF5 — Actualizar: «guardar solo lo que cambié». Un campo en blanco no viaja.
    public async Task<Resultado<bool>> Actualizar(
        int id,
        string? nombre,
        string? descripcion,
        string? tipo)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(nombre)) cuerpo["nombre"] = nombre;
        if (!string.IsNullOrWhiteSpace(descripcion)) cuerpo["descripcion"] = descripcion;
        if (!string.IsNullOrWhiteSpace(tipo)) cuerpo["tipo"] = tipo;

        return await Enviar(HttpMethod.Patch, $"/api/car_innovacion/{id}", cuerpo);
    }

    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    public async Task<Resultado<bool>> Eliminar(int id)
    {
        return await Enviar(HttpMethod.Delete, $"/api/car_innovacion/{id}", null);
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
