using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IReposotorioCuentas reposotorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IServicioUsuarios servicioUsuarios,
            IReposotorioCuentas reposotorioCuentas,
            IRepositorioCategorias repositorioCategorias,
            IRepositorioTransacciones repositorioTransacciones,
            IMapper mapper,
            IServicioReportes servicioReportes)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.reposotorioCuentas = reposotorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }
        public async Task<IActionResult> Index(int mes, int year)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();

            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioid, mes, year, ViewBag);
            
            return View(modelo);
        }

        [HttpGet]
        //urlRetorno, la usaremos para retornar al usuario a la vista donde se encontraba
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioid);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //usaremos automaper
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            //esto si es que es un ingreso
            modelo.MontoAnterior = modelo.Monto;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioid, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioid);
            modelo.urlRetorno = urlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioid);
                modelo.Categorias = await ObtenerCategorias(usuarioid, modelo.TipoOperacionId);
                return View(modelo);
            }
            //verificaosmque la cuenta exista
            var cuenta = await reposotorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioid);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioid);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //mapeamos
            var transaccion = mapper.Map<Transaccion>(modelo);
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }
            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            if (string.IsNullOrEmpty(modelo.urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                //LocalRedirect, hacemos auna redireccion auna url que se encuentra dentro de nuestro dominio
                return LocalRedirect(modelo.urlRetorno);
            }
            
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioid);
            modelo.Categorias = await ObtenerCategorias(usuarioid, modelo.TipoOperacionId);
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioid);
                modelo.Categorias = await ObtenerCategorias(usuarioid, modelo.TipoOperacionId);
                return View(modelo);
            }
            //validamso que la cuenta que el usuario manda, sea una cuenta valida
            var cuenta = await reposotorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioid);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //igualmente validamos al categoria
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioid);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId= usuarioid;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                //ManejoPresupuesto para guardar como gasto, en negativo
                modelo.Monto *= -1;
            }
            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await reposotorioCuentas.Buscar(usuarioId);
            //realziamos una proyeccion
            return cuentas.Select(x => new SelectListItem { 
                Text = x.Nombre, 
                Value = x.Id.ToString() 
            });
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioid, tipoOperacion);
            return Ok(categorias);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioid, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioid, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorId(id, usuarioid);
            if (transacciones is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        //Para los reportes diarios, semanal, mensual excel y calendario
        public async Task<IActionResult> Semanal(int mes, int year)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, year, ViewBag);
            
            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana)
                .Select(x => new ResultadoObtenerPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                        .Select(x => x.Monto).FirstOrDefault(),

                }).ToList();
            
            //creamos un arreglo con todos los dias del mes
            if(year == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                year = hoy.Year;
                mes = hoy.Month;
            }
            //creamos una fecha referencia
            var fechaReferencia = new DateTime(year, mes, 1);
            var diasMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            //segmentamos los dias en 7
            var diasSegementados = diasMes.Chunk(7).ToList();

            //generamos la fecha d einicio yy de fin de la semana
            for (int i = 0; i < diasSegementados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(year, mes, diasSegementados[i].First());
                var fechaFin = new DateTime(year, mes, diasSegementados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if (grupoSemana is null)
                {
                    //si no tiene registros ene sa semana, creamos un registro manualmente
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin,
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            //ordenamos de ofrma descendetnte
            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }
        public async Task<IActionResult> Mensual(int year)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (year == 0)
            {
                year = DateTime.Today.Year;
            }
            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, year);
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                .Select(x => new ResultadoObtenerPorMes()
                {
                    Mes = x.Key,
                    Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                                .Select(x => x.Monto)
                                .FirstOrDefault(),
                    Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                                .Select(x => x.Monto)
                                .FirstOrDefault()
                }).ToList();
            //contamos los meses del uno al doce
            for (int mes = 1; mes < 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(year, mes, 1);
                if(transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia,
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }
            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Year = year;
            modelo.TransaccionesPorMes = transaccionesAgrupadas; 
            
            return View(modelo);
        }
        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        //es de tipo FileResult, para q el usuario pueda descargar el archivo xml, al invocar edsta accion
        public async Task<FileResult> ExportarExcelPorMes(int mes, int year)
        {
            var fechaInicio = new DateTime(year, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                //instanciamos el parametro, directo en la funcion
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                });
            //nombramos el archivo
            string nomnreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            //Empeamos a creael archivo de excel, en la funcion de retorno, en este caso, lo hacemos directo
            return GenerarExcel(nomnreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorYear(int year)
        {
            var fechaInicio = new DateTime(year, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                });

            string nomnreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nomnreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            //agregamos una fecha de inicio bien baja, y agregamos una fecha de bsuqueda maxima
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(100);

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                });

            string nomnreArchivo = $"Manejo Presupuestos - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GenerarExcel(nomnreArchivo, transacciones);
        }

        /// <summary>
        /// Creara el arhcivo de excel, revision como parametro el nombre dela rchivo y el listado de transacciones
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo de destino</param>
        /// <param name="transacciones">Listado de transacciones, para ainsertar en ela rchivo excel</param>
        /// <returns>Retorna el archivo excel</returns>
        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            //para crear el archivo, debemos usar un datatable
            //instanciamos un datatable, y le agregamos el nombre, como queremos llamar a nuestra tabla
            DataTable dataTable = new DataTable("Transacciones");
            //agregamos las columnas, indicandole un rango de columnas a crear
            dataTable.Columns.AddRange(new DataColumn[]
            {
                //definimso la columna que tendra nuestro arhcivo de excel, en este caso tendra una columna llamada fecha, etc
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });

            //iteramos nuestras transacciones
            foreach (var transaccion in transacciones)
            {
                //agregamos la data de las transacicones
                //NOTA: importante, que dbee ir en el orden de las columnas creadas
                dataTable.Rows.Add(transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }

            //generamos el archivo PERSE

            using (XLWorkbook workbook = new XLWorkbook())
            {
                //Worksheets, quiere decir las hojas del excel
                //agregamos nuestro datatable
                workbook.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    //vamos a guardar el archivo de excel, en memori stream, para despues devolver al usuario
                    workbook.SaveAs(stream);
                    //con file, le dmaos la repsuesta al usuairo, para q pueda descargar ela rhcivo de excel
                    return File(stream.ToArray(),
                        //Agregamos el content/Type
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        //agregamos el nombre dela rchivo
                        nombreArchivo
                        );
                }
            }

        }
        public IActionResult Calendario()
        {
            return View();
        }


        //JsonResult, retornaremos un json
        //fullcalendar, nos envia la fecha de inicio y la fecha de fin
        /// <summary>
        /// Obtendremso la fecha del calendario de inicio y fin, estas fechas nos enviara directamente fullcalendar
        /// es necesario que tenga los nombres star y end, en los parametros
        /// </summary>
        /// <param name="start">Fecha de inicio</param>
        /// <param name="end">Fecha de fin</param>
        /// <returns>Retornaremos un JSON, para usar en la vista</returns>
        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = start,
                    FechaFin = end,
                });

            //creamos una clase, en la que las propiedades seran los eventos que recibe fullCalendar
            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                //usaremos operador ternario, si es gasto sera de color rojo
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto) ? "Red" : null
            });
            //transformamos a JSOn, nuestra clase con las propiedades del calendario
            return Json(eventosCalendario);
        }

        /// <summary>
        /// Funcion que retornara el detalle de la transaccion
        /// </summary>
        /// <param name="fecha">fecha de inicio y fin de las transaccione seleccionada</param>
        /// <returns>Retorna un JSON, con la transaccion</returns>
        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fecha,
                    FechaFin = fecha,
                });

            return Json(transacciones);
        }
    }
}
