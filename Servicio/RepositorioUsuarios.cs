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
            int UsuarioId = await connection.QuerySingleAsync<int>(@"
                    INSERT INTO USUARIOS (Email, EmailNormalizado, PasswordHash)
                    VALUES (@EMAIL, @EMAILNORMALIZADO, @PASSWORDHASH);
                    SELECT SCOPE_IDENTITY();", usuario);
            //INSERTAMOS DATA DE FORMA PRELEIMINAR AL USAURIO, USNAOD EL SP QUE SE CREAO EN LA ABSE DE DATOS
            await connection.ExecuteAsync("CrearDatosUsuarioNuevo", new { UsuarioId }, commandType: System.Data.CommandType.StoredProcedure);
            return UsuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"
                    SELECT * FROM USUARIOS WHERE EMAILNORMALIZADO = @EMAILNORMALIZADO", new { emailNormalizado });
        }
    }
}
