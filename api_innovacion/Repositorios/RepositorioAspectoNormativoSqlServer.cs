using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 de aspecto_normativo contra SQL Server, con Dapper: el SQL a mano, a la
/// vista y SIEMPRE parametrizado. Mismo molde que RepositorioAliadoSqlServer.
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioAspectoNormativoSqlServer : IRepositorioAspectoNormativo
{
    private readonly string _cadenaConexion;

    public RepositorioAspectoNormativoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper mapea por nombre.
    private const string COLUMNAS = "id AS Id, tipo AS Tipo, descripcion AS Descripcion, fuente AS Fuente";

    public async Task<IEnumerable<AspectoNormativo>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM aspecto_normativo
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<AspectoNormativo>(sql, new { Limite = limite });
    }

    public async Task<AspectoNormativo?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente (C8)
        var sql = $@"
            SELECT {COLUMNAS}
            FROM aspecto_normativo
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<AspectoNormativo>(sql, new { Id = id });
    }

    public async Task Crear(AspectoNormativo entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Un id repetido lo rechaza la llave primaria: 500 (C11).
        const string sql = @"
            INSERT INTO aspecto_normativo (id, tipo, descripcion, fuente, activo)
            VALUES (@Id, @Tipo, @Descripcion, @Fuente, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(AspectoNormativo entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE aspecto_normativo
            SET tipo = @Tipo, descripcion = @Descripcion, fuente = @Fuente
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? tipo, string? descripcion, string? fuente)
    {
        using var conexion = CrearConexion();

        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada, escrita
        // aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", tipo);
        }
        if (descripcion != null)
        {
            asignaciones.Add("descripcion = @Descripcion");
            parametros.Add("Descripcion", descripcion);
        }
        if (fuente != null)
        {
            asignaciones.Add("fuente = @Fuente");
            parametros.Add("Fuente", fuente);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE aspecto_normativo SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Cero filas afectadas = "no existe o ya estaba inactivo" = 404 (D-v1-4)
        const string sql = @"
            UPDATE aspecto_normativo
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
