﻿using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Servicio
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametrosObtenerTransaccionesPorUsuario modelo);
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
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                commandType: CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"
                        SELECT TRA.*, CAT.TipoOperacionId
                        FROM Transacciones TRA
                        INNER JOIN Categorias CAT ON CAT.Id = TRA.CategoriaId
                        WHERE TRA.Id = @ID AND TRA.UsuarioId = @USUARIOID",
                        new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                        select tra.Id, tra.Monto, tra.FechaTransaccion,
                        cat.Nombre as [Categoria], cue.Nombre as [Cuenta], cat.TipoOperacionId
                        from Transacciones tra
                        inner join Categorias cat on cat.Id = tra.CategoriaId
                        inner join Cuentas cue on cue.Id = tra.CuentaId
                        where tra.CuentaId = @CuentaId and tra.UsuarioId = @UsuarioId
                        and tra.FechaTransaccion between @fechaInicio and @fechafin",
                        modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametrosObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                        select tra.Id, tra.Monto, tra.FechaTransaccion,
                        cat.Nombre as [Categoria], cue.Nombre as [Cuenta], cat.TipoOperacionId
                        from Transacciones tra
                        inner join Categorias cat on cat.Id = tra.CategoriaId
                        inner join Cuentas cue on cue.Id = tra.CuentaId
                        where tra.UsuarioId = @UsuarioId
                        and tra.FechaTransaccion between @fechaInicio and @fechafin
                        order by tra.FechaTransaccion desc", modelo);
        }
    }
}
