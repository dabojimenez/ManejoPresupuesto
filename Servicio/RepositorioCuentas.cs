using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicio
{
    public interface IReposotorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuentas: IReposotorioCuentas
    {
        private readonly string _connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            string mensaje = "";
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var id = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance) 
                        VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                    SELECT SCOPE_IDENTITY();", cuenta);
                cuenta.Id = id;
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                throw;
            }
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Cuenta>(@"
                            SELECT Cuentas.Id, Cuentas.Nombre, Balance, tc.Nombre as TipoCuenta
                            FROM Cuentas
                            INNER JOIN TiposCuentas tc
                            on tc.Id = Cuentas.TipoCuentaId
                            where tc.UsuarioId = @UsuarioId
                            order by tc.Orden", new {usuarioId});
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"
                            SELECT Cuentas.Id, Cuentas.Nombre, Balance, Descripcion, tc.Id
                            FROM Cuentas
                            INNER JOIN TiposCuentas tc
                            on tc.Id = Cuentas.TipoCuentaId
                            where tc.UsuarioId = @UsuarioId AND Cuentas.id = @Id", new {id, usuarioId});
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
              @"UPDATE Cuentas
                    SET Nombre = @Nombre, balance = @Balance,
                    Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
                    WHERE Id = @Id;", cuenta);
        }
    }
}
