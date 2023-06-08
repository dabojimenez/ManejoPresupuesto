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

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios servicioUsuarios,
            IReposotorioCuentas reposotorioCuentas) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.reposotorioCuentas = reposotorioCuentas;
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

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.
                Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
