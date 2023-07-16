using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicio
{
    public interface IServicioReportes
    {
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioid, int mes, int year, dynamic ViewBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int year, dynamic ViewBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int year, dynamic ViewBag);
    }
    public class ServicioReportes: IServicioReportes
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;

        public ServicioReportes(IRepositorioTransacciones repositorioTransacciones,
            IHttpContextAccessor httpContextAccessor)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ReporteTransaccionesDetalladas> 
            ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int year, dynamic ViewBag)
        {
            //asignamos los dos valores de la funcion, de la siguiente forma
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, year);

            var parametro = new ParametrosObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            };

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        public async Task<ReporteTransaccionesDetalladas> 
            ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int year, dynamic ViewBag)
        {
            //asignamos los dos valores de la funcion, de la siguiente forma
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, year);

            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio,
            };

            var transacciones = await repositorioTransacciones.ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.yearAnterior = fechaInicio.AddMonths(-1).Year;

            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.yearPosterior = fechaInicio.AddMonths(1).Year;
            //enviamso el patch de la url
            ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private static ReporteTransaccionesDetalladas GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransaccionesDetalladas();


            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                //agrupamos nuestra transaccion por fecha
                .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        //haremos uso de una tupla que nos permitira
        //devolver dos valores de nuestro metodo
        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int year)
        {
            //inicializamos als fechas
            DateTime fechaInicio;
            DateTime fechaFin;
            //validamos que no sean fechas incorrrectas
            if (mes <= 0 || mes > 12 || year <= 1900)
            {
                var hoy = DateTime.Today;
                //fecha de inicio sera el dia uno del mes actual
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(year, mes, 1);
            }
            //llevamos la fecha fin hacia el ultimo dia del mismo mes de fecha inicio
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioid, int mes
            , int year, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, year);

            var parametro = new ParametrosObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioid,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            };

            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            var modelo = await repositorioTransacciones.ObtenerPorSemana(parametro);
            return modelo;
        }
    }
}
