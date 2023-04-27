using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioTiposCuentas
    {
        void Crear(TipoCuenta tipoCuenta);
    }
    public class RepositorioTiposCuentas: IRepositorioTiposCuentas
    {
        private readonly string _connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            //QuerySingle, lo que me permite es hacer un query, que estamos seguros que nos va a traer un solo resultado
            //en este caso, extraeremos el id (int/entero) de la cuenta que se crea
            var id = connection.QuerySingle<int>($@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) 
                                                            Values(@Nombre, @UsuarioId,0);
                                                        SELECT SCOPE_IDENTITY();", tipoCuenta);
            //SELECT SCOPE_IDENTITY(), NOS TRAE EL ULTIMO ID CREADO
            //Con dapper, simplemente le pasamos el modelo, y de forma directa realciona el nombre de la variable @Nombre, con la del modelo TipoCuenta
            tipoCuenta.Id = id;
        }
    }
}
