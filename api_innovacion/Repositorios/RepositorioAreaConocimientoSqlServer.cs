using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 de area_conocimiento contra SQL Server, con Dapper: el SQL a mano, a la
/// vista y SIEMPRE parametrizado. Mismo molde que RepositorioAliadoSqlServer.
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAreaConocimientoSqlServer : IRepositorioAreaConocimiento
{
    private readonly string _cadenaConexion;

    public RepositorioAreaConocimientoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper mapea por nombre.
    private const string COLUMNAS = "id AS Id, gran_area AS GranArea, area AS Area, disciplina AS Disciplina";

    public async Task<IEnumerable<AreaConocimiento>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM area_conocimiento
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<AreaConocimiento>(sql, new { Limite = limite });
    }

    public async Task<AreaConocimiento?> ObtenerPorId(string id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente (C8)
        var sql = $@"
            SELECT {COLUMNAS}
            FROM area_conocimiento
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AreaConocimiento>(sql, new { Id = id });
    }

    public async Task Crear(AreaConocimiento entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Un id repetido lo rechaza la llave primaria: 500 (C11).
        const string sql = @"
            INSERT INTO area_conocimiento (id, gran_area, area, disciplina, activo)
            VALUES (@Id, @GranArea, @Area, @Disciplina, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(AreaConocimiento entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE area_conocimiento
            SET gran_area = @GranArea, area = @Area, disciplina = @Disciplina
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(string id, string? granArea, string? area, string? disciplina)
    {
        using var conexion = CrearConexion();

        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada, escrita
        // aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (granArea != null)
        {
            asignaciones.Add("gran_area = @GranArea");
            parametros.Add("GranArea", granArea);
        }
        if (area != null)
        {
            asignaciones.Add("area = @Area");
            parametros.Add("Area", area);
        }
        if (disciplina != null)
        {
            asignaciones.Add("disciplina = @Disciplina");
            parametros.Add("Disciplina", disciplina);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE area_conocimiento SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(string id)
    {
        using var conexion = CrearConexion();
        // Cero filas afectadas = "no existe o ya estaba inactivo" = 404 (D-v1-4)
        const string sql = @"
            UPDATE area_conocimiento
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
