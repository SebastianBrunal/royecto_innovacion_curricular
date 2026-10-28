using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// La capa 3 de car_innovacion contra SQL Server, con Dapper: el SQL a mano, a la
/// vista y SIEMPRE parametrizado. Mismo molde que RepositorioAliadoSqlServer.
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico.
/// </summary>
public class RepositorioCaracteristicaInnovacionSqlServer : IRepositorioCaracteristicaInnovacion
{
    private readonly string _cadenaConexion;

    public RepositorioCaracteristicaInnovacionSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen snake_case (tabla) a PascalCase (entidad): Dapper mapea por nombre.
    private const string COLUMNAS = "id AS Id, nombre AS Nombre, descripcion AS Descripcion, tipo AS Tipo";

    public async Task<IEnumerable<CaracteristicaInnovacion>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM car_innovacion
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<CaracteristicaInnovacion>(sql, new { Limite = limite });
    }

    public async Task<CaracteristicaInnovacion?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un registro inactivo responde como inexistente (C8)
        var sql = $@"
            SELECT {COLUMNAS}
            FROM car_innovacion
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<CaracteristicaInnovacion>(sql, new { Id = id });
    }

    public async Task Crear(CaracteristicaInnovacion entidad)
    {
        using var conexion = CrearConexion();
        // Nace activo. Un id repetido lo rechaza la llave primaria: 500 (C11).
        const string sql = @"
            INSERT INTO car_innovacion (id, nombre, descripcion, tipo, activo)
            VALUES (@Id, @Nombre, @Descripcion, @Tipo, 1)";

        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> Reemplazar(CaracteristicaInnovacion entidad)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE car_innovacion
            SET nombre = @Nombre, descripcion = @Descripcion, tipo = @Tipo
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarParcial(int id, string? nombre, string? descripcion, string? tipo)
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
        if (tipo != null)
        {
            asignaciones.Add("tipo = @Tipo");
            parametros.Add("Tipo", tipo);
        }

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE car_innovacion SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Cero filas afectadas = "no existe o ya estaba inactivo" = 404 (D-v1-4)
        const string sql = @"
            UPDATE car_innovacion
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
