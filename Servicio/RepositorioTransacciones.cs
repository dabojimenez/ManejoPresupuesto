using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string _connectionString;
        private readonly IConfiguration configuration;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new
                {
                    transaccion.UsurioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                commandType: CommandType.StoredProcedure);

            transaccion.Id = id;
        }
    }
}
