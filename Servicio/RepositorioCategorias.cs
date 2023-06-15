using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
    }
    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string _connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                        INSERT INTO CATEGORIAS (Nombre, TipoOperacionId, UsuarioId)
                        VALUES (@Nombre, @TIPOOPERACIONID, @USUARIOID);
                        SELECT SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }
    }
}
