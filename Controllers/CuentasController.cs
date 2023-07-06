using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IReposotorioCuentas reposotorioCuentas;
        private readonly IMapper mapper;
        private readonly IRepositorioTransacciones repositorioTransacciones;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios servicioUsuarios,
            IReposotorioCuentas reposotorioCuentas,
            IMapper mapper,
            IRepositorioTransacciones repositorioTransacciones) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.reposotorioCuentas = reposotorioCuentas;
            this.mapper = mapper;
            this.repositorioTransacciones = repositorioTransacciones;
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            
            var modelo = new CuentaCreacionViewModel();
            //realizamos un mapeo de tiposcuentas a selectlistitem, pasnadole el Nombre para el texto, y el id, para el value
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            //validamos que el tipo cuenta que envia el usuario es existente, para eso obtenemos la cuenta de la base de datos
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if(!ModelState.IsValid)
            {
                //mostramos los tiposcuentas del usuario para cargar la vista
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }
            //si todo es valido creamos la cuenta
            await reposotorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await reposotorioCuentas.Buscar(usuarioId);
            //construimos nuestro modelo, agrupando por tipocuenta
            var modelo = cuentasConTipoCuenta
                //agrupacion por tipocuenta
                .GroupBy(x => x.TipoCuenta)
                //realizamos un mapeo con el select, realziando una proyeccion a indicecuentaviewmodel
                .Select( grupo => new IndiceCuentasViewModel
                {
                    //el key, representa el valor que utilizamos para realizar el groupby
                    TipoCuenta = grupo.Key,
                    //cuentas, obtenemos el ienumerable, deacuerdo al tipocuenta
                    Cuentas = grupo.AsEnumerable()
                }).ToList();
            return View(modelo);

        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await reposotorioCuentas.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                RedirectToAction("NoEncontradoHome","Home");
            }
            //para armar el modelo y enviar a la vista realizaremos lo siguiente
            //var modelo = new CuentaCreacionViewModel()
            //{
            //    Id = id,
            //    Nombre = cuenta.Nombre,
            //    TipoCuentaId = cuenta.TipoCuentaId,
            //    Balance = cuenta.Balance,
            //    Descripcion = cuenta.Descripcion,
            //};
            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaCreacionViewModel)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await reposotorioCuentas.ObtenerPorId(cuentaCreacionViewModel.Id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //validamos el tipocuenta
            var tipocuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaCreacionViewModel.TipoCuentaId, usuarioId);
            if (tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //Actualizamos la cuenta
            await reposotorioCuentas.Actualizar(cuentaCreacionViewModel);
            return RedirectToAction("Index");
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.
                Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await reposotorioCuentas.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await reposotorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await reposotorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detalle(int id, int mes, int year)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await reposotorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

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

            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = id,
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio,
            };

            var transacciones = await repositorioTransacciones.ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);
            var modelo = new ReporteTransaccionesDetalladas();
            //esta clase reutilizaremos ams adelante por eso usaremos el viewbag
            ViewBag.Cuenta = cuenta.Nombre.ToUpper();

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

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.yearAnterior = fechaInicio.AddMonths(-1).Year;

            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.yearPosterior = fechaInicio.AddMonths(1).Year;

            return View(modelo);
        }
    }
}
