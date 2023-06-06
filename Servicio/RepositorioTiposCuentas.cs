using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);

        //void Crear(TipoCuenta tipoCuenta);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string _connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            //QuerySingle, lo que me permite es hacer un query, que estamos seguros que nos va a traer un solo resultado
            //en este caso, extraeremos el id (int/entero) de la cuenta que se crea
            //var id = await connection.QuerySingleAsync<int>($@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) 
            //                                                Values(@Nombre, @UsuarioId,0);
            //                                            SELECT SCOPE_IDENTITY();", tipoCuenta);
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
                //ahora enviaremos un objeto anonimo, ya que no podemos enviar la clase, por los argumentos que el sp toma
                new
                {
                    usuarioId = tipoCuenta.UsuarioId,
                    nombre = tipoCuenta.Nombre
                }
                //indicamso que vamos a ejecutar un procedimiento almacenado
                , commandType: CommandType.StoredProcedure);
            //SELECT SCOPE_IDENTITY(), NOS TRAE EL ULTIMO ID CREADO
            //Con dapper, simplemente le pasamos el modelo, y de forma directa realciona el nombre de la variable @Nombre, con la del modelo TipoCuenta
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            //retornara, un tipo de dato de tipo entero, o por defecto traera un cero
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                                           $@"SELECT 1
                                               FROM TiposCuentas
                                                WHERE Nombre = @Nombre and UsuarioId = @UsuarioId;",
                                           new { nombre, usuarioId });
            //retornamos verdadero si es uno por que 1 == 1, pero si me trae cero ceria 0 == 1 y eso es false
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            var listaTipoCuentas = await connection.QueryAsync<TipoCuenta>($@"
                                SELECT 
                                Id, Nombre, Orden
                                FROM TiposCuentas
                                WHERE UsuarioId = @UsuarioId 
                                Order by Orden", new { usuarioId });
            return listaTipoCuentas;
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync($@"
                                UPDATE TiposCuentas
                                SET Nombre = @Nombre
                                WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>($@"
                                SELECT Id, Nombre, Orden
                                FROM TiposCuentas
                                WHERE Id = @Id and UsuarioId = @UsuarioId", new {id, usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync($@"DELETE TiposCuentas Where Id = @Id", new {id});
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden where Id = @Id;";
            using var connection = new SqlConnection(_connectionString);
            //este query, se ejecutar por cada tipocuenta, por cada item se ejecutara este query
            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }
    }


}
