using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 de enfoque contra SQL Server, con Dapper: el SQL a mano, a la
/// vista y SIEMPRE parametrizado. Mismo molde que RepositorioAliadoSqlServer.
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioEnfoqueSqlServer : IRepositorioEnfoque
{
    private readonly string _cadenaConexion;

    public RepositorioEnfoqueSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper mapea por nombre.
    private const string COLUMNAS = "id AS Id, nombre AS Nombre, descripcion AS Descripcion";

    public async Task<IEnumerable<Enfoque>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM enfoque
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<Enfoque>(sql, new { Limite = limite });
    }

    public async Task<Enfoque?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente (C8)
        var sql = $@"
            SELECT {COLUMNAS}
            FROM enfoque
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Enfoque>(sql, new { Id = id });
    }

    public async Task Crear(Enfoque entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Un id repetido lo rechaza la llave primaria: 500 (C11).
        const string sql = @"
            INSERT INTO enfoque (id, nombre, descripcion, activo)
            VALUES (@Id, @Nombre, @Descripcion, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(Enfoque entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE enfoque
            SET nombre = @Nombre, descripcion = @Descripcion
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? nombre, string? descripcion)
    {
        using var conexion = CrearConexion();

        // Lo que se compone son NOMBRES DE COLUMNA de una lista cerrada, escrita
        // aquí; los VALORES siempre viajan como @parametro.
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        if (nombre != null)
        {
            asignaciones.Add("nombre = @Nombre");
            parametros.Add("Nombre", nombre);
        }
        if (descripcion != null)
        {
            asignaciones.Add("descripcion = @Descripcion");
            parametros.Add("Descripcion", descripcion);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE enfoque SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Cero filas afectadas = "no existe o ya estaba inactivo" = 404 (D-v1-4)
        const string sql = @"
            UPDATE enfoque
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
