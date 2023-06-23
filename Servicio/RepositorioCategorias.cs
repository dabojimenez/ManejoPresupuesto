using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<Categoria> ObtenerPorId(int id, int usuarioid);
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

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Categoria>(@"
                        SELECT * FROM CATEGORIAS WHERE USUARIOID = @USUARIOID",
                        new { usuarioId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioid)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"
                        SELECT * FROM CATEGORIAS WHERE Id = @Id AND UsuarioId = @UsuarioId", 
                        new {id, usuarioid});
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"
                        UPDATE CATEGORIAS SET NOMBRE = @NOMBRE, TIPOOPERACIONID = @TIPOOPERACIONID
                        WHERE ID = @ID",
                        categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"DELETE Categorias WHERE Id = @Id",
                new {id});
        }
    }
}
