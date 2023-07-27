using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }
    public class RepositorioUsuarios: IRepositorioUsuarios
    {
        private readonly string _connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            //usuario.EmailNormalizado = usuario.Email.ToUpper();
            using var connection = new SqlConnection(_connectionString);
            int id = await connection.QuerySingleAsync<int>(@"
                    INSERT INTO USUARIOS (Email, EmailNormalizado, PasswordHash)
                    VALUES (@EMAIL, @EMAILNORMALIZADO, @PASSWORDHASH);
                    SELECT SCOPE_IDENTITY();", usuario);
            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"
                    SELECT * FROM USUARIOS WHERE EMAILNORMALIZADO = @EMAILNORMALIZADO", new { emailNormalizado });
        }
    }
}
