using FrontInnovacion.Components;
using FrontInnovacion.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server: el componente se renderiza en el servidor y el navegador
// recibe el HTML ya armado, manteniendo una conexión para los eventos.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================
// DE DÓNDE SALEN LOS DATOS
//
// De la API, por HTTP, y de ningún otro sitio. La dirección viene de la
// configuración: fuera de Docker vale lo de appsettings.json; dentro, el
// compose la sobreescribe con el NOMBRE del servicio —`api-innovacion`—,
// porque `localhost` dentro de un contenedor es el contenedor mismo.
// ============================================================
var urlApi = builder.Configuration["UrlApi"] ?? "http://localhost:8072";

// Todos los clientes apuntan a la MISMA API con el mismo tiempo de espera.
void ConfigurarCliente(HttpClient cliente)
{
    cliente.BaseAddress = new Uri(urlApi);
    cliente.Timeout = TimeSpan.FromSeconds(10);
}

// ============================================================
// UN SERVICIO POR RECURSO (Artículo 10.1)
//
// Una línea por cada tabla de la v1 — no una que sirva para cualquier tabla.
// ============================================================
builder.Services.AddHttpClient<ServicioAliado>(ConfigurarCliente);
builder.Services.AddHttpClient<ServicioAreaConocimiento>(ConfigurarCliente);
builder.Services.AddHttpClient<ServicioUniversidad>(ConfigurarCliente);
builder.Services.AddHttpClient<ServicioAspectoNormativo>(ConfigurarCliente);
builder.Services.AddHttpClient<ServicioPracticaEstrategia>(ConfigurarCliente);
builder.Services.AddHttpClient<ServicioEnfoque>(ConfigurarCliente);
builder.Services.AddHttpClient<ServicioCaracteristicaInnovacion>(ConfigurarCliente);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
