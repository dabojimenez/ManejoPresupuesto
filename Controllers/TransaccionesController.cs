using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    }
}
